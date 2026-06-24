/**
 * hoodcms/dev — cross-platform process spawning (HOOD-131).
 *
 * Every spawn goes through `cross-spawn` so `.cmd`/`.bat` shims (pnpm, dotnet on Windows)
 * don't blow up with `spawn EINVAL`. The parallel runner tears the whole process tree down
 * on a single Ctrl+C via `tree-kill` (SIGINT on POSIX, `taskkill /T /F` on Windows) so no
 * orphaned watchers survive.
 */
import spawn from 'cross-spawn';
import treeKill from 'tree-kill';
import type { NamedProc, SpawnOpts } from './types';

export interface CaptureResult {
    code: number | null;
    stdout: string;
    stderr: string;
}

function spawnOpts(cwd: string, opts?: SpawnOpts) {
    return {
        cwd: opts?.cwd || cwd,
        env: opts?.env ? { ...process.env, ...opts.env } : process.env,
    };
}

/** Run a command to completion with inherited stdio; reject on non-zero exit or spawn error. */
export function run(cwd: string, cmd: string, args: string[], opts?: SpawnOpts): Promise<void> {
    return new Promise((resolve, reject) => {
        const child = spawn(cmd, args, { stdio: 'inherit', ...spawnOpts(cwd, opts) });
        child.on('error', reject);
        child.on('exit', (code, signal) => {
            if (code === 0) resolve();
            else reject(new Error(`\`${cmd} ${args.join(' ')}\` exited with ${signal ?? code}`));
        });
    });
}

/** Run a command and capture stdout/stderr (used by the docker health poll). Never rejects on exit code. */
export function capture(cwd: string, cmd: string, args: string[], opts?: SpawnOpts): Promise<CaptureResult> {
    return new Promise((resolve, reject) => {
        const child = spawn(cmd, args, { stdio: ['ignore', 'pipe', 'pipe'], ...spawnOpts(cwd, opts) });
        let stdout = '';
        let stderr = '';
        child.stdout?.on('data', (d) => (stdout += d.toString()));
        child.stderr?.on('data', (d) => (stderr += d.toString()));
        child.on('error', reject);
        child.on('exit', (code) => resolve({ code, stdout, stderr }));
    });
}

/**
 * Run several long-lived processes together. The promise settles when the user interrupts
 * (Ctrl+C) or when any process exits on its own — in both cases every other process is torn
 * down by its whole tree, so a `dotnet watch` that has spawned the app won't leak.
 */
export function parallel(cwd: string, procs: NamedProc[], log: (m: string) => void): Promise<void> {
    return new Promise((resolve) => {
        const children: { name: string; pid?: number; killed: boolean }[] = [];
        let tearingDown = false;
        let settled = false;

        const onSignal = (signal: NodeJS.Signals) => teardown(signal);
        process.on('SIGINT', onSignal);
        process.on('SIGTERM', onSignal);

        function finish() {
            process.off('SIGINT', onSignal);
            process.off('SIGTERM', onSignal);
            if (!settled) {
                settled = true;
                resolve();
            }
        }

        function teardown(signal: NodeJS.Signals) {
            if (tearingDown) return;
            tearingDown = true;
            const live = children.filter((c) => c.pid && !c.killed);
            if (live.length === 0) return finish();
            log(`\nStopping ${live.length} watcher(s)…`);
            let remaining = live.length;
            for (const c of live) {
                c.killed = true;
                treeKill(c.pid!, signal, () => {
                    if (--remaining === 0) finish();
                });
            }
        }

        for (const proc of procs) {
            const record: { name: string; pid?: number; killed: boolean } = { name: proc.name, killed: false };
            children.push(record);
            const child = spawn(proc.cmd, proc.args, { stdio: 'inherit', ...spawnOpts(cwd, proc.opts) });
            record.pid = child.pid;
            child.on('error', (err) => {
                if (!tearingDown) {
                    record.killed = true;
                    log(`Watcher "${proc.name}" failed to start: ${err.message}`);
                    teardown('SIGINT');
                }
            });
            child.on('exit', () => {
                record.killed = true;
                if (!tearingDown) {
                    log(`Watcher "${proc.name}" stopped; tearing down the rest.`);
                    teardown('SIGINT');
                }
            });
        }
    });
}

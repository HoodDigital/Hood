/**
 * hoodcms/dev — docker compose helpers.
 *
 * The health wait is a plain Node poll loop — no `until`/`xargs` shell, so it runs the same
 * on PowerShell, cmd, bash and zsh. `--env-file` is wired in so `${VAR}` substitution in the
 * compose file resolves from `.env.local` (compose only auto-reads `.env`).
 */
import fs from 'node:fs';
import path from 'node:path';
import { resolveSaPassword } from './connection';
import { capture, run } from './process';
import type { ResolvedConfig, SpawnOpts } from './types';

const delay = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

/**
 * The environment compose is spawned with. Guarantees `HOOD_SA_PASSWORD` is set so the
 * `${HOOD_SA_PASSWORD}` substitutions in the compose file always resolve — that keeps
 * `hoodcms` zero-config even without a `.env.local` (a real env / `.env.local` value wins).
 */
function composeEnv(config: ResolvedConfig, extra?: NodeJS.ProcessEnv): NodeJS.ProcessEnv {
    return { HOOD_SA_PASSWORD: resolveSaPassword(config), ...extra };
}

/**
 * The leading `compose` argv. Absolute paths + an explicit `--project-directory` keep the
 * build context and `${VAR}` substitution correct no matter which directory `hoodcms` was
 * invoked from (Hood's compose file lives at the repo root, above the package dir).
 */
export function composePrefix(config: ResolvedConfig): string[] {
    const composeFile = path.resolve(config.cwd, config.composeFile);
    const projectDir = path.dirname(composeFile);
    const argv = ['compose', '-f', composeFile, '--project-directory', projectDir];
    for (const file of config.envFiles) {
        const resolved = path.resolve(config.cwd, file);
        if (fs.existsSync(resolved)) {
            argv.push('--env-file', resolved);
        }
    }
    return argv;
}

/** `docker compose …` with the compose file + env files wired in. */
export function compose(config: ResolvedConfig, args: string[], opts?: SpawnOpts): Promise<void> {
    return run(config.cwd, 'docker', [...composePrefix(config), ...args], {
        ...opts,
        env: composeEnv(config, opts?.env),
    });
}

/**
 * Bring up the DB service and block until its container healthcheck reports `healthy`.
 * Polls `docker inspect` in a Node loop (cross-shell safe), with a hard timeout.
 */
export async function waitForDb(config: ResolvedConfig, log: (m: string) => void): Promise<void> {
    await compose(config, ['up', '-d', config.dbService]);

    const timeoutMs = 180_000;
    const start = Date.now();
    log(`Waiting for "${config.dbService}" to become healthy`);

    for (;;) {
        const ids = await capture(config.cwd, 'docker', [...composePrefix(config), 'ps', '-q', config.dbService], {
            env: composeEnv(config),
        });
        const containerId = ids.stdout.trim().split(/\r?\n/)[0];

        if (containerId) {
            const inspected = await capture(config.cwd, 'docker', [
                'inspect',
                '-f',
                '{{.State.Health.Status}}',
                containerId,
            ]);
            const status = inspected.stdout.trim();
            if (status === 'healthy') {
                process.stdout.write(' ready.\n');
                return;
            }
            if (status === 'unhealthy') {
                throw new Error(`Container ${containerId} for "${config.dbService}" reported unhealthy.`);
            }
        }

        if (Date.now() - start > timeoutMs) {
            throw new Error(`Timed out after ${timeoutMs / 1000}s waiting for "${config.dbService}" to become healthy.`);
        }
        process.stdout.write('.');
        await delay(2000);
    }
}

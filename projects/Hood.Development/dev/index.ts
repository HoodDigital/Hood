/**
 * hoodcms/dev — the public, consumer-facing entry point (HOOD-131).
 *
 * A consumer adopts Hood's dev orchestration without cloning it:
 *
 *   // hood.dev.ts (optional — a vanilla consumer needs none of this)
 *   import { defineTasks } from 'hoodcms/dev';
 *
 *   export default defineTasks({
 *     appProject: 'src/MyApp',
 *     tasks: {
 *       seed: { describe: 'Seed demo data', run: (ctx) => ctx.run('dotnet', ['run', '--', 'seed']) },
 *       // override a base target by name:
 *       build: { describe: 'Build + pack', run: (ctx) => ctx.run('dotnet', ['pack']) },
 *     },
 *   });
 *
 * The `hood-dev` bin discovers that file (or `--config <path>`), merges it over Hood's
 * defaults, and dispatches. Everything runs through tsx, so the config and any custom
 * targets are written in TypeScript with full type-checking against `DevConfig`.
 */
import fs from 'node:fs';
import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { loadEnv, resolveConnection } from './lib/connection';
import { compose, waitForDb } from './lib/docker';
import { parallel, run } from './lib/process';
import { BASE_TASK_ALIASES, baseTasks } from './lib/tasks';
import type {
    DevConfig,
    NamedProc,
    ParsedArgs,
    ResolvedConfig,
    SpawnOpts,
    TaskContext,
    TaskDefinition,
} from './lib/types';

export type {
    DevConfig,
    NamedProc,
    ParsedArgs,
    ResolvedConfig,
    SpawnOpts,
    TaskContext,
    TaskDefinition,
    WatchConfig,
} from './lib/types';

/**
 * Typed identity helper for `hood.dev.ts`. Gives editor completion + compile-time checking
 * of the config object; returns it unchanged for the bin to consume.
 */
export function defineTasks(config: DevConfig): DevConfig {
    return config;
}

/**
 * Walk up from the invocation directory to the project root — the directory that holds the
 * compose file (or, failing that, a `.sln` / `.git`). This is what lets a vanilla consumer
 * run `hood-dev` from any subdirectory, and lets Hood dogfood it from the nested `hoodcms`
 * package dir with zero config (the defaults below describe Hood's root layout).
 */
export function findRoot(start: string, composeFileName = 'docker-compose.yml'): string {
    let dir = path.resolve(start);
    for (;;) {
        try {
            if (fs.existsSync(path.join(dir, composeFileName))) return dir;
            if (fs.existsSync(path.join(dir, '.git'))) return dir;
            if (fs.readdirSync(dir).some((f) => f.toLowerCase().endsWith('.sln'))) return dir;
        } catch {
            /* unreadable dir — stop climbing */
            break;
        }
        const parent = path.dirname(dir);
        if (parent === dir) break;
        dir = parent;
    }
    return path.resolve(start);
}

/** Find the single `*.sln` at the root, falling back to `Hood.sln`. */
function discoverSolution(cwd: string): string {
    try {
        const sln = fs.readdirSync(cwd).find((f) => f.toLowerCase().endsWith('.sln'));
        if (sln) return sln;
    } catch {
        /* fall through to default */
    }
    return 'Hood.sln';
}

/** Merge a consumer `DevConfig` over Hood's defaults into a fully-resolved config. */
export function resolveConfig(config: DevConfig, cwd: string): ResolvedConfig {
    const appProject = config.appProject ?? 'projects/Hood.Development';
    return {
        cwd,
        solution: config.solution ?? discoverSolution(cwd),
        appProject,
        schemaUpgrade: config.schemaUpgrade ?? [
            'dotnet',
            'run',
            '--project',
            'projects/Hood.SchemaTool',
            '-c',
            'Release',
            '--',
            'upgrade',
        ],
        composeFile: config.composeFile ?? 'docker-compose.yml',
        dbService: config.dbService ?? 'sqlserver',
        appService: config.appService ?? 'app',
        dbPort: config.dbPort ?? 14331,
        database: config.database ?? 'Hood.Web',
        connection: config.connection,
        saPassword: config.saPassword ?? 'Hood_Dev_Passw0rd!',
        envFiles: config.envFiles ?? ['.env.local'],
        watch: {
            frontend: config.watch?.frontend ?? [
                ['pnpm', 'run', 'watch-scss'],
                ['pnpm', 'run', 'watch-tsc'],
            ],
            backend: config.watch?.backend ?? ['dotnet', 'watch', '--project', appProject],
        },
        tasks: config.tasks ?? {},
    };
}

/** Build the command registry: Hood's base targets, then consumer overrides/extensions. */
function buildRegistry(config: ResolvedConfig): Map<string, TaskDefinition> {
    const registry = new Map<string, TaskDefinition>(Object.entries(baseTasks()));
    for (const [name, def] of Object.entries(config.tasks)) {
        registry.set(name, def);
    }
    return registry;
}

/** Resolve a command (honouring aliases) to its definition. */
function lookup(registry: Map<string, TaskDefinition>, command: string): TaskDefinition | undefined {
    return registry.get(BASE_TASK_ALIASES[command] ?? command);
}

/** Parse argv into a (possibly two-word) command, positionals, and `--flags`. */
export function parseArgs(argv: string[]): ParsedArgs {
    const positionals: string[] = [];
    const flags: Record<string, string | boolean> = {};

    for (let i = 0; i < argv.length; i++) {
        const token = argv[i];
        if (token.startsWith('--')) {
            const eq = token.indexOf('=');
            if (eq >= 0) {
                flags[token.slice(2, eq)] = token.slice(eq + 1);
            } else {
                const key = token.slice(2);
                const next = argv[i + 1];
                if (next !== undefined && !next.startsWith('--')) {
                    flags[key] = next;
                    i++;
                } else {
                    flags[key] = true;
                }
            }
        } else {
            positionals.push(token);
        }
    }

    const command = positionals.shift() ?? '';
    return { command, args: positionals, flags };
}

/** Build the `TaskContext` handed to every target. */
function makeContext(
    config: ResolvedConfig,
    parsed: ParsedArgs,
    registry: Map<string, TaskDefinition>,
): TaskContext {
    let cachedConnection: string | undefined;
    const log = (msg: string) => console.log(msg);

    const ctx: TaskContext = {
        config,
        args: parsed.args,
        flags: parsed.flags,
        cwd: config.cwd,
        connection: () => (cachedConnection ??= resolveConnection(config, parsed.flags)),
        run: (cmd: string, args: string[], opts?: SpawnOpts) => run(config.cwd, cmd, args, opts),
        parallel: (procs: NamedProc[]) => parallel(config.cwd, procs, log),
        compose: (args: string[], opts?: SpawnOpts) => compose(config, args, opts),
        waitForDb: () => waitForDb(config, log),
        invoke: (command: string) => {
            const def = lookup(registry, command);
            if (!def) throw new Error(`Unknown command: ${command}`);
            return Promise.resolve(def.run(ctx));
        },
        log,
    };
    return ctx;
}

/** Print the available commands. */
function printHelp(registry: Map<string, TaskDefinition>): void {
    const names = [...registry.keys()].sort();
    const width = Math.max(...names.map((n) => n.length), ...Object.keys(BASE_TASK_ALIASES).map((a) => a.length));
    console.log('hood-dev — Hood CMS local dev commands\n');
    console.log('Usage: hood-dev <command> [--connection <str>] [--config <hood.dev.ts>]\n');
    console.log('Commands:');
    for (const name of names) {
        console.log(`  ${name.padEnd(width)}  ${registry.get(name)!.describe}`);
    }
    for (const [alias, target] of Object.entries(BASE_TASK_ALIASES)) {
        console.log(`  ${alias.padEnd(width)}  Alias for "${target}".`);
    }
}

/** Resolve config, load env, build the registry, and dispatch one command. Returns an exit code. */
async function execute(userConfig: DevConfig, argv: string[], cwd: string): Promise<number> {
    const parsed = parseArgs(argv);
    const config = resolveConfig(userConfig, cwd);
    loadEnv(cwd, config.envFiles);
    const registry = buildRegistry(config);

    if (!parsed.command || parsed.command === 'help' || parsed.flags.help === true) {
        printHelp(registry);
        return 0;
    }

    const def = lookup(registry, parsed.command);
    if (!def) {
        console.error(`Unknown command: "${parsed.command}"\n`);
        printHelp(registry);
        return 1;
    }

    const ctx = makeContext(config, parsed, registry);
    await def.run(ctx);
    return 0;
}

/** Load the optional `hood.dev.ts` (or `--config <path>`). Returns `{}` when there is none. */
async function loadUserConfig(cwd: string, configFlag: string | boolean | undefined): Promise<DevConfig> {
    const explicit = typeof configFlag === 'string' ? path.resolve(cwd, configFlag) : undefined;
    if (explicit && !fs.existsSync(explicit)) {
        throw new Error(`--config file not found: ${explicit}`);
    }
    const file = explicit ?? path.resolve(cwd, 'hood.dev.ts');
    if (!fs.existsSync(file)) return {};

    const mod = await import(pathToFileURL(file).href);
    const config = (mod.default ?? mod.config ?? {}) as DevConfig;
    return config;
}

/**
 * The bin entry point: discover `hood.dev.ts`, merge it over the defaults, and run the
 * requested command. Returns the process exit code (does not call `process.exit`).
 */
export async function runCli(argv: string[] = process.argv.slice(2), startCwd: string = process.cwd()): Promise<number> {
    const root = findRoot(startCwd);
    const parsed = parseArgs(argv);
    const userConfig = await loadUserConfig(root, parsed.flags.config);
    return execute(userConfig, argv, root);
}

/**
 * Programmatic entry for a consumer who would rather wire the runner into their own bin than
 * ship a `hood.dev.ts`:
 *
 *   import { registerDevCommands } from 'hoodcms/dev';
 *   registerDevCommands({ appProject: 'src/MyApp' }).run();
 */
export function registerDevCommands(config: DevConfig = {}): { run(argv?: string[], startCwd?: string): Promise<number> } {
    return {
        run: (argv: string[] = process.argv.slice(2), startCwd: string = process.cwd()) =>
            execute(config, argv, findRoot(startCwd)),
    };
}

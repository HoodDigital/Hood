/**
 * hoodcms/dev — shared types for the cross-platform dev-command layer.
 *
 * These types are the public contract a consumer sees through `hoodcms/dev`. The
 * factory (`defineTasks`) is typed against `DevConfig`; custom targets receive a
 * `TaskContext`.
 */

/** A single dev target — Hood's base targets and any a consumer registers. */
export interface TaskDefinition {
    /** One-line help text shown by `hoodcms help`. */
    describe: string;
    /** Implementation. Throw (or let a spawned process exit non-zero) to fail the command. */
    run: (ctx: TaskContext) => Promise<void> | void;
}

/**
 * A restore source plugged into `db restore <file>`, dispatched by file extension.
 * Hood ships a built-in `.bacpac` provider; a consumer registers more through `hood.dev.ts`.
 */
export interface RestoreProvider {
    /** Lower-case extensions (with the dot) this provider handles, e.g. `['.bacpac']`. */
    extensions: string[];
    /** One-line description shown in help and dispatch errors. */
    describe: string;
    /** Restore `file` into the resolved database. Throw to fail the command. */
    restore(ctx: RestoreContext): Promise<void> | void;
}

/** What a `RestoreProvider` receives — the producer→file→provider contract, nothing source-specific. */
export interface RestoreContext {
    /** Absolute path to the dump/export file being restored. */
    file: string;
    /** The resolved, loopback-pinned host connection string (shared `resolveConnection()` chain). */
    connection: string;
    /** The full task context (`run`, `log`, `config`, …). */
    task: TaskContext;
}

/** Frontend + backend watch commands wired together by the `watch` target. */
export interface WatchConfig {
    /** Long-lived frontend watchers, each `[cmd, ...args]` (default: scss + tsc watch via pnpm). */
    frontend?: string[][];
    /** The backend watcher `[cmd, ...args]` (default: `dotnet watch --project <appProject>`). */
    backend?: string[];
}

/**
 * Consumer-facing configuration. Every field is optional — a vanilla consumer needs
 * no `hood.dev.ts` at all and runs on the defaults below (see `resolveConfig`).
 */
export interface DevConfig {
    /** Solution file built/cleaned by `build` / `clean` (default: the single `*.sln` at the root). */
    solution?: string;
    /** The runnable web project for `run` / `dev` (default: `projects/Hood.Development`). */
    appProject?: string;
    /**
     * Argv for the schema upgrade invoked by `db-upgrade` — the resolved `--connection`
     * is appended automatically (default: Hood's `Hood.SchemaTool` project, `upgrade`).
     */
    schemaUpgrade?: string[];
    /** docker compose file (default: `docker-compose.yml`). */
    composeFile?: string;
    /** compose DB service name (default: `sqlserver`). */
    dbService?: string;
    /** compose app service name (default: `app`). */
    appService?: string;
    /** Host port the DB container publishes on (default: `14331`). */
    dbPort?: number;
    /** Database name used when building the default connection (default: `Hood.Web`). */
    database?: string;
    /** Explicit connection string — highest precedence after `--connection`. */
    connection?: string;
    /**
     * Fallback SA password used to build the default Docker connection and the `sql` shell
     * when nothing else is set (default: `Hood_Dev_Passw0rd!`). Real secrets belong in
     * `.env.local`, never here.
     */
    saPassword?: string;
    /** `.env` files loaded (in order) before resolving config (default: `['.env.local']`). */
    envFiles?: string[];
    /**
     * Extra `db restore` providers, or overrides of a built-in by extension (last match wins).
     * Merged after Hood's built-ins (the `.bacpac` provider).
     */
    restoreProviders?: RestoreProvider[];
    /** Watch commands wired together by `dev`. */
    watch?: WatchConfig;
    /** Extra targets, or overrides of a base target by name. */
    tasks?: Record<string, TaskDefinition>;
}

/** `DevConfig` with every default filled in — what tasks actually see. */
export type ResolvedConfig = Required<Omit<DevConfig, 'connection' | 'tasks' | 'watch'>> & {
    connection?: string;
    watch: Required<WatchConfig>;
    tasks: Record<string, TaskDefinition>;
    /** Absolute root the commands run from (the invocation directory). */
    cwd: string;
};

/** Parsed command line: the (possibly two-word) command, trailing positionals, and flags. */
export interface ParsedArgs {
    command: string;
    args: string[];
    flags: Record<string, string | boolean>;
}

/** Everything a task needs — config, parsed args, and the cross-platform helpers. */
export interface TaskContext {
    config: ResolvedConfig;
    /** Positional args after the command. */
    args: string[];
    /** Parsed `--flags`. */
    flags: Record<string, string | boolean>;
    /** The invocation directory. */
    cwd: string;
    /** Resolve the host-side connection string (cached; pinned to `127.0.0.1`). */
    connection(): string;
    /** Run a command to completion via `cross-spawn` (inherited stdio); rejects on non-zero exit. */
    run(cmd: string, args: string[], opts?: SpawnOpts): Promise<void>;
    /** Run several long-lived processes together; a single Ctrl+C tears them all down. */
    parallel(procs: NamedProc[]): Promise<void>;
    /** `docker compose …` with the compose file and `--env-file` wired in. */
    compose(args: string[], opts?: SpawnOpts): Promise<void>;
    /** Start the DB service and block (Node poll loop) until its healthcheck passes. */
    waitForDb(): Promise<void>;
    /** Run another target by name (resolves consumer overrides) — e.g. `up` calls `db-upgrade`. */
    invoke(command: string): Promise<void>;
    /** Print a narrator line. */
    log(msg: string): void;
}

export interface SpawnOpts {
    /** Extra environment merged onto `process.env`. */
    env?: NodeJS.ProcessEnv;
    /** Working directory (default: the invocation directory). */
    cwd?: string;
}

export interface NamedProc {
    name: string;
    cmd: string;
    args: string[];
    opts?: SpawnOpts;
}

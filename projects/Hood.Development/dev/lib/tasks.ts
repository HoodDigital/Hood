/**
 * hoodcms/dev — Hood's base targets (HOOD-131).
 *
 * The cross-platform replacement for the old bash `Makefile`. Every target is defined here
 * once; a consumer registers extra targets (or overrides one of these by name) through
 * `hood.dev.ts`. Targets only touch the filesystem/processes through the `TaskContext`
 * helpers, so they behave identically on POSIX and Windows.
 */
import fs from 'node:fs';
import path from 'node:path';
import { resolveSaPassword } from './connection';
import type { TaskContext, TaskDefinition } from './types';

/** Copy `.env.example` → `.env.local` when the latter is missing (idempotent bootstrap). */
function ensureEnvLocal(ctx: TaskContext): void {
    const example = path.resolve(ctx.cwd, '.env.example');
    const local = path.resolve(ctx.cwd, '.env.local');
    if (fs.existsSync(local)) {
        ctx.log('.env.local already present — leaving it untouched.');
        return;
    }
    if (fs.existsSync(example)) {
        fs.copyFileSync(example, local);
        ctx.log('Created .env.local from .env.example — edit it to set your SA password.');
    } else {
        ctx.log('No .env.example found; skipping .env.local bootstrap.');
    }
}

export function baseTasks(): Record<string, TaskDefinition> {
    return {
        build: {
            describe: 'Build the whole solution (Release).',
            run: (ctx) => ctx.run('dotnet', ['build', ctx.config.solution, '-c', 'Release']),
        },

        up: {
            describe: 'Start the full stack — DB created/upgraded first, then the app.',
            run: async (ctx) => {
                await ctx.invoke('db-upgrade');
                await ctx.compose(['up', '-d', '--build']);
            },
        },

        down: {
            describe: 'Stop containers (keeps the DB volume).',
            run: (ctx) => ctx.compose(['down']),
        },

        clean: {
            describe: 'Stop containers, drop the DB volume, and clean the solution.',
            run: async (ctx) => {
                await ctx.compose(['down', '-v']);
                await ctx.run('dotnet', ['clean', ctx.config.solution]);
            },
        },

        logs: {
            describe: 'Tail app + DB container logs.',
            run: (ctx) => ctx.compose(['logs', '-f']),
        },

        sql: {
            describe: 'Open a sqlcmd shell inside the DB container.',
            run: (ctx) =>
                ctx.compose([
                    'exec',
                    ctx.config.dbService,
                    '/opt/mssql-tools18/bin/sqlcmd',
                    '-S',
                    'localhost',
                    '-U',
                    'sa',
                    '-P',
                    resolveSaPassword(ctx.config),
                    '-C',
                ]),
        },

        run: {
            describe: 'Run the app natively against the Docker SQL Server.',
            run: (ctx) =>
                // --no-launch-profile: the app's only launchSettings profiles are Windows-only
                // IIS Express ones, unusable by `dotnet run`. The CLI supplies env directly instead.
                ctx.run('dotnet', ['run', '--project', ctx.config.appProject, '--no-launch-profile'], {
                    env: {
                        ASPNETCORE_ENVIRONMENT: 'Development',
                        ConnectionStrings__DefaultConnection: ctx.connection(),
                    },
                }),
        },

        'db-up': {
            describe: 'Start SQL Server only and wait until healthy.',
            run: (ctx) => ctx.waitForDb(),
        },

        'db-upgrade': {
            describe: 'Create (if absent) + upgrade the Docker database via the schema tool.',
            run: async (ctx) => {
                await ctx.waitForDb();
                const [cmd, ...args] = ctx.config.schemaUpgrade;
                await ctx.run(cmd, [...args, '--connection', ctx.connection()]);
            },
        },

        watch: {
            describe: 'Watch frontend + backend together (single Ctrl+C stops both).',
            run: async (ctx) => {
                await ctx.waitForDb();
                const { frontend, backend } = ctx.config.watch;
                const appCwd = path.resolve(ctx.cwd, ctx.config.appProject);
                const procs = [
                    // Frontend watchers run in the app/package dir (where the pnpm scripts live).
                    ...frontend.map((argv, i) => ({
                        name: `frontend:${i + 1}`,
                        cmd: argv[0],
                        args: argv.slice(1),
                        opts: { cwd: appCwd },
                    })),
                    {
                        name: 'backend',
                        cmd: backend[0],
                        args: backend.slice(1),
                        opts: {
                            env: {
                                ASPNETCORE_ENVIRONMENT: 'Development',
                                ConnectionStrings__DefaultConnection: ctx.connection(),
                                // The frontend watchers emit into wwwroot while dotnet watch runs;
                                // without this, Hot Reload's static-file handler crashes with
                                // "Unexpected character 'true'" (roslyn#84062).
                                DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING: '1',
                            },
                        },
                    },
                ];
                ctx.log('Starting watchers — press Ctrl+C once to stop everything.');
                await ctx.parallel(procs);
            },
        },

        setup: {
            describe: 'Bootstrap local dev: create .env.local, install JS deps, restore .NET.',
            run: async (ctx) => {
                ensureEnvLocal(ctx);
                await ctx.run('pnpm', ['install'], { cwd: path.resolve(ctx.cwd, ctx.config.appProject) });
                await ctx.run('dotnet', ['restore', ctx.config.solution]);
            },
        },
    };
}

/** Aliases that resolve to a base target — `deps` is a synonym for `setup`. */
export const BASE_TASK_ALIASES: Record<string, string> = { deps: 'setup' };

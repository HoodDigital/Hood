/**
 * hoodcms/dev — the `db restore <file>` provider seam.
 *
 * A generic, file-based restore primitive: a producer (e.g. `hood cli az pull`, an S3 export,
 * a teammate's dump) writes a local file, and `db restore <file>` dispatches by extension to a
 * `RestoreProvider`. The file path is the entire contract — no source (Azure, S3, …) ever enters
 * this layer, and `hood-schema` stays a pure schema-migration tool. Consumers register extra
 * providers through `hood.dev.ts` without forking.
 */
import fs from 'node:fs';
import path from 'node:path';
import readline from 'node:readline';
import type { RestoreProvider, TaskContext } from './types';

/**
 * `.bacpac` via sqlpackage — the `Microsoft.SqlPackage` cross-platform .NET tool. Imports over
 * the host TCP port using the shared connection, so there is **no Docker coupling**. sqlpackage
 * drops + recreates the database internally, which also sidesteps the `.bak` `RESTORING`-state
 * footgun — so the DB must NOT be pre-created here (no `EnsureDatabase` before restore).
 *
 * Install once: `dotnet tool install --global microsoft.sqlpackage`.
 */
const bacpacProvider: RestoreProvider = {
    extensions: ['.bacpac'],
    describe: 'SQL Server .bacpac import via sqlpackage',
    restore: ({ file, connection, task }) =>
        task.run('sqlpackage', [
            '/Action:Import',
            `/SourceFile:${file}`,
            `/TargetConnectionString:${connection}`,
        ]),
};

/** Hood's built-in restore providers. `.bacpac` first — the Docker-free, `RESTORING`-safe path. */
export function builtinRestoreProviders(): RestoreProvider[] {
    return [bacpacProvider];
}

/** Resolve the provider for a file by extension. Last match wins, so a consumer overrides a built-in. */
export function providerFor(providers: RestoreProvider[], file: string): RestoreProvider | undefined {
    const ext = path.extname(file).toLowerCase();
    let match: RestoreProvider | undefined;
    for (const provider of providers) {
        if (provider.extensions.some((e) => e.toLowerCase() === ext)) match = provider;
    }
    return match;
}

/**
 * The `db restore <file>` flow: validate the file, confirm (restore is destructive), dispatch to
 * the extension's provider, then point the user at `db upgrade` to reconcile the schema.
 */
export async function runRestore(task: TaskContext): Promise<void> {
    const file = task.args[0];
    if (!file) throw new Error('db restore: a file path is required — `hoodcms db restore <file>`.');

    const abs = path.resolve(task.cwd, file);
    if (!fs.existsSync(abs)) throw new Error(`db restore: file not found: ${abs}`);

    const provider = providerFor(task.config.restoreProviders, abs);
    if (!provider) {
        const known = [...new Set(task.config.restoreProviders.flatMap((p) => p.extensions))].join(', ');
        const ext = path.extname(abs) || '(no extension)';
        throw new Error(`db restore: no provider for "${ext}". Known extensions: ${known}.`);
    }

    await confirmDestructive(task, abs, provider.describe);

    const connection = task.connection();
    task.log(`Restoring ${path.basename(abs)} via ${provider.describe}…`);
    await provider.restore({ file: abs, connection, task });
    task.log('Restore complete. Run `hoodcms db upgrade` to reconcile the schema (idempotent).');
}

/** Destructive-action guard: `--force`/`--yes` skips the prompt; otherwise require an interactive "yes". */
async function confirmDestructive(task: TaskContext, file: string, describe: string): Promise<void> {
    if (task.flags.force === true || task.flags.yes === true) return;
    if (!process.stdin.isTTY) {
        throw new Error('db restore overwrites the target database. Re-run with --force in a non-interactive shell.');
    }

    const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
    const answer = await new Promise<string>((resolve) =>
        rl.question(
            `This will OVERWRITE the database from ${path.basename(file)} (${describe}). Type "yes" to continue: `,
            resolve,
        ),
    );
    rl.close();
    if (answer.trim().toLowerCase() !== 'yes') throw new Error('db restore: aborted.');
}

/**
 * hoodcms/dev — environment + connection resolution (HOOD-131).
 *
 * One source of truth for the dev database connection, shared with the app: the same
 * `appsettings.Development.json` the app reads, plus the `.env.local` overlay. Host-side
 * connections are pinned to `127.0.0.1` (never `localhost`) to dodge the Docker Desktop
 * IPv6 pre-login stall.
 */
import fs from 'node:fs';
import path from 'node:path';
import { config as loadDotenv } from 'dotenv';
import type { ParsedArgs, ResolvedConfig } from './types';

/**
 * Load the `.env` files into `process.env`. dotenv does not override variables that are
 * already set, so the real shell environment always wins over a file — which is exactly
 * the precedence the resolution chain assumes.
 */
export function loadEnv(cwd: string, envFiles: string[]): void {
    for (const file of envFiles) {
        const resolved = path.resolve(cwd, file);
        if (fs.existsSync(resolved)) {
            loadDotenv({ path: resolved });
        }
    }
}

/**
 * Pin a host-side connection to the IPv4 loopback. `localhost` resolves to `::1` first on
 * many machines, and SQL Server's pre-login handshake stalls on the IPv6 attempt behind
 * Docker Desktop before falling back — so rewrite the `Server=localhost` host part.
 */
export function pinLoopback(connection: string): string {
    return connection.replace(/(Server\s*=\s*)localhost\b/i, '$1127.0.0.1');
}

/** Build the default Docker connection from an SA password — the zero-config fallback. */
function buildDefaultConnection(config: ResolvedConfig, password: string): string {
    return [
        `Server=127.0.0.1,${config.dbPort}`,
        `Database=${config.database}`,
        'User Id=sa',
        `Password=${password}`,
        'TrustServerCertificate=True',
        'Encrypt=False',
        'MultipleActiveResultSets=True',
    ].join(';');
}

/** Read `ConnectionStrings:DefaultConnection` from `appsettings.Development.json`, if present. */
function fromAppSettings(config: ResolvedConfig): string | undefined {
    const candidates = [
        path.resolve(config.cwd, config.appProject, 'appsettings.Development.json'),
        path.resolve(config.cwd, 'appsettings.Development.json'),
    ];
    for (const file of candidates) {
        if (!fs.existsSync(file)) continue;
        try {
            const json = JSON.parse(fs.readFileSync(file, 'utf8'));
            const conn = json?.ConnectionStrings?.DefaultConnection;
            if (typeof conn === 'string' && conn.trim()) return conn;
        } catch (err) {
            throw new Error(`Failed to parse ${file}: ${(err as Error).message}`);
        }
    }
    return undefined;
}

/**
 * Resolve the one host-side connection string used by every native command (db upgrade,
 * run). Precedence:
 *   1. `--connection <value>`
 *   2. `config.connection` (from `hood.dev.ts`)
 *   3. `HOOD_CONNECTION`
 *   4. `ConnectionStrings__DefaultConnection`
 *   5. `appsettings.Development.json`
 *   6. default Docker connection built from `HOOD_SA_PASSWORD` / `config.saPassword`
 *      (this is what keeps a vanilla consumer zero-config)
 *   7. error
 */
export function resolveConnection(config: ResolvedConfig, flags: ParsedArgs['flags']): string {
    const flagConn = typeof flags.connection === 'string' ? flags.connection : undefined;
    if (flagConn) return pinLoopback(flagConn);
    if (config.connection) return pinLoopback(config.connection);
    if (process.env.HOOD_CONNECTION) return pinLoopback(process.env.HOOD_CONNECTION);
    if (process.env.ConnectionStrings__DefaultConnection) {
        return pinLoopback(process.env.ConnectionStrings__DefaultConnection);
    }
    const fromApp = fromAppSettings(config);
    if (fromApp) return pinLoopback(fromApp);

    const password = process.env.HOOD_SA_PASSWORD || config.saPassword;
    if (password) return buildDefaultConnection(config, password);

    throw new Error(
        'Could not resolve a database connection. Set --connection, HOOD_CONNECTION, ' +
            'ConnectionStrings__DefaultConnection, a connection in appsettings.Development.json, ' +
            'or HOOD_SA_PASSWORD (copy .env.example to .env.local).',
    );
}

/** The effective SA password for the `sql` shell (env wins, then config default). */
export function resolveSaPassword(config: ResolvedConfig): string {
    return process.env.HOOD_SA_PASSWORD || config.saPassword;
}

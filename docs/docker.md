# Docker setup — local dev for Hood CMS

The repo ships a [`docker-compose.yml`](../docker-compose.yml) that runs **SQL Server + the Hood web host** in containers, so you can build and run the app end-to-end locally in a few minutes. This is the supported way to build & test every PR before it lands.

This file covers **local dev only**. For repo basics see [readme.md](../readme.md).

> **Framework note:** Hood targets **net10.0 + EF Core 10**. The .NET SDK is pinned via `global.json` and the target framework is centralised in `Directory.Build.props`. Database schema seeding is tracked in HOOD-53.

## Prerequisites

- Docker Engine + Docker Compose v2 (`docker compose ...` — no hyphen).
- [Node.js 22+](https://nodejs.org) and **pnpm** (via corepack: `corepack enable`) — the dev commands run on `hoodcms/dev`, a small cross-platform orchestration layer (works on Windows/PowerShell, macOS, and Linux).
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — to build/run the app natively (`pnpm hoodcms run`) rather than in a container. The exact SDK is pinned in `global.json`.

## The `hoodcms` commands

Local dev is driven by **`hoodcms`** — a cross-platform replacement for the old `Makefile`, published as the `hoodcms/dev` subpath and run through [tsx](https://tsx.is). Run the commands with `pnpm hoodcms …` from `projects/Hood.Development`:

```bash
cd projects/Hood.Development
pnpm hoodcms            # list all commands
pnpm hoodcms up         # start everything — DB created/upgraded first, then the app
```

`hoodcms` walks up to the repo root automatically (where `docker-compose.yml` and `Hood.sln` live), so it doesn't matter that the package sits one level down. There's no shell-specific syntax — the same commands run identically on PowerShell, cmd, bash and zsh.

### Available commands

| Command | Does |
|---|---|
| `pnpm hoodcms build` | `dotnet build Hood.sln -c Release` |
| `pnpm hoodcms up` | Upgrade the DB, then build + start the full stack (detached) |
| `pnpm hoodcms down` | Stop containers, keep the DB volume |
| `pnpm hoodcms clean` | Stop containers, drop the DB volume, `dotnet clean` |
| `pnpm hoodcms logs` | Tail app + DB container logs |
| `pnpm hoodcms sql` | `sqlcmd` shell inside the SQL container |
| `pnpm hoodcms run` | Run the app natively against the Docker DB (port `14331`) |
| `pnpm hoodcms db-up` | Start SQL Server only, wait until healthy |
| `pnpm hoodcms db-upgrade` | Create (if absent) + upgrade the database via the schema tool |
| `pnpm hoodcms watch` | Watch frontend + backend together — a single Ctrl+C stops both |
| `pnpm hoodcms setup` | Bootstrap: create `.env.local`, install JS deps, restore .NET (alias: `deps`) |

## Configuration — `.env.local`

The local SA password is **not** hardcoded anywhere. It comes from `${HOOD_SA_PASSWORD}`, resolved from `.env.local` (gitignored) with `.env.example` as the committed template:

```bash
cp .env.example .env.local   # then edit HOOD_SA_PASSWORD if you like
# or let setup do it for you:
cd projects/Hood.Development && pnpm hoodcms setup
```

`hoodcms` loads `.env.local`, passes it to docker compose, and resolves a single connection string shared by the app, the DB container, and the schema tool. The resolution order is:

1. `--connection "<str>"`
2. `HOOD_CONNECTION`
3. `ConnectionStrings__DefaultConnection`
4. `appsettings.Development.json`
5. the default Docker connection built from `HOOD_SA_PASSWORD` (zero-config fallback)

Host-side connections are pinned to **`127.0.0.1`** (not `localhost`) to avoid the Docker Desktop IPv6 pre-login stall.

Even with **no** `.env.local`, `hoodcms` works out of the box: it supplies the default dev password automatically. You only need `.env.local` to change the password or run raw `docker compose` (see below).

## What's in the compose file

| Service | Container | Host port | Purpose |
|---|---|---|---|
| `sqlserver` | `hood-sqlserver` | `14331` → 1433 | SQL Server 2022 Express. Data persists in the `sqlserver-data` named volume. |
| `app` | `hood-app` | `5070` → 8080 | The `Hood.Development` ASP.NET Core host, built from [`Dockerfile`](../Dockerfile). |

Host port `14331` avoids clashes with a native SQL Server (`1433`) or other local dev stacks.

The app's **Data Protection keys** persist in the `hood-keys` named volume (mounted at `/keys`, set via `Hood__DataProtectionKeyPath`). This keeps antiforgery tokens and auth cookies valid across container rebuilds — without it the key ring resets on every rebuild and you'd hit *"The antiforgery token could not be decrypted"* until you cleared cookies.

## Running the app natively (faster rebuilds / debugging)

Start just the database in Docker and run the host on the host machine:

```bash
cd projects/Hood.Development
pnpm hoodcms db-upgrade     # SQL Server up + healthy, then create/upgrade the DB (idempotent)
pnpm hoodcms run            # dotnet run, pointed at the Docker SQL Server on 127.0.0.1,14331
# App available at http://localhost:5070 (or the Kestrel default)
```

`pnpm hoodcms run` sets `ConnectionStrings__DefaultConnection` to target the container. Without it, [`appsettings.json`](../projects/Hood.Development/appsettings.json) points at `localhost\SQLEXPRESS` for a native SQL Server install.

### Watch mode

`pnpm hoodcms watch` runs the frontend asset watchers and `dotnet watch` together. A single Ctrl+C tears **both** down with no orphaned watchers (the watchers are stopped by process tree — SIGINT on POSIX, `taskkill /T /F` on Windows). Static-file Hot Reload handling is suppressed for the backend watcher so it doesn't fight the frontend emit into `wwwroot`.

## Database state

The stack brings up an **empty** SQL Server. `pnpm hoodcms up` (and `pnpm hoodcms db-upgrade`) apply the schema via the schema tool before the app starts; the app itself does **no** automatic migration or seeding at startup. The full schema/runner reference is in [`sql/README.md`](../sql/README.md).

To open a SQL shell inside the container:

```bash
pnpm hoodcms sql   # sqlcmd into hood-sqlserver
```

## Raw `docker compose` (without hoodcms)

`hoodcms` is the supported path, but the compose file still works directly — you just have to supply the SA password yourself, since it's no longer hardcoded:

```bash
cp .env.example .env.local
docker compose --env-file .env.local up --build
# App available at http://localhost:5070
```

## Extending the dev commands (consumers)

A downstream app that depends on `hoodcms` adopts the same commands without cloning anything. Add a script alias and an **optional** `hood.dev.ts` at the app root:

```jsonc
// package.json
{ "scripts": { "hood": "hoodcms" } }
```

```ts
// hood.dev.ts (optional — only if you need to override config or add targets)
import { defineTasks } from 'hoodcms/dev';

export default defineTasks({
  appProject: 'src/MyApp',           // override any default
  tasks: {
    seed: {                          // register an extra target
      describe: 'Seed demo data',
      run: (ctx) => ctx.run('dotnet', ['run', '--project', ctx.config.appProject, '--', 'seed']),
    },
    build: {                         // or override a base target by name
      describe: 'Build + pack',
      run: (ctx) => ctx.run('dotnet', ['pack']),
    },
  },
});
```

A vanilla consumer with Hood's layout needs no `hood.dev.ts` at all — the defaults resolve the solution, compose file, ports and connection automatically. Point `hoodcms` at a different config file with `--config <path>`.

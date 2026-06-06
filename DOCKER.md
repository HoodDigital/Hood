# Docker setup — local dev for Hood CMS

The repo ships a [`docker-compose.yml`](docker-compose.yml) that runs **SQL Server + the Hood web host** in containers, so you can build and run the app end-to-end locally in a few minutes. This is the supported way to build & test every PR before it lands.

This file covers **local dev only**. For repo basics see [readme.md](readme.md).

> **Framework note:** Hood targets **net10.0 + EF Core 10**. The .NET SDK is pinned via `global.json` and the target framework is centralised in `Directory.Build.props`. The CI/CD pipeline is tracked in HOOD-52 and database schema seeding in HOOD-53.

## Prerequisites

- Docker Engine + Docker Compose v2 (`docker compose ...` — no hyphen).
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — only needed to run the app natively (`make run`) rather than in a container. The exact SDK is pinned in `global.json`.

## What's in the compose file

| Service | Container | Host port | Purpose |
|---|---|---|---|
| `sqlserver` | `hood-sqlserver` | `14331` → 1433 | SQL Server 2022 Express. Data persists in the `sqlserver-data` named volume. |
| `app` | `hood-app` | `5070` → 8080 | The `Hood.Development` ASP.NET Core host, built from [`Dockerfile`](Dockerfile). |

Host port `14331` avoids clashes with a native SQL Server (`1433`) or other local dev stacks. The SA password defaults to `Hood_Dev_Passw0rd!` — fine for throwaway local use; change it for anything exposed.

The app's **Data Protection keys** persist in the `hood-keys` named volume (mounted at `/keys`, set via `Hood__DataProtectionKeyPath`). This keeps antiforgery tokens and auth cookies valid across container rebuilds — without it the key ring resets on every rebuild and you'd hit *"The antiforgery token could not be decrypted"* until you cleared cookies.

## Quick start

```bash
docker compose up --build
# App available at http://localhost:5070
```

Or via the [`Makefile`](Makefile):

```bash
make up        # start everything — creates/upgrades the database first, then the app
make logs      # tail container logs
make down      # stop (keeps the DB volume)
make clean     # stop + drop the DB volume
```

## Running the app natively (faster rebuilds / debugging)

Start just the database in Docker and run the host on the host machine:

```bash
make db-up      # SQL Server only, waits until healthy
make db-upgrade # create (if absent) + upgrade the database via hood-schema (idempotent)
make run       # dotnet run, pointed at the Docker SQL Server on localhost,14331
# App available at http://localhost:5070 (or the Kestrel default)
```

`make run` overrides `ConnectionStrings__DefaultConnection` to target the container. Without the override, [`appsettings.json`](projects/Hood.Development/appsettings.json) points at `localhost\SQLEXPRESS` for a native SQL Server install.

## Database state

The stack brings up an **empty** SQL Server — there is **no automatic migration or seeding at startup** (that was deliberately removed; schema is applied out-of-band). On first boot Hood detects the unseeded database, logs that it needs initialising, and still starts and serves. Applying the schema + seed data is tracked in **HOOD-53** (CLI / init-script); until that lands, expect data-backed pages to be empty or to surface the "database not initialised" path.

To open a SQL shell inside the container:

```bash
make sql       # sqlcmd into hood-sqlserver
```

## Common commands

| Command | Does |
|---|---|
| `make build` | `dotnet build Hood.sln -c Release` |
| `make up` | Build + start the full stack (detached) |
| `make db-up` | Start SQL Server only, wait until healthy |
| `make run` | Run the app natively against the Docker DB |
| `make down` | Stop containers, keep the DB volume |
| `make clean` | Stop containers, drop the DB volume, `dotnet clean` |
| `make logs` | Tail app + DB logs |
| `make sql` | `sqlcmd` shell inside the SQL container |

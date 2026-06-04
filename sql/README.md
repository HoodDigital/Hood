# Hood CMS — database schema & upgrades

Hood's schema is shipped as **plain, idempotent, forward-only SQL scripts**, applied by the **`hood-schema` runner** (DbUp) — or by hand. EF Core migrations live in `projects/Hood.Core/Migrations` and are used **only to author/regenerate** the SQL; nothing applies migrations at runtime, and no EF migration-history table is used.

Every script is guarded (`IF OBJECT_ID(...) IS NULL` / `IF NOT EXISTS`) and safe to re-run.

## Applying the schema — `hood-schema` (recommended)

`hood-schema` is a .NET tool that applies the schema with one command, for **both fresh installs and upgrades** — DbUp figures out what's needed from its journal (`dbo.SchemaVersions`) and never re-runs an applied script.

```bash
dotnet tool install --global Hood.SchemaTool
hood-schema upgrade --connection "Server=…;Database=…;User Id=…;Password=…;TrustServerCertificate=True"
```

- **Fresh database** → creates it (if absent) and applies everything → standard ASP.NET Identity schema.
- **Existing v6.0 or v6.1 database** → the table scripts skip (already present) and the delta tiers run in order (6.1 then 7.0); either way you land on v7. No baseline step.
- **Re-run** → a clean no-op.
- **Your own project SQL** → `--scripts <folder>` applies your `.sql` files **after** Hood's core, in the same journal. Name them so they sort in apply order (e.g. `001_…`, `002_…`); keep them idempotent + forward-only.

The Hood core scripts ship **embedded in `Hood.Core`**, so the tool is self-contained — the repo-root `sql/` files below are the human-readable source they're generated from.

## Applying by hand (alternative)

### Fresh install

```bash
sqlcmd -S <server> -d <db> -i sql/latest.sql
```

`sql/latest.sql` creates all tables and the reporting views for the **standard ASP.NET Identity** backend.

> Using the alternative **Auth0** backend instead? It's mutually exclusive with standard Identity (it recreates `AspNetUsers`). Apply `sql/7.0/contexts/Auth0.sql` + `sql/7.0/views/HoodAuth0UserProfiles.sql` instead of the Identity parts.

## Upgrading an existing database

Prefer the runner above — it does fresh + upgrade in one step. To upgrade by hand, apply the structural deltas for each tier you're crossing, then the views. **From 6.0.x** start with the `6.1` tier; **from 6.1.x** skip it:

```bash
# 6.0.x only — bring the database to the 6.1 baseline first:
sqlcmd -S <server> -d <db> -i sql/6.1/update.sql
# 6.0.x and 6.1.x — the 7.0 delta + views:
sqlcmd -S <server> -d <db> -i sql/7.0/update.sql
sqlcmd -S <server> -d <db> -i sql/7.0/views/HoodContentViews.sql
sqlcmd -S <server> -d <db> -i sql/7.0/views/HoodUserProfiles.sql
sqlcmd -S <server> -d <db> -i sql/7.0/views/HoodPropertyViews.sql
```

### What `sql/6.1/update.sql` does (6.0.x → 6.1)

Idempotent + forward-only structural cleanup that 6.1 applied:

- Drops the `AspNetUsers` foreign keys on content/property/logs (`AuthorId` / `AgentId` / `UserId` became plain columns).
- Drops the removed `HoodAddresses` table. **Destructive** — v7 has no such table; a 6.0 site with address data should migrate it out first (it's empty in the stock install).
- Drops columns removed in 6.1: `AspNetUsers.Latitude` / `.Longitude` and `HoodContent.Notes` / `.SystemNotes` / `.UserVars` (nothing in v7 reads them).

### What `sql/7.0/update.sql` does (6.1.x → 7.0)

The v7 delta is small and **drops no data-bearing base-table columns**:

- Removes the legacy duplicate user tables `ApplicationUser` and `UserProfiles` — `AspNetUsers` is now the single authoritative user store (nothing read or wrote the duplicates in 6.x).
- Drops `AspNetRoles.RemoteId` (removed from the v7 role model).
- Drops the unused `__HoodMigrationHistory` table (version is tracked in `HoodOptions`).
- Stamps `HoodOptions['Hood.Version'] = '7.0.0'`.

(The reporting views are applied separately — the `sql/7.0/views/*` scripts, idempotent DROP/CREATE — so they're shared by fresh installs, upgrades and the runner.)

Consumers on the 6.1.x golden-DB baseline (e.g. bma-live) take a **clean, zero-data-loss** upgrade — verified that nothing reads any removed field.

## Regenerating the SQL after a model change

1. Change the C# models in `projects/Hood.Core`.
2. Add a migration (authoring tool only):
   ```bash
   cd projects/Hood.Core
   dotnet ef migrations add <name> --context <Context> --output-dir Migrations/<Folder>
   ```
3. Regenerate the idempotent per-context DDL and reassemble `latest.sql`:
   ```bash
   dotnet ef migrations script --idempotent --context <Context> -o ../../sql/7.0/contexts/<Folder>.sql
   ```
   (Strip the UTF-8 BOM EF writes, and keep the `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` preamble at the top of `latest.sql` — the Identity filtered indexes and the views require it.)
4. Hand-write the matching `update.sql` delta for the new tier and document it in the table above.

## Layout

```
sql/
  latest.sql            # full fresh install (standard Identity) — what new databases run
  7.0/
    update.sql          # 6.1.x -> 7.0 upgrade delta
    contexts/*.sql      # per-context table DDL (generated, idempotent)
    views/*.sql         # the four reporting views (idempotent DROP/CREATE)
  6.1/
    update.sql          # 6.0.x -> 6.1 upgrade delta (drop legacy FKs/columns/HoodAddresses)
  6.0/                  # 6.0 baseline install (reference; the runner upgrades *from* it)
```

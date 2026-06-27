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

> Using the alternative **Auth0** backend instead? It's mutually exclusive with standard Identity (it recreates `AspNetUsers`). Apply `sql/07.00.00/03-context-auth0.sql` + `sql/07.00.00/93-view-auth0userprofiles.sql` instead of the Identity parts.

## Upgrading an existing database

Prefer the runner above — it does fresh + upgrade in one step. To upgrade by hand, apply the structural deltas for each tier you're crossing, then the views. **From 6.0.x** start with the `6.1` tier; **from 6.1.x** skip it:

```bash
# 6.0.x only — bring the database to the 6.1 baseline first:
sqlcmd -S <server> -d <db> -i sql/06.01.00/10-update-from-6.0.sql
# 6.0.x and 6.1.x — the 7.0 delta, data migrations + views (in filename order):
sqlcmd -S <server> -d <db> -i sql/07.00.00/202606021946-update-from-6.1.sql
sqlcmd -S <server> -d <db> -i sql/07.00.00/202606131600-unify-google-apikey.sql
sqlcmd -S <server> -d <db> -i sql/07.00.00/90-view-contentviews.sql
sqlcmd -S <server> -d <db> -i sql/07.00.00/91-view-propertyviews.sql
sqlcmd -S <server> -d <db> -i sql/07.00.00/92-view-userprofiles.sql
```

(Apply scripts in folder-then-filename order — the `MM.mm.pp/SS` key **is** the apply order.)

### What `06.01.00/10-update-from-6.0.sql` does (6.0.x → 6.1)

Idempotent + forward-only structural cleanup that 6.1 applied:

- Drops the `AspNetUsers` foreign keys on content/property/logs (`AuthorId` / `AgentId` / `UserId` became plain columns).
- Drops the removed `HoodAddresses` table. **Destructive** — v7 has no such table; a 6.0 site with address data should migrate it out first (it's empty in the stock install).
- Drops columns removed in 6.1: `AspNetUsers.Latitude` / `.Longitude` and `HoodContent.Notes` / `.SystemNotes` / `.UserVars` (nothing in v7 reads them).

### What `07.00.00/202606021946-update-from-6.1.sql` does (6.1.x → 7.0)

The v7 delta is small and **drops no data-bearing base-table columns**:

- Removes the legacy duplicate user tables `ApplicationUser` and `UserProfiles` — `AspNetUsers` is now the single authoritative user store (nothing read or wrote the duplicates in 6.x).
- Drops the unused `__HoodMigrationHistory` and EF `__EFMigrationsHistory` tables (version is tracked in `HoodOptions`; the runner journals in `dbo.SchemaVersions`).
- Stamps `HoodOptions['Hood.Version'] = '7.0.0'`.
- `AspNetRoles.RemoteId` is **kept** — it's nullable on both auth backends so the schema is one shape (the Auth0 backend maps local roles to Auth0 platform roles via it; standard Identity leaves it null).

(The reporting views are applied separately — the `*-view-*.sql` scripts, idempotent DROP/CREATE — so they're shared by fresh installs, upgrades and the runner.)

Consumers on the 6.1.x baseline take a **clean, zero-data-loss** upgrade — verified that nothing reads any removed field.

### What `07.00.00/202606131526-converge.sql` does (upgrade → fresh parity)

Idempotent convergence delta that runs after the update tier and before the views, so an **upgraded** 6.x database lands on the *same* schema as a **fresh** install. On a fresh install it's a no-op.

- Relaxes `AspNetUsers.Anonymous` to nullable (6.x carried it `NOT NULL`; a fresh v7 install models it nullable).

### What `07.00.00/202606131600-unify-google-apikey.sql` does (data migration)

Idempotent data migration for the stored `IntegrationSettings` JSON. The reCAPTCHA Enterprise work added a second Google Cloud API key (`GoogleRecaptchaApiKey`) alongside the existing Maps/Geocoding key (`GoogleMapsApiKey`); v7 collapses both into one `GoogleCloudApiKey`. The script seeds the unified key from the first non-empty legacy value and strips the old keys. No-op on a fresh install (the settings row is created lazily on first save) and on re-run (only seeds when the unified key is unset).

(The other historical drift converges via the fresh DDL itself — fresh installs now create the `AuthorId` / `UserId` / `AgentId` columns as `nvarchar(450)` **+ indexed**, the shape upgraded DBs already carry, and keep `AspNetRoles.RemoteId`. So there's nothing for the converge delta to alter for those.)

## Convergence invariant & the parity guard

Hood commits to **`fresh == upgrade`** — a database upgraded through the script chain is byte-for-byte identical to a fresh install, **within each auth backend**. Standard Identity and the Auth0 backend are *intentionally* different shapes (Auth0 omits the local-credential surface — password/security columns, the claims/logins/tokens tables — and adds `AspNetAuth0Identities`); that divergence is by design, not drift.

`projects/Hood.Tests/SchemaParityTests.cs` enforces it: it provisions a fresh database via the runner and asserts the converged shape (bounded + indexed user-reference columns, a nullable `AspNetRoles.RemoteId`, a nullable `AspNetUsers.Anonymous`), plus that the converge delta relaxes `Anonymous` and is idempotent. It's `SkippableFact`-gated — runs in CI (where a SQL Server is provisioned), skips locally without one.

## Regenerating the SQL after a model change

1. Change the C# models in `projects/Hood.Core`.
2. Add a migration (authoring tool only):
   ```bash
   cd projects/Hood.Core
   dotnet ef migrations add <name> --context <Context> --output-dir Migrations/<Folder>
   ```
3. Regenerate the idempotent per-context DDL (overwrite the context's existing numbered script) and reassemble `latest.sql`:
   ```bash
   dotnet ef migrations script --idempotent --context <Context> -o ../../sql/07.00.00/NN-context-<name>.sql
   ```
   (Strip the UTF-8 BOM EF writes, re-add the `-- Apply key …` banner line, and keep the `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` preamble at the top of `latest.sql` — the Identity filtered indexes and the views require it.)
4. Hand-write the matching upgrade delta as a **new** `MM.mm.pp/<yyyyMMddHHmm>-<name>.sql` script (timestamp-prefixed — typically mirroring the EF migration that authored it) and document it above. **While a version is in RC** the structural-snapshot scripts can be regenerated and the tier reorganised freely (nothing has run in production); **once it ships GA** the migrations are locked — only ever append a new timestamped script.

## Script ordering & layout

Scripts live in **zero-padded version folders** (`MM.mm.pp`, padded so `07.00.00` sorts before a future `10.00.00`), and the folder-then-filename key **is** the apply order — DbUp applies embedded scripts in ordinal `LogicalName` order. **Within** a folder the filename prefix encodes a *phase*:

- **Structural snapshots** — the per-context table DDL — use a low fixed band (`00`–`0x`). These are **regenerated** from the EF model, not point-in-time history, so they carry a stable ordinal, not a date.
- **Forward deltas & data-migrations** — the true append-only migrations — use a **`yyyyMMddHHmm` timestamp** prefix, typically the timestamp of the EF migration that authored them. Timestamps never collide across parallel branches and never need renumbering.
- **Reporting views** — also regenerated (idempotent DROP/CREATE) — use a high fixed band (`9x`) so they always apply last.

Ordinal sort gives `0x` < `2026…` < `9x`, so the two fixed bands bookend the timestamped middle. Ordering never relies on alphabetical accidents; the version is carried by the folder + the `-- Apply key …` header banner + the `HoodOptions['Hood.Version']` stamp. While a tier is in RC the prefixes are renumbered to stay sequential; after GA they're append-only.

```
sql/
  latest.sql                            # full fresh install (standard Identity) — what new databases run (NOT embedded in the runner)
  06.01.00/
    10-update-from-6.0.sql              # 6.0.x -> 6.1 upgrade delta (drop legacy FKs/columns/HoodAddresses)
  07.00.00/
    # structural snapshots — low band (00-0x), regenerated from the EF model
    00-context-content.sql             # per-context table DDL (generated, idempotent)
    01-context-hooddb.sql
    02-context-identity.sql            # standard ASP.NET Identity backend
    03-context-auth0.sql               # ALTERNATIVE Auth0 backend — NOT embedded; consumer applies instead of Identity
    04-context-property.sql
    # forward deltas & data-migrations — yyyyMMddHHmm timestamp, append-only
    202606021946-update-from-6.1.sql   # 6.1.x -> 7.0 upgrade delta
    202606131526-converge.sql          # convergence delta (upgraded 6.x -> fresh parity)
    202606131600-unify-google-apikey.sql  # data migration — collapse the two Google keys into GoogleCloudApiKey
    # reporting views — high band (9x), regenerated (idempotent DROP/CREATE)
    90-view-contentviews.sql
    91-view-propertyviews.sql
    92-view-userprofiles.sql
    93-view-auth0userprofiles.sql      # ALTERNATIVE Auth0 backend — NOT embedded; consumer applies instead of userprofiles
```

The runner embeds these from `Hood.Core` (`Hood.Core.csproj`) and applies them by ordinal `LogicalName` order **excluding** `latest.sql` and the two Auth0-backend scripts. Apply order: context snapshots → timestamped deltas (update → converge → data migrations) → views.

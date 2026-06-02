# Hood CMS — database schema & upgrades

Hood's schema is shipped as **plain, idempotent SQL scripts** that you run by hand (or via a runner). EF Core migrations live in `projects/Hood.Core/Migrations` and are used **only to author/regenerate** the SQL — nothing applies migrations at runtime.

Every script is safe to re-run.

## Fresh install

Run the full schema against an empty database:

```bash
sqlcmd -S <server> -d <db> -i sql/latest.sql
```

`sql/latest.sql` creates all tables and the reporting views for the **standard ASP.NET Identity** backend. That's it — the app boots against it.

> Using the alternative **Auth0** backend instead? It's mutually exclusive with standard Identity (it recreates `AspNetUsers`). Apply `sql/7.0/contexts/Auth0.sql` + `sql/7.0/views/HoodAuth0UserProfiles.sql` instead of the Identity parts.

## Upgrading an existing database

Schema is versioned in tiers. Run the `update.sql` for each tier **above** your current version, in order:

| You're on | Run, in order |
|---|---|
| 6.0.x | `sql/6.1/update.sql` → `sql/7.0/update.sql` |
| 6.1.x | `sql/7.0/update.sql` |

```bash
sqlcmd -S <server> -d <db> -i sql/7.0/update.sql
```

### What `sql/7.0/update.sql` does (6.1.x → 7.0)

The v7 delta is small and **drops no data-bearing base-table columns**:

- Removes the legacy duplicate user tables `ApplicationUser` and `UserProfiles` — `AspNetUsers` is now the single authoritative user store (nothing read or wrote the duplicates in 6.x).
- Drops `AspNetRoles.RemoteId` (removed from the v7 role model).
- Drops the unused `__HoodMigrationHistory` table (version is tracked in `HoodOptions`).
- Rebuilds the four reporting views — the 6.1 definitions referenced columns that never existed on the base tables and were silently broken; v7 fixes them.
- Stamps `HoodOptions['Hood.Version'] = '7.0.0'`.

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
  6.1/  6.0/            # prior tiers (unchanged)
```

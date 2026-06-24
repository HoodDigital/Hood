# Hood CMS
[![GitHub release (latest by date incl. pre-releases)](https://img.shields.io/github/v/release/HoodDigital/Hood?include_prereleases&label=Latest%20Release)](https://github.com/HoodDigital/Hood/releases)
[![CI](https://img.shields.io/github/actions/workflow/status/HoodDigital/Hood/backend.yml?branch=master&label=CI)](https://github.com/HoodDigital/Hood/actions/workflows/backend.yml)

A fully customisable content management system for ASP.NET Core, built on **.NET 10** and **EF Core 10**.

## Clone the demo project

Clone the demo Hood web project from [Hood.Demo](https://github.com/HoodDigital/Hood.Demo). Or enter the following command in Git Bash or your command prompt.
```
$ git clone https://github.com/HoodDigital/Hood.Demo
```

## Create a new project via the dotnet CLI

Coming soon.

## NuGet installation
[![NuGet stable](https://img.shields.io/nuget/v/Hood?label=NuGet%20Stable)](https://www.nuget.org/packages/Hood/)
[![NuGet prerelease](https://img.shields.io/nuget/vpre/Hood?label=NuGet%20Prerelease)](https://www.nuget.org/packages/Hood/)

Install Hood CMS via Package Manager.
```
> Install-Package Hood
```
or via .NET CLI
```
> dotnet add package Hood
```

## Client Side Code
[![npm stable](https://img.shields.io/npm/v/hoodcms?label=npm%20Stable)](https://www.npmjs.com/package/hoodcms)
[![npm prerelease](https://img.shields.io/npm/v/hoodcms/next?label=npm%20Prerelease)](https://www.npmjs.com/package/hoodcms?activeTab=versions)

The client side code is not required to run Hood CMS as all required JS/CSS are served via jsdelivr. However, if you want to extend or modify the client side code, you can download this npm package, which contains the required distribution CSS and JavaScript, as well as source SCSS and TypeScript files. 

https://www.npmjs.com/package/hoodcms

To install Hood CMS client side code via npm.
```
> npm install hoodcms
```
or
```
> pnpm add hoodcms
```

> To use your own client side code, you will also need to update script/link references in your theme's HTML or Razor C# files to use your own version of the code, rather than the CDN.

### Build preset

Building your own frontend on Hood's toolchain? Don't clone the configs — extend them. The `hoodcms` package exports Hood's audited build process:

```js
// rollup.config.mjs
import { hoodRollup } from 'hoodcms/build';
export default hoodRollup({ entries: { site: 'src/ts/site.ts' }, externals: ['owl.carousel'] });

// gulpfile.js
const { registerHoodTasks } = require('hoodcms/build/gulp');
registerHoodTasks(require('gulp'), { less: true });
```

Your tsconfigs `"extends": "hoodcms/tsconfig.base.json"` (declare your own `rootDir`/`outDir` — path options don't inherit across packages). The toolchain versions are published as **optional peer dependencies** — install the ones you use at Hood's audited versions.

## Database Installation/Update

Hood's schema ships as plain, idempotent, forward-only SQL scripts. EF Core migrations are only
an authoring tool; nothing applies them at runtime. Full detail and the regeneration workflow are
in [`sql/README.md`](sql/README.md).

### `hood-schema` runner (recommended)

`hood-schema` is a .NET CLI tool that applies the schema with one command — **fresh installs and
v6→v7 upgrades alike**. It journals applied scripts in its own `dbo.SchemaVersions` table, so it's
idempotent and figures out what's needed for you (no per-version chain to run by hand).

```bash
dotnet tool install --global Hood.SchemaTool
hood-schema upgrade --connection "Server=…;Database=…;User Id=…;Password=…;TrustServerCertificate=True"
```

Your own project SQL rides the same path via `--scripts <folder>` (applied after Hood's core). See
[`sql/README.md`](sql/README.md) for the full runner reference.

### By hand (alternative)

**Fresh install** — create the database, then execute `/sql/latest.sql` (standard ASP.NET Identity backend).

**Upgrade** — run the upgrade-delta scripts for each tier above your current version, in folder-then-filename order (the `MM.mm.pp/SS` key is the apply order):

| You're on | Run, in order |
|---|---|
| `6.0.x` | `/sql/06.01.00/10-update-from-6.0.sql` → `/sql/07.00.00/202606021946-update-from-6.1.sql` → `/sql/07.00.00/202606131600-unify-google-apikey.sql` → the `/sql/07.00.00/9*-view-*.sql` scripts |
| `6.1.x` | `/sql/07.00.00/202606021946-update-from-6.1.sql` → `/sql/07.00.00/202606131600-unify-google-apikey.sql` → the `/sql/07.00.00/9*-view-*.sql` scripts |

The deltas are small and drop no data-bearing columns — see [`sql/README.md`](sql/README.md) for
exactly what each tier changes.


## Project layout

Hood is split into focused packages under `projects/`, each published as its own NuGet package (except the test project):

| Package | Purpose |
|---|---|
| `Hood` | Complete Hood CMS package — all default controllers, packaged with the Bootstrap 4 default theme. |
| `Hood.Core` | DI / runtime engine — base controllers, attributes, contexts, extensions, filters. |
| `Hood.Admin` | Admin-area controllers. |
| `Hood.Development` | Dev-time helpers. |
| `Hood.SchemaTool` | CLI to apply the database schema (idempotent, forward-only) in a deploy pipeline. |
| `Hood.UI.Core` | Base UI scaffolding and shared view components. |
| `Hood.UI.Admin` | Admin UI — Areas and views. |
| `Hood.UI.Bootstrap3` / `Hood.UI.Bootstrap4` | Bootstrap 3 / 4 themed UI variants. |
| `Hood.Tests` | Test suite (not published). |


## Full documentation

Documentation is a work in progress. The most useful references today:

- [`sql/README.md`](sql/README.md) — database schema, the `hood-schema` runner, upgrade tiers, and how to regenerate the SQL.
- [`DOCKER.md`](DOCKER.md) — the containerised local dev rig (`docker compose up`).

Also, feel free to add your issues or pull requests to our GitHub — we always welcome contributions!

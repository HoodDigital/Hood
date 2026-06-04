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

To install Hood CMS client side code via NPM.
```
> npm install hoodcms
```
or
```
> yarn add hoodcms
```

> To use your own client side code, you will also need to update script/link references in your theme's HTML or Razor C# files to use your own version of the code, rather than the CDN.

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

**Upgrade** — run the `update.sql` for each tier above your current version, in order:

| You're on | Run, in order |
|---|---|
| `6.0.x` | `/sql/6.1/update.sql` → `/sql/7.0/update.sql` → the `/sql/7.0/views/*` scripts |
| `6.1.x` | `/sql/7.0/update.sql` → the `/sql/7.0/views/*` scripts |

The deltas are small and drop no data-bearing columns — see [`sql/README.md`](sql/README.md) for
exactly what each tier changes.


## Releases & branching

Hood uses a trunk-based flow with automatic, git-derived versioning (MinVer) and GitHub Actions.

**Branching.** Work happens on feature branches → PR → squash-merge to `master`. PRs run build + `dotnet test` and `npm run package` as gates; nothing merges red.

**Versioning is automatic — never set a version by hand.** [MinVer](https://github.com/adamralph/minver) derives it from git:

| Git state | Version | Published |
|---|---|---|
| A merge to `master` | `7.0.0-rc.N` (`N` = merges since the last tag) | NuGet.org **prerelease** + npm dist-tag **`next`** |
| A GitHub Release tag `v7.0.0` | `7.0.0` (clean) | NuGet.org **stable** + npm dist-tag **`latest`** |

So **every merge ships a prerelease automatically**, and **cutting a `v7.0.0` GitHub Release ships the stable**. To release a stable, draft a GitHub Release with the tag `v7.0.0` (matching the `MinVerMinimumMajorMinor` in [`Directory.Build.props`](Directory.Build.props)).

**Pipelines** ([`.github/workflows`](.github/workflows)): `backend.yml` packs the 8 NuGet packages (the seven libraries plus the `hood-schema` CLI tool); `frontend.yml` builds + publishes the `hoodcms` npm package (its version is derived from the same MinVer value, so npm and NuGet stay in lockstep). They're path-filtered, so a backend-only change doesn't rebuild the frontend and vice versa.

**Publishing — no tokens.** Both registries use OIDC **trusted publishing**: GitHub mints a short-lived identity token (`id-token: write`) that the registry exchanges for a temporary credential at publish time. Nothing long-lived is stored in GitHub. Both publish jobs run in the **`release`** GitHub environment (the trusted-publisher configs require it), and provenance is attached automatically.

- **npm** — [trusted publisher](https://docs.npmjs.com/trusted-publishers) on the `hoodcms` package pointing at `frontend.yml` + environment `release`. `npm publish` (npm ≥ 11.5.1) does the exchange.
- **NuGet** — [trusted publisher](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) on the packages pointing at `backend.yml` + environment `release`. The `NuGet/login` action exchanges the OIDC token for a temporary key; it reads your nuget.org username from the `NUGET_USER` repo variable. (`NuGet/login` isn't a GitHub-authored action, so it must be added to the repo's allowed-actions list.)


## Full documentation

Documentation is a work in progress. The most useful references today:

- [`sql/README.md`](sql/README.md) — database schema, the `hood-schema` runner, upgrade tiers, and how to regenerate the SQL.
- [`DOCKER.md`](DOCKER.md) — the containerised local dev rig (`docker compose up`).

Also, feel free to add your issues or pull requests to our GitHub — we always welcome contributions!

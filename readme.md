# Hood CMS
[![GitHub release (Latest by date including pre-releases)](https://img.shields.io/github/v/release/HoodDigital/Hood?include_prereleases&label=Latest%20Release)](https://github.com/HoodDigital/Hood/releases)

A fully customisable content management system built in ASP.NET Core 5 & Bootstrap 5.

## Clone demo project

Clone the demo ASP.NET Core 6 Web Project from our repository [here](https://github.com/HoodDigital/Hood.Demo). Or enter the following command in Git Bash or your command prompt.
```
$ git clone https://github.com/HoodDigital/Hood.Demo
```

## Create new ASP.NET Core 6 Web Project via dotnet CLI

Coming soon.

## Nuget Installation 
[![NuGet](https://img.shields.io/nuget/v/hood?label=NuGet%20Stable)](https://www.nuget.org/packages/Hood/)
[![MyGet Latest](https://img.shields.io/myget/hood/vpre/hood?label=MyGet)](https://www.myget.org/feed/hood/package/nuget/Hood)

Install Hood CMS via Package Manager.
```
> Install-Package Hood
```
or via .NET CLI
```
> dotnet add package Hood
```

## Client Side Code
[![npm Package](https://img.shields.io/npm/v/hoodcms)](https://www.npmjs.com/package/hoodcms)

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

Hood's schema ships as plain, idempotent SQL scripts — run by hand or via a runner. EF Core
migrations are only an authoring tool; nothing applies them at runtime. Full detail and the
regeneration workflow are in [`sql/README.md`](sql/README.md).

### Fresh installation

1. Create your database.
2. Execute file `/sql/latest.sql` (standard ASP.NET Identity backend).

### Upgrading an existing database

Run the `update.sql` for each tier above your current version, in order:

| You're on | Run, in order |
|---|---|
| `< 6.0.x` | migrate to `6.0.x` first, then `/sql/6.0/migrate.sql`, then follow the row below |
| `6.0.x` | `/sql/6.1/update.sql` → `/sql/7.0/update.sql` |
| `6.1.x` | `/sql/7.0/update.sql` |

The `6.1.x → 7.0` delta is small and drops no data-bearing columns — see
[`sql/README.md`](sql/README.md) for exactly what it changes.


## Releases & branching

Hood uses a trunk-based flow with automatic, git-derived versioning (MinVer) and GitHub Actions.

**Branching.** Work happens on feature branches → PR → squash-merge to `master`. PRs run build + `dotnet test` and `npm run package` as gates; nothing merges red.

**Versioning is automatic — never set a version by hand.** [MinVer](https://github.com/adamralph/minver) derives it from git:

| Git state | Version | Published |
|---|---|---|
| A merge to `master` | `7.0.0-rc.N` (`N` = merges since the last tag) | NuGet.org **prerelease** + npm dist-tag **`next`** |
| A GitHub Release tag `v7.0.0` | `7.0.0` (clean) | NuGet.org **stable** + npm dist-tag **`latest`** |

So **every merge ships a prerelease automatically**, and **cutting a `v7.0.0` GitHub Release ships the stable**. To release a stable, draft a GitHub Release with the tag `v7.0.0` (matching the `MinVerMinimumMajorMinor` in [`Directory.Build.props`](Directory.Build.props)).

**Pipelines** ([`.github/workflows`](.github/workflows)): `backend.yml` packs the 7 NuGet packages; `frontend.yml` builds + publishes the `hoodcms` npm package (its version is derived from the same MinVer value, so npm and NuGet stay in lockstep). They're path-filtered, so a backend-only change doesn't rebuild the frontend and vice versa.

**Publishing — no tokens.** Both registries use OIDC **trusted publishing**: GitHub mints a short-lived identity token (`id-token: write`) that the registry exchanges for a temporary credential at publish time. Nothing long-lived is stored in GitHub. Both publish jobs run in the **`release`** GitHub environment (the trusted-publisher configs require it), and provenance is attached automatically.

- **npm** — [trusted publisher](https://docs.npmjs.com/trusted-publishers) on the `hoodcms` package pointing at `frontend.yml` + environment `release`. `npm publish` (npm ≥ 11.5.1) does the exchange.
- **NuGet** — [trusted publisher](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) on the packages pointing at `backend.yml` + environment `release`. The `NuGet/login` action exchanges the OIDC token for a temporary key; it reads your nuget.org username from the `NUGET_USER` repo variable. (`NuGet/login` isn't a GitHub-authored action, so it must be added to the repo's allowed-actions list.)


## Full documentation
Documentation is a work in progress!

Also, feel free to add your issues or pull requests to our GitHub, we always welcome contributions!

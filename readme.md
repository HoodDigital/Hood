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

**Publishing credentials.**

- **npm** uses [trusted publishing](https://docs.npmjs.com/trusted-publishers) (OIDC) — no token. The `hoodcms` package on npmjs is configured with a trusted publisher pointing at this repo's `frontend.yml` workflow; the workflow mints a short-lived OIDC token (`id-token: write`) that npm exchanges at publish time, and provenance is attached automatically. Nothing to store in GitHub.
- **NuGet** uses a `NUGET_API_KEY` repo secret (Settings → Secrets and variables → Actions). Until it's set the backend workflow builds, tests and packs but skips the push.


## Full documentation
Documentation is a work in progress!

Also, feel free to add your issues or pull requests to our GitHub, we always welcome contributions!

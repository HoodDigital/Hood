# Hood.Development

The runnable Hood CMS web host — an ASP.NET Core app on **.NET 10** that dogfoods the
framework for local development. It's also the home of the **`hoodcms`** npm package
(Hood's client-side SCSS/TypeScript, plus the published `build` and `dev` tooling).

## Local development

From this directory:

```bash
pnpm install          # restore the JS tooling
pnpm hoodcms setup    # bootstrap: .env.local, JS deps, dotnet restore
pnpm hoodcms up       # create + upgrade the DB, then build + start the full stack
```

`hoodcms` is the cross-platform local-dev CLI (the `./dev` subpath of the `hoodcms`
package, run via [tsx](https://tsx.is)). Run `pnpm hoodcms` with no args for the full
command list. The complete walkthrough — containers, ports, the connection-resolution
chain, and every command — is in [`DOCKER.md`](../../DOCKER.md). Debugging from VS Code
is wired up in [`.vscode/launch.json`](../../.vscode/launch.json).

## The `hoodcms` client package

[![npm stable](https://img.shields.io/npm/v/hoodcms?label=npm%20Stable)](https://www.npmjs.com/package/hoodcms)

Hood's client-side code (distribution CSS/JS + source SCSS/TypeScript) is published to npm
as [`hoodcms`](https://www.npmjs.com/package/hoodcms). It isn't required to run Hood — the
assets are served from jsDelivr by default — but you can install it to extend or rebuild
the frontend:

```bash
> pnpm add hoodcms   # or: npm install hoodcms
```

See the [root README](../../readme.md) for the build-preset extension points
(`hoodcms/build`, `hoodcms/build/gulp`, and the shared tsconfig).

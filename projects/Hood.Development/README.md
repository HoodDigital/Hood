# hoodcms

[![npm stable](https://img.shields.io/npm/v/hoodcms?label=npm%20Stable)](https://www.npmjs.com/package/hoodcms)
[![npm prerelease](https://img.shields.io/npm/v/hoodcms/next?label=npm%20Prerelease)](https://www.npmjs.com/package/hoodcms?activeTab=versions)

The client-side toolkit for [**Hood CMS**](https://github.com/HoodDigital/Hood) — the
distribution CSS/JS that Hood's UI runs on, the SCSS/TypeScript sources behind it, Hood's
shared frontend build presets, and a cross-platform local-dev CLI.

## Installation

```bash
npm install hoodcms
# or
pnpm add hoodcms
```

## Using it with Hood CMS

You don't need this package just to **run** Hood — the required CSS/JS are served from
jsDelivr by default. Install `hoodcms` when you want to **extend or replace** the frontend.

### Use your own assets

Build your own CSS/JS from Hood's SCSS/TypeScript sources (shipped under the package's
`src/`), then point your theme's `<script>` / `<link>` references at your own build instead
of the CDN.

### Extend Hood's build toolchain

Don't fork Hood's build config — extend the published presets:

```js
// rollup.config.mjs
import { hoodRollup } from 'hoodcms/build';
export default hoodRollup({ entries: { site: 'src/ts/site.ts' }, externals: ['owl.carousel'] });
```

```js
// gulpfile.cjs
const { registerHoodTasks } = require('hoodcms/build/gulp');
registerHoodTasks(require('gulp'), { less: true });
```

```jsonc
// tsconfig.json — declare your own rootDir/outDir (path options don't inherit across packages)
{ "extends": "hoodcms/tsconfig.base.json" }
```

The toolchain packages (rollup, gulp, sass, …) are **optional peer dependencies** — install
the ones you use, at Hood's audited versions.

## Local dev orchestration (`hoodcms`)

The package also exposes a cross-platform local-dev CLI (the `hoodcms` bin) that runs a Hood
project's full stack — Docker SQL Server, schema upgrade, the app, and frontend + backend
watchers — with one command set on Windows, macOS and Linux:

```bash
npx hoodcms            # list all commands
npx hoodcms up         # start the stack: DB created + upgraded, then the app
npx hoodcms watch      # frontend + backend hot-reload together (one Ctrl+C stops both)
npx hoodcms down
```

It's zero-config by default; drop in an optional, fully-typed `hood.dev.ts` to override
settings or register your own targets:

```ts
import { defineTasks } from 'hoodcms/dev';

export default defineTasks({
  // override config and/or add custom targets here
});
```

## Links

- [Hood CMS on GitHub](https://github.com/HoodDigital/Hood)
- [Hood CMS server package on NuGet](https://www.nuget.org/packages/Hood/)

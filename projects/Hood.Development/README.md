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

Database commands share a `db <sub>` namespace (the hyphenated forms still work too):

```bash
npx hoodcms db up                       # start SQL Server only, wait until healthy
npx hoodcms db upgrade                  # create-if-absent + apply the Hood schema (idempotent)
npx hoodcms db restore dump.bacpac      # restore from a file, then run db upgrade to reconcile
npx hoodcms db reset                    # nuclear option: drop the DB volume and recreate the stack
```

#### Restoring a database (`db restore`)

`db restore <file>` is a **generic, file-based** restore: it dispatches by file extension to a
`RestoreProvider`. The **file path is the entire contract** — where the file came from (a
`hood cli az pull`, an S3 export, a teammate's dump) never enters this layer, and `hood-schema`
stays a pure schema-migration tool. Hood ships a built-in `.bacpac` provider via the
cross-platform `sqlpackage` .NET tool (`dotnet tool install --global microsoft.sqlpackage`),
connecting over the host TCP port — no Docker coupling.

Restore is **destructive** — it overwrites the target database — so it requires an interactive
`yes`, or a `--force` flag in non-interactive shells. A restored database may be behind the
current schema, so run `db upgrade` afterwards (DbUp's journal reconciles idempotently). If a
restore leaves the database wedged, `db reset` is the nuclear recovery path.

Register your own provider (e.g. `.bak`, `.sql`) without forking, in `hood.dev.ts`:

```ts
import { defineTasks } from 'hoodcms/dev';

export default defineTasks({
  restoreProviders: [
    {
      extensions: ['.sql'],
      describe: 'Plain T-SQL script',
      restore: ({ file, connection, task }) => task.run('sqlcmd', ['-S', '127.0.0.1,14331', '-i', file]),
    },
  ],
});
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

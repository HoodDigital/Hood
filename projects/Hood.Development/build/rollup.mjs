/**
 * hoodcms/build — Hood's rollup config factory (HOOD-83).
 *
 * Consumers extend Hood's build instead of cloning it:
 *
 *   import { hoodRollup } from 'hoodcms/build';
 *   export default hoodRollup({
 *     entries: {
 *       site:  'src/ts/site.ts',                                       // string = shared externals
 *       admin: { input: 'src/ts/admin.ts', externals: ['chart.js'] },  // object = per-entry extras
 *     },
 *     externals: ['owl.carousel'],   // merged onto Hood's base externals for every entry
 *   });
 *
 * Built in: Hood's base externals and UMD globals, banner/licence generated from the
 * consumer's own package.json (VERSION env wins), terser + typescript + node-resolve +
 * commonjs wiring, and the debug/production split (`rollup --config --debug` emits
 * readable bundles with sourcemaps to srcDir; production emits minified to distDir).
 */
import typescript from '@rollup/plugin-typescript';
import resolve from '@rollup/plugin-node-resolve';
import terser from '@rollup/plugin-terser';
import commonjs from '@rollup/plugin-commonjs';
import { createRequire } from 'node:module';
import path from 'node:path';

/** Hood's base externals — scripts consumers load from the page/CDN, never bundled. */
export const HOOD_EXTERNALS = [
    'jQuery',
    'bootstrap',
    'sweetalert2',
    'dropzone',
    '@simonwep/pickr',
    'hugerte/hugerte',
    'chart.js'
];

/** UMD global names for the base externals. */
export const HOOD_GLOBALS = {
    jQuery: '$',
    bootstrap: 'bootstrap',
    sweetalert2: 'Swal',
    dropzone: 'Dropzone',
    '@simonwep/pickr': 'Pickr',
    'hugerte/hugerte': 'hugerte',
    'chart.js': 'Chart'
};

/** The hood UMD alias footer — exposes the bundle under the historic global names. */
export const HOOD_FOOTER = `\
            if (typeof this !== 'undefined' && this.hood){\
              this.hoodCMS = this.Hood = this.hoodCMS = this.HoodCMS = this.hood\
            }`;

function buildBanner(packageJson) {
    const version = process.env.VERSION || packageJson.version;
    const year = new Date().getFullYear();
    let license = 'Proprietary and confidential. Unauthorized copying of this file, via any medium is strictly prohibited.';
    if (packageJson.license) {
        license = `Released under the ${packageJson.license} License.`;
    }
    let description = '';
    if (packageJson.description) {
        description = `\n* ${packageJson.description}`;
    }
    const author = packageJson.author || 'Hood Digital';
    return `/*!
* ${packageJson.name} v${version}${description}
* Written by ${author}, ${year}
* ${license}
*/`;
}

/**
 * Build a rollup config (the `commandLineArgs => configs` shape rollup expects) from the
 * consumer's entries. Options:
 *   entries    {name: string | {input, externals?, externalsOverride?, globals?, footer?,
 *              name?}} — required. `externals` merges onto the shared list;
 *              `externalsOverride` replaces it entirely.
 *   externals  string[] — merged onto HOOD_EXTERNALS for every entry
 *   globals    object   — merged onto HOOD_GLOBALS; globals are only emitted for entries
 *              that opt in (entry.globals or entry.useHoodGlobals: true) — unmapped entries
 *              keep rollup's guessed global names, matching Hood's historic output
 *   name       string   — UMD global name (default 'hood')
 *   footer     string   — default footer (default HOOD_FOOTER); per-entry override wins
 *   srcDir     string   — debug output root (default 'wwwroot/src/')
 *   distDir    string   — production output root (default 'wwwroot/dist/')
 *   tsconfig   {debug, production} — tsconfig per mode (defaults 'tsconfig.rollup.json' /
 *              'tsconfig.production.json')
 *   packageJson object  — consumer package.json for the banner (default: ./package.json)
 */
export function hoodRollup(options) {
    if (!options || !options.entries || Object.keys(options.entries).length === 0) {
        throw new Error('hoodRollup: options.entries is required — { name: input | {input, ...} }');
    }
    const require = createRequire(path.join(process.cwd(), 'package.json'));
    const packageJson = options.packageJson || require(path.join(process.cwd(), 'package.json'));
    const banner = buildBanner(packageJson);

    const sharedExternals = [...HOOD_EXTERNALS, ...(options.externals || [])];
    const sharedGlobals = { ...HOOD_GLOBALS, ...(options.globals || {}) };
    const umdName = options.name || 'hood';
    const defaultFooter = options.footer !== undefined ? options.footer : HOOD_FOOTER;
    const srcDir = options.srcDir || 'wwwroot/src/';
    const distDir = options.distDir || 'wwwroot/dist/';
    const tsconfig = {
        debug: (options.tsconfig && options.tsconfig.debug) || 'tsconfig.rollup.json',
        production: (options.tsconfig && options.tsconfig.production) || 'tsconfig.production.json'
    };

    return commandLineArgs => {
        const debug = commandLineArgs.debug === true;
        const destination = debug ? srcDir : distDir;

        const plugins = [
            resolve({ moduleDirectories: ['node_modules'] }),
            commonjs()
        ];
        if (!debug) {
            plugins.push(terser());
        }
        plugins.push(typescript({
            tsconfig: debug ? tsconfig.debug : tsconfig.production,
            outDir: destination + 'js',
            noEmit: false
        }));

        return Object.entries(options.entries).map(([entryName, entry]) => {
            const config = typeof entry === 'string' ? { input: entry } : entry;
            const output = {
                file: `${destination}js/${entryName}.js`,
                format: 'umd',
                name: config.name || umdName,
                banner: banner,
                footer: config.footer !== undefined ? config.footer : defaultFooter,
                sourcemap: debug,
                compact: !debug
            };
            if (config.globals || config.useHoodGlobals) {
                output.globals = { ...sharedGlobals, ...(config.globals || {}) };
            }
            return {
                input: config.input,
                output: output,
                onwarn(warning, rollupWarn) {
                    if (warning.code !== 'CIRCULAR_DEPENDENCY') {
                        rollupWarn(warning);
                    }
                },
                external: config.externalsOverride
                    ? config.externalsOverride
                    : config.externals
                        ? [...sharedExternals, ...config.externals]
                        : sharedExternals,
                plugins: plugins
            };
        });
    };
}

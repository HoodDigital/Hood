import typescript from '@rollup/plugin-typescript';
import resolve from '@rollup/plugin-node-resolve';
import uglify from "@lopatnov/rollup-plugin-uglify";

const packageJson = require('./package.json')
const version = process.env.VERSION || packageJson.version

const banner = `/*!
* ${packageJson.name} v${version}
* Released under the ${packageJson.license} License.
*/`;

const footer = `\
if (typeof this !== 'undefined' && this.hood){\
  this.hoodCMS = this.Hood = this.hoodCMS = this.HoodCMS = this.hood\
}`;


export default commandLineArgs => {

    let plugins = [
        resolve({
            moduleDirectories: ['node_modules']
        })
    ]

    let sourcemaps = true;
    let compact = false;
    let destination = 'wwwroot/src/';

    if (commandLineArgs.debug !== true) {

        plugins.push(uglify());
        plugins.push(typescript({
            tsconfig: "tsconfig.production.json"
        }));

        sourcemaps = false;
        compact = true;
        destination = 'wwwroot/dist/';

        console.log('---------------------------------------------------------------------------')
        console.log('-                       Building Hood CMS - APP                           -')
        console.log('-                              PRODUCTION                                 -')
        console.log('---------------------------------------------------------------------------')

    } else {

        plugins.push(typescript({
            tsconfig: "tsconfig.rollup.json"
        }));

        console.log('---------------------------------------------------------------------------')
        console.log('-                       Building Hood CMS - APP                           -')
        console.log('-                             DEVELOPMENT                                 -')
        console.log('---------------------------------------------------------------------------')

    }

    return [
        {
            input: 'src/ts/app.ts',
            output: {
                file: destination + 'js/app.js',
                format: 'umd',
                name: 'HoodCMS',
                banner: banner,
                footer: footer,
                globals: {
                    jQuery: '$',
                    bootstrap: 'bootstrap',
                    sweetalert2: 'Swal',
                    dropzone: 'Dropzone'
                },
                sourcemap: sourcemaps,
                compact: compact
            },
            // https://github.com/rollup/rollup/issues/2271
            onwarn(warning, rollupWarn) {
                if (warning.code !== 'CIRCULAR_DEPENDENCY') {
                    rollupWarn(warning)
                }
            },
            external: [
                'jQuery',
                'bootstrap',
                'sweetalert2',
                'dropzone'
            ],
            plugins: plugins
        },
        {
            input: 'src/ts/admin.ts',
            output: {
                file: destination + 'js/admin.js',
                format: 'umd',
                name: 'HoodCMS',
                banner: banner,
                footer: footer,
                globals: {
                    jQuery: '$',
                    bootstrap: 'bootstrap',
                    sweetalert2: 'Swal',
                    dropzone: 'Dropzone',
                    '@simonwep/pickr': 'Pickr',
                    'tinymce/tinymce': 'tinymce',
                    'chart.js': 'Chart'
                },
                sourcemap: sourcemaps,
                compact: compact
            },
            external: [
                'jQuery',
                'bootstrap',
                'sweetalert2',
                'dropzone',
                '@simonwep/pickr',
                'tinymce/tinymce',
                'chart.js'
            ],
            plugins: plugins
        },
        {
            input: 'src/ts/login.ts',
            output: {
                file: destination + 'js/login.js',
                format: 'umd',
                name: 'HoodCMS',
                banner: banner,
                footer: footer,
                globals: {
                    sweetalert2: 'Swal',
                    jQuery: '$',
                    bootstrap: 'bootstrap',
                    'google.maps': 'google.maps'
                },
                sourcemap: sourcemaps,
                compact: compact
            },
            external: [
                'jQuery',
                'bootstrap',
                'sweetalert2',
                'google.maps',
                'hood'
            ],
            plugins: plugins
        }
    ];
}
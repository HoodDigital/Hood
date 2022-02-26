import typescript from '@rollup/plugin-typescript';
import resolve from '@rollup/plugin-node-resolve';
import uglify from "@lopatnov/rollup-plugin-uglify";
import commonjs from "@rollup/plugin-commonjs";

const d = new Date();
let year = d.getFullYear();
const packageJson = require('./package.json')
const version = process.env.VERSION || packageJson.version

let license = 'Proprietary and confidential. Unauthorized copying of this file, via any medium is strictly prohibited.';
if (packageJson.license) {
    license = `Released under the ${packageJson.license} License.`;
}
let description = '';
if (packageJson.description) {
    description = `\n* ${packageJson.description}`;
}

let author = 'George Whysall';
if (packageJson.author) {
    author = `${packageJson.author}`;
}

const banner = `/*!
* ${packageJson.name} v${version}${description}
* Written by ${author}, ${year}
* ${license}
*/`;

const external = [
    'jQuery',
    'bootstrap',
    'sweetalert2',
    'dropzone',
    '@simonwep/pickr',
    'tinymce/tinymce',
    'chart.js'
];

export default commandLineArgs => {

    let plugins = [
        resolve({
            moduleDirectories: ['node_modules']
        }),
        commonjs()
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

    } else {

        plugins.push(typescript({
            tsconfig: "tsconfig.rollup.json"
        }));

    }

    return [{
        input: 'src/ts/app.ts',
        output: {
            file: destination + 'js/app.js',
            format: 'umd',
            name: 'hood',
            banner: banner,
            footer: `\
            if (typeof this !== 'undefined' && this.hood){\
              this.hoodCMS = this.Hood = this.hoodCMS = this.HoodCMS = this.hood\
            }`,
            sourcemap: sourcemaps,
            compact: compact
        },
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
    }, {
        input: 'src/ts/app.property.ts',
        output: {
            file: destination + 'js/app.property.js',
            format: 'umd',
            name: 'hood',
            banner: banner,
            footer: `\
            if (typeof this !== 'undefined' && this.hood){\
              this.hoodCMS = this.Hood = this.hoodCMS = this.HoodCMS = this.hood\
            }`,
            sourcemap: sourcemaps,
            compact: compact
        },
        onwarn(warning, rollupWarn) {
            if (warning.code !== 'CIRCULAR_DEPENDENCY') {
                rollupWarn(warning)
            }
        },
        external: external,
        plugins: plugins
    },
    {
        input: 'src/ts/admin.ts',
        output: {
            file: destination + 'js/admin.js',
            format: 'umd',
            name: 'hood',
            banner: banner,
            footer: `\
                if (typeof this !== 'undefined' && this.hood){\
                  this.hoodCMS = this.Hood = this.hoodCMS = this.HoodCMS = this.hood\
                }`,
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
        external: external,
        plugins: plugins
    },
    {
        input: 'src/ts/login.ts',
        output: {
            file: destination + 'js/login.js',
            format: 'umd',
            name: 'hood',
            banner: banner,
            footer: '',
            sourcemap: sourcemaps,
            compact: compact
        },
        external: external,
        plugins: plugins
    }
    ];
}
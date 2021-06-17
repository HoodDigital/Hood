import typescript from '@rollup/plugin-typescript';
import resolve from '@rollup/plugin-node-resolve';
import uglify from "@lopatnov/rollup-plugin-uglify";

export default commandLineArgs => {

    let plugins = [
        resolve({
            moduleDirectories: ['node_modules']
        })
    ]
    let sourcemaps = true;
    let compact = false;
    let destination = 'wwwroot/src/';
    let scssOutputStyle = 'nested';

    if (commandLineArgs.debug !== true) {

        plugins.push(uglify());
        plugins.push(typescript({
            tsconfig: "tsconfig.production.json"
        }));
        sourcemaps = false;
        compact = true;
        destination = 'wwwroot/dist/';
        scssOutputStyle = 'compressed';

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

 //   function loadSassPlugins(output) {
 //       return [
 //           resolve({
 //               moduleDirectories: ['node_modules']
 //           }),
 //           scss({
 //               output: destination + output,
 //               prefix: `/*!
 //* Hood CMS Compiled CSS
 //* To customise, please modify the SCSS and rebuild.
 //* Compiled on {{{compile_date}}}
 //* Copyright 2014 - {{{copyright_year}}} George Whysall & Hood Digital Ltd.
 //*/`,
 //               processor: css => postcss([autoprefixer({ overrideBrowserslist: "Edge 18" })]),
 //               processor: css => css.replace('{{{compile_date}}}', new Date().toLocaleString()).replace('{{{copyright_year}}}', new Date().getFullYear()),
 //               outputStyle: scssOutputStyle,
 //               sourceMapEmbed: true,
 //               sourceMapRoot: './src'
 //           }),
 //           uglify()
 //       ]
 //   }

    return [
        {
            input: 'src/ts/app.ts',
            output: {
                file: destination + 'js/app.js',
                format: 'iife',
                name: 'hood',
                globals: {
                    jQuery: '$',
                    bootstrap: 'bootstrap',
                    sweetalert2: 'Swal',
                    dropzone: 'Dropzone'
                },
                sourcemap: sourcemaps,
                compact: compact
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
                format: 'iife',
                name: 'hood',
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
                format: 'iife',
                name: 'hood.google',
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
    //    ,{
    //        input: 'src/scss/admin.scss',
    //        plugins: loadSassPlugins('css/admin.css'),
    //    },
    //    {
    //        input: 'src/scss/app.scss',
    //        plugins: loadSassPlugins('css/app.css'),
    //    },
    //    {
    //        input: 'src/scss/editor.scss',
    //        plugins: loadSassPlugins('css/editor.css'),
    //    },
    //    {
    //        input: 'src/scss/login.scss',
    //        plugins: loadSassPlugins('css/login.css'),
    //    }
    ];
}
import typescript from '@rollup/plugin-typescript';
import resolve from '@rollup/plugin-node-resolve';
import uglify from "@lopatnov/rollup-plugin-uglify";
import commonjs from "@rollup/plugin-commonjs";

const packageJson = require('./package.json')
const version = process.env.VERSION || packageJson.version

const banner = `/*
 * Copyright (C) Hood Digital Ltd. - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by George Whysall <george@connectmyevent.com>, 2021
 */`;

const footer = `\
if (typeof this !== 'undefined' && this.HoodDigital){\
  this.HoodDigital = this.Hood = this.hood = this.hood\
}`;

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
        input: 'src/ts/site.ts',
        output: {
            file: destination + 'js/site.js',
            format: 'umd',
            name: 'hood',
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
            } else {
                return false;
            }
        },
        external: [
            'jQuery',
            'bootstrap',
            'sweetalert2',
            'dropzone'
        ],
        plugins: plugins
    }];
}
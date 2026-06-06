/**
 * hoodcms/build/gulp — Hood's gulp task registration (HOOD-83).
 *
 * Consumers register Hood's asset tasks instead of cloning the gulpfile:
 *
 *   var gulp = require('gulp');
 *   var { registerHoodTasks } = require('hoodcms/build/gulp');
 *   registerHoodTasks(gulp, {
 *       scss: { src: ['./src/scss/site.scss'] },        // override any task's globs
 *       less: true                                       // opt in to the less pipeline
 *   });
 *
 * Registers: clean, scss, cssnano, copy (copy:src / copy:dist / copy:images) and —
 * when opted in — less. Globs/destinations are overridable per task; everything else
 * (autoprefixer, cssnano preset, tilde importer, binary-safe copies) is Hood's audited
 * configuration.
 */
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var less = require('gulp-less');
var postcss = require('gulp-postcss');
var { rimraf } = require('rimraf');
var sass = require('gulp-dart-sass');
var tilde = require('node-sass-tilde-importer');

/** Hood's cssnano preset — shared by dist + theme minification. */
var cssnanoOpts = cssnano({
    preset: ['default', {
        discardComments: {
            removeAll: true
        }
    }]
});

var defaults = {
    clean: {
        paths: [
            './wwwroot/src/',
            './wwwroot/dist/',
            './dist/',
            './images/',
            './src/js/',
            './src/css/',
            './src/ts/**/*.d.ts'
        ]
    },
    scss: { src: ['./src/scss/*.scss'], dest: './wwwroot/src/css/' },
    cssnano: { src: ['./wwwroot/src/css/*.css'], dest: './wwwroot/dist/css/' },
    less: { src: ['./src/less/*.less'], dest: './wwwroot/src/css/' },
    copySrc: { src: './wwwroot/src/**/*.*', dest: './src/' },
    copyDist: { src: './wwwroot/dist/**/*.*', dest: './dist/' },
    copyImages: { src: './wwwroot/images/**/*.+(png|jpg|gif|svg)', dest: './images/' }
};

function merge(name, overrides) {
    var override = overrides && overrides[name];
    if (override === undefined || override === true) {
        return defaults[name];
    }
    return Object.assign({}, defaults[name], override);
}

/**
 * Register Hood's asset tasks on the consumer's gulp instance. Overrides, keyed by task
 * (clean, scss, cssnano, less, copySrc, copyDist, copyImages), replace globs/destinations;
 * `less: true` opts in to the less pipeline (registered only when requested — Hood itself
 * uses less for themes only).
 */
function registerHoodTasks(gulp, overrides) {
    overrides = overrides || {};

    gulp.task('clean', function () {
        return rimraf(merge('clean', overrides).paths, { glob: true });
    });

    gulp.task('scss', function () {
        var task = merge('scss', overrides);
        return gulp.src(task.src, { sourcemaps: true })
            .pipe(sass({
                outputStyle: 'expanded',
                indentType: 'tab',
                indentWidth: 1,
                importer: tilde
            }).on('error', sass.logError))
            .pipe(postcss([autoprefixer()]))
            .pipe(gulp.dest(task.dest, { sourcemaps: '.' }));
    });

    gulp.task('cssnano', function () {
        var task = merge('cssnano', overrides);
        return gulp.src(task.src)
            .pipe(postcss([cssnanoOpts]))
            .pipe(gulp.dest(task.dest));
    });

    if (overrides.less) {
        gulp.task('less', function () {
            var task = merge('less', overrides);
            return gulp.src(task.src)
                .pipe(less())
                .pipe(postcss([autoprefixer()]))
                .pipe(gulp.dest(task.dest));
        });
    }

    // Copies pass binaries (images, fonts) through untouched — gulp 5 re-encodes stream
    // contents as UTF-8 unless encoding is disabled.
    gulp.task('copy:src', function () {
        var task = merge('copySrc', overrides);
        return gulp.src(task.src, { encoding: false }).pipe(gulp.dest(task.dest));
    });
    gulp.task('copy:dist', function () {
        var task = merge('copyDist', overrides);
        return gulp.src(task.src, { encoding: false }).pipe(gulp.dest(task.dest));
    });
    gulp.task('copy:images', function () {
        var task = merge('copyImages', overrides);
        return gulp.src(task.src, { encoding: false }).pipe(gulp.dest(task.dest));
    });
    gulp.task('copy', gulp.series('copy:src', 'copy:dist', 'copy:images'));
}

module.exports = { registerHoodTasks: registerHoodTasks, cssnanoOpts: cssnanoOpts };

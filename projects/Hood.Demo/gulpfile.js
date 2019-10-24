/*
 *
 *   Includes
 *
 */

var gulp = require('gulp');
var sass = require('gulp-sass');
var less = require('gulp-less');
var rimraf = require('gulp-rimraf');
var cssnano = require('gulp-cssnano');
var rename = require('gulp-rename');
var path = require('path');
var sourcemaps = require('gulp-sourcemaps');

/*
 * 
 *   Site functions, to compile theme less / scss and JS
 * 
 */

gulp.task('themes:clean', function (cb) {
    return gulp.src([
        './wwwroot/themes/*/css/'
    ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});
gulp.task('themes:scss', function () {
    return gulp.src([
        './wwwroot/themes/*/scss/*.scss'
    ])
        .pipe(sourcemaps.init())
        .pipe(
            sass({
                outputStyle: 'expanded',
                indentType: 'tab',
                indentWidth: 1
            }).on('error', sass.logError)
        )
        .pipe(sourcemaps.write())
        .pipe(rename(function (filePath) {
            let parentFolder = path.dirname(filePath.dirname);
            filePath.dirname = path.join(parentFolder, 'css');
        }))
        .pipe(gulp.dest('./wwwroot/themes/'));
});
gulp.task('themes:less', function () {
    lss = less({ relativeUrls: true });
    lss.on('error', function (e) {
        console.log(e);
        lss.end();
    });
    return gulp.src([
        './wwwroot/themes/*/less/*.less'
    ])
        .pipe(sourcemaps.init())
        .pipe(lss)
        .pipe(sourcemaps.write())
        .pipe(rename(function (filePath) {
            let parentFolder = path.dirname(filePath.dirname);
            filePath.dirname = path.join(parentFolder, 'css');
        }))
        .pipe(gulp.dest('./wwwroot/themes/'));
});
gulp.task('themes:cssnano', function () {
    return gulp.src([
        './wwwroot/themes/*/css/*.css'
    ])
        .pipe(cssnano({
            discardComments: {
                removeAll: true
            }
        }))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('./wwwroot/themes/'));
});

gulp.task('themes:build',
    gulp.parallel(
        'themes:scss',
        'themes:less'
    )
);
gulp.task('themes',
    gulp.series('themes:clean',
        'themes:build',
        'themes:cssnano'
    )
);
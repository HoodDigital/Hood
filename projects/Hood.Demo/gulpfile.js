/// <binding BeforeBuild='views' />
// Useful gulp functions for the development of HoodCMS.
// Note this is a demo project and should not be used for production HoodCMS projects.
// In production, you should install the nuget and bower packages to your HoodCMS project.

var gulp = require('gulp'),
    sass = require('gulp-sass'),
    less = require('gulp-less'),
    rimraf = require('gulp-rimraf'),
    cssnano = require('gulp-cssnano'),
    rename = require('gulp-rename'),
    path = require('path'),
    sourcemaps = require('gulp-sourcemaps'),
    hood = {
        js: './wwwroot/hood/js/',
        css: './wwwroot/hood/css/',
        scss: './wwwroot/hood/scss/',
        images: './wwwroot/hood/images/'
    },
    output = {
        js: './../../js/',
        css: './../../css/',
        scss: './../../scss/',
        images: './../../images/',
        sql: './../../sql/'
    };

// Cleans all dist/src/images output folders, as well as the hood folders.
gulp.task('themes:clean', function (cb) {
    return gulp.src([
        './wwwroot/themes/*/css/'
    ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});

// Site workload, to compile theme less/scss and JS.
gulp.task('themes:scss', function () {
    return gulp.src([
        './wwwroot/themes/*/scss/*.scss'
    ])
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: 'expanded', indentType: 'tab', indentWidth: 1 }).on('error', sass.logError))
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

gulp.task('themes:build', gulp.parallel('themes:scss', 'themes:less'));

gulp.task('themes', gulp.series('themes:clean', 'themes:build', 'themes:cssnano'));
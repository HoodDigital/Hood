/// <binding BeforeBuild='publish' />
// Useful gulp functions for the development of HoodCMS.
// Note this is a demo project and should not be used for production HoodCMS projects.
// In production, you should install the nuget and bower packages to your HoodCMS project.

var gulp = require('gulp'),
    path = require('path'),
    fs = require('fs'),
    rimraf = require('gulp-rimraf'),
    less = require('gulp-less'),
    concat = require('gulp-concat'),
    cssmin = require('gulp-cssmin'),
    uglify = require('gulp-uglify'),
    rename = require('gulp-rename'),
    stripCss = require('gulp-strip-css-comments'),
    stripJs = require('gulp-strip-comments'),
    sourcemaps = require('gulp-sourcemaps');

// Site gulpage
jsFolder = './wwwroot/js/',
cssFolder = './wwwroot/css/',
lessFolder = './wwwroot/less/';
libFolder = './wwwroot/lib/';
hoodFolder = './../Hood.Client/';

gulp.task('clean', function (cb) {
    return gulp.src([
        jsFolder + '*.min.js',
        jsFolder + '*.packaged.js',
        cssFolder
    ], { read: false, allowEmpty: true })
    .pipe(rimraf({ force: true }));
});

gulp.task('less:src', function () {
    return gulp
        .src(lessFolder + '*.less')
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(less({ relativeUrls: true }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(cssFolder));
});

gulp.task('less', function () {
    return gulp
        .src(lessFolder + '*.less')
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(less({ relativeUrls: true }))
        .pipe(stripCss({ preserve: false }))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(cssFolder));
});

gulp.task('js', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp
        .src(jsFolder + 'site.js')
        .pipe(l)
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest(jsFolder));
});

gulp.task('js:package', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([
        hoodFolder + 'js/includes/google.js',
        jsFolder + 'site.min.js'
    ])
    .pipe(concat('site.packaged.js'))
    .pipe(l)
    .pipe(gulp.dest(jsFolder))
    .pipe(stripJs())
    .pipe(gulp.dest(jsFolder));
});

gulp.task('watch', function () {
    gulp.watch(lessFolder + '**/*.less', gulp.series('publish'));
});

gulp.task("publish", gulp.series('clean', 'less:src', 'less', 'js', 'js:package'));
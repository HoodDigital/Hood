// Useful gulp functions for the development of HoodCMS.
// Note this is a demo project and should not be used for production HoodCMS projects.
// In production, you should install the nuget and bower packages to your HoodCMS project.

var gulp = require("gulp"),
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
    sourcemaps = require('gulp-sourcemaps'),
    hood = {
        lib: './wwwroot/lib/hood/src/',
        js: './wwwroot/lib/hood/src/js/',
        less: './wwwroot/lib/hood/src/less/',
        images: './wwwroot/lib/hood/images/',
        dist: './wwwroot/lib/hood/dist/'
    },
    output = {
        dist: './../../dist/',
        images: './../../images/',
        src: './../../src/'
    };

// Function to assist in development, copies views from the Hood.Web.MVC project into the Core project.
// This is to speed up development, you can edit views on the demo project, then copy into the core when correct. 
// Note: the demo project does not contain any views in the master repo, unless you add them then the ones you 
// add/edit/work on can then be copied to the Core directory prior to publishing a build.
gulp.task('update-views', function () {
    return gulp.src([
        './Views/**/*.cshtml'
    ], { base: './Views/' })
    .pipe(gulp.dest('./../Hood.Core/Views/'));
});
gulp.task('update-admin-views', function () {
    return gulp.src([
        './Areas/Admin/Views/**/*.cshtml'
    ], { base: './Areas/Admin/Views/' })
    .pipe(gulp.dest('./../Hood.Core/Areas/Admin/Views/'));
});

// Cleans all dist/src/images output folders, as well as the lib/hood/dev dist and lib/hood/src/css folders.
gulp.task('clean', function (cb) {
    return gulp.src([
        hood.dist,
        hood.lib + "/css/",
        output.dist,
        output.images,
        output.src
    ], { read: false })
    .pipe(rimraf({ force: true }));
});

// Processes less and saves the outputted css UNMINIFIED to the src directories.
gulp.task('less', ['less:src'], function () {
    return gulp
        .src(hood.less + '*.less', { base: hood.less })
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(less({ relativeUrls: true }))
        .pipe(stripCss({ preserve: false }))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(output.dist + "/css/"))
        .pipe(gulp.dest(hood.dist + "/css/"));
});

// Processes less and saves the outputted css MINIFIED to the dist directories.
gulp.task('less:src', ['less:copy'], function () {
    return gulp
        .src(hood.less + '*.less', { base: hood.less })
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(less({ relativeUrls: true }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(output.src + "/css/"))
        .pipe(gulp.dest(hood.lib + "/css/"));
});
gulp.task('less:copy', function () {
    return gulp.src(hood.less + "**/*.less", { base: hood.less })
    .pipe(gulp.dest(output.src + "/less/"));
});

// Processes the JS and saves outputted js UNMINIFIED to the src directories, then minifies the output to the dist directories.
gulp.task('js', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src(hood.js + "**/*.js", { base: hood.js })
    .pipe(gulp.dest(output.src + "/js/"))
    .pipe(l)
    .pipe(rename({ suffix: ".min" }))
    .pipe(gulp.dest(output.dist + "/js/"))
    .pipe(gulp.dest(hood.dist + "/js/"));
});

// Copies any files to the dist/src directories.
gulp.task('files', function () {
    return gulp.src([
        hood.lib + "**/*.html",
        hood.lib + "**/*.json",
        hood.lib + "**/*.txt",
        hood.lib + "**/*.xml",
        hood.lib + "**/*.ttf",
        hood.lib + "**/*.woff",
        hood.lib + "**/*.eot",
        hood.lib + "**/*.woff2",
        hood.lib + "**/*.mp4",
        hood.lib + "**/*.flv",
        hood.lib + "**/*.webm",
        hood.lib + "**/*.ogv",
        hood.lib + "**/*.json"
    ], { base: hood.lib })
    .pipe(gulp.dest(output.dist))    
    .pipe(gulp.dest(output.src))
    .pipe(gulp.dest(hood.dist));
});

// Copies any image files from the images directories to the distribution images directory.
// Any minification should occur here.
gulp.task('images', function () {
    return gulp.src([
        hood.images + "**/*.png",
        hood.images + "**/*.jpg",
        hood.images + "**/*.gif",
        hood.images + "**/*.svg"
    ], { base: hood.images })
    .pipe(gulp.dest(output.images));
});

// The build function, copies all less, processes less, copies and processes js, files and images
gulp.task("build", ['less', 'js', 'files', 'images']);

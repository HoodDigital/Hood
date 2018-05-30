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
    lib = './wwwroot/lib/',
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
        './Views/**/*.cshtml',
        './Views/**/*.json'
    ], { base: './Views/' })
    .pipe(gulp.dest('./../Hood/Views/'));
});
gulp.task('update-admin-views', function () {
    return gulp.src([
        './Areas/Admin/Views/**/*.cshtml',
        './Areas/Admin/Views/**/*.json'
    ], { base: './Areas/Admin/Views/' })
    .pipe(gulp.dest('./../Hood/Areas/Admin/Views/'));
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

// Bundle the app into the packaged form, so sites can use the app.packaged.js file.
gulp.task("js:package:app", function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([
        libFolder + 'jquery-validation/dist/jquery.validate.min.js',
        libFolder + 'jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
        hood.js + "includes/production.js",
        lib + 'loaders.css/loaders.css.js',
        lib + 'FitVids/jquery.fitvids.js',
        hood.js + "includes/globals.js",
        hood.js + "includes/stringhelpers.js",
        hood.js + "includes/alerts.js",
        hood.js + "includes/helpers.js",
        hood.js + "includes/forms.js",
        hood.js + "includes/handlers.js",
        hood.js + "includes/pager.js",
        hood.js + "includes/validator.js",
        hood.js + "includes/modals.js",
        hood.js + "includes/inline.js",
        hood.js + "includes/addresses.js",
        hood.js + "includes/cart.js",
        hood.js + "app.js"
    ], { base: '.' })
    .pipe(concat('app.packaged.js'))
    .pipe(l)
    .pipe(gulp.dest(output.dist + 'js/'))
    .pipe(gulp.dest(hood.dist + 'js/'));
});

// Package the login javascript
gulp.task("js:package:login", function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([
        lib + 'jQuery-Mask-Plugin/dist/jquery.mask.js',
        hood.js + "includes/production.js",
        hood.js + 'login.js'
    ], { base: '.' })
    .pipe(concat('login.packaged.js'))
    .pipe(l)
    .pipe(gulp.dest(output.dist + 'js/'))
    .pipe(gulp.dest(hood.dist + 'js/'));
});

// Package the admin Javascript
gulp.task("js:package:admin", function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([
        hood.js + "includes/production.js",
        hood.js + "includes/globals.js",
        hood.js + "includes/stringhelpers.js",
        hood.js + "includes/uploader.js",
        hood.js + "includes/alerts.js",
        hood.js + "includes/helpers.js",
        hood.js + "includes/forms.js",
        hood.js + "includes/handlers.js",
        hood.js + "includes/datalist.js",
        hood.js + "includes/fileupload.js",
        hood.js + "includes/observable.js",
        hood.js + "includes/pager.js",
        hood.js + "includes/validator.js",
        hood.js + "includes/skininit.js",
        hood.js + "includes/blades.js",
        hood.js + "includes/modals.js",
        hood.js + "includes/inline.js",
        hood.js + "includes/core.js",
        hood.js + "includes/media.js",
        hood.js + "includes/users.js",
        hood.js + "includes/themes.js",
        hood.js + "includes/property.js",
        hood.js + "includes/subscriptions.js",
        hood.js + "includes/content.js",
        hood.js + "includes/forums.js",
        hood.js + "includes/logs.js",
        hood.js + "includes/google.js",
        hood.js + "admin.js"
    ], { base: '.' })
    .pipe(concat('admin.packaged.js'))
    .pipe(l)
    .pipe(gulp.dest(output.dist + 'js/'))
    .pipe(gulp.dest(hood.dist + 'js/'));
});

// The build function, copies all less, processes less, copies and processes js, files and images
gulp.task("package", ['js:package:admin', 'js:package:app', 'js:package:login']);
gulp.task("build", ['less', 'js', 'files', 'images', 'package']);

// Site gulpage
jsFolder = './wwwroot/js/',
cssFolder = './wwwroot/css/',
lessFolder = './wwwroot/less/';
libFolder = './wwwroot/lib/';
hoodFolder = './wwwroot/lib/hood/';

gulp.task('site:clean', function (cb) {
    return gulp.src([
        jsFolder + '*.min.js',
        jsFolder + '*.packaged.js',
        cssFolder
    ], { read: false })
    .pipe(rimraf({ force: true }));
});
gulp.task('site:less', ['site:less:src'], function () {
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
gulp.task('site:less:src', function () {
    return gulp
        .src(lessFolder + '*.less')
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(less({ relativeUrls: true }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(cssFolder));
});
gulp.task('site:js', function () {
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
gulp.task('site:js:package', ['site:js'], function () {
    return gulp.src([
        libFolder + 'hood/dist/js/includes/google.min.js',
        jsFolder + 'site.min.js',
    ])
    .pipe(concat('site.packaged.js'))
    .pipe(gulp.dest(jsFolder))
    .pipe(stripJs())
    .pipe(gulp.dest(jsFolder));
});
// TODO: Add themes less/js processing.

gulp.task("publish", ['site:less', 'site:js', 'site:js:package']);
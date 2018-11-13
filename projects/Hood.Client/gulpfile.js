/// <binding ProjectOpened='build' />
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
    lib = './lib/',
    hood = {
        js: './js/',
        less: './less/',
        images: './images/'
    },
    demo = {
        dist: './../Hood.Web/wwwroot/lib/hood/dist/',
        images: './../Hood.Web/wwwroot/lib/hood/images/',
        src: './../Hood.Web/wwwroot/lib/hood/src/'
    },
    output = {
        dist: './../../dist/',
        images: './../../images/',
        src: './../../src/'
    };

// Cleans all dist/src/images output folders, as well as the Hood.Web lib/hood folders.
gulp.task('clean', function (cb) {
    return gulp.src([
        output.dist,
        output.images,
        output.src,
        demo.dist,
        demo.images,
        demo.src
    ], { read: false, allowEmpty: true })
    .pipe(rimraf({ force: true }));
});

// Copies all less files to the src directory and the Hood.Web src directory.
gulp.task('less:copy', function () {
    return gulp.src(hood.less + "**/*.less")
    .pipe(gulp.dest(demo.src + "/less/"))
    .pipe(gulp.dest(output.src + "/less/"));
});

// Processes less and saves the outputted css UNMINIFIED to the src directories and the Hood.Web src directory.
gulp.task('less:src', function () {
    return gulp
        .src(hood.less + '*.less')
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(less({ relativeUrls: true }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(output.src + "/css/"))
        .pipe(gulp.dest(demo.src + "/css/"));
});

// Processes less and saves the outputted css MINIFIED to the dist directories and the Hood.Web dist directory.
gulp.task('less', function () {
    lss = less({ relativeUrls: true });
    lss.on('error', function (e) {
        console.log(e);
        lss.end();
    });
    return gulp
        .src(hood.less + '*.less')
        .pipe(sourcemaps.init({ largeFile: true }))
        .pipe(lss)
        .pipe(stripCss({ preserve: false }))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(sourcemaps.write("/"))
        .pipe(gulp.dest(output.dist + "/css/"))
        .pipe(gulp.dest(demo.dist + "/css/"));
});

// Processes the JS and saves outputted js UNMINIFIED to the src directories and the Hood.Web src directory, 
// then minifies the output to the dist directories and the Hood.Web dist directory.
gulp.task('js', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src(hood.js + "**/*.js", { base: hood.js })
    .pipe(gulp.dest(output.src + "/js/"))
    .pipe(gulp.dest(demo.src + "/js/"))
    .pipe(l)
    .pipe(rename({ suffix: ".min" }))
    .pipe(gulp.dest(output.dist + "/js/"))
    .pipe(gulp.dest(demo.dist + "/js/"));
});

// Copies any image files from the images directories to the distribution images directory and the Hood.Web images directory.
gulp.task('images', function () {
    return gulp.src([
        hood.images + "**/*.png",
        hood.images + "**/*.jpg",
        hood.images + "**/*.gif",
        hood.images + "**/*.svg"
    ], { base: hood.images })
    .pipe(gulp.dest(demo.images))
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
        hood.js + "includes/production.js",
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
    .pipe(gulp.dest(demo.dist + 'js/'));
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
    .pipe(gulp.dest(demo.dist + 'js/'));
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
        hood.js + "includes/pager.js",
        hood.js + "includes/validator.js",
        hood.js + "includes/blades.js",
        hood.js + "includes/modals.js",
        hood.js + "includes/inline.js",
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
    .pipe(gulp.dest(demo.dist + 'js/'));
});

// The build function, copies all less, processes less, copies and processes js, files and images
gulp.task("package", gulp.series('js:package:admin', 'js:package:app', 'js:package:login'));
gulp.task("build", gulp.series('clean', 'less:copy', gulp.parallel('less:src', 'less', 'js', 'images'), 'package'));
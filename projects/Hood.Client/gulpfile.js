/// <binding AfterBuild='build' />
// Useful gulp functions for the development of HoodCMS.
// Note this is a demo project and should not be used for production HoodCMS projects.
// In production, you should install the nuget and bower packages to your HoodCMS project.

var gulp = require("gulp"),
    rimraf = require('gulp-rimraf'),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    rename = require('gulp-rename'),
    lib = './lib/',
    hood = {
        js: './hood/js/',
        css: './hood/css/',
        scss: './hood/scss/',
        images: './hood/images/'
    },
    demo = {
        srcjs: './../Hood.Web/wwwroot/hood/js/',
        distjs: './../Hood.Web/wwwroot/hood/js/',
        css: './../Hood.Web/wwwroot/hood/css/',
        scss: './../Hood.Web/wwwroot/hood/scss/',
        images: './../Hood.Web/wwwroot/hood/images/'
    },
    output = {
        srcjs: './../../js/',
        distjs: './../../js/',
        css: './../../css/',
        scss: './../../scss/',
        images: './../../images/'
    };

// Cleans all dist/src/images output folders, as well as the Hood.Web lib/hood folders.
gulp.task('clean', function (cb) {
    return gulp.src([
        output.images,
        output.css,
        output.scss,
        output.distjs,
        output.srcjs,
        output.images,
        demo.images,
        demo.css,
        demo.scss,
        demo.distjs,
        demo.srcjs,
        demo.images
    ], { read: false, allowEmpty: true })
    .pipe(rimraf({ force: true }));
});

// Copies all less files to the src directory and the Hood.Web src directory.
gulp.task('css:copy', function () {
    return gulp.src(hood.css + "/**/*.css")
    .pipe(gulp.dest(demo.css))
    .pipe(gulp.dest(output.css));
});
gulp.task('scss:copy', function () {
    return gulp.src(hood.scss + "**/*.scss")
        .pipe(gulp.dest(demo.scss))
        .pipe(gulp.dest(output.scss));
});

// then minifies the output to the dist directories and the Hood.Web dist directory.
gulp.task('js', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src(hood.js + "**/*.js", { base: hood.js })
    .pipe(gulp.dest(output.srcjs))
    .pipe(gulp.dest(demo.srcjs))
    .pipe(l)
    .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest(output.distjs))
        .pipe(gulp.dest(demo.distjs));
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
    .pipe(gulp.dest(demo.distjs))
    .pipe(gulp.dest(output.distjs));
});

// Package the login javascript
gulp.task("js:package:login", function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([
        lib + 'jquery-mask/jquery.mask.js',
        hood.js + "includes/production.js",
        hood.js + 'login.js'
    ], { base: '.' })
    .pipe(concat('login.packaged.js'))
    .pipe(l)
    .pipe(gulp.dest(demo.distjs))
    .pipe(gulp.dest(output.distjs));
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
    .pipe(gulp.dest(demo.distjs))
    .pipe(gulp.dest(output.distjs));
});

// The build function, copies all less, processes less, copies and processes js, files and images
gulp.task("package", gulp.series('js:package:admin', 'js:package:app', 'js:package:login'));
gulp.task("build", gulp.series('clean', gulp.parallel('css:copy', 'scss:copy', 'js', 'images'), 'package'));
gulp.task('watch', function () {
    gulp.watch([
        hood.css + '**/*.css',
        hood.js + '**/*.js'
    ], gulp.series('build'));
});

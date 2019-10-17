/*
 *
 *   Includes
 *
 */

var gulp = require('gulp');
var babel = require('gulp-babel');
var sass = require('gulp-sass');
var less = require('gulp-less');
var rimraf = require('gulp-rimraf');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var cssnano = require('gulp-cssnano');
var rename = require('gulp-rename');
var imagemin = require('gulp-imagemin');
var path = require('path');
var sourcemaps = require('gulp-sourcemaps');

/*
 *
 *   Variables
 *
 */

var lib = './wwwroot/lib/';
var hood = {
    js: './wwwroot/hood/js/',
    css: './wwwroot/hood/css/',
    scss: './wwwroot/hood/scss/',
    less: './wwwroot/hood/less/',
    images: './wwwroot/hood/images/'
};
var output = {
    js: './../../js/',
    css: './../../css/',
    scss: './../../scss/',
    less: './../../less/',
    images: './../../images/',
    sql: './../../sql/'
};

/*
 *
 *   Hood functions, to compile Hood less / scss and JS for the npm Package
 *
 */

gulp.task('hood:clean', function (cb) {
    return gulp.src([
        output.css,
        output.scss,
        output.js,
        output.images,
        hood.css,
        hood.js + 'theme.*.js'
    ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});

gulp.task('hood:scss', function () {
    return gulp.src(hood.scss + '*.scss')
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: 'expanded', indentType: 'tab', indentWidth: 1 }).on('error', sass.logError))
        .pipe(sourcemaps.write())
        .pipe(gulp.dest(hood.css));
});
gulp.task('hood:scss:copy', function () {
    return gulp.src(hood.scss + "**/*.scss")
        .pipe(gulp.dest(output.scss));
});

gulp.task('hood:theme:bootstrap4', function () {
    return gulp.src([
        './wwwroot/themes/bootstrap4/scss/styles.scss',
        './wwwroot/themes/bootstrap4/scss/preload.scss'
    ])
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: 'expanded', indentType: 'tab', indentWidth: 1 }).on('error', sass.logError))
        .pipe(sourcemaps.write())
        .pipe(rename({ suffix: '.bs4' }))
        .pipe(gulp.dest('./wwwroot/hood/css/'));
});
gulp.task('hood:theme:bootstrap3', function () {
    lss = less({ relativeUrls: true });
    lss.on('error', function (e) {
        console.log(e);
        lss.end();
    });

    return gulp.src([
        './wwwroot/themes/bootstrap3/less/styles.less',
        './wwwroot/themes/bootstrap3/less/preload.less'
    ])
        .pipe(sourcemaps.init())
        .pipe(lss)
        .pipe(sourcemaps.write())
        .pipe(rename({ suffix: '.bs3' }))
        .pipe(gulp.dest('./wwwroot/hood/css/'));
});

gulp.task('hood:theme:js:bootstrap3', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src('./wwwroot/themes/bootstrap3/js/theme.js')
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(rename({ suffix: '.bs3' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js))
        .pipe(l)
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});
gulp.task('hood:theme:js:bootstrap4', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src('./wwwroot/themes/bootstrap4/js/theme.js')
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(rename({ suffix: '.bs4' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js))
        .pipe(l)
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});


gulp.task('hood:less:copy', function () {
    return gulp.src(hood.less + "**/*.less")
        .pipe(gulp.dest(output.less));
});

gulp.task('hood:cssnano', function () {
    return gulp.src([hood.css + '**/*.css', '!' + hood.css + '**/*.min.css'])
        .pipe(gulp.dest(output.css))
        .pipe(cssnano({
            discardComments: {
                removeAll: true
            }
        }))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(hood.css))
        .pipe(gulp.dest(output.css));
});

gulp.task('hood:images', function () {
    return gulp.src(hood.images + '**/*.+(png|jpg|gif|svg)')
        .pipe(imagemin())
        .pipe(gulp.dest(output.images));
});

gulp.task('hood:js', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([hood.js + '**/*.js', '!' + hood.js + '**/*.min.js', '!' + hood.js + '**/*.packaged.js'], { base: hood.js })
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(gulp.dest(output.js))
        .pipe(l)
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});
gulp.task('hood:js:core', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([

        hood.js + 'core/production.js',
        hood.js + 'core/globals.js',

        hood.js + 'core/helpers.js',
        hood.js + 'core/handlers.js',
        hood.js + 'core/stringhelpers.js',
        hood.js + 'core/validator.js',

        hood.js + 'core/addresses.js',
        hood.js + 'core/alerts.js',
        hood.js + 'core/forms.js',
        hood.js + 'core/inline.js',
        hood.js + 'core/media.js'

    ], { base: '.' })
        .pipe(concat('core.js'))
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js))
        .pipe(l)
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});
gulp.task('hood:js:package:app', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([

        hood.js + 'core/production.js',
        hood.js + 'core/globals.js',

        hood.js + 'core/helpers.js',
        hood.js + 'core/handlers.js',
        hood.js + 'core/stringhelpers.js',
        hood.js + 'core/validator.js',

        hood.js + 'core/addresses.js',
        hood.js + 'core/alerts.js',
        hood.js + 'core/forms.js',
        hood.js + 'core/inline.js',
        hood.js + 'core/media.js',

        hood.js + 'app/cart.js',
        hood.js + 'app/stripe.js',
        hood.js + 'app.js'

    ], { base: '.' })
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(concat('app.packaged.js'))
        .pipe(l)
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});
gulp.task('hood:js:package:login', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([

        lib + 'jquery-mask/jquery.mask.js',

        hood.js + 'login.js'

    ], { base: '.' })
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(concat('login.packaged.js'))
        .pipe(l)
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});
gulp.task('hood:js:package:admin', function () {
    l = uglify({});
    l.on('error', function (e) {
        console.log(e);
        l.end();
    });
    return gulp.src([

        hood.js + 'core.js',

        hood.js + 'admin/content.js',
        hood.js + 'admin/forums.js',
        hood.js + 'admin/logs.js',
        hood.js + 'admin/property.js',
        hood.js + 'admin/subscriptions.js',
        hood.js + 'admin/themes.js',
        hood.js + 'admin/users.js',

        hood.js + 'app/google.js',

        hood.js + 'admin.js'

    ], { base: '.' })
        .pipe(babel({
            presets: ['@babel/env']
        }))
        .pipe(concat('admin.packaged.js'))
        .pipe(l)
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));

});

gulp.task('hood:package',
    gulp.series(
        'hood:js',
        'hood:js:core',
        gulp.parallel(
            'hood:js:package:admin',
            'hood:js:package:app',
            'hood:js:package:login'
        )
    )
);
gulp.task('hood',
    gulp.series(
        'hood:clean',
        gulp.parallel(
            'hood:scss',
            'hood:js',
            'hood:images'
        ),
        gulp.parallel(
            'hood:theme:js:bootstrap3',
            'hood:theme:bootstrap3',
            'hood:theme:js:bootstrap4',
            'hood:theme:bootstrap4'
        ),
        gulp.parallel(
            'hood:scss:copy',
            'hood:less:copy',
            'hood:cssnano',
            'hood:package'
        )
    )
);

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
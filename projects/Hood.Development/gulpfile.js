/// <binding BeforeBuild='hood:views' />
/*
 *
 *   Includes
 *
 */

var gulp = require('gulp');
var babel = require('gulp-babel');
var less = require('gulp-less');
var rimraf = require('gulp-rimraf');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var cssnano = require('gulp-cssnano');
var rename = require('gulp-rename');
var imagemin = require('gulp-imagemin');
var path = require('path');
var sourcemaps = require('gulp-sourcemaps');

gulp.task('clean', function (cb) {
    return gulp.src([
        './wwwroot/src/',
        './wwwroot/dist/',
        './dist/',
        './src/js/',
        './src/css/'
    ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});
gulp.task('copy:src', function () {
    return gulp.src('./wwwroot/src/**/*.*')
        .pipe(gulp.dest('./src/'));
});
gulp.task('copy:dist', function () {
    return gulp.src('./wwwroot/dist/**/*.*')
        .pipe(gulp.dest('./dist/'));
});
gulp.task('copy',
    gulp.series(
        'copy:src',
        'copy:dist'
    )
);


/*
 *
 *   LEGACY STUFF
 *
 */

var lib = './wwwroot/lib/';
var hood = {
    js: './wwwroot/hood/js/',
    css: './wwwroot/hood/css/',
    less: './wwwroot/hood/less/',
    images: './wwwroot/hood/images/'
};
var output = {
    js: './../../js/',
    css: './../../css/',
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
            presets: ['@babel/preset-env']
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
            presets: ['@babel/preset-env']
        }))
        .pipe(rename({ suffix: '.bs4' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js))
        .pipe(l)
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(hood.js))
        .pipe(gulp.dest(output.js));
});

gulp.task('hood:button:bootstrap3', function () {
    lss = less({ relativeUrls: true });
    lss.on('error', function (e) {
        console.log(e);
        lss.end();
    });

    return gulp.src([
        './wwwroot/hood/less/button/button.less'
    ])
        .pipe(sourcemaps.init())
        .pipe(lss)
        .pipe(sourcemaps.write())
        .pipe(rename({ suffix: '.bs3' }))
        .pipe(gulp.dest('./wwwroot/hood/css/'));
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
        .pipe(gulp.dest(hood.images))
        .pipe(gulp.dest(output.images));
});

gulp.task('hood:views:clean', function (cb) {
    return gulp.src([
        './../Hood.UI.Core/BaseUI/',
        './../Hood.UI.Bootstrap3/UI/',
        './../Hood.UI.Bootstrap4/UI/',
        './../Hood.Admin/Areas/Admin/UI/'
    ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});
gulp.task('hood:views:core', function () {
    return gulp.src('./Themes/core/Views/**/*.*')
        .pipe(gulp.dest('./../Hood.UI.Core/BaseUI/'));
});
gulp.task('hood:views:bootstrap3', function () {
    return gulp.src('./Themes/bootstrap3/Views/**/*.*')
        .pipe(gulp.dest('./../Hood.UI.Bootstrap3/UI/'));
});
gulp.task('hood:views:bootstrap4', function () {
    return gulp.src('./Themes/bootstrap4/Views/**/*.*')
        .pipe(gulp.dest('./../Hood.UI.Bootstrap4/UI/'));
});
gulp.task('hood:views:admin', function () {
    return gulp.src('./Areas/Admin/UI/**/*.*')
        .pipe(gulp.dest('./../Hood.Admin/Areas/Admin/UI/'));
});

gulp.task('hood:views',
    gulp.series(
        'hood:views:clean',
        gulp.parallel(
            'hood:views:core',
            'hood:views:bootstrap3',
            'hood:views:bootstrap4',
            'hood:views:admin'
        )
    )
);

gulp.task('hood',
    gulp.series(
        'hood:clean',
        'hood:views',
        gulp.parallel(
            'hood:images'
        ),
        gulp.parallel(
            'hood:button:bootstrap3',
            'hood:theme:js:bootstrap3',
            'hood:theme:bootstrap3',
        ),
        gulp.parallel(
            'hood:less:copy',
            'hood:cssnano'
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

gulp.task('themes',
    gulp.series('themes:clean',
        'themes:less',
        'themes:cssnano'
    )
); 
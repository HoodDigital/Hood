/// <binding BeforeBuild='views' />

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
        './images/',
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
gulp.task('copy:images', function () {
    return gulp.src('./wwwroot/images/**/*.+(png|jpg|gif|svg)')
        .pipe(imagemin())
        .pipe(gulp.dest('./images/'));
});

gulp.task('copy',
    gulp.series(
        'copy:src',
        'copy:dist',
        'copy:images'
    )
);

gulp.task('themes:bootstrap3:less', function () {
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
gulp.task('themes:bootstrap3:button', function () {
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

gulp.task('views:clean', function (cb) {
    return gulp.src([
        './../Hood.UI.Core/BaseUI/',
        './../Hood.UI.Bootstrap3/UI/',
        './../Hood.UI.Bootstrap4/UI/',
        './../Hood.Admin/Areas/Admin/UI/'
    ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});
gulp.task('views:core', function () {
    return gulp.src('./Views/**/*.*')
        .pipe(gulp.dest('./../Hood.UI.Core/BaseUI/'));
});
gulp.task('views:bootstrap3', function () {
    return gulp.src('./Themes/bootstrap3/Views/**/*.*')
        .pipe(gulp.dest('./../Hood.UI.Bootstrap3/UI/'));
});
gulp.task('views:bootstrap4', function () {
    return gulp.src('./Themes/bootstrap4/Views/**/*.*')
        .pipe(gulp.dest('./../Hood.UI.Bootstrap4/UI/'));
});
gulp.task('views:admin', function () {
    return gulp.src('./Areas/Admin/UI/**/*.*')
        .pipe(gulp.dest('./../Hood.Admin/Areas/Admin/UI/'));
});

gulp.task('views',
    gulp.series(
        'views:clean',
        gulp.parallel(
            'views:core',
            'views:bootstrap3',
            'views:bootstrap4',
            'views:admin'
        )
    )
);

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
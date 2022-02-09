var autoprefixer = require('gulp-autoprefixer');
var gulp = require('gulp');
var cssnano = require('gulp-cssnano');
var imagemin = require('gulp-imagemin');
var less = require('gulp-less');
var path = require('path');
var rename = require('gulp-rename');
var rimraf = require('gulp-rimraf');
var sass = require('gulp-dart-sass');
var sourcemaps = require('gulp-sourcemaps');
var tilde = require('node-sass-tilde-importer');

gulp.task('clean', function(cb) {
    return gulp.src([
            './wwwroot/css/',
            './wwwroot/src/',
            './wwwroot/dist/'
        ], { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});

gulp.task('less', function() {
    return gulp
        .src([
            './src/less/*.less'
        ])
        .pipe(sourcemaps.init())
        .pipe(less({ relativeUrls: true }))
        .pipe(sourcemaps.write(''))
        .pipe(gulp.dest('./wwwroot/src/css/'));
});

gulp.task('scss', function() {
    return gulp.src([
            './src/scss/*.scss'
        ])
        .pipe(sourcemaps.init())
        .pipe(autoprefixer())
        .pipe(sass({
            outputStyle: 'expanded',
            indentType: 'tab',
            indentWidth: 1,
            importer: tilde
        }).on('error', sass.logError))
        .pipe(sourcemaps.write(''))
        .pipe(gulp.dest('./wwwroot/src/css/'));
});

gulp.task('images', function () {
    return gulp.src('./src/images/**/*.+(png|jpg|gif|svg)')
        .pipe(imagemin())
        .pipe(gulp.dest('./wwwroot/images/'));
});

gulp.task('cssnano', function() {
    return gulp.src([
            './wwwroot/src/css/*.css'
        ])
        .pipe(cssnano({
            discardComments: {
                removeAll: true
            }
        }))
        .pipe(gulp.dest('./wwwroot/dist/css/'));
});
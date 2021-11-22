var autoprefixer = require('gulp-autoprefixer');
var gulp = require('gulp');
var cssnano = require('gulp-cssnano');
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
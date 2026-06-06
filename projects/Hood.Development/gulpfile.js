// Hood.Development's gulpfile — the standard asset tasks come from the published preset
// (dogfood, HOOD-83); only the Hood-specific themes pipeline lives here.
var gulp = require('gulp');
var less = require('gulp-less');
var path = require('path');
var postcss = require('gulp-postcss');
var rename = require('gulp-rename');
var { rimraf } = require('rimraf');
var sass = require('gulp-dart-sass');
var { registerHoodTasks, cssnanoOpts } = require('hoodcms/build/gulp');

registerHoodTasks(gulp);

gulp.task('themes:clean', function() {
    return rimraf('./wwwroot/themes/*/css/', { glob: true });
});
gulp.task('themes:scss', function() {
    return gulp.src([
            './wwwroot/themes/*/scss/*.scss'
        ], { sourcemaps: true })
        .pipe(sass({ outputStyle: 'expanded', indentType: 'tab', indentWidth: 1 }).on('error', sass.logError))
        .pipe(rename(function(filePath) {
            let parentFolder = path.dirname(filePath.dirname);
            filePath.dirname = path.join(parentFolder, 'css');
        }))
        .pipe(gulp.dest('./wwwroot/themes/', { sourcemaps: true }));
});
gulp.task('themes:less', function() {
    lss = less({ relativeUrls: true });
    lss.on('error', function(e) {
        console.log(e);
        lss.end();
    });
    return gulp.src([
            './wwwroot/themes/*/less/*.less'
        ], { sourcemaps: true })
        .pipe(lss)
        .pipe(rename(function(filePath) {
            let parentFolder = path.dirname(filePath.dirname);
            filePath.dirname = path.join(parentFolder, 'css');
        }))
        .pipe(gulp.dest('./wwwroot/themes/', { sourcemaps: true }));
});
gulp.task('themes:cssnano', function() {
    return gulp.src([
            './wwwroot/themes/*/css/*.css'
        ])
        .pipe(postcss([cssnanoOpts]))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('./wwwroot/themes/'));
});
gulp.task('themes',
    gulp.series('themes:clean',
        'themes:less',
        'themes:cssnano'
    )
);

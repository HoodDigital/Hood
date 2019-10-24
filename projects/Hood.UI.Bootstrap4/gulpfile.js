// Copies view files in from Hood.Web dev project on build.
var gulp = require('gulp'),
    rimraf = require('gulp-rimraf');

gulp.task('clean', function (cb) {
    return gulp.src('./UI/', { read: false, allowEmpty: true })
        .pipe(rimraf({ force: true }));
});
gulp.task('copy', function () {
    return gulp.src('./../Hood.Web/Themes/bootstrap4/Views/**/*.*')
        .pipe(gulp.dest('./UI/'));
});
gulp.task('build', gulp.series('clean', 'copy'));

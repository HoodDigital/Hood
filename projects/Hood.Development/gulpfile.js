var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var gulp = require('gulp');
var less = require('gulp-less');
var path = require('path');
var postcss = require('gulp-postcss');
var rename = require('gulp-rename');
var { rimraf } = require('rimraf');
var sass = require('gulp-dart-sass');
var tilde = require('node-sass-tilde-importer');

// cssnano preset shared by the dist + theme minification tasks.
var cssnanoOpts = cssnano({
    preset: ['default', {
        discardComments: {
            removeAll: true
        }
    }]
});

gulp.task('clean', function() {
    return rimraf([
        './wwwroot/src/',
        './wwwroot/dist/',
        './dist/',
        './images/',
        './src/js/',
        './src/css/',
        './src/ts/**/*.d.ts'
    ], { glob: true });
});

// Copies pass binaries (images, fonts) through untouched — gulp 5 re-encodes
// stream contents as UTF-8 unless encoding is disabled.
gulp.task('copy:src', function() {
    return gulp.src('./wwwroot/src/**/*.*', { encoding: false })
        .pipe(gulp.dest('./src/'));
});
gulp.task('copy:dist', function() {
    return gulp.src('./wwwroot/dist/**/*.*', { encoding: false })
        .pipe(gulp.dest('./dist/'));
});
gulp.task('copy:images', function() {
    return gulp.src('./wwwroot/images/**/*.+(png|jpg|gif|svg)', { encoding: false })
        .pipe(gulp.dest('./images/'));
});
gulp.task('copy',
    gulp.series(
        'copy:src',
        'copy:dist',
        'copy:images'
    )
);


gulp.task('scss', function() {
    return gulp.src([
            './src/scss/*.scss'
        ], { sourcemaps: true })
        .pipe(sass({
            outputStyle: 'expanded',
            indentType: 'tab',
            indentWidth: 1,
            importer: tilde
        }).on('error', sass.logError))
        .pipe(postcss([autoprefixer()]))
        .pipe(gulp.dest('./wwwroot/src/css/', { sourcemaps: '.' }));
});


gulp.task('cssnano', function() {
    return gulp.src([
            './wwwroot/src/css/*.css'
        ])
        .pipe(postcss([cssnanoOpts]))
        //.pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('./wwwroot/dist/css/'));
});


gulp.task('views:clean', function() {
    return rimraf([
        './../Hood.UI.Core/BaseUI/',
        './../Hood.UI.Bootstrap3/UI/',
        './../Hood.UI.Bootstrap4/UI/',
        './../Hood.UI.Admin/Areas/Admin/UI/'
    ]);
});
gulp.task('views:core', function() {
    return gulp.src('./Views/**/*.*', { encoding: false })
        .pipe(gulp.dest('./../Hood.UI.Core/BaseUI/'));
});
gulp.task('views:bootstrap3', function() {
    return gulp.src('./Themes/bootstrap3/Views/**/*.*', { encoding: false })
        .pipe(gulp.dest('./../Hood.UI.Bootstrap3/UI/'));
});
gulp.task('views:bootstrap4', function() {
    return gulp.src('./Themes/bootstrap4/Views/**/*.*', { encoding: false })
        .pipe(gulp.dest('./../Hood.UI.Bootstrap4/UI/'));
});
gulp.task('views:admin', function() {
    return gulp.src('./Areas/Admin/UI/**/*.*', { encoding: false })
        .pipe(gulp.dest('./../Hood.UI.Admin/Areas/Admin/UI/'));
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

/// <binding ProjectOpened='watch-js' />
var $ = require("gulp-load-plugins")({ lazy: true });
var gulp = $.help(require("gulp"));
var args = require("yargs").argv;
var config = require("./gulp/config/gulpfile.config")();
var logger = require("./gulp/helper/logger");
var gulpSequence = require("gulp-sequence");
var gulpLess = require("gulp-less");
const tscConfig = require("./tsconfig.json");
const del = require("del");
var run = require('gulp-run-command').default;

gulp.task('AngularBuild',
  run([
      'ng build staffSurvey --prod --output-hashing none',
      'ng build internalConnections --prod --output-hashing none',
      'ng build attributePicker --prod --output-hashing none',
      'ng build tripMaps --prod --output-hashing none'
    ]
  ));

  
gulp.task("default", gulpSequence("clean", ["copy:libsJs", "copy:libsCss", "compileTs","AngularBuild"]));

var tsProject;
// clean the contents of the distribution directory
gulp.task("clean",
    function() {
        var taskName = this.seq.slice(0, 1);
        logger.info("Started", taskName);
        return del("dist/**/*");
    });


// copy vendor libraries javascript files
gulp.task("copy:libsJs",
    function() {
        var taskName = this.seq.slice(0, 1);
        logger.info("Started", taskName);
        return gulp.src([
                "node_modules/jquery/dist/jquery.min.js",
                "node_modules/@progress/kendo-ui/js/kendo.autocomplete.js",
                "node_modules/@progress/kendo-ui/js/kendo.dataviz.js",
                "node_modules/@progress/kendo-ui/js/kendo.editor.js",
                "node_modules/@progress/kendo-ui/js/kendo.progressbar.js",
                "node_modules/@progress/kendo-ui/js/kendo.scheduler.js",
                "node_modules/qs/dist/qs.js",
                "node_modules/vis/dist/vis.min.js",
                "node_modules/moment/min/moment-with-locales.min.js"
            ])
            .pipe(gulp.dest(config.jsVendorDest));
    });

// copy vendor css files
gulp.task("copy:libsCss",
    function() {
        var taskName = this.seq.slice(0, 1);
        logger.info("Started", taskName);
        return gulp.src([
                "node_modules/@progress/kendo-ui/css/web/kendo.common.min.css",
                "node_modules/@progress/kendo-ui/css/web/kendo.default.min.css",
                "node_modules/@progress/kendo-ui/css/web/kendo.default.mobile.min.css",
                "node_modules/vis/dist/vis.min.css",
                "node_modules/bootstrap/dist/css/bootstrap.min.css"
            ])
            .pipe(gulp.dest(config.cssVendorDest));
    });

//Compile Typescript files to javascript files
gulp.task("compileTs",
    function() {
        var taskName = this.seq.slice(0, 1);
        logger.info("Started", taskName);
        var ts = require("gulp-typescript");
        return gulp
            .src(config.tsAppSrc)
            .pipe($.if(args.print, $.debug({ tile: "File" })))
            .pipe($.plumber({
                errorHandler: function(err) {
                    logger.error(err, taskName);
                }
            }))
            .pipe(ts(tscConfig.compilerOptions))
            .pipe(gulp.dest(config.jsAppDest));
    },
    {
        options: {
            'print': "Prints the file name that is parsed"
        }
    });

gulp.task('compile-less', function () {
    gulp.src(config.lessAppSrc)
        .pipe(gulpLess())
        .pipe(gulp.dest(config.cssAppDest));
}); 

//Add the watch task to compile typescript to javascript when the file is modified
gulp.task("watch", ["watch-js"]); 

gulp.task("watch-js", /* run the "compileTs" task at the beginning */
    ["compileTs", "compile-less"],
    function() {
        gulp.watch([config.tsAppSrc], ["compileTs"]); // run the "compileTs" task when a file is modified
        return gulp.watch([config.lessAppSrc], ["compile-less"]); // run the "compileTs" task when a file is modified
    });
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
gulp.task('compVendor', function () {
  return gulp.src('./WebComponents/dist/vendor.bundle.js', { base: './' })
    .pipe(concat('./WebComponents/dist/vendor.bundle.js'))
    .pipe(uglify())
    .pipe(gulp.dest('./'));
});

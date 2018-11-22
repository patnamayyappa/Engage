module.exports = function () {

    var root = './';
    var globalCssPath = root + 'build/css';
    var config = {
        cssVendorDest: "dist/lib/css",
        jsVendorDest: "dist/lib",
        jsAppDest: "dist/app",
        tsAppSrc: "src/app/**/*.ts",
        lessAppSrc: "src/app/**/*.less",
        cssAppDest: "dist/app",
        distTypeScript: './dist/', //ditribution folder path for typescript
    };

    return config;
}
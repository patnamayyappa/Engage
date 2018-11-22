const ExtractTextPlugin = require('extract-text-webpack-plugin');
const path = require('path');
const webpack = require('webpack');
const log = require('npmlog');
const app = require('./package.json');
const src = path.resolve(__dirname, './src');

const root = __dirname;

const BUILD_DIR = './build';
const NODE_ENV = process.env.NODE_ENV ? process.env.NODE_ENV : 'development';
const OPTIMIZE = process.env.OPTIMIZE ? JSON.parse(process.env.OPTIMIZE) : NODE_ENV === 'production';
const ASSETS_LIMIT = typeof process.env.ASSETS_LIMIT !== 'undefined' ? parseInt(process.env.ASSETS_LIMIT, 10) : 5000;

let plugins = [];

log.info('webpack', `running ${NODE_ENV.toUpperCase()} build`);

if (OPTIMIZE) {
    plugins = plugins.concat([
        new webpack.optimize.ModuleConcatenationPlugin(),
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: true
            },
            comments: false
        }),
        new webpack.BannerPlugin([
            `    ${app.name} by ${app.author}`,
            `    Version: ${app.version}`,
            `    Date: ${new Date().toISOString()}`
        ].join('\n'))
    ]);
}

module.exports = {
    context: path.resolve(root + '/src'),
    entry: ['./index.js'],
    devtool: OPTIMIZE ? false : 'source-map',
    output: {
        path: path.resolve(`${root}/${BUILD_DIR}`),
        sourceMapFilename: 'map.[file]',
        filename: 'studentsSuccessNetworkMain.js'
    },
    module: {
        // rules: [
        //     {
        //         enforce: 'pre',
        //         test: /\.(js|jsx)$/,
        //         exclude: /node_modules/,
        //         loader: 'eslint-loader'
        //     }
        // ],
        loaders: [
            {
                test: /\.(js|jsx|es6)$/,
                exclude: /node_modules/,
                loader: 'babel-loader',
                query: {
                    presets: ['es2015', 'react', 'stage-0'],
                    plugins: ['transform-decorators-legacy']
                }
            },
            {
                test: /\.html$/,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]'
                    }
                }, {
                    loader: 'extract-loader',
                    options: {
                        publicPath: './'
                    }
                }, {
                    loader: 'html-loader',
                    options: {
                        attrs: ['img:src', 'link:href']
                    }
                }]
            },
            {
                test: /\.json$/,
                loader: 'json-loader'
            },
            {
                test: /\.css$/,
                use: [
                    { loader: 'file-loader', options: { context: src, name: '[path][name].[ext]' } },
                    {loader: 'extract-loader',options: {publicPath: './'}},
                    'css-loader'
                ]
            },
            {
                test: /\.(ttf|eot|woff|woff2)$/,
                loader: `url-loader?limit=${ASSETS_LIMIT}&name=fonts/[name].[ext].css`
            },
            {
                test: /\.svg/,
                use: {
                    loader: 'svg-url-loader',
                    options: {}
                }
            },
            {
                test: /\.(png|jpg)$/,
                loader: `url-loader?limit=${ASSETS_LIMIT}&name=assets/[hash].[ext]`
            }
        ]
    },

    plugins: [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.DefinePlugin({
            'process.env.NODE_ENV': JSON.stringify(NODE_ENV)
        }),
        ...plugins,
        new webpack.EnvironmentPlugin([
            'NODE_ENV'
        ]),
        new ExtractTextPlugin('[name].css'),
        new webpack.LoaderOptionsPlugin({
            options: {
                customInterpolateName: (loaderContext) => {
                    return loaderContext.replace(/-/g, '');
                }
            }
        })
    ]
};
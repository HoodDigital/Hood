const devMode = process.env.NODE_ENV !== 'production';

const webpack = require('webpack');
const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin');

const plugins = [];
    // enable in production only
    plugins.push(new MiniCssExtractPlugin({
        filename: "[name].css",
        chunkFilename: "[id].css"
    }));

module.exports = {

    plugins,

    entry: {
        hood: './src/ts/hood.ts',
        admin: ['./src/ts/admin.ts', './src/scss/admin.scss']
    },

    mode: devMode ? "development" : "production",

    devtool: devMode ? "source-map" : false,

    module: {
        rules: [
            
            {
                test: /\.tsx?$/,
                use: [
                    {
                        loader: 'ts-loader'
                    }
                ],
                exclude: [/node_modules/, /wwwroot/],
            },
            {
                test: /\.(sa|sc|c)ss$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader'
                    },
                    {
                        loader: 'postcss-loader'
                    },
                    {
                        loader: 'sass-loader'
                    }
                ]
            }
        ],
    },

    resolve: {
        extensions: ['.tsx', '.ts', '.js', '.scss'],
    },

    externals: {
        jquery: 'jQuery',
        bootstrap: 'bootstrap',
        dropzone: 'Dropzone',
        'tinymce/tinymce': 'tinymce',
    },

    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, devMode ? "wwwroot/src" :'wwwroot/dist')
    },

    optimization: {
        minimize: true,
        minimizer: [
            new TerserPlugin()
        ]
    },

};
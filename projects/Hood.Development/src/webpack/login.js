const webpack = require('webpack');
const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const NodePolyfillPlugin = require("node-polyfill-webpack-plugin")

const plugins = [];
plugins.push(new MiniCssExtractPlugin({
    filename: "css/[name].css"
}));

console.log('---------------------------------------------------------------------------')
console.log('-                                                                         -')
console.log('-                      Building Hood CMS - LOGIN                          -')
console.log('-                             DEVELOPMENT                                 -')
console.log('-                                                                         -')
console.log('---------------------------------------------------------------------------')

module.exports = {

    plugins,

    entry: {
        login: ['./src/scss/login.scss']
    },

    mode: "development",

    devtool: "source-map",

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
        bootstrap: 'bootstrap'
    },

    output: {
        filename: 'js/[name].js',
        path: path.resolve("./wwwroot/src")
    },

    optimization: {
        minimize: true,
        minimizer: [
            new TerserPlugin()
        ]
    },

};
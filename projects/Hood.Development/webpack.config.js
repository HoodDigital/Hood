const webpack = require('webpack');
const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin');

module.exports = {

    entry: {
        hood: './src/ts/hood.ts',
        admin: ['./src/ts/admin.ts', './src/scss/admin.scss']
    },

    mode: 'production',

    module: {
        rules: [
            
            {
                test: /\.tsx?$/,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            name: 'js/[name].js',
                        }
                    },
                    {
                        loader: 'ts-loader'
                    }
                ],
                exclude: [/node_modules/, /wwwroot/],
            },
            {
                test: /\.ss?css$/,
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
        path: path.resolve(__dirname, 'wwwroot/dist')
    },

    optimization: {
        minimize: true,
        minimizer: [
            new TerserPlugin()
        ]
    },

    devtool: "source-map",

    plugins: [
        new MiniCssExtractPlugin({
            filename: "[name].css",
            chunkFilename: "[id].css"
        })
    ]
};
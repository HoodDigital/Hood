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
                test: /\.scss$/,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            name: 'css/[name].blocks.css',
                        }
                    },
                    {
                        loader: 'extract-loader'
                    },
                    {
                        loader: 'css-loader?-url'
                    },
                    {
                        loader: 'postcss-loader'
                    },
                    {
                        loader: 'sass-loader',
                        options: {
                            // Prefer `dart-sass`
                            sourceMap: true,
                            implementation: require("sass"),
                            sassOptions: {
                                fiber: false,
                            }
                        }
                    }
                ]
            }
        ],
    },

    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
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
            new TerserPlugin(),
            new OptimizeCSSAssetsPlugin()
        ]
    },
};
const path = require('path');

module.exports = {
    entry: {
        hood: './src/hood.ts',
        admin: './src/admin.ts'
    },
    module: {
        rules: [{
            test: /\.tsx?$/,
            use: 'ts-loader',
            exclude: [/node_modules/, /wwwroot/],
        }, ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    externals: {
        jquery: 'jQuery',
        bootstrap: 'bootstrap',
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
        ],
        runtimeChunk: 'single',
        splitChunks: {
            cacheGroups: {
                vendor: {
                    test: /[\\\/]node_modules[\\\/]/,
                    name: 'vendors',
                    chunks: 'all'
                },
                styles: {
                    name: 'styles',
                    test: /\.css$/,
                    chunks: 'all',
                    enforce: true
                }
            }
        }
    }
};
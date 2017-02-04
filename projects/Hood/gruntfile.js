var semver = require('semver');
module.exports = function (grunt) {
    grunt.initConfig({
        bumpup: {
            options: {
                dateformat: 'YYYY-MM-DD HH:mm',
                normalize: false
            },
            setters: {
                // Version setter that overrides the standard semver 2.0 functionality to do nuget compatible semver 1.0 versioning.
                // OVERRIDES THE prepatch releaseType
                version: function (old, releaseType, options) {
                    if (releaseType == 'prepatch') {
                        if (old.includes('-preview')) {
                            pre = old.split('-preview')[1];
                            old = old.split('-preview')[0];
                            return old + '-preview' + (Number(pre) + 1);
                        } else {
                            return old + '-preview1';
                        }
                    }
                    if (releaseType == 'patch') {
                        if (old.includes('-preview')) {
                            pre = old.split('-preview')[1];
                            old = old.split('-preview')[0];
                            if (pre.split('-').length > 0) {
                                preMain = pre.split('-')[0];
                                prePatch = pre.split('-')[1];
                                return old + '-preview' + preMain + "-" + (Number(prePatch) + 1);
                            } else {
                                return old + '-preview' + pre + "-1";
                            }
                        } else {
                            return old + '-preview1-1';
                        }
                    }
                    return semver.inc(old, releaseType);
                }
            },
            files: ['project.json', 'pacakge.json', '../Hood.Tests/project.json', '../Hood.Web.MVC/project.json', '../../bower.json'],
        },
        bump: {
            options: {
                files: ['package.json'],
                updateConfigs: [],
                add: true,
                addFiles: ['.'], // '.' for all files except ingored files in .gitignore 
                commit: true,
                commitMessage: 'Published tag v%VERSION%',
                commitFiles: ['package.json'], // '-a' for all files 
                createTag: true,
                tagName: 'v%VERSION%',
                tagMessage: 'Published tag v%VERSION%',
                push: true,
                pushTo: 'origin',
                npm: false,
                npmTag: 'Published tag v%VERSION%',
                gitDescribeOptions: '--tags --always --abbrev=1 --dirty=-d' // options to use with '$ git describe' 
            }
        }
    });

    grunt.loadNpmTasks('grunt-bumpup');
    grunt.loadNpmTasks('grunt-push-release');
    grunt.registerTask("major", ['bumpup:major']);
    grunt.registerTask("minor", ['bumpup:minor']);
    grunt.registerTask("prerelease", ['bumpup:prepatch']);
    grunt.registerTask("prerelease-tag", ['bumpup:prepatch', 'push-commit']);
    grunt.registerTask("patch", ['bumpup:patch']);
    grunt.registerTask("patch-tag", ['bumpup:patch', 'push-commit']);
};

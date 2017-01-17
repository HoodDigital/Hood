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
                    return semver.inc(old, releaseType);
                }
            },
            files: ['project.json', '../Hood.Tests/project.json', '../Hood.Web.MVC/project.json', '../../bower.json'],
        }
    });

    grunt.loadNpmTasks('grunt-bumpup');
    grunt.registerTask("major", ['bumpup:major']);
    grunt.registerTask("minor", ['bumpup:minor']);
    grunt.registerTask("patch", ['bumpup:patch']);
    grunt.registerTask("prerelease", ['bumpup:prepatch']);
};

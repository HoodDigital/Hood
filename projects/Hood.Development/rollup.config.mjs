// Hood.Development's own build, expressed through the published preset (dogfood).
// The self-referencing 'hoodcms/build' import exercises the package exports map exactly
// as a consumer would.
import { hoodRollup } from 'hoodcms/build';

export default hoodRollup({
    entries: {
        // app historically ships with the reduced externals list (the extra UI libs are
        // bundled where used); the others take Hood's full base list.
        'app': {
            input: 'src/ts/app.ts',
            externalsOverride: ['jQuery', 'bootstrap', 'sweetalert2', 'dropzone'],
            useHoodGlobals: true
        },
        'app.property': { input: 'src/ts/app.property.ts', useHoodGlobals: true },
        'admin': { input: 'src/ts/admin.ts', useHoodGlobals: true },
        'login': { input: 'src/ts/login.ts', footer: '' }
    }
});

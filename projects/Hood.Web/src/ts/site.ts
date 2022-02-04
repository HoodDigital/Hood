import { Alerts, HoodApi } from "hoodcms";

export class Site extends HoodApi {

    constructor() {
        super();
        this.initialise();
    }

    initialise() {
        super.initialise();
        Alerts.success('Hood CMS is loaded! Customise your TS code in /src/ts/site.ts!');
    }
}

new Site();
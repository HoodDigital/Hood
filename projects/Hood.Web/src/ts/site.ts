import { Alerts, BaseSite } from "hoodcms";

export class Site extends BaseSite {

    constructor() {
        super();
    }

    initialise() {
        super.initialise();
    }

    override onLoad() {
        super.onLoad();

        Alerts.success('Hood CMS is loaded! Customise your TS code in /src/ts/site.ts!');
    }

    override onResize() {
        super.onResize();
    }
}

new Site();
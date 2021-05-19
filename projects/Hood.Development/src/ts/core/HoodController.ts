import { Alerts } from "./Alerts";
import { Editors } from "./Editors";
import { Handlers } from "./Handlers";
import { Uploader } from "./Uploader";

export class HoodController {
    editors: Editors;
    uploader: Uploader;
    handlers: Handlers;

    constructor() {

        // Global Services
        this.uploader = new Uploader();
        this.handlers = new Handlers();

        // Global Handlers
        this.setupLoaders();

    }

    setupLoaders(): void {
        $('body').on('loader-show', function () { Alerts.success("Showing loader..."); })
        $('body').on('loader-hide', function () { Alerts.error("Hiding loader..."); })
    }
}
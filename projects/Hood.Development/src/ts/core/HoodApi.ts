import { Alerts } from "./Alerts";
import { ColorPickers } from "./ColorPicker";
import { Editors } from "./Editors";
import { Handlers } from "./Handlers";
import { MediaModal } from "./Media";
import { Uploader } from "./Uploader";

declare global {
    interface Hood {
        alerts: Alerts;
        uploader: Uploader;
        handlers: Handlers;
    }

    interface Window {
        hood: Hood;
    }
}

export class HoodApi implements Hood {
    alerts: Alerts = new Alerts();
    uploader: Uploader = new Uploader();
    handlers: Handlers = new Handlers();
    editors: Editors = new Editors();
    colorPickers: ColorPickers = new ColorPickers();

    private mediaModal: MediaModal = new MediaModal();

    constructor() {
        // Global Handlers
        this.setupLoaders();
    }

    setupLoaders(): void {
        $('body').on('loader-show', function () { Alerts.success("Showing loader..."); })
        $('body').on('loader-hide', function () { Alerts.error("Hiding loader..."); })
    }
}

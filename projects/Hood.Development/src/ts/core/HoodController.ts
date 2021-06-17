import { Alerts } from "./Alerts";
import { Handlers } from "./Handlers";
import { MediaModal } from "./Media";
import { Uploader } from "./Uploader";

export class BaseController {
    uploader: Uploader;
    handlers: Handlers;
    mediaModal: MediaModal;

    constructor() {

        // Global Services
        this.uploader = new Uploader();
        this.handlers = new Handlers();

        // Global Handlers
        this.setupLoaders();

        // Media Modal Service
        this.mediaModal = new MediaModal();
    }

    setupLoaders(): void {
        $('body').on('loader-show', function () { Alerts.success("Showing loader..."); })
        $('body').on('loader-hide', function () { Alerts.error("Hiding loader..."); })
    }
}
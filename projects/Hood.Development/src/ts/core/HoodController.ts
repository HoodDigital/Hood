import { Alerts, Editors, Handlers, MediaModal, Uploader } from "../hood";

export class BaseController {
    editors: Editors;
    uploader: Uploader;
    handlers: Handlers;
    mediaModal: MediaModal;

    constructor() {

        // Global Services
        this.editors = new Editors();
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
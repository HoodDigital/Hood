import { MediaObject, MediaService } from "../core/Media";
import { Alerts } from "../core/Alerts";

export class MediaController {

    constructor() {
        this.manage();
    }

    list: HTMLElement;
    service: MediaService;

    manage(): void {
        this.list = document.getElementById('media-list');
        this.service = new MediaService(this.list, {
            action: 'show',
            onAction: function (this: MediaController, mediaObject: MediaObject) {
                Alerts.log(`Showing media object id ${mediaObject.id} - ${mediaObject.filename}`);
            }.bind(this),
            onListLoad: function () {
                Alerts.log('Commencing media list fetch.');
            },
            onError: function (jqXHR: any, textStatus: any, errorThrown: any) {
                Alerts.log(`Error loading media list: ${textStatus}`);
            },
            onListRender: function (data: string) {
                Alerts.log('Fetched media list data.');
                return data;
            },
            onListComplete: function () {
                Alerts.log('Finished loading media list...', 'info');
            }
        });
    }
}

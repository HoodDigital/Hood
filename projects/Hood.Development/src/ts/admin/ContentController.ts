import { KeyValue } from "../interfaces/KeyValue";
import { ContentStatistics } from "./models/Content";
import { PropertyStatistics } from "./models/Property";
import { UserStatistics } from "./models/Users";
import { Chart } from 'chart.js';
import { Alerts, DataList, MediaObject, MediaService } from "../hood";

export class ContentController {

    constructor() {
        this.manage();
    }

    /**
    * Content list element, #content-list
    */
    element: HTMLElement;

    /**
    * Content DataList object.
    */
    list: DataList;


    /**
    * Content list element, #content-list
    */
    categoryElement: HTMLElement;

    /**
    * Content DataList object.
    */
    categoryList: DataList;

    manage(): void {
        this.element = document.getElementById('content-list');
        this.list = new DataList(this.element, {
            onComplete: function (this: ContentController, sender: HTMLElement, data: string) {

                Alerts.log('Finished loading content list.', 'info');

            }.bind(this)
        });

        this.categoryElement = document.getElementById('content-categories-list');
        this.categoryList = new DataList(this.categoryElement, {
            onComplete: function (this: ContentController, sender: HTMLElement, data: string) {

                Alerts.log('Finished loading category list.', 'info');

            }.bind(this)
        });

    }
}

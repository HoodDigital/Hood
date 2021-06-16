import { Alerts } from "../core/Alerts";
import { DataList } from "../core/DataList";

export class LogsController {
    element: HTMLElement;
    list: DataList;

    constructor() {
        this.element = document.getElementById('log-list');
        if (this.element) {
            this.list = new DataList(this.element, {
                onComplete: function (this: LogsController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading logs list.', 'info');

                }.bind(this)
            });
        }
    }
}
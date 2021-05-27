import { SweetAlertResult } from "sweetalert2";
import { Alerts, DataList, Inline, Response } from "../hood";

export class ThemesController {
    element: HTMLElement;
    list: DataList;

    constructor() {
        $('body').on('click', '.activate-theme', this.activate.bind(this));

        this.element = document.getElementById('themes-list');
        if (this.element) {
            this.list = new DataList(this.element, {
                onComplete: function (this: ThemesController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading users list.', 'info');

                }.bind(this)
            });
        }
    }

    activate(this: ThemesController, e: JQuery.ClickEvent): void {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The site will change themes, and the selected theme will be live right away."
        }, function (this: ThemesController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ThemesController, data: Response) {

                    Response.process(data, 5000);

                    setTimeout(function (this: ThemesController) {

                        if (this.list) {
                            this.list.reload();
                        }

                    }.bind(this), 2000);

                }.bind(this));
            }
        }.bind(this))

    }
}
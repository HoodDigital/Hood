import { Alerts, DataList, Inline, MediaService, ModalController, Validator } from "../hood";
import { SweetAlertResult } from "sweetalert2";
import { Response } from "../core/Response";

export class PropertyController {

    constructor() {
        this.manage();

        $('body').on('click', '.property-create', this.create.bind(this));
        $('body').on('click', '.property-delete', this.delete.bind(this));
        $('body').on('click', '.property-set-status', this.setStatus.bind(this));

        $('body').on('keyup', '#Slug', function (this: HTMLElement) {
            let slugValue: string = $(this).val() as string;
            $('.slug-display').html(slugValue);
        });

    }

    /**
    * Property list element, #property-list
    */
    element: HTMLElement;

    /**
    * Property DataList object.
    */
    list: DataList;

    manage(this: PropertyController): void {

        this.element = document.getElementById('property-list');
        if (this.element) {
            this.list = new DataList(this.element, {
                onComplete: function (this: PropertyController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading property list.', 'info');

                }.bind(this)
            });
        }

    }

    create(this: PropertyController, e: JQuery.ClickEvent) {

        e.preventDefault();
        e.stopPropagation();
        let createPropertyModal: ModalController = new ModalController({
            onComplete: function (this: PropertyController) {
                let form = document.getElementById('property-create-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: PropertyController, sender: Validator, response: Response) {

                        Response.process(response, 5000);

                        if (this.list) {
                            this.list.Reload();
                        }

                        if (response.success) {
                            createPropertyModal.close();
                        } 

                    }.bind(this)
                });
            }.bind(this)
        });
        createPropertyModal.show($(e.currentTarget).attr('href'), this.element);
    }

    delete(this: PropertyController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The property will be permanently removed."
        }, function (this: PropertyController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: PropertyController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.Reload();
                    }

                    if (e.currentTarget.dataset.redirect) {

                        Alerts.message('Just taking you back to the property list.', 'Redirecting...');
                        setTimeout(function () {
                            window.location = e.currentTarget.dataset.redirect;
                        }, 1500);

                    }

                }.bind(this));
            }
        }.bind(this))

    }

    setStatus(this: PropertyController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The change will happen immediately."
        }, function (this: PropertyController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: PropertyController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.Reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }

}

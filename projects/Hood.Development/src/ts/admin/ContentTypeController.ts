import { Alerts, DataList, Inline, ModalController, Validator } from "../hood";
import { SweetAlertResult } from "sweetalert2";
import { Response } from "../core/Response";

export class ContentTypeController {

    constructor() {
        this.manage();

        $('body').on('click', '.content-type-create', this.create.bind(this));
        $('body').on('click', '.content-type-delete', this.delete.bind(this));
        $('body').on('click', '.content-type-set-status', this.setStatus.bind(this));

        $('body').on('click', '.content-custom-field-create', this.createField.bind(this));
        $('body').on('click', '.content-custom-field-delete', this.deleteField.bind(this));

        $('body').on('keyup', '#Slug', function (this: HTMLElement) {
            let slugValue: string = $(this).val() as string;
            $('.slug-display').html(slugValue);
        });

    }

    element: HTMLElement;
    list: DataList;

    fieldsElement: HTMLElement;
    fieldsList: DataList;

    manage(this: ContentTypeController): void {

        this.element = document.getElementById('content-type-list');
        if (this.element) {
            this.list = new DataList(this.element, {
                onComplete: function (this: ContentTypeController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading content type list.', 'info');

                }.bind(this)
            });
        }

        this.fieldsElement = document.getElementById('content-custom-field-list');
        if (this.fieldsElement) {
            this.fieldsList = new DataList(this.fieldsElement, {
                onComplete: function (this: ContentTypeController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading content type list.', 'info');

                }.bind(this)
            });
        }



    }

    create(this: ContentTypeController, e: JQuery.ClickEvent) {

        e.preventDefault();
        e.stopPropagation();
        let createContentModal: ModalController = new ModalController({
            onComplete: function (this: ContentTypeController) {
                let form = document.getElementById('content-type-create-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: ContentTypeController, sender: Validator, response: Response) {

                        Response.process(response, 5000);

                        if (this.list) {
                            this.list.reload();
                        }

                        if (response.success) {
                            createContentModal.close();
                        } 

                    }.bind(this)
                });
            }.bind(this)
        });
        createContentModal.show($(e.currentTarget).attr('href'), this.element);
    }

    delete(this: ContentTypeController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The content type will be permanently removed. Content will remain, but will be unusable and marked as the old content type."
        }, function (this: ContentTypeController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentTypeController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.reload();
                    }

                    if (e.currentTarget.dataset.redirect) {

                        Alerts.message('Just taking you back to the content list.', 'Redirecting...');
                        setTimeout(function () {
                            window.location = e.currentTarget.dataset.redirect;
                        }, 1500);

                    }

                }.bind(this));
            }
        }.bind(this))

    }

    setStatus(this: ContentTypeController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The change will happen immediately."
        }, function (this: ContentTypeController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentTypeController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }


    createField(this: ContentTypeController, e: JQuery.ClickEvent) {

        e.preventDefault();
        e.stopPropagation();
        let createContentModal: ModalController = new ModalController({
            onComplete: function (this: ContentTypeController) {
                let form = document.getElementById('content-custom-field-create-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: ContentTypeController, sender: Validator, response: Response) {

                        Response.process(response, 5000);

                        if (this.fieldsList) {
                            this.fieldsList.reload();
                        }

                        if (response.success) {
                            createContentModal.close();
                        }

                    }.bind(this)
                });
            }.bind(this)
        });
        createContentModal.show($(e.currentTarget).attr('href'), this.element);
    }

    deleteField(this: ContentTypeController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The field will be permanently removed. However fields will still be attached to content."
        }, function (this: ContentTypeController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentTypeController, data: Response) {

                    Response.process(data, 5000);

                    if (this.fieldsList) {
                        this.fieldsList.reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }


}

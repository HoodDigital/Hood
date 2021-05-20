import { Alerts, DataList, Inline, MediaService, ModalController, Validator } from "../hood";
import { SweetAlertResult } from "sweetalert2";
import { Response } from "../core/Response";

export class ContentController {

    constructor() {
        this.manage();

        $('body').on('click', '.content-categories-delete', this.deleteCategory.bind(this));
        $('body').on('click', '.content-category-create', this.createCategory.bind(this));

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

    manage(this: ContentController): void {
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

    createCategory(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        let createCategoryModal: ModalController = new ModalController({
            onComplete: function (this: ContentController) {
                let form = document.getElementById('content-directories-edit-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: ContentController, sender: Validator, data: Response) {
                        if (data.success) {
                            this.categoryList.Reload();
                            createCategoryModal.close();
                            Alerts.success(data.message);
                        } else {
                            Alerts.error(data.error);
                        }
                    }.bind(this)
                });
            }.bind(this)
        });
        createCategoryModal.show($(e.target).attr('href'), this.element);
    }

    deleteCategory(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        Alerts.confirm({
            // Confirm options...
        }, function (this: ContentController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e, function (this: ContentController, sender: HTMLElement, data: Response) {

                    // category deleted...
                    Response.process(data);
                    this.categoryList.Reload();

                }.bind(this), 5000);
            }
        }.bind(this))
    }

}

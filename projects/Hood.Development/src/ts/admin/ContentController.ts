import { Alerts, DataList, Inline, MediaService, ModalController, Validator } from "../hood";
import { SweetAlertResult } from "sweetalert2";
import { Response } from "../core/Response";

export class ContentController {

    constructor() {
        this.manage();

        $('body').on('click', '.content-create', this.create.bind(this));
        $('body').on('click', '.content-delete', this.delete.bind(this));
        $('body').on('click', '.content-set-status', this.setStatus.bind(this));

        $('body').on('click', '.content-categories-delete', this.deleteCategory.bind(this));
        $('body').on('click', '.content-categories-create,.content-categories-edit', this.createOrEditCategory.bind(this));
        $('body').on('change', '.content-categories-check', this.toggleCategory.bind(this));

        $('body').on('click', '.content-media-delete', this.removeMedia.bind(this));

        $('body').on('keyup', '#Slug', function (this: HTMLElement) {
            let slugValue: string = $(this).val() as string;
            $('.slug-display').html(slugValue);
        });

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
        if (this.element) {
            this.list = new DataList(this.element, {
                onComplete: function (this: ContentController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading content list.', 'info');

                }.bind(this)
            });
        }

        this.categoryElement = document.getElementById('content-categories-list');
        if (this.categoryElement) {
            this.categoryList = new DataList(this.categoryElement, {
                onComplete: function (this: ContentController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading category list.', 'info');

                }.bind(this)
            });
        }

    }

    create(this: ContentController, e: JQuery.ClickEvent) {

        e.preventDefault();
        e.stopPropagation();
        let createContentModal: ModalController = new ModalController({
            onComplete: function (this: ContentController) {
                let form = document.getElementById('content-create-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: ContentController, sender: Validator, response: Response) {

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

    delete(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The content will be permanently removed."
        }, function (this: ContentController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentController, data: Response) {

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

    setStatus(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The change will happen immediately."
        }, function (this: ContentController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }

    clone(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "This will duplicate the content and everything inside it."
        }, function (this: ContentController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }

    createOrEditCategory(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        let createCategoryModal: ModalController = new ModalController({
            onComplete: function (this: ContentController) {
                let form = document.getElementById('content-categories-edit-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: ContentController, sender: Validator, response: Response) {

                        Response.process(response, 5000);

                        if (this.list) {
                            this.list.reload();
                        }

                        if (response.success) {
                            this.categoryList.reload();
                            createCategoryModal.close();
                        }

                    }.bind(this)
                });
            }.bind(this)
        });
        createCategoryModal.show($(e.currentTarget).attr('href'), this.element);
    }

    deleteCategory(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
        }, function (this: ContentController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentController, data: Response) {

                    // category deleted...
                    Response.process(data, 5000);

                    this.categoryList.reload();

                }.bind(this));
            }
        }.bind(this))
    }

    toggleCategory(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        $.post($(e.currentTarget).data('url'), { categoryId: $(e.currentTarget).val(), add: $(e.currentTarget).is(':checked') }, function (this: ContentController, response: Response) {

            Response.process(response, 5000);

            if (this.categoryList) {
                this.categoryList.reload();
            }

        }.bind(this));
    
    }

    removeMedia(this: ContentController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The media will be removed from the property, but will still be in the media collection."
        }, function (this: ContentController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: ContentController, data: Response) {

                    Response.process(data, 5000);

                    // reload the property media gallery, should be attached to #property-gallery-list
                    let mediaGalleryEl = document.getElementById('content-gallery-list');
                    if (mediaGalleryEl.hoodDataList) {
                        mediaGalleryEl.hoodDataList.reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }

}

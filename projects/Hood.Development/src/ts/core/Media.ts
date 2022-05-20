import { SweetAlertResult } from "sweetalert2";

import { Alerts } from "./Alerts";
import { Response } from "./Response";
import { DataList } from "./DataList";
import { ModalController } from "./Modal";
import { Validator } from "./Validator";
import { Inline } from "./Inline";
import { Editor } from "tinymce";

export declare interface MediaObject {

    id: number;
    fileSize: number;
    fileType: string;
    filename: string;
    blobReference: string;

    createdOn: Date;

    url: string;

    thumbUrl: string;
    smallUrl: string;
    mediumUrl: string;
    largeUrl: string;
    uniqueId: string;

    genericFileType: 'Image' | 'PDF' | 'Word' | 'Photoshop' | 'Excel' | 'PowerPoint' | 'Directory' | 'Audio' | 'Video' | 'Unknown';

    downloadUrl: string;
    downloadUrlHttps: string;

    icon: string;
    formattedSize: string;
    path: string;

}

export interface MediaOptions {

    /**
    * Action for the media service to take when a media item is clicked.
    */
    action: 'insert' | 'attach' | 'select' | string;

    /**
     * Selector for any elements that require a refresh of background-image or src (if applicable). 
     */
    refresh?: string;

    /**
     * Insert/Select Only: Input field to insert image url to. 
     */
    size?: string;

    /**
     * Select/Gallery Only: Input field to insert image url to. 
     */
    target?: string;

    /**
     * Insert Only: Input field to insert image url to. 
     */
    targetEditor?: Editor;

    /**
     * Attach Only: Post url to send the selected media id to when clicked. Must accept mediaId as a parameter.
     */
    url?: string;

    /**
     * Called before the action is carried out.
     */
    beforeAction?: (sender?: HTMLElement) => void;

    /**
     * Called when the action is complete.
     */
    onAction?: (mediaObject: MediaObject, sender?: HTMLElement) => void;

    /**
     * Called before the media list is is fetched.
     */
    onListLoad?: (sender?: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the media list. Must return the data back to datalist to render.
     */
    onListRender?: (html: string, sender?: HTMLElement) => string;

    /**
     * Called when media list loading and rendering is complete.
     */
    onListComplete?: (html: string, sender?: HTMLElement) => void;

    /**
     * Called when an error occurs.
     */
    onError?: (jqXHR: any, textStatus: any, errorThrown: any) => void;

}

export class MediaService {
    media: DataList;
    element: HTMLElement;
    options: MediaOptions = {
        action: 'show',
        size: 'large'
    };
    currentBlade: ModalController;
    progress: JQuery<HTMLElement>;
    progressText: JQuery<HTMLElement>;
    uploadButton: HTMLElement;
    uploader: HTMLElement;
    progressArea: HTMLElement;
    dropzone: any;
    galleryInitialised: boolean = false;

    constructor(element: HTMLElement, options: MediaOptions) {
        this.element = element;
        if (!this.element) {
            return;
        }

        this.options = { ...this.options, ...options };

        $('body').off('click', '.media-delete', this.delete.bind(this));
        $('body').on('click', '.media-delete', this.delete.bind(this));

        $(this.element).on('click', '.media-item', this.action.bind(this));
        $(this.element).on('click', '.media-create-directory', this.createDirectory.bind(this))
        $(this.element).on('click', '.media-delete-directory', this.deleteDirectory.bind(this));

        this.media = new DataList(this.element, {
            onLoad: this.options.onListLoad,
            onError: this.options.onError,
            onRender: this.options.onListRender,
            onComplete: function (this: MediaService, html: string, sender: HTMLElement) {
                this.initUploader();

                // if this is gallery type, add the "Add to gallery button" and hook it to the add function
                if (this.options.action == 'gallery' && !this.galleryInitialised) {
                    $('#media-select-modal .modal-footer').removeClass('d-none');
                    $('#media-select-modal .modal-footer').on('click', this.galleryAdd.bind(this));
                    this.galleryInitialised = true;
                }

                if (this.options.onListComplete) {
                    this.options.onListComplete(html, sender)
                }
            }.bind(this)
        });

    }

    initUploader() {
        this.uploadButton = document.getElementById('media-add');
        this.uploader = document.getElementById('media-upload');
        if (!this.uploadButton || !this.uploader)
            return;

        this.progressArea = document.getElementById('media-progress');
        this.progressText = $('<div class="progress-text text-muted mb-3"><i class="fa fa-info-circle me-2"></i>Uploading file <span></span>...</div>');
        this.progress = $('<div class="progress"><div class= "progress-bar progress-bar-striped" role="progressbar" style="width:0%" aria-valuenow="10" aria-valuemin="0" aria-valuemax="100" ></div></div>');

        this.progressText.appendTo(this.progressArea);
        this.progress.appendTo(this.progressArea);

        var dz: Dropzone = null;
        $("#media-upload").dropzone({
            url: $("#media-upload").data('url') + "?directoryId=" + $("#media-list > #upload-directory-id").val(),
            thumbnailWidth: 80,
            thumbnailHeight: 80,
            parallelUploads: 5,
            paramName: 'files',
            acceptedFiles: $("#media-upload").data('types') || ".png,.jpg,.jpeg,.gif,.pdf",
            autoProcessQueue: true, // Make sure the files aren't queued until manually added
            previewsContainer: false, // Define the container to display the previews
            clickable: "#media-add", // Define the element that should be used as click trigger to select files.
            dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
            dictResponseError: 'Error while uploading file!',
            init: function () {
                dz = this;
            }
        });

        dz.on("error", function (this: MediaService, file: Dropzone.DropzoneFile, errormessage: any) {
            Alerts.warning(errormessage);
        }.bind(this));

        dz.on("success", function (this: MediaService, file: Dropzone.DropzoneFile, data: Response) {
            Response.process(data);
        }.bind(this));

        dz.on("addedfile", function (this: MediaService, file: Dropzone.DropzoneFile) {
            this.progress.find('.progress-bar').css({ width: 0 + "%" });
            this.progressText.find('span').html(0 + "%");
        }.bind(this));

        // Update the total progress bar
        dz.on("totaluploadprogress", function (this: MediaService, totalProgress: number, totalBytes: number, totalBytesSent: number) {
            this.progress.find('.progress-bar').css({ width: totalProgress + "%" });
            this.progressText.find('span').html(Math.round(totalProgress) + "% - " + totalBytesSent.formatKilobytes() + " / " + totalBytes.formatKilobytes());
        }.bind(this));

        dz.on("sending", function (this: MediaService, file: Dropzone.DropzoneFile) {
            // Show the total progress bar when upload starts
            this.progressArea.classList.remove('collapse');
            this.progress.find('.progress-bar').css({ width: 0 + "%" });
            this.progressText.find('span').html(0 + "%");
        }.bind(this));

        // Hide the total progress bar when nothing's uploading anymore
        dz.on("complete", function (this: MediaService, file: Dropzone.DropzoneFile) {
            this.media.reload();
        }.bind(this));

        // Hide the total progress bar when nothing's uploading anymore
        dz.on("queuecomplete", function (this: MediaService) {
            this.progressArea.classList.add('collapse');
            this.media.reload();
        }.bind(this));
    }

    createDirectory(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        let createDirectoryModal: ModalController = new ModalController({
            onComplete: function (this: MediaService) {
                let form = document.getElementById('content-directories-edit-form') as HTMLFormElement;
                new Validator(form, {
                    onComplete: function (this: MediaService, response: Response) {

                        Response.process(response, 5000);

                        if (response.success) {
                            this.media.reload();
                            createDirectoryModal.close();
                        }

                    }.bind(this)
                });
            }.bind(this)
        });
        createDirectoryModal.show($(e.currentTarget).attr('href'), this.element);
    }

    deleteDirectory(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        Alerts.confirm({

        }, function (this: MediaService, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: MediaService, response: Response, sender: HTMLElement) {
                    // Refresh the list, using the parent directory id - stored in the response's data array.
                    Response.process(response, 5000);

                    if (response.data.length > 0) {
                        var listUrl = document.createElement('a');
                        listUrl.href = $(this.element).data('url');
                        listUrl.search = `?dir=${response.data[0]}`;
                        this.media.reload(new URL(listUrl.href));
                    }
                }.bind(this));
            }
        }.bind(this))
    }

    uploadUrl() {
        return $("#media-upload").data('url') + "?directoryId=" + $("#media-list > #upload-directory-id").val();
    }

    action(this: MediaService, e: JQuery.ClickEvent) {

        e.preventDefault();
        e.stopPropagation();

        // Load media object from clicked item in the media list
        let mediaObject: MediaObject = $(e.currentTarget).data('json') as MediaObject;

        // Perform the chosen action, which is set on the service's options when loaded.
        switch (this.options.action) {
            case 'select':
                this.select(mediaObject, e);
                break;
            case 'insert':
                this.insert(mediaObject, e);
                break;
            case 'attach':
                this.attach(mediaObject, e);
                break;
            case 'gallery':
                this.galleryClick(mediaObject, e);
                break;
            default:
                this.show(mediaObject, e);
                break;
        }

    }

    show(this: MediaService, mediaObject: MediaObject, sender: JQuery.ClickEvent) {

        // Load the media as a new blade to display.
        this.currentBlade = new ModalController();
        this.currentBlade.show($(sender.target).data('blade'), sender.target);

        // TODO: On close, reload the list and reinstate the modal??
    }

    select(this: MediaService, mediaObject: MediaObject, e: JQuery.ClickEvent): void {
        Alerts.log(`[MediaService.select] Selecting media object id ${mediaObject.id} - ${mediaObject.filename} and inserting ${this.options.size} url to target: ${this.options.target}`);
        if (this.options.target) {
            let target = $(this.options.target);

            switch (this.options.size) {
                case 'thumb':
                    target.val(mediaObject.thumbUrl);
                    break;
                case 'small':
                    target.val(mediaObject.smallUrl);
                    break;
                case 'medium':
                    target.val(mediaObject.mediumUrl);
                    break;
                case 'large':
                    target.val(mediaObject.largeUrl);
                    break;
                case 'full':
                    target.val(mediaObject.url);
                    break;
            }
        }
        if (this.options.refresh) {
            MediaService.refresh(mediaObject, this.options.refresh);
        }

        // Run the callback for onAction
        if (this.options.onAction) {
            this.options.onAction(mediaObject);
        }
    }

    insert(this: MediaService, mediaObject: MediaObject, e: JQuery.ClickEvent): void {
        // basic functionality to insert the correct string from the media response (from uploader) to given input element. 
        Alerts.log(`[MediaService.insert] Selecting media object id ${mediaObject.id} - ${mediaObject.filename} and inserting ${this.options.size} image to target editor: ${this.options.target}`);
        this.options.targetEditor.insertContent('<img alt="' + mediaObject.filename + '" src="' + mediaObject.url + '" class="img-fluid" />');

        // Run the callback for onAction
        if (this.options.onAction) {
            this.options.onAction(mediaObject);
        }
   }

    attach(this: MediaService, mediaObject: MediaObject, e: JQuery.ClickEvent): void {
        // once file is uploaded to given directory, send media id to the given attach endpoint.
        Alerts.log(`[MediaService.attach] Attaching media object id ${mediaObject.id} - ${mediaObject.filename} to url: ${this.options.url}`);
        $.post(this.options.url, { mediaId: mediaObject.id }, function (this: MediaService, response: Response) {
            Response.process(response, 5000);
            MediaService.refresh(response.media, this.options.refresh);
            // Run the callback for onAction
            if (this.options.onAction) {
                this.options.onAction(mediaObject);
            }
        }.bind(this));
    }

    selectedMedia: MediaObject[] = new Array();

    galleryClick(this: MediaService, mediaObject: MediaObject, e: JQuery.ClickEvent): void {
        // once file is uploaded to given directory, send media id to the given attach endpoint.
        if (!this.isMediaSelected(mediaObject)) {
            Alerts.log(`[MediaService.galleryClick] Adding to selected media objects - id ${mediaObject.id} - ${mediaObject.filename}.`);
           this.selectedMedia.push(mediaObject);
            $(e.currentTarget).parents('.media-item').addClass('active');
        } else {
            Alerts.log(`[MediaService.galleryClick] Removing media from selection - id ${mediaObject.id} - ${mediaObject.filename}.}`);
            this.selectedMedia = this.selectedMedia.filter(function (obj: MediaObject) {
                return obj.id !== mediaObject.id;
            });
            $(e.currentTarget).parents('.media-item').removeClass('active');
        }
    }

    galleryAdd(this: MediaService, e: JQuery.ClickEvent): void {

        e.preventDefault();
        e.stopPropagation();

        // once file is uploaded to given directory, send media id to the given attach endpoint.
        Alerts.log(`[MediaService.galleryAdd] Adding ${this.selectedMedia.length} selected media objects  to url: ${this.options.url}`);

        let mediaIds = this.selectedMedia.map(function (v: MediaObject) {
            return v.id;
        });

        // create the url to send to (add media id's to it as query params)
        $.post(this.options.url, { media: mediaIds }, function (this: MediaService, data: Response) {

            Response.process(data);

            // refresh the gallery - - 
            let galleryEl = document.getElementById(this.options.target);
            let gallery: DataList = galleryEl.hoodDataList as DataList;
            gallery.reload();

            // Run the callback for onAction
            if (this.options.onAction) {
                this.options.onAction(data.media);
            }

        }.bind(this));

    }

    isMediaSelected(mediaObject: MediaObject): boolean {
        let added: boolean = false;
        this.selectedMedia.forEach(function (value: MediaObject, index: number, array: MediaObject[]) {
            if (value.id == mediaObject.id) {
                added = true;
            }
        });
        return added;
    }

    static refresh(media: MediaObject, refresh: string): void {

        let icon = media.icon;

        if (media.genericFileType === "Image") {
            icon = media.mediumUrl;
        }

        if (refresh) {
            let $image = $(refresh);
            $image.css({
                'background-image': 'url(' + icon + ')'
            });
            $image.find('img').attr('src', icon);
            $image.removeClass('loading');
        }

    }

    delete(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        Alerts.confirm({
            html: 'This file will be permanently deleted, are you sure?'
        }, function (this: MediaService, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: MediaService, response: Response) {

                    Response.process(response, 5000);
                    this.media.reload();
                    if (this.currentBlade) {
                        this.currentBlade.close();
                    }

                }.bind(this));
            }
        }.bind(this))
    }

}

export class MediaModal {
    modal: ModalController;
    list: HTMLElement;
    service: MediaService;
    element: HTMLElement;

    constructor() {
        $('body').on('click', '[data-hood-media=attach],[data-hood-media=select],[data-hood-media=gallery]', this.load.bind(this));
        $('body').on('click', '[data-hood-media=clear]', this.clear.bind(this));
        $('[data-hood-media=gallery]').each(this.initGallery.bind(this));
    }

    initGallery(this: MediaModal, index: number, element: HTMLElement): void {

        // setup the gallery list also, just a simple list jobby, and attach it to the 
        let el = document.getElementById(element.dataset.hoodMediaTarget);
        if (el) {
            new DataList(el, {
                onComplete: function (this: MediaModal, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading gallery media list.', 'info');

                }.bind(this)
            });
        }
    }

    load(this: MediaModal, e: JQuery.ClickEvent): void {
        e.preventDefault();
        e.stopPropagation();

        this.element = e.currentTarget;

        if (this.modal && this.modal.isOpen) {
            return;
        }

        this.modal = new ModalController({

            onComplete: function (this: MediaModal, sender: HTMLElement) {

                this.list = document.getElementById('media-list');
                this.service = new MediaService(this.list, {
                    action: this.element.dataset.hoodMedia,
                    url: this.element.dataset.hoodMediaUrl,
                    refresh: this.element.dataset.hoodMediaRefresh,
                    target: this.element.dataset.hoodMediaTarget,
                    size: this.element.dataset.hoodMediaSize,
                    beforeAction: function (this: MediaModal, sender: HTMLElement, mediaObject: MediaObject) {
                    }.bind(this),
                    onAction: function (this: MediaModal, sender: HTMLElement, mediaObject: MediaObject) {

                        this.modal.close();

                    }.bind(this),
                    onListLoad: (sender: HTMLElement) => {
                    },
                    onListRender: (data: string) => {
                        return data;
                    },
                    onListComplete: (data: string) => {
                    },
                    onError: (jqXHR: any, textStatus: any, errorThrown: any) => {
                    },
                });

            }.bind(this)

        });
        this.modal.show($(e.currentTarget).data('hood-media-list'), e.currentTarget);
    }

    clear(this: MediaModal, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({

        }, function (this: MediaModal, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: MediaModal, response: Response) {

                    Response.process(response, 5000);
                    MediaService.refresh(response.media, e.currentTarget.dataset.hoodMediaRefresh);

                }.bind(this));
            }
        }.bind(this))
    }
}

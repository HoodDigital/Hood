import { Alerts } from "./Alerts";
import { Response } from "./Response";
import { DataList, DataListOptions } from "./DataList";
import { ModalController } from "./Modal";
import { Validator } from "./Validator";
import { Modal } from "bootstrap";
import { Inline } from "./Inline";
import * as Dropzone from "dropzone";
import { SweetAlertResult } from "sweetalert2";

const dz = Dropzone
dz.autoDiscover = false;

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
     * Insert/Select Only: Input field to insert image url to. 
     */
    target?: HTMLElement;

    /**
     * Attach Only: Post url to send the selected media id to when clicked. Must accept mediaId as a parameter.
     */
    url?: string;

    /**
     * Attach Only: JSON input, add this to
     */
    json?: string;

    /**
     * Called before the action is carried out.
     */
    beforeAction?: (sender: HTMLElement) => void;

    /**
     * Called when the action is complete.
     */
    onAction?: (sender: HTMLElement, mediaObject: MediaObject) => void;

    /**
     * Called before the media list is is fetched.
     */
    onListLoad?: (sender: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the media list. Must return the data back to datalist to render.
     */
    onListRender?: (sender: HTMLElement, html: string) => string;

    /**
     * Called when media list loading and rendering is complete.
     */
    onListComplete?: (sender: HTMLElement, html: string) => void;

    /**
     * Called when an error occurs.
     */
    onError?: (jqXHR: any, textStatus: any, errorThrown: any) => void;

}

export class MediaService {
    media: DataList;
    element: HTMLElement;
    options: MediaOptions = {
        action: 'show'
    };
    currentBlade: ModalController;
    progress: JQuery<HTMLElement>;
    progressText: JQuery<HTMLElement>;
    uploadButton: HTMLElement;
    uploader: HTMLElement;
    progressArea: HTMLElement;
    dropzone: Dropzone;

    constructor(element: HTMLElement, options: MediaOptions) {
        this.element = element;
        if (!this.element) {
            return;
        }

        this.options = { ...this.options, ...options };

        $('body').off('click', '.media-delete', this.delete.bind(this));
        $('body').on('click', '.media-delete', this.delete.bind(this));

        $('body').off('click', '[data-hood-media=clear]', this.clear.bind(this));
        $('body').on('click', '[data-hood-media=clear]', this.clear.bind(this));

        $(this.element).on('click', '.media-item', this.action.bind(this));
        $(this.element).on('click', '.media-create-directory', this.createDirectory.bind(this))
        $(this.element).on('click', '.media-delete-directory', this.deleteDirectory.bind(this));

        this.media = new DataList(this.element, {
            onLoad: this.options.onListLoad,
            onError: this.options.onError,
            onRender: this.options.onListRender,
            onComplete: function (this: MediaService, sender: HTMLElement, html: string) {
                this.initUploader();
                if (this.options.onListComplete) {
                    this.options.onListComplete(sender, html)
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

        this.dropzone = new Dropzone("#media-upload", {
            url: $("#media-upload").data('url') + "?directoryId=" + $("#media-list > #upload-directory-id").val(),
            thumbnailWidth: 80,
            thumbnailHeight: 80,
            parallelUploads: 5,
            paramName: 'files',
            acceptedFiles: $("#media-upload").data('types') || ".png,.jpg,.jpeg,.gif",
            autoProcessQueue: true, // Make sure the files aren't queued until manually added
            previewsContainer: false, // Define the container to display the previews
            clickable: "#media-add", // Define the element that should be used as click trigger to select files.
            dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
            dictResponseError: 'Error while uploading file!'
        });

        this.dropzone.on("success", function (this: MediaService, file: Dropzone.DropzoneFile, data: Response) {
            Response.process(data);
        }.bind(this));

        this.dropzone.on("addedfile", function (this: MediaService, file: Dropzone.DropzoneFile) {
            this.progress.find('.progress-bar').css({ width: 0 + "%" });
            this.progressText.find('span').html(0 + "%");
        }.bind(this));

        // Update the total progress bar
        this.dropzone.on("totaluploadprogress", function (this: MediaService, totalProgress: number, totalBytes: number, totalBytesSent: number) {
            this.progress.find('.progress-bar').css({ width: totalProgress + "%" });
            this.progressText.find('span').html(totalProgress + "%");
        }.bind(this));

        this.dropzone.on("sending", function (this: MediaService, file: Dropzone.DropzoneFile) {
            // Show the total progress bar when upload starts
            this.progressArea.classList.remove('collapse');
            this.progress.find('.progress-bar').css({ width: 0 + "%" });
            this.progressText.find('span').html(0 + "%");
        }.bind(this));

        // Hide the total progress bar when nothing's uploading anymore
        this.dropzone.on("complete", function (this: MediaService, file: Dropzone.DropzoneFile) {
            this.media.Reload();
        }.bind(this));

        // Hide the total progress bar when nothing's uploading anymore
        this.dropzone.on("queuecomplete", function (this: MediaService) {
            this.progressArea.classList.add('collapse');
            this.media.Reload();
        }.bind(this));
    }

    createDirectory(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        let createDirectoryModal: ModalController = new ModalController({
            onComplete: function (this: MediaService) {
                let form = document.getElementById('content-directories-edit-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (this: MediaService, sender: Validator, data: Response) {
                        if (data.success) {
                            this.media.Reload();
                            createDirectoryModal.close();
                            Alerts.success(data.message);
                        } else {
                            Alerts.error(data.error);
                        }
                    }.bind(this)
                });
            }.bind(this)
        });
        createDirectoryModal.show($(e.target).attr('href'), this.element);
    }

    deleteDirectory(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        Alerts.confirm({

        }, function (this: MediaService, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e, function (this: MediaService, sender: HTMLElement, data: Response) {
                    // parent directory id is stored in the response data array.
                    if (data.data.length > 0) {
                        var listUrl = document.createElement('a');
                        listUrl.href = $(this.element).data('url');
                        listUrl.search = `?dir=${data.data[0]}`;
                        this.media.Reload(new URL(listUrl.href));
                    }
                }.bind(this), 5000);
            }
        }.bind(this))
    }

    uploadUrl() {
        return $("#media-upload").data('url') + "?directoryId=" + $("#media-list > #upload-directory-id").val();
    }

    action(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        let mediaObject: MediaObject = $(e.target).data('json') as MediaObject;
        if (this.options.onAction) {
            this.options.onAction(this.element, mediaObject);
        }
        switch (this.options.action) {
            case 'insert':
                this.insert(mediaObject, e);
                break;
            case 'attach':
                this.attach(mediaObject, e);
                break;
            default:
                this.show(mediaObject, e);
                break;
        }
    }

    show(this: MediaService, mediaObject: MediaObject, sender: JQuery.ClickEvent) {
        this.currentBlade = new ModalController();
        this.currentBlade.show($(sender.target).data('blade'), sender.target);
    }

    insert(this: MediaService, mediaObject: MediaObject, e: JQuery.ClickEvent): void {
        // basic functionality to insert the correct string from the media response (from uploader) to given input element. 
    }

    attach(this: MediaService, mediaObject: MediaObject, e: JQuery.ClickEvent): void {
        // once file is uploaded to given directory, send media id to the given attach endpoint.
        Alerts.log(`[MediaService.attach] Attaching media object id ${mediaObject.id} - ${mediaObject.filename} to url: ${this.options.url}`);
        $.post(this.options.url, { mediaId: mediaObject.id }, function (this: MediaService, data: Response) {
            Response.process(data);
            this.refresh(data);
        }.bind(this));
    }

    clear(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        Alerts.confirm({

        }, function (this: MediaService, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e, function (this: MediaService, sender: HTMLElement, data: Response) {

                    Response.process(data);
                    this.refresh(data);

                }.bind(this), 5000);
            }
        }.bind(this))
    }

    refresh(this: MediaService, data: Response): void {

        let icon = data.media.icon;
        if (data.media.genericFileType === "Image") {
            icon = data.media.mediumUrl;
        }

        if (this.options.refresh) {
            let $image = $(this.options.refresh);
            $image.css({
                'background-image': 'url(' + icon + ')'
            });
            $image.find('img').attr('src', icon);
            $image.removeClass('loading');
        }

        if (this.options.json && data.mediaJson) {
            let $json = $(this.options.json);
            $json.val(data.mediaJson);
        }

    }

    delete(this: MediaService, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        Alerts.confirm({
            html: 'This file will be permanently deleted, are you sure?'
        }, function (this: MediaService, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e, function (this: MediaService, sender: HTMLElement, data: Response) {
                    this.media.Reload();
                    if (this.currentBlade) {
                        this.currentBlade.close();
                    }
                }.bind(this), 5000);
            }
        }.bind(this))
    }

    destroy(this: MediaService) {
        this.dropzone.destroy();
    }
}

export class MediaModal {
    modal: ModalController;
    list: HTMLElement;
    service: MediaService;
    element: HTMLElement;

    constructor() {
        $('body').on('click', '[data-hood-media=attach],[data-hood-media=select]', this.load.bind(this));
    }

    load(this: MediaModal, e: JQuery.ClickEvent): void {

        this.element = e.target;
        this.modal = new ModalController({

            onComplete: function (this: MediaModal, sender: HTMLElement) {

                this.list = document.getElementById('media-list');
                this.service = new MediaService(this.list, {
                    action: this.element.dataset.hoodMedia,
                    url: this.element.dataset.hoodMediaUrl,
                    json: this.element.dataset.hoodMediaJson,
                    refresh: this.element.dataset.hoodMediaRefresh,
                    beforeAction: function (this: MediaModal, sender: HTMLElement, mediaObject: MediaObject) {
                        Alerts.log(`[beforeAction] Attaching media object id ${mediaObject.id} - ${mediaObject.filename} to url: ${this.element.dataset.hoodMediaUrl}`);
                    }.bind(this),
                    onAction: function (this: MediaModal, sender: HTMLElement, mediaObject: MediaObject) {
                        Alerts.log(`[onAction] Attached media object id ${mediaObject.id} - ${mediaObject.filename} to url: ${this.element.dataset.hoodMediaUrl}`);
                        this.service.destroy();
                        this.modal.close();
                    }.bind(this),
                    onListLoad: (sender: HTMLElement) => {
                        Alerts.log('Commencing media list fetch.');
                    },
                    onListRender: (sender: HTMLElement, data: string) => {
                        Alerts.log('Fetched media list data.');
                        return data;
                    },
                    onListComplete: (sender: HTMLElement, data: string) => {
                        Alerts.log('Finished loading media list... now attach the media actions to the things.', 'warning');
                    },
                    onError: (jqXHR: any, textStatus: any, errorThrown: any) => {
                        Alerts.log(`Error loading media list: ${textStatus}`);
                    },
                });

            }.bind(this)

        });
        this.modal.show($(e.target).data('hood-media-list'), e.target);
    }
}

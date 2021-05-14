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
    action?: 'insert' | 'attach' | 'select';

    /**
     * Called before the data is fetched.
     */
    onListLoad?: (sender: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the list. Must return the data back to datalist to render.
     */
    onListRender?: (sender: HTMLElement, html: string) => string;

    /**
     * Called when loading and rendering is complete.
     */
    onListComplete?: (sender: HTMLElement, html: string) => void;

    /**
     * Called when an error occurs.
     */
    onError?: (jqXHR: any, textStatus: any, errorThrown: any) => void;

    /**
     * Called before the data is fetched.
     */
    beforeAction?: (sender: HTMLElement) => void;

    /**
     * Called before the data is fetched.
     */
    onAction?: (sender: HTMLElement) => void;

}

export class MediaService {
    media: DataList;
    element: HTMLElement;
    options: MediaOptions = {
    };
    currentBlade: ModalController;
    progress: JQuery<HTMLElement>;
    progressText: JQuery<HTMLElement>;
    uploadButton: HTMLElement;
    uploader: HTMLElement;
    progressArea: HTMLElement;

    constructor(element: HTMLElement, options: MediaOptions) {
        this.element = element;
        if (!this.element) {
            return;
        }

        this.options = { ...this.options, ...options };

        $('body').on('click', '.media-delete', this.delete.bind(this));

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

        let myDropzone = new Dropzone("#media-upload", {
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

        myDropzone.on("success", function (this: MediaService, file: Dropzone.DropzoneFile, data: Response) {
            Response.process(data);
        }.bind(this));

        myDropzone.on("addedfile", function (this: MediaService, file: Dropzone.DropzoneFile) {
            this.progress.find('.progress-bar').css({ width: 0 + "%" });
            this.progressText.find('span').html(0 + "%");
        }.bind(this));

        // Update the total progress bar
        myDropzone.on("totaluploadprogress", function (this: MediaService, totalProgress: number, totalBytes: number, totalBytesSent: number) {
            this.progress.find('.progress-bar').css({ width: totalProgress + "%" });
            this.progressText.find('span').html(totalProgress + "%");
        }.bind(this));

        myDropzone.on("sending", function (this: MediaService, file: Dropzone.DropzoneFile) {
            // Show the total progress bar when upload starts
            this.progressArea.classList.remove('collapse');
            this.progress.find('.progress-bar').css({ width: 0 + "%" });
            this.progressText.find('span').html(0 + "%");
        }.bind(this));

        // Hide the total progress bar when nothing's uploading anymore
        myDropzone.on("complete", function (this: MediaService, file: Dropzone.DropzoneFile) {
            this.media.Reload();
        }.bind(this));

        // Hide the total progress bar when nothing's uploading anymore
        myDropzone.on("queuecomplete", function (this: MediaService) {
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
        switch (this.options.action) {
            case 'insert':
                this.insert(mediaObject);
                break;
            case 'attach':
                this.insert(mediaObject);
                break;
            default:
                this.currentBlade = new ModalController();
                this.currentBlade.show($(e.target).data('blade'), e.target);
                break;
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

    insert(mediaObject: MediaObject): void {
        // basic functionality to insert the correct string from the media response (from uploader) to given input element. 
    }

    attach(mediaObject: MediaObject): void {
        // once file is uploaded to given directory, send media id to the given attach endpoint.
    }

}

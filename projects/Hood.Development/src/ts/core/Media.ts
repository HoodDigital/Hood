import { Alerts } from "./Alerts";
import { Response } from "./Response";
import { DataList } from "./DataList";
import { ModalController } from "./Modal";
import { Validator } from "./Validator";
import { Modal } from "bootstrap";

export declare interface MediaObject {

    id: number;
    FileSize: number;
    FileType: string;
    Filename: string;
    BlobReference: string;

    CreatedOn: Date;

    Url: string;

    ThumbUrl: string;
    SmallUrl: string;
    MediumUrl: string;
    LargeUrl: string;
    UniqueId: string;

    GenericFileType: 'Image' | 'PDF' | 'Word' | 'Photoshop' | 'Excel' | 'PowerPoint' | 'Directory' | 'Audio' | 'Video' | 'Unknown';

    DownloadUrl: string;
    DownloadUrlHttps: string;

    Icon: string;
    FormattedSize: string;
    Path: string;

}

export interface MediaOptions {

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

    }

    constructor(element: HTMLElement, options: MediaOptions) {
        this.element = element;
        if (!this.element) {
            return;
        }

        this.options = { ...this.options, ...options };

        $(this.element).on('click', '.media-delete', this.delete);
        $(this.element).on('click', '.media-create-directory', this.createDirectory)
        $(this.element).on('click', '.media-directories-delete', this.deleteDirectory);


        this.media = new DataList(this.element, {
            onLoad: this.options.onListLoad,
            onError: this.options.onError,
            onRender: this.options.onListRender,
            onComplete: this.options.onListComplete
        });

    }

    createDirectory(e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();
        let that = this;
        let createDirectoryModal: ModalController = new ModalController({
            onComplete: function () {
                let form = document.getElementById('content-directories-edit-form') as HTMLFormElement;
                let validator = new Validator(form, {
                    onComplete: function (sender: Validator, data: Response) {
                        data.process();
                        that.media.Reload();
                        createDirectoryModal.close();
                    }
                });
            }
        });
        createDirectoryModal.show($(this).attr('href'), this.element);
    }

    deleteDirectory(this: HTMLElement, e: JQuery.ClickEvent) {
        throw new Error("Method not implemented.");
    }

    delete(this: HTMLElement, e: JQuery.ClickEvent) {
        throw new Error("Method not implemented.");
    }

    static insert(): void {
        // basic functionality to insert the correct string from the media response (from uploader) to given input element. 
    }

    static attach(): void {
        // once file is uploaded to given directory, send media id to the given attach endpoint.
    }

}

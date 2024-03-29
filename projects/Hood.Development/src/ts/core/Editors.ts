import tinymce from 'tinymce/tinymce';

import { MediaObject, MediaService } from './Media';
import { ModalController } from './Modal';

export declare interface EditorOptions {
    linkClasses?: any;
    imageClasses?: any;
}

export class Editors {
    options: EditorOptions = {
        linkClasses: [
            { title: 'None', value: '' },
            { title: 'Button link', value: 'btn btn-default' },
            { title: 'Theme coloured button link', value: 'btn btn-primary' },
            { title: 'Popup image/video', value: 'colorbox-iframe' },
            { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
            { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
            { title: 'Large link', value: 'font-lg' },
            { title: 'Large button link', value: 'btn btn-default btn-lg' },
            { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
            { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
            { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
            { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
        ],
        imageClasses: [
            { title: 'None', value: '' },
            { title: 'Full Width', value: 'user-image full' },
            { title: 'Left Aligned', value: 'user-image left' },
            { title: 'Centered', value: 'user-image center' },
            { title: 'Right Aligned', value: 'user-image right' },
            { title: 'Inline with text, top aligned', value: 'user-image inline top' },
            { title: 'Inline with text, middle aligned', value: 'user-image inline' },
            { title: 'Inline with text, bottom aligned', value: 'user-image inline bottom' },
            { title: 'Pulled Left', value: 'user-image pull-left' },
            { title: 'Pulled Right', value: 'user-image pull-right' },
        ]
    }

    constructor(options?: EditorOptions) {
        this.options = { ...this.options, ...options };
    }

    richTextEditors() {


        tinymce.init({
            selector: '.tinymce-full',
            height: 150,
            menubar: false,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste textcolor'
            ],
            toolbar: "fullscreen code | styleselect forecolor backcolor | hoodimage link media image | bold italic | alignleft aligncenter alignright | bullist numlist | table | undo redo",
            link_class_list: this.options.linkClasses,
            image_class_list: this.options.imageClasses,
            setup: this.setupCommands.bind(this),
            image_dimensions: false,
            content_css: [
                '/dist/css/editor.css'
            ],

        });

        tinymce.init({
            selector: '.tinymce-simple',
            height: 150,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            menubar: false,
            toolbar: 'fullscreen | bold italic | bullist numlist | undo redo | link',
            link_class_list: this.options.linkClasses,
            image_class_list: this.options.imageClasses,
            setup: this.setupCommands.bind(this),
            image_dimensions: false
        });

        tinymce.init({
            selector: '.tinymce-full-content',
            height: 500,
            menubar: false,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste textcolor'
            ],
            toolbar: "fullscreen code | styleselect forecolor backcolor | hoodimage link media image | bold italic | alignleft aligncenter alignright | bullist numlist | table | undo redo",
            link_class_list: this.options.linkClasses,
            image_class_list: this.options.imageClasses,
            setup: this.setupCommands.bind(this),
            image_dimensions: false
        });

        tinymce.init({
            selector: '.tinymce-simple-content',
            height: 500,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste'
            ],
            menubar: false,
            toolbar: 'fullscreen | bold italic | bullist numlist | undo redo | link',
            link_class_list: this.options.linkClasses,
            image_class_list: this.options.imageClasses,
            image_dimensions: false
        });
    }

    mediaModal: ModalController;
    list: HTMLElement;
    service: MediaService;
    currentEditor: JQuery<HTMLElement>;

    setupCommands(editor: any) {
        this.currentEditor = $('#' + editor.id);
        if (this.currentEditor.data('hoodMediaList')) {

            editor.ui.registry.addButton('hoodimage', {
                text: 'Insert image...',
                icon: 'image',
                onAction: function (this: Editors, e: any) {

                    this.mediaModal = new ModalController({

                        onComplete: function (this: Editors, sender: HTMLElement) {

                            this.list = document.getElementById('media-list');
                            this.service = new MediaService(this.list, {
                                action: 'insert',
                                url: this.currentEditor.data('hoodMediaList'),
                                targetEditor: editor,
                                size: this.currentEditor.data('hoodMediaSize'),
                                beforeAction: function (this: Editors, sender: HTMLElement, mediaObject: MediaObject) {
                                }.bind(this),
                                onAction: function (this: Editors, sender: HTMLElement, mediaObject: MediaObject) {
                                    this.mediaModal.close();
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
                    this.mediaModal.show(this.currentEditor.data('hoodMediaList'), e.currentTarget);

                }.bind(this)
            });

        }
    }

}
import Pickr from '@simonwep/pickr';
import tinymce, { Editor } from 'tinymce/tinymce';
import { Alerts, MediaModal, MediaObject, MediaService } from '../hood';
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
        this.richTextEditors();
        this.colorPickers();
    }

    colorPickers() {
        let updateColorFieldValue = function (color: any, eventSource: any, instance: any) {
            let elemId = $(instance._root.button).parent().data('target');
            $(instance._root.button).parent().css({ 'background-color': color.toHEXA().toString() });
            let colorHex = instance.getColor().toHEXA();
            var result = "";
            for (let i = colorHex.length - 1; i >= 0; i--) {
                result = colorHex[i] + result;
            }
            $(elemId).val('#' + result);
            $(elemId).change();
        };
        var pickrs = [];
        // Simple example, see optional options for more configuration.
        $('.color-picker').each(function (index: number, elem: HTMLElement) {

            let lockOpacity = true;
            if ($(this).data('opacity') == 'true') {
                lockOpacity = false;
            }

            let pickr = Pickr.create({
                el: elem.children[0] as HTMLElement,
                appClass: 'custom-class',
                theme: 'monolith',
                useAsButton: true,
                default: $(this).data('default') || 'none',
                lockOpacity: lockOpacity,
                defaultRepresentation: 'HEXA',
                position: 'bottom-end',
                components: {
                    opacity: true,
                    hue: true,

                    interaction: {
                        hex: false,
                        rgba: false,
                        hsva: false,
                        input: true,
                        clear: true
                    }
                }
            })

                .on('init', function (instance: any) {
                    let elemId = $(instance._root.button).parent().data('target');
                    let value = $(elemId).val();
                    $(instance._root.button).on('click', $.proxy(function () {
                        this.show();
                    }, instance));
                    $(elemId).on('click', $.proxy(function () {
                        this.show();
                    }, instance));
                    if (value) {
                        instance.setColor(value);
                        updateColorFieldValue(instance.getColor(), null, instance);
                    }
                })

                .on('clear', function (instance: any) {
                    let elemId = $(instance._root.button).parent().data('target');
                    instance.setColor('transparent');
                    updateColorFieldValue(instance.getColor(), null, instance);
                    $(elemId).val('');
                    $(elemId).change();
                })

                .on('change', updateColorFieldValue);

            pickrs.push(pickr);

        });
    }

    richTextEditors() {


        console.error("Cannot load tinymce-full, Media.Actions.Load.Insert() is not implemented.");
        tinymce.init({
            selector: '.tinymce-full',
            height: 150,
            menubar: false,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
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
            toolbar: 'bold italic | bullist numlist | undo redo | link',
            link_class_list: this.options.linkClasses,
            image_class_list: this.options.imageClasses,
            setup: this.setupCommands.bind(this),
            image_dimensions: false
        });

        console.error("Cannot load tinymce-full-content, Media.Actions.Load.Insert() is not implemented.");
        tinymce.init({
            selector: '.tinymce-full-content',
            height: 500,
            menubar: false,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
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
                'insertdatetime media contextmenu paste code'
            ],
            menubar: false,
            toolbar: 'bold italic | bullist numlist | undo redo | link',
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

            editor.addButton('hoodimage', {
                text: 'Insert image...',
                icon: false,
                onclick: function (this: Editors, e: any) {

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
                                    this.service.destroy();
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
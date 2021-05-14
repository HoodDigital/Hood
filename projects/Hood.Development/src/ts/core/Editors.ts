import tinymce from 'tinymce/tinymce';

export class Editors {
    constructor() {

        console.error("Cannot load tinymce-full, Media.Actions.Load.Insert() is not implemented.");
        //tinymce.init({
        //    selector: '.tinymce-full',
        //    height: 150,
        //    menubar: false,
        //    plugins: [
        //        'advlist autolink lists link image charmap print preview anchor media',
        //        'searchreplace visualblocks code fullscreen',
        //        'insertdatetime media contextmenu paste code'
        //    ],
        //    toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
        //    link_class_list: this.LinkClasses,
        //    image_class_list: this.ImageClasses,
        //    setup: Media.Actions.Load.Insert,
        //    image_dimensions: false
        //});

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
            link_class_list: this.LinkClasses,
            image_class_list: this.ImageClasses,
            image_dimensions: false
        });

        console.error("Cannot load tinymce-full-content, Media.Actions.Load.Insert() is not implemented.");
        //tinymce.init({
        //    selector: '.tinymce-full-content',
        //    height: 500,
        //    menubar: false,
        //    plugins: [
        //        'advlist autolink lists link image charmap print preview anchor media',
        //        'searchreplace visualblocks code fullscreen',
        //        'insertdatetime media contextmenu paste code'
        //    ],
        //    toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
        //    link_class_list: this.LinkClasses,
        //    image_class_list: this.ImageClasses,
        //    setup: Media.Actions.Load.Insert,
        //    image_dimensions: false
        //});

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
            link_class_list: this.LinkClasses,
            image_class_list: this.ImageClasses,
            image_dimensions: false
        });
    }

    private LinkClasses = [
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
    ];

    private ImageClasses = [
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
    ];

}
if (!$.hood)
    $.hood = {}
$.hood.FileUpload = function (element, options) {
    this.Options = $.extend({
        element: element,
        attachUrl: '/admin/media/attach',
        refreshUrl: null,
        imageTag: null,
        completeFunction: function () {}
    }, options || {});
    this.Options.refreshUrl = $(this.Options.element).data('refresh');
    this.Options.imageTag = $(this.Options.element).data('tag');
    this.Options.Image = $('.' + this.Options.imageTag);
    this.Uploader = $(this.Options.element).kendoUpload({

        async: {
            saveUrl: "/admin/media/upload?entity=" + $(this.Options.element).data('entity')
                                 + "&id=" + $(this.Options.element).data('id')
                                 + "&field=" + $(this.Options.element).data('field')
                                 + "&refresh=" + this.Options.refreshUrl
                                 + "&tag=" + this.Options.imageTag
        },

        success: $.proxy(function (e) {
            $.hood.Alerts.Success("Uploaded!");
            if (!$.hood.Helpers.IsNullOrUndefined(this.Options.imageTag) && !$.hood.Helpers.IsNullOrUndefined(this.Options.refreshUrl)) {
                this.Options.Image.addClass('loading');
                $.get(this.Options.refreshUrl, { id: $(this.Options.element).data('id') }, $.proxy(function (data) {
                    this.Options.Image.css({
                        'background-image': 'url(' + data.SmallUrl + ')'
                    });
                    this.Options.Image.find('img').attr('src', data.SmallUrl);
                    this.Options.Image.removeClass('loading');
                    this.Options.completeFunction();
                }, this));
            }
        }, this),

        error: function (e) {
            // Array with information about the uploaded files
            var files = e.files;
            if (e.operation === "upload") {
                $.hood.Alerts.Error(e.XMLHttpRequest.response, "Error Uploading");
            }
        },

        select: $.proxy(function (e) {
            $.each(e.files, $.proxy(function (index, value) {
                // Size Check.
                maxSize = 1048576 * 4;
                if (!$.hood.Helpers.IsNullOrUndefined($(this.Options.element).data('maxsize'))) {
                    maxSize = Number($(this.Options.element).data('maxsize'));
                }
                yourSizeMb = (Number(value.size) / 1048576).toFixed(2);
                maxSizeMb = (Number(maxSize) / 1048576).toFixed(2);
                if (value.size > maxSize) {
                    $.hood.Alerts.Error("The image file must be less than " + maxSizeMb + "Mb. Your file is " + yourSizeMb + "Mb.", "File is too large!")
                    e.preventDefault();
                }

                // Type Check
                allowedTypes = "*";
                if (!$.hood.Helpers.IsNullOrUndefined($(this.Options.element).data('type'))) {
                    allowedTypes = $(this.Options.element).data('type');
                    if (allowedTypes === "image")
                        allowedTypes = ".jpg.jpeg.gif.png.tiff.bmp";
                    if (allowedTypes === "document")
                        allowedTypes = ".doc.docx.pdf.xls.xlsx.ppt.pptx";
                }
                if (allowedTypes !== "*") {
                    if (allowedTypes.contains(value.extension) === -1) {
                        $.hood.Alerts.Error("The file you have provided is not one of the allowed types for this upload.", "Wrong file type!")
                        e.preventDefault();
                    }
                }
            }, this));

        }, this),

        multiple: false

    });
}
$.fn.hoodFileUpload = function (options) {
    return this.each(function () {
        var element = $(this);
        // Return early if this element already has a plugin instance
        if (element.data('hoodFileUpload')) return;
        // pass options to plugin constructor
        var hoodFileUpload = new $.hood.FileUpload(this, options);
        // Store plugin object in this element's data
        element.data('hoodFileUpload', hoodFileUpload);
    });
};
$(document).ready(function () {
    $('.hood-fileupload').each(function () {
        $(this).hoodFileUpload({
            attachUrl: $(this).data('attach') ? $(this).data('attach') : '/admin/media/upload',
            refreshUrl: $(this).data('refresh') ? $(this).data('refresh') : "",
            imageTag: $(this).data('tag') ? $(this).data('tag') : ""
        });
    });
});


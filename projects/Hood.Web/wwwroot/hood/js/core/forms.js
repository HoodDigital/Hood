if (!$.hood)
    $.hood = {};
$.hood.Forms = {
    Init: function () {
        $('.floating-label > label').each(function () {
            let $me = $(this);
            $me.parent().append($me);
        });
    },
    GetAllowedExtensions: function (section) {
        switch (section) {
            case "Image":
                return ['png', 'jpg', 'jpeg', 'bmp', 'gif'];
            case "Document":
                return ['doc', 'docx', 'pdf', 'rtf'];
            case "All":
                return '';
        }
    },
    GetAllowedFiles: function (section) {
        switch (section) {
            case "Image":
                return 'image/png,jpg,image/jpeg,image/gif';
            case "Document":
                return 'application/msword,application/pdf,text/rtf';
            case "All":
                return '';
        }
    }
};
$(document).ready($.hood.Forms.Init);

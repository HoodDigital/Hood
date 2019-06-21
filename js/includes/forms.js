if (!$.hood)
    $.hood = {}
$.hood.Forms = {
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
    },
    ValidationErrorPlacement: function (error, element) {
        if (element.is(':checkbox') || element.is(':radio')) {
            var controls = element.closest('div[class*="col-"]');
            if (controls.find(':checkbox,:radio').length > 1) controls.append(error);
            else error.insertAfter(element.nextAll('.lbl:eq(0)').eq(0));
        }
        else if (element.is('.select2')) {
            error.insertAfter(element.siblings('[class*="select2-container"]:eq(0)'));
        }
        else if (element.is('.chosen-select')) {
            error.insertAfter(element.siblings('[class*="chosen-container"]:eq(0)'));
        } if (element.is('.drop-error')) {
            error.insertAfter(element.parents('.input-group'));
        }
        else error.insertAfter(element.parent());
    },
    ValidationSuccess: function (e) {
        $(e).closest('label').removeClass('state-error').addClass('state-success');
        $(e).remove();
    },
    ValidationInvalid: function (event, validator) { //display error alert on form submit   
        $('.alert-error', $('.login-form')).show();
    },
    ValidationHighlight: function (e) {
        $(e).closest('label').removeClass('state-success').addClass('state-error');
    }
};

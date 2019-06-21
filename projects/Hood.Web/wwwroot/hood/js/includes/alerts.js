if (!$.hood)
    $.hood = {};
$.hood.Alerts = {
    Error: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, 'error');
        else
            this.Alert(msg, title, callback, 'error');
    },
    Warning: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, 'warning');
        else
            this.Alert(msg, title, callback, 'warning');
    },
    Message: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, 'info');
        else
            this.Alert(msg, title, callback, 'info');
    },
    Success: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, 'success');
        else
            this.Alert(msg, title, callback, 'success');
    },
    Alert: function (msg, title, cssClass = 'error', image = '') {
        var template = $('#template-toast').html();
        var id = 'toast-' + $('#template-toast').children().length;
        var toastData = {
            id: id,
            image: image,
            cssClass: cssClass,
            title: title,
            message: msg
        };
        var result = Mustache.render(template, toastData);
        $('#toasts').append(result);
        $('#' + id).toast({
            autohide: true,
            delay: 500
        });
    },
    SweetAlert: function (msg, title, type = 'info', confirmButtonText = 'Ok', footer = '') {
        Swal.fire({
            title: title,
            text: msg,
            type: type,
            confirmButtonText: confirmButtonText,
            footer: footer
        });
    },
    Confirm: function (msg, title, callback, type = 'info', confirmButtonText = 'Ok', cancelButtonText = 'Cancel', footer = '') {
        Swal.fire({
            title: title,
            text: msg,
            type: type,
            footer: footer,
            showCancelButton: true,
            confirmButtonText: confirmButtonText,
            cancelButtonText: cancelButtonText
        }).then(callback(result));
    }
};

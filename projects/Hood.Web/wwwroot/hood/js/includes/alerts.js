const swalWithBootstrapButtons = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-success btn-lg m-1 pl-4 pr-4',
        cancelButton: 'btn btn-danger btn-lg m-1'
    },
    buttonsStyling: false
});
const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: true,
    //timer: 10000
});

if (!$.hood)
    $.hood = {};
$.hood.Alerts = {
    Error: function (msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'error', footer, showConfirmButton, timer, callback);
        else
            this.Alert(msg, title, 'error');
    },
    Warning: function (msg, title, sweetAlert, footer, showConfirmButton, timer, callback)  {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'warning', footer, showConfirmButton, timer, callback);
        else
            this.Alert(msg, title, 'warning');
    },
    Message: function (msg, title, sweetAlert, footer, showConfirmButton, timer, callback)  {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'info', footer, showConfirmButton, timer, callback);
        else
            this.Alert(msg, title, 'info');
    },
    Success: function (msg, title, sweetAlert, footer, showConfirmButton, timer, callback)  {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'success', footer, showConfirmButton, timer, callback);
        else
            this.Alert(msg, title, 'success');
    },
    Alert: function (msg, title, type) {
       Toast.fire({
            type: type || 'info',
            html: msg,
            title: title
        });
    },
    SweetAlert: function (msg, title, type, footer, showConfirmButton, timer, callback) {
        swalWithBootstrapButtons.fire({
            title: title,
            html: msg,
            type: type || 'info',
            footer: footer,
            showConfirmButton: showConfirmButton,
            timer: timer
        }).then(callback);
    },
    Confirm: function (msg, title, callback, type, footer, confirmButtonText, cancelButtonText) {
        swalWithBootstrapButtons.fire({
            title: title || 'Woah!',
            html: msg || 'Are you sure you want to do this?',
            type: type || 'warning',
            footer: footer || '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>',
            showCancelButton: true,
            confirmButtonText: confirmButtonText || 'Ok',
            cancelButtonText: cancelButtonText || 'Cancel'
        }).then(function (result) { callback(result); });
    }
};

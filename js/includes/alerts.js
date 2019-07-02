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
    showConfirmButton: false,
    timer: 30000
});

if (!$.hood)
    $.hood = {};
$.hood.Alerts = {
    Error: function (msg, title, sweetAlert, footer, showConfirmButton, timer) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'error', footer, showConfirmButton, timer);
        else
            this.Alert(msg, title, 'error');
    },
    Warning: function (msg, title, sweetAlert, footer, showConfirmButton, timer)  {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'warning', footer, showConfirmButton, timer);
        else
            this.Alert(msg, title, 'warning');
    },
    Message: function (msg, title, sweetAlert, footer, showConfirmButton, timer)  {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'info', footer, showConfirmButton, timer);
        else
            this.Alert(msg, title, 'info');
    },
    Success: function (msg, title, sweetAlert, footer, showConfirmButton, timer)  {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, 'success', footer, showConfirmButton, timer);
        else
            this.Alert(msg, title, 'success');
    },
    Alert: function (msg, title, type) {
        $('.swal2-container').remove();
       Toast.fire({
            type: type || 'info',
            text: msg,
            title: title
        });
    },
    SweetAlert: function (msg, title, type, footer, showConfirmButton, timer) {
        $('.swal2-container').remove();
        swalWithBootstrapButtons.fire({
            title: title,
            text: msg,
            type: type || 'info',
            footer: footer,
            showConfirmButton: showConfirmButton,
            timer: timer
        }).then(function (result) { alert('test'); });
    },
    Confirm: function (msg, title, callback, type, footer, confirmButtonText, cancelButtonText) {
        $('.swal2-container').remove();
        swalWithBootstrapButtons.fire({
            title: title,
            text: msg,
            type: type || 'info',
            footer: footer,
            showCancelButton: true,
            confirmButtonText: confirmButtonText || 'Ok',
            cancelButtonText: cancelButtonText || 'Cancel'
        }).then(function (result) { callback(result); });
    }
};

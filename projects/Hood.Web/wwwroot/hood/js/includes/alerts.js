if (!$.hood)
    $.hood = {}
$.hood.Alerts = {
    Error: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, "error");
        else
            this.Alert(msg, title, callback, "error");
    },
    Warning: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, "warning");
        else
            this.Alert(msg, title, callback, "warning");
    },
    Message: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, "info");
        else
            this.Alert(msg, title, callback, "info");
    },
    Success: function (msg, title, callback, sweetAlert) {
        if (sweetAlert === true)
            this.SweetAlert(msg, title, callback, "success");
        else
            this.Alert(msg, title, callback, "success");
    },
    Alert: function (msg, title, callback, type) {
        $.getScript('/lib/toastr/toastr.min.js', $.proxy(function () {
            $.loadCss('toastr-css', '/lib/toastr/toastr.min.css');
            msg = msg.replace(/(?:\r\n|\r|\n)/g, '<br />');
            toastr.options = {
                closeButton: true,
                debug: true,
                progressBar: true,
                preventDuplicates: true,
                positionClass: "toast-bottom-left",
                showDuration: "400",
                hideDuration: "1000",
                timeOut: "7000",
                extendedTimeOut: "1000",
                showEasing: "swing",
                hideEasing: "linear",
                showMethod: "fadeIn",
                hideMethod: "fadeOut"
            };
            toastr.options.onclick = callback;
            var $toast = toastr[type](msg, title);
        }, this));
    },
    SweetAlert: function (msg, title, callback, type) {
        $.getScript('/lib/sweetalert/dist/sweetalert.min.js', $.proxy(function () {
            $.loadCss('sweetalert-css', '/lib/sweetalert/dist/sweetalert.css');
            swal({
                title: title,
                text: msg,
                timer: 55000,
                type: type
            });
        }, this));
    }
};

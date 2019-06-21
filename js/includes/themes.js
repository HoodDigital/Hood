if (!$.hood)
    $.hood = {}
$.hood.Themes = {
    Init: function () {
        $('body').on('click', '.activate-theme', this.Activate);
    },
    Activate: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The site will change themes, and the selected theme will be live right away.",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes, go ahead.",
            cancelButtonText: "No, cancel!",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
            closeOnCancel: false
        },
        function (isConfirm) {
            if (isConfirm) {
                // delete functionality
                $.post('/admin/themes/activate', { name: $this.data('name') }, function (data) {
                    if (data.Success) {
                        window.location = '/admin/theme';
                    } else {
                        swal("Error", "There was a problem activating the theme:\n\n" + data.Errors, "error");
                    }
                });
            } else {
                swal("Cancelled", "It's all good in the hood!", "error");
            }
        });
    },
}
$.hood.Themes.Init();

if (!$.hood)
    $.hood = {}
$.hood.Api = {
    Init: function () {
        $('body').on('click', '.delete-api-key', this.Delete);
        $('body').on('click', '.create-api-key', this.Create.Init);
        $('body').on('click', '.activate-api-key', this.Activate);
        $('body').on('click', '.deactivate-api-key', this.Deactivate);
        $('body').on('click', '.create-api-key', this.Create.Init);
    },
    Delete: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The api key will be permanently removed.",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes, go ahead.",
            cancelButtonText: "No, cancel!",
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
            closeOnCancel: false
        },
        function (isConfirm) {
            if (isConfirm) {
                // delete functionality
                $.post('/admin/api/keys/delete/' + $this.data('id'), null, function (data) {
                    if (data.Success) {
                        $.hood.Blades.Close();
                        swal({
                            title: "Deleted!",
                            text: "The api key has now been removed from the website.",
                            timer: 1300,
                            type: "success"
                        });
                        setTimeout(function () {
                            window.location = data.Url;
                        }, 500);
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem deleting the api key: " + data.Errors,
                            timer: 5000,
                            type: "error"
                        });
                    }
                });
            } else {
                swal("Cancelled", "It's all good in the hood!", "error");
            }
        });
    },
    Activate: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The key will usable right away.",
            type: "warning",
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
                $.post('/admin/api/keys/activate/' + $this.data('id'), null, function (data) {
                    if (data.Success) {
                        swal({
                            title: "Activated!",
                            text: "The key has now been activated.",
                            timer: 1300,
                            type: "success"
                        });
                        setTimeout(function () {
                            window.location = data.Url;
                        }, 500);
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem publishing the key: " + data.Errors,
                            timer: 5000,
                            type: "error"
                        });
                    }
                });
            } else {
                swal("Cancelled", "It's all good in the hood!", "error");
            }
        });
    },
    Deactivate: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The key will be unusable.",
            type: "warning",
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
                $.post('/admin/api/keys/deactivate/' + $this.data('id'), null, function (data) {
                    if (data.Success) {
                        swal({
                            title: "Deactivated!",
                            text: "The key has now been deactivated.",
                            timer: 1300,
                            type: "success"
                        });
                        setTimeout(function () {
                            window.location = data.Url;
                        }, 500);
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem deactivating the key: " + data.Errors,
                            timer: 5000,
                            type: "error"
                        });
                    }
                });
            } else {
                swal("Cancelled", "It's all good in the hood!", "error");
            }
        });
    },
    Create: {
        Init: function (e) {
            var $this = $(this);
            e.preventDefault();
            $.hood.Blades.OpenWithLoader('button.create-api-key', '/admin/api/keys/create/', $.hood.Api.Create.SetupCreateForm);
        },
        SetupCreateForm: function () {
            $('#create-api-key-form').find('.datepicker').datepicker({
                todayBtn: "linked",
                keyboardNavigation: false,
                forceParse: false,
                calendarWeeks: true,
                autoclose: true,
                orientation: "bottom",
                format: "dd/mm/yyyy",
            });
            $('#create-api-key-form').hoodValidator({
                validationRules: {
                    Title: {
                        required: true
                    },
                    Description: {
                        required: true
                    }
                },
                submitButtonTag: $('#create-api-key-submit'),
                submitUrl: '/admin/api/keys/create',
                submitFunction: function (data) {
                    if (data.Success) {
                        swal("Created!", "The api key has now been created!", "success");
                        setTimeout(function () {
                            window.location = data.Url;
                        }, 500);
                    } else {
                        swal("Error", "There was a problem creating the api key:\n\n" + data.Errors, "error");
                    }
                }
            });
        }
    },
}
$(window).on('load', function () {
    $.hood.Api.Init();
});


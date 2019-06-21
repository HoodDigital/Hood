if (!$.hood)
    $.hood = {}
$.hood.Users = {
    Init: function () {
        $('body').on('click', '.delete-user', this.Delete);
        $('body').on('click', '.create-user', this.Create.Init);
        $('body').on('change', '#cuGeneratePassword', this.Create.GeneratePassword);
        $('body').on('change', '.role-check', this.Edit.ToggleRole);
        $('body').on('click', '.reset-password', this.Edit.ResetPassword);
    },
    Delete: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The user will be permanently removed and all associated files will be deleted from the system.\n\nThis process CANNOT be undone!\n\nNote: This process will also cancel any active subscriptions.",
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
                    $.post('/admin/users/delete', { id: $this.data('id') }, function (data) {
                        if (data.Success) {
                            swal({
                                title: "Deleted!",
                                text: "The user has now been removed from the website.",
                                timer: 1300,
                                type: "success"
                            });
                            setTimeout(function () {
                                window.location = data.Url;
                            }, 500);
                            swal("Deleted", "", "success");
                        } else {
                            swal("Error", "There was a problem deleting the user:\n\n" + data.Errors, "error");
                        }
                    });
                } else {
                    swal("Cancelled", "It's all good in the hood!", "error");
                }
            });
    },
    Create: {
        Init: function (e) {
            // close open blade
            $('button.create-user').removeClass('btn-primary').addClass('btn-default').html('<i class="fa fa-refresh fa-spin"></i>&nbsp;Loading...');
            $('#right-sidebar').removeClass('animate-all sidebar-open');
            setTimeout(function () {
                $('#right-sidebar').empty();
            });
            $('#right-sidebar').addClass('animate-all');
            // load in the create user blade
            $.get('/admin/users/create/', null, function (data) {
                $('#right-sidebar').html(data);
                // slide open the blade
                $('#right-sidebar').addClass('sidebar-open');
                $('button.create-user').removeClass('btn-default').addClass('btn-primary').html('<i class="fa fa-user-plus"></i>&nbsp;Create new user');
                $.hood.Users.Create.SetupCreateForm();
                $.hood.Helpers.ResetSidebarScroll();
            });

        },
        CancelCreate: function (e) {
            // close open blade
            $('#right-sidebar').removeClass('animate-all sidebar-open');
            setTimeout(function () {
                $('#right-sidebar').empty();
            });

        },
        SetupCreateForm: function () {
            $('#create-user-form').hoodValidator({
                validationRules: {
                    cuFirstName: {
                        required: true
                    },
                    cuLastName: {
                        required: true
                    },
                    cuUserName: {
                        required: true,
                        email: true
                    },
                    cuPassword: {
                        required: true
                    }
                },
                submitButtonTag: $('#create-user-submit'),
                submitUrl: '/admin/users/add',
                submitFunction: function (data) {
                    if (data.Success) {
                        swal({
                            title: "Created!",
                            text: "The user has now been created and can log in right away.",
                            timer: 1300,
                            type: "success"
                        });
                        setTimeout(function () {
                            window.location = data.Url;
                        }, 500);
                    } else {
                        swal("Error", "There was a problem creating the user:\n\n" + data.Errors, "error");
                    }
                }
            });
        },
        GeneratePassword: function () {
            if ($(this).is(':checked')) {
                $('#cuPassword').val($.hood.Helpers.GenerateRandomString(0))
            } else {
                $('#cuPassword').val('')
            }
        }
    },
    Edit: {
        ResetPassword: function () {
            swal({
                title: "Reset password",
                text: "Please enter a new password for the user...",
                type: "input",
                showCancelButton: true,
                closeOnCancel: true,
                closeOnConfirm: false,
                showLoaderOnConfirm: true,
                animation: "slide-from-top",
                inputPlaceholder: "New password..."
            }, function (inputValue) {
                if (inputValue === false) return false; if (inputValue === "") {
                    swal.showInputError("You didn't supply a new password, we can't reset the password without it!"); return false
                }
                $.post('/admin/users/reset/', { id: $('#edit-user-form').data('id'), password: inputValue }, function (data) {
                    if (data.Success) {
                        swal("Success!", "The password has been reset.", "success");
                    } else {
                        swal("Oops!", "There was a problem resetting the password:\n\n" + data.Errors, "error");
                    }
                });
            });
        },
        ToggleRole: function () {
            if ($(this).is(':checked')) {
                $.post('/admin/users/addtorole/', { id: $(this).data('id'), role: $(this).val() }, function (data) {
                    if (data.Success) {
                        $.hood.Alerts.Success("Added user to role.");
                    } else {
                        $.hood.Alerts.Error("Couldn't add the user to the role: " + data.Error);
                    }
                });
            } else {
                $.post('/admin/users/removefromrole/', { id: $(this).data('id'), role: $(this).val() }, function (data) {
                    if (data.Success) {
                        $.hood.Alerts.Success("Removed user from role.");
                    } else {
                        $.hood.Alerts.Error("Couldn't remove the user from the role: " + data.Error);
                    }
                });
            }
        }
    }
}
$.hood.Users.Init();

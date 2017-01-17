if (!$.hood)
    $.hood = {}
$.hood.Users = {
    Init: function () {
        $('body').on('click', '.delete-user', this.Delete);
        $('body').on('click', '.create-user', this.Create.Init);
        $('body').on('click', '.add-to-role', this.Edit.AddToRole);
        $('body').on('click', '.remove-from-role', this.Edit.RemoveFromRole);
        $('body').on('change', '#cuGeneratePassword', this.Create.GeneratePassword);
        if ($('#manage-users-list').doesExist())
            this.Manage.Init();
        if ($('#edit-user-form').doesExist())
            this.Edit.Init();
    },
    Manage: {
        Init: function () {
            $('#manage-users-list').hoodDataList({
                url: '/admin/users/get',
                params: $.hood.Users.Manage.Filter,
                pageSize: 12,
                pagers: '.manage-users-pager',
                template: '#manage-users-template',
                dataBound: function () { },
                refreshOnChange: ".manage-users-change",
                refreshOnClick: ".manage-users-click",
                serverAction: "GET"
            });
        },
        Filter: function () {
            return {
                search: $('#manage-users-search').val(),
                status: $('#manage-users-status').val(),
                sort: $('#manage-users-sort').val(),
                role: $('#manage-users-role').val()
            };
        },
        Refresh: function () {
            if ($('#manage-users-list').doesExist())
                $('#manage-users-list').data('hoodDataList').Refresh()
        }
    },
    Delete: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The user will be permanently removed and all associated files will be deleted from the system.\n\nThis process CANNOT be undone!",
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
                        $('#manage-users-list').data('hoodDataList').Refresh();
                        swal("Deleted", "The user has now been removed from the website.", "success");
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
                        $('#manage-users-list').data('hoodDataList').Refresh();
                        swal("Created!", "The user has now been created and can log in right away!", "success");
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
        Init: function () {
            $('body').on('click', '.reset-password', this.ResetPassword);
        },
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
        AddToRole: function () {
            $userID = $(this).data('id');
            $.post('/admin/users/addtorole/', { id: $(this).data('id'), role: $('#roleToAdd').val() }, function (data) {
                if (data.Success) {
                    $.hood.Users.LoadRoles('#role-list', $userID);
                    swal({
                        title: "Added!",
                        text: "The user has now been added to the new role.",
                        timer: 1300,
                        type: "success"
                    });
                } else {
                    swal({
                        title: "Error!",
                        text: "There was a problem adding the user to the role:\n\n" + data.Errors,
                        timer: 1300,
                        type: "error"
                    });
                }
            });
        },
        RemoveFromRole: function () {
            $userID =  $(this).data('id');
            $.post('/admin/users/removefromrole/', { id: $(this).data('id'), role: $(this).data('role') }, function (data) {
                if (data.Success) {
                    $.hood.Users.LoadRoles('#role-list', $userID);
                    swal({
                        title: "Removed!",
                        text: "The user has now been removed from the role.",
                        timer: 1300,
                        type: "success"
                    });
                } else {
                    swal({
                        title: "Error!",
                        text: "There was a problem removing the user from the role:\n\n" + data.Errors,
                        timer: 1300,
                        type: "error"
                    });
                }
            });
        }
    },
    LoadRoles: function (tag, id) {
        $this = $(tag);
        $.get('/admin/users/roles/', { id: id }, function (data) {
             $this.html(data);
        });
    }
}
$.hood.Users.Init();

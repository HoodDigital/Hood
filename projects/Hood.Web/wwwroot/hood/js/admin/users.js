if (!$.hood)
    $.hood = {};
$.hood.Users = {
    Init: function () {
        $('body').on('click', '.user-delete', this.Delete);

        $('body').on('click', '.user-reset-password', this.Edit.ResetPassword);

        $('body').on('click', '.user-notes-add', this.Edit.Notes.Add);
        $('body').on('click', '.user-notes-delete', this.Edit.Notes.Delete);

        $('body').on('change', '#user-create-form #GeneratePassword', this.Create.GeneratePassword);
        $('body').on('change', '.user-role-check', this.Edit.ToggleRole);
    },

    Loaded: function (data) {
        $.hood.Loader(false);
    },
    Reload: function (complete) {
        $.hood.Inline.Reload($('#user-list'), complete);
    },

    Delete: function (e) {
        e.preventDefault();
        $tag = $(this);

        deleteUserCallback = function (isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function (data) {
                    $.hood.Helpers.ProcessResponse(data, $tag);
                    $.hood.Users.Reload();
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The user will be permanently removed and all associated files will be deleted from the system.",
            "Are you sure?",
            deleteUserCallback,
            'error',
            '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong><br />This process will also cancel any active subscriptions.</span>',
        );
    },

    Create: {
        Loaded: function (e) {
            $('#user-create-form').hoodValidator({
                validationRules: {
                    FirstName: {
                        required: true
                    },
                    LastName: {
                        required: true
                    },
                    UserName: {
                        required: true,
                        email: true
                    },
                    Password: {
                        required: true
                    }
                },
                submitButtonTag: $('#user-create-submit'),
                submitUrl: $('#user-create-form').attr('action'),
                submitFunction: function (data) {
                    $.hood.Helpers.ProcessResponse(data, $tag);
                }
            });
        },
        GeneratePassword: function () {
            if ($(this).is(':checked')) {
                $('#user-create-form #Password').val($.hood.Helpers.GenerateRandomString(0));
                $('#user-create-form #Password').attr('type', 'text');
            } else {
                $('#user-create-form #Password').val('');
                $('#user-create-form #Password').attr('type', 'password');
            }
        }
    },

    Edit: {
        ResetPassword: function (e) {
            e.preventDefault();
            $tag = $(this);

            resetPasswordCallback = function (inputValue) {
                if (inputValue === false) return false; if (inputValue === "") {
                    swal.showInputError("You didn't supply a new password, we can't reset the password without it!"); return false
                }
                $.post($tag.attr('href'), { password: inputValue }, function (data) {
                    $.hood.Helpers.ProcessResponse(data, $tag);
                });
            };

            $.hood.Alerts.Prompt(
                "Please enter a new password for the user...",
                "Reset password",
                resetPasswordCallback
            );

        },
        Notes: {
            Add: function (e) {
                e.preventDefault();
                $tag = $(this);

                addNoteCallback = function (inputValue) {
                    if (inputValue === false || inputValue === "") {
                        swal.showInputError("You enter anything!");
                        return false;
                    }
                    $.post($tag.attr('href'), { note: inputValue }, function (data) {
                        $.hood.Helpers.ProcessResponse(data, $tag);
                        $.hood.Inline.Reload('#user-notes');
                    });
                };

                $.hood.Alerts.Prompt(
                    "Enter and store a note about this user. These are internal, and are not shown to the user.",
                    "Add a note",
                    addNoteCallback,
                    'textarea'
                );

            },
            Delete: function (e) {
                e.preventDefault();
                $tag = $(this);

                deleteUserNoteCallback = function (isConfirm) {
                    if (isConfirm) {
                        // delete functionality
                        $.post($tag.attr('href'), function (data) {
                            $.hood.Helpers.ProcessResponse(data, $tag);
                            $.hood.Inline.Reload('#user-notes');
                        });
                    } else {
                        swal("Cancelled", "It's all good in the hood!", "error");
                    }
                };

                $.hood.Alerts.Confirm(
                    "Are you sure?",
                    "The note will be removed permanently.",
                    deleteUserNoteCallback
                );

            }
        },
        ToggleRole: function () {
            if ($(this).is(':checked')) {
                $.post('/admin/users/addtorole/', { id: $(this).data('id'), role: $(this).val() }, function (data) {
                    $.hood.Helpers.ProcessResponse(data, $tag);
                });
            } else {
                $.post('/admin/users/removefromrole/', { id: $(this).data('id'), role: $(this).val() }, function (data) {
                    $.hood.Helpers.ProcessResponse(data, $tag);
                });
            }
        }
    }
};
$.hood.Users.Init();

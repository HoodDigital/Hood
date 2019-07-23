if (!$.hood)
    $.hood = {};
$.hood.Users = {
    Init: function () {
        $('body').on('click', '.user-delete', $.hood.Users.Delete);

        $('body').on('click', '.user-reset-password', $.hood.Users.Edit.ResetPassword);

        $('body').on('click', '.user-notes-add', $.hood.Users.Edit.Notes.Add);
        $('body').on('click', '.user-notes-delete', $.hood.Users.Edit.Notes.Delete);

        $('body').on('change', '#user-create-form #GeneratePassword', $.hood.Users.Create.GeneratePassword);
        $('body').on('change', '.user-role-check', $.hood.Users.Edit.ToggleRole);
    },

    Loaded: function (data) {
        $.hood.Loader(false);
    },
    Reload: function (complete) {
        if ($('#user-list').doesExist())
            $.hood.Inline.Reload($('#user-list'), complete);
    },

    Delete: function (e) {
        e.preventDefault();
        $tag = $(this);

        deleteUserCallback = function (isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Users.Reload();
                    if (data.Success) {
                        if ($tag && $tag.data('redirect')) {
                            $.hood.Alerts.Success(`<strong>User deleted, redirecting...</strong><br />Just taking you back to the user list.`);
                            setTimeout(function () {
                                window.location = $tag.data('redirect');
                            }, 1500);
                        }
                    }
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The user will be permanently removed, any site content will be reassigned to the ownership of the site owner, any user content (Forum Topics & Posts) will be deleted from the system and all associated media files.",
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
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Users.Reload();
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
                    $.hood.Helpers.ProcessResponse(data);
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
                        $.hood.Helpers.ProcessResponse(data);
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
                            $.hood.Helpers.ProcessResponse(data);
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
                $.post($(this).data('url'), { role: $(this).val(), add: true }, function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                });
            } else {
                $.post($(this).data('url'), { role: $(this).val(), add: false }, function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                });
            }
        }
    }
};
$(document).ready($.hood.Users.Init);

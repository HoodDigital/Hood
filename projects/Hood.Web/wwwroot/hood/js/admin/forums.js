if (!$.hood)
    $.hood = {};
$.hood.Forums = {
    Init: function () {
        $('body').on('click', '.forum-delete', $.hood.Forums.Delete);
        $('body').on('click', '.forum-archive', $.hood.Forums.Archive);
        $('body').on('click', '.forum-publish', $.hood.Forums.Publish);

        $('body').on('click', '.forum-categories-delete', $.hood.Forums.Categories.Delete);
        $('body').on('change', '.forum-categories-check', $.hood.Forums.Categories.ToggleCategory);

        $('body').on('keyup', '#Slug', function () {
            $('.slug-display').html($(this).val());
        });

        if ($('#edit-forum').doesExist())
            $.hood.Forums.Edit.Init();
    },

    Lists: {
        Forums: {
            Loaded: function (data) {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#forum-list').doesExist())
                    $.hood.Inline.Reload($('#forum-list'), complete);
            }
        },
        Categories: {
            Loaded: function (data) {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#forum-categories-list').doesExist())
                    $.hood.Inline.Reload($('#forum-categories-list'), complete);
            }
        }
    },

    Delete: function (e) {
        e.preventDefault();
        let $tag = $(this);

        let deleteForumCallback = function (isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Forums.Lists.Forums.Reload();
                    if (data.Success) {
                        if ($tag && $tag.data('redirect')) {
                            $.hood.Alerts.Success(`<strong>Forum deleted, redirecting...</strong><br />Just taking you back to the forum list.`);
                            setTimeout(function () {
                                window.location = $tag.data('redirect');
                            }, 1500);
                        }
                    }
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The forum will be permanently removed.",
            "Are you sure?",
            deleteForumCallback,
            'error',
            '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
        );
    },

    Publish: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The item will be visible on the website.",
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
                    $.post('/admin/forums/publish/' + $this.data('id'), null, function (data) {
                        if (data.Success) {
                            swal({
                                title: "Published!",
                                text: "The item has now been published.",
                                timer: 1300,
                                type: "success"
                            });
                            setTimeout(function () {
                                window.location = data.Url;
                            }, 500);
                        } else {
                            swal({
                                title: "Error!",
                                text: "There was a problem publishing the item: " + data.Errors,
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
    Archive: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The item will be hidden from the website.",
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
                    $.post('/admin/forums/archive/' + $this.data('id'), null, function (data) {
                        if (data.Success) {
                            swal({
                                title: "Archived!",
                                text: "The item has now been archived.",
                                timer: 1300,
                                type: "success"
                            });
                            setTimeout(function () {
                                window.location = data.Url;
                            }, 500);
                        } else {
                            swal({
                                title: "Error!",
                                text: "There was a problem archiving the item: " + data.Errors,
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

    Create: function () {
        $('#forum-create-form').find('.datepicker').datetimepicker({
            locale: 'en-gb',
            format: 'L'
        });
        $('#forum-create-form').hoodValidator({
            validationRules: {
                Title: {
                    required: true
                },
                Description: {
                    required: true
                }
            },
            submitButtonTag: $('#forum-create-submit'),
            submitUrl: $('#forum-create-form').attr('action'),
            submitFunction: function (data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Forums.Lists.Forums.Reload();
            }
        });
    },

    Edit: {
        Init: function () {
            $('.datepicker').datetimepicker({
                locale: 'en-gb',
                format: 'L'
            });
            $('.datetimepicker').datetimepicker({
                locale: 'en-gb',
                format: 'LT'
            });
        }
    },

    Categories: {
        Editor: function () {
            $('#forum-categories-edit-form').hoodValidator({
                validationRules: {
                    DisplayName: {
                        required: true
                    },
                    Slug: {
                        required: true
                    }
                },
                submitButtonTag: $('#forum-categories-edit-submit'),
                submitUrl: $('#forum-categories-edit-form').attr('action'),
                submitFunction: function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Forums.Lists.Categories.Reload();
                }
            });
        },
        ToggleCategory: function () {
            $.post($(this).data('url'), { categoryId: $(this).val(), add: $(this).is(':checked') }, function (data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Forums.Lists.Categories.Reload();
            });
        },
        Delete: function (e) {
            e.preventDefault();
            let $tag = $(this);

            let deleteCategoryCallback = function (isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Forums.Lists.Categories.Reload();
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The category will be permanently removed.",
                "Are you sure?",
                deleteCategoryCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
            );
        }
    }
};

$(document).ready($.hood.Forums.Init);

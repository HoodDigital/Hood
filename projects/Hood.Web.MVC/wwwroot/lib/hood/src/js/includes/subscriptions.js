if (!$.hood)
    $.hood = {}
$.hood.Subscriptions = {
    Init: function () {
        $('body').on('click', '.delete-subscription', this.Delete);
        $('body').on('click', '.create-subscription', this.Create.Init);

        if ($('#manage-subscription-list').doesExist())
            this.Manage.Init();
        if ($('#manage-subscriber-list').doesExist())
            this.Subscribers.Init();
        if ($('#edit-subscription').doesExist())
            this.Edit.Init();
    },
    Manage: {
        Init: function () {
            $('#manage-subscription-list').hoodDataList({
                url: '/admin/subscriptions/get',
                params: function () {
                    return {
                        search: $('#manage-subscription-search').val(),
                        sort: $('#manage-subscription-sort').val()
                    };
                },
                pageSize: 12,
                pagers: '.manage-subscription-pager',
                template: '#manage-subscription-template',
                dataBound: function () { },
                refreshOnChange: ".manage-subscription-change",
                refreshOnClick: ".manage-subscription-click",
                serverAction: "GET"
            });
        },
        Filter: function () {
            return {
                search: $('#manage-subscription-search').val(),
                sort: $('#manage-subscription-sort').val()
            };
        },
        Refresh: function () {
            if ($('#manage-subscription-list').doesExist())
                $('#manage-subscription-list').data('hoodDataList').Refresh();
            $.hood.Blades.Reload();
        }
    },
    Subscribers: {
        Init: function () {
            $('#manage-subscriber-list').hoodDataList({
                url: '/admin/subscribers/get',
                params: function () {
                    return {
                        subscriptionId: $('#manage-subscriber-type').val(),
                        search: $('#manage-subscriber-search').val(),
                        sort: $('#manage-subscriber-sort').val()
                    };
                },
                pageSize: 12,
                pagers: '.manage-subscriber-pager',
                template: '#manage-subscriber-template',
                dataBound: function () { },
                refreshOnChange: ".manage-subscriber-change",
                refreshOnClick: ".manage-subscriber-click",
                serverAction: "GET"
            });
        },
        Filter: function () {
            return {
                search: $('#manage-subscriber-search').val(),
                sort: $('#manage-subscriber-sort').val()
            };
        },
        Refresh: function () {
            if ($('#manage-subscriber-list').doesExist())
                $('#manage-subscriber-list').data('hoodDataList').Refresh();
            $.hood.Blades.Reload();
        }
    },
    Delete: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The subscription will be permanently removed.",
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
                $.post('/admin/subscriptions/delete', { id: $this.data('id') }, function (data) {
                    if (data.Success) {
                        if (!$('#manage-subscription-list').doesExist())
                            window.location = '/admin/subscriptions/';
                        $.hood.Subscriptions.Manage.Refresh();
                        $.hood.Blades.Close();
                        swal({
                            title: "Deleted!",
                            text: "The subscription has now been removed from the website.",
                            timer: 1300,
                            type: "success"
                        });
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem deleting the subscription: " + data.Errors,
                            timer: 1300,
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
            e.preventDefault();
            $.hood.Blades.OpenWithLoader('button.create-subscription', '/admin/subscriptions/create/', $.hood.Subscriptions.Create.SetupCreateForm);
        },
        SetupCreateForm: function () {
            $('#create-subscription-form').find('.datepicker').datepicker({
                todayBtn: "linked",
                keyboardNavigation: false,
                forceParse: false,
                calendarWeeks: true,
                autoclose: true,
                orientation: "bottom",
                format: "dd/mm/yyyy",
            });
            $('#create-subscription-form').hoodValidator({
                validationRules: {
                    Title: {
                        required: true
                    },
                    Description: {
                        required: true
                    },
                    Amount: {
                        required: true
                    },
                    Currency: {
                        required: true
                    },
                    Interval: {
                        required: true
                    },
                    IntervalCount: {
                        required: true
                    },
                    Name: {
                        required: true
                    }
                },
                submitButtonTag: $('#create-subscription-submit'),
                submitUrl: '/admin/subscriptions/add',
                submitFunction: function (data) {
                    if (data.Success) {
                        $('#manage-subscription-list').data('hoodDataList').Refresh();
                        swal("Created!", "The subscription has now been created!", "success");
                    } else {
                        swal("Error", "There was a problem creating the subscription:\n\n" + data.Errors, "error");
                    }
                }
            });
        }
    },
    Edit: {
        Init: function () {
            this.LoadEditors('#edit-subscription');
            $.hood.Editor.Init('.edit-subscription-editor');
        },
        Blade: function () {
            this.LoadEditors('#subscription-blade');
            $('#subscription-blade select').each($.hood.Handlers.SelectSetup);
            $('#subscription-blade-form').hoodValidator({
                validationRules: {
                    Title: {
                        required: true
                    },
                    Excerpt: {
                        required: true
                    }
                },                
                submitButtonTag: $('#save-blade'),
                submitUrl: '/admin/subscriptions/save/' + $('#subscription-blade-form').data('id'),
                submitFunction: function (data) {
                    if (data.Succeeded) {
                        $('#manage-subscription-list').data('hoodDataList').Refresh();
                        $.hood.Alerts.Success("Updated.");
                    } else {
                        $.hood.Alerts.Error("There was an error saving.");
                    }
                }
            });
        },
        LoadEditors: function (tag) {
            // Load the url thing if on page editor.
            $(tag).find('.datepicker').datepicker({
                todayBtn: "linked",
                keyboardNavigation: false,
                forceParse: true,
                calendarWeeks: true,
                autoclose: true,
                orientation: "bottom",
                format: "dd/mm/yyyy",
            });

        }
    },
}
$.hood.Subscriptions.Init();

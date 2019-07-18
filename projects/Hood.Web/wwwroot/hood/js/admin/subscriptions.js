if (!$.hood)
    $.hood = {};
$.hood.Subscriptions = {
    Init: function () {
        $('body').on('click', '.subscriptions-plans-delete', $.hood.Subscriptions.Plans.Delete);
        if ($('#subscriptions-plans-edit-form').doesExist())
            $.hood.Subscriptions.Plans.Edit();

        $('body').on('click', '.subscriptions-groups-delete', $.hood.Subscriptions.Groups.Delete);
        if ($('#subscriptions-groups-edit-form').doesExist())
            $.hood.Subscriptions.Groups.Edit();

        if ($('#subscriptions-stripe-edit-form').doesExist())
            $.hood.Subscriptions.Stripe.Edit();
    },

    Lists: {
        Plans: {
            Loaded: function () {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#subscriptions-plans-list').doesExist())
                    $.hood.Inline.Reload($('#subscriptions-plans-list'), complete);
            }
        },
        Stripe: {
            Loaded: function () {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#subscriptions-stripe-list').doesExist())
                    $.hood.Inline.Reload($('#subscriptions-stripe-list'), complete);
            }
        },
        Groups: {
            Loaded: function () {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#subscriptions-groups-list').doesExist())
                    $.hood.Inline.Reload($('#subscriptions-groups-list'), complete);
            }
        },
        Subscribers: {
            Loaded: function () {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#subscriptions-subscribers-list').doesExist())
                    $.hood.Inline.Reload($('#subscriptions-subscribers-list'), complete);
            }
        }
    },

    Plans: {
        Delete: function (e) {
            e.preventDefault();
            $tag = $(this);

            deletePlanCallback = function (isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Subscriptions.Lists.Plans.Reload();
                        if (data.Success) {
                            if ($tag && $tag.data('redirect')) {
                                $.hood.Alerts.Success(`<strong>Plan deleted, redirecting...</strong><br />Just taking you back to the subscription plan list.`);
                                setTimeout(function () {
                                    window.location = $tag.data('redirect');
                                }, 1500);
                            }
                        }
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The plan will be permanently removed.",
                "Are you sure?",
                deleteContentCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
            );
        },
        Create: {
            Init: function () {
                $('#subscriptions-plans-create-form').find('.datepicker').datetimepicker({
                    locale: 'en-gb',
                    format: 'L'
                });
                $('#subscriptions-plans-create-form').hoodValidator({
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
                    submitButtonTag: $('#subscriptions-plans-create-submit'),
                    submitUrl: $('#subscriptions-plans-create-form').attr('action'),
                    submitFunction: function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Subscriptions.Lists.Plans.Reload();
                    }
                });
            }
        },
        Edit: function () {
        }
    },

    Groups: {
        Delete: function (e) {
            e.preventDefault();
            $tag = $(this);

            deleteGroupCallback = function (isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Subscriptions.Lists.Groups.Reload();
                        if (data.Success) {
                            if ($tag && $tag.data('redirect')) {
                                $.hood.Alerts.Success(`<strong>Group deleted, redirecting...</strong><br />Just taking you back to the subscription group list.`);
                                setTimeout(function () {
                                    window.location = $tag.data('redirect');
                                }, 1500);
                            }
                        }
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The group will be permanently removed.",
                "Are you sure?",
                deleteContentCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
            );
        },
        Create: {
            Init: function () {
                $('#subscriptions-groups-create-form').find('.datepicker').datetimepicker({
                    locale: 'en-gb',
                    format: 'L'
                });
                $('#subscriptions-groups-create-form').hoodValidator({
                    validationRules: {
                        DisplayName: {
                            required: true
                        },
                        Slug: {
                            required: true
                        },
                        Body: {
                            required: true
                        }
                    },
                    submitButtonTag: $('#subscriptions-groups-create-submit'),
                    submitUrl: $('#subscriptions-groups-create-form').attr('action'),
                    submitFunction: function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Subscriptions.Lists.Groups.Reload();
                    }
                });
            }
        },
        Edit: function () {
        }
    },

    Stripe: {
        Edit: function () {
        }
    }
};
$(document).ready($.hood.Subscriptions.Init);

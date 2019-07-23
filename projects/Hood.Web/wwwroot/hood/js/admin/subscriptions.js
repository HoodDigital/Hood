if (!$.hood)
    $.hood = {};
$.hood.Subscriptions = {
    Init: function () {
        $('body').on('click', '.subscriptions-plans-delete', $.hood.Subscriptions.Plans.Delete);
        if ($('#subscriptions-plans-edit-form').doesExist())
            $.hood.Subscriptions.Plans.Edit();

        $('body').on('click', '.subscriptions-products-delete', $.hood.Subscriptions.Products.Delete);
        if ($('#subscriptions-products-edit-form').doesExist())
            $.hood.Subscriptions.Products.Edit();

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
        StripeProducts: {
            Loaded: function () {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#subscriptions-stripe-products-list').doesExist())
                    $.hood.Inline.Reload($('#subscriptions-stripe-products-list'), complete);
            }
        },
        Products: {
            Loaded: function () {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#subscriptions-products-list').doesExist())
                    $.hood.Inline.Reload($('#subscriptions-products-list'), complete);
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
                deletePlanCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
            );
        },
        Create: function () {
            $('#subscriptions-plans-create-form').hoodValidator({
                validationRules: {
                    Name: {
                        required: true
                    },
                    Description: {
                        required: true
                    },
                    CreatePrice: {
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

    Products: {
        Delete: function (e) {
            e.preventDefault();
            $tag = $(this);

            deleteProductCallback = function (isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Subscriptions.Lists.Products.Reload();
                        if (data.Success) {
                            if ($tag && $tag.data('redirect')) {
                                $.hood.Alerts.Success(`<strong>Product deleted, redirecting...</strong><br />Just taking you back to the subscription product list.`);
                                setTimeout(function () {
                                    window.location = $tag.data('redirect');
                                }, 1500);
                            }
                        }
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The product will be permanently removed.",
                "Are you sure?",
                deleteProductCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
            );
        },
        Create: function () {
            $('#susbcriptions-products-create-form').find('.datepicker').datetimepicker({
                locale: 'en-gb',
                format: 'L'
            });
            $('#susbcriptions-products-create-form').hoodValidator({
                validationRules: {
                    DisplayName: {
                        required: true
                    }
                },
                submitButtonTag: $('#susbcriptions-products-create-submit'),
                submitUrl: $('#susbcriptions-products-create-form').attr('action'),
                submitFunction: function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Subscriptions.Lists.Products.Reload();
                }
            });
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

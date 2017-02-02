if (!$.hood)
    $.hood = {}
$.hood.Addresses = {
    Init: function () {
        $('body').on('click', '.add-new-address', $.hood.Addresses.New);
        $('body').on('click', '.set-billing', $.hood.Addresses.SetBilling);
        $('body').on('click', '.set-delivery', $.hood.Addresses.SetDelivery);
        $('body').on('click', '.delete-address', $.hood.Addresses.Delete);
        $('body').on('click', '.edit-address', $.hood.Addresses.Edit);
        if ($(".address-select").length > 0) {
            $.hood.Addresses.Refresh();
        }
    },
    New: function (e) {
        $(this).data('temp', $(this).html());
        $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
        $.hood.Modals.Open('/account/addresses/create', null, '', $.hood.Addresses.PostLoad);
    },
    Edit: function () {
        $(this).data('temp', $(this).html());
        $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
        $.hood.Modals.Open('/account/addresses/edit', { id: $(this).data('id') }, '', $.hood.Addresses.PostLoad);
    },
    Refresh: function () {
        $.hood.Inline.Reload('.address-list');
        // reload any selectlists that contain billing or delivery addresses (checkouts etc.)
        $.get('/admin/users/getaddresses', null, function (data) {
            $('.address-select').empty().append($('<option>', { value: '', text: '--- Choose an address ---' }));
            for (var i in data) {
                var id = data[i].Id;
                var address = data[i].FullAddress;
                $('.address-select').append($('<option>', { value: id, text: address }));
            }
        });
    },
    PostLoad: function () {
        $.hood.Handlers.Addresses.InitAutocomplete();
        $('.btn-loading').each(function () {
            $(this).removeClass('btn-loading').html($(this).data('temp'));
        });
        $('#address-form').hoodValidator({
            validationRules: {
                Number: {
                    required: true
                },
                Address1: {
                    required: true
                },
                City: {
                    required: true
                },
                County: {
                    required: true
                },
                Postcode: {
                    required: true
                },
                Country: {
                    required: true
                }
            },
            submitButtonTag: $('#save-address'),
            submitUrl: $('#address-form').attr('action'),
            submitFunction: function (data) {
                if (data.Success) {
                    $.hood.Addresses.Refresh();
                    $.hood.Modals.Close('#add-address-modal');
                } else {
                    $.hood.Alerts.Error(data.Errors, "Error Saving Address!");
                }
            }
        });
    },
    SetBilling: function (e) {
        $(this).data('temp', $(this).html());
        $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
        $.post('/account/addresses/setbilling?id=' + $(this).data('id'), null, function (data) {
            $('.btn-loading').each(function () {
                $(this).removeClass('btn-loading').html($(this).data('temp'));
            });
            if (data.success) {
                $.hood.Alerts.Success("Your billing address has been updated.", "Billing Address Updated!");
                $.hood.Addresses.Refresh();
            } else {
                $.hood.Alerts.Error("Couldn't update your billing address...", "Couldn't Update Billing Address!");
            }
        });
        e.preventDefault();
    },
    SetDelivery: function (e) {
        $(this).data('temp', $(this).html());
        $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
        $.post('/account/addresses/setdelivery?id=' + $(this).data('id'), function (data) {
            $('.btn-loading').each(function () {
                $(this).removeClass('btn-loading').html($(this).data('temp'));
            });
            if (data.success) {
                $.hood.Alerts.Success("Your delivery address has been updated.", "Delivery Address Updated!");
                $.hood.Addresses.Refresh();
            } else {
                $.hood.Alerts.Error("Couldn't update your delivery address...", "Couldn't Update Delivery Address!");
            }
        });
        e.preventDefault();
    },
    Delete: function (e) {
        $(this).data('temp', $(this).html());
        $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
        $.post('/account/addresses/delete', { id: $(this).data('id') }, function (data) {
            $('.btn-loading').each(function () {
                $(this).removeClass('btn-loading').html($(this).data('temp'));
            });
            if (data.success) {
                $.hood.Alerts.Success("Your address has been deleted.", "Address Deleted!");
                $.hood.Addresses.Refresh();
            } else {
                $.hood.Alerts.Error("Couldn't delete your address, it may be in use as your billing or delivery address...", "Couldn't Delete Address!");
            }
        });
        e.preventDefault();
    }
}
$.hood.Addresses.Init();
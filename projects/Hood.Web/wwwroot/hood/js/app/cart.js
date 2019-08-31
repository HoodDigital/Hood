if (!$.hood)
    $.hood = {}
$.hood.Cart = {
    Init: function () {
        $('#CheckoutProgress').fadeOut();
        $('body').on('click', '.cart-add', $.hood.Cart.Add);
        $('body').on('click', '.cart-clear', $.hood.Cart.Clear);
        $('body').on('click', '.cart-update', $.hood.Cart.Update);
        $('body').on('click', '.cart-remove', $.hood.Cart.Remove);
        $('body').on('change', '.delivery-select', function () {
            $('#DeliveryAddressId').val($(this).val());
        });
        $('body').on('change', '.billing-select', function () {
            $('#BillingAddressId').val($(this).val());
        });
    },
    Add: function (e) {
        $(this).data('temp', $(this).html());
        $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
        $.post('/cart/add', { productID: $(this).data('id'), qty: 1 }, function (data) {
            $('.btn-loading').each(function () {
                $(this).removeClass('btn-loading').html($(this).data('temp'));
            });
            if (data.Success) {
                $.hood.Alerts.Success("Added to your cart!");
                $.hood.Cart.ReloadMiniCart();
            } else {
                $.hood.Alerts.Error("Couldn't add to your cart...");
                $.hood.Cart.ReloadMiniCart();
            }
        });
        e.preventDefault();
    },
    Update: function (e) {
        $.post('/cart/update', { productID: $(this).data('id'), qty: $(this).data('change') }, function (data) {
            if (data.Success) {
                $.hood.Alerts.Success("Your shopping cart has been updated!");
                $.hood.Cart.ReloadMiniCart();
            } else {
                $.hood.Alerts.Error("Couldn't update your cart...");
            }
        });
        e.preventDefault();
    },
    Remove: function (e) {
        $.post('/cart/remove', { productID: $(this).data('id') }, function (data) {
            if (data.Success) {
                $.hood.Alerts.Success("Your shopping cart has been updated!");
                $.hood.Cart.ReloadMiniCart();
            } else {
                $.hood.Alerts.Error("Couldn't update your cart...");
            }
        });
        e.preventDefault();
    },
    Clear: function (e) {
        $.post('/cart/clear', {}, function (data) {
            if (data.Success) {
                $.hood.Alerts.Success("Your shopping cart has been cleared!");
                $.hood.Cart.ReloadMiniCart();
            } else {
                $.hood.Alerts.Error("Couldn't clear your cart...");
            }
        });
        e.preventDefault();
    },
    ReloadMiniCart: function (e) {
        $('.hood-inline-cart').each(function () {
            $.hood.Inline.Reload(this);
        });
    }
}
$.hood.Cart.Init();

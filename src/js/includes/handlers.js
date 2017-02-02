if (!$.hood)
    $.hood = {};
$.hood.Handlers = {
    Init: function () {
        // Click to select boxes
        $('body').on('click', '.select-text', $.hood.Handlers.SelectTextContent);
        $('body').on('click', '.btn.click-select[data-target][data-value]', $.hood.Handlers.ClickSelectButton);
        $('body').on('click', '.click-select[data-target][data-value]', $.hood.Handlers.ClickSelect);
        $('body').on('click', '.slide-link', $.hood.Handlers.SlideToAnchor);
        $('body').on('change', 'input[type=checkbox][data-input]', $.hood.Handlers.CheckboxChange);

        $('select[data-selected]').each($.hood.Handlers.SelectSetup);
        // date/time meta editor
        $('body').on('change', '.inline-date', $.hood.Handlers.DateChange);

    },
    Addresses: {
        AddressForm: {
            QuickName: '{route}, {postal_town}, {postal_code}',
            Number: '{street_number}',
            Address1: '{route}',
            Address2: '{locality}',
            City: '{postal_town}',
            County: '{administrative_area_level_2}',
            Country: '{country}',
            Postcode: '{postal_code}'
        },
        placeSearch: null,
        place: null,
        autocomplete: null,
        InitAutocomplete: function () {
            this.autocomplete = new google.maps.places.Autocomplete(document.getElementById('address-autocomplete'), { types: ['geocode'] });
            this.autocomplete.addListener('place_changed', $.hood.Handlers.Addresses.FillInAddress);
        },
        FillInAddress: function () {
            // Get the place details from the autocomplete object.
            $.hood.Handlers.Addresses.place = $.hood.Handlers.Addresses.autocomplete.getPlace();

            for (var component in $.hood.Handlers.Addresses.AddressForm) {
                if ($('#' + component).doesExist()) {
                    $('#' + component).val('');
                    newVal = $.hood.Handlers.Addresses.AddressForm[component];
                    placeholders = $.getPlaceholders(newVal);
                    for (var placeholder in placeholders) {
                        newVal = newVal.replace("{" + placeholders[placeholder] + "}", $.hood.Handlers.Addresses.GetValueFromAddressComponents(placeholders[placeholder]));
                    }
                    if (!newVal.contains('undefined'))
                        $('#' + component).val(newVal);
                }
            }
            $('#Latitude').val($.hood.Handlers.Addresses.place.geometry.location.lat);
            $('#Longitude').val($.hood.Handlers.Addresses.place.geometry.location.lng);
        },
        GetValueFromAddressComponents: function (key) {

            // Get each component of the address from the place details
            // and fill the corresponding field on the form.
            for (var i = 0; i < $.hood.Handlers.Addresses.place.address_components.length; i++) {
                var addressType = $.hood.Handlers.Addresses.place.address_components[i].types[0];
                if (addressType == key) {
                    return $.hood.Handlers.Addresses.place.address_components[i].long_name;
                }
            }

        },
        // Bias the autocomplete object to the user's geographical location,
        // as supplied by the browser's 'navigator.geolocation' object. 
        GeoLocate: function () {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                    var geolocation = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude
                    };
                    var circle = new google.maps.Circle({
                        center: geolocation,
                        radius: position.coords.accuracy
                    });
                    $.hood.Handlers.Addresses.autocomplete.setBounds(circle.getBounds());
                });
            }
        }
    },
    DateChange: function (e) {
        // update the date element attached to the field's attach
        $field = $(this).parents('.hood-date').find('.date-output');
        date = $field.parents('.hood-date').find('.date-value').val();
        pattern = /^([0-9]{2})\/([0-9]{2})\/([0-9]{4})$/;
        if (!pattern.test(date))
            date = "01/01/2001";
        hour = $field.parents('.hood-date').find('.hour-value').val();
        if (!$.isNumeric(hour))
            hour = "00";
        minute = $field.parents('.hood-date').find('.minute-value').val();
        if (!$.isNumeric(minute))
            minute = "00";
        $field.val(date + " " + hour + ":" + minute + ":00");
        $field.attr("value", date + " " + hour + ":" + minute + ":00");
    },
    CheckboxChange: function (e) {
        // when i change - create an array, with any other checked of the same data-input checkboxes. and add to the data-input referenced tag.
        var items = new Array();
        $('input[data-input="' + $(this).data('input') + '"]').each(function () {
            if ($(this).is(":checked"))
                items.push($(this).val());
        });
        id = '#' + $(this).data('input');
        vals = JSON.stringify(items);
        $(id).val(vals);
    },
    SelectSetup: function () {
        sel = $(this).data('selected');
        if ($(this).data('selected') !== 'undefined' && $(this).data('selected') !== '') {
            selected = String($(this).data('selected'));
            $(this).val(selected);
        }
    },
    ClickSelect: function () {
        var $this = $(this);
        targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
    },
    ClickSelectButton: function () {
        var $this = $(this);
        targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
        $('.click-select[data-target="' + $this.data('target') + '"]').html($this.data('temp')).removeClass('active');
        $this.data('temp', $this.html()).html('Selected').addClass('active');
    },
    SelectTextContent: function () {
        var $this = $(this);
        $this.select();
        // Work around Chrome's little problem
        $this.mouseup(function () {
            // Prevent further mouseup intervention
            $this.unbind("mouseup");
            return false;
        });
    },
    SlideToAnchor: function () {
        var scrollTop = $('body').scrollTop();
        var top = $($.attr(this, 'href')).offset().top;

        $('html, body').animate({
            scrollTop: top
        }, Math.abs(top - scrollTop));
        return false;
    }
};
$.hood.Handlers.Init();
function initGoogleMapsAutocomplete() {
    if ($('#address-autocomplete').doesExist()) {
        $.hood.Handlers.Addresses.InitAutocomplete();
    }
}
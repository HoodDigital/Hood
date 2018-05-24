if (!$.hood)
    $.hood = {};
$.hood.Google = {
    Maps: function () {
        $('.google-map').each(function () {
            var myLatLng = { lat: $(this).data('lat'), lng: $(this).data('long') };

            var map = new google.maps.Map(this, {
                zoom: $(this).data('zoom') || 15,
                center: myLatLng,
                scrollwheel: false
            });

            var marker = new google.maps.Marker({
                position: myLatLng,
                map: map,
                title: $(this).data('marker')
            });

            $(window).on('resize', function () {

                google.maps.event.trigger(map, 'resize');
            });
            google.maps.event.trigger(map, 'resize');
        });
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
            this.autocomplete.addListener('place_changed', $.hood.Google.Addresses.FillInAddress);
        },
        FillInAddress: function () {
            // Get the place details from the autocomplete object.
            $.hood.Google.Addresses.place = $.hood.Google.Addresses.autocomplete.getPlace();

            for (var component in $.hood.Google.Addresses.AddressForm) {
                if ($('#' + component).doesExist()) {
                    $('#' + component).val('');
                    newVal = $.hood.Google.Addresses.AddressForm[component];
                    placeholders = $.getPlaceholders(newVal);
                    for (var placeholder in placeholders) {
                        newVal = newVal.replace("{" + placeholders[placeholder] + "}", $.hood.Google.Addresses.GetValueFromAddressComponents(placeholders[placeholder]));
                    }
                    if (!newVal.contains('undefined'))
                        $('#' + component).val(newVal);
                }
            }
            $('#Latitude').val($.hood.Google.Addresses.place.geometry.location.lat);
            $('#Longitude').val($.hood.Google.Addresses.place.geometry.location.lng);
        },
        GetValueFromAddressComponents: function (key) {

            // Get each component of the address from the place details
            // and fill the corresponding field on the form.
            for (var i = 0; i < $.hood.Google.Addresses.place.address_components.length; i++) {
                var addressType = $.hood.Google.Addresses.place.address_components[i].types[0];
                if (addressType == key) {
                    return $.hood.Google.Addresses.place.address_components[i].long_name;
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
                    $.hood.Google.Addresses.autocomplete.setBounds(circle.getBounds());
                });
            }
        }

    },
    ClusteredMap: function () {

        var mapElement = $('#clustered-map');
        if (!mapElement.length)
            return;

        var scrollWheel = false;
        if (mapElement.attr('data-scrollwheel'))
            scrollWheel = mapElement.data('scrollwheel');

        var zoom = false;
        if (mapElement.attr('data-zoom'))
            zoom = mapElement.data('zoom');

        var map = new google.maps.Map(document.getElementById('clustered-map'), {
            zoom: zoom,
            scrollwheel: scrollWheel,
            center: { lat: mapElement.data('lat'), lng: mapElement.data('long') }
        });

        // Create an array  of alphabetical characters used to label the markers.
        var labels = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';

        var locations = mapElement.data('locations');
        var clickFunction = mapElement.data('click')
        // Add some markers to the map.
        // Note: The code uses the JavaScript Array.prototype.map() method to
        // create an array of markers based on a given "locations" array.
        // The map() method here has nothing to do with the Google Maps API.
        var markers = locations.map(function (location, i) {
            var marker = new google.maps.Marker({
                position: new google.maps.LatLng(location.Latitude, location.Longitude),
                label: location.Title
            });
            marker.addListener('click', function () {
                eval(clickFunction + "(" + location.AssociatedId + ")");
            });
            return marker;
        });

        // Add a marker clusterer to manage the markers.
        var markerCluster = new MarkerClusterer(map, markers, { imagePath: '/lib/hood/images/maps/m' });
    }
};
function initGoogleMapsComplete() {
    if ($('#address-autocomplete').length > 0) {
        $.hood.Google.Addresses.InitAutocomplete();
    }
    $.hood.Google.Maps();
    $.hood.Google.ClusteredMap();
    // try calling initMaps, this may have been added to pages.
    try {initMap();} catch (ex) {}
}
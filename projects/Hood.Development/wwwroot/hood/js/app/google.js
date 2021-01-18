if (!$.hood)
    $.hood = {};
$.hood.Google = {
    Maps: function () {
        $('.google-map').each(function () {
            var myLatLng = new google.maps.LatLng($(this).data('lat'), $(this).data('long'));

            console.log('Loading map at: ' + $(this).data('lat') + ', ' + $(this).data('long'));

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

            $(window).resize(function () {
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
                    let newVal = $.hood.Google.Addresses.AddressForm[component];
                    let  placeholders = $.hood.Google.GetPlaceholders(newVal);
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
                if (addressType === key) {
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
            maxZoom: 18,
            scrollwheel: scrollWheel,
            center: { lat: mapElement.data('lat'), lng: mapElement.data('long') }
        });

        var oms = new OverlappingMarkerSpiderfier(map, {
            markersWontMove: true,
            markersWontHide: true,
            basicFormatEvents: true,
            circleFootSeparation: 80,
            spiralFootSeparation: 80
        });

        var iconSize = new google.maps.Size(30, 41);

        // Create an array  of alphabetical characters used to label the markers.
        var labels = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';

        var locations = mapElement.data('locations');
        var clickFunction = mapElement.data('click');
        // Add some markers to the map.
        // Note: The code uses the JavaScript Array.prototype.map() method to
        // create an array of markers based on a given "locations" array.
        // The map() method here has nothing to do with the Google Maps API.
        var markers = locations.map(function (location, i) {
            var marker = new google.maps.Marker({
                position: new google.maps.LatLng(location.Latitude, location.Longitude),
                optimized: !isIE  // makes SVG icons work in IE
            });
            marker.setIcon({
                url: mapElement.data('marker'),
                size: iconSize,
                scaledSize: iconSize  // makes SVG icons work in IE
            });
            google.maps.event.addListener(marker, 'spider_click', function (e) {  // 'spider_click', not plain 'click'
                func = clickFunction + "(" + location.AssociatedId + ")";
                eval(func);
            });

            var icon1 = mapElement.data('marker');
            var icon2 = mapElement.data('highlight');

            var info = '<div class="google-popup">' +
                '<img src="' + location.ImageUrl + '" class="map-image" />' +
                '<p>' + location.Address1 + ', ' + location.Postcode + '</p>' +
                '<p>' + location.Description + '</p>' +
                '</div>';

            var infowindow = new google.maps.InfoWindow({
                content: info
            });

            google.maps.event.addListener(marker, 'mouseover', function () {
                infowindow.open(map, this);
                marker.setIcon(icon2);
            });
            google.maps.event.addListener(marker, 'mouseout', function () {
                infowindow.close();
                marker.setIcon(icon1);
            });

            oms.addMarker(marker);

            return marker;
        });

        // Add a marker clusterer to manage the markers.
        var markerCluster = new MarkerClusterer(map, markers, {
            imagePath: 'https://cdn.jsdelivr.net/npm/hoodcms@4.0.1/images/maps/m',
            maxZoom: 15
        });


    },
    GetPlaceholders: function (str) {
        var regex = /\{(\w+)\}/g;
        var result = [];
        while (match === regex.exec(str)) {
            result.push(match[1]);
        }
        return result;
    }
};
function initGoogleMapsComplete() {
    if ($('#address-autocomplete').length > 0) {
        $.hood.Google.Addresses.InitAutocomplete();
    }
    $.hood.Google.Maps();
    $.hood.Google.ClusteredMap();
    // try calling initMaps, this may have been added to pages.
    try { initMap(); } catch (ex) { $.noop(); }
}
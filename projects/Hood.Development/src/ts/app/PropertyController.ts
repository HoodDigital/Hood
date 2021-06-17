import 'google.maps';
import { Alerts } from "../core/Alerts";
import { DataList } from "../core/DataList";

declare global {
    namespace google.maps {
        interface Marker {
            info: string;
        }
    }
}

export class PropertyController {
    constructor() {
        this.initList();
    }

    element: HTMLElement;
    list: DataList;
    mapListElement: HTMLElement;
    mapList: DataList;
    map: google.maps.Map = null;
    center: google.maps.LatLngLiteral = { lat: 30, lng: -110 };
    mapElement: HTMLElement;
    markers: any[];

    initList() {

        this.element = document.getElementById('property-list');
        if (!this.element) {
            return;
        }

        this.list = new DataList(this.element, {
            onComplete: function (this: PropertyController, data: string, sender: HTMLElement = null) {

                Alerts.log('Finished loading property list.', 'info');

            }.bind(this)
        });

    }

    initMapList() {

        this.mapListElement = document.getElementById('property-map-list');
        if (!this.mapElement) {
            return;
        }

        this.mapList = new DataList(this.mapListElement, {
            onComplete: function (this: PropertyController, data: string, sender: HTMLElement = null) {

                Alerts.log('Finished loading map list.', 'info');
                this.reloadMarkers();

            }.bind(this)
        });
    }

    initMap(mapElementId: string = 'property-map') {

        this.mapElement = document.getElementById(mapElementId);
        if (!this.mapElement) {
            return;
        }

        this.center = { lat: +this.mapElement.dataset.lat, lng: +this.mapElement.dataset.long };

        this.map = new google.maps.Map(this.mapElement, {
            zoom: +this.mapElement.dataset.zoom || 15,
            center: this.center,
            scrollwheel: false
        });

        $(window).resize(function (this: PropertyController) {
            google.maps.event.trigger(this.map, 'resize');
        }.bind(this));

        google.maps.event.trigger(this.map, 'resize');

        this.initMapList();

    }

    reloadMarkers() {

        var infowindow: google.maps.InfoWindow = null;

        if (!this.mapElement) {
            return;
        }

        var map = this.map;

        if (this.markers) {
            for (var i = 0; i < this.markers.length; i++) {
                this.markers[i].setMap(null);
            }
        }

        this.markers = [];

        var locations = $("#property-map-locations").data('locations');

        locations.map(function (this: PropertyController, location: any, i: number) {

            let marker = new google.maps.Marker({
                position: new google.maps.LatLng(+location.Latitude, +location.Longitude),
                map: this.map,
                optimized: true // makes SVG icons work in IE
            });



            //marker.setIcon({
            //    url: '/images/marker.png',
            //    size: new google.maps.Size(30, 41),
            //    scaledSize: new google.maps.Size(30, 41)
            //});

            marker.info = `<div class="card border-0" style="max-width:300px">
    <div style="background-image:url(${location.ImageUrl})" class="rounded img-full img img-wide"></div>
    <div class="card-body border-0">
        <p style="overflow: hidden;text-overflow: ellipsis;white-space: nowrap;">
            <strong>${location.Address1}, ${location.Postcode}</strong>
        </p>
        <p>${location.Description}</p>
        <a href="${location.MarkerUrl}" class="btn btn-block btn-primary">Find out more...</a>
    </div>
</div>`;

            google.maps.event.addListener(marker, 'click', function (this: google.maps.Marker) {

                if (infowindow) {
                    infowindow.close();
                }
                infowindow = new google.maps.InfoWindow({
                    content: this.info
                });
                infowindow.open(map, this);

            }.bind(this));

            this.markers.push(marker);

        }.bind(this));
    }
}

/// <reference types="google.maps" />

import { Alerts } from "../core/Alerts";
import { DataList } from "../core/DataList";

declare global {
    namespace google.maps {
        interface Marker {
            info: string;
        }
    }
}

export interface PropertyServiceOptions {

    listElementId?: string;
    mapListElementId?: string;
    mapElementId?: string;
    
    /**
     * Called before the data is fetched.
     */
    onListLoad?: (sender?: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the list. Must return the data back to datalist to render.
     */
    onListRender?: (html: string, sender?: HTMLElement) => string;
    /**
     * Called before the data is fetched.
     */
    onMapLoad?: (data: string, sender?: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the list. Must return the data back to datalist to render.
     */
    onMapRender?: (sender?: HTMLElement) => string;

}

export class PropertyService {
    options: PropertyServiceOptions = {
        listElementId: 'property-list',
        mapListElementId: 'property-map-list',
        mapElementId: 'property-map'
    };

    constructor(options?: PropertyServiceOptions) {

        this.options = { ...this.options, ...options };

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

        this.element = document.getElementById(this.options.listElementId);
        if (!this.element) {
            return;
        }

        this.list = new DataList(this.element, {
            onLoad: function (this: PropertyService, sender: HTMLElement = null) {

                if (this.options.onListLoad) {
                    this.options.onListLoad(sender);
                }

            }.bind(this),
            onComplete: function (this: PropertyService, data: string, sender: HTMLElement = null) {

                if (this.options.onListRender) {
                    this.options.onListRender(data, sender);
                }
                Alerts.log('Finished loading property list.', 'info');

            }.bind(this)
        });

    }

    initMapList() {

        this.mapListElement = document.getElementById(this.options.mapListElementId);
        if (!this.mapElement) {
            return;
        }

        this.mapList = new DataList(this.mapListElement, {
            onComplete: function (this: PropertyService, data: string, sender: HTMLElement = null) {

                if (this.options.onMapLoad) {
                    this.options.onMapLoad(data, sender);
                }
                Alerts.log('Finished loading map list.', 'info');
                this.reloadMarkers();

            }.bind(this)
        });
    }

    initMap() {

        this.mapElement = document.getElementById(this.options.mapElementId);
        if (!this.mapElement) {
            return;
        }

        this.center = { lat: +this.mapElement.dataset.lat, lng: +this.mapElement.dataset.long };

        this.map = new google.maps.Map(this.mapElement, {
            zoom: +this.mapElement.dataset.zoom || 15,
            center: this.center,
            scrollwheel: false
        });

        $(window).resize(function (this: PropertyService) {
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

        locations.map(function (this: PropertyService, location: any, i: number) {

            let marker = new google.maps.Marker({
                position: new google.maps.LatLng(+location.Latitude, +location.Longitude),
                map: this.map,
                optimized: true // makes SVG icons work in IE
            });

            if (this.mapElement.dataset.marker) {
                marker.setIcon(this.mapElement.dataset.marker);
            }

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

            marker.addListener("click", () => {
                if (infowindow) {
                    infowindow.close();
                }
                infowindow = new google.maps.InfoWindow({
                    content: marker.info
                });
                infowindow.open({
                    anchor: marker,
                    map,
                    shouldFocus: false,
                });
            });

            this.markers.push(marker);

        }.bind(this));


        if (this.options.onMapRender) {
            this.options.onMapRender();
        }
    }
}

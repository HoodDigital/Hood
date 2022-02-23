/// <reference types="google.maps" />

import { Response } from "./Response";
import { Alerts } from "./Alerts";
import { Handlers } from "./Handlers";
import { Uploader } from "./Uploader";
import { Validator } from "./Validator";

declare global {
    interface Hood {
        alerts: Alerts;
        uploader: Uploader;
        handlers: Handlers;
    }

    interface Window {
        hood: Hood;
    }
}

/**
 * Base class for extending a Hood CMS website, ensure you call HoodApi.initialise() to setup loaders and contact form defaults.
 */
export class HoodApi {
    alerts: Alerts = new Alerts();
    handlers: Handlers = new Handlers();
    uploader: Uploader = new Uploader();

    constructor() {
    }

    // Initialise Hood CMS site defaults, can be overridden or individual setup items can be called instead of the initialise function. 
    initialise() {
        // Initialise loaders (default, adds loading to body tag)
        this.setupLoaders();

        // Hook up default handlers.
        this.handlers.initDefaultHandlers();

        // Init hood contact forms. 
        this.initContactForms();
    }

    /**
     * Default Hood CMS loaders, can be used however, this simply adds a "loading" class to the body tag on show/hide.
     */
    setupLoaders(): void {
        $('body').on('loader-show', function () { document.body.classList.add('loading') })
        $('body').on('loader-hide', function () { document.body.classList.remove('loading') })
    }

    /**
     * Default initialisation function for Google Maps, should be called as the callback from the Google Maps API script tag.
     */
    initGoogleMaps(tag: string = '.google-map') {

        $(tag).each(function () {

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

            $(window).on('resize', function () {
                google.maps.event.trigger(map, 'resize');
            });
            google.maps.event.trigger(map, 'resize');

        });

    }
    
    /**
     * Initialisation function for contact forms on the site, will add validator, and submit/functionality to any forms matching the given tag selector string.
     */
    initContactForms(tag: string = '.contact-form') {

        let $form = $(tag);

        $form.find('.thank-you').hide();
        $form.find('.form-content').show();

        let form: HTMLFormElement = $(tag)[0] as HTMLFormElement;
        new Validator(form, {
            onComplete: function (this: HoodApi, response: Response) {
                if (response.success) {

                    if ($form.attr('data-redirect'))
                        window.location.href = $form.attr('data-redirect');

                    if ($form.attr('data-alert-message'))
                        Alerts.success($form.attr('data-alert-message'));

                    $form.find('.form').hide();
                    $form.find('.thank-you').show();
                } else {
                    if ($form.attr('data-alert-error'))
                        Alerts.error($form.attr('data-alert-error'));
                    else {
                        console.error(response.errors);
                        Alerts.error("There are errors on the form, please check your answers and try again.");
                    }
                }

            }.bind(this)
        });

    }

}
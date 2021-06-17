import 'google.maps'

import { BaseController } from "./core/HoodController";
import { PropertyController } from './app/PropertyController';
import { Validator } from './core/Validator';
import { Response } from './core/Response';
import { Alerts } from './core/Alerts';

export * from "./hood";

export class App extends BaseController {
    propertyController: PropertyController;

    constructor() {
        super();

        // Hook up default handlers.
        this.handlers.initDefaultHandlers();


        // Admin Controllers
        this.propertyController = new PropertyController();
    }

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

            $(window).resize(function () {
                google.maps.event.trigger(map, 'resize');
            });
            google.maps.event.trigger(map, 'resize');

        });

    }

    initContactForms(tag: string = '.contact-form') {

        let $form = $(tag);

        $form.find('.thank-you').hide();
        $form.find('.form-content').show();
        $('body').on('submit', tag, this.submitContactForm);

        let form: HTMLFormElement = $(tag)[0] as HTMLFormElement;
        new Validator(form, {
            onComplete: function (this: App, response: Response) {

                Response.process(response, 5000);

                if (response.success) {

                    if ($form.attr('data-redirect'))
                        window.location.href = $form.attr('data-redirect');

                    if ($form.attr('data-alert-message'))
                        Alerts.success($form.attr('data-alert-message'), "Success");

                    $form.find('.form').hide();
                    $form.find('.thank-you').show();
                } else {
                    if ($form.attr('data-alert-error'))
                        Alerts.error($form.attr('data-alert-error'), "Error");
                    else
                        Alerts.error("There was an error sending the message: " + response.errors, "Error");
                }

            }.bind(this)
        });

    }

    submitContactForm(this: HTMLElement, e: JQuery.SubmitEvent) {
        e.preventDefault();

        $(this).addClass('loading');

        var $form: JQuery<HTMLElement> = $(this);

        if ($form.valid()) {
            $.post($form.attr('action'), $form.serialize(), function (data) {
            });
        }
        return false;

    }

}

export var app = new App();

import 'jquery-slimscroll';

export * from "./hood";
import { Uploader } from "./core/Uploader";
import { Editors } from "./core/Editors";
import { HomeController } from "./admin/HomeController";
import { Loader } from "./core/Loader";
import { Alerts } from './hood';
import { DataList } from "./core/DataList";

import { HoodController } from './core/HoodController';
import { MediaController } from './admin/MediaController';

class Admin extends HoodController {
    home: HomeController;
    media: MediaController;

    constructor() {
        super();

        this.editors = new Editors();
        this.home = new HomeController();
        this.media = new MediaController();

        let contentListDiv = document.getElementById('content-list');
        let contentList = new DataList(contentListDiv, {
            onLoad: (sender: HTMLElement) => {
                Alerts.success('Commencing content list fetch.');
            },
            onError: (sender: HTMLElement, data: string) => {
                Alerts.error('Error loading content list.');
            },
            onRender: (sender: HTMLElement, data: string) => {
                Alerts.success('Fetched content list data.');
                return data;
            },
            onComplete: (sender: HTMLElement, data: string) => {
                Alerts.success('Finished loading content list.');
            }
        });





        $('.restrict-to-slug').restrictToSlug();
        $('.restrict-to-page-slug').restrictToPageSlug();
        $('.restrict-to-meta-slug').restrictToMetaSlug();
        $('.character-counter').characterCounter();
        $('.character-counter').trigger('change');
        $('.warning-alert').warningAlert();

        $('.mobile-sidebar-toggle').on('click', function () {
            $('nav.sidebar').toggleClass('open');
        });
         
        $('.right-sidebar-toggle').on('click', function () {
            $('#right-sidebar').toggleClass('sidebar-open');
        });

        $(".alert.auto-dismiss").fadeTo(5000, 500).slideUp(500, function () {
            $(".alert.auto-dismiss").slideUp(500);
        });

        $('.sidebar-scroll').slimScroll({
            height: '100%',
            railOpacity: 0.4,
            wheelStep: 10
        });

    }
}

new Admin();

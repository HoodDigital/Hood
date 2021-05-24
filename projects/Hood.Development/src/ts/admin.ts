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
import { MediaModal } from './core/Media';
import { ContentController } from './admin/ContentController';
import * as bootstrap from 'bootstrap';
import { PropertyController } from './admin/PropertyController';

class Admin extends HoodController {

    mediaModal: MediaModal;

    home: HomeController;
    media: MediaController;
    content: ContentController;
    property: PropertyController;

    constructor() {
        super();

        // Admin Services
        this.mediaModal = new MediaModal();
        this.mediaModal.initUploaders();

        // Admin Controllers
        this.home = new HomeController();
        this.media = new MediaController();
        this.content = new ContentController();
        this.property = new PropertyController();

        if ($('#page-tabs').length > 0) {
            this.CheckAndLoadTabs('#page-tabs');
        }

        // Admin Handlers
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

    CheckAndLoadTabs(tag: string): void  {
        $(tag + ' a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
            localStorage.setItem('tab-' + $(tag).data('hoodTabs'), $(e.target).attr('href'));
        });

        let activeTab = localStorage.getItem('tab-' + $(tag).data('hoodTabs'));
        let triggerEl = $(tag + ' a[data-bs-toggle="tab"]')[0];
        if (activeTab) {
            triggerEl = document.querySelector(tag + ' a[href="' + activeTab + '"]')
        }
        let tabTrigger = new bootstrap.Tab(triggerEl);
        tabTrigger.show();
    }
}

new Admin();

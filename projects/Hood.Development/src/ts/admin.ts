import 'jquery-slimscroll';

export * from "./hood";
import * as bootstrap from 'bootstrap';

import { BaseController } from './core/HoodController';

import { HomeController } from "./admin/HomeController";
import { MediaController } from './admin/MediaController';
import { ContentController } from './admin/ContentController';
import { PropertyController } from './admin/PropertyController';
import { UsersController } from './admin/UsersController';
import { ThemesController } from './admin/ThemesController';

class Admin extends BaseController {

    home: HomeController;
    media: MediaController;
    content: ContentController;
    property: PropertyController;
    themes: ThemesController;
    users: UsersController;

    constructor() {
        super();

        // Admin Controllers
        this.home = new HomeController();
        this.media = new MediaController();
        this.content = new ContentController();
        this.property = new PropertyController();
        this.themes = new ThemesController();
        this.users = new UsersController();

        if ($('#page-tabs').length > 0) {
            this.checkAndLoadTabs('#page-tabs');
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

    checkAndLoadTabs(tag: string): void  {
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

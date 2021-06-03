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
import { ContentTypeController } from './admin/ContentTypeController';
import { LogsController } from './admin/LogsController';
import { PropertyImporter } from './admin/PropertyImporter';

class Admin extends BaseController {

    home: HomeController;
    media: MediaController;
    content: ContentController;
    contentType: ContentTypeController;
    logs: LogsController;
    property: PropertyController;
    propertyImporter: PropertyImporter;
    themes: ThemesController;
    users: UsersController;

    constructor() {
        super();

        // Admin Controllers
        this.home = new HomeController();
        this.media = new MediaController();
        this.content = new ContentController();
        this.contentType = new ContentTypeController();
        this.logs = new LogsController();
        this.property = new PropertyController();
        this.propertyImporter = new PropertyImporter();
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
            let store: any = JSON.parse(localStorage.getItem('tabs-' + tag)) || {};
            store['tab-' + $(tag).data('hoodTabs')] = $(e.target).attr('href');
            localStorage.setItem('tabs-' + tag, JSON.stringify(store));
        });

        let store: any = JSON.parse(localStorage.getItem('tabs-' + tag)) || {};
        let activeTab = store['tab-' + $(tag).data('hoodTabs')];
        let triggerEl = $(tag + ' a[data-bs-toggle="tab"]')[0];
        if (activeTab) {
            triggerEl = document.querySelector(tag + ' a[href="' + activeTab + '"]')
        }
        let tabTrigger = new bootstrap.Tab(triggerEl);
        tabTrigger.show();

    }


}

new Admin();

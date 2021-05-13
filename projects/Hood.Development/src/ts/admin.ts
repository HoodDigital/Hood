export * from "./hood";
import { Alerts } from "./core/Alerts";
import { Uploader } from "./core/Uploader";
import { Editors } from "./admin/Editors";
import { Stats } from "./admin/Stats";

class Admin {
    uploader: Uploader; 
    editors: Editors;
    stats: Stats;

    constructor() {

        this.editors = new Editors();
        this.stats = new Stats();
        this.uploader = new Uploader();

        // hookups- put somewhere
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

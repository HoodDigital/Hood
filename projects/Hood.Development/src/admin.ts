import { Editors } from "./ts/admin/Editors";
import { Stats } from "./ts/admin/Stats";
import { Uploader } from "./ts/core/Uploader";

class Admin {
    editors: Editors;
    stats: Stats;
    uploader: Uploader;

    constructor() {
        this.editors = new Editors();
        this.stats = new Stats();
        this.uploader = new Uploader();

        $('.mobile-sidebar-toggle').on('click', function () {
            $('nav.sidebar').toggleClass('open');
        });

        $('.right-sidebar-toggle').on('click', function () {
            $('#right-sidebar').toggleClass('sidebar-open');
        });

        $(".alert.auto-dismiss").fadeTo(5000, 500).slideUp(500, function () {
            $(".alert.auto-dismiss").slideUp(500);
        });

        console.error("Admin.Constructor() - slimscroll is not implemented... is it needed??");
        //$('.sidebar-scroll').slimScroll({
        //    height: '100%',
        //    railOpacity: 0.4,
        //    wheelStep: 10
        //});

        console.error("Admin.Constructor() - counterUp is not implemented... is it needed??");
        //if ($('[data-plugin="counter"]') && $.counterUp)
        //    $('[data-plugin="counter"]').counterUp({
        //        delay: 10,
        //        time: 800
        //    });

    }
}

new Admin();

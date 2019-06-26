if (!$.hood)
    $.hood = {};
$.hood.Admin = {
    Init: function () {
        $('.navbar-nav .collapse').on("show.bs.collapse", function () {
            !function (t) {
                var e = t.closest(".navbar-nav, .navbar-nav .nav").querySelectorAll(".collapse");
                [].forEach.call(e, function (e) {
                    e !== t && $(e).collapse("hide");
                });
            }(this);
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

        $('[data-plugin="counter"]').counterUp({
            delay: 10,
            time: 3000
        });
        
        $.hood.Admin.Editors.Init();

        $('.colorpicker-component').colorpicker();

    },
    Editors: {
        Init: function () {
            // Load any tinymce editors.
            tinymce.init({
                selector: '.tinymce-full',
                height: 150,
                menubar: false,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
                link_class_list: $.hood.LinkClasses,
                image_class_list: $.hood.ImageClasses,
                setup: $.hood.Uploader.Load.Insert,
                image_dimensions: false,
                content_css: [
                    '/css/site.min.css'
                ]
            });

            tinymce.init({
                selector: '.tinymce-simple',
                height: 150,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                menubar: false,
                toolbar: 'bold italic | bullist numlist | undo redo | link',
                link_class_list: $.hood.LinkClasses,
                image_class_list: $.hood.ImageClasses,
                image_dimensions: false,
                content_css: [
                    '/css/site.min.css'
                ]
            });

            tinymce.init({
                selector: '.tinymce-full-content',
                height: 500,
                menubar: false,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
                link_class_list: $.hood.LinkClasses,
                image_class_list: $.hood.ImageClasses,
                setup: $.hood.Uploader.Load.Insert,
                image_dimensions: false,
                content_css: [
                    '/css/site.min.css'
                ]
            });

            tinymce.init({
                selector: '.tinymce-simple-content',
                height: 500,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                menubar: false,
                toolbar: 'bold italic | bullist numlist | undo redo | link',
                link_class_list: $.hood.LinkClasses,
                image_class_list: $.hood.ImageClasses,
                image_dimensions: false,
                content_css: [
                    '/css/site.min.css'
                ]
            });
        }
    },
    Load: function () {
        $.hood.Admin.Resize();
    },
    Resize: function () {
        if ($(window).width() > 768) {
            $('.content-body').css({
                'min-height': $(window).height() - 216
            });
        }
    }
};
$(window).on('ready', $.hood.Admin.Init);
$(window).on('load', $.hood.Admin.Load);
$(window).on('resize', $.hood.Admin.Resize);


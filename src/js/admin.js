/*
 *
 *   INSPINIA - Responsive Admin Theme
 *   version 2.2
 *
 */


$(document).ready(function () {

    // Add body-small class if window less than 768px
    if ($(this).width() < 769) {
        $('body').addClass('body-small')
    } else {
        $('body').removeClass('body-small')
    }

    // MetsiMenu

    $.hood.Helpers.InitIboxes('body');
    $.hood.Helpers.InitMetisMenu('#side-menu');

    // Close menu in canvas mode
    $('.close-canvas-menu').click(function () {
        $("body").toggleClass("mini-nav");
        SmoothlyMenu();
    });

    // Open close right sidebar
    $('.right-sidebar-toggle').click(function () {
        $('#right-sidebar').toggleClass('sidebar-open');
    });

    // Initialize slimscroll for right sidebar
    $('.sidebar-container').slimScroll({
        height: '100%',
        railOpacity: 0.4,
        wheelStep: 10
    });

    // Open close small chat
    $('.open-small-chat').click(function () {
        $(this).children().toggleClass('fa-comments').toggleClass('fa-remove');
        $('.small-chat-box').toggleClass('active');
    });

    // Initialize slimscroll for small chat
    $('.small-chat-box .content').slimScroll({
        height: '234px',
        railOpacity: 0.4
    });

    // Small todo handler
    $('.check-link').click(function () {
        var button = $(this).find('i');
        var label = $(this).next('span');
        button.toggleClass('fa-check-square').toggleClass('fa-square-o');
        label.toggleClass('todo-completed');
        return false;
    });
    if (typeof (Storage) !== "undefined") {
        if (localStorage.getItem("hood-admin-minimised") == "small")
            $("body").addClass("mini-nav")
        else
            $("body").removeClass("mini-nav")
    }
    // Minimalize menu
    $('.sidebar-toggle').click(function () {
        $("body").toggleClass("mini-nav");
        SmoothlyMenu();
        if (typeof (Storage) !== "undefined") {
            if ($("body").hasClass("mini-nav"))
                localStorage.setItem("hood-admin-minimised", "small");
            else
                localStorage.setItem("hood-admin-minimised", "large");
        }
    });

    // Tooltips demo
    $('.tooltip-demo').tooltip({
        selector: "[data-toggle=tooltip]",
        container: "body"
    });

    // Move modal to body
    // Fix Bootstrap backdrop issu with animation.css
    $('.modal').appendTo("body");

    // Full height of sidebar
    function fix_height() {
        var heightWithoutNavbar = $("body > #wrapper").height() - 61;
        $(".sidebard-panel").css("min-height", heightWithoutNavbar + "px");

        var navbarHeigh = $('nav.navbar-default').height();
        var wrapperHeigh = $('#page-wrapper').height();

        if (navbarHeigh > wrapperHeigh) {
            $('#page-wrapper').css("min-height", navbarHeigh + "px");
        }

        if (navbarHeigh < wrapperHeigh) {
            $('#page-wrapper').css("min-height", $(window).height() + "px");
        }

        if ($('body').hasClass('fixed-nav')) {
            $('#page-wrapper').css("min-height", $(window).height() - 60 + "px");
        }

    }

    fix_height();

    // Fixed Sidebar
    $(window).bind("load", function () {
        if ($("body").hasClass('fixed-sidebar')) {
            $('.sidebar-collapse').slimScroll({
                height: '100%',
                railOpacity: 0.9
            });
        }
    });

    // Move right sidebar top after scroll
    $(window).scroll(function () {
        if ($(window).scrollTop() > 0 && !$('body').hasClass('fixed-nav')) {
            $('#right-sidebar').addClass('sidebar-top');
        } else {
            $('#right-sidebar').removeClass('sidebar-top');
        }
    });

    $(window).bind("load resize scroll", function () {
        if (!$("body").hasClass('body-small')) {
            fix_height();
        }
    });

    $("[data-toggle=popover]")
        .popover();

    // Add slimscroll to element
    $('.full-height-scroll').slimscroll({
        height: '100%'
    })
});


// Minimalize menu when screen is less than 768px
$(window).bind("resize", function () {
    if ($(this).width() < 769) {
        $('body').addClass('body-small')
    } else {
        $('body').removeClass('body-small')
    }
});

// Local Storage functions
// Set proper body class and plugins based on user configuration
$(document).ready(function () {
    if (localStorageSupport) {

        var collapse = localStorage.getItem("collapse_menu");
        var fixedsidebar = localStorage.getItem("fixedsidebar");
        var fixednavbar = localStorage.getItem("fixednavbar");
        var boxedlayout = localStorage.getItem("boxedlayout");
        var fixedfooter = localStorage.getItem("fixedfooter");

        var body = $('body');

        if (fixedsidebar == 'on') {
            body.addClass('fixed-sidebar');
            $('.sidebar-collapse').slimScroll({
                height: '100%',
                railOpacity: 0.9
            });
        }

        if (collapse == 'on') {
            if (body.hasClass('fixed-sidebar')) {
                if (!body.hasClass('body-small')) {
                    body.addClass('mini-nav');
                }
            } else {
                if (!body.hasClass('body-small')) {
                    body.addClass('mini-nav');
                }

            }
        }

        if (fixednavbar == 'on') {
            $(".navbar-static-top").removeClass('navbar-static-top').addClass('navbar-fixed-top');
            body.addClass('fixed-nav');
        }

        if (boxedlayout == 'on') {
            body.addClass('boxed-layout');
        }

        if (fixedfooter == 'on') {
            $(".footer").addClass('fixed');
        }
    }
});

// check if browser support HTML5 local storage
function localStorageSupport() {
    return (('localStorage' in window) && window['localStorage'] !== null)
}

// For demo purpose - animation css script
function animationHover(element, animation) {
    element = $(element);
    element.hover(
        function () {
            element.addClass('animated ' + animation);
        },
        function () {
            //wait for animation to finish before removing classes
            window.setTimeout(function () {
                element.removeClass('animated ' + animation);
            }, 2000);
        });
}

function SmoothlyMenu() {
    if (!$('body').hasClass('mini-nav') || $('body').hasClass('body-small')) {
        // Hide menu in order to smoothly turn on when maximize menu
        $('#side-menu').hide();
        // For smoothly turn on menu
        setTimeout(
            function () {
                $('#side-menu').fadeIn(500);
            }, 100);
    } else if ($('body').hasClass('fixed-sidebar')) {
        $('#side-menu').hide();
        setTimeout(
            function () {
                $('#side-menu').fadeIn(500);
            }, 300);
    } else {
        // Remove all inline style from jquery fadeIn function to reset menu state
        $('#side-menu').removeAttr('style');
    }
}

// Dragable panels
function WinMove() {
    var element = "[class*=col]";
    var handle = ".ibox-title";
    var connect = "[class*=col]";
    $(element).sortable(
        {
            handle: handle,
            connectWith: connect,
            tolerance: 'pointer',
            forcePlaceholderSize: true,
            opacity: 0.8
        })
        .disableSelection();
}

if (!$.hood)
    $.hood = {}
$.hood.Admin = {
    Init: function () {
        $.hood.Helpers.InitMetisMenu(document);
        $.hood.Helpers.InitIboxes(document);

        // Load any tinymce editors.
        tinymce.init({
            selector: '.tinymce-full-admin',
            height: 500,
            menubar: false,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist outdent indent | undo redo | link image media hoodimage | code',
            link_class_list: [
                { title: 'None', value: '' },
                { title: 'Button link', value: 'btn btn-default' },
                { title: 'Theme coloured button link', value: 'btn btn-primary' },
                { title: 'Popup image/video', value: 'colorbox-iframe' },
                { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
                { title: 'Large link', value: 'font-lg' },
                { title: 'Large button link', value: 'btn btn-default btn-lg' },
                { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
                { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
                { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
            ],
            setup: $.hood.Uploader.Load.Insert,
            content_css: [
            ]
        });

        tinymce.init({
            selector: '.tinymce-full',
            height: 500,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            menubar: false,
            toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist outdent indent | undo redo | link image media hoodimage',
            link_class_list: [
                { title: 'None', value: '' },
                { title: 'Button link', value: 'btn btn-default' },
                { title: 'Theme coloured button link', value: 'btn btn-primary' },
                { title: 'Popup image/video', value: 'colorbox-iframe' },
                { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
                { title: 'Large link', value: 'font-lg' },
                { title: 'Large button link', value: 'btn btn-default btn-lg' },
                { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
                { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
                { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
            ],
            setup: $.hood.Uploader.Load.Insert,
            content_css: [
            ]
        });

        tinymce.init({
            selector: '.tinymce-simple-admin',
            height: 150,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            menubar: false,
            toolbar: 'bold italic | bullist numlist | undo redo | link | code',
            link_class_list: [
                { title: 'None', value: '' },
                { title: 'Button link', value: 'btn btn-default' },
                { title: 'Theme coloured button link', value: 'btn btn-primary' },
                { title: 'Popup image/video', value: 'colorbox-iframe' },
                { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
                { title: 'Large link', value: 'font-lg' },
                { title: 'Large button link', value: 'btn btn-default btn-lg' },
                { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
                { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
                { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
            ],
            content_css: [
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
            link_class_list: [
                { title: 'None', value: '' },
                { title: 'Button link', value: 'btn btn-default' },
                { title: 'Theme coloured button link', value: 'btn btn-primary' },
                { title: 'Popup image/video', value: 'colorbox-iframe' },
                { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
                { title: 'Large link', value: 'font-lg' },
                { title: 'Large button link', value: 'btn btn-default btn-lg' },
                { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
                { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
                { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
                { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
            ],
            content_css: [
            ]
        });

        $('.colorpicker-component').colorpicker();

    },
};
$(window).load(function () {
    $.hood.Admin.Init();
});



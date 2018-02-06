$(document).ready(function () {

    if ($(this).width() < 769) {
        $('body').addClass('body-small')
    } else {
        $('body').removeClass('body-small')
    }


    $.hood.Helpers.InitIboxes('body');
    $.hood.Helpers.InitMetisMenu('#side-menu');

    $('.close-canvas-menu').click(function () {
        $("body").toggleClass("mini-nav");
        SmoothlyMenu();
    });
    $('.right-sidebar-toggle').click(function () {
        $('#right-sidebar').toggleClass('sidebar-open');
    });

    $(".alert.auto-dismiss").fadeTo(2000, 500).slideUp(500, function () {
        $(".alert.auto-dismiss").slideUp(500);
    });

    $('.sidebar-container').slimScroll({
        height: '100%',
        railOpacity: 0.4,
        wheelStep: 10
    });

    $('.open-small-chat').click(function () {
        $(this).children().toggleClass('fa-comments').toggleClass('fa-remove');
        $('.small-chat-box').toggleClass('active');
    });

    $('.small-chat-box .content').slimScroll({
        height: '234px',
        railOpacity: 0.4
    });

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
   $('.sidebar-toggle').click(function () {
        $("body").toggleClass("mini-nav");
        if ($("body").hasClass("mini-nav")) {
            $(this).children('.fa').removeClass('fa-list');
            $(this).children('.fa').addClass('fa-bars');
        } else {
            $(this).children('.fa').addClass('fa-list');
            $(this).children('.fa').removeClass('fa-bars');
        }
        SmoothlyMenu();
        if (typeof (Storage) !== "undefined") {
            if ($("body").hasClass("mini-nav"))
                localStorage.setItem("hood-admin-minimised", "small");
            else
                localStorage.setItem("hood-admin-minimised", "large");
        }
    });

    $('.tooltip-demo').tooltip({
        selector: "[data-toggle=tooltip]",
        container: "body"
    });

    $('.modal').appendTo("body");

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

    $(window).bind("load", function () {
        if ($("body").hasClass('fixed-sidebar')) {
            $('.sidebar-collapse').slimScroll({
                height: '100%',
                railOpacity: 0.9
            });
        }
    });

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
});

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

function Resize() {
    if ($(window).width() > 768) {
        $('.content-body').css({
            'min-height': $(window).height() - 216
        });
    }
}
$(window).resize(Resize);
Resize();

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
            link_class_list: $.hood.LinkClasses,
            image_class_list: $.hood.ImageClasses,
            setup: $.hood.Uploader.Load.Insert,
            image_dimensions: false,
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
            link_class_list: $.hood.LinkClasses,
            image_class_list: $.hood.ImageClasses,
            setup: $.hood.Uploader.Load.Insert,
            image_dimensions: false,
            content_css: [
            ]
        });

        tinymce.init({
            selector: '.tinymce-simple-admin',
            height: 500,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            menubar: false,
            toolbar: 'bold italic | bullist numlist | undo redo | link | code',
            link_class_list: $.hood.LinkClasses,
            image_class_list: $.hood.ImageClasses,
            image_dimensions: false,
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
            link_class_list: $.hood.LinkClasses,
            image_class_list: $.hood.ImageClasses,
            image_dimensions: false,
            content_css: [
            ]
        });

        tinymce.init({
            selector: '.tinymce-basic',
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
            ]
        });

        $('.colorpicker-component').colorpicker();

    },
};
$(window).load(function () {
    $.hood.Admin.Init();
});



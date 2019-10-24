$(document).ready(function () {
    Resize();
    $(window).on('scroll', Resize);

    StickyMenu();

    $(window).on('scroll', function () {
        StickyMenu();
    });

    $('body').on('click', '.menu-toggle', function () {
        $('body').toggleClass('menu-open');
    });

    setTimeout(function () {
        $('div.preloader > img').fadeOut();
        setTimeout(function () {
            $('div.preloader').fadeOut('slow');
        }, 500);
    }, 2500);

    $('.scroll-top').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    });
    $('.scroll-to-target').on('click', function (e) {
        var url = $(this).attr('href').split('#')[0];
        if (url !== window.location.pathname && url !== "") {
            return;
        }
        e.preventDefault();
        var target = this.hash;
        var $target = $(target);
        $('html, body').stop().animate({
            'scrollTop': $target.offset().top - 0
        }, 900, 'swing');
    });
    $('.booking-form-click').click(function () {
        $('body').toggleClass('booking');
    });

    $('.collapse').on('shown.bs.collapse', function () {
        $(this).parent().find(".fa-chevron-down").removeClass("fa-chevron-down").addClass("fa-chevron-up");
    }).on('hidden.bs.collapse', function () {
        $(this).parent().find(".fa-chevron-up").removeClass("fa-chevron-up").addClass("fa-chevron-down");
    });

    setTimeout(function () {

        $('.image-carousel').owlCarousel({
            loop: true,
            margin: 0,
            nav: false,
            dots: false,
            animateOut: 'fadeOut',
            animateIn: 'fadeIn',
            lazyLoad: true,
            autoplay: true,
            autoplayTimeout: 6000,
            autoplayHoverPause: false,
            responsive: {
                0: {
                    items: 1
                }
            }
        });

    }, 1000);


});

function Resize() {
    width = $(window).width();
    height = $(window).height();
    if (width > 996) {
        if (height > 700) {
            $('section.hero').css({
                height: height - 25
            });
        } else {
            $('section.hero').css({
                height: 700
            });
        }
    } else {
        $('section.hero').css({
            height: 'auto'
        });
    }
}

function StickyMenu() {
    if ($(window).scrollTop() > 25) {
        $('header').addClass('sticky');
    } else {
        $('header').removeClass('sticky');
    }
}


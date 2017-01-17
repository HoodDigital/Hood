if (!$.hood)
    $.hood = {};
$.hood.App.Extensions = {
    Ready: function () {
        $.hood.App.Init({
            OwlCarousel: {
                Enabled: true
            }
        });
        $.hood.App.Extensions.Resize();
    },
    InitCarousels: function () {
        $('.property-mini-carousel').owlCarousel({
            loop: true,
            margin: 2,
            nav: true,
            responsive: {
                0: {
                    items: 1
                },
                600: {
                    items: 3
                },
                1000: {
                    items: 5
                }
            }
        })
    },
    Load: function () {
        $.hood.App.Extensions.InitCarousels();
    },

    Resize: function () {
    }
}
$.window = $(window),
$.wrapper = $('#wrapper'),
$.header = $('#header'),
$.headerWrap = $('#header-wrap'),
$.content = $('#content'),
$.footer = $('#footer');
var windowWidth = $.window.width()
defaultLogo = $('#logo').find('.standard-logo'),
retinaLogo = $('#logo').find('.retina-logo'),
defaultLogoWidth = defaultLogo.find('img').outerWidth(),
defaultLogoImg = defaultLogo.find('img').attr('src'),
retinaLogoImg = retinaLogo.find('img').attr('src'),
defaultDarkLogo = defaultLogo.attr('data-dark-logo'),
retinaDarkLogo = retinaLogo.attr('data-dark-logo');
$.background = $('#site-background-image');
$(document).ready($.hood.App.Extensions.Ready);
$(document).load($.hood.App.Extensions.Load);
$(window).resize($.hood.App.Resize);
$(window).resize($.hood.App.Extensions.Resize);

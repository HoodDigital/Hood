if (!$.hood)
    $.hood = {};
$.hood.App.Extensions = {
    Ready: function () {
        // Init the hood js app with default settings.
        $.hood.App.Init({
            scrollOffset: 64,
            LoaderHideDelay: 500,
            ShowCookieMessage: !$('body').hasClass('disable-cookies'),
            LoadTweets: $('body').hasClass('hood-tweets'),
            Header: {
                Enabled: true,
                Type: 'click'
            },
            Wow: {
                Enabled: $('.wow').length,
                Settings: {
                    live: true
                }
            },
            VideoBackgrounds: {
                Enabled: $('.vide, .video-panel, .video-bg').length
            },
            LoadSharers: $('#share').length,
            SharerOptions: ["email", "twitter", "facebook", "googleplus", "linkedin", "pinterest", "whatsapp"]
        });

        // Call the resize function.
        $.hood.App.Extensions.Resize();

        // Add further ready time functionality here.
    },
    Load: function () {
        // Add any load time functionality here.
    },
    Resize: function () {
        // Add any resize functionality here.
    }
}

// Variables for the $.hood.App
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

// Initialise $.hood.App
$(document).ready($.hood.App.Extensions.Ready);
$(document).load($.hood.App.Extensions.Load);
$(window).resize($.hood.App.Resize);
$(window).resize($.hood.App.Extensions.Resize);
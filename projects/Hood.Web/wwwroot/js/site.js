if (!$.hood)
    $.hood = {};

$.hood.Site = {
    // This init function will be called back by app.js, when it has been successfully loaded. 
    // If preloaders are present, they will hide any delay while loading the app.js files.
    Init: function () {

        // Overwrite any variables for $.hood.App
        $.window = $(window),
        $.wrapper = $('#wrapper'),
        $.header = $('#header'),
        $.headerWrap = $('#header-wrap'),
        $.content = $('#content'),
        $.footer = $('#footer');
        $.mobileMenu = $('#mobile-menu');
        $.mobileMenuTrigger = $('.mobile-menu-trigger');
        $.background = $('#site-background-image');
        $.sideMenus = $('.side-push-panel');
        $.sideMenuTrigger = $(".side-panel-trigger");
        var windowWidth = $.window.width()

        stickyHeaderClass = 'sticky-header',
        mobileMenuOpenClass = 'mobile-menu-open',
        sidePushPanelClass = 'side-push-panel',
        sidePushPanelOpenClass = 'side-panel-open',

        defaultLogo = $('#logo').find('.standard-logo'),
        defaultLogoImg = defaultLogo.find('img').attr('src'),
        defaultDarkLogo = defaultLogo.attr('data-dark-logo'),
        defaultMobileLogo = defaultLogo.attr('data-mobile-logo'),

        defaultLogoWidth = defaultLogo.find('img').outerWidth(),

        retinaLogo = $('#logo').find('.retina-logo'),
        retinaLogoImg = retinaLogo.find('img').attr('src'),
        retinaDarkLogo = retinaLogo.attr('data-dark-logo'),
        retinaMobileLogo = retinaLogo.attr('data-mobile-logo'),

        owlCarousels = $('#content').find('.owl-carousel-basic');

        // Init the hood js app with default settings.
        $.hood.App.Init();
    },
    Ready: function () {
        // Call the resize function.
        $.hood.Site.Resize();
        // Add any ready time functionality here.
    },
    Load: function () {
        // Add any load time functionality here.
    },
    Resize: function () {
        // Add any resize functionality here.
    }
}

// Initialise $.hood.App
$(document).on('ready', $.hood.Site.Ready);
$(window).on('load', $.hood.Site.Load);
$(window).on('resize', $.hood.Site.Resize);

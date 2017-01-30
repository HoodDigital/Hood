if (!$.hood)
    $.hood = {}
$.hood.App = {
    Options: {
        scrollOffset: 64,
        LoaderHideDelay: 500,
        ShowCookieMessage: !$('body').hasClass('disable-cookies'),
        Colorbox: $('body').hasClass('colorbox'),
        Parallax: $('body').hasClass('parallax'),
        MegaMenu: $('body').hasClass('mega-menu'),
        LoadTweets: $('body').hasClass('hood-tweets'),
        Header: {
            Enabled: true,
            Type: 'hover',
            Settings: {
                delay: 250,
                speed: 350,
                animation: { opacity: 'show' },
                animationOut: { opacity: 'hide' },
                cssArrows: false,
                onShow: $.noop
            },
            FullWidthMenu: {
                gutter: 30
            }
        },
        Wow: {
            Enabled: $('.wow').length,
            Settings: {
                boxClass: 'wow',
                animateClass: 'animated',
                offset: 0,
                mobile: false,
                live: true
            }
        },
        OwlCarousel: {
            Enabled: $('body').hasClass('owl'),
            LoadCss: true,
            CssFiles: [
                '/lib/OwlCarousel2/dist/assets/owl.carousel.min.css'
            ],
            DefaultSettings: {
                loop: true,
                margin: 10,
                nav: true,
                responsive: {
                    0: {
                        items: 1
                    },
                    768: {
                        items: 3
                    },
                    1024: {
                        items: 5
                    }
                }
            }
        },
        VideoBackgrounds: {
            Enabled: $('.vide, .video-panel, .video-bg').length,
            Settings: {
                volume: 1,
                playbackRate: 1,
                muted: true,
                loop: true,
                autoplay: true,
                position: '50% 50%',
                posterType: 'jpg',
                resizing: true,
                bgColor: 'transparent',
                className: ''
            }
        },
        LoadSharers: $('#share').length,
        SharerOptions: ["email", "twitter", "facebook", "googleplus", "linkedin", "pinterest", "whatsapp"]
    },
    Init: function (options) {
        $.hood.App.Options = $.extend($.hood.App.Options, options || {});
        $.hood.App.Loader.Init();
        $.hood.App.Header.Init();
        $.hood.App.Accordion();
        $.hood.App.Counters();
        $.hood.App.Scroll.Init();
        $.hood.App.Scroll.Functions();
        $.hood.App.Scroll.InitialPosition();
        $.hood.App.SkillsBars();
        $.hood.App.Mailchimp.Init();
        $.hood.App.ResizeVideos();
        $.hood.App.ContactForms.Init();
        if ($.hood.App.Options.OwlCarousel.Enabled)
            $.hood.App.OwlCarousel();
        if ($.hood.App.Options.VideoBackgrounds.Enabled)
            $.hood.App.VideoBackgrounds();
        if ($.hood.App.Options.LoadTweets)
            $.hood.App.Tweets();
        if ($.hood.App.Options.Colorbox)
            $.hood.App.Colorbox();
        if ($.hood.App.Options.Parallax)
            $.hood.App.Parallax();
        if ($.hood.App.Options.LoadSharers)
            $.hood.App.Sharers();
        if ($.hood.App.Options.ShowCookieMessage)
            $.hood.App.Cookies();
        if (!$.mobile) {
            if ($.hood.App.Options.Wow.Enabled)
                $.hood.App.Wow();
        }
    },
    Loader: {
        LoadList: new Array(),
        Init: function () {
            var event = new CustomEvent('load-completed');
            $(document).on('load-completed', 'body', $.hood.App.Loader.Complete);
            $.hood.App.Loader.AddItem('loaders');
            $.getScript('/lib/loaders.css/loaders.css.js', $.proxy(function () {
                $.loadCss('loaders-css', '/lib/loaders.css/loaders.min.css');
                $('.loader-inner').loaders();
                $.hood.App.Loader.ItemComplete('loaders');
            }, this));
        },
        Complete: function () {
            $.hood.App.Extensions.Load();
            $('#loader').fadeOut();
            $('#preloader').delay($.hood.App.Options.LoaderHideDelay).slideUp('slow');
            $('body').delay($.hood.App.Options.LoaderHideDelay + 250).css({ 'overflow-y': 'auto' });
        },
        AddItem: function (name) {
            $.hood.App.Loader.LoadList.push({
                Name: name,
                Complete: false
            });
        },
        ItemComplete: function (name) {
            console.log('Loaded: ' + name);
            for (cnt = 0; cnt < $.hood.App.Loader.LoadList.length; cnt++) {
                if ($.hood.App.Loader.LoadList[cnt].Name == name)
                    $.hood.App.Loader.LoadList[cnt].Complete = true;
            }
            if ($.hood.App.Loader.CheckLoaded())
                $('body').trigger('load-completed');
        },
        CheckLoaded: function () {
            for (cnt = 0; cnt < $.hood.App.Loader.LoadList.length; cnt++) {
                if (!$.hood.App.Loader.LoadList[cnt].Complete)
                    return false;
            }
            return true;
        }
    },
    Header: {
        Init: function () {
            $.hood.App.Loader.AddItem('supermenu');
            if ($.hood.App.Header.Type == 'hover') {
                if (!$().superfish && !$().superclick)
                    $.getScript('/lib/superclick/dist/js/superfish.min.js', function () {
                        $.hood.App.Header.Load();
                    });
                else
                    $.hood.App.Header.Load();
            } else {
                if (!$().superfish && !$().superclick)
                    $.getScript('/lib/superclick/dist/js/superclick.min.js', function () {
                        $.hood.App.Header.Load();
                    });
                else
                    $.hood.App.Header.Load();
            }
        },
        Load: function (type) {
            $.hood.App.Header.Setup();
            $.hood.App.Header.MenuFunctions();
            $.hood.App.Header.FullWidthMenu();
            $.hood.App.Header.OverlayMenu();
            $.hood.App.Header.StickyMenu();
            $.hood.App.Header.SideHeader();
            $.hood.App.Header.SidePanel();
            $.hood.App.Header.MobileMenu();
            $.hood.App.Header.Logo();
            $.hood.App.Loader.ItemComplete('supermenu');
        },
        Setup: function () {
            if ($.body.hasClass('device-lg') || $.body.hasClass('device-md')) {
                $('nav.primary ul ul, nav.primary ul .mega-menu-content').css('display', 'block');
                $.hood.App.Header.Invert();
                $('nav.primary ul ul, nav.primary ul .mega-menu-content').css('display', '');
            }
            if (!$().superfish && !$().superclick) {
                $.body.addClass('no-superfish');
                return true;
            }

            opts = {
                popUpSelector: 'ul,.mega-menu-content,.top-link-section',
                delay: $.hood.App.Options.Header.Settings.delay,
                speed: $.hood.App.Options.Header.Settings.speed,
                animation: $.hood.App.Options.Header.Settings.animation,
                animationOut: $.hood.App.Options.Header.Settings.animationOut,
                cssArrows: $.hood.App.Options.Header.Settings.cssArrows,
                onShow: function () {
                    var megaMenuContent = $(this);
                    if (megaMenuContent.hasClass('mega-menu-content') && megaMenuContent.find('.widget').length > 0) {
                        if ($.body.hasClass('device-lg') || $.body.hasClass('device-md')) {
                            $.commonHeight(megaMenuContent);
                        } else {
                            megaMenuContent.children().height('');
                        }
                    }
                    $.hood.App.Options.Header.Settings.onShow();
                }
            };

            if ($.hood.App.Header.Type == 'hover') {
                $('body nav.primary > ul, .top-links > ul').superfish(opts);
            } else {
                $('body nav.primary > ul, .top-links > ul').superclick(opts);
            }

            $('nav.primary-trigger,#overlay-menu-close').click(function () {
                if ($('nav.primary').find('ul.mobile-primary-menu').length > 0) {
                    $('nav.primary > ul.mobile-primary-menu, nav.primary > div > ul.mobile-primary-menu').toggleClass("show");
                } else {
                    $('nav.primary > ul, nav.primary > div > ul').toggleClass("show");
                }
                $.body.toggleClass("primary-menu-open");
                return false;
            });

            if ($.mobile.Any()) {
                $.body.addClass('device-touch');
            }

            if ($.hood.App.Header.Type != 'hover') {
                $('body nav.primary > ul input, .top-links > ul input, body nav.primary > ul select, .top-links > ul select, body nav.primary > ul textarea, .top-links > ul input textarea, body nav.primary > ul .keep-open, .top-links > ul .keep-open').on('click', function (e) {
                    e.stopPropagation();
                });
                $('a.sf-with-ul[href]').click(function () {
                    if ($(this).parents('.sfHover').length)
                        window.location = $(this).attr('href');
                })
            }
        },
        Invert: function () {
            $('nav.primary .mega-menu-content, nav.primary ul ul').each(function (index, element) {
                var $menuChildElement = $(element),
					menuChildOffset = $menuChildElement.offset(),
					menuChildWidth = $menuChildElement.width(),
					menuChildLeft = menuChildOffset.left;
                if (windowWidth - (menuChildWidth + menuChildLeft) < 0) {
                    $menuChildElement.addClass('menu-pos-invert');
                }
            });
        },
        MenuFunctions: function () {

            $('nav.primary ul li:has(ul)').addClass('sub-menu');
            $('.top-links ul li:has(ul) > a').append('<i class="fa fa-caret-down"></i>');
            $('.top-links > ul').addClass('clearfix');

            if ($.body.hasClass('device-lg') || $.body.hasClass('device-md')) {
                $('nav.primary.sub-title').children('ul').children('.current').prev().css({ backgroundImage: 'none' });
            }

            if ($.mobile.Android()) {
                $('nav.primary ul li.sub-menu').children('a').on('touchstart', function (e) {
                    if (!$(this).parent('li.sub-menu').hasClass('sfHover')) {
                        e.preventDefault();
                    }
                });
            }

            if ($.mobile.Windows()) {
                if ($().superfish || $().superclick) {
                    $('nav.primary > ul, .top-links > ul').superfish('destroy').addClass('windows-mobile-menu');
                } else {
                    $('nav.primary > ul, .top-links > ul').addClass('windows-mobile-menu');
                }

                $('nav.primary ul li:has(ul)').append('<a href="#" class="wn-submenu-trigger"><i class="icon-angle-down"></i></a>');

                $('nav.primary ul li.sub-menu').children('a.wn-submenu-trigger').click(function (e) {
                    $(this).parent().toggleClass('open');
                    $(this).parent().find('> ul, > .mega-menu-content').stop(true, true).toggle();
                    return false;
                });
            }

        },
        FullWidthMenu: function () {
            $('.mega-menu .mega-menu-content').css({ 'width': $.wrapper.width() - 2 * $.hood.App.Options.Header.FullWidthMenu.gutter });
        },
        OverlayMenu: function () {
            if ($.body.hasClass('overlay-menu')) {
                var OverlayMenuItem = $('nav.primary').children('ul').children('li'),
					OverlayMenuItemHeight = OverlayMenuItem.outerHeight(),
					OverlayMenuItemTHeight = OverlayMenuItem.length * OverlayMenuItemHeight,
					firstItemOffset = ($.window.height() - OverlayMenuItemTHeight) / 2;

                $('nav.primary').children('ul').children('li:first-child').css({ 'margin-top': firstItemOffset + 'px' });
            }
        },
        StickyMenu: function (headerOffset) {
            if ($.window.scrollTop() > headerOffset) {
                if ($.body.hasClass('device-lg') || $.body.hasClass('device-md')) {
                    $('body:not(.side-header) #header:not(.no-sticky)').addClass('sticky-header');
                    if (!$.headerWrap.hasClass('force-not-dark')) { $.headerWrap.removeClass('not-dark'); }
                } else if ($.body.hasClass('device-xs') || $.body.hasClass('device-xxs') || $.body.hasClass('device-sm')) {
                    if ($.body.hasClass('sticky-responsive-menu')) {
                        $('#header:not(.no-sticky)').addClass('responsive-sticky-header');
                    }
                }
            } else {
                $.hood.App.Header.RemoveStickyness();
            }
        },
        RemoveStickyness: function () {
            if ($.header.hasClass('sticky-header')) {
                $('body:not(.side-header) #header:not(.no-sticky)').removeClass('sticky-header');
                if (!$.headerWrap.hasClass('force-not-dark')) { $.headerWrap.removeClass('not-dark'); }
            }
            if ($.header.hasClass('responsive-sticky-header')) {
                $('body.sticky-responsive-menu #header').removeClass('responsive-sticky-header');
            }
            if (($.body.hasClass('device-xs') || $.body.hasClass('device-xxs') || $.body.hasClass('device-sm')) && (typeof ResponsiveMenuClasses === 'undefined')) {
                if (!$.headerWrap.hasClass('force-not-dark')) { $.headerWrap.removeClass('not-dark'); }
            }
        },
        SideHeader: function () {
            $("#header-trigger").click(function () {
                $('body.open-header').toggleClass("side-header-open");
                return false;
            });
        },
        SidePanel: function () {
            $(".side-panel-trigger").click(function () {
                $.body.toggleClass("side-panel-open");
                if ($.body.hasClass('device-touch') && $.body.hasClass('side-push-panel')) {
                    $.body.toggleClass("ohidden");
                }
                return false;
            });
        },
        MobileMenu: function () {
            $(".mobile-menu-trigger").click(function () {
                if ($.body.hasClass('side-push-panel')) {
                    $.body.removeClass("side-panel-open");
                }
                $.body.toggleClass("mobile-menu-open");
                return false;
            });
        },
        Logo: function () {
            if (($.header.hasClass('dark') || $.body.hasClass('dark')) && !$.headerWrap.hasClass('not-dark')) {
                if (defaultDarkLogo) { defaultLogo.find('img').attr('src', defaultDarkLogo); }
                if (retinaDarkLogo) { retinaLogo.find('img').attr('src', retinaDarkLogo); }
            } else {
                if (defaultLogoImg) { defaultLogo.find('img').attr('src', defaultLogoImg); }
                if (retinaLogoImg) { retinaLogo.find('img').attr('src', retinaLogoImg); }
            }
            if ($.body.hasClass('device-xs') || $.body.hasClass('device-xxs')) {
                if (defaultMobileLogo) { defaultLogo.find('img').attr('src', defaultMobileLogo); }
                if (retinaMobileLogo) { retinaLogo.find('img').attr('src', retinaMobileLogo); }
            }
        }
    },
    Scroll: {
        Init: function () {

            var headerOffset = 0,
                headerWrapOffset = 0,
                pageMenuOffset = 0;

            if ($.header.length > 0) { headerOffset = $.header.offset().top; }
            if ($.header.length > 0) { headerWrapOffset = $.headerWrap.offset().top; }

            var headerDefinedOffset = $.header.attr('data-sticky-offset');
            if (typeof headerDefinedOffset !== 'undefined') {
                if (headerDefinedOffset == 'full') {
                    headerWrapOffset = $.window.height();
                    var headerOffsetNegative = $.header.attr('data-sticky-offset-negative');
                    if (typeof headerOffsetNegative !== 'undefined') { headerWrapOffset = headerWrapOffset - headerOffsetNegative - 1; }
                } else {
                    headerWrapOffset = Number(headerDefinedOffset);
                }
            }

            $.hood.App.Header.StickyMenu(headerWrapOffset);

            $.window.on('scroll', function () {

                $('body.open-header.close-header-on-scroll').removeClass("side-header-open");
                $.hood.App.Header.StickyMenu(headerWrapOffset);
                $.hood.App.Header.Logo();

            });
        },
        InitialPosition: function () {
            var hash = window.location.hash;
            if (hash !== "") {
                var target = hash;
                var $target = $(target);
                if ($target.doesExist()) {
                    $('html, body').stop().delay(350).animate({
                        'scrollTop': $target.offset().top - $.hood.App.Options.scrollOffset
                    }, 500, 'swing');
                    history.pushState("", document.title, window.location.pathname + window.location.search);
                }
            }
        },
        Functions: function () {
            $('.scroll-top').click(function () {
                $('html, body').animate({ scrollTop: 0 }, 800);
                return false;
            });
            $('.scroll-down, .scroll-to-target').on('click', function (e) {
                var url = $(this).attr('href').split('#')[0];
                if (url !== window.location.pathname && url !== "") {
                    return;
                }
                e.preventDefault();
                var target = this.hash;
                var $target = $(target);
                $('html, body').stop().animate({
                    'scrollTop': $target.offset().top - $.hood.App.Options.scrollOffset
                }, 900, 'swing');
            });
        }
    },
    ContactForms: {
        Init: function () {

            $('.contact-form .thank-you').hide();

            $('body').on('submit', '.contact-form', function (e) {
                e.preventDefault();
                $form = $(this);
                $.post($form.attr('action'), $form.serialize(), function (data) {
                    if (data.Success) {
                        if ($form.attr('data-redirect').length > 0)
                            window.location = $form.attr('data-redirect');

                        if ($form.attr('data-alert-message').length > 0)
                            $.hood.Alerts.Success($form.attr('data-alert-message'), "Thank You!", null, true);

                        $form.find('.form').hide();
                        $form.find('.thank-you').show();
                    } else {
                        if (typeof ($form.attr('data-alert-error')) != 'undefined')
                            $.hood.Alerts.Success($form.attr('data-alert-error'), "Error", null, true);

                        $.hood.Alerts.Error("There was an error sending the message", "Error", null, true);
                    }
                });
            });
        }
    },
    Accordion: function () {
        $('.accordion-title').click(function (e) {
            $(this).next().slideToggle('easeOut');
            $(this).toggleClass('active');
            $("accordion-title").toggleClass('active');
            $(".accordion-content").not($(this).next()).slideUp('easeIn');
            $(".accordion-title").not($(this)).removeClass('active');
        });
        $(".accordion-content").addClass("defualt-hidden");
    },
    Cookies: function () {
        $.hood.App.Loader.AddItem('cookies');
        window.cookieconsent_options = { "message": "This website uses cookies to ensure you get the best experience on our website.", "dismiss": "Got it!", "learnMore": "More info", "link": null, "theme": "dark-bottom" };
        $.getScript('//cdnjs.cloudflare.com/ajax/libs/cookieconsent2/1.0.10/cookieconsent.min.js', function () {
            $.hood.App.Loader.ItemComplete('cookies');
        });
    },
    PushMenu: function () {
        $.hood.App.Loader.AddItem('jPushMenu');
        $.getScript('/lib/jPushMenu/js/jPushMenu.js', $.proxy(function () {
            $('.toggle-menu').jPushMenu();
            $.hood.App.Loader.ItemComplete('jPushMenu');
        }, this));
    },
    Parallax: function () {
        //Parallax Function element
        $('.parallax').each(function () {
            var $el = $(this);
            $(window).scroll(function () {
                parallax($el);
            });
            parallax($el);
        });
        function parallax($el) {
            var diff_s = $(window).scrollTop();
            var parallax_height = $('.parallax').height();
            var yPos_p = (diff_s * 0.5);
            var yPos_m = -(diff_s * 0.5);
            var diff_h = diff_s / parallax_height;

            if ($('.parallax').hasClass('parallax-section1')) {
                $el.css('top', yPos_p);
            }
            if ($('.parallax').hasClass('parallax-section2')) {
                $el.css('top', yPos_m);
            }
            if ($('.parallax').hasClass('parallax-static')) {
                $el.css('top', (diff_s * 1));
            }
            if ($('.parallax').hasClass('parallax-opacity')) {
                $el.css('opacity', (1 - diff_h * 1));
            }

            if ($('.parallax').hasClass('parallax-background1')) {
                $el.css("background-position", 'left' + " " + yPos_p + "px");
            }
            if ($('.parallax').hasClass('parallax-background2')) {
                $el.css("background-position", 'left' + " " + -yPos_p + "px");

            }
        };
    },
    Colorbox: function () {
        $.hood.App.Loader.AddItem('colorbox');
        $.getScript('/lib/colorbox/jquery.colorbox-min.js', $.proxy(function () {
            $(".colorbox").colorbox({
                rel: 'gallery',
                maxWidth: "95%",
                maxHeight: "95%"

            });
            $(".colorbox-iframe").colorbox({
                iframe: true,
                maxWidth: "95%",
                maxHeight: "95%",
                innerWidth: 640,
                innerHeight: 390
            });
            $.hood.App.Loader.ItemComplete('colorbox');
        }, this));
    },
    SkillsBars: function () {
        // Skills Progressbar Elements
        $('.skillbar').each(function () {
            $(this).find('.skillbar-bar').animate({
                width: $(this).attr('data-percent')
            }, 2000);
        });
    },
    Counters: function () {
        //Counter
        $('.counter').each(function () {
            var $this = $(this),
                countTo = $this.attr('data-count');
            $({ countNum: $this.text() }).animate({
                countNum: countTo
            },
            {
                duration: 8000,
                easing: 'linear',
                step: function () {
                    $this.text(Math.floor(this.countNum));
                },
                complete: function () {
                    $this.text(this.countNum);
                    //alert('finished');
                }
            });
        });
    },
    OwlCarousel: function () {
        $.hood.App.Loader.AddItem('owl');
        if ($.hood.App.Options.OwlCarousel.LoadCss) {
            for (i = 0; i < $.hood.App.Options.OwlCarousel.CssFiles.length; i++) {
                $.loadCss('owl-css-' + i, $.hood.App.Options.OwlCarousel.CssFiles[i]);
            }
        }
        $.getScript('/lib/OwlCarousel2/dist/owl.carousel.min.js', function () {
            owlCarousels.owlCarousel($.hood.App.Options.OwlCarousel.DefaultSettings);
            $.hood.App.Loader.ItemComplete('owl');
        });
    },
    VideoBackgrounds: function () {
        $.hood.App.Loader.AddItem('vide');
        $.getScript('/lib/vide/dist/jquery.vide.min.js', function () {
            $('.vide, .video-panel, .video-bg').each(function () {
                $(this).vide($(this).data('path'), $.hood.App.Options.VideoBackgrounds.Settings);
            });
            $.hood.App.Loader.ItemComplete('vide');
        });
    },
    Resize: function () {
        var t = setTimeout(function () {
            $.hood.App.Header.FullWidthMenu();
            $.hood.App.Header.OverlayMenu();
            if ($.body.hasClass('device-lg') || $.body.hasClass('device-md')) {
                $('nav.primary').find('ul.mobile-primary-menu').removeClass('show');
            }
        }, 500);
        windowWidth = $.window.width();
    },
    ResizeVideos: function () {

        if (!$().fitVids) {
            console.log('resizeVideos: FitVids not Defined.');
            return true;
        }

        $("#content,#footer,#slider:not(.revslider-wrap),.landing-offer-media,.portfolio-ajax-modal,.mega-menu-column").fitVids({
            customSelector: "iframe[src^='http://www.dailymotion.com/embed'], iframe[src*='maps.google.com'], iframe[src*='google.com/maps']",
            ignore: '.no-fv'
        });
    },
    Sharers: function () {
        $.getScript('https://cdn.jsdelivr.net/jquery.jssocials/1.4.0/jssocials.min.js', $.proxy(function () {
            $.loadCss('sharer-css', 'https://cdn.jsdelivr.net/jquery.jssocials/1.4.0/jssocials.css');
            $.loadCss('sharer-theme-css', 'https://cdn.jsdelivr.net/jquery.jssocials/1.4.0/jssocials-theme-flat.css');
            $("#share").jsSocials({
                shares: this.Options.SharerOptions,
                showLabel: true,
                showCount: true,
                shareIn: "popup",
            });
        }, this));
    },
    Tweets: function () {
        if ($(".hood-tweets").doesExist()) {
            $.hood.App.Loader.AddItem('tweets');
            if (typeof kendo !== 'undefined') {
                $.hood.App.LoadTweets();
            } else {
                // for now we need to load kendo to do this
                $.getScript('http://kendo.cdn.telerik.com/2016.2.714/js/kendo.ui.core.min.js', function () {
                    $.hood.App.LoadTweets();
                });
            }
        }
    },
    LoadTweets: function () {
        // Load the od' tweets.
        $.get('/content/tweets', null, function (ret) {
            data = ret.Data;
            if (data.length > 0) {
                // we have tweets.
                $(".hood-tweets").empty();
                for (i = 0; i < 6; i++) {
                    var template = kendo.template($("#hood-tweet-template").html());
                    var result = template(data[i]); //Execute the template
                    $(".hood-tweets").append(result); //Append the result
                }
            }
        }).success(function () {
        }).fail(function () {
        }).error(function () {
        }).complete(function () {
        }).always(function () {
            $.hood.App.Loader.ItemComplete('tweets');
        });
    },
    Mailchimp: {
        Init: function () {
            $('body').on('submit', '#mailchimp-signup-form', $.hood.App.Mailchimp.SendForm);
        },
        SendForm: function (e) {
            data = $(this).serialize();
            $.post('/signup/mailchimp', data, function (data) {
                if (data.Success) {
                    $.hood.Alerts.Success("You have been sent a sign up confirmation, please check your email inbox.", "Thank you!", "success")
                } else {
                    $.hood.Alerts.Success("There was a problem signing up: " + data.Errors, "Error!", "error")
                }
            });
            return false;
        }
    },
    Wow: function () {
        $.hood.App.Loader.AddItem('wow');
        $.getScript('/lib/wowjs/dist/wow.min.js', function () {
            wow = new WOW($.hood.App.Options.Wow.Settings);
            wow.init();
            $.hood.App.Loader.ItemComplete('wow');
        });
    }
};
$.window = $(window);

var windowWidth = $.window.width()
defaultLogo = $('#logo').find('.standard-logo'),
retinaLogo = $('#logo').find('.retina-logo'),
defaultLogoWidth = defaultLogo.find('img').outerWidth(),
defaultLogoImg = defaultLogo.find('img').attr('src'),
retinaLogoImg = retinaLogo.find('img').attr('src'),
defaultDarkLogo = defaultLogo.attr('data-dark-logo'),
retinaDarkLogo = retinaLogo.attr('data-dark-logo'),
defaultMobileLogo = defaultLogo.attr('data-mobile-logo'),
retinaMobileLogo = retinaLogo.attr('data-mobile-logo'),
owlCarousels = $('#content').find('.owl-carousel-basic');

//oldHeaderClasses = $.header.attr('class'),
//oldHeaderWrapClasses = $.headerWrap.attr('class'),
//StickyMenuClasses = $.header.attr('data-sticky-class'),
//ResponsiveMenuClasses = $.header.attr('data-responsive-class'),
//defaultLogo = $('#logo').find('.standard-logo'),
//defaultLogoWidth = defaultLogo.find('img').outerWidth(),
//retinaLogo = $('#logo').find('.retina-logo'),
//defaultStickyLogo = defaultLogo.attr('data-sticky-logo'),
//retinaStickyLogo = retinaLogo.attr('data-sticky-logo'),
//$pagemenu = $('#page-menu'),
//$onePageMenuEl = $('.one-page-menu'),
//onePageGlobalOffset = 0,
//$portfolio = $('.portfolio'),
//$shop = $('.shop'),
//$gridContainer = $('.grid-container'),
//$slider = $('#slider'),
//$sliderParallaxEl = $('.slider-parallax'),
//swiperSlider = '',
//$pageTitle = $('#page-title'),
//$portfolioItems = $('.portfolio-ajax').find('.portfolio-item'),
//$portfolioDetails = $('#portfolio-ajax-wrap'),
//$portfolioDetailsContainer = $('#portfolio-ajax-container'),
//$portfolioAjaxLoader = $('#portfolio-ajax-loader'),
//$portfolioFilter = $('.portfolio-filter,.custom-filter'),
//prevPostPortId = '',
//$topSearch = $('#top-search'),
//$topCart = $('#top-cart'),
//$verticalMiddleEl = $('.vertical-middle'),
//$topSocialEl = $('#top-social').find('li'),
//$siStickyEl = $('.si-sticky'),
//$dotsMenuEl = $('.dots-menu'),
//$goToTopEl = $('#gotoTop'),
//$fullScreenEl = $('.full-screen'),
//$commonHeightEl = $('.common-height'),
//$testimonialsGridEl = $('.testimonials-grid'),
//$pageSectionEl = $('.page-section'),
//$owlCarouselEl = $('.owl-carousel'),
//$parallaxEl = $('.parallax'),
//$parallaxPageTitleEl = $('.page-title-parallax'),
//$parallaxPortfolioEl = $('.portfolio-parallax').find('.portfolio-image'),
//$textRotaterEl = $('.text-rotater'),
//$cookieNotification = $('#cookie-notification')


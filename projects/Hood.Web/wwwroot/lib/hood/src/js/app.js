// Global variables for the $.hood.App
// Overwrite these in your site.js to use different classes/elements etc.

$.window = $(window),
    $.wrapper = $('#wrapper'),
    $.header = $('#header'),
    $.headerWrap = $('#header-wrap'),
    $.content = $('#content'),
    $.footer = $('#footer'),
    $.mobileMenu = $('#mobile-menu'),
    $.mobileMenuTrigger = $('.mobile-menu-trigger'),
    $.background = $('#site-background-image'),
    $.sideMenus = $('.side-push-panel'),
    $.sideMenuTrigger = $(".side-panel-trigger"),
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
    retinaMobileLogo = retinaLogo.attr('data-mobile-logo');

var windowWidth = $.window.width()

if (!$.hood)
    $.hood = {}
$.hood.App = {
    Options: {
        scrollOffset: 64,
        Scroll: {
            StickyHeader: true,
            InitialPosition: false,
            Functions: true,
            ToTargetSelector: ".scroll-to-target",
            ToTopSelector: '.scroll-top'
        },
        Loader: {
            Delay: 500,
            Complete: null,
            Items: [

            ]
        },
        ShowCookieMessage: !$('body').hasClass('disable-cookies'),
        Forums: true,
        Colorbox: true,
        ContactForms: true,
        FitVids: true,
        Accordion: true,
        Counters: true,
        Uploaders: true,
        PaymentPages: true,
        Header: true,
        RichTextEditors: true,
        LoadSharers: $('#share').length,
        SharerOptions: ["email", "twitter", "facebook", "googleplus", "linkedin", "pinterest", "whatsapp"]
    },
    Init: function (options) {

        $.hood.App.Options = $.extend($.hood.App.Options, options || {});
        if (options) {
            if (options.Header) $.hood.App.Options.Header = $.extend($.hood.App.Options.Header, options.Header || {});
            if (options.Header) $.hood.App.Options.Header.Settings = $.extend($.hood.App.Options.Header.Settings, options.Header.Settings || {});
        }

        $.hood.App.Loader.Init();

        if ($.hood.App.Options.Header)
            $.hood.App.Header.Init();

        if ($.hood.App.Options.Scroll.Functions)
            $.hood.App.Scroll.Functions();

        if ($.hood.App.Options.PaymentPages)
            $.hood.App.PaymentPages.Init();

        if ($.hood.App.Options.Accordion)
            $.hood.App.Accordion();

        if ($.hood.App.Options.Counters)
            $.hood.App.Counters();

        if ($.hood.App.Options.FitVids)
            $.hood.App.ResizeVideos();

        if ($.hood.App.Options.ContactForms)
            $.hood.App.ContactForms.Init();

        if ($.hood.App.Options.Uploaders)
            $.hood.App.Uploaders.Init();

        if ($.hood.App.Options.Colorbox)
            $.hood.App.Colorbox();

        if ($.hood.App.Options.LoadSharers)
            $.hood.App.Sharers();

        if ($.hood.App.Options.ShowCookieMessage)
            $.hood.App.Cookies();

        if ($.hood.App.Options.RichTextEditors)
            $.hood.App.RichTextEditors();
    },
    onInitComplete: function () {
        if ($.hood.App.Options.Scroll.StickyHeader)
            $.hood.App.Scroll.Init();
        if ($.hood.App.Options.Scroll.InitialPosition)
            $.hood.App.Scroll.InitialPosition();
    },
    Loader: {
        LoadList: new Array(),
        Init: function () {
            if (document.createEvent) {
                var event = new CustomEvent('load-completed');
                $(document).on('load-completed', 'body', $.hood.App.onInitComplete);
                $(document).on('load-completed', 'body', $.hood.App.Options.Loader.Complete || $.hood.App.Loader.Complete);
                for (i = 0; i < $.hood.App.Options.Loader.Items.length; i++) {
                    $.hood.App.Loader.AddItem($.hood.App.Options.Loader.Items[i]);
                }
            } else {
                // if we can't add events, we cant load, its going to be ugly, but show the site.
                if ($.hood.App.Options.Loader.Complete)
                    $.hood.App.Options.Loader.Complete();
                else
                    $.hood.App.Loader.Complete();
            }
        },
        Complete: function () {
            $('#loader').fadeOut(100);
            $('#preloader').delay($.hood.App.Options.Loader.Delay).fadeOut(100);
            setTimeout(function () {
                if ($.hood.App.Options.Forums)
                    $.hood.App.Forums.Init();
            }, $.hood.App.Options.Loader.Delay + 200);
        },
        AddItem: function (name) {
            $.hood.App.Loader.LoadList.push({
                Name: name,
                Complete: false
            });
        },
        ItemComplete: function (name) {
            for (cnt = 0; cnt < $.hood.App.Loader.LoadList.length; cnt++) {
                if ($.hood.App.Loader.LoadList[cnt].Name == name) {
                    $.hood.App.Loader.LoadList[cnt].Complete = true;
                    console.log('Loaded: ' + name);
                }
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
            $.hood.App.Header.SidePanel();
            $.hood.App.Header.MobileMenu();
            $.hood.App.Header.Logo();
        },
        StickyMenu: function () {
            var headerOffset = 0,
                headerWrapOffset = 0,
                pageMenuOffset = 0;

            if ($.header.length > 0) { headerOffset = $.header.offset().top; }
            if ($.header.length > 0) { headerWrapOffset = $.headerWrap.offset().top; }

            if ($.window.scrollTop() > headerOffset) {
                $.header.addClass(stickyHeaderClass);
            } else {
                $.hood.App.Header.RemoveStickyness();
            }
        },
        RemoveStickyness: function () {
            if ($.header.hasClass(stickyHeaderClass)) {
                $.header.removeClass(stickyHeaderClass);
            }
        },
        SidePanel: function () {
            $.sideMenuTrigger.click(function () {
                $.body.toggleClass(sidePushPanelOpenClass);
                if ($.body.hasClass('device-touch') && $.body.hasClass(sidePushPanelClass)) {
                    $.body.toggleClass("ohidden");
                }
                return false;
            });
        },
        MobileMenu: function () {
            $.mobileMenuTrigger.click(function () {
                if ($.body.hasClass(sidePushPanelClass)) {
                    $.body.removeClass(sidePushPanelClass);
                }
                $.body.toggleClass(mobileMenuOpenClass);
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
        }
    },
    Scroll: {
        Init: function () {
            $.hood.App.Header.StickyMenu();
            $.window.on('scroll', function () {

                $('body.open-header.close-header-on-scroll').removeClass("side-header-open");
                $.hood.App.Header.StickyMenu();
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
            $($.hood.App.Options.Scroll.ToTopSelector).click(function () {
                $('html, body').animate({ scrollTop: 0 }, 800);
                return false;
            });
            $($.hood.App.Options.Scroll.ToTargetSelector).on('click', function (e) {
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
            $('.contact-form .form-submit').show();
            $('body').on('submit', '.contact-form', function (e) {
                e.preventDefault();
                if ($('.g-recaptcha')) {
                    grecaptcha.reset();
                    grecaptcha.execute();
                } else {
                    $.hood.App.ContactForms.Submit($(this).attr('id'));
                }
                return false;
            });
            $('body').on('click', '.form-submit-basic', function () {
                $.hood.App.ContactForms.Submit($(this).data('tag'));
            });
        },
        Submit: function (tag) {
            $form = $(tag);
            $.post($form.attr('action'), $form.serialize(), function (data) {
                if (data.Success) {
                    if ($form.attr('data-redirect'))
                        window.location = $form.attr('data-redirect');

                    if ($form.attr('data-alert-message'))
                        $.hood.Alerts.Success($form.attr('data-alert-message'), "Success", null, true);

                    $form.find('.form').hide();
                    $form.find('.thank-you').show();
                } else {
                    if (typeof ($form.attr('data-alert-error')) != 'undefined')
                        $.hood.Alerts.Success($form.attr('data-alert-error'), "Error", null, true);

                    $.hood.Alerts.Error("There was an error sending the message", "Error", null, true);
                }
            });
            return false;
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
    Colorbox: function () {
        $.hood.App.Loader.AddItem('colorbox');
        $.getScript('/lib/jquery-colorbox/jquery.colorbox-min.js', $.proxy(function () {
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
    Counters: function () {
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
                    }
                });
        });
    },
    Forums: {
        Init: function () {
            // check for highlight.
            var highlight = $.getUrlVars()['highlight'];
            if ($.isNumeric(highlight)) {
                $post = $('#post-' + highlight);
                $('html,body').animate({ scrollTop: $post.offset().top - $.hood.App.Options.scrollOffset }, 'slow');
                $post.addClass('highlight');
            }
        }
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
                shareIn: "popup"
            });
        }, this));
    },
    Uploaders: {
        Init: function () {
            if ($('#media-upload').doesExist() || $('#avatar-upload').doesExist()) {
                $.hood.App.Loader.AddItem('uploaders');
                $.getScript('/lib/dropzone/dist/min/dropzone.min.js', $.proxy(function () {
                    if ($('#media-upload').doesExist())
                        $.hood.App.Uploaders.Gallery();
                    if ($('#avatar-upload').doesExist())
                        $.hood.App.Uploaders.Avatar();
                    $.hood.App.Loader.ItemComplete('uploaders');
                }, this));
            }
        },
        Gallery: function () {
            Dropzone.autoDiscover = false;

            var previewNode = document.querySelector("#media-upload-template");
            previewNode.id = "";
            var previewTemplate = previewNode.parentNode.innerHTML;
            previewNode.parentNode.removeChild(previewNode);

            var galleryDropzone = new Dropzone("#media-upload", {
                url: $("#media-upload").data('url'),
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                previewTemplate: previewTemplate,
                paramName: 'files',
                autoProcessQueue: true, // Make sure the files aren't queued until manually added
                previewsContainer: "#previews", // Define the container to display the previews
                clickable: ".fileinput-button", // Define the element that should be used as click trigger to select files.
                dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
                dictResponseError: 'Error while uploading file!'
            });
            $("#media-upload .cancel").hide();

            galleryDropzone.on("addedfile", function (file) {
                $(file.previewElement.querySelector(".complete")).hide();
                $(file.previewElement.querySelector(".cancel")).show();
                $("#media-upload .cancel").show();
            });

            // Update the total progress bar
            galleryDropzone.on("totaluploadprogress", function (progress) {
                document.querySelector("#total-progress .progress-bar").style.width = progress + "%";
            });

            galleryDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                document.querySelector("#total-progress").style.opacity = "1";
                // And disable the start button
            });

            // Hide the total progress bar when nothing's uploading anymore
            galleryDropzone.on("complete", function (file) {
                $(file.previewElement.querySelector(".cancel")).hide();
                $(file.previewElement.querySelector(".progress")).hide();
                $(file.previewElement.querySelector(".complete")).show();
                $.hood.Inline.Refresh('.gallery');
            });

            // Hide the total progress bar when nothing's uploading anymore
            galleryDropzone.on("queuecomplete", function (progress) {
                document.querySelector("#total-progress").style.opacity = "0";
                $("#media-upload .cancel").hide();
            });

            galleryDropzone.on("success", function (file, response) {
                $.hood.Inline.Refresh('.gallery');
                if (response.Success) {
                    $.hood.Alerts.Success("New images added!");
                } else {
                    $.hood.Alerts.Error("There was a problem adding the profile image: " + response.Error);
                }
            });

            // Setup the buttons for all transfers
            // The "add files" button doesn't need to be setup because the config
            // `clickable` has already been specified.
            document.querySelector(".actions .cancel").onclick = function () {
                galleryDropzone.removeAllFiles(true);
            };
        },
        Avatar: function () {
            Dropzone.autoDiscover = false;
            var avatarDropzone = new Dropzone("#avatar-upload", {
                url: $("#avatar-upload").data('url'),
                maxFiles: 1,
                paramName: 'file',
                parallelUploads: 1,
                autoProcessQueue: true, // Make sure the files aren't queued until manually added
                previewsContainer: false, // Define the container to display the previews
                clickable: "#avatar-upload" // Define the element that should be used as click trigger to select files.
            });
            avatarDropzone.on("addedfile", function () {
                if (this.files[1] != null) {
                    this.removeFile(this.files[0]);
                }
            });
            // Update the total progress bar
            avatarDropzone.on("totaluploadprogress", function (progress) {
                document.querySelector("#avatar-total-progress .progress-bar").style.width = progress + "%";
            });
            avatarDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                document.querySelector("#avatar-total-progress").style.opacity = "1";
                $($("#avatar-upload").data('preview')).addClass('loading');
            });
            avatarDropzone.on("queuecomplete", function (progress) {
                document.querySelector("#avatar-total-progress").style.opacity = "0";
            });
            avatarDropzone.on("success", function (file, response) {
                if (response.Success) {
                    $("#AvatarJson").val(JSON.stringify(response.Image));
                    $($("#avatar-upload").data('preview')).css({
                        'background-image': 'url(' + response.Image.SmallUrl + ')'
                    });
                    $($("#avatar-upload").data('preview')).find('img').attr('src', response.Image.SmallUrl);
                    $.hood.Alerts.Success("New profile image added!");
                } else {
                    $.hood.Alerts.Error("There was a problem adding the profile image: " + response.Error);
                }
                $($("#avatar-upload").data('preview')).removeClass('loading');
            });
        }
    },
    PaymentPages: {
        Init: function () {
            $('body').on('click', '.btn.price-select[data-target][data-value]', $.hood.App.PaymentPages.PriceSelect);
            $('body').on('click', '.change-price-option', $.hood.App.PaymentPages.ChangePrice);
        },
        ChangePrice: function () {
            $('#price-panel').collapse('show');
            $('#billing-panel').collapse('hide');
            $('#confirm-panel').collapse('hide');
        },
        PriceSelect: function () {
            var $this = $(this);
            targetId = '#' + $this.data('target');
            $(targetId).val($this.data('value'));
            $(".selected-price-text").html($(targetId).find(":selected").text());
            $('.price-select[data-target="' + $this.data('target') + '"]').each(function () { $(this).html($(this).data('temp')).removeClass('active'); });
            $('.price-select[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () { $(this).data('temp', $(this).html()).html('Selected').addClass('active') });;
            $('#price-panel').collapse('hide');
            $('#billing-panel').collapse('show');
            $('#confirm-panel').collapse('hide');
        }
    },
    RichTextEditors: function () {
        tinymce.init({
            selector: '.tinymce-public',
            height: 250,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste emoticons'
            ],
            menubar: false,
            toolbar: 'styleselect | bold italic | bullist numlist outdent indent | undo redo | link image media emoticons',
            image_dimensions: false,
            content_css: [
            ]
        });
    }
};
if ($.hood.Site && $.hood.Site.Init());
$(window).resize($.hood.App.Resize);
var googleRecaptchaCallback = function (token) {
    $.hood.App.ContactForms.Submit($('#g-recaptcha').data('target'));
}
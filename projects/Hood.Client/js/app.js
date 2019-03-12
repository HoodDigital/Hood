$.window = $(window),
    $.header = $('#header'),
    $.mobileMenu = $('#mobile-menu'),
    $.mobileMenuTrigger = $('.mobile-menu-trigger'),
    $.sideMenuTrigger = $(".side-panel-trigger"),
    stickyHeaderClass = 'sticky-header',
    mobileMenuOpenClass = 'mobile-menu-open',
    sidePushPanelClass = 'side-push-panel',
    sidePushPanelOpenClass = 'side-panel-open';

if (!$.hood)
    $.hood = {};

$.hood.App = {
    Options: {
        Scroll: {
            StickyHeader: true,
            InitialPosition: false,
            Functions: true,
            Offset: 117,
            ToTargetSelector: ".scroll-to-target",
            ToTopSelector: '.scroll-top'
        },
        Loader: {
            Delay: 500,
            Complete: null,
            Items: [

            ]
        },
        Accordion: true,
        Alerts: true,
        Colorbox: true,
        ContactForms: true,
        Counters: true,
        FitVids: true,
        Forums: true,
        Header: true,
        PaymentPages: true,
        RichTextEditors: $('.tinymce-public').length,
        Uploaders: true
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
                if ($.hood.App.Options.Alerts)
                    $.hood.App.Alerts();
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
                if ($.hood.App.Loader.LoadList[cnt].Name === name) {
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
        },
        StickyMenu: function () {
            var headerOffset = 0;
            if ($.header.length > 0) { headerOffset = $.header.offset().top; }
            header = $.header.height();
            win = $.window.height() + (2 * header);
            doc = $(document).height();
            if ($.window.scrollTop() > headerOffset && doc > win) {
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
        }
    },
    Scroll: {
        Init: function () {
            $.hood.App.Header.StickyMenu();
            $.window.on('scroll', function () {

                $('body.open-header.close-header-on-scroll').removeClass("side-header-open");
                $.hood.App.Header.StickyMenu();

            });
        },
        InitialPosition: function () {
            var hash = window.location.hash;
            if (hash !== "") {
                var target = hash;
                var $target = $(target);
                if ($target.doesExist()) {
                    $('html, body').stop().delay(350).animate({
                        'scrollTop': $target.offset().top - $.hood.App.Options.Scroll.Offset
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
                    'scrollTop': $target.offset().top - $.hood.App.Options.Scroll.Offset
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
                $(this).addClass('loading');
                $.hood.App.ContactForms.Submit(this);
                return false;
            });
        },
        Submit: function (tag) {
            $form = $(tag);
            if ($form.valid()) {
                if ($form.hasClass('g-recaptcha') && grecaptcha.getResponse() === "") {
                    $.hood.Alerts.Error('Please tell us you are not a robot!', 'Confirm Humanity!', null, true);
                    return false;
                } else {
                    $.post($form.attr('action'), $form.serialize(), function (data) {
                        if (data.Success) {
                            if ($form.attr('data-redirect'))
                                window.location = $form.attr('data-redirect');

                            if ($form.attr('data-alert-message'))
                                $.hood.Alerts.Success($form.attr('data-alert-message'), "Success", null, true);

                            $form.find('.form').hide();
                            $form.find('.thank-you').show();
                        } else {
                            if (typeof ($form.attr('data-alert-error')) !== 'undefined')
                                $.hood.Alerts.Success($form.attr('data-alert-error'), "Error", null, true);

                            $.hood.Alerts.Error("There was an error sending the message: " + data.Errors, "Error", null, true);
                        }
                        $form.removeClass('loading');
                    });
                }
            }
            return false;
        }
    },
    Alerts: function () {
        $(".alert.auto-dismiss").fadeTo(5000, 500).slideUp(500, function () {
            $(".alert.auto-dismiss").slideUp(500);
        });
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
                $('html,body').animate({ scrollTop: $post.offset().top - $.hood.App.Options.Scroll.Offset }, 'slow');
                $post.addClass('highlight');
                if ($.getUrlVars()['message'] === "Created")
                    $post.addClass('created');
            }

            var reply = $.getUrlVars()['reply'];
            if ($.isNumeric(reply)) {
                $post = $('#forum-post-form');
                $('html,body').animate({ scrollTop: $post.offset().top - $.hood.App.Options.Scroll.Offset }, 'slow');
            }

            // toggle editors
            $('.forum').on('click', '.edit-post', function (e) {
                $(this).parents('.post').find('.post-view').slideToggle();
                $(this).parents('.post').find('.edit-view').slideToggle();
            });
        }
    },
    ResizeVideos: function () {
        if (!$().fitVids) {
            console.log('resizeVideos: FitVids not Defined.');
            return true;
        }
        $("body").fitVids({
            customSelector: "iframe[src^='http://www.dailymotion.com/embed'], iframe[src*='maps.google.com'], iframe[src*='google.com/maps']",
            ignore: '.no-fv'
        });
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
                avatarDropzone.removeFile(file);
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
            body_class: 'tiny-mce-body',
            content_css: '/css/site.css'
        });
    }
};
if ($.hood.Site && $.hood.Site.Init());
$(window).on('resize', $.hood.App.Resize);

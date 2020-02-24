$.window = $(window);
if (!$.hood)
    $.hood = {};
$.hood.App = {
    Options: {
        Header: {
            Target: '#header',
            Sticky: true,
            StickyClass: 'sticky-header'
        },
        Alerts: true,
        Colorbox: true,
        ContactForms: true,
        Forums: true,
        PaymentPages: true,
        RichTextEditors: $('.tinymce-public').length
    },
    Init: function (options) {

        $.hood.App.Options = $.extend($.hood.App.Options, options || {});

        if (options) {
            if (options.Header) $.hood.App.Options.Header = $.extend($.hood.App.Options.Header, options.Header || {});
        }

        $.hood.App.Header.Init();

        if ($.hood.App.Options.PaymentPages)
            $.hood.App.PaymentPages.Init();

        if ($.hood.App.Options.FitVids)
            $.hood.App.ResizeVideos();

        if ($.hood.App.Options.ContactForms)
            $.hood.App.ContactForms.Init();

        if ($.hood.App.Options.Colorbox)
            $.hood.App.Colorbox();

        if ($.hood.App.Options.LoadSharers)
            $.hood.App.Sharers();

        if ($.hood.App.Options.RichTextEditors)
            $.hood.App.RichTextEditors();
    },
    Ready: function () {
        $.hood.App.Init();
        $.hood.App.Resize();
    },
    Load: function () {
    },
    Resize: function () {
    },
    Header: {
        Init: function () {
            if ($.hood.App.Options.Header.Sticky) {
                $.hood.App.Header.StickyMenu();
                $.window.on('scroll', function () {
                    $.hood.App.Header.StickyMenu();
                });
            }
        },
        StickyMenu: function () {
            var headerOffset = 0;
            var $header = $($.hood.App.Options.Header.Target);
            if ($header.length > 0) { headerOffset = $header.offset().top; }
            let header = $header.height();
            let  win = $.window.height() + 2 * header;
            let doc = $(document).height();
            if ($.window.scrollTop() > headerOffset && doc > win) {
                $header.addClass($.hood.App.Options.Header.StickyClass);
            } else {
                $.hood.App.Header.RemoveStickyness();
            }
        },
        RemoveStickyness: function () {
            var $header = $($.hood.App.Options.Header.Target);
            if ($header.hasClass($.hood.App.Options.Header.StickyClass)) {
                $header.removeClass($.hood.App.Options.Header.StickyClass);
            }
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
            var $form = $(tag);
            if ($form.valid()) {
                $.post($form.attr('action'), $form.serialize(), function (data) {
                    if (data.Success) {
                        if ($form.attr('data-redirect'))
                            window.location = $form.attr('data-redirect');

                        if ($form.attr('data-alert-message'))
                            $.hood.Alerts.Success($form.attr('data-alert-message'), "Success", null, true);

                        $form.find('.form').hide();
                        $form.find('.thank-you').show();
                    } else {
                        if ($form.attr('data-alert-error'))
                            $.hood.Alerts.Error($form.attr('data-alert-error'), "Error", null, true);
                        else
                            $.hood.Alerts.Error("There was an error sending the message: " + data.Errors, "Error", null, true);
                    }
                    $form.removeClass('loading');
                });
            }
            return false;
        }
    },
    Alerts: function () {
        $(".alert.auto-dismiss").fadeTo(5000, 500).slideUp(500, function () {
            $(".alert.auto-dismiss").slideUp(500);
        });
    },
    Colorbox: function () {
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
    },
    Forums: {
        Init: function () {
            // check for highlight.
            var highlight = $.getUrlVars()['highlight'];
            if ($.isNumeric(highlight)) {
                let $post = $('#post-' + highlight);
                $('html,body').animate({ scrollTop: $post.offset().top }, 'slow');
                $post.addClass('highlight');
                if ($.getUrlVars()['message'] === "Created")
                    $post.addClass('created');
            }

            var reply = $.getUrlVars()['reply'];
            if ($.isNumeric(reply)) {
                let $post = $('#forum-post-form');
                $('html,body').animate({ scrollTop: $post.offset().top }, 'slow');
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
    }, 

    Property: {
        Loaded: function (data) {
            $.hood.Loader(false);
            $.hood.Google.ClusteredMap();
        },
        Reload: function (complete) {
            if ($('#property-list').doesExist())
                $.hood.Inline.Reload($('#property-list'), complete);
        }
    }
};

// Initialise
$(function () { $.hood.App.Ready(); });
$(window).on('load', $.hood.App.Load);
$(window).on('resize', $.hood.App.Resize);

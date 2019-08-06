"use strict";

$.window = $(window);
if (!$.hood) $.hood = {};
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
  Init: function Init(options) {
    $.hood.App.Options = $.extend($.hood.App.Options, options || {});

    if (options) {
      if (options.Header) $.hood.App.Options.Header = $.extend($.hood.App.Options.Header, options.Header || {});
    }

    $.hood.App.Header.Init();
    if ($.hood.App.Options.PaymentPages) $.hood.App.PaymentPages.Init();
    if ($.hood.App.Options.FitVids) $.hood.App.ResizeVideos();
    if ($.hood.App.Options.ContactForms) $.hood.App.ContactForms.Init();
    if ($.hood.App.Options.Colorbox) $.hood.App.Colorbox();
    if ($.hood.App.Options.LoadSharers) $.hood.App.Sharers();
    if ($.hood.App.Options.RichTextEditors) $.hood.App.RichTextEditors();
  },
  Ready: function Ready() {
    $.hood.App.Init();
    $.hood.App.Resize();
  },
  Load: function Load() {},
  Resize: function Resize() {},
  Header: {
    Init: function Init() {
      if ($.hood.App.Options.Header.Sticky) {
        $.hood.App.Header.StickyMenu();
        $.window.on('scroll', function () {
          $.hood.App.Header.StickyMenu();
        });
      }
    },
    StickyMenu: function StickyMenu() {
      var headerOffset = 0;
      var $header = $($.hood.App.Options.Header.Target);

      if ($header.length > 0) {
        headerOffset = $header.offset().top;
      }

      var header = $header.height();
      var win = $.window.height() + 2 * header;
      var doc = $(document).height();

      if ($.window.scrollTop() > headerOffset && doc > win) {
        $header.addClass($.hood.App.Options.Header.StickyClass);
      } else {
        $.hood.App.Header.RemoveStickyness();
      }
    },
    RemoveStickyness: function RemoveStickyness() {
      var $header = $($.hood.App.Options.Header.Target);

      if ($header.hasClass($.hood.App.Options.Header.StickyClass)) {
        $header.removeClass($.hood.App.Options.Header.StickyClass);
      }
    }
  },
  ContactForms: {
    Init: function Init() {
      $('.contact-form .thank-you').hide();
      $('.contact-form .form-submit').show();
      $('body').on('submit', '.contact-form', function (e) {
        e.preventDefault();
        $(this).addClass('loading');
        $.hood.App.ContactForms.Submit(this);
        return false;
      });
    },
    Submit: function Submit(tag) {
      var $form = $(tag);

      if ($form.valid()) {
        if ($form.hasClass('g-recaptcha')) {
          var recaptchaId = $form.find('.recaptcha').data('recaptchaid');

          if (grecaptcha.getResponse(recaptchaId) === "") {
            $.hood.Alerts.Error('Please tell us you are not a robot!', 'Confirm Humanity!', null, true);
            $form.removeClass('loading');
            return false;
          }
        }

        $.post($form.attr('action'), $form.serialize(), function (data) {
          if (data.Success) {
            if ($form.attr('data-redirect')) window.location = $form.attr('data-redirect');
            if ($form.attr('data-alert-message')) $.hood.Alerts.Success($form.attr('data-alert-message'), "Success", null, true);
            $form.find('.form').hide();
            $form.find('.thank-you').show();
          } else {
            if (typeof $form.attr('data-alert-error') !== 'undefined') $.hood.Alerts.Success($form.attr('data-alert-error'), "Error", null, true);
            $.hood.Alerts.Error("There was an error sending the message: " + data.Errors, "Error", null, true);
          }

          $form.removeClass('loading');
        });
      }

      return false;
    }
  },
  Alerts: function Alerts() {
    $(".alert.auto-dismiss").fadeTo(5000, 500).slideUp(500, function () {
      $(".alert.auto-dismiss").slideUp(500);
    });
  },
  Colorbox: function Colorbox() {
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
    Init: function Init() {
      // check for highlight.
      var highlight = $.getUrlVars()['highlight'];

      if ($.isNumeric(highlight)) {
        var $post = $('#post-' + highlight);
        $('html,body').animate({
          scrollTop: $post.offset().top - $.hood.App.Options.Scroll.Offset
        }, 'slow');
        $post.addClass('highlight');
        if ($.getUrlVars()['message'] === "Created") $post.addClass('created');
      }

      var reply = $.getUrlVars()['reply'];

      if ($.isNumeric(reply)) {
        var _$post = $('#forum-post-form');

        $('html,body').animate({
          scrollTop: _$post.offset().top - $.hood.App.Options.Scroll.Offset
        }, 'slow');
      } // toggle editors


      $('.forum').on('click', '.edit-post', function (e) {
        $(this).parents('.post').find('.post-view').slideToggle();
        $(this).parents('.post').find('.edit-view').slideToggle();
      });
    }
  },
  ResizeVideos: function ResizeVideos() {
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
    Init: function Init() {
      $('body').on('click', '.btn.price-select[data-target][data-value]', $.hood.App.PaymentPages.PriceSelect);
      $('body').on('click', '.change-price-option', $.hood.App.PaymentPages.ChangePrice);
    },
    ChangePrice: function ChangePrice() {
      $('#price-panel').collapse('show');
      $('#billing-panel').collapse('hide');
      $('#confirm-panel').collapse('hide');
    },
    PriceSelect: function PriceSelect() {
      var $this = $(this);
      targetId = '#' + $this.data('target');
      $(targetId).val($this.data('value'));
      $(".selected-price-text").html($(targetId).find(":selected").text());
      $('.price-select[data-target="' + $this.data('target') + '"]').each(function () {
        $(this).html($(this).data('temp')).removeClass('active');
      });
      $('.price-select[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () {
        $(this).data('temp', $(this).html()).html('Selected').addClass('active');
      });
      ;
      $('#price-panel').collapse('hide');
      $('#billing-panel').collapse('show');
      $('#confirm-panel').collapse('hide');
    }
  },
  RichTextEditors: function RichTextEditors() {
    tinymce.init({
      selector: '.tinymce-public',
      height: 250,
      plugins: ['advlist autolink lists link image charmap print preview anchor media', 'searchreplace visualblocks code fullscreen', 'insertdatetime media contextmenu paste emoticons'],
      menubar: false,
      toolbar: 'styleselect | bold italic | bullist numlist outdent indent | undo redo | link image media emoticons',
      image_dimensions: false,
      body_class: 'tiny-mce-body',
      content_css: '/css/site.css'
    });
  }
}; // Initialise

$(function () {
  $.hood.App.Ready();
});
$(window).on('load', $.hood.App.Load);
$(window).on('resize', $.hood.App.Resize);
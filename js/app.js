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
    ContactForms: true
  },
  Init: function Init(options) {
    $.hood.App.Options = $.extend($.hood.App.Options, options || {});

    if (options) {
      if (options.Header) $.hood.App.Options.Header = $.extend($.hood.App.Options.Header, options.Header || {});
    }

    if ($.hood.App.Options.Header.Sticky) $.hood.App.Header.Init();
    if ($.hood.App.Options.ContactForms) $.hood.App.ContactForms.Init();
    if ($.hood.App.Options.Alerts) $.hood.App.Alerts();
    if ($.hood.App.Options.Colorbox) $.hood.App.Colorbox();
  },
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
        $.post($form.attr('action'), $form.serialize(), function (data) {
          if (data.Success) {
            if ($form.attr('data-redirect')) window.location = $form.attr('data-redirect');
            if ($form.attr('data-alert-message')) $.hood.Alerts.Success($form.attr('data-alert-message'), "Success", null, true);
            $form.find('.form').hide();
            $form.find('.thank-you').show();
          } else {
            if ($form.attr('data-alert-error')) $.hood.Alerts.Error($form.attr('data-alert-error'), "Error", null, true);else $.hood.Alerts.Error("There was an error sending the message: " + data.Errors, "Error", null, true);
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
  Property: {
    Loaded: function Loaded(data) {
      $.hood.Loader(false);
      $.hood.Google.ClusteredMap();
    },
    Reload: function Reload(complete) {
      if ($('#property-list').doesExist()) $.hood.Inline.Reload($('#property-list'), complete);
    }
  }
};
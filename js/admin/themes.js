"use strict";

if (!$.hood) $.hood = {};
$.hood.Themes = {
  Init: function Init() {
    $('body').on('click', '.activate-theme', $.hood.Themes.Activate);
  },
  Loaded: function Loaded(data) {
    $.hood.Loader(false);
  },
  Reload: function Reload(complete) {
    if ($('#themes-list').doesExist()) $.hood.Inline.Reload($('#themes-list'), complete);
  },
  Activate: function Activate(e) {
    e.preventDefault();
    $tag = $(this);

    activateThemeCallback = function activateThemeCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Themes.Reload();
        });
      }
    };

    $.hood.Alerts.Confirm("The site will change themes, and the selected theme will be live right away.", "Are you sure?", activateThemeCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process will take effect immediately!</strong></span>');
  }
};
$(document).ready($.hood.Themes.Init);
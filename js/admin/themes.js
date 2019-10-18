"use strict";

var _coreJs = require("core-js");

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
    var $tag = $(this);

    var activateThemeCallback = function activateThemeCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          (0, _coreJs.setTimeout)(function () {
            $.hood.Themes.Reload();
          }, 2000);
        });
      }
    };

    $.hood.Alerts.Confirm("The site will change themes, and the selected theme will be live right away.", "Are you sure?", activateThemeCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process will take effect immediately!</strong></span>');
  }
};
$(document).ready($.hood.Themes.Init);
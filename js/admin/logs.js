"use strict";

if (!$.hood) $.hood = {};
$.hood.Logs = {
  Init: function Init() {
    $('body').on('click', '.log-inline', function (e) {
      e.preventDefault();
      $('#log-list').data('url', $(this).attr('href'));
      $.hood.Logs.ReloadLogsAjax();

      if ($('#log-list').hasClass('url-change')) {
        newUrl = window.location.pathname + "?" + $(this).attr('href').split('?')[1];
        history.pushState(null, null, newUrl);
      }
    });
    $.get($('#log-list').data('url'), null, function (data) {
      $('#log-list').empty().html(data);
    });
    var logReload = setInterval($.hood.Logs.ReloadLogs, 5000);
  },
  ReloadLogs: function ReloadLogs() {
    if ($('#log-list').hasClass('live') || $('#log-list-live').is(":checked")) {
      $.hood.Logs.ReloadLogsAjax();
    }
  },
  ReloadLogsAjax: function ReloadLogsAjax() {
    $.get($('#log-list').data('url'), null, function (data) {
      $('#log-list').empty().html(data);
    });
  }
};
$(window).on('load', function () {
  if ($('#log-list').doesExist()) {
    $.hood.Logs.Init();
  }
});
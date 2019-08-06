"use strict";

if (!$.hood) $.hood = {};
$.hood.Inline = {
  Tags: {},
  Init: function Init() {
    $('.hood-inline:not(.refresh)').each($.hood.Inline.Load);
    $('body').on('click', '.hood-inline-task', $.hood.Inline.Task);
    $('body').on('click', '.hood-modal', function (e) {
      e.preventDefault();
      $.hood.Inline.Modal($(this).attr('href'), $(this).data('complete'));
    });
    $.hood.Inline.DataList.Init();
  },
  Refresh: function Refresh(tag) {
    $(tag || '.hood-inline').each($.hood.Inline.Load);
  },
  Load: function Load() {
    $.hood.Inline.Reload(this);
  },
  Reload: function Reload(tag, complete) {
    var $tag = $(tag);
    $tag.addClass('loading');
    if (!complete) complete = $tag.data('complete');
    var urlLoad = $tag.data('url');
    $.get(urlLoad, $.proxy(function (data) {
      $(this).html(data);
      $(this).removeClass('loading');
    }, $tag)).done(function () {
      $.hood.Inline.RunComplete(complete);
    }).fail($.hood.Inline.HandleError).always($.hood.Inline.Finish);
  },
  CurrentModal: null,
  Modal: function Modal(url, complete) {
    var closePrevious = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : false;

    if (closePrevious && $.hood.Inline.CurrentModal) {
      $.hood.Inline.CurrentModal.modal('hide');
    }

    $.get(url, function (data) {
      var modalId = '#' + $(data).attr('id');
      $(data).addClass('hood-inline-modal');

      if ($(modalId).length) {
        $(modalId).remove();
      }

      $('body').append(data);
      $.hood.Inline.CurrentModal = $(modalId);
      $(modalId).modal(); // Workaround for sweetalert popups.

      $(modalId).on('shown.bs.modal', function () {
        $(document).off('focusin.modal');
      });
    }).done(function () {
      $.hood.Inline.RunComplete(complete);
    }).fail($.hood.Inline.HandleError).always($.hood.Inline.Finish);
  },
  CloseModal: function CloseModal() {
    if ($.hood.Inline.CurrentModal) {
      $.hood.Inline.CurrentModal.modal('hide');
    }
  },
  Task: function Task(e) {
    e.preventDefault();
    var $tag = $(e.currentTarget);
    $tag.addClass('loading');
    complete = $tag.data('complete');
    $.get($tag.attr('href'), function (data) {
      $.hood.Helpers.ProcessResponse(data);

      if (data.Success) {
        if ($tag && $tag.data('redirect')) {
          setTimeout(function () {
            window.location = $tag.data('redirect');
          }, 1500);
        }
      }

      $tag.removeClass('loading');
    }).done(function () {
      $.hood.Inline.RunComplete(complete);
    }).fail($.hood.Inline.HandleError).always($.hood.Inline.Finish);
  },
  DataList: {
    Init: function Init() {
      $('.hood-inline-list.query').each(function () {
        $(this).data('url', $(this).data('url') + window.location.search);
      });
      $('.hood-inline-list:not(.refresh)').each($.hood.Inline.Load);
      $('body').on('click', '.hood-inline-list .pagination a', function (e) {
        e.preventDefault();
        $.hood.Loader(true);
        var url = document.createElement('a');
        url.href = $(this).attr('href');
        var $list = $(this).parents('.hood-inline-list');
        $.hood.Inline.DataList.Reload($list, url);
      });
      $('body').on('submit', '.hood-inline-list form', function (e) {
        e.preventDefault();
        $.hood.Loader(true);
        var $form = $(this);
        var $list = $form.parents('.hood-inline-list');
        var url = document.createElement('a');
        url.href = $list.data('url');
        url.search = "?" + $form.serialize();
        $.hood.Inline.DataList.Reload($list, url);
      });
      $('body').on('submit', 'form.inline', function (e) {
        e.preventDefault();
        $.hood.Loader(true);
        var $form = $(this);
        var $list = $($form.data('target'));
        var url = document.createElement('a');
        url.href = $list.data('url');
        url.search = "?" + $form.serialize();
        $.hood.Inline.DataList.Reload($list, url);
      });
      $('body').on('change', 'form.inline .refresh-on-change, .hood-inline-list form', function (e) {
        e.preventDefault();
        $.hood.Loader(true);
        var $form = $(this).parents('form');
        var $list = $($form.data('target'));
        var url = document.createElement('a');
        url.href = $list.data('url');
        url.search = "?" + $form.serialize();
        $.hood.Inline.DataList.Reload($list, url);
      });
    },
    Reload: function Reload(list, url) {
      if (history.pushState && list.hasClass('query')) {
        var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + '?' + url.href.substring(url.href.indexOf('?') + 1);
        window.history.pushState({
          path: newurl
        }, '', newurl);
      }

      list.data('url', $.hood.Helpers.InsertQueryStringParamToUrl(url, 'inline', 'true'));
      $.hood.Inline.Reload(list);
    }
  },
  HandleError: function HandleError(xhr) {
    if (xhr.status === 500) {
      $.hood.Alerts.Error("<strong>Error " + xhr.status + "</strong><br />There was an error processing the content, please contact an administrator if this continues.<br/>");
    } else if (xhr.status === 404) {
      $.hood.Alerts.Error("<strong>Error " + xhr.status + "</strong><br />The content could not be found.<br/>");
    } else if (xhr.status === 401) {
      $.hood.Alerts.Error("<strong>Error " + xhr.status + "</strong><br />You are not allowed to view this resource, are you logged in correctly?<br/>");
      window.location = window.location;
    }
  },
  Finish: function Finish(data) {
    // Function can be overridden, to add global functionality to end of inline loads.
    $.hood.Loader(false);
  },
  RunComplete: function RunComplete(complete) {
    if (!$.hood.Helpers.IsNullOrUndefined(complete)) {
      var func = eval(complete);

      if (typeof func === 'function') {
        func();
      }
    }
  }
};
$(document).ready($.hood.Inline.Init); // Backwards compatibility.

$.hood.Modals = {
  Open: $.hood.Inline.Modal
};
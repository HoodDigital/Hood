if (!$.hood)
    $.hood = {};

$.hood.Inline = {
    Init: function () {
        $('.hood-inline:not(.refresh)').each($.hood.Inline.Load);
        $('body').on('click', '.hood-inline-task', $.hood.Inline.Task);
        $('body').on('click', '.hood-modal', function (e) {
            e.preventDefault();
            $.hood.Inline.Modal($(this).attr('href'), $(this).data('complete'));
        });
        $.hood.Inline.DataList.Init();
    },
    Refresh: function (tag) {
        $(tag || '.hood-inline').each($.hood.Inline.Load);
    },
    Load: function (e) {
        $.hood.Inline.Reload(this);
    },
    Reload: function (tag) {
        $tag = $(tag);
        $tag.addClass('loading');
        var urlLoad = $tag.data('url');
        $.get(urlLoad, function (data) {
            $tag.html(data);
            $tag.removeClass('loading');
        })
            .done(function (data) { $.hood.Inline.RunComplete($tag.data('complete'), data); })
            .fail($.hood.Inline.HandleError)
            .always($.hood.Inline.Finish);
    },
    Modal: function (url, complete) {
        $.get(url, function (data) {
            modalId = '#' + $(data).attr('id');
            $(data).addClass('hood-inline-modal');

            if ($(modalId).length) {
                $(modalId).remove();
            }

            $('body').append(data);
            $(modalId).modal();

            // Workaround for sweetalert popups.
            $(modalId).on('shown.bs.modal', function () {
                $(document).off('focusin.modal');
            });
        })
            .done(function (data) { $.hood.Inline.RunComplete(complete, data); })
            .fail($.hood.Inline.HandleError)
            .always($.hood.Inline.Finish);
    },
    Task: function (e) {
        e.preventDefault();

        $tag = $(e.currentTarget);
        $tag.addClass('loading');

        $.get($tag.attr('href'), function (data) {
            if (data.Success) {
                $.hood.Alerts.Success(data.Message);
            } else {
                $.hood.Alerts.Error(data.Errors, data.Message);
            }
            if (data.Url) {
                setTimeout(function () {
                    window.location = data.Url;
                }, 500);
            }
            $tag.removeClass('loading');
        })
            .done(function (data) { $.hood.Inline.RunComplete(complete, data); })
            .fail($.hood.Inline.HandleError)
            .always($.hood.Inline.Finish);
    },
    DataList: {
        Init: function () {
            $('.hood-inline-list.query').each(function () {
                $(this).data('url', $(this).data('url') + window.location.search);
            });
            $('.hood-inline-list:not(.refresh)').each($.hood.Inline.Load);
            $('body').on('click', '.hood-inline-list .pagination a', function (e) {
                e.preventDefault();
                $.hood.Loader(true);
                var url = document.createElement('a');
                url.href = $(this).attr('href');
                $list = $(this).parents('.hood-inline-list');
                $.hood.Inline.DataList.Reload($list, url);
            });
            $('body').on('submit', '.hood-inline-list form', function (e) {
                e.preventDefault();
                $.hood.Loader(true);
                $form = $(this);
                $list = $form.parents('.hood-inline-list');
                var url = document.createElement('a');
                url.href = $list.data('url');
                url.search = "?" + $form.serialize();
                $.hood.Inline.DataList.Reload($list, url);
            });
            $('body').on('submit', 'form.inline', function (e) {
                e.preventDefault();
                $.hood.Loader(true);
                $form = $(this);
                $list = $($form.data('target'));
                var url = document.createElement('a');
                url.href = $list.data('url');
                url.search = "?" + $form.serialize();
                $.hood.Inline.DataList.Reload($list, url);
            });
            $('body').on('change', 'form.inline .refresh-on-change, .hood-inline-list form', function (e) {
                e.preventDefault();
                $.hood.Loader(true);
                $form = $(this).parents('form');
                $list = $($form.data('target'));
                var url = document.createElement('a');
                url.href = $list.data('url');
                url.search = "?" + $form.serialize();
                $.hood.Inline.DataList.Reload($list, url);
            });
        },
        Reload: function (list, url) {
            if (history.pushState && list.hasClass('query')) {
                var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + '?' + url.href.substring(url.href.indexOf('?') + 1);
                window.history.pushState({ path: newurl }, '', newurl);
            }
            list.data('url', $.hood.Helpers.InsertQueryStringParamToUrl(url, 'inline', 'true'));
            $.hood.Inline.Reload(list, list.data('complete'));
        }
    },
    HandleError: function (xhr) {
        if (xhr.status === 500) {
            $.hood.Alerts.Error("<strong>" + xhr.status + "</strong>: There was an error processing the content, please contact an administrator if this continues.<br/>");
        } else if (xhr.status === 404) {
            $.hood.Alerts.Error("<strong>" + xhr.status + "</strong>: The content could not be found.<br/>");
        } else if (xhr.status === 401) {
            $.hood.Alerts.Error("<strong>" + xhr.status + "</strong>: You are not allowed to view this resource, are you logged in correctly?<br/>");
        }
    },
    Finish: function (data) {
        // Function can be overridden, to add global functionality to end of inline loads.
        $.hood.Loader(false);
    },
    RunComplete: function (complete, data = null) {
        if (!$.hood.Helpers.IsNullOrUndefined(complete)) {
            var func = eval(complete);
            if (typeof func === 'function') {
                if (data !== null) {
                    func(data);
                } else {
                    func();
                }
            }
        }
    }
};
$.hood.Inline.Init();

// Backwards compatibility.
$.hood.Modals = {
    Open: $.hood.Inline.Modal
};


if (!$.hood)
    $.hood = {};

$.hood.Inline = {
    Init: function () {
        $('.hood-inline:not(.refresh)').each($.hood.Inline.Load);
        $('body').on('click', '.hood-inline-task', $.hood.Inline.Task);
        $.hood.Inline.DataList.Init();
    },
    Refresh: function () {
        $('.hood-inline').each($.hood.Inline.Load);
    },
    Load: function (e) {
        $.hood.Inline.Reload(this);
    },
    Reload: function (tag, complete) {
        $(tag).addClass('loading');
        params = null;
        if ($(tag).attr('data-params')) {
            params = eval($(tag).data('params'));
        }
        var urlLoad = $(tag).data('url');
        $.get(urlLoad, params, function (data) {
            $(tag).html(data);
            if (!$.hood.Helpers.IsNullOrUndefined(complete)) {
                if ($.hood.Helpers.IsFunction(complete))
                    complete(data);
                else
                    eval(complete + "(data)");
            }
            if ($(tag).attr("data-complete")) {
                eval($(tag).data('complete') + "(data)");
            }
        })
        .fail(function (data) {
            $.hood.Alerts.Error("There was an error loading the inline panel's URL:<br/><strong>" + urlLoad + "</strong>");
        })
        .always(function (data) {
            $.hood.Modals.Loading = false;
            $(tag).removeClass('loading');
        });
    },
    Task: function (e) {
        $.hood.Loader(true);
        e.preventDefault();
        $tag = $(e.currentTarget);
        $tagcontents = $(e.currentTarget).html();
        $(e.currentTarget).addClass('loading');
        var urlLoad = $(e.currentTarget).attr('href');
        $.get(urlLoad, null, function (data) {
            if (data.Success) {
                $.hood.Alerts.Success(data.Message);
            } else {
                $.hood.Alerts.Error(data.Errors);
            }
            if (data.Url) {
                setTimeout(function () {
                    window.location = data.Url;
                }, 500);
            }
            if ($tag.attr("data-complete")) {
                eval($tag.data('complete') + "(data)");
            }
        })
            .fail(function (data) {
                $.hood.Alerts.Error("There was an error processing the request:<br/><strong>" + urlLoad + "</strong>");
            })
            .always(function (data) {
                $.hood.Loader(false);
                $.hood.Modals.Loading = false;
                $(e.currentTarget).removeClass('loading');
            });
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
        Reload: function(list, url) {
            if (history.pushState && list.hasClass('query')) {
                var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + '?' + url.href.substring(url.href.indexOf('?') + 1);
                window.history.pushState({ path: newurl }, '', newurl);
            }
            list.data('url', $.hood.Helpers.InsertQueryStringParamToUrl(url, 'inline', 'true'));
            $.hood.Inline.Reload(list, list.data('complete'));
        }
    }
};
$.hood.Inline.Init();

 

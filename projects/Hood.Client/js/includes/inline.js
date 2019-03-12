if (!$.hood)
    $.hood = {}

$.hood.Inline = {
    Init: function () {
        $('.hood-inline:not(.refresh)').each($.hood.Inline.Load)
        $('body').on('click', '.hood-inline-task', $.hood.Inline.Task);
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
        if (($(tag).attr('data-params'))) {
            params = eval($(tag).data('params'));
        }
        var urlLoad = $(tag).data('url');
        $.get(urlLoad, params, function (data) {
            $(tag).html(data);
            try {
                $.hood.Helpers.InitMetisMenu(tag);
            } catch (ex) { }
            try {
                $.hood.Helpers.InitIboxes(tag);
            } catch (ex) { }
            if (!$.hood.Helpers.IsNullOrUndefined(complete)) {
                complete(data);
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
                $.hood.Alerts.Error("There was an error processing the AJAX request:<br/><strong>" + urlLoad + "</strong>");
            })
            .always(function (data) {
                $.hood.Modals.Loading = false;
                $(e.currentTarget).removeClass('loading');
            });
    }
};
$.hood.Inline.Init();
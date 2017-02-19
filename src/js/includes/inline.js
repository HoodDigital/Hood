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
        if ($.hood.Helpers.IsNullOrUndefined($(tag).data('params'))) {
            params = eval($(tag).data('params'));
        }
        $.get($(tag).data('url'), params, function (data) {
            $(tag).html(data);
            try {
                $.hood.Helpers.InitMetisMenu(tag);
            } catch (ex) { }
            try {
                $.hood.Helpers.InitIboxes(tag);
            } catch (ex) { }
        })
        .fail(function (data) {
            $.hood.Alerts.Error("There was an error loading the panel.<br/><br />" + data.status + " - " + data.statusText);
        })
        .always(function (data) {
            if (!$.hood.Helpers.IsNullOrUndefined($(tag).data('complete'))) {
                eval($(tag).data('complete'));
            }
            if (!$.hood.Helpers.IsNullOrUndefined(complete)) {
                complete();
            }
            $.hood.Modals.Loading = false;
            $(tag).removeClass('loading');
        });
    },
    Task: function (e) {
        e.preventDefault();
        $tagcontents = $(e.currentTarget).html();
        $(e.currentTarget).addClass('loading');
        $.get($(e.currentTarget).attr('href'), null, function (data) {
            if (data.Success) {
                $.hood.Alerts.Success(data.Message);
            } else {
                $.hood.Alerts.Error(data.Errors);
            }
        })
        .done($.proxy(function () {
            if (!$.hood.Helpers.IsNullOrUndefined($(e.currentTarget).data('complete'))) {
                eval($(e.currentTarget).data('complete'));
            }
        }, e.currentTarget))
        .fail(function (data) {
            $.hood.Alerts.Error("There was an error during the operation.<br/><br />" + data.status + " - " + data.statusText);
        })
        .always(function (data) {
            $.hood.Modals.Loading = false;
            $(e.currentTarget).removeClass('loading');
        });
    }
};
$.hood.Inline.Init();
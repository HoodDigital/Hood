if (!$.hood)
    $.hood = {};
$.hood.Logs = {
    Init: function () {
        $('body').on('change', '.logs-inline', $.hood.Logs.InlineToggle);
    },

    Loaded: function(sender, data) {
        $.hood.Loader(false);
    },
    Reload: function (complete) {
        if ($('#logs-list').doesExist())
            $.hood.Inline.Reload($('#logs-list'), complete);
    },

    ReloadInterval: null,
    InlineToggle: function (e) {
        if ($(this).is(':checked')) {
            $.hood.Logs.ReloadInterval = setInterval($.hood.Logs.Reload, 5000);
        }
        else {
            $.hood.Logs.ReloadInterval = setInterval($.hood.Logs.Reload, 5000);
        }
    }
};
$(document).ready($.hood.Logs.Init);

if (!$.hood)
    $.hood = {};
$.hood.Themes = {
    Init: function() {
        $('body').on('click', '.activate-theme', $.hood.Themes.Activate);
    },

    Loaded: function(sender, data) {
        $.hood.Loader(false);
    },
    Reload: function(complete) {
        if ($('#themes-list').doesExist())
            $.hood.Inline.Reload($('#themes-list'), complete);
    },

    Activate: function(e) {
        e.preventDefault();
        let $tag = $(this);

        let activateThemeCallback = function(isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function(data) {
                    $.hood.Helpers.ProcessResponse(data);
                    setTimeout(function() { $.hood.Themes.Reload(); }, 2000);
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The site will change themes, and the selected theme will be live right away.",
            "Are you sure?",
            activateThemeCallback,
            'error',
            '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process will take effect immediately!</strong></span>',
        );
    }
};
$(document).ready($.hood.Themes.Init);

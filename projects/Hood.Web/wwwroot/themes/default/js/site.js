if (!$.hood)
    $.hood = {};

$.hood.Site = {
    Init: function () {
        // Add any init time functionality here - when the code is first loaded.

    },
    Ready: function () {
        $.hood.Site.Resize();
        // Add any ready time functionality here - when the document is ready.

    },
    Load: function () {
        // Add any load time functionality here - when the document is loaded.

    },
    Resize: function () {
        // Add any resize functionality here - whenever the window is resized.

    }
};

// Initialise
$(function () { $.hood.Site.Ready(); });
$(window).on('load', $.hood.Site.Load);
$(window).on('resize', $.hood.Site.Resize);

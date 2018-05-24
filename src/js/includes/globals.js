if (!$.hood)
    $.hood = {}
$.body = $('body');
// Console hack
var console = window.console || {};
console.log = console.log || function () { };
console.warn = console.warn || function () { };
console.error = console.error || function () { };
console.info = console.info || function () { };
$.fn.doesExist = function () {
    return $(this).length > 0;
};
$.fn.restrictToSlug = function (restrictPattern) {
    var targets = $(this);

    // The characters inside this pattern are accepted
    // and everything else will be 'cleaned'
    // For example 'ABCdEfGhI5' become 'ABCEGI5'
    var pattern = restrictPattern ||
        /[^0-9a-zA-Z-//]*/g; // default pattern

    var restrictHandler = function () {
        var val = $(this).val();
        var newVal = val.replace(pattern, '');

        // This condition is to prevent selection and keyboard navigation issues
        if (val !== newVal) {
            $(this).val(newVal);
        }
    };

    targets.on('keyup', restrictHandler);
    targets.on('paste', restrictHandler);
    targets.on('change', restrictHandler);
};
$('.restrict-to-slug').restrictToSlug();
$.fn.restrictToPageSlug = function (restrictPattern) {
    var targets = $(this);

    // The characters inside this pattern are accepted
    // and everything else will be 'cleaned'
    var pattern = restrictPattern ||
        /[^0-9a-zA-Z-//]*/g; // default pattern

    var restrictHandler = function () {
        var val = $(this).val();
        var newVal = val.replace(pattern, '');
        if ((newVal.match(new RegExp("/", "g")) || []).length > 4) {
            var pos = newVal.lastIndexOf('/');
            newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
            $.hood.Alerts.Warning("You can only have up to 4 '/' characters in a url slug.");
        }
        // This condition is to prevent selection and keyboard navigation issues
        if (val !== newVal) {
            $(this).val(newVal);
        }
    };

    targets.on('keyup', restrictHandler);
    targets.on('paste', restrictHandler);
    targets.on('change', restrictHandler);
};
$('.restrict-to-page-slug').restrictToPageSlug();
$.fn.characterCounter = function (val) {
    var targets = $(this);
    var characterCounterHandler = function () {
        counter = $(this).data('counter');
        max = Number($(this).attr('maxlength'));
        len = $(this).val().length;
        $(counter).text(max - len);
        cls = "text-success";
        if (max - len < max / 10)
            cls = "text-danger"
        $(counter).parent().removeClass('text-success').removeClass('text-danger').addClass(cls);
    };
    targets.on('keyup', characterCounterHandler);
    targets.on('paste', characterCounterHandler);
    targets.on('change', characterCounterHandler);
};
$('.character-counter').characterCounter();
$('.character-counter').trigger('change');
$.fn.addLoader = function () {
    $(this).data('loadercontent', $(this).html());
    $(this).addClass('loading').append('<i class="fa fa-refresh fa-spin m-l-sm"></i>');
};
$.fn.removeLoader = function () {
    $(this).empty().html($(this).data('loadercontent'));
    $(this).removeClass('loading');
};
$.fn.warningAlert = function (val) {
    var targets = $(this);
    var warningAlertHandler = function (e) {
        var target = e.currentTarget;
        $.loadCss('sweetalert-css', '/lib/sweetalert/dist/sweetalert.css');
        $.getScript('/lib/sweetalert/dist/sweetalert.min.js', $.proxy(function () {
            swal({
                title: "Whoa!",
                text: targets.data('warning'),
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes, go ahead.",
                cancelButtonText: "No, cancel!",
                closeOnConfirm: false,
                showLoaderOnConfirm: true,
                closeOnCancel: false
            },
                function (isConfirm) {
                    if (isConfirm) {
                        url = $(target).attr('href');
                        window.location = url;
                    } else {
                        swal("Cancelled", "It's all good in the hood!", "error");
                    }
                });
        }, this));
        return false;
    };
    targets.on('click', warningAlertHandler);
};
$('.warning-alert').warningAlert();
$.getPlaceholders = function (str) {
    var regex = /\{(\w+)\}/g;
    var result = [];
    while (match = regex.exec(str)) {
        result.push(match[1]);
    }
    return result;
}
$.commonHeight = function (element, columnTag) {
    var maxHeight = 0;
    element.children(columnTag).each(function () {
        var elementChild = $(this).children();
        if (elementChild.hasClass('max-height')) {
            maxHeight = elementChild.outerHeight();
        } else {
            if (elementChild.outerHeight() > maxHeight)
                maxHeight = elementChild.outerHeight();
        }
    });
    element.children(columnTag).each(function () {
        $(this).height(maxHeight);
    });
}
$.loadCss = function (id, location) {
    if (!$('link#' + id).length)
        $('<link/>', {
            id: id,
            rel: 'stylesheet',
            type: 'text/css',
            href: location
        }).appendTo('head');
};
$.getUrlVars = function () {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}
$.decodeUrl = function (str) {
    return decodeURIComponent(str).replace('+', ' ');
}
$.numberWithCommas = function (x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
if (typeof kendo !== 'undefined') {
    kendo.data.binders.date = kendo.data.Binder.extend({
        init: function (element, bindings, options) {
            kendo.data.Binder.fn.init.call(this, element, bindings, options);

            this.dateformat = $(element).data("dateformat");
        },
        refresh: function () {
            var data = this.bindings["date"].get();
            if (data) {
                var dateObj = new Date(data);
                $(this.element).text(kendo.toString(dateObj, this.dateformat));
            }
        }
    });
}
if ($.validator) {
    $.validator.addMethod("time", function (value, element) {
        return this.optional(element) || /^(([0-1]?[0-9])|([2][0-3])):([0-5]?[0-9])(:([0-5]?[0-9]))?$/i.test(value);
    }, "Please enter a valid time.");
    $.validator.addMethod(
        "ukdate",
        function (value, element) {
            // put your own logic here, this is just a (crappy) example
            return value.match(/^\d\d?\/\d\d?\/\d\d\d\d$/);
        },
        "Please enter a date in the format dd/mm/yyyy."
    );
}
$.mobile = {
    Android: function () {
        return navigator.userAgent.match(/Android/i);
    },
    BlackBerry: function () {
        return navigator.userAgent.match(/BlackBerry/i);
    },
    iOS: function () {
        return navigator.userAgent.match(/iPhone|iPad|iPod/i);
    },
    Opera: function () {
        return navigator.userAgent.match(/Opera Mini/i);
    },
    Windows: function () {
        return navigator.userAgent.match(/IEMobile/i);
    },
    Any: function () {
        return ($.mobile.Android() || $.mobile.BlackBerry() || $.mobile.iOS() || $.mobile.Opera() || $.mobile.Windows());
    }
};
if (!$.mobile.Android) {
    $.body.addClass("android-device");
    $.device = "android";
} else if (!$.mobile.BlackBerry) {
    $.body.addClass("blackberry-device");
    $.device = "blackberry";
} else if (!$.mobile.iOS) {
    $.body.addClass("ios-device");
    $.device = "ios";
} else if (!$.mobile.Opera) {
    $.body.addClass("opera-device");
    $.device = "opera";
} else if (!$.mobile.Windows) {
    $.body.addClass("windows-device");
    $.device = "windows";
} else {
    $.body.addClass("desktop-device");
    $.device = "desktop";
}

// Custom Event polyfill
(function () {
    if (typeof window.CustomEvent === "function") return false;
    function CustomEvent(event, params) {
        params = params || { bubbles: false, cancelable: false, detail: undefined };
        var evt = document.createEvent('CustomEvent');
        evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail);
        return evt;
    }
    CustomEvent.prototype = window.Event.prototype;
    window.CustomEvent = CustomEvent;
})();
$.hood.LinkClasses = [
    { title: 'None', value: '' },
    { title: 'Button link', value: 'btn btn-default' },
    { title: 'Theme coloured button link', value: 'btn btn-primary' },
    { title: 'Popup image/video', value: 'colorbox-iframe' },
    { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
    { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
    { title: 'Large link', value: 'font-lg' },
    { title: 'Large button link', value: 'btn btn-default btn-lg' },
    { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
    { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
    { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
    { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
];
$.hood.ImageClasses = [
    { title: 'None', value: '' },
    { title: 'Full Width', value: 'user-image full' },
    { title: 'Left Aligned', value: 'user-image left' },
    { title: 'Centered', value: 'user-image center' },
    { title: 'Right Aligned', value: 'user-image right' },
    { title: 'Inline with text, top aligned', value: 'user-image inline top' },
    { title: 'Inline with text, middle aligned', value: 'user-image inline' },
    { title: 'Inline with text, bottom aligned', value: 'user-image inline bottom' },
    { title: 'Pulled Left', value: 'user-image pull-left' },
    { title: 'Pulled Right', value: 'user-image pull-right' },
];
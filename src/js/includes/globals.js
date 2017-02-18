$.body = $('body');
// Console hack
var console = window.console || {};
console.log = console.log || function () { };
console.warn = console.warn || function () { };
console.error = console.error || function () { };
console.info = console.info || function () { };
/*! jRespond.js v 0.10 | Author: Jeremy Fields [jeremy.fields@viget.com], 2013 | License: MIT */
!function (a, b, c) { "object" == typeof module && module && "object" == typeof module.exports ? module.exports = c : (a[b] = c, "function" == typeof define && define.amd && define(b, [], function () { return c })) }(this, "jRespond", function (a, b, c) { "use strict"; return function (a) { var b = [], d = [], e = a, f = "", g = "", i = 0, j = 100, k = 500, l = k, m = function () { var a = 0; return a = "number" != typeof window.innerWidth ? 0 !== document.documentElement.clientWidth ? document.documentElement.clientWidth : document.body.clientWidth : window.innerWidth }, n = function (a) { if (a.length === c) o(a); else for (var b = 0; b < a.length; b++) o(a[b]) }, o = function (a) { var e = a.breakpoint, h = a.enter || c; b.push(a), d.push(!1), r(e) && (h !== c && h.call(null, { entering: f, exiting: g }), d[b.length - 1] = !0) }, p = function () { for (var a = [], e = [], h = 0; h < b.length; h++) { var i = b[h].breakpoint, j = b[h].enter || c, k = b[h].exit || c; "*" === i ? (j !== c && a.push(j), k !== c && e.push(k)) : r(i) ? (j === c || d[h] || a.push(j), d[h] = !0) : (k !== c && d[h] && e.push(k), d[h] = !1) } for (var l = { entering: f, exiting: g }, m = 0; m < e.length; m++) e[m].call(null, l); for (var n = 0; n < a.length; n++) a[n].call(null, l) }, q = function (a) { for (var b = !1, c = 0; c < e.length; c++) if (a >= e[c].enter && a <= e[c].exit) { b = !0; break } b && f !== e[c].label ? (g = f, f = e[c].label, p()) : b || "" === f || (f = "", p()) }, r = function (a) { if ("object" == typeof a) { if (a.join().indexOf(f) >= 0) return !0 } else { if ("*" === a) return !0; if ("string" == typeof a && f === a) return !0 } }, s = function () { var a = m(); a !== i ? (l = j, q(a)) : l = k, i = a, setTimeout(s, l) }; return s(), { addFunc: function (a) { n(a) }, getBreakpoint: function () { return f } } } }(this, this.document));
var jRes = jRespond([
	{
	    label: 'smallest',
	    enter: 0,
	    exit: 479
	}, {
	    label: 'handheld',
	    enter: 480,
	    exit: 767
	}, {
	    label: 'tablet',
	    enter: 768,
	    exit: 991
	}, {
	    label: 'laptop',
	    enter: 992,
	    exit: 1199
	}, {
	    label: 'desktop',
	    enter: 1200,
	    exit: 10000
	}
]);
jRes.addFunc([
    {
        breakpoint: 'desktop',
        enter: function () { $.body.addClass('device-lg'); },
        exit: function () { $.body.removeClass('device-lg'); }
    }, {
        breakpoint: 'laptop',
        enter: function () { $.body.addClass('device-md'); },
        exit: function () { $.body.removeClass('device-md'); }
    }, {
        breakpoint: 'tablet',
        enter: function () { $.body.addClass('device-sm'); },
        exit: function () { $.body.removeClass('device-sm'); }
    }, {
        breakpoint: 'handheld',
        enter: function () { $.body.addClass('device-xs'); },
        exit: function () { $.body.removeClass('device-xs'); }
    }, {
        breakpoint: 'smallest',
        enter: function () { $.body.addClass('device-xxs'); },
        exit: function () { $.body.removeClass('device-xxs'); }
    }
]);
$.fn.doesExist = function () {
    return $(this).length > 0;
};
$.fn.restrictToSlug = function (restrictPattern) {
    var targets = $(this);

    // The characters inside this pattern are accepted
    // and everything else will be 'cleaned'
    // For example 'ABCdEfGhI5' become 'ABCEGI5'
    var pattern = restrictPattern ||
        /[^0-9a-z-]*/g; // default pattern

    var restrictHandler = function () {
        var val = $(this).val();
        var newVal = val.replace(pattern, '').toLowerCase();

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
    // For example 'ABCdEfGhI5' become 'ABCEGI5'
    var pattern = restrictPattern ||
        /[^0-9a-z-//]*/g; // default pattern

    var restrictHandler = function () {
        var val = $(this).val();
        var newVal = val.replace(pattern, '').toLowerCase();
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
$.commonHeight = function (element) {
    var maxHeight = 0;
    element.children('.mega-menu-column').each(function () {
        var elementChild = $(this).children();
        if (elementChild.hasClass('max-height')) {
            maxHeight = elementChild.outerHeight();
        } else {
            if (elementChild.outerHeight() > maxHeight)
                maxHeight = elementChild.outerHeight();
        }
    });
    element.children('.mega-menu-column').each(function () {
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

$.decodeUrl = function (str) {
    return decodeURIComponent(str).replace('+', ' ');
}

/*
 * Javascript Humane Dates
 * Copyright (c) 2008 Dean Landolt (deanlandolt.com)
 * Re-write by Zach Leatherman (zachleat.com)
 *
 * Adopted from the John Resig's pretty.js
 * at http://ejohn.org/blog/javascript-pretty-date
 * and henrah's proposed modification
 * at http://ejohn.org/blog/javascript-pretty-date/#comment-297458
 *
 * Licensed under the MIT license.
 */

function humaneDate(date, compareTo) {

    if (!date) {
        return;
    }

    var lang = {
        ago: 'Ago',
        from: '',
        now: 'Just Now',
        minute: 'Minute',
        minutes: 'Minutes',
        hour: 'Hour',
        hours: 'Hours',
        day: 'Day',
        days: 'Days',
        week: 'Week',
        weeks: 'Weeks',
        month: 'Month',
        months: 'Months',
        year: 'Year',
        years: 'Years'
    },
        formats = [
            [60, lang.now],
            [3600, lang.minute, lang.minutes, 60], // 60 minutes, 1 minute
            [86400, lang.hour, lang.hours, 3600], // 24 hours, 1 hour
            [604800, lang.day, lang.days, 86400], // 7 days, 1 day
            [2628000, lang.week, lang.weeks, 604800], // ~1 month, 1 week
            [31536000, lang.month, lang.months, 2628000], // 1 year, ~1 month
            [Infinity, lang.year, lang.years, 31536000] // Infinity, 1 year
        ],
        isString = typeof date === 'string',
        date = isString ?
                    new Date(('' + date).replace(/-/g, "/").replace(/[TZ]/g, " ")) :
                    date,
        compareTo = compareTo || new Date,
        seconds = (compareTo - date +
                        (compareTo.getTimezoneOffset() -
                            // if we received a GMT time from a string, doesn't include time zone bias
                            // if we got a date object, the time zone is built in, we need to remove it.
                            (isString ? 0 : date.getTimezoneOffset())
                        ) * 60000
                    ) / 1000,
        token;

    if (seconds < 0) {
        seconds = Math.abs(seconds);
        token = lang.from ? ' ' + lang.from : '';
    } else {
        token = lang.ago ? ' ' + lang.ago : '';
    }

    /*
     * 0 seconds && < 60 seconds        Now
     * 60 seconds                       1 Minute
     * > 60 seconds && < 60 minutes     X Minutes
     * 60 minutes                       1 Hour
     * > 60 minutes && < 24 hours       X Hours
     * 24 hours                         1 Day
     * > 24 hours && < 7 days           X Days
     * 7 days                           1 Week
     * > 7 days && < ~ 1 Month          X Weeks
     * ~ 1 Month                        1 Month
     * > ~ 1 Month && < 1 Year          X Months
     * 1 Year                           1 Year
     * > 1 Year                         X Years
     *
     * Single units are +10%. 1 Year shows first at 1 Year + 10%
     */

    function normalize(val, single) {
        var margin = 0.1;
        if (val >= single && val <= single * (1 + margin)) {
            return single;
        }
        return val;
    }

    for (var i = 0, format = formats[0]; formats[i]; format = formats[++i]) {
        if (seconds < format[0]) {
            if (i === 0) {
                // Now
                return format[1];
            }

            var val = Math.ceil(normalize(seconds, format[3]) / (format[3]));
            return val +
                    ' ' +
                    (val !== 1 ? format[2] : format[1]) +
                    (i > 0 ? token : '');
        }
    }
};

if (typeof jQuery !== 'undefined') {
    jQuery.fn.humaneDates = function (options) {
        var settings = jQuery.extend({
            'lowercase': false
        }, options);

        return this.each(function () {
            var $t = jQuery(this),
                date = $t.attr('datetime') || $t.attr('title');

            date = humaneDate(date);

            if (date && settings['lowercase']) {
                date = date.toLowerCase();
            }

            if (date && $t.html() !== date) {
                // don't modify the dom if we don't have to
                $t.html(date);
            }
        });
    };
}

(function (window, document, undefined) {

    /*
     * Grab all iframes on the page or return
     */
    var iframes = document.getElementsByTagName('iframe');

    /*
     * Loop through the iframes array
     */
    for (var i = 0; i < iframes.length; i++) {

        var iframe = iframes[i],

        /*
           * RegExp, extend this if you need more players
           */
        players = /www.youtube.com|player.vimeo.com|www.londonlive.co.uk|www.slideshare.net|www.ustream.tv/;

        /*
         * If the RegExp pattern exists within the current iframe
         */
        if (iframe.src.search(players) > 0) {

            /*
             * Calculate the video ratio based on the iframe's w/h dimensions
             */
            var videoRatio = (iframe.height / iframe.width) * 100;

            /*
             * Replace the iframe's dimensions and position
             * the iframe absolute, this is the trick to emulate
             * the video ratio
             */
            iframe.style.position = 'absolute';
            iframe.style.top = '0';
            iframe.style.left = '0';
            iframe.width = '100%';
            iframe.height = '100%';

            /*
             * Wrap the iframe in a new <div> which uses a
             * dynamically fetched padding-top property based
             * on the video's w/h dimensions
             */
            var wrap = document.createElement('div');
            wrap.className = 'fluid-vids';
            wrap.style.width = '100%';
            wrap.style.position = 'relative';
            wrap.style.paddingTop = videoRatio + '%';

            /*
             * Add the iframe inside our newly created <div>
             */
            var iframeParent = iframe.parentNode;
            iframeParent.insertBefore(wrap, iframe);
            wrap.appendChild(iframe);

        }

    }

})(window, document);

var dropdownSelectors = $('.dropdown, .dropup');

// Custom function to read dropdown data
// =========================
function dropdownEffectData(target) {
    // @todo - page level global?
    var effectInDefault = null,
        effectOutDefault = null;
    var dropdown = $(target),
        dropdownMenu = $('.dropdown-menu', target);
    var parentUl = dropdown.parents('ul.nav');

    // If parent is ul.nav allow global effect settings
    if (parentUl.size() > 0) {
        effectInDefault = parentUl.data('dropdown-in') || null;
        effectOutDefault = parentUl.data('dropdown-out') || null;
    }

    return {
        target: target,
        dropdown: dropdown,
        dropdownMenu: dropdownMenu,
        effectIn: dropdownMenu.data('dropdown-in') || effectInDefault,
        effectOut: dropdownMenu.data('dropdown-out') || effectOutDefault,
    };
}

// Custom function to start effect (in or out)
// =========================
function dropdownEffectStart(data, effectToStart) {
    if (effectToStart) {
        data.dropdown.addClass('dropdown-animating');
        data.dropdownMenu.addClass('animated');
        data.dropdownMenu.addClass(effectToStart);
    }
}

// Custom function to read when animation is over
// =========================
function dropdownEffectEnd(data, callbackFunc) {
    var animationEnd = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
    data.dropdown.one(animationEnd, function () {
        data.dropdown.removeClass('dropdown-animating');
        data.dropdownMenu.removeClass('animated');
        data.dropdownMenu.removeClass(data.effectIn);
        data.dropdownMenu.removeClass(data.effectOut);

        // Custom callback option, used to remove open class in out effect
        if (typeof callbackFunc === 'function') {
            callbackFunc();
        }
    });
}

// Bootstrap API hooks
// =========================
dropdownSelectors.on({
    "show.bs.dropdown": function () {
        // On show, start in effect
        var dropdown = dropdownEffectData(this);
        dropdownEffectStart(dropdown, dropdown.effectIn);
    },
    "shown.bs.dropdown": function () {
        // On shown, remove in effect once complete
        var dropdown = dropdownEffectData(this);
        if (dropdown.effectIn && dropdown.effectOut) {
            dropdownEffectEnd(dropdown, function () { });
        }
    },
    "hide.bs.dropdown": function (e) {
        // On hide, start out effect
        var dropdown = dropdownEffectData(this);
        if (dropdown.effectOut) {
            e.preventDefault();
            dropdownEffectStart(dropdown, dropdown.effectOut);
            dropdownEffectEnd(dropdown, function () {
                dropdown.dropdown.removeClass('open');
            });
        }
    },
});


"use strict";

var console = {};

console.log = function () {};

window.console = console;
if (!$.hood) $.hood = {};
$.body = $('body'); // Console hack

var console = window.console || {};

console.log = console.log || function () {};

console.warn = console.warn || function () {};

console.error = console.error || function () {};

console.info = console.info || function () {};

$.fn.exists = function () {
  return $(this).length;
};

$.fn.doesExist = function () {
  return $(this).length;
};

$.fn.restrictToSlug = function (restrictPattern) {
  var targets = $(this); // The characters inside this pattern are accepted
  // and everything else will be 'cleaned'
  // For example 'ABCdEfGhI5' become 'ABCEGI5'

  var pattern = restrictPattern || /[^0-9a-zA-Z]*/g; // default pattern

  var restrictHandler = function restrictHandler() {
    var val = $(this).val();
    var newVal = val.replace(pattern, ''); // This condition is to prevent selection and keyboard navigation issues

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
  var targets = $(this); // The characters inside this pattern are accepted
  // and everything else will be 'cleaned'

  var pattern = restrictPattern || /[^0-9a-zA-Z-//]*/g; // default pattern

  var restrictHandler = function restrictHandler() {
    var val = $(this).val();
    var newVal = val.replace(pattern, '');

    if ((newVal.match(new RegExp("/", "g")) || []).length > 4) {
      var pos = newVal.lastIndexOf('/');
      newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
      $.hood.Alerts.Warning("You can only have up to 4 '/' characters in a url slug.");
    } // This condition is to prevent selection and keyboard navigation issues


    if (val !== newVal) {
      $(this).val(newVal);
    }
  };

  targets.on('keyup', restrictHandler);
  targets.on('paste', restrictHandler);
  targets.on('change', restrictHandler);
};

$('.restrict-to-page-slug').restrictToPageSlug();

$.fn.restrictToMetaSlug = function (restrictPattern) {
  var targets = $(this); // The characters inside this pattern are accepted
  // and everything else will be 'cleaned'

  var pattern = restrictPattern || /[^0-9a-zA-Z.]*/g; // default pattern

  var restrictHandler = function restrictHandler() {
    var val = $(this).val();
    var newVal = val.replace(pattern, '');

    if ((newVal.match(new RegExp(".", "g")) || []).length > 1) {
      var pos = newVal.lastIndexOf('.');
      newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
      $.hood.Alerts.Warning("You can only have up to 1 '.' characters in a meta slug.");
    } // This condition is to prevent selection and keyboard navigation issues


    if (val !== newVal) {
      $(this).val(newVal);
    }
  };

  targets.on('keyup', restrictHandler);
  targets.on('paste', restrictHandler);
  targets.on('change', restrictHandler);
};

$('.restrict-to-meta-slug').restrictToMetaSlug();

$.fn.characterCounter = function (val) {
  var targets = $(this);

  var characterCounterHandler = function characterCounterHandler() {
    counter = $(this).data('counter');
    max = Number($(this).attr('maxlength'));
    len = $(this).val().length;
    $(counter).text(max - len);
    cls = "text-success";
    if (max - len < max / 10) cls = "text-danger";
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

$.fn.warningAlert = function () {
  var targets = $(this);

  var warningAlertHandler = function warningAlertHandler(e) {
    e.preventDefault();

    var warningAlertCallback = function warningAlertCallback(confirmed) {
      if (confirmed) {
        var url = $(e.currentTarget).attr('href');
        window.location = url;
      }
    };

    $.hood.Alerts.Confirm($(e.currentTarget).data('warning'), $(e.currentTarget).data('title'), warningAlertCallback, 'warning', $(e.currentTarget).data('footer'), 'Ok', 'Cancel');
    return false;
  };

  targets.on('click', warningAlertHandler);
};

$('.warning-alert').warningAlert();

$.commonHeight = function (element, columnTag) {
  var maxHeight = 0;
  element.children(columnTag).each(function () {
    var elementChild = $(this).children();

    if (elementChild.hasClass('max-height')) {
      maxHeight = elementChild.outerHeight();
    } else {
      if (elementChild.outerHeight() > maxHeight) maxHeight = elementChild.outerHeight();
    }
  });
  element.children(columnTag).each(function () {
    $(this).height(maxHeight);
  });
};

$.loadCss = function (id, location) {
  if (!$('link#' + id).length) $('<link/>', {
    id: id,
    rel: 'stylesheet',
    type: 'text/css',
    href: location
  }).appendTo('head');
};

$.getUrlVars = function () {
  var vars = [],
      hash;
  var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');

  for (var i = 0; i < hashes.length; i++) {
    hash = hashes[i].split('=');
    vars.push(hash[0]);
    vars[hash[0]] = hash[1];
  }

  return vars;
};

$.decodeUrl = function (str) {
  return decodeURIComponent(str).replace('+', ' ');
};

$.numberWithCommas = function (x) {
  return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
};

if ($.validator) {
  $.validator.addMethod("time", function (value, element) {
    return this.optional(element) || /^(([0-1]?[0-9])|([2][0-3])):([0-5]?[0-9])(:([0-5]?[0-9]))?$/i.test(value);
  }, "Please enter a valid time.");
  $.validator.addMethod("ukdate", function (value, element) {
    // put your own logic here, this is just a (crappy) example
    return value.match(/^\d\d?\/\d\d?\/\d\d\d\d$/);
  }, "Please enter a date in the format dd/mm/yyyy.");
}

$.mobile = {
  Android: function Android() {
    return navigator.userAgent.match(/Android/i);
  },
  BlackBerry: function BlackBerry() {
    return navigator.userAgent.match(/BlackBerry/i);
  },
  iOS: function iOS() {
    return navigator.userAgent.match(/iPhone|iPad|iPod/i);
  },
  Opera: function Opera() {
    return navigator.userAgent.match(/Opera Mini/i);
  },
  Windows: function Windows() {
    return navigator.userAgent.match(/IEMobile/i);
  },
  Any: function Any() {
    return $.mobile.Android() || $.mobile.BlackBerry() || $.mobile.iOS() || $.mobile.Opera() || $.mobile.Windows();
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
  // Force prevent autocomplete
  // Thanks to SaidbakR - https://stackoverflow.com/a/50438500/1663500
  var trackInputs = {
    password: "0",
    username: "0"
  }; //Password and username fields ids as object's property, and "0" as its their values

  $('body').on('change', '.prevent-autocomplete', function (e) {
    // Change event is fired as autocomplete occurred at the input field 
    var trackId = $(this).attr('id'); //get the input field id to access the trackInputs object            

    if (trackInputs[trackId] === '0' || trackInputs[trackId] !== $(this).val()) {
      //trackInputs property value not changed or the prperty value ever it it is not equals the input field value
      $(this).val(''); // empty the field
    }
  });
  $('body').on('keyup', '.prevent-autocomplete', function (e) {
    var trackId = $(this).attr('id');
    trackInputs[trackId] = $(this).val(); //Update trackInputs property with the value of the field with each keyup.
  });
})(); // Custom Event polyfill


(function () {
  if (typeof window.CustomEvent === "function") return false;

  function CustomEvent(event, params) {
    params = params || {
      bubbles: false,
      cancelable: false,
      detail: undefined
    };
    var evt = document.createEvent('CustomEvent');
    evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail);
    return evt;
  }

  CustomEvent.prototype = window.Event.prototype;
  window.CustomEvent = CustomEvent;
})();

$.hood.LinkClasses = [{
  title: 'None',
  value: ''
}, {
  title: 'Button link',
  value: 'btn btn-default'
}, {
  title: 'Theme coloured button link',
  value: 'btn btn-primary'
}, {
  title: 'Popup image/video',
  value: 'colorbox-iframe'
}, {
  title: 'Button popup link',
  value: 'btn btn-default colorbox-iframe'
}, {
  title: 'Theme coloured button popup link',
  value: 'btn btn-primary colorbox-iframe'
}, {
  title: 'Large link',
  value: 'font-lg'
}, {
  title: 'Large button link',
  value: 'btn btn-default btn-lg'
}, {
  title: 'Large theme coloured button link',
  value: 'btn btn-primary btn-lg'
}, {
  title: 'Large popup image/video',
  value: 'font-lg colorbox-iframe'
}, {
  title: 'Large Button popup link',
  value: 'btn btn-default btn-lg colorbox-iframe'
}, {
  title: 'Theme coloured button popup link',
  value: 'btn btn-primary btn-lg colorbox-iframe'
}];
$.hood.ImageClasses = [{
  title: 'None',
  value: ''
}, {
  title: 'Full Width',
  value: 'user-image full'
}, {
  title: 'Left Aligned',
  value: 'user-image left'
}, {
  title: 'Centered',
  value: 'user-image center'
}, {
  title: 'Right Aligned',
  value: 'user-image right'
}, {
  title: 'Inline with text, top aligned',
  value: 'user-image inline top'
}, {
  title: 'Inline with text, middle aligned',
  value: 'user-image inline'
}, {
  title: 'Inline with text, bottom aligned',
  value: 'user-image inline bottom'
}, {
  title: 'Pulled Left',
  value: 'user-image pull-left'
}, {
  title: 'Pulled Right',
  value: 'user-image pull-right'
}];
new CustomEvent('loader-show');
new CustomEvent('loader-hide');

$.hood.Loader = function (show) {
  if (show) $('body').trigger('loader-show');else $('body').trigger('loader-hide');
};

if (!$.hood) $.hood = {};
$.hood.Helpers = {
  IsNullOrUndefined: function IsNullOrUndefined(a) {
    var rc = false;

    if (a === null || typeof a === "undefined" || a === "") {
      rc = true;
    }

    return rc;
  },
  IsSet: function IsSet(a) {
    var rc = false;

    if (a === null || typeof a === "undefined" || a === "") {
      rc = true;
    }

    return !rc;
  },
  IsEventSupported: function IsEventSupported(eventName) {
    var el = document.createElement('div');
    eventName = 'on' + eventName;
    var isSupported = eventName in el;

    if (!isSupported) {
      el.setAttribute(eventName, 'return;');
      isSupported = typeof el[eventName] === 'function';
    }

    el = null;
    return isSupported;
  },
  IsFunction: function IsFunction(functionToCheck) {
    return functionToCheck && {}.toString.call(functionToCheck) === '[object Function]';
  },
  IsUrlExternal: function IsUrlExternal(url) {
    var match = url.match(/^([^:\/?#]+:)?(?:\/\/([^\/?#]*))?([^?#]+)?(\?[^#]*)?(#.*)?/);
    if (typeof match[1] === "string" && match[1].length > 0 && match[1].toLowerCase() !== location.protocol) return true;
    if (typeof match[2] === "string" && match[2].length > 0 && match[2].replace(new RegExp(":(" + {
      "http:": 80,
      "https:": 443
    }[location.protocol] + ")?$"), "") !== location.host) return true;
    return false;
  },
  IsInIframe: function IsInIframe() {
    try {
      return window.self !== window.top;
    } catch (e) {
      return true;
    }
  },
  HtmlEncode: function HtmlEncode(value) {
    //create a in-memory div, set it's inner text(which jQuery automatically encodes)
    //then grab the encoded contents back out.  The div never exists on the page.
    return $('<div/>').text(value).html();
  },
  HtmlDecode: function HtmlDecode(value) {
    return $('<div/>').html(value).text();
  },
  FormatCurrency: function FormatCurrency(n, currency) {
    return currency + " " + n.toFixed(2).replace(/./g, function (c, i, a) {
      return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
  },
  FormatKilobytes: function FormatKilobytes(n) {
    n = n / 1024;
    return n.toFixed(2).replace(/./g, function (c, i, a) {
      return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
  },
  FormatMegabytes: function FormatMegabytes(n) {
    n = n / 1024;
    n = n / 1024;
    return currency + " " + n.toFixed(2).replace(/./g, function (c, i, a) {
      return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
  },
  ProcessResponse: function ProcessResponse(data) {
    var title = '';
    if (data.Title) title = "<strong>".concat(data.Title, "</strong><br />");

    if (data.Success) {
      $.hood.Alerts.Success("".concat(title).concat(data.Message));
    } else {
      $.hood.Alerts.Error("".concat(title).concat(data.Errors));
    }
  },
  GetQueryStringParamByName: function GetQueryStringParamByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)", 'i'),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
  },
  InsertQueryStringParam: function InsertQueryStringParam(key, value) {
    key = escape(key);
    value = escape(value);
    var kvp = document.location.search.substr(1).split('&');

    if (kvp === '') {
      document.location.search = '?' + key + '=' + value;
    } else {
      var i = kvp.length;
      var x;

      while (i--) {
        x = kvp[i].split('=');

        if (x[0] === key) {
          x[1] = value;
          kvp[i] = x.join('=');
          break;
        }
      }

      if (i < 0) {
        kvp[kvp.length] = [key, value].join('=');
      } //this will reload the page, it's likely better to store this until finished


      document.location.search = kvp.join('&');
    }
  },
  InsertQueryStringParamToUrl: function InsertQueryStringParamToUrl(url, key, value) {
    key = escape(key);
    value = escape(value);
    var kvp = url.search.substr(1).split('&');

    if (kvp === '') {
      url.search = '?' + key + '=' + value;
    } else {
      var i = kvp.length;
      var x;

      while (i--) {
        x = kvp[i].split('=');

        if (x[0] === key) {
          x[1] = value;
          kvp[i] = x.join('=');
          break;
        }
      }

      if (i < 0) {
        kvp[kvp.length] = [key, value].join('=');
      } //this will reload the page, it's likely better to store this until finished


      url.search = kvp.join('&');
    }

    return url;
  },
  UrlToLocationObject: function UrlToLocationObject(href) {
    var l = document.createElement("a");
    l.href = href;
    return l;
  },
  FindAndRemoveFromArray: function FindAndRemoveFromArray(array, property, value) {
    $.each(array, function (index, result) {
      if (result[property] === value) {
        //Remove from array
        array.splice(index, 1);
      }
    });
  },
  FindAndReturnFromArray: function FindAndReturnFromArray(array, property, value) {
    var outputItem = null;
    $.each(array, function (index, result) {
      if (result[property] === value) {
        //return it
        outputItem = result;
      }
    });
    return outputItem;
  },
  LeftPad: function LeftPad(number, targetLength) {
    var output = number + '';

    while (output.length < targetLength) {
      output = '0' + output;
    }

    return output;
  },
  DateAdd: function DateAdd(date, interval, units) {
    var ret = new Date(date); //don't change original date

    switch (interval.toLowerCase()) {
      case 'year':
        ret.setFullYear(ret.getFullYear() + units);
        break;

      case 'quarter':
        ret.setMonth(ret.getMonth() + 3 * units);
        break;

      case 'month':
        ret.setMonth(ret.getMonth() + units);
        break;

      case 'week':
        ret.setDate(ret.getDate() + 7 * units);
        break;

      case 'day':
        ret.setDate(ret.getDate() + units);
        break;

      case 'hour':
        ret.setTime(ret.getTime() + units * 3600000);
        break;

      case 'minute':
        ret.setTime(ret.getTime() + units * 60000);
        break;

      case 'second':
        ret.setTime(ret.getTime() + units * 1000);
        break;

      default:
        ret = undefined;
        break;
    }

    return ret;
  },
  GenerateRandomString: function GenerateRandomString(numSpecials) {
    var specials = '!@#$&*';
    var lowercase = 'abcdefghijklmnopqrstuvwxyz';
    var uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    var numbers = '0123456789';
    var all = lowercase + uppercase + numbers;
    var password = (specials.pick(1) + lowercase.pick(1) + uppercase.pick(1) + all.pick(5, 10)).shuffle();
    return password;
  }
};
if (!$.hood) $.hood = {};
$.hood.Handlers = {
  Init: function Init() {
    // Click to select boxes
    $('body').on('click', '.select-text', $.hood.Handlers.SelectTextContent);
    $('body').on('click', '.btn.click-select[data-target][data-value]', $.hood.Handlers.ClickSelect);
    $('body').on('click', '.click-select.show-selected[data-target][data-value]', $.hood.Handlers.ClickSelect);
    $('body').on('click', '.click-select:not(.show-selected)[data-target][data-value]', $.hood.Handlers.ClickSelectClean);
    $('body').on('click', '.slide-link', $.hood.Handlers.SlideToAnchor);
    $('body').on('click', '.scroll-target, .scroll-to-target', $.hood.Handlers.ScrollToTarget);
    $('body').on('click', '.scroll-top, .scroll-to-top', $.hood.Handlers.ScrollToTop);
    $('body').on('change', 'input[type=checkbox][data-input]', $.hood.Handlers.CheckboxChange);
    $('body').on('change', '.submit-on-change', $.hood.Handlers.SubmitOnChange);
    $('select[data-selected]').each($.hood.Handlers.SelectSetup);
    $('body').on('change', '.inline-date', $.hood.Handlers.DateChange);
    $.hood.Handlers.Uploaders.Init();
  },
  ScrollToTop: function ScrollToTop(e) {
    if (e) e.preventDefault();
    $('html, body').animate({
      scrollTop: 0
    }, 800);
    return false;
  },
  ScrollToTarget: function ScrollToTarget(e) {
    if (e) e.preventDefault();
    var url = $(this).attr('href').split('#')[0];

    if (url !== window.location.pathname && url !== "") {
      return;
    }

    var target = this.hash;
    var $target = $(target);
    $('html, body').stop().animate({
      'scrollTop': $target.offset().top - $.hood.App.Options.Scroll.Offset
    }, 900, 'swing');
  },
  SubmitOnChange: function SubmitOnChange(e) {
    if (e) e.preventDefault();
    $(this).parents('form').submit();
  },
  DateChange: function DateChange(e) {
    if (e) e.preventDefault(); // update the date element attached to the field's attach

    var $field = $(this).parents('.hood-date').find('.date-output');
    var date = $field.parents('.hood-date').find('.date-value').val();
    var pattern = /^([0-9]{2})\/([0-9]{2})\/([0-9]{4})$/;
    if (!pattern.test(date)) date = "01/01/2001";
    var hour = $field.parents('.hood-date').find('.hour-value').val();
    if (!$.isNumeric(hour)) hour = "00";
    var minute = $field.parents('.hood-date').find('.minute-value').val();
    if (!$.isNumeric(minute)) minute = "00";
    $field.val(date + " " + hour + ":" + minute + ":00");
    $field.attr("value", date + " " + hour + ":" + minute + ":00");
  },
  CheckboxChange: function CheckboxChange(e) {
    if (e) e.preventDefault(); // when i change - create an array, with any other checked of the same data-input checkboxes. and add to the data-input referenced tag.

    var items = new Array();
    $('input[data-input="' + $(this).data('input') + '"]').each(function () {
      if ($(this).is(":checked")) items.push($(this).val());
    });
    var id = '#' + $(this).data('input');
    var vals = JSON.stringify(items);
    $(id).val(vals);
  },
  SelectSetup: function SelectSetup() {
    var sel = $(this).data('selected');

    if ($(this).data('selected') !== 'undefined' && $(this).data('selected') !== '') {
      var selected = String($(this).data('selected'));
      $(this).val(selected);
    }
  },
  ClickSelect: function ClickSelect() {
    var $this = $(this);
    var targetId = '#' + $this.data('target');
    $(targetId).val($this.data('value'));
    $(targetId).trigger('change');
    $('.click-select[data-target="' + $this.data('target') + '"]').each(function () {
      $(this).html($(this).data('temp')).removeClass('active');
    });
    $('.click-select[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () {
      $(this).data('temp', $(this).html()).html('Selected').addClass('active');
    });
  },
  ClickSelectClean: function ClickSelectClean() {
    var $this = $(this);
    var targetId = '#' + $this.data('target');
    $(targetId).val($this.data('value'));
    $(targetId).trigger('change');
    $('.click-select.clean[data-target="' + $this.data('target') + '"]').each(function () {
      $(this).removeClass('active');
    });
    $('.click-select.clean[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () {
      $(this).addClass('active');
    });
  },
  SelectTextContent: function SelectTextContent() {
    var $this = $(this);
    $this.select(); // Work around Chrome's little problem

    $this.mouseup(function () {
      // Prevent further mouseup intervention
      $this.unbind("mouseup");
      return false;
    });
  },
  SlideToAnchor: function SlideToAnchor() {
    var scrollTop = $('body').scrollTop();
    var top = $($.attr(this, 'href')).offset().top;
    $('html, body').animate({
      scrollTop: top
    }, Math.abs(top - scrollTop));
    return false;
  },
  Uploaders: {
    Init: function Init() {
      $(".upload-progress-bar").hide();
      $.getScript('/lib/dropzone/min/dropzone.min.js', $.proxy(function () {
        $('.image-uploader').each(function () {
          $.hood.Handlers.Uploaders.SingleImage($(this).attr('id'), $(this).data('json'));
        });
        $('.gallery-uploader').each(function () {
          $.hood.Handlers.Uploaders.Gallery($(this).attr('id'), $(this).data('json'));
        });
      }, this));
    },
    RefreshImage: function RefreshImage(data) {
      $('.' + data.Class).css({
        'background-image': 'url(' + data.Image + ')'
      });
      $('.' + data.Class).find('img').attr('src', data.Image);
    },
    SingleImage: function SingleImage(tag, jsontag) {
      tag = '#' + tag;
      var $tag = $(tag);
      Dropzone.autoDiscover = false;
      var avatarDropzone = new Dropzone(tag, {
        url: $(tag).data('url'),
        maxFiles: 1,
        paramName: 'file',
        parallelUploads: 1,
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: false,
        // Define the container to display the previews
        clickable: tag // Define the element that should be used as click trigger to select files.

      });
      avatarDropzone.on("addedfile", function () {}); // Update the total progress bar

      avatarDropzone.on("totaluploadprogress", function (progress) {
        $(".upload-progress-bar." + tag.replace('#', '') + " .progress-bar").css({
          width: progress + "%"
        });
      });
      avatarDropzone.on("sending", function (file) {
        $(".upload-progress-bar." + tag.replace('#', '')).show();
        $($(tag).data('preview')).addClass('loading');
      });
      avatarDropzone.on("queuecomplete", function (progress) {
        $(".upload-progress-bar." + tag.replace('#', '')).hide();
      });
      avatarDropzone.on("success", function (file, response) {
        if (response.Success) {
          if (response.Media) {
            $(jsontag).val(JSON.stringify(response.Media));
            $($(tag).data('preview')).css({
              'background-image': 'url(' + response.Media.SmallUrl + ')'
            });
            $($(tag).data('preview')).find('img').attr('src', response.Media.SmallUrl);
          }

          $.hood.Alerts.Success("New image added!");
        } else {
          $.hood.Alerts.Error("There was a problem adding the image: " + response.Error);
        }

        avatarDropzone.removeFile(file);
        $($(tag).data('preview')).removeClass('loading');
      });
    },
    Gallery: function Gallery(tag) {
      Dropzone.autoDiscover = false;
      var previewNode = document.querySelector(tag + "-template");
      previewNode.id = "";
      var previewTemplate = previewNode.parentNode.innerHTML;
      previewNode.parentNode.removeChild(previewNode);
      var galleryDropzone = new Dropzone(tag, {
        url: $(tag).data('url'),
        thumbnailWidth: 80,
        thumbnailHeight: 80,
        parallelUploads: 5,
        previewTemplate: previewTemplate,
        paramName: 'files',
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: "#previews",
        // Define the container to display the previews
        clickable: ".fileinput-button",
        // Define the element that should be used as click trigger to select files.
        dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
        dictResponseError: 'Error while uploading file!'
      });
      $(tag + " .cancel").hide();
      galleryDropzone.on("addedfile", function (file) {
        $(file.previewElement.querySelector(".complete")).hide();
        $(file.previewElement.querySelector(".cancel")).show();
        $(tag + " .cancel").show();
      }); // Update the total progress bar

      galleryDropzone.on("totaluploadprogress", function (progress) {
        document.querySelector("#total-progress .progress-bar").style.width = progress + "%";
      });
      galleryDropzone.on("sending", function (file) {
        // Show the total progress bar when upload starts
        document.querySelector("#total-progress").style.opacity = "1"; // And disable the start button
      }); // Hide the total progress bar when nothing's uploading anymore

      galleryDropzone.on("complete", function (file) {
        $(file.previewElement.querySelector(".cancel")).hide();
        $(file.previewElement.querySelector(".progress")).hide();
        $(file.previewElement.querySelector(".complete")).show();
        $.hood.Inline.Refresh('.gallery');
      }); // Hide the total progress bar when nothing's uploading anymore

      galleryDropzone.on("queuecomplete", function (progress) {
        document.querySelector("#total-progress").style.opacity = "0";
        $(tag + " .cancel").hide();
      });
      galleryDropzone.on("success", function (file, response) {
        $.hood.Inline.Refresh('.gallery');

        if (response.Success) {
          $.hood.Alerts.Success("New images added!");
        } else {
          $.hood.Alerts.Error("There was a problem adding the profile image: " + response.Error);
        }
      }); // Setup the buttons for all transfers
      // The "add files" button doesn't need to be setup because the config
      // `clickable` has already been specified.

      document.querySelector(".actions .cancel").onclick = function () {
        galleryDropzone.removeAllFiles(true);
      };
    }
  }
};
$(document).ready($.hood.Handlers.Init);

String.prototype.contains = function (it) {
  return this.indexOf(it) !== -1;
};

String.prototype.pick = function (min, max) {
  var n,
      chars = '';

  if (typeof max === 'undefined') {
    n = min;
  } else {
    n = min + Math.floor(Math.random() * (max - min));
  }

  for (var i = 0; i < n; i++) {
    chars += this.charAt(Math.floor(Math.random() * this.length));
  }

  return chars;
}; // Credit to @Christoph: http://stackoverflow.com/a/962890/464744


String.prototype.shuffle = function () {
  var array = this.split('');
  var tmp,
      current,
      top = array.length;
  if (top) while (--top) {
    current = Math.floor(Math.random() * (top + 1));
    tmp = array[current];
    array[current] = array[top];
    array[top] = tmp;
  }
  return array.join('');
};

String.prototype.toSeoUrl = function () {
  var output = this.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, "-").toLowerCase();
  /* remove first dash */

  if (output.charAt(0) === '-') output = output.substring(1);
  /* remove last dash */

  var last = output.length - 1;
  if (output.charAt(last) === '-') output = output.substring(0, last);
  return output;
};

if (!$.hood) $.hood = {};

$.hood.FormValidator = function (element, options) {
  this.Options = $.extend({
    formTag: element,
    validationRules: null,
    validationMessages: {},
    placeBelow: true,
    submitButtonTag: null,
    submitUrl: null,
    submitFunction: null,
    serializationFunction: function serializationFunction() {
      rtn = $(this.formTag).serialize();
      return rtn;
    }
  }, options || {});

  this.LoadValidation = function () {
    if ($.hood.Helpers.IsNullOrUndefined(this.Options.formTag)) return;
    $(this.Options.formTag).find('input, textarea, select').keypress($.proxy(function (e) {
      if (e.which === 13) {
        $.proxy(this.submitForm(), this);
        e.preventDefault();
        return false;
      }
    }, this));
    $(this.Options.formTag).validate({
      submitHandler: function submitHandler(e) {
        e.preventDefault();
      },
      errorClass: this.Options.errorClass,
      focusInvalid: false,
      rules: this.Options.validationRules,
      messages: this.Options.validationMessages
    });
    if ($.hood.Helpers.IsNullOrUndefined(this.Options.submitButtonTag)) return;
    $(this.Options.submitButtonTag).click($.proxy(this.submitForm, this));
  };

  this.submitForm = function () {
    if ($(this.Options.formTag).valid()) {
      this.TempButtonContent = $(this.Options.submitButtonTag).removeClass('btn-primary').addClass('btn-default').html();
      $(this.Options.submitButtonTag).removeClass('btn-primary').addClass('btn-default').html('<i class="fa fa-refresh fa-spin"></i>&nbsp;Loading...');
      $(this.Options.formTag).find('input[type=checkbox]').each(function () {
        if ($(this).is(':checked')) {
          $(this).val('true');
        }
      });
      $.post(this.Options.submitUrl, this.Options.serializationFunction(), $.proxy(function (data) {
        $(this.Options.submitButtonTag).removeClass('btn-default').addClass('btn-primary').html(this.TempButtonContent);
        this.Options.submitFunction(data);
      }, this));
    }
  };

  this.LoadValidation();
  if (this.Options.placeBelow) $(this.Options.formTag).addClass("validation-below");
};

$.fn.hoodValidator = function (options) {
  return this.each(function () {
    var element = $(this); // Return early if this element already has a plugin instance

    if (element.data('hoodValidator')) return; // pass options to plugin constructor

    var hoodValidator = new $.hood.FormValidator(this, options); // Store plugin object in this element's data

    element.data('hoodValidator', hoodValidator);
  });
};

if (!$.hood) $.hood = {};
$.hood.Addresses = {
  Init: function Init() {
    $('body').on('click', '.add-new-address', $.hood.Addresses.New);
    $('body').on('click', '.set-billing', $.hood.Addresses.SetBilling);
    $('body').on('click', '.set-delivery', $.hood.Addresses.SetDelivery);
    $('body').on('click', '.delete-address', $.hood.Addresses.Delete);
    $('body').on('click', '.edit-address', $.hood.Addresses.Edit);

    if ($(".address-select").length > 0) {
      $.hood.Addresses.Refresh();
    }
  },
  New: function New(e) {
    $(this).data('temp', $(this).html());
    $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
    $.hood.Modals.Open('/account/addresses/create', null, '', $.hood.Addresses.PostLoad);
  },
  Edit: function Edit() {
    $(this).data('temp', $(this).html());
    $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
    $.hood.Modals.Open('/account/addresses/edit', {
      id: $(this).data('id')
    }, '', $.hood.Addresses.PostLoad);
  },
  Refresh: function Refresh() {
    $.hood.Inline.Reload('.address-list'); // reload any selectlists that contain billing or delivery addresses (checkouts etc.)

    $.get('/account/addresses/get', null, function (data) {
      $('.address-select').empty().append($('<option>', {
        value: '',
        text: '--- Choose an address ---'
      }));

      for (var i in data) {
        var id = data[i].Id;
        var address = data[i].FullAddress;
        $('.address-select').append($('<option>', {
          value: id,
          text: address
        }));
      }
    });
  },
  PostLoad: function PostLoad() {
    $.hood.Google.Addresses.InitAutocomplete();
    $('.btn-loading').each(function () {
      $(this).removeClass('btn-loading').html($(this).data('temp'));
    });
    $('#address-form').hoodValidator({
      validationRules: {
        Number: {
          required: true
        },
        Address1: {
          required: true
        },
        City: {
          required: true
        },
        County: {
          required: true
        },
        Postcode: {
          required: true
        },
        Country: {
          required: true
        }
      },
      submitButtonTag: $('#save-address'),
      submitUrl: $('#address-form').attr('action'),
      submitFunction: function submitFunction(data) {
        if (data.Success) {
          $.hood.Addresses.Refresh();
          $.hood.Modals.Close('#add-address-modal');
        } else {
          $.hood.Alerts.Error(data.Errors, "Error Saving Address!");
        }
      }
    });
  },
  SetBilling: function SetBilling(e) {
    $(this).data('temp', $(this).html());
    $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
    $.post('/account/addresses/setbilling?id=' + $(this).data('id'), null, function (data) {
      $('.btn-loading').each(function () {
        $(this).removeClass('btn-loading').html($(this).data('temp'));
      });

      if (data.success) {
        $.hood.Alerts.Success("Your billing address has been updated.", "Billing Address Updated!");
        $.hood.Addresses.Refresh();
      } else {
        $.hood.Alerts.Error("Couldn't update your billing address...", "Couldn't Update Billing Address!");
      }
    });
    e.preventDefault();
  },
  SetDelivery: function SetDelivery(e) {
    $(this).data('temp', $(this).html());
    $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
    $.post('/account/addresses/setdelivery?id=' + $(this).data('id'), function (data) {
      $('.btn-loading').each(function () {
        $(this).removeClass('btn-loading').html($(this).data('temp'));
      });

      if (data.success) {
        $.hood.Alerts.Success("Your delivery address has been updated.", "Delivery Address Updated!");
        $.hood.Addresses.Refresh();
      } else {
        $.hood.Alerts.Error("Couldn't update your delivery address...", "Couldn't Update Delivery Address!");
      }
    });
    e.preventDefault();
  },
  Delete: function Delete(e) {
    $(this).data('temp', $(this).html());
    $(this).addClass('btn-loading').append('<i class="fa fa-refresh fa-spin m-l-xs"></i>');
    $.post('/account/addresses/delete', {
      id: $(this).data('id')
    }, function (data) {
      $('.btn-loading').each(function () {
        $(this).removeClass('btn-loading').html($(this).data('temp'));
      });

      if (data.success) {
        $.hood.Alerts.Success("Your address has been deleted.", "Address Deleted!");
        $.hood.Addresses.Refresh();
      } else {
        $.hood.Alerts.Error("Couldn't delete your address, it may be in use as your billing or delivery address...", "Couldn't Delete Address!");
      }
    });
    e.preventDefault();
  }
};
$(document).ready($.hood.Addresses.Init);
var swalWithBootstrapButtons = Swal.mixin({
  customClass: {
    confirmButton: 'btn btn-success btn-lg m-1 pl-4 pr-4',
    cancelButton: 'btn btn-danger btn-lg m-1'
  },
  buttonsStyling: false
});
var Toast = Swal.mixin({
  toast: true,
  position: 'top-end',
  showConfirmButton: true
});
if (!$.hood) $.hood = {};
$.hood.Alerts = {
  Error: function Error(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'error', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'error');
  },
  Warning: function Warning(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'warning', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'warning');
  },
  Message: function Message(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'info', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'info');
  },
  Success: function Success(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'success', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'success');
  },
  Alert: function Alert(msg, title, type) {
    Toast.fire({
      type: type || 'info',
      html: msg,
      title: title
    });
  },
  SweetAlert: function SweetAlert(msg, title, type, footer, showConfirmButton, timer, callback) {
    swalWithBootstrapButtons.fire({
      title: title,
      html: msg,
      type: type || 'info',
      footer: footer,
      showConfirmButton: showConfirmButton,
      timer: timer
    }).then(function (result) {
      if (!result.dismiss) callback(result);
    });
  },
  Confirm: function Confirm(msg, title, callback, type, footer, confirmButtonText, cancelButtonText) {
    swalWithBootstrapButtons.fire({
      title: title || 'Woah!',
      html: msg || 'Are you sure you want to do this?',
      type: type || 'warning',
      footer: footer || '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>',
      showCancelButton: true,
      confirmButtonText: confirmButtonText || 'Ok',
      cancelButtonText: cancelButtonText || 'Cancel'
    }).then(function (result) {
      if (!result.dismiss) callback(result.value);
    });
  },
  Prompt: function Prompt(msg, title, callback, inputType, type, footer, confirmButtonText, cancelButtonText, inputAttributes) {
    swalWithBootstrapButtons.fire({
      input: inputType || 'text',
      inputAttributes: inputAttributes || {
        autocapitalize: 'off'
      },
      title: title || 'Enter a value',
      html: msg || 'Fill in the field and press Ok to continue.',
      type: type || 'info',
      footer: footer,
      showCancelButton: true,
      confirmButtonText: confirmButtonText || 'Ok',
      cancelButtonText: cancelButtonText || 'Cancel'
    }).then(function (result) {
      if (!result.dismiss) callback(result.value);
    });
  }
};
if (!$.hood) $.hood = {};
$.hood.Forms = {
  Init: function Init() {
    $('.floating-label > label').each(function () {
      var $me = $(this);
      $me.parent().append($me);
    });
  },
  GetAllowedExtensions: function GetAllowedExtensions(section) {
    switch (section) {
      case "Image":
        return ['png', 'jpg', 'jpeg', 'bmp', 'gif'];

      case "Document":
        return ['doc', 'docx', 'pdf', 'rtf'];

      case "All":
        return '';
    }
  },
  GetAllowedFiles: function GetAllowedFiles(section) {
    switch (section) {
      case "Image":
        return 'image/png,jpg,image/jpeg,image/gif';

      case "Document":
        return 'application/msword,application/pdf,text/rtf';

      case "All":
        return '';
    }
  }
};
$(document).ready($.hood.Forms.Init);
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
if (!$.hood) $.hood = {};
$.hood.Media = {
  Init: function Init() {
    $('body').on('click', '.media-delete', $.hood.Media.Delete);
    $('body').on('click', '.media-directories-delete', $.hood.Media.Directories.Delete);
    $.hood.Media.Upload.Init();
    $.hood.Media.Actions.Init();
  },
  Loaded: function Loaded(data) {
    $.hood.Loader(false);
  },
  BladeLoaded: function BladeLoaded(data) {
    $.hood.Media.LoadMediaPlayers();
  },
  Reload: function Reload(complete) {
    $.hood.Inline.Reload($('#media-list'), complete);
  },
  ReloadDirectories: function ReloadDirectories(complete) {
    $.hood.Inline.Reload($('#media-directories-list'), complete);
  },
  Actions: {
    Init: function Init() {
      // ATTACH FUNCTION - ATTACHES THE IMAGE TO A SPECIFIC ENTITY ATTACHABLE FIELD
      $('body').on('click', '.hood-image-attach', $.hood.Media.Actions.Load.Attach);
      $('body').on('click', '.hood-image-clear', $.hood.Media.Actions.Complete.Clear); // INSERT FUNCTION - INSERTS AN IMAGE TAG INTO THE CURRENTLY SELECTED EDITOR

      $('body').on('click', '.hood-image-insert', $.hood.Media.Actions.Load.Insert); // SELECT FUNCTION - INSERTS THE SELECTED URL INTO TEXTBOX ATTACHED TO SELECTOR

      $('body').on('click', '.hood-media-select', $.hood.Media.Actions.Load.Select);
    },
    Target: null,
    Json: null,
    Current: {
      Attach: null
    },
    Load: {
      Attach: function Attach(e) {
        e.preventDefault();
        $.hood.Media.Actions.Target = $($(this).data('tag'));
        $.hood.Media.Actions.Json = $($(this).data('json'));
        $.hood.Inline.Modal($(this).data('url'), function () {
          $.hood.Media.Reload(function () {
            $('body').off('click', '.media-attach');
            $('body').on('click', '.media-attach', $.hood.Media.Actions.Complete.Attach);
          });
          $.hood.Media.Upload.Init();
        });
      },
      Insert: function Insert(editor) {
        editor.addButton('hoodimage', {
          text: 'Insert image...',
          icon: false,
          onclick: function onclick() {
            $.hood.Inline.Modal($(this).data('url'), function () {
              $.hood.Media.Reload(function () {
                $('body').off('click');
                $('body').on('click', '.media-insert', $.proxy($.hood.Media.Actions.Complete.Insert, editor));
              });
              $.hood.Media.Upload.Init();
            });
          }
        });
      },
      Select: function Select(e) {
        $.hood.Media.Actions.Target = $($(this).data('target'));
        $.hood.Inline.Modal($(this).data('url'), function () {
          $.hood.Media.Reload(function () {
            $('body').off('click', '.media-select');
            $('body').on('click', '.media-select', $.hood.Media.Actions.Complete.Select);
          });
          $.hood.Media.Upload.Init();
        });
      }
    },
    Complete: {
      Attach: function Attach(e) {
        e.preventDefault();
        var $image = $.hood.Media.Actions.Target;
        var $json = $.hood.Media.Actions.Json;
        $.post($(this).data('url'), function (data) {
          $.hood.Helpers.ProcessResponse(data);

          if (data.Success) {
            var _icon = data.Media.Icon;

            if (data.Media.GeneralFileType === "Image") {
              _icon = data.Media.MediumUrl;
            }

            if (!$.hood.Helpers.IsNullOrUndefined($image)) {
              $image.css({
                'background-image': 'url(' + _icon + ')'
              });
              $image.find('img').attr('src', _icon);
              $image.removeClass('loading');
            }

            if (!$.hood.Helpers.IsNullOrUndefined($json)) {
              $json.val(data.MediaJson);
            }
          }
        }).done(function () {
          $('#media-select-modal').modal('hide');
        }).fail($.hood.Inline.HandleError);
      },
      Insert: function Insert(e) {
        var url = $(e.target).data('url');
        var editor = this;
        editor.insertContent('<img alt="Your image..." src="' + url + '"/>');
        $.hood.Modals.Close('#attach-media-modal');
      },
      Select: function Select(e) {
        var url = $(this).data('url');
        var tag = $.hood.Media.Actions.Target;
        $(tag).each(function () {
          if ($(this).is("input")) {
            $(this).val(url);
          } else {
            $(this).attr('src', url);
            $(this).css({
              'background-image': 'url(' + url + ')'
            });
            $(this).find('img').attr('src', url);
          }
        });
        $.hood.Alerts.Success("Image URL has been inserted.<br /><strong>Remember to press save!</strong>");
        $('#media-select-modal').modal('hide');
      },
      Clear: function Clear(e) {
        e.preventDefault();
        var $image = $($(this).data('tag'));
        var $json = $($(this).data('json'));
        $.post($(this).data('url'), function (data) {
          $.hood.Helpers.ProcessResponse(data);

          if (data.Success) {
            icon = data.Media.Icon;

            if (data.Media.GeneralFileType === "Image") {
              icon = data.Media.MediumUrl;
            }

            if (!$.hood.Helpers.IsNullOrUndefined($image)) {
              $image.css({
                'background-image': 'url(' + icon + ')'
              });
              $image.find('img').attr('src', icon);
              $image.removeClass('loading');
            }

            if (!$.hood.Helpers.IsNullOrUndefined($json)) {
              $json.val(data.Json);
            }
          }
        }).fail($.hood.Inline.HandleError);
      }
    },
    RefreshImage: function RefreshImage(tag, url, id) {
      var $image = $(tag);
      $image.addClass('loading');
      $.get(url, {
        id: id
      }, $.proxy(function (data) {
        $image.css({
          'background-image': 'url(' + data.SmallUrl + ')'
        });
        $image.find('img').attr('src', data.SmallUrl);
        $image.removeClass('loading');
      }, this));
    }
  },
  Upload: {
    Init: function Init() {
      if (!$('#media-add').doesExist()) return;
      $('#media-total-progress').hide();
      Dropzone.autoDiscover = false;
      var myDropzone = new Dropzone("#media-upload", {
        url: $.hood.Media.Upload.UploadUrl,
        thumbnailWidth: 80,
        thumbnailHeight: 80,
        parallelUploads: 5,
        previewTemplate: false,
        paramName: 'files',
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: false,
        // Define the container to display the previews
        clickable: "#media-add",
        // Define the element that should be used as click trigger to select files.
        dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
        dictResponseError: 'Error while uploading file!'
      });
      myDropzone.on("success", function (file, data) {
        $.hood.Helpers.ProcessResponse(data);
      });
      myDropzone.on("addedfile", function (file) {
        $('#media-total-progress .progress-bar').css({
          width: 0 + "%"
        });
        $('#media-total-progress .progress-bar .percentage').html(0 + "%");
      }); // Update the total progress bar

      myDropzone.on("totaluploadprogress", function (progress) {
        $('#media-total-progress .progress-bar').css({
          width: progress + "%"
        });
        $('#media-total-progress .progress-bar .percentage').html(progress + "%");
      });
      myDropzone.on("sending", function (file) {
        // Show the total progress bar when upload starts
        $('#media-total-progress').fadeIn();
        $('#media-total-progress .progress-bar').css({
          width: "0%"
        });
        $('#media-total-progress .progress-bar .percentage').html("0%");
      }); // Hide the total progress bar when nothing's uploading anymore

      myDropzone.on("complete", function (file) {
        $.hood.Media.Reload();
      }); // Hide the total progress bar when nothing's uploading anymore

      myDropzone.on("queuecomplete", function (progress) {
        $('#media-total-progress').hide();
        $.hood.Media.Reload();
      });
    },
    UploadUrl: function UploadUrl() {
      return $("#media-upload").data('url') + "?directoryId=" + $("input[type='radio'][name='dir']:checked").val();
    }
  },
  Delete: function Delete(e) {
    var $this = $(this);

    var deleteMediaCallback = function deleteMediaCallback(confirmed) {
      if (confirmed) {
        // delete functionality
        $.post('/admin/media/delete', {
          id: $this.data('id')
        }, function (data) {
          $.hood.Helpers.ProcessResponse(data);

          if (data.Success) {
            $.hood.Media.Reload();
            $('.modal-backdrop').remove();
            $('.modal').modal('hide');
          }
        });
      }
    };

    $.hood.Alerts.Confirm("The media file will be permanently removed. This cannot be undone.", "Are you sure?", deleteMediaCallback, 'warning', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> Ensure this file is not attached to any posts, pages or features of the site, or it will appear as a broken image or file.</span>', 'Ok', 'Cancel');
  },
  RestrictDir: function RestrictDir() {
    var pattern = /[^0-9A-Za-z- ]*/g; // default pattern

    var val = $(this).val();
    var newVal = val.replace(pattern, ''); // This condition is to prevent selection and keyboard navigation issues

    if (val !== newVal) {
      $(this).val(newVal);
    }
  },
  Directories: {
    Editor: function Editor() {
      $('#content-directories-edit-form').hoodValidator({
        validationRules: {
          DisplayName: {
            required: true
          },
          Slug: {
            required: true
          }
        },
        submitButtonTag: $('#content-directories-edit-submit'),
        submitUrl: $('#content-directories-edit-form').attr('action'),
        submitFunction: function submitFunction(data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Media.ReloadDirectories();
          $.hood.Media.Reload();
        }
      });
    },
    Delete: function Delete(e) {
      e.preventDefault();
      var $this = $(this);

      var deleteDirectoryCallback = function deleteDirectoryCallback(confirmed) {
        if (confirmed) {
          $.post($this.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Media.ReloadDirectories();
            $.hood.Media.Reload();
          });
        }
      };

      $.hood.Alerts.Confirm("The directory and all files will be permanently removed.", "Are you sure?", deleteDirectoryCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle mr-2"></i><strong>This cannot be undone!</strong><br />Ensure these files are not attached to any posts, pages or features of the site, or it will appear as a broken image or file.</span>');
    }
  },
  Players: {},
  LoadMediaPlayers: function LoadMediaPlayers() {
    var tag = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : '.hood-media';
    var videoOptions = {
      techOrder: ["azureHtml5JS", "flashSS", "html5FairPlayHLS", "silverlightSS", "html5"],
      nativeControlsForTouch: false,
      controls: true,
      autoplay: false,
      seeking: true
    };
    $(tag).each(function () {
      try {
        player = $.hood.Media.Players[$(this).data('id')];

        if (player) {
          try {
            player.dispose();
          } catch (ex) {
            console.log("There was a problem disposing the old media player: ".concat(ex));
          }
        }

        $.hood.Media.Players[$(this).data('id')] = amp($(this).attr('id'), videoOptions);
        player = $.hood.Media.Players[$(this).data('id')];
        player.src([{
          src: $(this).data('file'),
          type: $(this).data('type')
        }]);
      } catch (ex) {
        console.log("There was a problem playing the media file: ".concat(ex));
      }
    });
  }
};
$(document).ready($.hood.Media.Init);
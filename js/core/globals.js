"use strict";

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
    var counter = $(this).data('counter');
    var max = Number($(this).attr('maxlength'));
    var len = $(this).val().length;
    $(counter).text(max - len);
    var cls = "text-success";
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
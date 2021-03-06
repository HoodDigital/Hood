"use strict";

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
    var isSupported = (eventName in el);

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
  FallbackCopyTextToClipboard: function FallbackCopyTextToClipboard(text) {
    var textArea = document.createElement("textarea");
    textArea.value = text; // Avoid scrolling to bottom

    textArea.style.top = "0";
    textArea.style.left = "0";
    textArea.style.position = "fixed";
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
      var successful = document.execCommand('copy');
    } catch (err) {
      console.error('Oops, unable to copy', err);
    }

    document.body.removeChild(textArea);
  },
  CopyTextToClipboard: function CopyTextToClipboard(text) {
    if (!navigator.clipboard) {
      $.hood.Handlers.FallbackCopyTextToClipboard(text);
      return;
    }

    navigator.clipboard.writeText(text).then(function () {}, function (err) {
      console.error('Could not copy text: ', err);
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
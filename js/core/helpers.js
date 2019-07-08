"use strict";

if (!$.hood) $.hood = {};
$.hood.Helpers = {
  InIframe: function InIframe() {
    try {
      return window.self !== window.top;
    } catch (e) {
      return true;
    }
  },
  GetQueryStringParamByName: function GetQueryStringParamByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)", 'i'),
        results = regex.exec(location.search);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
  },
  InsertQueryStringParam: function InsertQueryStringParam(key, value) {
    key = escape(key);
    value = escape(value);
    var kvp = document.location.search.substr(1).split('&');

    if (kvp == '') {
      document.location.search = '?' + key + '=' + value;
    } else {
      var i = kvp.length;
      var x;

      while (i--) {
        x = kvp[i].split('=');

        if (x[0] == key) {
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

    if (kvp == '') {
      url.search = '?' + key + '=' + value;
    } else {
      var i = kvp.length;
      var x;

      while (i--) {
        x = kvp[i].split('=');

        if (x[0] == key) {
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
      isSupported = typeof el[eventName] == 'function';
    }

    el = null;
    return isSupported;
  },
  IsFunction: function IsFunction(functionToCheck) {
    return functionToCheck && {}.toString.call(functionToCheck) === '[object Function]';
  },
  HtmlEncode: function HtmlEncode(value) {
    //create a in-memory div, set it's inner text(which jQuery automatically encodes)
    //then grab the encoded contents back out.  The div never exists on the page.
    return $('<div/>').text(value).html();
  },
  HtmlDecode: function HtmlDecode(value) {
    return $('<div/>').html(value).text();
  },
  SlimScrollTopBottom: function SlimScrollTopBottom(elem, scrollUp) {
    if (scrollUp) {
      $(elem).slimScroll({
        scrollTo: '0px'
      });
    } else {
      var scrollTo_val = $(elem).prop('scrollHeight') + 'px';
      $(elem).slimScroll({
        scrollTo: scrollTo_val
      });
    }
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
  IsUrlExternal: function IsUrlExternal(url) {
    var match = url.match(/^([^:\/?#]+:)?(?:\/\/([^\/?#]*))?([^?#]+)?(\?[^#]*)?(#.*)?/);
    if (typeof match[1] === "string" && match[1].length > 0 && match[1].toLowerCase() !== location.protocol) return true;
    if (typeof match[2] === "string" && match[2].length > 0 && match[2].replace(new RegExp(":(" + {
      "http:": 80,
      "https:": 443
    }[location.protocol] + ")?$"), "") !== location.host) return true;
    return false;
  },
  UrlToLocationObject: function UrlToLocationObject(href) {
    var l = document.createElement("a");
    l.href = href;
    return l;
  },
  SeoUrl: function SeoUrl(txt_src) {
    var output = txt_src.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, "-").toLowerCase();
    /* remove first dash */

    if (output.charAt(0) == '-') output = output.substring(1);
    /* remove last dash */

    var last = output.length - 1;
    if (output.charAt(last) == '-') output = output.substring(0, last);
    return output;
  },
  FindAndRemoveFromArray: function FindAndRemoveFromArray(array, property, value) {
    $.each(array, function (index, result) {
      if (result[property] == value) {
        //Remove from array
        array.splice(index, 1);
      }
    });
  },
  FindAndReturnFromArray: function FindAndReturnFromArray(array, property, value) {
    var outputItem = null;
    $.each(array, function (index, result) {
      if (result[property] == value) {
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
  SubmitPageToForm: function myFunction(action, method, input) {
    'use strict';

    var form;
    form = $('<form />', {
      action: action,
      method: method,
      style: 'display: none;'
    });

    if (typeof input !== 'undefined' && input !== null) {
      for (var key in input) {
        if (input.hasOwnProperty(key)) {
          $('<input />', {
            type: 'hidden',
            name: key,
            value: input[key]
          }).appendTo(form);
        }
      }
    }

    form.appendTo('body').submit();
  },
  GenerateRandomString: function GenerateRandomString(numSpecials) {
    specials = '!@#$&*';
    lowercase = 'abcdefghijklmnopqrstuvwxyz';
    uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    numbers = '0123456789';
    all = lowercase + uppercase + numbers;
    password = (specials.pick(1) + lowercase.pick(1) + uppercase.pick(1) + all.pick(5, 10)).shuffle();
    return password;
  },
  ResetSidebarScroll: function ResetSidebarScroll() {
    $('.sidebar-container').slimScroll({
      height: '100%',
      railOpacity: 0.4,
      wheelStep: 10
    });
  },
  InitMetisMenu: function InitMetisMenu(tag) {
    $(tag).find('.metisMenu').metisMenu();
  }
};
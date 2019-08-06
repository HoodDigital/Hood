if (!$.hood)
    $.hood = {};
$.hood.Helpers = {

    IsNullOrUndefined: function (a) {
        let rc = false;
        if (a === null || typeof (a) === "undefined" || a === "") {
            rc = true;
        }
        return rc;
    },
    IsSet: function (a) {
        let rc = false;
        if (a === null || typeof (a) === "undefined" || a === "") {
            rc = true;
        }
        return !rc;
    },
    IsEventSupported: function (eventName) {
        let el = document.createElement('div');
        eventName = 'on' + eventName;
        var isSupported = (eventName in el);
        if (!isSupported) {
            el.setAttribute(eventName, 'return;');
            isSupported = typeof el[eventName] === 'function';
        }
        el = null;
        return isSupported;
    },
    IsFunction: function (functionToCheck) {
        return functionToCheck && {}.toString.call(functionToCheck) === '[object Function]';
    },
    IsUrlExternal: function (url) {
        let match = url.match(/^([^:\/?#]+:)?(?:\/\/([^\/?#]*))?([^?#]+)?(\?[^#]*)?(#.*)?/);
        if (typeof match[1] === "string" && match[1].length > 0 && match[1].toLowerCase() !== location.protocol) return true;
        if (typeof match[2] === "string" && match[2].length > 0 && match[2].replace(new RegExp(":(" + { "http:": 80, "https:": 443 }[location.protocol] + ")?$"), "") !== location.host) return true;
        return false;
    },
    IsInIframe: function () {
        try {
            return window.self !== window.top;
        } catch (e) {
            return true;
        }
    },

    HtmlEncode: function (value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(value).html();
    },
    HtmlDecode: function (value) {
        return $('<div/>').html(value).text();
    },

    FormatCurrency: function (n, currency) {
        return currency + " " + n.toFixed(2).replace(/./g, function (c, i, a) {
            return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
        });
    },
    FormatKilobytes: function (n) {
        n = n / 1024;
        return n.toFixed(2).replace(/./g, function (c, i, a) {
            return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
        });
    },
    FormatMegabytes: function (n) {
        n = n / 1024;
        n = n / 1024;
        return currency + " " + n.toFixed(2).replace(/./g, function (c, i, a) {
            return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
        });
    },

    ProcessResponse: function (data) {
        let title = '';
        if (data.Title) title = `<strong>${data.Title}</strong><br />`;
        if (data.Success) {
            $.hood.Alerts.Success(`${title}${data.Message}`);
        } else {
            $.hood.Alerts.Error(`${title}${data.Errors}`);
        }
    },

    GetQueryStringParamByName: function (name) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)", 'i'),
            results = regex.exec(location.search);
        return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    },
    InsertQueryStringParam: function (key, value) {
        key = escape(key); value = escape(value);
        var kvp = document.location.search.substr(1).split('&');
        if (kvp === '') {
            document.location.search = '?' + key + '=' + value;
        }
        else {

            var i = kvp.length; var x; while (i--) {
                x = kvp[i].split('=');

                if (x[0] === key) {
                    x[1] = value;
                    kvp[i] = x.join('=');
                    break;
                }
            }

            if (i < 0) { kvp[kvp.length] = [key, value].join('='); }

            //this will reload the page, it's likely better to store this until finished
            document.location.search = kvp.join('&');
        }
    },
    InsertQueryStringParamToUrl: function (url, key, value) {
        key = escape(key); value = escape(value);
        var kvp = url.search.substr(1).split('&');
        if (kvp === '') {
            url.search = '?' + key + '=' + value;
        }
        else {

            var i = kvp.length; var x; while (i--) {
                x = kvp[i].split('=');

                if (x[0] === key) {
                    x[1] = value;
                    kvp[i] = x.join('=');
                    break;
                }
            }

            if (i < 0) { kvp[kvp.length] = [key, value].join('='); }

            //this will reload the page, it's likely better to store this until finished
            url.search = kvp.join('&');
        }
        return url;
    },

    UrlToLocationObject: function (href) {
        let l = document.createElement("a");
        l.href = href;
        return l;
    },

    FindAndRemoveFromArray: function (array, property, value) {
        $.each(array, function (index, result) {
            if (result[property] === value) {
                //Remove from array
                array.splice(index, 1);
            }
        });
    },
    FindAndReturnFromArray: function (array, property, value) {
        let outputItem = null;
        $.each(array, function (index, result) {
            if (result[property] === value) {
                //return it
                outputItem = result;
            }
        });
        return outputItem;
    },

    LeftPad: function (number, targetLength) {
        let output = number + '';
        while (output.length < targetLength) {
            output = '0' + output;
        }
        return output;
    },

    DateAdd: function (date, interval, units) {
        let ret = new Date(date); //don't change original date
        switch (interval.toLowerCase()) {
            case 'year': ret.setFullYear(ret.getFullYear() + units); break;
            case 'quarter': ret.setMonth(ret.getMonth() + 3 * units); break;
            case 'month': ret.setMonth(ret.getMonth() + units); break;
            case 'week': ret.setDate(ret.getDate() + 7 * units); break;
            case 'day': ret.setDate(ret.getDate() + units); break;
            case 'hour': ret.setTime(ret.getTime() + units * 3600000); break;
            case 'minute': ret.setTime(ret.getTime() + units * 60000); break;
            case 'second': ret.setTime(ret.getTime() + units * 1000); break;
            default: ret = undefined; break;
        }
        return ret;
    },

    GenerateRandomString: function (numSpecials) {
        let  specials = '!@#$&*';
        let  lowercase = 'abcdefghijklmnopqrstuvwxyz';
        let uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
        let numbers = '0123456789';
        let all = lowercase + uppercase + numbers;
        let password = (specials.pick(1) + lowercase.pick(1) + uppercase.pick(1) + all.pick(5, 10)).shuffle();
        return password;
    }
};

export class Helpers {
    constructor() {

    }

    static isNullOrUndefined(a: any) {
        let rc = false;
        if (a === null || typeof a === "undefined" || a === "") {
            rc = true;
        }
        return rc;
    }

    static isSet(a: any) {
        let rc = false;
        if (a === null || typeof a === "undefined" || a === "") {
            rc = true;
        }
        return !rc;
    }

    static isFunction(a: string) {
        return a && {}.toString.call(a) === '[object Function]';
    }

    static insertQueryStringParamToUrl(url: { search: string; }, key: string, value: string) {
        key = escape(key); value = escape(value);
        var kvp = url.search.substr(1).split('&');
        if (kvp.length == 1) {
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
    }

}



import { Alerts } from "./alerts";
import { Response } from "./response";

export class Helpers {

    constructor() {

    }

    static isNullOrUndefined(a: string) {
        let rc = false;
        if (a === null || typeof a === "undefined" || a === "") {
            rc = true;
        }
        return rc;
    }

    static isSet(a: string) {
        let rc = false;
        if (a === null || typeof a === "undefined" || a === "") {
            rc = true;
        }
        return !rc;
    }

    static isFunction(a: string) {
        return a && {}.toString.call(a) === '[object Function]';
    }

    static processResponse (data: Response) {
        let title = '';
        if (data.Title) title = `<strong>${data.Title}</strong><br />`;
        //if (data.Success) {
        //    //Alerts.Success();
        //} else {
        //    Alerts.Error(`${title}${data.Errors}`);
        //}
    }

}



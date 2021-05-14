import { KeyValue } from "../interfaces/KeyValue";
import { Alerts } from "./Alerts";
import { MediaObject } from "./Media";

export declare interface Response {

    data: any[];
    count: number;

    url: string;

    title: string;
    message: string;

    success: boolean;

    exception: KeyValue<string, string>[];
    errors: string;
    error: string;

    media: MediaObject;

    process(autoHide: number): void;

}

export class Response {
    constructor() {

    }

    static process(data: Response | any, autoHide: number = 5000) {
        if (data.success) {
            Alerts.success(data.message, data.title, autoHide);
        } else {
            Alerts.error(data.message, data.title, autoHide);
        }
    }
}


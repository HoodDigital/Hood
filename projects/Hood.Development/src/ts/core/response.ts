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

    process(autoHide: number = 5000) {
        if (this.success) {
            Alerts.success(this.message, this.title, autoHide);
        } else {
            Alerts.error(this.message, this.title, autoHide);
        }
    }
}


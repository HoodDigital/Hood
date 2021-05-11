import { Alerts } from "./Alerts";

export class Response {
    constructor() {

    }

    Title: string;
    Success: boolean;
    Message: string;
    Errors: string[];
    Error: string;
    Media: {
        Url: string;
        SmallUrl: string;
        MediumUrl: string;
        LargeUrl: string;
    }

    static process(data: Response, autoHide: number = 5000) {
        if (data.Success) {
            Alerts.Success(data.Message, data.Title, autoHide);
        } else {
            Alerts.Error(data.Message, data.Title, autoHide);
        }
    }

}
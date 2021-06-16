import { Alerts } from "./core/Alerts";
import { BaseController } from "./core/HoodController";

export * from "./hood";

export class App extends BaseController {
    constructor() {
        super();
    }
    saySomethingGrate(something: string) {
        Alerts.success('This is ' + something);
    }
}

export var app = new App();

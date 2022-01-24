/// <reference types="google.maps" />

import { HoodApi } from ".";

export class App extends HoodApi {

    constructor() {
        super();
        // Setup defaults with HoodApi default initialise function. 
        this.initialise();
    }

}

window.hood = new App();

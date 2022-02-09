/// <reference types="google.maps" />

import { HoodApi, PropertyService } from ".";

export class App extends HoodApi {
    property: PropertyService;

    constructor() {
        super();

        // Setup defaults with HoodApi default initialise function. 
        this.initialise();

        // Initialise the property controllers.
        this.property = new PropertyService();
    }

}

window.hood = new App();

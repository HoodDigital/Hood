/// <reference types="google.maps" />

import { HoodApi } from "./core/HoodApi";
import { PropertyController } from './app/PropertyController';

export class App extends HoodApi {
    property: PropertyController;
    
    constructor() {
        super();

        // Setup defaults with HoodApi default initialise function. 
        this.initialise();

        // Initialise the property controllers.
        this.property = new PropertyController();
    }

}

window.hood = new App();

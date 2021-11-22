export class BaseSite  {

    constructor() {

        // Initialise
        this.initialise();

        $(window).on('load', this.onLoad.bind(this));
        $(window).on('resize', this.onResize.bind(this));

    }

    initialise() {
    }

    onLoad() {
    }

    onResize() {
    }
}
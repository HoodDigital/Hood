export class Loader  {
    constructor() {
        new CustomEvent('loader-show');
        new CustomEvent('loader-hide');
    }

    static show(): void {
        $('body').trigger('loader-show');
    }

    static hide(): void {
        $('body').trigger('loader-hide');
    }
}

new Loader()
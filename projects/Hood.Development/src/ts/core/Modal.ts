import { Modal } from "bootstrap";
import { Inline } from "./Inline";

export interface ModalOptions {

    /**
     * Called before the data is fetched.
     */
    onLoad?: (sender: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the list. Must return the data back to datalist to render.
     */
    onRender?: (sender: HTMLElement, html: string) => string;

    /**
     * Called when loading and rendering is complete.
     */
    onComplete?: (sender: HTMLElement) => void;

    /**
     * Called when loading and rendering is complete.
     */
    onError?: (jqXHR: any, textStatus: any, errorThrown: any) => void;

    closePrevious?: boolean;
}


export class ModalController {
    element: HTMLElement;
    modal: Modal;
    options: ModalOptions = {
        closePrevious: true
    }

    constructor(options: ModalOptions) {
        this.options = { ...this.options, ...options };
    }

    show(url: string | URL, sender: HTMLElement) {
        let that = this;

        if (that.options.onLoad) {
            that.options.onLoad(that.element);
        }

        $.get(url as string, function (data: string) {

            if (that.modal && that.options.closePrevious) {
                that.close();
            }

            if (that.options.onRender) {
                data = that.options.onRender(that.element, data);
            }

            that.element = that.createElementFromHTML(data);
            that.element.classList.add('hood-inline-modal');

            $('body').append(that.element);
            that.modal = new Modal(that.element, {});
            that.modal.show();

            // Workaround for sweetalert popups.
            that.element.addEventListener('shown.bs.modal', function () {
                $(document).off('focusin.modal');
            });
            that.element.addEventListener('hidden.bs.modal', function (e) {
                that.modal.dispose();
            });

            if (that.options.onComplete) {
                that.options.onComplete(that.element);
            }
        })
            .fail(that.options.onError ?? Inline.HandleError);
    }

    close() {
        if (this.modal) {
            this.modal.hide();
            this.modal.dispose();
        }
    }

    createElementFromHTML(htmlString: string): HTMLElement {
        var div = document.createElement('div');
        div.innerHTML = htmlString.trim();

        // Change this to div.childNodes to support multiple top-level nodes
        return div.firstChild as HTMLElement;
    }
}
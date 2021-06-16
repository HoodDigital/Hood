import { Alerts } from "./Alerts";
import { Response } from "./Response";

export interface InlineOptions {

    /**
     * Called before the data is fetched.
     */
    onLoad?: (sender?: HTMLElement) => void;

    /**
     * Called before the fetched HTML is rendered to the list. Must return the data back to datalist to render.
     */
    onRender?: (html: string, sender?: HTMLElement) => string;

    /**
     * Called when loading and rendering is complete.
     */
    onComplete?: (html: string, sender?: HTMLElement) => void;

    /**
     * Called when an error occurs.
     */
    onError?: (jqXHR: any, textStatus: any, errorThrown: any) => void;

}

export class Inline {
    static load(tag: HTMLElement, options: InlineOptions) {

        let $tag = $(tag) as JQuery<HTMLElement>;
        $tag.addClass('loading');
        if (options.onLoad) {
            options.onLoad(tag);
        }

        let url: string = $tag.data('url');
        $.get(url, function (data: string) {

            if (options.onRender) {
                data = options.onRender(data, tag);
            }

            $tag.html(data);
            $tag.removeClass('loading');

            if (options.onComplete) {
                options.onComplete(data, tag);
            }

        })
            .fail(options.onError ?? Inline.handleError);
    }

    static task(url: string, sender: HTMLElement,
        complete: (data: Response, sender?: HTMLElement) => void = null,
        error: (jqXHR: any, textStatus: any, errorThrown: any) => void = null): void {

        if (sender) {
            sender.classList.add('loading');
        }
        $.get(url, function (response: Response) {
            if (sender) {
                sender.classList.remove('loading');
            }
            if (complete) {
                complete(response, sender);
            }
        })
            .fail(error ?? Inline.handleError);

    }

    static post(url: string, sender: HTMLElement,
        complete: (data: Response, sender?: HTMLElement) => void = null,
        error: (jqXHR: any, textStatus: any, errorThrown: any) => void = null): void {

        if (sender) {
            sender.classList.add('loading');
        }
        $.post(url, function (response: Response) {
            if (sender) {
                sender.classList.remove('loading');
            }
            if (complete) {
                complete(response, sender);
            }
        })
            .fail(error ?? Inline.handleError);

    }

    static handleError(xhr: { status: string | number; }) {
        if (xhr.status === 500) {
            Alerts.error("There was an error processing the content, please contact an administrator if this continues.", "Error " + xhr.status);
        } else if (xhr.status === 404) {
            Alerts.error("The content could not be found.", "Error " + xhr.status);
        } else if (xhr.status === 401) {
            Alerts.error("You are not allowed to view this resource, are you logged in correctly?", "Error " + xhr.status);
            window.location = window.location;
        }
    }
}


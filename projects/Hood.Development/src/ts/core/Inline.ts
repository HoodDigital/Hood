import { Alerts } from "./Alerts";
import { Response } from "./Response";
import { Loader } from "./Loader";

export interface InlineOptions {

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
    onComplete?: (sender: HTMLElement, html: string) => void;

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
                data = options.onRender(tag, data);
            }

            $tag.html(data);
            $tag.removeClass('loading');

            if (options.onComplete) {
                options.onComplete(tag, data);
            }

        })
            .fail(options.onError ?? Inline.handleError);
    }

    static task(e: JQuery.ClickEvent,
        complete: (sender: HTMLElement, data: Response) => void = null,
        autoHideResponse: number = null,
        error: (jqXHR: any, textStatus: any, errorThrown: any) => void = null): void {

        e.preventDefault();
        let $tag = $(e.currentTarget);
        $tag.addClass('loading');
        $.get($tag.attr('href'), function (data: Response) {
            Response.process(data, autoHideResponse);
            if (data.success) {
                if ($tag && $tag.data('redirect')) {
                    setTimeout(function () {
                        window.location = $tag.data('redirect');
                    }, 1500);
                }
            }
            $tag.removeClass('loading');
            if (complete) {
                complete(e.currentTarget, data);
            }
        })
            .fail(error ?? Inline.handleError);

    }

    static post(e: JQuery.ClickEvent,
        complete: (sender: HTMLElement, data: Response) => void = null,
        autoHideResponse: number = null,
        error: (jqXHR: any, textStatus: any, errorThrown: any) => void = null): void {

        e.preventDefault();
        let $tag = $(e.currentTarget);
        $tag.addClass('loading');
        $.post($tag.attr('href'), function (data: Response) {
            Response.process(data, autoHideResponse);
            if (data.success) {
                if ($tag && $tag.data('redirect')) {
                    setTimeout(function () {
                        window.location = $tag.data('redirect');
                    }, 1500);
                }
            }
            $tag.removeClass('loading');
            if (complete) {
                complete(e.currentTarget, data);
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


import { Alerts } from "./Alerts";
import { Inline } from "./Inline";

declare global {
    interface HTMLElement {
        hoodDataList: DataList;
    }
}

export interface DataListOptions {

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

/**
  * Attach a data list feed to an HTML element. The element must have a data-url attribute to connect to a feed.
  */

export class DataList {
    element: HTMLElement;
    options: DataListOptions;

    /**
      * @param element The datalist element. The element must have a data-url attribute to connect to a feed.
      */
    constructor(element: HTMLElement, options: DataListOptions) {

        this.element = element;
        this.element.hoodDataList = this;
        if (typeof(element) == 'undefined' || element == null) {
            Alerts.log('Could not DataList to element, element does not exist.', 'error');
            return;
        }

        this.options = { ...this.options, ...options };

        if ($(this.element).hasClass('query')) {
            let pageUrl = $(this.element).data('url') + window.location.search;
            $(this.element).attr('data-url', pageUrl);
            $(this.element).data('url', pageUrl);
        }

        if (!$(this.element).hasClass('refresh-only')) {
            var listUrl = document.createElement('a');
            listUrl.href = $(this.element).data('url');
            this.reload(new URL(listUrl.href));
        }

        $(this.element).on('click', '.pagination a, a.hood-inline-list-target', function (this: DataList, e: JQuery.ClickEvent) {

            e.preventDefault();

            var url = document.createElement('a');
            url.href = (e.currentTarget as HTMLAnchorElement).href;

            var listUrl = document.createElement('a');
            listUrl.href = $(this.element).data('url');
            listUrl.search = url.search;

            this.reload(new URL(listUrl.href));

        }.bind(this));

        $('body').on('submit', `form.inline[data-target="#${this.element.id}"]`, function (this: DataList, e: JQuery.ClickEvent) {

            e.preventDefault();

            let $form = $(e.currentTarget);

            var listUrl = document.createElement('a');
            listUrl.href = $(this.element).data('url');

            listUrl.search = "?" + $form.serialize();
            this.reload(new URL(listUrl.href));

        }.bind(this));

        //    $('body').on('submit', '.hood-inline-list form', function (e) {
        //        e.preventDefault();
        //        $.hood.Loader(true);
        //        let $form = $(this);
        //        let $list = $form.parents('.hood-inline-list');
        //        var url = document.createElement('a');
        //        url.href = $list.data('url');
        //        url.search = "?" + $form.serialize();
        //        $.hood.Inline.DataList.Reload($list, url);
        //    });
    }

    reload(this: DataList, url: URL = null) {
        if (url) {
            if (history.pushState && $(this.element).hasClass('query')) {
                let newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + (url.href.contains('?') ? "?" + url.href.substring(url.href.indexOf('?') + 1) : '');
                window.history.pushState({ path: newurl }, '', newurl);
            }
            $(this.element).data('url', url);
        }
        Inline.load(this.element, { ...this.options });
    }

}

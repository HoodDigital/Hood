import * as $ from 'jquery';

export class Handlers {
    constructor() {

        $('body').on('click', '.scroll-top, .scroll-to-top', this.ScrollToTop);
        $('body').on('click', '.scroll-target, .scroll-to-target', this.ScrollToTarget);
        $('body').on('click', '.slide-link', this.SlideToAnchor);

        $('body').on('change', '.submit-on-change', this.SubmitOnChange);
        $('body').on('change', '.inline-date', this.DateChange);
        $('body').on('change', 'input[type=checkbox][data-input]', this.CheckboxChange);

        $('select[data-selected]').each(this.SelectSetup);

        $('body').on('click', '.select-text', this.SelectTextContent);
        $('body').on('click', '.btn.click-select[data-target][data-value]', this.ClickSelect);
        $('body').on('click', '.click-select.show-selected[data-target][data-value]', this.ClickSelect);
        $('body').on('click', '.click-select:not(.show-selected)[data-target][data-value]', this.ClickSelectClean);

        $('[data-hood-icon]').each(this.IconSelector);

    }

    IconSelector(this: HTMLElement, index: number, element: HTMLElement) {
        let input = $(this).find('input[data-hood-icon-input]')
        let display = $(this).find('[data-hood-icon-display]')
        let collapse = $(this).find('.collapse')

        $(this).find('[data-hood-icon-key][data-hood-icon-value]').on('click', function () {
            let key = $(this).data('hoodIconKey');
            let value = $(this).data('hoodIconValue');
            display.html(value);
            input.val(key);
            if (collapse) {
                collapse.removeClass('show');
            }
        });
    }

    ScrollToTop(this: HTMLAnchorElement, e: JQuery.ClickEvent) {
        if (e) e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    }

    ScrollToTarget(this: HTMLAnchorElement, e: JQuery.ClickEvent) {
        if (e) e.preventDefault();
        let url = $(this).attr('href').split('#')[0];
        if (url !== window.location.pathname && url !== "") {
            return;
        }
        let target = this.hash;
        let $target = $(target);
        let $header = $('header.header');
        let headerOffset = 0;
        if ($header) {
            headerOffset = $header.height();
        }
        if ($(this).data('offset'))
            $('html, body').stop().animate({
                'scrollTop': $target.offset().top - $(this).data('offset')
            }, 900, 'swing');
        else
            $('html, body').stop().animate({
                'scrollTop': $target.offset().top - headerOffset
            }, 900, 'swing');
    }

    SlideToAnchor(this: HTMLAnchorElement) {
        let scrollTop = $('body').scrollTop();
        let top = $($(this).attr('href')).offset().top;

        $('html, body').animate({
            scrollTop: top
        }, Math.abs(top - scrollTop));
        return false;
    }

    SubmitOnChange(this: HTMLFormElement, e: JQuery.ChangeEvent) {
        if (e) e.preventDefault();
        $(this).parents('form').submit();
    }

    DateChange(this: HTMLInputElement, e: JQuery.ChangeEvent) {
        if (e) e.preventDefault();
        // update the date element attached to the field's attach
        let $field = $(this).parents('.hood-date').find('.date-output');
        let date: string = $field.parents('.hood-date').find('.date-value').val() as string;
        let pattern = /^([0-9]{2})\/([0-9]{2})\/([0-9]{4})$/;
        if (!pattern.test(date))
            date = "01/01/2001";
        let hour = $field.parents('.hood-date').find('.hour-value').val();
        if (!$.isNumeric(hour))
            hour = "00";
        let minute = $field.parents('.hood-date').find('.minute-value').val();
        if (!$.isNumeric(minute))
            minute = "00";
        $field.val(date + " " + hour + ":" + minute + ":00");
        $field.attr("value", date + " " + hour + ":" + minute + ":00");
    }

    CheckboxChange(this: HTMLInputElement, e: JQuery.ChangeEvent<HTMLElement>) {
        if (e) e.preventDefault();
        // when i change - create an array, with any other checked of the same data-input checkboxes. and add to the data-input referenced tag.
        let items = new Array();
        $('input[data-input="' + $(this).data('input') + '"]').each(function () {
            if ($(this).is(":checked"))
                items.push($(this).val());
        });
        let id = '#' + $(this).data('input');
        let vals = JSON.stringify(items);
        $(id).val(vals);
    }

    SelectSetup(this: HTMLElement, index: number, element: HTMLElement) {
        let sel = $(this).data('selected');
        if (sel !== 'undefined' && sel !== '') {
            let selected = String(sel);
            $(this).val(selected);
        }
    }

    SelectTextContent(this: HTMLInputElement) {
        let $this = $(this);
        $this.select();
        // Work around Chrome's little problem
        $this.mouseup(function () {
            // Prevent further mouseup intervention
            $this.unbind("mouseup");
            return false;
        });
    }

    ClickSelect(this: HTMLElement) {
        let $this = $(this);
        let targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
        $(targetId).trigger('change');
        $('.click-select[data-target="' + $this.data('target') + '"]').each(function () { $(this).html($(this).data('temp')).removeClass('active'); });
        $('.click-select[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () { $(this).data('temp', $(this).html()).html('Selected').addClass('active'); });
    }

    ClickSelectClean(this: HTMLElement) {
        let $this = $(this);
        let targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
        $(targetId).trigger('change');
        $('.click-select.clean[data-target="' + $this.data('target') + '"]').each(function () { $(this).removeClass('active'); });
        $('.click-select.clean[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () { $(this).addClass('active'); });
    }

}
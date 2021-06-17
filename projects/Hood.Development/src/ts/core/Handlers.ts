export class Handlers {
    constructor() {
    }

    initDefaultHandlers() {

        this.checkboxToCsvInput();
        this.iconSelector();
        this.initSelectValues();
        this.scrollHandlers();
        this.selectText();
        this.setValueOnClick();
        this.submitOnChange();

    }

    /**
     * Sets values of any selects that have the value set in data-selected, useful for 
     */
    initSelectValues(tag: string = 'body') {
        $(tag).find('select[data-selected]').each(this.initSelectValuesHandler);
    }

    initSelectValuesHandler(this: HTMLElement, index: number, element: HTMLElement) {
        let sel = $(this).data('selected');
        if (sel !== 'undefined' && sel !== '') {
            let selected = String(sel);
            $(this).val(selected);
        }
    }

    /**
     * Sets up any Hood Icon selector fields, requires the correct HTML setup.
     */
    iconSelector(tag: string = 'body') {
        $(tag).find('[data-hood-icon]').each(this.iconSelectorHandler);
    }

    iconSelectorHandler(this: HTMLElement, index: number, element: HTMLElement) {
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

    /**
     * Submits the form when input is changed, mark inputs with .submit-on-change class.
     */
    selectText(tag: string = 'body') {
        $(tag).on('click', '.select-text', this.selectTextHandler);
    }

    selectTextHandler(this: HTMLInputElement) {
        let $this = $(this);
        $this.select();
        // Work around Chrome's little problem
        $this.mouseup(function () {
            // Prevent further mouseup intervention
            $this.unbind("mouseup");
            return false;
        });
    }

    /**
     * Attaches handlers for default scrolling functions, scroll to top, scroll to target (with header.header offset calculated)
     * and scroll to target direct (with no calculated offset).
     */
    scrollHandlers(tag: string = 'body') {
        $(tag).on('click', '.scroll-top, .scroll-to-top', this.scrollTop);
        $(tag).on('click', '.scroll-target, .scroll-to-target', this.scrollTarget);
        $(tag).on('click', '.scroll-target-direct, .scroll-to-target-direct', this.scrollTargetDirect);
    }

    scrollTop(this: HTMLAnchorElement, e: JQuery.ClickEvent) {
        if (e) e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    }

    scrollTarget(this: HTMLAnchorElement, e: JQuery.ClickEvent) {
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

    scrollTargetDirect(this: HTMLAnchorElement) {
        let scrollTop = $('body').scrollTop();
        let top = $($(this).attr('href')).offset().top;

        $('html, body').animate({
            scrollTop: top
        }, Math.abs(top - scrollTop));
        return false;
    }

    /**
    * Compiles any selected checkboxes with matching data-hood-csv-input tags,
    * then saves the CSV list of the values to the input given in the tag.
    */
    checkboxToCsvInput(tag: string = 'body') {
        $(tag).on('change', 'input[type=checkbox][data-hood-csv-input]', this.checkboxToCsvInputHandler);
    }

    checkboxToCsvInputHandler(this: HTMLInputElement, e: JQuery.ChangeEvent<HTMLElement>) {
        if (e) e.preventDefault();
        // when i change - create an array, with any other checked of the same data-input checkboxes. and add to the data-input referenced tag.
        let items = new Array();
        $('input[data-hood-csv-input="' + $(this).data('hoodCsvInput') + '"]').each(function () {
            if ($(this).is(":checked"))
                items.push($(this).val());
        });
        let id = '#' + $(this).data('input');
        let vals = JSON.stringify(items);
        $(id).val(vals);
    }

    /**
    * Submits the form when input is changed, mark inputs with .submit-on-change class.
    */
    submitOnChange(tag: string = 'body') {
        $(tag).on('change', '.submit-on-change', this.submitOnChangeHandler);
    }

    submitOnChangeHandler(this: HTMLFormElement, e: JQuery.ChangeEvent) {
        if (e) e.preventDefault();
        $(this).parents('form').submit();
    }

    /**
    * Sets the value of the input [data-target] when clicked to the value in [data-value]
    */
    setValueOnClick(tag: string = 'body') {
        $(tag).on('click', '.click-select[data-target][data-value]', this.setValueOnClickHandler);
    }

    setValueOnClickHandler(this: HTMLElement) {
        let $this = $(this);
        let targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
        $(targetId).trigger('change');
        $('.click-select.clean[data-target="' + $this.data('target') + '"]').each(function () { $(this).removeClass('active'); });
        $('.click-select.clean[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () { $(this).addClass('active'); });
    }

}
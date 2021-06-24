import { SweetAlertResult } from 'sweetalert2';
import { Alerts } from '../core/Alerts';

declare global {
    interface JQuery {
        exists(): number;
        restrictToSlug(): void;
        restrictToPageSlug(): void;
        restrictToMetaSlug(): void;
        characterCounter(): void;
        warningAlert(): void;
    }
}

$.fn.exists = function () {
    return $(this).length;
};

$.fn.restrictToSlug = function (restrictPattern: RegExp = /[^0-9a-zA-Z]*/g) {
    let targets = $(this);

    // The characters inside this pattern are accepted
    // and everything else will be 'cleaned'
    // For example 'ABCdEfGhI5' become 'ABCEGI5'

    var restrictHandler = function () {
        var val = $(this).val() as string;
        var newVal = val.replace(restrictPattern, '');

        // This condition is to prevent selection and keyboard navigation issues
        if (val !== newVal) {
            $(this).val(newVal);
        }
    };

    targets.on('keyup', restrictHandler);
    targets.on('paste', restrictHandler);
    targets.on('change', restrictHandler);
};
$.fn.restrictToPageSlug = function (restrictPattern: RegExp = /[^0-9a-zA-Z-//]*/g) {
    let targets = $(this);

    // The characters inside this pattern are accepted
    // and everything else will be 'cleaned'

    let restrictHandler = function () {
        var val = $(this).val() as string;
        var newVal = val.replace(restrictPattern, '');
        if ((newVal.match(new RegExp("/", "g")) || []).length > 4) {
            var pos = newVal.lastIndexOf('/');
            newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
            Alerts.warning("You can only have up to 4 '/' characters in a url slug.");
        }
        // This condition is to prevent selection and keyboard navigation issues
        if (val !== newVal) {
            $(this).val(newVal);
        }
    };

    targets.on('keyup', restrictHandler);
    targets.on('paste', restrictHandler);
    targets.on('change', restrictHandler);
};
$.fn.restrictToMetaSlug = function (restrictPattern: RegExp = /[^0-9a-zA-Z.]*/g) {
    let targets = $(this);

    // The characters inside this pattern are accepted
    // and everything else will be 'cleaned'

    let restrictHandler = function () {
        let val = $(this).val() as string;
        let newVal = val.replace(restrictPattern, '');
        if ((newVal.match(new RegExp(".", "g")) || []).length > 1) {
            let pos = newVal.lastIndexOf('.');
            newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
            Alerts.warning("You can only have up to 1 '.' characters in a meta slug.");
        }
        // This condition is to prevent selection and keyboard navigation issues
        if (val !== newVal) {
            $(this).val(newVal);
        }
    };

    targets.on('keyup', restrictHandler);
    targets.on('paste', restrictHandler);
    targets.on('change', restrictHandler);
};

$.fn.characterCounter = function () {
    let targets = $(this);
    let characterCounterHandler = function () {
        let counter = $(this).data('counter');
        let max = Number($(this).attr('maxlength'));
        let len = ($(this).val() as string).length;
        $(counter).text(max - len);
        let cls = "text-success";
        if (max - len < max / 10)
            cls = "text-danger";
        $(counter).parent().removeClass('text-success').removeClass('text-danger').addClass(cls);
    };
    targets.on('keyup', characterCounterHandler);
    targets.on('paste', characterCounterHandler);
    targets.on('change', characterCounterHandler);
};

$.fn.warningAlert = function () {
    let targets = $(this);
    let warningAlertHandler = function (this: HTMLElement, e: JQuery.ClickEvent) {
        e.preventDefault();
        let warningAlertCallback = function (result: SweetAlertResult) {
            if (result.isConfirmed) {
                let url = $(e.currentTarget).attr('href');
                window.location.href = url;
            }
        };
        Alerts.confirm(
            {
                title: $(e.currentTarget).data('title'),
                html: $(e.currentTarget).data('warning'),
                footer: $(e.currentTarget).data('footer'),
                icon: 'warning'
            },
            warningAlertCallback
        );
        return false;
    };
    targets.on('click', warningAlertHandler);
};


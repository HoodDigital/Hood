import { Inline } from "./Inline";
import { Response } from "./Response";
import 'jquery-validation';

export interface ValidatorOptions {

    /**
     * Called before the submit of the form data, you can return the data to be serialised.
     */
    onSubmit?: (sender: Validator) => void;

    /**
     * Called when submit is complete.
     */
    onComplete?: (sender: Validator, data: Response) => void;

    /**
     * Called when an error occurs.
     */
    onError?: (jqXHR: any, textStatus: any, errorThrown: any) => void;

    validationRules?: null;
    validationMessages?: {};


    serializationFunction?: () => string;

    placeBelow?: true;
    focusInvalid?: boolean;

    errorClass?: string;

}

export class Validator {
    element: HTMLFormElement;
    options: ValidatorOptions = {};

    /**
      * @param element The datalist element. The element must have a data-url attribute to connect to a feed.
      */
    constructor(element: HTMLFormElement, options: ValidatorOptions) {

        let that = this;
        this.element = element;
        if (!this.element) {
            return;
        }

        this.options.serializationFunction = function () {
            let rtn = $(that.element).serialize();
            return rtn;
        }

        this.options = { ...this.options, ...options };

        $(this.element).validate({
            submitHandler: function (e) {
                e.preventDefault();
            },
            errorClass: this.options.errorClass,
            focusInvalid: this.options.focusInvalid,
            rules: this.options.validationRules,
            messages: this.options.validationMessages,
        });
        $(this.element).on('submit', function () {
            that.submitForm();
        });
    }

    submitForm() {
        let that = this;

        let $form = $(this.element);
        if ($form.valid()) {

            $form.addClass('loading');

            $form.find('input[type=checkbox]').each(function () {
                if ($(this).is(':checked')) {
                    $(this).val('true');
                }
            });

            this.options.onSubmit(this);

            let formData = this.options.serializationFunction();

            $.post($form.attr('action'), formData, function (data) {
                that.options.onComplete(that, data);
            })
                .fail(that.options.onError ?? Inline.HandleError);
        }
    }
}
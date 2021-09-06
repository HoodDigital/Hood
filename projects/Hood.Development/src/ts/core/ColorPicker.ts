import Pickr from '@simonwep/pickr';
import '@simonwep/pickr';

export class ColorPickers {
    constructor(tag: string = '.color-picker') {

        let updateColorFieldValue = function (color: Pickr.HSVaColor, eventSource: any, instance: any) {
            let elemId = $(instance._root.button).parent().data('target');
            $(instance._root.button).parent().css({ 'background-color': color.toHEXA().toString() });
            let colorHex = instance.getColor().toHEXA();
            var result = "";
            for (let i = colorHex.length - 1; i >= 0; i--) {
                result = colorHex[i] + result;
            }
            $(elemId).val('#' + result);
            $(elemId).change();
        };

        var pickrs = [];
        // Simple example, see optional options for more configuration.
        $(tag).each(function (index: number, elem: HTMLElement) {

            let lockOpacity = true;
            if ($(this).data('opacity') == 'true') {
                lockOpacity = false;
            }

            let pickr = Pickr.create({
                el: elem.children[0] as HTMLElement,
                appClass: 'custom-class',
                theme: 'monolith',
                useAsButton: true,
                default: $(this).data('default') || 'none',
                lockOpacity: lockOpacity,
                defaultRepresentation: 'HEXA',
                position: 'bottom-end',
                components: {
                    opacity: true,
                    hue: true,

                    interaction: {
                        hex: false,
                        rgba: false,
                        hsva: false,
                        input: true,
                        clear: true
                    }
                }
            })

                .on('init', function (instance: any) {
                    let elemId = $(instance._root.button).parent().data('target');
                    let value = $(elemId).val();
                    $(instance._root.button).parent().on('click', $.proxy(function () {
                        this.show();
                    }, instance));
                    $(elemId).on('click', $.proxy(function () {
                        this.show();
                    }, instance));
                    if (value) {
                        instance.setColor(value);
                        updateColorFieldValue(instance.getColor(), null, instance);
                    }
                })

                .on('clear', function (instance: any) {
                    let elemId = $(instance._root.button).parent().data('target');
                    instance.setColor('transparent');
                    updateColorFieldValue(instance.getColor(), null, instance);
                    $(elemId).val('');
                    $(elemId).change();
                })

                .on('change', updateColorFieldValue);

            pickrs.push(pickr);

        });

    }
}
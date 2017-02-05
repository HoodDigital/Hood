if (!$.hood)
    $.hood = {};
$.hood.Handlers = {
    Init: function () {
        // Click to select boxes
        $('body').on('click', '.select-text', $.hood.Handlers.SelectTextContent);
        $('body').on('click', '.btn.click-select[data-target][data-value]', $.hood.Handlers.ClickSelectButton);
        $('body').on('click', '.click-select[data-target][data-value]', $.hood.Handlers.ClickSelect);
        $('body').on('click', '.slide-link', $.hood.Handlers.SlideToAnchor);
        $('body').on('change', 'input[type=checkbox][data-input]', $.hood.Handlers.CheckboxChange);

        $('select[data-selected]').each($.hood.Handlers.SelectSetup);
        // date/time meta editor
        $('body').on('change', '.inline-date', $.hood.Handlers.DateChange);

    },
    DateChange: function (e) {
        // update the date element attached to the field's attach
        $field = $(this).parents('.hood-date').find('.date-output');
        date = $field.parents('.hood-date').find('.date-value').val();
        pattern = /^([0-9]{2})\/([0-9]{2})\/([0-9]{4})$/;
        if (!pattern.test(date))
            date = "01/01/2001";
        hour = $field.parents('.hood-date').find('.hour-value').val();
        if (!$.isNumeric(hour))
            hour = "00";
        minute = $field.parents('.hood-date').find('.minute-value').val();
        if (!$.isNumeric(minute))
            minute = "00";
        $field.val(date + " " + hour + ":" + minute + ":00");
        $field.attr("value", date + " " + hour + ":" + minute + ":00");
    },
    CheckboxChange: function (e) {
        // when i change - create an array, with any other checked of the same data-input checkboxes. and add to the data-input referenced tag.
        var items = new Array();
        $('input[data-input="' + $(this).data('input') + '"]').each(function () {
            if ($(this).is(":checked"))
                items.push($(this).val());
        });
        id = '#' + $(this).data('input');
        vals = JSON.stringify(items);
        $(id).val(vals);
    },
    SelectSetup: function () {
        sel = $(this).data('selected');
        if ($(this).data('selected') !== 'undefined' && $(this).data('selected') !== '') {
            selected = String($(this).data('selected'));
            $(this).val(selected);
        }
    },
    ClickSelect: function () {
        var $this = $(this);
        targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
    },
    ClickSelectButton: function () {
        var $this = $(this);
        targetId = '#' + $this.data('target');
        $(targetId).val($this.data('value'));
        $('.click-select[data-target="' + $this.data('target') + '"]').html($this.data('temp')).removeClass('active');
        $this.data('temp', $this.html()).html('Selected').addClass('active');
    },
    SelectTextContent: function () {
        var $this = $(this);
        $this.select();
        // Work around Chrome's little problem
        $this.mouseup(function () {
            // Prevent further mouseup intervention
            $this.unbind("mouseup");
            return false;
        });
    },
    SlideToAnchor: function () {
        var scrollTop = $('body').scrollTop();
        var top = $($.attr(this, 'href')).offset().top;

        $('html, body').animate({
            scrollTop: top
        }, Math.abs(top - scrollTop));
        return false;
    }
};
$.hood.Handlers.Init();

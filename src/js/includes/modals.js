if (!$.hood)
    $.hood = {}
$.hood.Modals = {
    Loading: false,
    Open: function (url, params, target, completeFunction) {
        if ($.hood.Modals.Loading)
            return;
        $.hood.Modals.Loading = true;
        $(target).data('temp', $(target).html());
        $(target).addClass('loading').append('<i class="fa fa-refresh fa-spin m-l-sm"></i>');
        $.get(url, params, function (data) {
            // get the id of the new modal object in the data.
            modalId = '#' + $(data).attr('id');
            if ($(modalId).length) {
                $(modalId).remove();
            }
            $('body').append(data);
            $(modalId).modal();
            // Workaround for sweetalert popups.
            $(modalId).on('shown.bs.modal', function () {
                $(document).off('focusin.modal');
            });
        })
         .done(function () {
             if (!$.hood.Helpers.IsNullOrUndefined(completeFunction)) {
                 try {
                     completeFunction();
                 } catch (ex) {
                     $.hood.Alerts.Error(ex.message);
                 }
             }
         })
         .fail(function (data) {
             $.hood.Alerts.Error("There was an error loading the modal window.<br/><br />" + data.status + " - " + data.statusText);
         })
         .always(function (data) {
             $(target).removeClass('loading').html($(target).data('temp'));
             $('body').css({ 'padding-right': 0 });
             $.hood.Modals.Loading = false;
         });
    },
    Close: function (target) {
        $(target).modal('hide');
        $(target).remove();
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
    }
};

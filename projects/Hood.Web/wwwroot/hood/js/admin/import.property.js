if (!$.hood)
    $.hood = {};
if (!$.hood.Import)
    $.hood.Import = {};
$.hood.Import.Property = {
    UpdateInterval: -1,
    Init: function () {
        if ($('#import-property-start').doesExist()) {
            $.hood.Import.Property.Update();
            $('#import-property-start').click(function () {
                //$(this).addLoader();
                $.ajax({
                    url: $('#import-property-start').data('url'),
                    type: "POST",
                    error: function (jqXHR, textStatus, errorThrown) {
                        $.hood.Import.Property.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                    },
                    success: function (result) {
                        $.hood.Import.Property.Update();
                    },
                    complete: function () {
                        //$(this).removeLoader();
                    }
                });
            });
            $('#import-property-cancel').click(function () {
                //$(this).addLoader();
                $.ajax({
                    url: $('#import-property-cancel').data('url'),
                    type: "POST",
                    error: function (jqXHR, textStatus, errorThrown) {
                        $.hood.Import.Property.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                    },
                    success: function (result) {
                        $.hood.Import.Property.Update();
                    },
                    complete: function () {
                        //$(this).removeLoader();
                    }
                });
            });
        }
    },
    Update: function () {
        $.ajax({
            url: $('#import-property-status').data('url'),
            type: "POST",
            error: function (jqXHR, textStatus, errorThrown) {
                $.hood.Import.Property.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
            },
            success: function (result) {
                if (result.Importer.Running) {
                    $.hood.Import.Property.View.ShowInfo();
                    clearInterval($.hood.Import.Property.UpdateInterval);
                    $.hood.Import.Property.UpdateInterval = setTimeout($.hood.Import.Property.Update, 250);
                } else {
                    clearInterval($.hood.Import.Property.UpdateInterval);
                    $.hood.Import.Property.View.HideInfo();
                }
                $('.tp').html(result.Importer.Total);
                $('#pu').html(result.Importer.Updated);
                $('#pa').html(result.Importer.Added);
                $('#pp').html(result.Importer.Processed);
                $('#pd').html(result.Importer.Deleted);
                $('#ToAdd').html(result.Importer.ToAdd);
                $('#ToUpdate').html(result.Importer.ToUpdate);
                $('#ToDelete').html(result.Importer.ToDelete);
                $('#pt').html(result.Importer.StatusMessage);
                $('#fp').html(Math.round(result.Ftp.Complete * 100) / 100);
                $('#ft').html(result.Ftp.StatusMessage);
                $('.pc').html(Math.round(result.Importer.Complete * 100) / 100);
                $('#progressbar').css({
                    width: result.Importer.Complete + "%"
                });
                if (result.Importer.Errors.length) {
                    errorHtml = "";
                    for (i = 0; i < result.Importer.Errors.length; i++) {
                        errorHtml += '<p class="text-danger">' + result.Importer.Errors[i] + '</p>';
                    }
                    $('#import-property-errors').html(errorHtml);
                } else {
                    $('#import-property-errors').html("<p>No errors reported.</p>");
                }
                if (result.Importer.Warnings.length) {
                    warningHtml = "";
                    for (i = 0; i < result.Importer.Warnings.length; i++) {
                        warningHtml += '<p class="text-warning">' + result.Importer.Warnings[i] + '</p>';
                    }
                    $('#import-property-warnings').html(warningHtml);
                } else {
                    $('#import-property-warnings').html("<p>No warnings reported.</p>");
                }
            }
        });
    },
    View: {
        HideInfo: function () {
            $('#import-property-start').removeAttr('disabled');
            $('#import-property-cancel').attr('disabled', 'disabled');
            $('#import-property-progress').hide();
        },
        ShowInfo: function () {
            $('#import-property-cancel').removeAttr('disabled');
            $('#import-property-start').attr('disabled', 'disabled');
            $('#import-property-progress').show();
        },
        ShowError: function (string) {
            $('#import-property-error-message').html(string).addClass('alert').addClass('alert-danger').addClass('m-t-lg');
        }
    }
};
$(document).ready($.hood.Import.Reporter.Init);

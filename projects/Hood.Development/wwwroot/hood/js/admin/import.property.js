if (!$.hood)
    $.hood = {};
if (!$.hood.Import)
    $.hood.Import = {};
$.hood.Import.Property = {
    UpdateInterval: -1,
    Init: function() {
        if ($('#import-property-start').doesExist()) {
            $.hood.Import.Property.Update();
            $('#import-property-start').click(function() {
                $.ajax({
                    url: $('#import-property-start').data('url'),
                    type: "POST",
                    error: function(jqXHR, textStatus, errorThrown) {
                        $.hood.Import.Property.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                    },
                    success: function(result) {
                        $.hood.Import.Property.Update();
                    }
                });
            });
            $('#import-property-cancel').click(function() {
                $.ajax({
                    url: $('#import-property-cancel').data('url'),
                    type: "POST",
                    error: function(jqXHR, textStatus, errorThrown) {
                        $.hood.Import.Property.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                    },
                    success: function(result) {
                        $.hood.Import.Property.Update();
                    }
                });
            });
        }
    },
    Update: function() {
        $.ajax({
            url: $('#import-property-status').data('url'),
            type: "POST",
            error: function(jqXHR, textStatus, errorThrown) {
                $.hood.Import.Property.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
            },
            success: function(result) {
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
                    for (i = result.Importer.Errors.length - 1; i >= 0; i--) {
                        errorHtml += '<div class="text-danger">' + result.Importer.Errors[i] + '</div>';
                    }
                    $('#import-property-errors').html(errorHtml);
                } else {
                    $('#import-property-errors').html("<div>No errors reported.</div>");
                }
                if (result.Importer.Warnings.length) {
                    warningHtml = "";
                    for (i = result.Importer.Warnings.length - 1; i >= 0; i--) {
                        warningHtml += '<div class="text-warning">' + result.Importer.Warnings[i] + '</div>';
                    }
                    $('#import-property-warnings').html(warningHtml);
                } else {
                    $('#import-property-warnings').html("<div>No warnings reported.</div>");
                }
            }
        });
    },
    View: {
        HideInfo: function() {
            $('#import-property-start').removeAttr('disabled');
            $('#import-property-cancel').attr('disabled', 'disabled');
            $('#import-property-progress').removeClass('d-block');
            $('#import-property-progress').addClass('d-none');
        },
        ShowInfo: function() {
            $('#import-property-cancel').removeAttr('disabled');
            $('#import-property-start').attr('disabled', 'disabled');
            $('#import-property-progress').addClass('d-block');
            $('#import-property-progress').removeClass('d-none');
        },
        ShowError: function(string) {
            $('#import-property-error-message').html(string).addClass('alert').addClass('alert-danger').addClass('m-t-lg');
        }
    }
};
$(document).ready($.hood.Import.Property.Init);

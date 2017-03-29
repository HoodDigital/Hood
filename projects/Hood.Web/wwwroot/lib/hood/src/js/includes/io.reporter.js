if (!$.hood)
    $.hood = {};
$.hood.IO = {
    Reporter: {
        UpdateInterval: -1,
        Init: function () {
            $.hood.IO.Reporter.Update();
            $('#runUpdate').click(function () {
                $.ajax({
                    url: $('#runUpdate').data('start'),
                    type: "POST",
                    error: function (jqXHR, textStatus, errorThrown) {
                        $.hood.IO.Reporter.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                    },
                    success: function (result) {
                        $.hood.IO.Reporter.Update();
                    }
                });
            });
            $('#cancel').click(function () {
                $.ajax({
                    url: $('#runUpdate').data('cancel'),
                    type: "POST",
                    error: function (jqXHR, textStatus, errorThrown) {
                        $.hood.IO.Reporter.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                    },
                    success: function (result) {
                        $.hood.IO.Reporter.Update();
                    }
                });
            });
        },
        Update: function () {
            $.ajax({
                url: $('#runUpdate').data('update'),
                type: "POST",
                error: function (jqXHR, textStatus, errorThrown) {
                    $.hood.IO.Reporter.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
                },
                success: function (result) {
                    if (result.Running) {
                        $.hood.IO.Reporter.View.ShowInfo();
                        clearInterval($.hood.IO.Reporter.UpdateInterval);
                        $.hood.IO.Reporter.UpdateInterval = setTimeout($.hood.IO.Reporter.Update, 250);
                    } else {
                        clearInterval($.hood.IO.Reporter.UpdateInterval);
                        $.hood.IO.Reporter.View.HideInfo();
                    }
                    $('.tp').html(result.Total);
                    $('#pp').html(result.Processed);
                    $('#pt').html(result.Message);
                    $('.pc').html(Math.round(result.PercentComplete));
                    if (result.HasFile) {
                        $('#download').show();
                        $('#download-expire').html(result.ExpireTime);
                        $('#download-file').attr('href', result.Download);
                    } else {
                        $('#download').hide();
                    }
                    $('#progressbar').css({
                        width: result.PercentComplete + "%"
                    });
                }
            });
        },
        View: {
            HideInfo: function () {
                $('#runUpdate').removeAttr('disabled');
                $('#cancel').attr('disabled', 'disabled');
                $('#update-progress').hide();
            },
            ShowInfo: function () {
                $('#cancel').removeAttr('disabled');
                $('#runUpdate').attr('disabled', 'disabled');
                $('#update-progress').show();
            },
            ShowError: function (string) {
                $('#error').html(string).addClass('alert').addClass('alert-danger').addClass('m-t-lg');
            }
        }
    }
}
$.hood.IO.Reporter.Init();
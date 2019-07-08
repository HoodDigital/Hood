"use strict";

if (!$.hood) $.hood = {};
$.hood.IO = {
  Reporter: {
    UpdateInterval: -1,
    Init: function Init() {
      $.hood.IO.Reporter.Update();
      $('#import-property-start').click(function () {
        $.ajax({
          url: $('#import-property-start').data('start'),
          type: "POST",
          error: function error(jqXHR, textStatus, errorThrown) {
            $.hood.IO.Reporter.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
          },
          success: function success(result) {
            $.hood.IO.Reporter.Update();
          }
        });
      });
      $('#import-property-cancel').click(function () {
        $.ajax({
          url: $('#import-property-start').data('import-property-cancel'),
          type: "POST",
          error: function error(jqXHR, textStatus, errorThrown) {
            $.hood.IO.Reporter.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
          },
          success: function success(result) {
            $.hood.IO.Reporter.Update();
          }
        });
      });
    },
    Update: function Update() {
      $.ajax({
        url: $('#import-property-start').data('update'),
        type: "POST",
        error: function error(jqXHR, textStatus, errorThrown) {
          $.hood.IO.Reporter.View.ShowError("There was an error, " + jqXHR + "<br />" + textStatus + "<br />" + errorThrown);
        },
        success: function success(result) {
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
      HideInfo: function HideInfo() {
        $('#import-property-start').removeAttr('disabled');
        $('#import-property-cancel').attr('disabled', 'disabled');
        $('#import-property-progress').hide();
      },
      ShowInfo: function ShowInfo() {
        $('#import-property-cancel').removeAttr('disabled');
        $('#import-property-start').attr('disabled', 'disabled');
        $('#import-property-progress').show();
      },
      ShowError: function ShowError(string) {
        $('#import-property-error-message').html(string).addClass('alert').addClass('alert-danger').addClass('m-t-lg');
      }
    }
  }
};
$.hood.IO.Reporter.Init();
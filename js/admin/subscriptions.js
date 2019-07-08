"use strict";

if (!$.hood) $.hood = {};
$.hood.Subscriptions = {
  Init: function Init() {
    $('body').on('click', '.delete-subscription', this.Delete);
    $('body').on('click', '.create-subscription', this.Create.Init);
    if ($('#edit-subscription').doesExist()) this.Edit.Init();
  },
  Delete: function Delete(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The subscription will be permanently removed.",
      type: "warning",
      confirmButtonColor: "#DD6B55",
      confirmButtonText: "Yes, go ahead.",
      cancelButtonText: "No, cancel!",
      showCancelButton: true,
      closeOnConfirm: false,
      showLoaderOnConfirm: true,
      closeOnCancel: false
    }, function (isConfirm) {
      if (isConfirm) {
        // delete functionality
        $.post('/admin/subscriptions/delete', {
          id: $this.data('id')
        }, function (data) {
          if (data.Success) {
            if (!$('#manage-subscription-list').doesExist()) window.location = '/admin/subscriptions/';
            $.hood.Subscriptions.Manage.Refresh();
            $.hood.Blades.Close();
            swal({
              title: "Deleted!",
              text: "The subscription has now been removed from the website.",
              timer: 1300,
              type: "success"
            });
          } else {
            swal({
              title: "Error!",
              text: "There was a problem deleting the subscription: " + data.Errors,
              timer: 1300,
              type: "error"
            });
          }
        });
      } else {
        swal("Cancelled", "It's all good in the hood!", "error");
      }
    });
  },
  Create: {
    Init: function Init(e) {
      e.preventDefault();
      $.hood.Blades.OpenWithLoader('button.create-subscription', '/admin/subscriptions/create/', $.hood.Subscriptions.Create.SetupCreateForm);
    },
    SetupCreateForm: function SetupCreateForm() {
      $('#create-subscription-form').find('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
      $('#create-subscription-form').hoodValidator({
        validationRules: {
          Title: {
            required: true
          },
          Description: {
            required: true
          },
          Amount: {
            required: true
          },
          Currency: {
            required: true
          },
          Interval: {
            required: true
          },
          IntervalCount: {
            required: true
          },
          Name: {
            required: true
          }
        },
        submitButtonTag: $('#create-subscription-submit'),
        submitUrl: '/admin/subscriptions/add',
        submitFunction: function submitFunction(data) {
          if (data.Success) {
            swal("Created!", "The subscription has now been created!", "success");
            if (data.Url) window.location = data.Url;
          } else {
            swal("Error", "There was a problem creating the subscription:\n\n" + data.Errors, "error");
          }
        }
      });
    }
  },
  Edit: {
    Init: function Init() {
      this.LoadEditors('#edit-subscription');
      $.hood.Editor.Init('.edit-subscription-editor');
    },
    Blade: function Blade() {
      this.LoadEditors('#subscription-blade');
      $('#subscription-blade select').each($.hood.Handlers.SelectSetup);
      $('#subscription-blade-form').hoodValidator({
        validationRules: {
          Title: {
            required: true
          },
          Excerpt: {
            required: true
          }
        },
        submitButtonTag: $('#save-blade'),
        submitUrl: '/admin/subscriptions/save/' + $('#subscription-blade-form').data('id'),
        submitFunction: function submitFunction(data) {
          if (data.Succeeded) {
            $('#manage-subscription-list').data('hoodDataList').Refresh();
            $.hood.Alerts.Success("Updated.");
          } else {
            $.hood.Alerts.Error("There was an error saving.");
          }
        }
      });
    },
    LoadEditors: function LoadEditors(tag) {
      // Load the url thing if on page editor.
      $(tag).find('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
    }
  }
};
$.hood.Subscriptions.Init();
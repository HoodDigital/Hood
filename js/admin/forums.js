"use strict";

if (!$.hood) $.hood = {};
$.hood.Forum = {
  Init: function Init() {
    $('body').on('click', '.delete-forum', this.Delete);
    $('body').on('click', '.create-forum', this.Create.Init);
    $('body').on('click', '.publish-forum', this.Publish);
    $('body').on('click', '.archive-forum', this.Archive);
    $('body').on('click', '.create-forum', this.Create.Init);
    $('body').on('click', '.edit-forum-category', this.Categories.Edit);
    $('body').on('click', '.save-forum-category', this.Categories.Save);
    $('body').on('click', '.add-forum-category', this.Categories.Add);
    $('body').on('click', '.delete-forum-category', this.Categories.Delete);
    $('body').on('change', '.forum-category-check', this.Categories.ToggleCategory);
    $('body').on('keyup', '#Slug', function () {
      $('.slug-display').html($(this).val());
    });
    if ($('#edit-forum').doesExist()) this.Edit.Init();
  },
  Categories: {
    Edit: function Edit(e) {
      var $this = $(this);
      e.preventDefault();
      $.hood.Blades.OpenWithLoader('.edit-forum-category', '/admin/forums/categories/edit/' + $(this).data('id'), null);
    },
    Save: function Save(e) {
      $.post('/admin/forums/categories/save/', $('#edit-forum-category-form').serialize(), function (data) {
        if (data.Success) {
          $.hood.Inline.Reload('.categorylist');
          swal({
            title: "Saved!",
            text: "The category has been saved.",
            timer: 1300,
            type: "success"
          });
        } else {
          swal({
            title: "Error!",
            text: "There was a problem saving the category: " + data.Error,
            timer: 1300,
            type: "error"
          });
        }
      });
    },
    Add: function Add(e) {
      $.post('/admin/forums/categories/create/', $('#add-forum-category-form').serialize(), function (data) {
        if (data.Success) {
          $.hood.Inline.Reload('.categorylist');
          swal({
            title: "Saved!",
            text: "The category has been added.",
            timer: 1300,
            type: "success"
          });
        } else {
          swal({
            title: "Error!",
            text: "There was a problem adding the category: " + data.Error,
            timer: 1300,
            type: "error"
          });
        }
      });
    },
    ToggleCategory: function ToggleCategory() {
      if ($(this).is(':checked')) {
        $.post('/admin/forums/categories/add/', {
          categoryId: $(this).val(),
          forumId: $(this).data('id')
        }, function (data) {
          if (data.Success) {
            $.hood.Alerts.Success("Added category.");
          } else {
            $.hood.Alerts.Error("Couldn't add the category: " + data.Error);
          }
        });
      } else {
        $.post('/admin/forums/categories/remove/', {
          categoryId: $(this).val(),
          forumId: $(this).data('id')
        }, function (data) {
          if (data.Success) {
            $.hood.Alerts.Success("Removed category.");
          } else {
            $.hood.Alerts.Error("Couldn't add the category: " + data.Error);
          }
        });
      }
    },
    Delete: function Delete() {
      var $this = $(this);
      swal({
        title: "Are you sure?",
        text: "The field will be permanently removed.",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Yes, go ahead.",
        cancelButtonText: "No, cancel!",
        closeOnConfirm: false,
        showLoaderOnConfirm: true,
        closeOnCancel: false
      }, function (isConfirm) {
        if (isConfirm) {
          // delete functionality
          $.post('/admin/forums/categories/delete/' + $this.data('id'), null, function (data) {
            if (data.Success) {
              $.hood.Inline.Reload('.categorylist');
              swal({
                title: "Deleted!",
                text: "The field has now been removed from the website.",
                timer: 1300,
                type: "success"
              });
            } else {
              swal({
                title: "Error!",
                text: "There was a problem deleting the field: " + data.Errors,
                timer: 5000,
                type: "error"
              });
            }
          });
        } else {
          swal("Cancelled", "It's all good in the hood!", "error");
        }
      });
    }
  },
  Delete: function Delete(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The forum will be permanently removed.",
      type: "warning",
      showCancelButton: true,
      confirmButtonColor: "#DD6B55",
      confirmButtonText: "Yes, go ahead.",
      cancelButtonText: "No, cancel!",
      closeOnConfirm: false,
      showLoaderOnConfirm: true,
      closeOnCancel: false
    }, function (isConfirm) {
      if (isConfirm) {
        // delete functionality
        $.post('/admin/forums/delete/' + $this.data('id'), null, function (data) {
          if (data.Success) {
            $.hood.Blades.Close();
            swal({
              title: "Deleted!",
              text: "The forum has now been removed from the website.",
              timer: 1300,
              type: "success"
            });
            setTimeout(function () {
              window.location = data.Url;
            }, 500);
          } else {
            swal({
              title: "Error!",
              text: "There was a problem deleting the forum: " + data.Errors,
              timer: 5000,
              type: "error"
            });
          }
        });
      } else {
        swal("Cancelled", "It's all good in the hood!", "error");
      }
    });
  },
  Publish: function Publish(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The item will be visible on the website.",
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
        $.post('/admin/forums/publish/' + $this.data('id'), null, function (data) {
          if (data.Success) {
            swal({
              title: "Published!",
              text: "The item has now been published.",
              timer: 1300,
              type: "success"
            });
            setTimeout(function () {
              window.location = data.Url;
            }, 500);
          } else {
            swal({
              title: "Error!",
              text: "There was a problem publishing the item: " + data.Errors,
              timer: 5000,
              type: "error"
            });
          }
        });
      } else {
        swal("Cancelled", "It's all good in the hood!", "error");
      }
    });
  },
  Archive: function Archive(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The item will be hidden from the website.",
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
        $.post('/admin/forums/archive/' + $this.data('id'), null, function (data) {
          if (data.Success) {
            swal({
              title: "Archived!",
              text: "The item has now been archived.",
              timer: 1300,
              type: "success"
            });
            setTimeout(function () {
              window.location = data.Url;
            }, 500);
          } else {
            swal({
              title: "Error!",
              text: "There was a problem archiving the item: " + data.Errors,
              timer: 5000,
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
      var $this = $(this);
      e.preventDefault();
      $.hood.Blades.OpenWithLoader('button.create-forum', '/admin/forums/create/', $.hood.Forum.Create.SetupCreateForm);
    },
    SetupCreateForm: function SetupCreateForm() {
      $('#create-forum-form').find('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
      $('#create-forum-form').hoodValidator({
        validationRules: {
          Title: {
            required: true
          },
          Description: {
            required: true
          }
        },
        submitButtonTag: $('#create-forum-submit'),
        submitUrl: '/admin/forums/create',
        submitFunction: function submitFunction(data) {
          if (data.Success) {
            swal("Created!", "The forum has now been created!", "success");
            setTimeout(function () {
              window.location = data.Url;
            }, 500);
          } else {
            swal("Error", "There was a problem creating the forum:\n\n" + data.Errors, "error");
          }
        }
      });
    }
  },
  Edit: {
    Init: function Init() {
      $('#edit-forum-form').find('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
    }
  }
};
$(window).on('load', function () {
  $.hood.Forum.Init();
});
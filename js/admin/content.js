"use strict";

if (!$.hood) $.hood = {};
$.hood.Content = {
  Init: function Init() {
    $('body').on('click', '.content-delete', $.hood.Content.Delete);
    $('body').on('click', '.content-meta-delete', $.hood.Content.Meta.Delete);
    $('body').on('click', '.content-clone', $.hood.Content.Clone);
    $('body').on('click', '.content-set-status', $.hood.Content.SetStatus);
    $('body').on('click', '.content-media-delete', $.hood.Content.Media.Delete);
    $('body').on('click', '.content-categories-delete', $.hood.Content.Categories.Delete);
    $('body').on('change', '.content-categories-check', $.hood.Content.Categories.ToggleCategory);
    $('body').on('click', '.add-custom-field', $.hood.Content.Types.AddField);
    $('body').on('click', '.delete-custom-field', $.hood.Content.Types.DeleteField);
    $('body').on('keyup', '#Slug', function () {
      $('.slug-display').html($(this).val());
    });
    if ($('#content-edit-form').doesExist()) $.hood.Content.Edit.Init();
    if ($('#content-meta-form').doesExist()) $.hood.Content.Meta.Init();
  },
  Lists: {
    Content: {
      Loaded: function Loaded(data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#content-list').doesExist()) $.hood.Inline.Reload($('#content-list'), complete);
      }
    },
    Categories: {
      Loaded: function Loaded(data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#content-categories-list').doesExist()) $.hood.Inline.Reload($('#content-categories-list'), complete);
      }
    },
    Fields: {
      Loaded: function Loaded(data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#content-meta-list').doesExist()) $.hood.Inline.Reload($('#content-meta-list'), complete);
      }
    },
    Media: {
      Loaded: function Loaded(data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#content-media-list').doesExist()) $.hood.Inline.Reload($('#content-media-list'), complete);
      }
    }
  },
  Delete: function Delete(e) {
    e.preventDefault();
    var $tag = $(this);

    var deleteContentCallback = function deleteContentCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Content.Lists.Content.Reload();

          if (data.Success) {
            if ($tag && $tag.data('redirect')) {
              $.hood.Alerts.Success("<strong>Content deleted, redirecting...</strong><br />Just taking you back to the content list.");
              setTimeout(function () {
                window.location = $tag.data('redirect');
              }, 1500);
            }
          }
        });
      }
    };

    $.hood.Alerts.Confirm("The content will be permanently removed.", "Are you sure?", deleteContentCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
  },
  SetStatus: function SetStatus(e) {
    e.preventDefault();
    var $tag = $(this);

    var publishContentCallback = function publishContentCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), $tag.data('status'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Content.Lists.Content.Reload();
        });
      }
    };

    $.hood.Alerts.Confirm("The item will be immediately visible on the website.", "Are you sure?", publishContentCallback, 'warning');
  },
  Clone: function Clone(e) {
    e.preventDefault();
    var $tag = $(this);

    var duplicateContentCallback = function duplicateContentCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), $tag.data('status'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Content.Lists.Content.Reload();
        });
      }
    };

    $.hood.Alerts.Confirm("This will duplicate the content and everything inside it.", "Are you sure?", duplicateContentCallback, 'warning');
  },
  Create: function Create() {
    $('#content-create-form').find('.datepicker').datetimepicker({
      locale: 'en-gb',
      format: 'L'
    });
    $('#content-create-form').hoodValidator({
      validationRules: {
        Title: {
          required: true
        },
        Except: {
          required: true
        },
        PublishDate: {
          required: true,
          ukdate: true
        }
      },
      submitButtonTag: $('#content-create-submit'),
      submitUrl: $('#content-create-form').attr('action'),
      submitFunction: function submitFunction(data) {
        $.hood.Helpers.ProcessResponse(data);
        $.hood.Content.Lists.Content.Reload();
      }
    });
  },
  Edit: {
    Init: function Init() {
      $('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
      $('.datetimepicker').datetimepicker({
        locale: 'en-gb',
        format: 'LT'
      });
      $.hood.Content.Edit.InitImageUploader();
    },
    InitImageUploader: function InitImageUploader() {
      if (!$('#content-gallery-add').doesExist()) return;
      $('#content-gallery-total-progress').hide();
      Dropzone.autoDiscover = false;
      var myDropzone = new Dropzone("#content-gallery-upload", {
        url: $('#content-gallery-upload').data('url'),
        thumbnailWidth: 80,
        thumbnailHeight: 80,
        parallelUploads: 5,
        previewTemplate: false,
        paramName: 'files',
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: false,
        // Define the container to display the previews
        clickable: "#content-gallery-add",
        // Define the element that should be used as click trigger to select files.
        dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
        dictResponseError: 'Error while uploading file!'
      });
      myDropzone.on("success", function (file, data) {
        $.hood.Helpers.ProcessResponse(data);
        $.hood.Content.Lists.Media.Reload();
      });
      myDropzone.on("addedfile", function (file) {
        $('#content-gallery-total-progress .progress-bar').css({
          width: 0 + "%"
        });
        $('#content-gallery-total-progress .progress-bar .percentage').html(0 + "%");
      }); // Update the total progress bar

      myDropzone.on("totaluploadprogress", function (progress) {
        $('#content-gallery-total-progress .progress-bar').css({
          width: progress + "%"
        });
        $('#content-gallery-total-progress .progress-bar .percentage').html(progress + "%");
      });
      myDropzone.on("sending", function (file) {
        // Show the total progress bar when upload starts
        $('#content-gallery-total-progress').fadeIn();
        $('#content-gallery-total-progress .progress-bar').css({
          width: "0%"
        });
        $('#content-gallery-total-progress .progress-bar .percentage').html("0%");
      }); // Hide the total progress bar when nothing's uploading anymore

      myDropzone.on("complete", function (file) {
        $.hood.Content.Lists.Media.Reload();
      }); // Hide the total progress bar when nothing's uploading anymore

      myDropzone.on("queuecomplete", function (progress) {
        $('#content-gallery-total-progress').hide();
        $.hood.Content.Lists.Media.Reload();
      });
    }
  },
  Categories: {
    Editor: function Editor() {
      $('#content-categories-edit-form').hoodValidator({
        validationRules: {
          DisplayName: {
            required: true
          },
          Slug: {
            required: true
          }
        },
        submitButtonTag: $('#content-categories-edit-submit'),
        submitUrl: $('#content-categories-edit-form').attr('action'),
        submitFunction: function submitFunction(data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Content.Lists.Categories.Reload();
        }
      });
    },
    ToggleCategory: function ToggleCategory() {
      $.post($(this).data('url'), {
        categoryId: $(this).val(),
        add: $(this).is(':checked')
      }, function (data) {
        $.hood.Helpers.ProcessResponse(data);
        $.hood.Content.Lists.Categories.Reload();
      });
    },
    Delete: function Delete(e) {
      e.preventDefault();
      var $tag = $(this);

      var deleteCategoryCallback = function deleteCategoryCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Content.Lists.Categories.Reload();
          });
        }
      };

      $.hood.Alerts.Confirm("The category will be permanently removed.", "Are you sure?", deleteCategoryCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
    }
  },
  // Metadata
  Meta: {
    Create: function Create() {
      $('#add-field-form').hoodValidator({
        validationRules: {
          cfName: {
            required: true
          },
          cfType: {
            required: true
          }
        },
        submitButtonTag: $('#add-field-submit'),
        submitUrl: '/admin/content/addmeta',
        submitFunction: function submitFunction(data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Content.Lists.Fields.Reload();
        }
      });
    },
    Delete: function Delete(e) {
      e.preventDefault();
      var $tag = $(this);

      var deleteCategoryCallback = function deleteCategoryCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Content.Lists.Fields.Reload();
          });
        }
      };

      $.hood.Alerts.Confirm("The field will be permanently removed.", "Are you sure?", deleteCategoryCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
    }
  },
  // Media
  Media: {
    Delete: function Delete(e) {
      e.preventDefault();
      var $tag = $(this);

      var deleteMediaCallback = function deleteMediaCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Content.Lists.Media.Reload();
          });
        }
      };

      $.hood.Alerts.Confirm("The image/media will be permanently removed.", "Are you sure?", deleteMediaCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong><br /><span class="text-warning">If this is set as a featured image, this may cause issues, make sure to set another image as featured before deleting this one.</span></span>');
    }
  },
  // Content Types
  Types: {
    AddField: function AddField() {
      var name = $('#custom-field-name-' + $(this).data('id')).val();
      var fields = $.hood.Content.Types.GetFieldsList($(this).data('id'));
      var exists = false;
      $.each(fields, function (key, value) {
        if (value.Name === name) exists = true;
      });

      if (!name) {
        $.hood.Alerts.Error("You have to name the field.");
        return;
      }

      if (!fields) {
        $.hood.Alerts.Error("The field list is empty.");
        return;
      }

      if (exists) {
        $.hood.Alerts.Error("Cannot insert two fields with the same key.");
        return;
      } // Add the new item.


      var newField = {
        Name: $('#custom-field-name-' + $(this).data('id')).data('prefix') + $('#custom-field-name-' + $(this).data('id')).val(),
        Default: $('#custom-field-default-' + $(this).data('id')).val(),
        Type: $('#custom-field-type-' + $(this).data('id')).val(),
        System: false
      };
      fields.push(newField);
      $.hood.Content.Types.ReRenderFields(fields, $(this).data('id'));
      $('#custom-fields-' + $(this).data('id')).val(JSON.stringify(fields));
      $.hood.Alerts.Success("Added field.");
    },
    DeleteField: function DeleteField() {
      var fields = $.hood.Content.Types.GetFieldsList($(this).data('id'));
      var name = $(this).data('name');
      fields = $.grep(fields, function (e) {
        return e.Name !== name;
      });
      $.hood.Content.Types.ReRenderFields(fields, $(this).data('id'));
      $('#custom-fields-' + $(this).data('id')).val(JSON.stringify(fields));
      $.hood.Alerts.Success("Deleted field.");
    },
    GetFieldsList: function GetFieldsList(id) {
      // Take the contents of the fields input. 
      var fieldsInput = $('#custom-fields-' + id).val(); // if it is null, we need a new object.

      if (fieldsInput !== null && fieldsInput !== '') {
        var obj = JSON.parse(fieldsInput); // if not, we can deserialise to an array of FieldAreas

        for (var x in obj) {
          if (obj[x].hasOwnProperty('Name')) {
            return obj;
          }
        }
      } // if not, we can deserialise to an array of FieldAreas


      return new Array();
    },
    ReRenderFields: function ReRenderFields(arr, id) {
      var newList = $('#field-list-' + id).empty();

      for (i = 0; i < arr.length; i++) {
        var fld = "<tr><td class='col-xs-8'><strong>" + arr[i].Name + "</strong> " + arr[i].Type + "</td><td class='col-xs-4 text-right'>";

        if (!arr[i].System) {
          fld += "<a class='delete-custom-field btn btn-xs bg-color-red txt-color-white' data-name='" + arr[i].Name + "' data-id='" + id + "'><i class='fa fa-trash-o'></i></a>";
        } else {
          fld += '<span class="label label-default">System Field</span>';
        }

        fld += "</td></tr>";
        newList.append(fld);
      }
    }
  }
};
$(document).ready($.hood.Content.Init);
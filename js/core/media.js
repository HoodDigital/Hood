"use strict";

if (!$.hood) $.hood = {};
$.hood.Media = {
  Init: function Init() {
    $('body').on('click', '.delete-media', $.hood.Media.Delete);
    $('body').on('click', '.delete-directory', $.hood.Media.Directories.Delete);
    $('body').on('click', '.create-directory', $.hood.Media.Directories.Create);
    $.hood.Media.Upload.Init();
    $.hood.Media.Actions.Init();
  },
  Loaded: function Loaded(data) {
    $.hood.Loader(false);
  },
  Reload: function Reload(complete) {
    $.hood.Inline.Reload($('#media-list'), complete);
  },
  Actions: {
    Init: function Init() {
      // ATTACH FUNCTION - ATTACHES THE IMAGE TO A SPECIFIC ENTITY ATTACHABLE FIELD
      $('body').on('click', '.hood-image-attach', $.hood.Media.Actions.Load.Attach); // INSERT FUNCTION - INSERTS AN IMAGE TAG INTO THE CURRENTLY SELECTED EDITOR

      $('body').on('click', '.hood-image-insert', $.hood.Media.Actions.Load.Insert); // SET FUNCTION - SETS THE VALUE OF THE TAGGED INPUT

      $('body').on('click', '.hood-media-select', $.hood.Media.Actions.Load.Select); // SWITCH FUNCTION - REPLACES IMAGES IN THE DESIGNER

      $('body').on('click', '.hood-image-switch', $.hood.Media.Actions.Load.Switch);
    },
    Switching: null,
    Current: {
      Attach: null
    },
    Load: {
      Attach: function Attach(e) {
        $.hood.Media.Actions.Attacher = {
          Id: $(this).data('id'),
          Entity: $(this).data('entity'),
          Field: $(this).data('field'),
          Type: $(this).data('type'),
          Refresh: $(this).data('refresh'),
          Tag: $(this).data('tag'),
          Title: $(this).attr('title'),
          JsonField: $(this).data('json')
        };
        $.hood.Modals.Open('/admin/media/attach/', $.hood.Media.Actions.Attacher, '.hood-image-attach', function () {
          $.hood.Media.Reload(function () {
            $('body').off('click');
            $('body').on('click', '.attach-media-select', $.hood.Media.Actions.Complete.Attach);
          });
          $.hood.Media.Upload.Init();
        });
      },
      Insert: function Insert(editor) {
        editor.addButton('hoodimage', {
          text: 'Insert image...',
          icon: false,
          onclick: function onclick() {
            $.hood.Modals.Open('/admin/media/insert/', null, '.hood-image-attach', function () {
              $.hood.Media.Reload(function () {
                $('body').off('click');
                $('body').on('click', '.media-insert', $.proxy($.hood.Media.Actions.Complete.Insert, editor));
              });
              $.hood.Media.Upload.Init();
            });
          }
        });
      },
      Select: function Select(e) {
        $.hood.Media.Actions.Switching = $($(this).data('target'));
        $.hood.Modals.Open($(this).data('url'), null, '.hood-image-select', function () {
          $.hood.Media.Reload(function () {
            $('body').off('click', '.media-select');
            $('body').on('click', '.media-select', $.hood.Media.Actions.Complete.Select);
          });
          $.hood.Media.Upload.Init();
        });
      },
      Switch: function Switch(e) {
        $.hood.Media.Actions.Switching = $(this);
        $.hood.Modals.Open($(this).data('url'), null, '.hood-image-switch', function () {
          $.hood.Media.Reload(function () {
            $('body').off('click');
            $('body').on('click', '.media-select', $.hood.Media.Actions.Complete.Switch);
          });
          $.hood.Media.Upload.Init();
        });
      }
    },
    Complete: {
      Attach: function Attach(e) {
        $(this).data('temp', $(this).html());
        $(this).addClass('loading').append('<i class="fa fa-refresh fa-spin"></i>');
        var $image = $('.' + $.hood.Media.Actions.Attacher.Tag);
        params = {
          Id: $(this).data('id'),
          Entity: $(this).data('entity'),
          Field: $(this).data('field'),
          MediaId: $(this).data('media')
        };
        $.post('/admin/media/attach/', params, function (data) {
          if (data.Success) {
            $.hood.Alerts.Success("Attached!");
            $image.addClass('loading');
            icon = data.Media.Icon;

            if (data.Media.GeneralFileType === "Image") {
              icon = data.Media.MediumUrl;
            }

            $image.css({
              'background-image': 'url(' + icon + ')'
            });
            $image.find('img').attr('src', icon);
            $image.removeClass('loading');

            if (!$.hood.Helpers.IsNullOrUndefined($.hood.Media.Actions.Attacher.JsonField)) {
              $jsonField = $('#' + $.hood.Media.Actions.Attacher.JsonField);
              $jsonField.val(data.Json);
            }
          } else {
            $.hood.Alerts.Error(data.Errors, "Error attaching.");
          }
        }).done(function () {}).fail(function (data) {
          $.hood.Alerts.Error(data.status + " - " + data.statusText, "Error communicating.");
        }).always($.proxy(function (data) {
          $(this).removeClass('loading').html($(this).data('temp'));
          $.hood.Modals.Close('#attach-media-modal');
        }, this));
      },
      Insert: function Insert(e) {
        url = $(e.target).data('url');
        editor = this;
        editor.insertContent('<img alt="Your image..." src="' + url + '"/>');
        $.hood.Modals.Close('#attach-media-modal');
      },
      Select: function Select(e) {
        url = $(this).data('url');
        tag = $.hood.Media.Actions.Switching;
        $(tag).each(function () {
          if ($(this).is("input")) {
            $(this).val(url);
          } else {
            $(this).attr('src', url);
            $(this).css({
              'background-image': 'url(' + url + ')'
            });
            $(this).find('img').attr('src', url);
          }
        });
        $.hood.Alerts.Success("Image URL has been inserted.<br /><strong>Remember to press save!</strong>");
        $('#media-select-modal').modal('hide');
      },
      Switch: function Switch(e) {
        url = $(this).data('url');
        $tag = $.hood.Media.Actions.Switching;
        $tag.css({
          'background-image': 'url(' + url + ')'
        });
        $tag.find('img').attr('src', url);
        $.hood.Modals.Close('#attach-media-modal');
        $.hood.Alerts.Success("Attached!");
      }
    },
    RefreshImage: function RefreshImage(tag, url, id) {
      var $image = $(tag);
      $image.addClass('loading');
      $.get(url, {
        id: id
      }, $.proxy(function (data) {
        $image.css({
          'background-image': 'url(' + data.SmallUrl + ')'
        });
        $image.find('img').attr('src', data.SmallUrl);
        $image.removeClass('loading');
      }, this));
    }
  },
  Upload: {
    Init: function Init() {
      if (!$('#media-add').doesExist()) return;
      $('#media-total-progress').hide();
      Dropzone.autoDiscover = false;
      var myDropzone = new Dropzone("#media-upload", {
        url: $.hood.Media.Upload.UploadUrl,
        thumbnailWidth: 80,
        thumbnailHeight: 80,
        parallelUploads: 5,
        previewTemplate: false,
        paramName: 'files',
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: false,
        // Define the container to display the previews
        clickable: "#media-add",
        // Define the element that should be used as click trigger to select files.
        dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
        dictResponseError: 'Error while uploading file!'
      });
      myDropzone.on("success", function (file, response) {
        if (response.Success === false) {
          $.hood.Alerts.Error("Uploads failed: " + response.Message);
        } else {
          $.hood.Alerts.Success("Uploads completed successfully.");
        }
      });
      myDropzone.on("addedfile", function (file) {}); // Update the total progress bar

      myDropzone.on("totaluploadprogress", function (progress) {
        $('#media-total-progress .progress-bar').css({
          width: progress + "%"
        });
        $('#media-total-progress .progress-bar .percentage').html(progress + "%");
      });
      myDropzone.on("sending", function (file) {
        // Show the total progress bar when upload starts
        $('#media-total-progress').fadeIn();
        $('#media-total-progress .progress-bar').css({
          width: "0%"
        });
        $('#media-total-progress .progress-bar .percentage').html("0%");
      }); // Hide the total progress bar when nothing's uploading anymore

      myDropzone.on("complete", function (file) {
        $.hood.Media.Reload();
      }); // Hide the total progress bar when nothing's uploading anymore

      myDropzone.on("queuecomplete", function (progress) {
        $('#media-total-progress').hide();
        $.hood.Media.Reload();
      });
    },
    UploadUrl: function UploadUrl() {
      return "/admin/media/upload/simple?directory=" + $('#Directory').val();
    }
  },
  Delete: function Delete(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The media file will be permanently removed.\n\nWarning: Ensure this file is not attached to any posts, pages or features of the site, or it will appear as a broken image or file.",
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
        $.post('/admin/media/delete', {
          id: $this.data('id')
        }, function (data) {
          if (data.Success) {
            $.hood.Media.Reload();
            swal({
              title: "Deleted!",
              text: "The media file has now been removed from the website.",
              timer: 1300,
              type: "success"
            });
          } else {
            swal({
              title: "Error!",
              text: "There was a problem deleting the media file: " + data.Errors,
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
  RestrictDir: function RestrictDir() {
    var pattern = /[^0-9A-Za-z- ]*/g; // default pattern

    var val = $(this).val();
    var newVal = val.replace(pattern, ''); // This condition is to prevent selection and keyboard navigation issues

    if (val !== newVal) {
      $(this).val(newVal);
    }
  },
  Directories: {
    Create: function Create() {
      $('body').on('keyup', '.sweet-alert input', $.hood.Media.RestrictDir);
      Swal.fire({
        title: "Create directory",
        text: "Please enter a name for your new directory:",
        input: 'text',
        inputAttributes: {
          placeholder: "Directory name...",
          autocapitalize: 'off'
        },
        showCancelButton: true,
        confirmButtonText: 'Add Directory',
        showLoaderOnConfirm: true,
        preConfirm: function preConfirm(directory) {
          if (directory === false) return false;

          if (directory === "") {
            Swal.showValidationMessage("You didn't supply a directory name, we can't create one without it!");
            return false;
          }

          return fetch("/admin/media/directory/add?directory=".concat(directory)).then(function (response) {
            return response.json();
          })["catch"](function (error) {
            Swal.showValidationMessage("Request failed: ".concat(error));
          });
        },
        allowOutsideClick: function allowOutsideClick() {
          return !Swal.isLoading();
        }
      }).then(function (result) {
        if (result.value.Success) {
          $.hood.Media.Reload();
          Swal.fire({
            title: "Woohoo!",
            text: "Directory has been successfully added...",
            type: "success"
          });
        } else {
          Swal.showValidationMessage("There was a problem creating the new directory:\n\n".concat(result.value.Errors));
        }

        $('body').off('keyup', '.sweet-alert input', $.hood.Media.RestrictDir);
      });
    },
    Delete: function Delete(e) {
      var $this = $(this);
      message = "The directory and all files will be permanently removed.\n\nWarning: Ensure these files are not attached to any posts, pages or features of the site, or it will appear as a broken image or file.";

      if ($('#Directory').val() === "Default") {
        $.hood.Alerts.Error("You have to select a directory to delete.", "Error!", true, '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> You cannot delete the "Default" directory.</span>', true, 5000);
      } else {
        if ($('#Directory').val() === "") message = "You have selected to delete All directories, this will remove ALL files and ALL directories from the site. Are you sure!?";

        deleteDirectoryCallback = function deleteDirectoryCallback(result) {
          if (result.value) {
            $.post('/admin/media/directory/delete', {
              directory: $('#Directory').val()
            }, function (data) {
              if (data.Success) {
                $.hood.Media.Reload();
                $.hood.Alerts.Success("The directory has now been removed from the website.", "Deleted!", true, null, true, 5000);
              } else {
                $.hood.Alerts.Error("There was a problem deleting the directory.", "Error!", true, null, true, 5000);
              }
            });
          }
        };

        $.hood.Alerts.Confirm(message, "Are you sure?", deleteDirectoryCallback);
      }
    }
  },
  Players: {},
  LoadMediaPlayers: function LoadMediaPlayers(tag) {
    var videoOptions = {
      techOrder: ["azureHtml5JS", "flashSS", "html5FairPlayHLS", "silverlightSS", "html5"],
      nativeControlsForTouch: false,
      controls: true,
      autoplay: false,
      seeking: true
    };
    $(tag).each(function () {
      player = $.hood.Media.Players[$(this).data('id')];
      if (player) player.dispose();
      $.hood.Media.Players[$(this).data('id')] = amp($(this).attr('id'), videoOptions);
      player = $.hood.Media.Players[$(this).data('id')];
      player.src([{
        src: $(this).data('file'),
        type: $(this).data('type')
      }]);
    });
  }
};
$.hood.Media.Init();
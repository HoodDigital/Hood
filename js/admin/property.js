"use strict";

if (!$.hood) $.hood = {};
$.hood.Property = {
  Init: function Init() {
    $('body').on('click', '.delete-property', $.hood.Property.Delete);
    $('body').on('click', '.archive-property', $.hood.Property.Archive);
    $('body').on('click', '.publish-property', $.hood.Property.Publish);
    $('body').on('click', '.create-property', $.hood.Property.Create.Init);
    if ($('#edit-property').doesExist()) $.hood.Property.Edit.Init();
  },
  Delete: function Delete(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The property will be permanently removed.",
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
        $.post('/admin/property/delete', {
          id: $this.data('id')
        }, function (data) {
          if (data.Success) {
            if (!$('#manage-property-list').doesExist()) window.location = data.Url;
            $.hood.Property.Manage.Refresh();
            $.hood.Blades.Close();
            swal({
              title: "Deleted!",
              text: "The property has now been removed from the website.",
              timer: 1300,
              type: "success"
            });
          } else {
            swal({
              title: "Error!",
              text: "There was a problem deleting the property: " + data.Errors,
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
  Publish: function Publish(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The property will be visible on the website.",
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
        $.post('/admin/property/publish', {
          id: $this.data('id')
        }, function (data) {
          if (data.Success) {
            if (!$('#manage-property-list').doesExist()) window.location = data.Url;
            $.hood.Property.Manage.Refresh();
            $.hood.Blades.Close();
            swal({
              title: "Published!",
              text: "The property has now been published.",
              timer: 1300,
              type: "success"
            });
          } else {
            swal({
              title: "Error!",
              text: "There was a problem publishing the property: " + data.Errors,
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
  Archive: function Archive(e) {
    var $this = $(this);
    swal({
      title: "Are you sure?",
      text: "The property will be hidden from the website.",
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
        $.post('/admin/property/archive', {
          id: $this.data('id')
        }, function (data) {
          if (data.Success) {
            if (!$('#manage-property-list').doesExist()) window.location = data.Url;
            $.hood.Property.Manage.Refresh();
            $.hood.Blades.Close();
            swal({
              title: "Archived!",
              text: "The property has now been archived.",
              timer: 1300,
              type: "success"
            });
          } else {
            swal({
              title: "Error!",
              text: "There was a problem archiving the property: " + data.Errors,
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
      $.hood.Blades.OpenWithLoader('button.create-property', '/admin/property/create/', $.hood.Property.Create.SetupCreateForm);
    },
    SetupCreateForm: function SetupCreateForm() {
      $('#create-property-form').find('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
      $('#create-property-form').hoodValidator({
        validationRules: {
          cpTitle: {
            required: true
          },
          cpAddress1: {
            required: true
          },
          cpCity: {
            required: true
          },
          cpCounty: {
            required: true
          },
          cpCountry: {
            required: true
          },
          cpPostcode: {
            required: true
          },
          cpPublishDate: {
            required: true,
            ukdate: true
          }
        },
        submitButtonTag: $('#create-property-submit'),
        submitUrl: '/admin/property/add',
        submitFunction: function submitFunction(data) {
          if (data.Success) {
            swal("Created!", "The property has now been created!", "success");
            setTimeout(function () {
              window.location = data.Url;
            }, 500);
          } else {
            swal("Error", "There was a problem creating the content:\n\n" + data.Errors, "error");
          }
        }
      });
      $.hood.Google.Addresses.InitAutocomplete();
    }
  },
  Edit: {
    Init: function Init() {
      this.LoadEditors('#edit-property');
      $.hood.Property.Upload.InitImageUploader();
      $.hood.Property.Upload.InitFloorplanUploader();
      $('body').on('click', '.add-floor', this.AddFloor);
      $('body').on('click', '.delete-floor', this.DeleteFloor);
      $('body').on('change', '.recalc-floor', this.RecalcFloor);
    },
    AddFloor: function AddFloor() {
      var number = $('#Floor-Number').val();
      var floors = $.hood.Property.Edit.GetFloorsList();
      exists = false;
      $.each(floors, function (key, value) {
        if (value.Number == number) exists = true;
      });

      if (exists) {
        $.hood.Alerts.Error("Cannot insert two floors with the same number.");
        return;
      } // Add the new item.


      newFloor = {
        Name: $('#Floor-Name').val(),
        SquareFeet: $('#Floor-SquareFeet').val(),
        SquareMetres: $('#Floor-SquareMetres').val(),
        Number: $('#Floor-Number').val()
      };
      floors.push(newFloor);
      $.hood.Property.Edit.ReRenderFloors(floors);
      $('#Floors').val(JSON.stringify(floors));
    },
    DeleteFloor: function DeleteFloor() {
      var floors = $.hood.Property.Edit.GetFloorsList();
      var number = $(this).data('number');
      floors = $.grep(floors, function (e) {
        return e.Number != number;
      });
      $.hood.Property.Edit.ReRenderFloors(floors);
      $('#Floors').val(JSON.stringify(floors));
    },
    RecalcFloor: function RecalcFloor(e) {
      if (this.id == "Floor-SquareMetres") $('#Floor-SquareFeet').val(Number($(this).val()) * 10.7639);else {
        $('#Floor-SquareMetres').val(Number($(this).val()) / 10.7639);
      }
    },
    GetFloorsList: function GetFloorsList() {
      // Take the contents of the floors input. 
      floorsInput = $('#Floors').val(); // if it is null, we need a new object.

      if (floorsInput != null && floorsInput != '') {
        var obj = JSON.parse(floorsInput); // if not, we can deserialise to an array of FloorAreas

        for (var x in obj) {
          if (obj[x].hasOwnProperty('Name')) {
            return obj;
          }
        }
      } // if not, we can deserialise to an array of FloorAreas


      return new Array();
    },
    ReRenderFloors: function ReRenderFloors(arr) {
      arr.sort(function (a, b) {
        var keyA = Number(a.Number),
            keyB = Number(b.Number); // Compare the 2

        if (keyA < keyB) return -1;
        if (keyA > keyB) return 1;
        return 0;
      });
      newList = $('.floor-list').empty();

      for (i = 0; i < arr.length; i++) {
        newList.append("<div class='row m-b-xs'><div class='col-xs-4'><strong>" + arr[i].Name + "</strong> " + arr[i].Number + "</div><div class='col-xs-8'>" + $.numberWithCommas(Math.round(arr[i].SquareMetres)) + " m<sup>2</sup> [" + $.numberWithCommas(Math.round(arr[i].SquareFeet)) + " sq. ft.] <a class='delete-floor btn btn-xs bg-color-red txt-color-white' data-number='" + arr[i].Number + "'><i class='fa fa-trash-o'></i></a></div></div>");
      }
    },
    Blade: function Blade() {
      this.LoadEditors('#property-blade');
      $('#property-blade select').each($.hood.Handlers.SelectSetup);
      $('#property-blade-form').hoodValidator({
        validationRules: {
          Title: {
            required: true
          },
          Excerpt: {
            required: true
          }
        },
        submitButtonTag: $('#save-blade'),
        submitUrl: '/admin/property/save/' + $('#property-blade-form').data('id'),
        submitFunction: function submitFunction(data) {
          if (data.Succeeded) {
            $('#manage-property-list').data('hoodDataList').Refresh();
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
  },
  Upload: {
    InitImageUploader: function InitImageUploader() {
      Dropzone.autoDiscover = false;
      var pgDropzone = new Dropzone("#property-gallery-upload", {
        url: "/admin/property/upload/gallery?id=" + $("#property-gallery-upload").data('id'),
        thumbnailWidth: 80,
        thumbnailHeight: 80,
        parallelUploads: 5,
        previewTemplate: false,
        paramName: 'files',
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: false,
        // Define the container to display the previews
        clickable: "#property-gallery-add",
        // Define the element that should be used as click trigger to select files.
        dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
        dictResponseError: 'Error while uploading file!'
      });
      pgDropzone.on("success", function (file, response) {
        if (response.Success == false) {
          $.hood.Alerts.Error("Uploads failed: " + response.Error);
        } else {
          $.hood.Alerts.Success("Uploads completed successfully.");
        }
      }); // Update the total progress bar

      pgDropzone.on("totaluploadprogress", function (progress) {
        document.querySelector("#gallery-total-progress .progress-bar").style.width = progress + "%";
      });
      pgDropzone.on("sending", function (file) {
        // Show the total progress bar when upload starts
        document.querySelector("#gallery-total-progress").style.opacity = "1";
      }); // Hide the total progress bar when nothing's uploading anymore

      pgDropzone.on("complete", function (file) {
        $.hood.Inline.Refresh('.gallery');
      }); // Hide the total progress bar when nothing's uploading anymore

      pgDropzone.on("queuecomplete", function (progress) {
        document.querySelector("#gallery-total-progress").style.opacity = "0";
        $.hood.Inline.Refresh('.gallery');
      });
    },
    InitFloorplanUploader: function InitFloorplanUploader() {
      Dropzone.autoDiscover = false;
      var fpDropzone = new Dropzone("#property-floorplans-upload", {
        url: "/admin/property/upload/floorplan?id=" + $("#property-floorplans-upload").data('id'),
        thumbnailWidth: 80,
        thumbnailHeight: 80,
        parallelUploads: 5,
        previewTemplate: false,
        paramName: 'files',
        autoProcessQueue: true,
        // Make sure the files aren't queued until manually added
        previewsContainer: false,
        // Define the container to display the previews
        clickable: "#property-floorplans-add",
        // Define the element that should be used as click trigger to select files.
        dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
        dictResponseError: 'Error while uploading file!'
      });
      fpDropzone.on("success", function (file, response) {
        if (response.Success == false) {
          $.hood.Alerts.Error("Uploads failed: " + response.Error);
        } else {
          $.hood.Alerts.Success("Uploads completed successfully.");
        }
      }); // Update the total progress bar

      fpDropzone.on("totaluploadprogress", function (progress) {
        document.querySelector("#floorplans-total-progress .progress-bar").style.width = progress + "%";
      });
      fpDropzone.on("sending", function (file) {
        // Show the total progress bar when upload starts
        document.querySelector("#floorplans-total-progress").style.opacity = "1";
      }); // Hide the total progress bar when nothing's uploading anymore

      fpDropzone.on("complete", function (file) {
        $.hood.Inline.Refresh('.gallery');
      }); // Hide the total progress bar when nothing's uploading anymore

      fpDropzone.on("queuecomplete", function (progress) {
        document.querySelector("#floorplans-total-progress").style.opacity = "0";
        $.hood.Inline.Refresh('.floorplans');
        $.hood.Alerts.Success("Uploads completed successfully.");
      });
    }
  }
};
$(document).ready($.hood.Property.Init);
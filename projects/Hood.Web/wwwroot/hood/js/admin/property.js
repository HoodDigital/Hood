if (!$.hood)
    $.hood = {};
$.hood.Property = {
    Init: function () {
        $('body').on('click', '.property-delete', $.hood.Property.Delete);
        $('body').on('click', '.property-set-status', $.hood.Property.SetStatus);

        if ($('#edit-property').doesExist())
            $.hood.Property.Edit.Init();
    },

    Lists: {
        Property: {
            Loaded: function (data) {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#property-list').doesExist())
                    $.hood.Inline.Reload($('#property-list'), complete);
            }
        },
        Media: {
            Loaded: function (data) {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#property-media-list').doesExist())
                    $.hood.Inline.Reload($('#property-media-list'), complete);
            }
        },
        Floorplans: {
            Loaded: function (data) {
                $.hood.Loader(false);
            },
            Reload: function (complete) {
                if ($('#property-floorplan-list').doesExist())
                    $.hood.Inline.Reload($('#property-floorplan-list'), complete);
            }
        }
    },

    Delete: function (e) {
        e.preventDefault();
        $tag = $(this);

        deletePropertyCallback = function (isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Property.Lists.Property.Reload();
                    if (data.Success) {
                        if ($tag && $tag.data('redirect')) {
                            $.hood.Alerts.Success(`<strong>Property deleted, redirecting...</strong><br />Just taking you back to the property list.`);
                            setTimeout(function () {
                                window.location = $tag.data('redirect');
                            }, 1500);
                        }
                    }
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The property will be permanently removed.",
            "Are you sure?",
            deletePropertyCallback,
            'error',
            '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
        );
    },

    SetStatus: function (e) {
        e.preventDefault();
        $tag = $(this);

        publishPropertyCallback = function (isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), $tag.data('status'), function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Property.Lists.Property.Reload();
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The item will be immediately visible on the website.",
            "Are you sure?",
            publishPropertyCallback,
            'warning'
        );
    },

    Create: function () {
        $('#property-create-form').find('.datepicker').datetimepicker({
            locale: 'en-gb',
            format: 'L'
        });
        $('#property-create-form').hoodValidator({
            validationRules: {
                Title: {
                    required: true
                },
                Address1: {
                    required: true
                },
                City: {
                    required: true
                },
                County: {
                    required: true
                },
                Country: {
                    required: true
                },
                Postcode: {
                    required: true
                },
                PublishDate: {
                    required: true,
                    ukdate: true
                }
            },
            submitButtonTag: $('#property-create-submit'),
            submitUrl: $('#property-create-form').attr('action'),
            submitFunction: function (data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Property.Lists.Property.Reload();
            }
        });
        $.hood.Google.Addresses.InitAutocomplete();
    },

    Edit: {
        Init: function () {
            $('.datepicker').datetimepicker({
                locale: 'en-gb',
                format: 'L'
            });

            $.hood.Property.Upload.InitImageUploader();
            $.hood.Property.Upload.InitFloorplanUploader();

            $('body').on('click', '.add-floor', $.hood.Property.Edit.AddFloor);
            $('body').on('click', '.delete-floor', $.hood.Property.Edit.DeleteFloor);
            $('body').on('change', '.recalc-floor', $.hood.Property.Edit.RecalcFloor);
        },

        AddFloor: function () {
            var number = $('#Floor-Number').val();
            var floors = $.hood.Property.Edit.GetFloorsList();
            exists = false;
            $.each(floors, function (key, value) {
                if (value.Number == number)
                    exists = true;
            });
            if (exists) {
                $.hood.Alerts.Error("Cannot insert two floors with the same number.");
                return;
            }
            // Add the new item.
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
        DeleteFloor: function () {
            var floors = $.hood.Property.Edit.GetFloorsList();
            var number = $(this).data('number');
            floors = $.grep(floors, function (e) {
                return e.Number != number;
            });
            $.hood.Property.Edit.ReRenderFloors(floors);
            $('#Floors').val(JSON.stringify(floors));
        },
        RecalcFloor: function (e) {
            if (this.id == "Floor-SquareMetres")
                $('#Floor-SquareFeet').val(Number($(this).val()) * 10.7639);
            else {
                $('#Floor-SquareMetres').val(Number($(this).val()) / 10.7639);
            }
        },
        GetFloorsList: function () {
            // Take the contents of the floors input. 
            floorsInput = $('#Floors').val();
            // if it is null, we need a new object.
            if (floorsInput != null && floorsInput != '') {
                var obj = JSON.parse(floorsInput);
                // if not, we can deserialise to an array of FloorAreas
                for (var x in obj) {
                    if (obj[x].hasOwnProperty('Name')) {
                        return obj
                    }
                }
            }
            // if not, we can deserialise to an array of FloorAreas
            return new Array();
        },
        ReRenderFloors: function (arr) {
            arr.sort(function (a, b) {
                var keyA = Number(a.Number),
                    keyB = Number(b.Number);
                // Compare the 2
                if (keyA < keyB) return -1;
                if (keyA > keyB) return 1;
                return 0;
            });
            newList = $('.floor-list').empty();
            for (i = 0; i < arr.length; i++) {
                newList.append("<div class='row m-b-xs'><div class='col-xs-4'><strong>" + arr[i].Name + "</strong> " + arr[i].Number + "</div><div class='col-xs-8'>" + $.numberWithCommas(Math.round(arr[i].SquareMetres)) + " m<sup>2</sup> [" + $.numberWithCommas(Math.round(arr[i].SquareFeet)) + " sq. ft.] <a class='delete-floor btn btn-xs bg-color-red txt-color-white' data-number='" + arr[i].Number + "'><i class='fa fa-trash-o'></i></a></div></div>");
            }
        }
    },

    Upload: {
        InitImageUploader: function () {

            Dropzone.autoDiscover = false;

            var pgDropzone = new Dropzone("#property-gallery-upload", {
                url: "/admin/property/upload/gallery?id=" + $("#property-gallery-upload").data('id'),
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                previewTemplate: false,
                paramName: 'files',
                autoProcessQueue: true, // Make sure the files aren't queued until manually added
                previewsContainer: false, // Define the container to display the previews
                clickable: "#property-gallery-add", // Define the element that should be used as click trigger to select files.
                dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
                dictResponseError: 'Error while uploading file!'
            });

            pgDropzone.on("success", function (file, response) {
                if (response.Success == false) {
                    $.hood.Alerts.Error("Uploads failed: " + response.Error);
                } else {
                    $.hood.Alerts.Success("Uploads completed successfully.");
                }
            });

            // Update the total progress bar
            pgDropzone.on("totaluploadprogress", function (progress) {
                document.querySelector("#gallery-total-progress .progress-bar").style.width = progress + "%";
            });

            pgDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                document.querySelector("#gallery-total-progress").style.opacity = "1";
            });

            // Hide the total progress bar when nothing's uploading anymore
            pgDropzone.on("complete", function (file) {
                $.hood.Inline.Refresh('.gallery');
            });

            // Hide the total progress bar when nothing's uploading anymore
            pgDropzone.on("queuecomplete", function (progress) {
                document.querySelector("#gallery-total-progress").style.opacity = "0";
                $.hood.Inline.Refresh('.gallery');
            });
        },
        InitFloorplanUploader: function () {

            Dropzone.autoDiscover = false;

            var fpDropzone = new Dropzone("#property-floorplans-upload", {
                url: "/admin/property/upload/floorplan?id=" + $("#property-floorplans-upload").data('id'),
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                previewTemplate: false,
                paramName: 'files',
                autoProcessQueue: true, // Make sure the files aren't queued until manually added
                previewsContainer: false, // Define the container to display the previews
                clickable: "#property-floorplans-add", // Define the element that should be used as click trigger to select files.
                dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
                dictResponseError: 'Error while uploading file!'
            });

            fpDropzone.on("success", function (file, response) {
                if (response.Success == false) {
                    $.hood.Alerts.Error("Uploads failed: " + response.Error);
                } else {
                    $.hood.Alerts.Success("Uploads completed successfully.");
                }
            });

            // Update the total progress bar
            fpDropzone.on("totaluploadprogress", function (progress) {
                document.querySelector("#floorplans-total-progress .progress-bar").style.width = progress + "%";
            });

            fpDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                document.querySelector("#floorplans-total-progress").style.opacity = "1";
            });

            // Hide the total progress bar when nothing's uploading anymore
            fpDropzone.on("complete", function (file) {
                $.hood.Inline.Refresh('.gallery');
            });

            // Hide the total progress bar when nothing's uploading anymore
            fpDropzone.on("queuecomplete", function (progress) {
                document.querySelector("#floorplans-total-progress").style.opacity = "0";
                $.hood.Inline.Refresh('.floorplans');
                $.hood.Alerts.Success("Uploads completed successfully.");
            });
        }
    }
};
$(document).ready($.hood.Property.Init);


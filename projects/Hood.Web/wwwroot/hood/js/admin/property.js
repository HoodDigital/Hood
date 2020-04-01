if (!$.hood)
    $.hood = {};
$.hood.Property = {
    Init: function() {
        $('body').on('click', '.property-delete', $.hood.Property.Delete);
        $('body').on('click', '.property-delete-all', $.hood.Property.DeleteAll);
        $('body').on('click', '.property-delete-floor', $.hood.Property.DeleteFloorArea);
        $('body').on('click', '.property-set-status', $.hood.Property.SetStatus);

        $('body').on('click', '.property-media-delete', $.hood.Property.Media.Delete);

        if ($('#property-edit-form').doesExist())
            $.hood.Property.Edit.Init();
    },

    Lists: {
        Property: {
            Loaded: function(sender, data) {
                $.hood.Loader(false);
                $.hood.Google.ClusteredMap();
            },
            Reload: function(complete) {
                if ($('#property-list').doesExist())
                    $.hood.Inline.Reload($('#property-list'), complete);
            }
        },
        Media: {
            Loaded: function(sender, data) {
                $.hood.Loader(false);
            },
            Reload: function(complete) {
                if ($('#property-gallery-list').doesExist())
                    $.hood.Inline.Reload($('#property-gallery-list'), complete);
            }
        },
        Floorplans: {
            Loaded: function(sender, data) {
                $.hood.Loader(false);
            },
            Reload: function(complete) {
                if ($('#property-floorplans-list').doesExist())
                    $.hood.Inline.Reload($('#property-floorplans-list'), complete);
            }
        },
        Floorareas: {
            Loaded: function(sender, data) {
                $.hood.Loader(false);
            },
            Reload: function(complete) {
                if ($('#property-floors-list').doesExist())
                    $.hood.Inline.Reload($('#property-floors-list'), complete);
            }
        }
    },

    Delete: function(e) {
        e.preventDefault();
        let $tag = $(this);

        let deletePropertyCallback = function(isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function(data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Property.Lists.Property.Reload();
                    if (data.Success) {
                        if ($tag && $tag.data('redirect')) {
                            $.hood.Alerts.Success(`<strong>Property deleted, redirecting...</strong><br />Just taking you back to the property list.`);
                            setTimeout(function() {
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

    DeleteAll: function(e) {
        e.preventDefault();
        let $tag = $(this);

        Swal.fire({
            title: "Are you sure?",
            html: "ALL of the properties will be permanently removed. Type 'DELETE' to continue.",
            input: 'text',
            inputAttributes: {
                autocapitalize: 'off'
            },
            footer: '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This can take a few minutes to complete and CANNOT be undone.</strong></span>',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            showLoaderOnConfirm: true,
            preConfirm: (login) => {
                if (login.toLowerCase() === "delete") {
                    var url = $tag.attr('href');
                    $.post($tag.attr('href'), function(data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Property.Lists.Property.Reload();
                        if (data.Success) {
                            if ($tag && $tag.data('redirect')) {
                                $.hood.Alerts.Success(`<strong>Properties deleted, redirecting...</strong><br />Just taking you back to the property list.`);
                                setTimeout(function() {
                                    window.location = $tag.data('redirect');
                                }, 1500);
                            }
                        }
                        swal.close();
                    })
                        .fail($.hood.Inline.HandleError)
                        .always($.hood.Inline.Finish);
                } else {
                    Swal.showValidationMessage(
                        `You did not type DELETE.`
                    );
                    return false;
                }
            },
            allowOutsideClick: () => !Swal.isLoading()
        });
    },

    SetStatus: function(e) {
        e.preventDefault();
        let $tag = $(this);

        let publishPropertyCallback = function(isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), $tag.data('status'), function(data) {
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

    Create: function() {
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
            submitFunction: function(data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Property.Lists.Property.Reload();
            }
        });
        $.hood.Google.Addresses.InitAutocomplete();
    },

    CreateFloorArea: function() {
        $('#property-floorArea-create-form').hoodValidator({
            validationRules: {
                Name: {
                    required: true
                },
                Number: {
                    required: true
                },
                SquareFeet: {
                    required: true
                },
                SquareMetres: {
                    required: true
                }
            },
            submitButtonTag: $('#property-floorArea-create-submit'),
            submitUrl: $('#property-floorArea-create-form').attr('action'),
            submitFunction: function(data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Property.Lists.Floorareas.Reload();
            }
        });
        $('body').on('change', '.recalc-floor', $.hood.Property.RecalcFloor);
    },
    RecalcFloor: function(e) {
        if (this.id === "SquareMetres")
            $('#property-floorArea-create-form #SquareFeet').val(Number($(this).val()) * 10.7639);
        else {
            $('#property-floorArea-create-form #SquareMetres').val(Number($(this).val()) / 10.7639);
        }
    },
    DeleteFloorArea: function(e) {
        e.preventDefault();
        let $tag = $(this);

        let deleteFloorAreaCallback = function(isConfirm) {
            if (isConfirm) {
                $.post($tag.attr('href'), function(data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Property.Lists.Floorareas.Reload();
                });
            }
        };

        $.hood.Alerts.Confirm(
            "The floor will be permanently removed.",
            "Are you sure?",
            deleteFloorAreaCallback,
            'error',
            '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
        );
    },

    Edit: {
        Init: function() {
            $('.datepicker').datetimepicker({
                locale: 'en-gb',
                format: 'L'
            });

            $.hood.Property.Upload.InitImageUploader();
            $.hood.Property.Upload.InitFloorplanUploader();
        }
    },

    Upload: {
        InitImageUploader: function() {
            if (!$('#property-gallery-add').doesExist())
                return;

            $('#property-gallery-total-progress').hide();

            Dropzone.autoDiscover = false;

            var myDropzone = new Dropzone("#property-gallery-upload", {
                url: $('#property-gallery-upload').data('url'),
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

            myDropzone.on("success", function(file, data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Property.Lists.Media.Reload();
            });

            myDropzone.on("addedfile", function(file) {
                $('#property-gallery-total-progress .progress-bar').css({ width: 0 + "%" });
                $('#property-gallery-total-progress .progress-bar .percentage').html(0 + "%");
            });

            // Update the total progress bar
            myDropzone.on("totaluploadprogress", function(progress) {
                $('#property-gallery-total-progress .progress-bar').css({ width: progress + "%" });
                $('#property-gallery-total-progress .progress-bar .percentage').html(progress + "%");
            });

            myDropzone.on("sending", function(file) {
                // Show the total progress bar when upload starts
                $('#property-gallery-total-progress').fadeIn();
                $('#property-gallery-total-progress .progress-bar').css({ width: "0%" });
                $('#property-gallery-total-progress .progress-bar .percentage').html("0%");
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("complete", function(file) {
                $.hood.Property.Lists.Media.Reload();
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("queuecomplete", function(progress) {
                $('#property-gallery-total-progress').hide();
                $.hood.Property.Lists.Media.Reload();
            });
        },
        InitFloorplanUploader: function() {
            if (!$('#property-floorplans-add').doesExist())
                return;

            $('#property-floorplans-total-progress').hide();

            Dropzone.autoDiscover = false;

            var myDropzone = new Dropzone("#property-floorplans-upload", {
                url: $('#property-floorplans-upload').data('url'),
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

            myDropzone.on("success", function(file, data) {
                $.hood.Helpers.ProcessResponse(data);
                $.hood.Property.Lists.Floorplans.Reload();
            });

            myDropzone.on("addedfile", function(file) {
                $('#property-floorplans-total-progress .progress-bar').css({ width: 0 + "%" });
                $('#property-floorplans-total-progress .progress-bar .percentage').html(0 + "%");
            });

            // Update the total progress bar
            myDropzone.on("totaluploadprogress", function(progress) {
                $('#property-floorplans-total-progress .progress-bar').css({ width: progress + "%" });
                $('#property-floorplans-total-progress .progress-bar .percentage').html(progress + "%");
            });

            myDropzone.on("sending", function(file) {
                // Show the total progress bar when upload starts
                $('#property-floorplans-total-progress').fadeIn();
                $('#property-floorplans-total-progress .progress-bar').css({ width: "0%" });
                $('#property-floorplans-total-progress .progress-bar .percentage').html("0%");
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("complete", function(file) {
                $.hood.Property.Lists.Floorplans.Reload();
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("queuecomplete", function(progress) {
                $('#property-floorplans-total-progress').hide();
                $.hood.Property.Lists.Floorplans.Reload();
            });
        }
    },

    Media: {
        Delete: function(e) {
            e.preventDefault();
            let $tag = $(this);

            let deleteMediaCallback = function(isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function(data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Property.Lists.Media.Reload();
                        $.hood.Property.Lists.Floorplans.Reload();
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The image/media will be permanently removed.",
                "Are you sure?",
                deleteMediaCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong><br /><span class="text-warning">If this is set as a featured image, this may cause issues, make sure to set another image as featured before deleting this one.</span></span>',
            );

        }
    }

};
$(document).ready($.hood.Property.Init);


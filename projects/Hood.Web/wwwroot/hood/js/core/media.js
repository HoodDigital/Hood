if (!$.hood)
    $.hood = {};
$.hood.Media = {
    Init: function () {
        $('body').on('click', '.media-delete', $.hood.Media.Delete);
        $('body').on('click', '.media-directories-delete', $.hood.Media.Directories.Delete);
        $.hood.Media.Upload.Init();
        $.hood.Media.Actions.Init();
    },

    Loaded: function (data) {
        $.hood.Loader(false);
    },
    BladeLoaded: function (data) {
        $.hood.Media.LoadMediaPlayers();
    },
    Reload: function (complete) {
        $.hood.Inline.Reload($('#media-list'), complete);
    },
    ReloadDirectories: function (complete) {
        $.hood.Inline.Reload($('#media-directories-list'), complete);
    },

    Actions: {
        Init: function () {
            // ATTACH FUNCTION - ATTACHES THE IMAGE TO A SPECIFIC ENTITY ATTACHABLE FIELD
            $('body').on('click', '.hood-image-attach', $.hood.Media.Actions.Load.Attach);
            $('body').on('click', '.hood-image-clear', $.hood.Media.Actions.Complete.Clear);

            // INSERT FUNCTION - INSERTS AN IMAGE TAG INTO THE CURRENTLY SELECTED EDITOR
            $('body').on('click', '.hood-image-insert', $.hood.Media.Actions.Load.Insert);

            // SELECT FUNCTION - INSERTS THE SELECTED URL INTO TEXTBOX ATTACHED TO SELECTOR
            $('body').on('click', '.hood-media-select', $.hood.Media.Actions.Load.Select);
        },
        Target: null,
        Json: null,
        Current: {
            Attach: null
        },
        Load: {
            Attach: function (e) {
                e.preventDefault();
                $.hood.Media.Actions.Target = $($(this).data('tag'));
                $.hood.Media.Actions.Json = $($(this).data('json'));
                $.hood.Inline.Modal($(this).data('url'), function () {
                    $.hood.Media.Reload(function () {
                        $('body').off('click', '.media-attach');
                        $('body').on('click', '.media-attach', $.hood.Media.Actions.Complete.Attach);
                    });
                    $.hood.Media.Upload.Init();
                });
            },
            Insert: function (editor) {
                editor.addButton('hoodimage', {
                    text: 'Insert image...',
                    icon: false,
                    onclick: function () {
                        $.hood.Inline.Modal($(this).data('url'), function () {
                            $.hood.Media.Reload(function () {
                                $('body').off('click');
                                $('body').on('click', '.media-insert', $.proxy($.hood.Media.Actions.Complete.Insert, editor));
                            });
                            $.hood.Media.Upload.Init();
                        });
                    }
                });
            },
            Select: function (e) {
                $.hood.Media.Actions.Target = $($(this).data('target'));
                $.hood.Inline.Modal($(this).data('url'), function () {
                    $.hood.Media.Reload(function () {
                        $('body').off('click', '.media-select');
                        $('body').on('click', '.media-select', $.hood.Media.Actions.Complete.Select);
                    });
                    $.hood.Media.Upload.Init();
                });
            }
        },
        Complete: {
            Attach: function (e) {
                e.preventDefault();
                $image = $.hood.Media.Actions.Target;
                $json = $.hood.Media.Actions.Json;
                $.post($(this).data('url'), function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    if (data.Success) {

                        icon = data.Media.Icon;
                        if (data.Media.GeneralFileType === "Image") {
                            icon = data.Media.MediumUrl;
                        }

                        if (!$.hood.Helpers.IsNullOrUndefined($image)) {
                            $image.css({
                                'background-image': 'url(' + icon + ')'
                            });
                            $image.find('img').attr('src', icon);
                            $image.removeClass('loading');
                        }

                        if (!$.hood.Helpers.IsNullOrUndefined($json)) {
                            $json.val(data.MediaJson);
                        }

                    }
                })
                    .done(function () {
                        $('#media-select-modal').modal('hide');
                    })
                    .fail($.hood.Inline.HandleError);
            },
            Insert: function (e) {
                url = $(e.target).data('url');
                editor = this;
                editor.insertContent('<img alt="Your image..." src="' + url + '"/>');
                $.hood.Modals.Close('#attach-media-modal');
            },
            Select: function (e) {
                url = $(this).data('url');
                tag = $.hood.Media.Actions.Target;
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
            Clear: function (e) {
                e.preventDefault();
                $image = $($(this).data('tag'));
                $json = $($(this).data('json'));
                $.post($(this).data('url'), function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    if (data.Success) {

                        icon = data.Media.Icon;
                        if (data.Media.GeneralFileType === "Image") {
                            icon = data.Media.MediumUrl;
                        }

                        if (!$.hood.Helpers.IsNullOrUndefined($image)) {
                            $image.css({
                                'background-image': 'url(' + icon + ')'
                            });
                            $image.find('img').attr('src', icon);
                            $image.removeClass('loading');
                        }

                        if (!$.hood.Helpers.IsNullOrUndefined($json)) {
                            $json.val(data.Json);
                        }

                    }
                })
                    .fail($.hood.Inline.HandleError);
            }
        },
        RefreshImage: function (tag, url, id) {
            var $image = $(tag);
            $image.addClass('loading');
            $.get(url, { id: id }, $.proxy(function (data) {
                $image.css({
                    'background-image': 'url(' + data.SmallUrl + ')'
                });
                $image.find('img').attr('src', data.SmallUrl);
                $image.removeClass('loading');
            }, this));
        }
    },

    Upload: {
        Init: function () {
            if (!$('#media-add').doesExist())
                return;

            $('#media-total-progress').hide();

            Dropzone.autoDiscover = false;

            var myDropzone = new Dropzone("#media-upload", {
                url: $.hood.Media.Upload.UploadUrl,
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                previewTemplate: false,
                paramName: 'files',
                autoProcessQueue: true, // Make sure the files aren't queued until manually added
                previewsContainer: false, // Define the container to display the previews
                clickable: "#media-add", // Define the element that should be used as click trigger to select files.
                dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
                dictResponseError: 'Error while uploading file!'
            });

            myDropzone.on("success", function (file, data) {
                $.hood.Helpers.ProcessResponse(data);
            });

            myDropzone.on("addedfile", function (file) {
                $('#media-total-progress .progress-bar').css({ width: 0 + "%" });
                $('#media-total-progress .progress-bar .percentage').html(0 + "%");
            });

            // Update the total progress bar
            myDropzone.on("totaluploadprogress", function (progress) {
                $('#media-total-progress .progress-bar').css({ width: progress + "%" });
                $('#media-total-progress .progress-bar .percentage').html(progress + "%");
            });

            myDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                $('#media-total-progress').fadeIn();
                $('#media-total-progress .progress-bar').css({ width: "0%" });
                $('#media-total-progress .progress-bar .percentage').html("0%");
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("complete", function (file) {
                $.hood.Media.Reload();
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("queuecomplete", function (progress) {
                $('#media-total-progress').hide();
                $.hood.Media.Reload();
            });
        },
        UploadUrl: function () {
            return $("#media-upload").data('url') + "?directoryId=" + $("input[type='radio'][name='dir']:checked").val();
        }
    },

    Delete: function (e) {

        var $this = $(this);
        deleteMediaCallback = function (confirmed) {
            if (confirmed) {
                // delete functionality
                $.post('/admin/media/delete', { id: $this.data('id') }, function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    if (data.Success) {
                        $.hood.Media.Reload();
                        $('.modal-backdrop').remove();
                        $('.modal').modal('hide');
                    }
                });
            } 
        };

        $.hood.Alerts.Confirm(
            "The media file will be permanently removed. This cannot be undone.",
            "Are you sure?",
            deleteMediaCallback,
            type = 'warning',
            footer = '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> Ensure this file is not attached to any posts, pages or features of the site, or it will appear as a broken image or file.</span>',
            confirmButtonText = 'Ok',
            cancelButtonText = 'Cancel'
        );
    },

    RestrictDir: function () {
        var pattern = /[^0-9A-Za-z- ]*/g; // default pattern
        var val = $(this).val();
        var newVal = val.replace(pattern, '');
        // This condition is to prevent selection and keyboard navigation issues
        if (val !== newVal) {
            $(this).val(newVal);
        }
    },

    Directories: {
        Editor: function () {
            $('#content-directories-edit-form').hoodValidator({
                validationRules: {
                    DisplayName: {
                        required: true
                    },
                    Slug: {
                        required: true
                    }
                },
                submitButtonTag: $('#content-directories-edit-submit'),
                submitUrl: $('#content-directories-edit-form').attr('action'),
                submitFunction: function (data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Media.ReloadDirectories();
                    $.hood.Media.Reload();
                }
            });
        },
        Delete: function (e) {
            e.preventDefault();
            var $this = $(this);

            deleteDirectoryCallback = function (confirmed) {
                if (confirmed) {
                    $.post($this.attr('href'), function (data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Media.ReloadDirectories();
                        $.hood.Media.Reload();
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The directory and all files will be permanently removed.",
                "Are you sure?",
                deleteDirectoryCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle mr-2"></i><strong>This cannot be undone!</strong><br />Ensure these files are not attached to any posts, pages or features of the site, or it will appear as a broken image or file.</span>',            );

        }
    },

    Players: {},
    LoadMediaPlayers: function (tag = '.hood-media') {
        var videoOptions = {
            techOrder: ["azureHtml5JS", "flashSS", "html5FairPlayHLS", "silverlightSS", "html5"],
            nativeControlsForTouch: false,
            controls: true,
            autoplay: false,
            seeking: true
        };
        $(tag).each(function () {
            try {
                player = $.hood.Media.Players[$(this).data('id')];
                if (player) {
                    try {
                        player.dispose();
                    } catch (ex) {
                        console.log(`There was a problem disposing the old media player: ${ex}`);
                    }
                }
                $.hood.Media.Players[$(this).data('id')] = amp($(this).attr('id'), videoOptions);
                player = $.hood.Media.Players[$(this).data('id')];
                player.src([
                    {
                        src: $(this).data('file'),
                        type: $(this).data('type')
                    }
                ]);
            } catch (ex) {
                console.log(`There was a problem playing the media file: ${ex}`);
            }
        });
    }
};
$(document).ready($.hood.Media.Init);

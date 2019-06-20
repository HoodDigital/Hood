if (!$.hood)
    $.hood = {}
$.hood.Media = {
    Init: function () {
        $('body').on('click', '.delete-media', this.Delete);
        $('body').on('click', '.delete-directory', this.DeleteDirectory);
        $('body').on('click', '.create-directory', this.CreateDirectory)
        if ($('#media-list').doesExist())
            this.Manage.Init();
        if ($('#media-upload').doesExist())
            this.Upload.Init();
    },
    Manage: {
        Init: function () {
            $('#media-list').hoodDataList({
                url: '/admin/media/get',
                params: function () {
                    return {
                        search: $('#media-search').val(),
                        status: $('#media-status').val(),
                        sort: $('#media-sort').val(),
                        type: $('#media-type').val(),
                        directory: $('#media-directory').val()
                    };
                },
                pageSize: 36,
                pagers: '.media-pager',
                template: '#media-template',
                dataBound: function () { },
                refreshOnChange: ".media-change",
                refreshOnClick: ".media-click",
                serverAction: "GET"
            });
        },
        Refresh: function () {
            $.hood.Media.RefreshDirectories();
            if ($('#media-list').doesExist())
                $('#media-list').data('hoodDataList').Refresh()
        }
    },
    Upload: {
        Init: function () {

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

            myDropzone.on("success", function (file, response) {
                if (response.Success === false) {
                    $.hood.Alerts.Error("Uploads failed: " + response.Message);
                } else {
                    $.hood.Alerts.Success("Uploads completed successfully.");
                }
            });

            myDropzone.on("addedfile", function (file) {
                // Hookup the start button
            });

            // Update the total progress bar
            myDropzone.on("totaluploadprogress", function (progress) {
                document.querySelector("#media-total-progress .progress-bar").style.width = progress + "%";
            });

            myDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                document.querySelector("#media-total-progress").style.opacity = "1";
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("complete", function (file) {
                $.hood.Media.Manage.Refresh();
            });

            // Hide the total progress bar when nothing's uploading anymore
            myDropzone.on("queuecomplete", function (progress) {
                document.querySelector("#media-total-progress").style.opacity = "0";
                $.hood.Media.Manage.Refresh();
            });
        },
        UploadUrl: function () {
            return "/admin/media/upload/simple?directory=" + $('#media-directory').val();
        }
    },
    RefreshDirectories: function () {
        $.hood.Inline.Reload('.directories', function () {
            $('.media-click').parent('li').removeClass('active');
            $('.media-click[data-value="' + $('#media-directory').val() + '"]').parent('li').addClass('active');
        });
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
    CreateDirectory: function () {
        $('body').on('keyup', '.sweet-alert input', $.hood.Media.RestrictDir);
        swal({
            title: "Create directory",
            text: "Please enter a name for your new directory:",
            type: "input",
            showCancelButton: true,
            closeOnCancel: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
            animation: "slide-from-top",
            inputPlaceholder: "Directory name..."
        }, function (inputValue) {
            if (inputValue === false) return false; if (inputValue === "") {
                swal.showInputError("You didn't supply a directory name, we can't create one without it!"); return false
            }
            $.get('/admin/media/addDirectory/', { directory: inputValue }, function (data) {
                if (data.Success) {
                    $.hood.Media.Manage.Refresh();
                    swal("Woohoo!", "Directory has been successfully added...", "success");
                } else {
                    swal("Oops!", "There was a problem creating the new directory:\n\n" + data.Errors, "error");
                }
            });
            $('body').off('keyup', '.sweet-alert input', $.hood.Media.RestrictDir);
            return true;
        });
    },
    Delete: function (e) {
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
        },
            function (isConfirm) {
                if (isConfirm) {
                    // delete functionality
                    $.post('/admin/media/delete', { id: $this.data('id') }, function (data) {
                        if (data.Success) {
                            $.hood.Media.Manage.Refresh();
                            $.hood.Blades.Close();
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
    DeleteDirectory: function (e) {
        var $this = $(this);
        message = "The directory and all files will be permanently removed.\n\nWarning: Ensure these files are not attached to any posts, pages or features of the site, or it will appear as a broken image or file.";
        if ($('#media-directory').val() === "") {
            message = "You have selected to delete All directories, this will remove ALL files and ALL directories from the site. Are you sure!?";
        }
        swal({
            title: "Are you sure?",
            text: message,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes, go ahead.",
            cancelButtonText: "No, cancel!",
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
            closeOnCancel: false
        },
            function (isConfirm) {
                if (isConfirm) {
                    // delete functionality
                    $.post('/admin/media/directory/delete', { directory: $('#media-directory').val() }, function (data) {
                        if (data.Success) {
                            $.hood.Media.Manage.Refresh();
                            $.hood.Blades.Close();
                            swal({
                                title: "Deleted!",
                                text: "The directory has now been removed from the website.",
                                timer: 1300,
                                type: "success"
                            });
                        } else {
                            swal({
                                title: "Error!",
                                text: "There was a problem directory the . " + data.Errors,
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
    Players: {},
    LoadMediaPlayers: function (tag) {
        var videoOptions = {
            techOrder: ["azureHtml5JS", "flashSS", "html5FairPlayHLS", "silverlightSS", "html5"],
            nativeControlsForTouch: false,
            controls: true,
            autoplay: false,
            seeking: true
        };
        $(tag).each(function () {
            player = $.hood.Media.Players[$(this).data('id')];
            if (player)
                player.dispose();
            $.hood.Media.Players[$(this).data('id')] = amp($(this).attr('id'), videoOptions);
            player = $.hood.Media.Players[$(this).data('id')];
            player.src([
                {
                    src: $(this).data('file'),
                    type: $(this).data('type')
                }
            ]);
        });
    }
};
$.hood.Media.Init();

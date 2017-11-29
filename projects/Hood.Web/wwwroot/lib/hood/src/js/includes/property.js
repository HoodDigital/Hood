if (!$.hood)
    $.hood = {}
$.hood.Property = {
    Init: function () {
        $('body').on('click', '.delete-property', this.Delete);
        $('body').on('click', '.archive-property', this.Archive);
        $('body').on('click', '.publish-property', this.Publish);
        $('body').on('click', '.create-property', this.Create.Init);

        if ($('#manage-property-list').doesExist())
            this.Manage.Init();
        if ($('#edit-property').doesExist())
            this.Edit.Init();
    },
    Manage: {
        Loaded: false,
        Init: function () {
            vars = $.getUrlVars();
            if (typeof (vars["search"]) !== 'undefined' && vars["search"] !== '')
                $('#manage-property-search').val($.decodeUrl(vars["search"]));
            if (typeof (vars["sort"]) !== 'undefined' && vars["sort"] !== '')
                $('#manage-property-sort').val($.decodeUrl(vars["sort"]));
            if (typeof (vars["type"]) !== 'undefined' && vars["type"] !== '')
                $('#manage-property-type').val($.decodeUrl(vars["type"]));
            if (typeof (vars["planning"]) !== 'undefined' && vars["planning"] !== '')
                $('#manage-property-planning').val($.decodeUrl(vars["planning"]));
            if (vars["all"] === 'false')
                $('#manage-property-showall').prop("checked", false);
            else
                $('#manage-property-showall').prop("checked", true);

            $('#manage-property-list').hoodDataList({
                url: '/admin/property/get',
                params: this.Params,
                pageSize: 12,
                pagers: '.manage-property-pager',
                template: '#manage-property-template',
                dataBound: function () {
                    if (!$.hood.Property.Manage.Loaded) {
                        page = $.getUrlVars()["page"];
                        if (isNaN(page))
                            page = 1;
                        this.dataSource.page(page);
                        $.hood.Property.Manage.Loaded = true;
                    }
                    if (history.pushState) {
                        var newurl = location.pathname + '?' + $.param($.hood.Property.Manage.Params()) + '&page=' + this.dataSource.page();
                        window.history.pushState({ path: newurl }, '', newurl);
                    }
                },
                refreshOnChange: ".manage-property-change",
                refreshOnClick: ".manage-property-click",
                serverAction: "GET"
            });
        },
        Params: function () {
            return {
                search: $('#manage-property-search').val(),
                sort: $('#manage-property-sort').val(),
                type: $('#manage-property-type').val(),
                planning: $('#manage-property-planning').val(),
                all: $('#manage-property-showall').is(':checked')
            };
        },
        Filter: function () {
            return {
                search: $('#manage-property-search').val(),
                sort: $('#manage-property-sort').val()
            };
        },
        Refresh: function () {
            if ($('#manage-property-list').doesExist())
                $('#manage-property-list').data('hoodDataList').Refresh();
            $.hood.Blades.Reload();
        }
    },
    Delete: function (e) {
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
        },
        function (isConfirm) {
            if (isConfirm) {
                // delete functionality
                $.post('/admin/property/delete', { id: $this.data('id') }, function (data) {
                    if (data.Success) {
                        if (!$('#manage-property-list').doesExist())
                            window.location = data.Url;
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
    Publish: function (e) {
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
        },
        function (isConfirm) {
            if (isConfirm) {
                // delete functionality
                $.post('/admin/property/publish', { id: $this.data('id') }, function (data) {
                    if (data.Success) {
                        if (!$('#manage-property-list').doesExist())
                            window.location = data.Url;
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
    Archive: function (e) {
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
        },
        function (isConfirm) {
            if (isConfirm) {
                // delete functionality
                $.post('/admin/property/archive', { id: $this.data('id') }, function (data) {
                    if (data.Success) {
                        if (!$('#manage-property-list').doesExist())
                            window.location = data.Url;
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
        Init: function (e) {
            e.preventDefault();
            $.hood.Blades.OpenWithLoader('button.create-property', '/admin/property/create/', $.hood.Property.Create.SetupCreateForm);
        },
        SetupCreateForm: function () {
            $('#create-property-form').find('.datepicker').datepicker({
                todayBtn: "linked",
                keyboardNavigation: false,
                forceParse: false,
                calendarWeeks: true,
                autoclose: true,
                orientation: "bottom",
                format: "dd/mm/yyyy",
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
                submitFunction: function (data) {
                    if (data.Success) {
                        $('#manage-property-list').data('hoodDataList').Refresh();
                        swal("Created!", "The property has now been created!", "success");
                    } else {
                        swal("Error", "There was a problem creating the property:\n\n" + data.Errors, "error");
                    }
                }
            });
            $.hood.Handlers.Addresses.InitAutocomplete();
        }
    },
    Edit: {
        Init: function () {
            this.LoadEditors('#edit-property');
            $.hood.Property.Upload.InitImageUploader();
            $.hood.Property.Upload.InitFloorplanUploader();
            $('body').on('click', '.add-floor', this.AddFloor);
            $('body').on('click', '.delete-floor', this.DeleteFloor);
            $('body').on('change', '.recalc-floor', this.RecalcFloor);
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
        },
        Blade: function () {
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
                submitFunction: function (data) {
                    if (data.Succeeded) {
                        $('#manage-property-list').data('hoodDataList').Refresh();
                        $.hood.Alerts.Success("Updated.");
                    } else {
                        $.hood.Alerts.Error("There was an error saving.");
                    }
                }
            });
        },
        LoadEditors: function (tag) {
            // Load the url thing if on page editor.
            $(tag).find('.datepicker').datepicker({
                todayBtn: "linked",
                keyboardNavigation: false,
                forceParse: true,
                calendarWeeks: true,
                autoclose: true,
                orientation: "bottom",
                format: "dd/mm/yyyy",
            });

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
}
$.hood.Property.Init();

if (!$.hood)
    $.hood = {}
$.hood.Content = {
    Init: function () {
        $('body').on('click', '.delete-content', this.Delete);
        $('body').on('click', '.delete-content-field', this.Meta.Delete);
        $('body').on('click', '.clone-content', this.Clone);
        $('body').on('click', '.create-content', this.Create.Init);
        $('body').on('click', '.publish-content', this.Publish);
        $('body').on('click', '.archive-content', this.Archive);
        $('body').on('click', '.create-content', this.Create.Init);
        $('body').on('click', '.edit-category', this.Categories.Edit);
        $('body').on('click', '.save-category', this.Categories.Save);
        $('body').on('click', '.add-category', this.Categories.Add);
        $('body').on('click', '.delete-category', this.Categories.Delete);
        $('body').on('click', '.add-custom-field', this.Types.AddField);
        $('body').on('click', '.delete-custom-field', this.Types.DeleteField);

        $('body').on('keyup', '#Slug', function () {
            $('.slug-display').html($(this).val());
        });

        if ($('#manage-content-list').doesExist())
            this.Manage.Init();
        if ($('#edit-content').doesExist())
            this.Edit.Init();
        if ($('#add-field-form').doesExist())
            this.Meta.Init();

    },
    Types: {
        AddField: function () {
            var name = $('#custom-field-name-' + $(this).data('id')).val();
            var fields = $.hood.Content.Types.GetFieldsList($(this).data('id'));
            exists = false;
            $.each(fields, function (key, value) {
                if (value.Name == name)
                    exists = true;
            });
            if (exists) {
                $.hood.Alerts.Error("Cannot insert two fields with the same number.");
                return;
            }
            // Add the new item.
            newField = {
                Name: "Custom." + $('#custom-field-name-' + $(this).data('id')).val(),
                Default: $('#custom-field-default-' + $(this).data('id')).val(),
                Type: $('#custom-field-type-' + $(this).data('id')).val(),
                System: false
            };
            fields.push(newField);
            $.hood.Content.Types.ReRenderFields(fields, $(this).data('id'));
            $('#fields-' + $(this).data('id')).val(JSON.stringify(fields));
            $.hood.Alerts.Success("Added field.");
        },
        DeleteField: function () {
            var fields = $.hood.Content.Types.GetFieldsList($(this).data('id'));
            var name = $(this).data('name');
            fields = $.grep(fields, function (e) {
                return e.Name != name;
            });
            $.hood.Content.Types.ReRenderFields(fields, $(this).data('id'));
            $('#fields-' + $(this).data('id')).val(JSON.stringify(fields));
            $.hood.Alerts.Success("Deleted field.");
        },
        GetFieldsList: function (id) {
            // Take the contents of the fields input. 
            fieldsInput = $('#fields-' + id).val();
            // if it is null, we need a new object.
            if (fieldsInput != null && fieldsInput != '') {
                var obj = JSON.parse(fieldsInput);
                // if not, we can deserialise to an array of FieldAreas
                for (var x in obj) {
                    if (obj[x].hasOwnProperty('Name')) {
                        return obj
                    }
                }
            }
            // if not, we can deserialise to an array of FieldAreas
            return new Array();
        },
        ReRenderFields: function (arr, id) {
            newList = $('#field-list-' + id).empty();
            for (i = 0; i < arr.length; i++) {
                fld = "<tr><td class='col-xs-8'><strong>" + arr[i].Name + "</strong> " + arr[i].Type + "</td><td class='col-xs-4 text-right'>";
                if (!arr[i].System) {
                    fld += "<a class='delete-custom-field btn btn-xs bg-color-red txt-color-white' data-name='" + arr[i].Name + "' data-id='" + id + "'><i class='fa fa-trash-o'></i></a>"
                }
                else {
                    fld += '<span class="label label-default">System Field</span>';
                }
                fld += "</td></tr>";
                newList.append(fld)
            }
        }
    },
    Categories: {
        Edit: function (e) {
            var $this = $(this);
            e.preventDefault();
            $.hood.Blades.OpenWithLoader('.edit-category', '/admin/categories/edit/' + $(this).data('id') + '?type=' + $(this).data('type'), null);
        },
        Save: function (e) {
            $.post('/admin/categories/save/', $('#edit-category-form').serialize(), function (data) {
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
        Add: function (e) {
            $.post('/admin/categories/add/', $('#add-category-form').serialize(), function (data) {
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
        Delete: function () {
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
            },
            function (isConfirm) {
                if (isConfirm) {
                    // delete functionality
                    $.post('/admin/categories/delete/' + $this.data('id'), null, function (data) {
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
                                timer: 1300,
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
    Manage: {
        Init: function () {
            $('#manage-content-list').hoodDataList({
                url: '/admin/content/get',
                params: function () {
                    return {
                        search: $('#manage-content-search').val(),
                        sort: $('#manage-content-sort').val(),
                        category: $('#manage-content-category').val(),
                        type: $('#manage-content-list').data('type')
                    };
                },
                pageSize: 12,
                pagers: '.manage-content-pager',
                template: '#manage-content-template',
                dataBound: function () { },
                refreshOnChange: ".manage-content-change",
                refreshOnClick: ".manage-content-click",
                serverAction: "GET"
            });


        },
        Filter: function () {
            return {
                search: $('#manage-content-search').val(),
                sort: $('#manage-content-sort').val()
            };
        },
        Refresh: function () {
            if ($('#manage-content-list').doesExist())
                $('#manage-content-list').data('hoodDataList').Refresh();
            $.hood.Blades.Reload();
        }
    },
    Delete: function (e) {
        var $this = $(this);
        swal({
            title: "Are you sure?",
            text: "The content will be permanently removed.",
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
                $.post('/admin/content/' + $this.data('id') + '/delete', null, function (data) {
                    if (data.Success) {
                        if (!$('#manage-content-list').doesExist())
                            window.location = '/admin/content/' + $this.data('type') + '/manage/';
                        $.hood.Content.Manage.Refresh();
                        $.hood.Blades.Close();
                        swal({
                            title: "Deleted!",
                            text: "The content has now been removed from the website.",
                            timer: 1300,
                            type: "success"
                        });
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem deleting the content: " + data.Errors,
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
            text: "The item will be visible on the website.",
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
                $.post('/admin/content/' + $this.data('id') + '/publish', null, function (data) {
                    if (data.Success) {
                        $.hood.Content.Manage.Refresh();
                        $.hood.Blades.Close();
                        swal({
                            title: "Published!",
                            text: "The item has now been published.",
                            timer: 1300,
                            type: "success"
                        });
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem publishing the item: " + data.Errors,
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
            text: "The item will be hidden from the website.",
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
                $.post('/admin/content/' + $this.data('id') + '/archive', null, function (data) {
                    if (data.Success) {
                        $.hood.Content.Manage.Refresh();
                        $.hood.Blades.Close();
                        swal({
                            title: "Archived!",
                            text: "The item has now been archived.",
                            timer: 1300,
                            type: "success"
                        });
                    } else {
                        swal({
                            title: "Error!",
                            text: "There was a problem archiving the item: " + data.Errors,
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
    Clone: function (e) {
        var $this = $(this);
        $.post('/admin/content/' + $this.data('id') + '/clone', null, function (data) {
            if (data.Success) {
                window.location = '/admin/edit/' + data.id;
                swal({
                    title: "Cloned!",
                    text: "The content has now been cloned just forwarding you to the new content...",
                    type: "success"
                });
                $.hood.Content.Manage.Refresh();
                $.hood.Blades.Close();
            } else {
                swal({
                    title: "Error!",
                    text: "There was a problem cloning the content: " + data.Errors,
                    timer: 1300,
                    type: "error"
                });
            }
        });
    },
    Create: {
        Init: function (e) {
            var $this = $(this);
            e.preventDefault();
            $.hood.Blades.OpenWithLoader('button.create-content', '/admin/content/' + $this.data('type') + '/create/', $.hood.Content.Create.SetupCreateForm);
        },
        SetupCreateForm: function () {
            $('#create-content-form').find('.datepicker').datepicker({
                todayBtn: "linked",
                keyboardNavigation: false,
                forceParse: false,
                calendarWeeks: true,
                autoclose: true,
                orientation: "bottom",
                format: "dd/mm/yyyy",
            });
            $('#create-content-form').hoodValidator({
                validationRules: {
                    cpTitle: {
                        required: true
                    },
                    cpExcept: {
                        required: true
                    },
                    cpPublishDate: {
                        required: true,
                        ukdate: true
                    }
                },
                submitButtonTag: $('#create-content-submit'),
                submitUrl: '/admin/content/add',
                submitFunction: function (data) {
                    if (data.Success) {
                        $('#manage-content-list').data('hoodDataList').Refresh();
                        swal("Created!", "The content has now been created!", "success");
                    } else {
                        swal("Error", "There was a problem creating the content:\n\n" + data.Errors, "error");
                    }
                }
            });
        }
    },
    Edit: {
        Init: function () {
            this.LoadEditors('#edit-content');
            $.hood.Content.Upload.InitImageUploader();

            tinymce.init({
                selector: '.edit-content-editor',
                height: 500,
                plugins: [
                  'advlist autolink lists link image charmap print preview anchor',
                  'searchreplace visualblocks code fullscreen',
                  'insertdatetime media contextmenu paste code'
                ],
                toolbar: 'insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image hoodimage',
                link_class_list: [
                    { title: 'None', value: '' },
                    { title: 'Button link', value: 'btn btn-default' },
                    { title: 'Theme coloured button link', value: 'btn btn-primary' },
                    { title: 'Popup image/video', value: 'colorbox-iframe' },
                    { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
                    { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
                    { title: 'Large link', value: 'font-lg' },
                    { title: 'Large button link', value: 'btn btn-default btn-lg' },
                    { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
                    { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
                    { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
                    { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
                ],
                setup: $.hood.Uploader.Load.Insert,
                content_css: [
                ]
            });

            tinymce.init({
                selector: '.edit-content-editor-simple',
                height: 500,
                plugins: [
                  'advlist autolink lists link image charmap print preview anchor',
                  'searchreplace visualblocks code fullscreen',
                  'insertdatetime media contextmenu paste code'
                ],
                menubar: false,
                toolbar: 'styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | undo redo | link image hoodimage',
                link_class_list: [
                    { title: 'None', value: '' },
                    { title: 'Button link', value: 'btn btn-default' },
                    { title: 'Theme coloured button link', value: 'btn btn-primary' },
                    { title: 'Popup image/video', value: 'colorbox-iframe' },
                    { title: 'Button popup link', value: 'btn btn-default colorbox-iframe' },
                    { title: 'Theme coloured button popup link', value: 'btn btn-primary colorbox-iframe' },
                    { title: 'Large link', value: 'font-lg' },
                    { title: 'Large button link', value: 'btn btn-default btn-lg' },
                    { title: 'Large theme coloured button link', value: 'btn btn-primary btn-lg' },
                    { title: 'Large popup image/video', value: 'font-lg colorbox-iframe' },
                    { title: 'Large Button popup link', value: 'btn btn-default btn-lg colorbox-iframe' },
                    { title: 'Theme coloured button popup link', value: 'btn btn-primary btn-lg colorbox-iframe' }
                ],
                setup: $.hood.Uploader.Load.Insert,
                content_css: [
                ]
            });

            //$('body').on('click', '#content-category-submit', this.AddCategory);
            if ($('#designer-window').doesExist())
                this.Designer.Init();

            $('body').on('click', '#add-content-category', this.Categories.AddCategory);
            $('body').on('click', '.remove-category', this.Categories.RemoveCategory);

            var xhr;
            new autoComplete({
                selector: 'input.autocomplete-category',
                source: function (term, response) {
                    try { xhr.abort(); } catch (e) { }
                    xhr = $.getJSON('/admin/content/categorysuggestions/', { query: term }, function (data) { response(data); });
                }
            });

        },
        Categories: {
            AddCategory: function () {
                $.post($(this).data('url'), { category: $('#content-category-name').val() }, function (data) {
                    if (data.Success) {
                        $.hood.Alerts.Success("Added category.");
                    } else {
                        $.hood.Alerts.Error("Couldn't add the category: " + data.Error);
                    }
                    $.hood.Inline.Refresh(".categories");
                });
            },
            RemoveCategory: function () {
                alert('not implemented');
            }
        },
        Designer: {
            Window: $('#designer-window'),
            Area: function () { return $('#designer-window').contents().find("#editable-content"); },
            Init: function () {

                $('body').on('change', '#preview-size', function () {
                    $('#designer-window').attr('class', 'designer-window ' + $(this).val());
                });

                $('body').on('change', '#Body', function () {
                    $.hood.Content.Designer.Area().html($(this).val());
                });

            }
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
    Meta: {
        Create: function () {
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
                submitFunction: function (data) {
                    if (data.Success) {
                        $.hood.Inline.Reload('#content-meta-fields');
                        swal("Created!", "The field has now been created!", "success");
                    } else {
                        swal("Error", "There was a problem creating the content:\n\n" + data.Errors, "error");
                    }
                }
            });
        },
        Delete: function () {
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
            },
            function (isConfirm) {
                if (isConfirm) {
                    // delete functionality
                    $.post('/admin/content/deletemeta', { id: $this.data('id') }, function (data) {
                        if (data.Success) {
                            $.hood.Inline.Reload('#content-meta-fields');
                            $.hood.Blades.Close();
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
                                timer: 1300,
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
    Upload: {
        InitImageUploader: function () {
            if (!$("#content-gallery-upload").doesExist())
                return;

            Dropzone.autoDiscover = false;

            var pgDropzone = new Dropzone("#content-gallery-upload", {
                url: "/admin/content/upload/gallery?id=" + $("#content-gallery-upload").data('id'),
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                previewTemplate: false,
                paramName: 'files',
                autoProcessQueue: true, // Make sure the files aren't queued until manually added
                previewsContainer: false, // Define the container to display the previews
                clickable: "#content-gallery-add", // Define the element that should be used as click trigger to select files.
                dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
                dictResponseError: 'Error while uploading file!'
            });


            pgDropzone.on("success", function (file, response) {
                if (response.Success === false) {
                    $.hood.Alerts.Error("Uploads failed: " + response.Error);
                } else {
                    $.hood.Alerts.Success("Uploads completed successfully.");
                }
            });

            pgDropzone.on("addedfile", function (file) {
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
        }
    }
}
$(window).load(function () {
    $.hood.Content.Init();
});


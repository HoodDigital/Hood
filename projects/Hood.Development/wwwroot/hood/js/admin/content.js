if (!$.hood)
    $.hood = {};
$.hood.Content = {
    Init: function() {
        $('body').on('click', '.content-meta-delete', $.hood.Content.Meta.Delete);
        $('body').on('click', '.content-clone', $.hood.Content.Clone);

        $('body').on('click', '.content-media-delete', $.hood.Content.Media.Delete);

        $('body').on('click', '.add-custom-field', $.hood.Content.Types.AddField);
        $('body').on('click', '.delete-custom-field', $.hood.Content.Types.DeleteField);

        if ($('#content-meta-form').doesExist())
            $.hood.Content.Meta.Init();
    },

    Lists: {
        Fields: {
            Loaded: function(sender, data) {
                $.hood.Loader(false);
            },
            Reload: function(complete) {
                if ($('#content-meta-list').doesExist())
                    $.hood.Inline.Reload($('#content-meta-list'), complete);
            }
        },
        Media: {
            Loaded: function(sender, data) {
                $.hood.Loader(false);
            },
            Reload: function(complete) {
                if ($('#content-media-list').doesExist())
                    $.hood.Inline.Reload($('#content-media-list'), complete);
            }
        }
    },

    // Metadata
    Meta: {
        Create: function() {
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
                submitFunction: function(data) {
                    $.hood.Helpers.ProcessResponse(data);
                    $.hood.Content.Lists.Fields.Reload();
                }
            });
        },
        Delete: function(e) {
            e.preventDefault();
            let $tag = $(this);

            let deleteCategoryCallback = function(isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function(data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Content.Lists.Fields.Reload();
                    });
                }
            };

            $.hood.Alerts.Confirm(
                "The field will be permanently removed.",
                "Are you sure?",
                deleteCategoryCallback,
                'error',
                '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>',
            );

        }
    },

    // Media
    Media: {
        Delete: function(e) {
            e.preventDefault();
            let $tag = $(this);

            let deleteMediaCallback = function(isConfirm) {
                if (isConfirm) {
                    $.post($tag.attr('href'), function(data) {
                        $.hood.Helpers.ProcessResponse(data);
                        $.hood.Content.Lists.Media.Reload();
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
    },

    // Content Types
    Types: {
        AddField: function() {
            let name = $('#custom-field-name-' + $(this).data('id')).val();
            let fields = $.hood.Content.Types.GetFieldsList($(this).data('id'));
            let exists = false;
            $.each(fields, function(key, value) {
                if (value.Name === name)
                    exists = true;
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
            }
            // Add the new item.
            let newField = {
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
        DeleteField: function() {
            let fields = $.hood.Content.Types.GetFieldsList($(this).data('id'));
            let name = $(this).data('name');
            fields = $.grep(fields, function(e) {
                return e.Name !== name;
            });
            $.hood.Content.Types.ReRenderFields(fields, $(this).data('id'));
            $('#custom-fields-' + $(this).data('id')).val(JSON.stringify(fields));
            $.hood.Alerts.Success("Deleted field.");
        },
        GetFieldsList: function(id) {
            // Take the contents of the fields input. 
            let fieldsInput = $('#custom-fields-' + id).val();
            // if it is null, we need a new object.
            if (fieldsInput !== null && fieldsInput !== '') {
                var obj = JSON.parse(fieldsInput);
                // if not, we can deserialise to an array of FieldAreas
                for (var x in obj) {
                    if (obj[x].hasOwnProperty('Name')) {
                        return obj;
                    }
                }
            }
            // if not, we can deserialise to an array of FieldAreas
            return new Array();
        },
        ReRenderFields: function(arr, id) {
            let newList = $('#field-list-' + id).empty();
            for (i = 0; i < arr.length; i++) {
                let fld = "<tr><td class='col-xs-8'><strong>" + arr[i].Name + "</strong> " + arr[i].Type + "</td><td class='col-xs-4 text-right'>";
                if (!arr[i].System) {
                    fld += "<a class='delete-custom-field btn btn-xs bg-color-red txt-color-white' data-name='" + arr[i].Name + "' data-id='" + id + "'><i class='fa fa-trash-o'></i></a>";
                }
                else {
                    fld += '<span class="label label-default">System Field</span>';
                }
                fld += "</td></tr>";
                newList.append(fld);
            }
        }
    }

};
$(document).ready($.hood.Content.Init);

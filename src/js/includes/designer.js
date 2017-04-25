if (!$.hood)
    $.hood = {}
$.hood.Designer = {
    Drake: null,
    Init: function () {
        $('body').on('click', '.save-content', $.hood.Designer.Save);
        $('body').on('click', '.hood-add-block', $.hood.Designer.Blocks.Add);
        $('body').on('click', '.choose-content-block', $.hood.Designer.Blocks.ChooseBlock);
        $('body').on('click', '.insert-content-block', $.hood.Designer.Blocks.InsertBlock);
        $('body').on('click', '.hood-delete-block', $.hood.Designer.Blocks.Delete);
        $('body').on('click', 'figure.img', $.hood.Designer.Images.Change);

        $('body').on('click', 'a:not(.admin)', function (event) {
            $.hood.Alerts.Warning("Links do not work in the preview window.");
            event.preventDefault();
        });
        $('body').on('click', '.toggle-editors', function (event) {
            if ($('body').hasClass('hood-editors-active')) {
                $.hood.Designer.ContentMode();
            } else {
                $.hood.Designer.LayoutMode();
            }
        });

        if ($.hood.Helpers.InIframe()) {
            $('#designer-bar .back-to-edit').hide();
            $('.hood-admin-editor').hide();
        } else {
            $('#designer-bar .open-full').hide();
        }

        // initialise the content
        this.InitContent();
        // once content is up to date, init dragula.
        this.InitDragula();
        this.InitEditors();
    },
    Loader: {
        Show: function () {
            $('div#hood-designer-loader').fadeIn();
        },
        Hide: function () {
            $('div#hood-designer-loader').fadeOut();
        }
    },
    ContentMode: function () {
        $('.toggle-editors').removeClass('btn-default').addClass('btn-info').html('<i class="fa fa-arrows-alt m-r-sm"></i>Edit Mode');
        $('body').removeClass('hood-editors-active');
    },
    LayoutMode: function () {
        $('.toggle-editors').removeClass('btn-info').addClass('btn-default').html('<i class="fa fa-desktop m-r-sm"></i>Preview Mode');
        $('body').addClass('hood-editors-active');
    },
    InitContent: function () {
        // if content has no 'hood-drag' containers
        if (!$('#editable-content').find('.hood-block').length) {
            content = $('#editable-content').html();
            // surround the entire content with a general content - and then surround that with a default, single column contiainer.
            editor = $('<div class="hood-block-general tinymce-free"></div>').append(content);
            block = $('<div class="hood-block"></div>')
            block.append(editor);
            block.append($.hood.Designer.Blocks.BlockControls);
            $('#editable-content').empty().append(block);
        } else {
            // simply add any drag handles to any blocks.
            $('#editable-content').find('.hood-block').append($.hood.Designer.Blocks.BlockControls);
        }
        setTimeout(function () {
            $.hood.Designer.Loader.Hide();
        }, 1500);
    },
    InitDragula: function () {
        this.Drake = dragula({
            revertOnSpill: true,
            moves: function (el, container, handle) {
                return handle.classList.contains('hood-drag-handle');
            },
            isContainer: function (el) {
                return el.classList.contains('editable-content');
            }
        });
    },
    InitEditors: function () {
        var linkClasses = [
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
        ];

        tinymce.init({
            selector: '.tinymce-free',
            plugins: [
                'advlist autolink lists link image charmap print preview anchor media table',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media contextmenu paste code'
            ],
            toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist outdent indent | undo redo | link image media hoodimage | code',
            selection_toolbar: 'styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | undo redo | link image',
            insert_toolbar: 'quickimage quicktable',
            height: 500,
            menubar: false,
            link_class_list: linkClasses,
            setup: $.hood.Uploader.Load.Insert,
            inline: true,
            image_advtab: true,
            paste_data_images: true,
            images_upload_handler: function (blobInfo, success, failure) {
                var xhr, formData;

                xhr = new XMLHttpRequest();
                xhr.withCredentials = false;
                xhr.open('POST', '/admin/media/upload/single?directory=' + $('#designer-bar').data('upload'));

                xhr.onload = function () {
                    var json;

                    if (xhr.status != 200) {
                        failure('HTTP Error: ' + xhr.status);
                        return;
                    }

                    json = JSON.parse(xhr.responseText);

                    if (!json || typeof json.location != 'string') {
                        failure('Invalid JSON: ' + xhr.responseText);
                        return;
                    }

                    success(json.location);
                };

                formData = new FormData();
                formData.append('file', blobInfo.blob(), blobInfo.filename());

                xhr.send(formData);
            },
            content_css: [
            ]
        });
        tinymce.init({
            selector: '.tinymce-text',
            toolbar: 'bold italic underline | quicklink | undo redo',
            menubar: false,
            inline: true
        });
        tinymce.init({
            selector: '.tinymce-buttons',
            plugins: 'link contextmenu',
            toolbar: 'link | undo redo',
            menubar: false,
            inline: true
        });
    },
    KillDragula: function () {
        this.Drake.destroy();
    },
    KillEditors: function () {
        tinymce.remove(".tinymce-free");
        tinymce.remove(".tinymce-text");
        tinymce.remove(".tinymce-buttons");
    },
    Blocks: {
        BlockControls: '<div class="hood-drag-handle"></div><div class="hood-delete-block"><i class="fa fa-trash-o"></i></div>',
        Add: function (e) {
            $.hood.Designer.Loader.Show();
            $.hood.Modals.Open('/admin/content/choose', null, '.hood-attach', $.proxy(function () {
                $.hood.Designer.Blocks.BlockList();
                $.hood.Designer.Loader.Hide();
            }, this));
        },
        ChooseBlock: function (e) {
            ds = $('#content-block-list').data('hoodDataList').DataSource;
            itm = ds.getByUid($(this).data('uid'));
            template = kendo.template($("#content-block-insert-template").html());
            preview = template(itm);
            $("#insert-block-details").html(preview);
        },
        InsertBlock: function (e) {
            $.hood.Designer.Loader.Show();
            $.hood.Designer.KillDragula();
            $.hood.Designer.KillEditors();

            params = $('#content-block-variables').serializeArray();
            params.push({ name: "url", value: $(this).data('url') });

            $.get('/admin/content/block', params, function (block) {

                html = $('<div></div>')
                    .append(block)
                    .find('.hood-block')
                    .append($.hood.Designer.Blocks.BlockControls)
                    .end()
                    .html();
                $('#editable-content').append(html);

            }).done(function () {
                $.hood.Alerts.Success("Inserted.");
            }).fail(function () {
                $.hood.Alerts.Error("There was a problem adding the block.");
            }).always(function () {
                $.hood.Designer.InitDragula();
                $.hood.Designer.InitEditors();
                $.hood.Designer.Loader.Hide();
            });

        },
        BlockList: function () {
            $('#content-block-list').hoodDataList({
                url: '/admin/content/blocks',
                params: function () {
                    return {
                    };
                },
                pageSize: 12,
                pagers: '.content-block-pager',
                template: '#content-block-template',
                dataBound: function () { },
                refreshOnChange: ".content-block-change",
                refreshOnClick: ".content-block-click",
                serverAction: "GET"
            });
        },
        Refresh: function () {
            if ($('#content-block-list').doesExist())
                $('#content-block-list').data('hoodDataList').Refresh()
        },
        Delete: function (e) {
            $(this).parent().remove();
        },
    },
    Images: {
        Change: function () {

        }
    },
    Save: function () {
        $.hood.Designer.Loader.Show();
        $('body').removeClass('hood-editors-active');
        $.hood.Designer.KillDragula();
        $.hood.Designer.KillEditors();
        var content = $('#editable-content').html();
        var html = $('<div></div>');
        html.append(content);
        $(html).find('.hood-drag-handle').remove();
        $(html).find('.hood-delete-block').remove();
        html = $(html)
                .find('.tinymce-free')
                .removeClass('mce-content-body').removeAttr('style').removeAttr('contenteditable').removeAttr('spellcheck').removeAttr('id')
                .end()
                .html();
        params = {
            Id: $('#editable-content').data('id'),
            Body: html
        };
        $.post('/admin/content/designer/save/', params, function (data) {
            if (data.Succeeded) {
                $.hood.Alerts.Success("Saved");
            } else {
                $.hood.Alerts.Error(data.ErrorString);
            }
        })
        .done(function () {
        })
        .fail(function () {
        })
        .always(function () {
            $.hood.Designer.InitDragula();
            $.hood.Designer.InitEditors();
            $.hood.Designer.Loader.Hide();
        });
    }
}
$(window).load(function () {
    $.hood.Designer.Init();
});
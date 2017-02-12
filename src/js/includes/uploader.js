if (!$.hood)
    $.hood = {}
$.hood.Uploader = {
    Init: function () {
        // ATTACH FUNCTION - ATTACHES THE IMAGE TO A SPECIFIC ENTITY ATTACHABLE FIELD
        $('body').on('click', '.hood-image-attach', $.hood.Uploader.Load.Attach);

        // INSERT FUNCTION - INSERTS AN IMAGE TAG INTO THE CURRENTLY SELECTED EDITOR
        $('body').on('click', '.hood-image-insert', $.hood.Uploader.Load.Insert);

        // SET FUNCTION - SETS THE VALUE OF THE TAGGED INPUT
        $('body').on('click', '.hood-image-set', $.hood.Uploader.Load.Set);

        // SWITCH FUNCTION - REPLACES IMAGES IN THE DESIGNER
        $('body').on('click', '.hood-image-switch', $.hood.Uploader.Load.Switch);
    },
    Switching: null,
    Load: {
        Attach: function (e) {
            params = {
                Id: $(this).data('id'),
                Entity: $(this).data('entity'),
                Field: $(this).data('field'),
                Type: $(this).data('type'),
                Refresh: $(this).data('refresh'),
                Tag: $(this).data('tag')
            }
            $.hood.Modals.Open('/admin/media/attach/', params, '.hood-image-attach', $.proxy(function () {
                $('#attach-media-title').html($(this).attr('title'))
                $('body').off('click', '.attach-media-select', $.hood.Uploader.Complete.Attach);
                $('body').on('click', '.attach-media-select', $.hood.Uploader.Complete.Attach);
                $.hood.Media.Manage.Init();
                $.hood.Media.Upload.Init();
            }, this));
        },
        Insert: function (editor) {
            editor.addButton('hoodimage', {
                text: 'Insert image...',
                icon: false,
                onclick: function () {
                    $.hood.Modals.Open('/admin/media/insert/', null, '.hood-image-attach', $.proxy(function () {
                        $('body').off('click', '.media-insert', $.proxy($.hood.Uploader.Complete.Insert, editor));
                        $('body').on('click', '.media-insert', $.proxy($.hood.Uploader.Complete.Insert, editor));
                        $.hood.Media.Manage.Init();
                        $.hood.Media.Upload.Init();
                    }, this));
                }
            });
        },
        Set: function (e) {
            try {
                $.hood.Designer.Loader.Show();
            } catch (ex) { }
            $.hood.Uploader.Switching = $($(this).data('target'));
            params = {
                Restrict: $(this).data('restrict')
            }
            $.hood.Modals.Open('/admin/media/select/', params, '.hood-image-set', $.proxy(function () {
                $('#attach-media-title').html($(this).attr('title'))
                $('body').off('click', '.media-select', $.hood.Uploader.Complete.Set);
                $('body').on('click', '.media-select', $.hood.Uploader.Complete.Set);
                $.hood.Media.Manage.Init();
                $.hood.Media.Upload.Init();
                try {
                    $.hood.Designer.Loader.Hide();
                } catch (ex) { }
            }, this));
        },
        Switch: function (e) {
            try {
                $.hood.Designer.Loader.Show();
            } catch (ex) { }
            $.hood.Uploader.Switching = $(this);
            params = {
                Tag: $(this).data('target'),
                Restrict: $(this).data('restrict')
            }
            $.hood.Modals.Open('/admin/media/select/', params, '.hood-image-switch', $.proxy(function () {
                $('#attach-media-title').html($(this).attr('title'))
                $('body').on('click', '.media-select', $.hood.Uploader.Complete.Switch);
                $.hood.Media.Manage.Init();
                $.hood.Media.Upload.Init();
                try {
                    $.hood.Designer.Loader.Hide();
                } catch (ex) { }
            }, this));
        },
    },
    Complete: {
        Attach: function (e) {
            $(this).data('temp', $(this).html());
            $(this).addClass('loading').append('<i class="fa fa-refresh fa-spin m-l-sm"></i>');
            params = {
                Id: $(this).data('id'),
                Entity: $(this).data('entity'),
                Field: $(this).data('field'),
                MediaId: $(this).data('media')
            };
            $.post('/admin/media/attach/', params, $.proxy(function (data) {
                if (data.Success) {
                    $.hood.Alerts.Success("Attached!");
                    if (!$.hood.Helpers.IsNullOrUndefined($(this).data('tag')) && !$.hood.Helpers.IsNullOrUndefined($(this).data('refresh'))) {
                        $image = $('.' + $(this).data('tag'));
                        $image.addClass('loading');
                        $.get($(this).data('refresh'), { id: $(this).data('id') }, $.proxy(function (data) {
                            $image = $('.' + $(this).data('tag'));
                            $image.css({
                                'background-image': 'url(' + data.SmallUrl + ')'
                            });
                            $image.find('img').attr('src', data.SmallUrl);
                            $image.removeClass('loading');
                        }, this));
                    }
                } else {
                    $.hood.Alerts.Error(data.Errors, "Error attaching.");
                }
            }, this))
            .done(function () {
            })
            .fail(function (data) {
                $.hood.Alerts.Error(data.status + " - " + data.statusText, "Error communicating.");
            })
            .always($.proxy(function (data) {
                $(this).removeClass('loading').html($(this).data('temp'));
                $.hood.Modals.Close('#attach-media-modal');
            }, this));
        },
        Insert: function (e) {
            url = $(e.target).data('url');
            editor = this;
            editor.insertContent('<img alt="Your image..." src="' + url + '"/>');
            $.hood.Modals.Close('#attach-media-modal');
        },
        Set: function (e) {
            url = $(this).data('url');
            tag = $.hood.Uploader.Switching;
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
            })
            $.hood.Modals.Close('#attach-media-modal');
            $.hood.Alerts.Success("Attached!");
        },
        Switch: function (e) {
            url = $(this).data('url');
            tag = $.hood.Uploader.Switching;
            $(tag).css({
                'background-image': 'url(' + url + ')'
            });
            $(tag).find('img').attr('src', url);
            $.hood.Modals.Close('#attach-media-modal');
            $.hood.Alerts.Success("Attached!");
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
};
$.hood.Uploader.Init();

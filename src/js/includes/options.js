if (!$.hood)
    $.hood = {};
$.hood.Options = {
    Init: function () {
        $('body').on('click', '.update-site-setting', this.Controls.UpdateSetting);
        $('body').on('click', '.manage-options-click', this.Manage.Refresh);
        $('body').on('change', '.manage-options-change', this.Manage.Refresh);
        if ($('#manage-options-list').doesExist())
            this.Manage.Init();
    },
    Manage: {
        Init: function () {
            this.Datasource = new kendo.data.DataSource({
                transport: {
                    read: {
                        url: "/admin/options/get",
                        dataType: "json",
                        data: function () {
                            return {
                                search: $('#manage-options-search').val(),
                                sort: $('#manage-options-sort').val(),
                                type: $('#manage-options-type').val()
                            };
                        },
                        contentType: "application/x-www-form-urlencoded"
                    },
                    update: {
                        url: "/admin/options/update",
                        type: "POST",
                        contentType: "application/x-www-form-urlencoded"
                    },
                    destroy: {
                        url: "/admin/options/delete",
                        type: "POST",
                        contentType: "application/x-www-form-urlencoded"
                    },
                    create: {
                        type: "POST",
                        url: "/admin/options/add",
                        contentType: "application/x-www-form-urlencoded"
                    }
                },
                batch: true,
                pageSize: 20,
                serverFiltering: false,
                serverPaging: true,
                schema: {
                    model: {
                        id: "Id",
                        fields: {
                            Id: { validation: { required: true } },
                            Value: { validation: { required: true } }
                        }
                    },
                    data: function (response) {
                        return response.Data;
                    },
                    total: function (response) {
                        return response.Count;
                    }
                }
            });
            $("#manage-options-list").kendoGrid({
                dataSource: this.Datasource,
                pageable: true,
                columns: [
                    { field: "Id", title: "Key", width: "25%" },
                    { field: "Value", title: "Value" },
                    { command: ["edit", "destroy"], title: "&nbsp;", width: "250px" }],
                editable: "inline"
            });
        },
        Refresh: function () {
            $("#manage-options-list").data('kendoGrid').dataSource.read();
        }
    },
    Controls: {
        UpdateSetting: function (e) {
            $this = this;
            $content = $($this).html();
            $($this).removeClass('btn-primary').addClass('btn-default').html('<i class="fa fa-refresh fa-spin"></i>&nbsp;Loading...');
            $.post('/admin/options/set/', { name: $($this).data('name'), value: $($this).data('value') }, function (data) {
                if (data.Success) {
                    $.hood.Alerts.Warning("Refreshing now, please wait...", "Saved.");
                    window.location = window.location;
                }
                else {
                    $.hood.Alerts.Warning(data.Errors, "Error Processing");
                }
            })
            .done(function () {

            })
            .fail(function () {
                $.hood.Alerts.Warning("There was an error communicating with the server.");
            })
            .always(function () {
                $($this).removeClass('btn-default').addClass('btn-primary').html($content);
            });
        }
    }
};
$.hood.Options.Init();

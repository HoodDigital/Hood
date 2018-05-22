if (!$.hood)
    $.hood = {}
$.hood.DataList = function (element, options) {
    this.Options = $.extend({
        element: element,
        url: '/',
        params: function () {  },
        pageSize: 12,
        pagers: '.pagers',
        template: '#template',
        dataBound: function () {  },
        refreshOnChange: "",
        refreshOnClick: "",
        serverAction: "GET", 
        schema: {
            data: function (response) {
                return response.Data; 
            },
            total: function (response) {
                return response.Count;
            },
        }
    }, options || {});

    this.DataSource = new kendo.data.DataSource({
        type: "json",
        transport: {
            read: {
                url: this.Options.url,
                type: this.Options.serverAction,
                data: eval(this.Options.params),
                contentType: "application/x-www-form-urlencoded"
            }
        },
        pageSize: this.Options.pageSize,
        serverFiltering: false,
        serverPaging: true,
        schema: this.Options.schema,
    });

    this.List = $(this.Options.element).kendoListView({
        dataSource: this.DataSource,
        template: kendo.template($(this.Options.template).html()),
        dataBound: this.Options.dataBound
    });

    $(this.Options.pagers).hoodPager({
        dataSource: this.DataSource
    });

    this.Refresh = function () {
        //this.DataSource.filter(eval(this.Options.params));
        this.DataSource.read();
    }
    $(this.Options.refreshOnClick).off('click');
    $(this.Options.refreshOnChange).off('change');
    $('body').on('click', this.Options.refreshOnClick, $.proxy(function (e) {
        this.Refresh();
    }, this));
    $('body').on('change', this.Options.refreshOnChange, $.proxy(function (e) {
        this.Refresh();
    }, this));

}
$.fn.hoodDataList = function (options) {
    return this.each(function () {
        var element = $(this);
        // Return early if this element already has a plugin instance
        if (element.data('hoodDataList')) return;
        // pass options to plugin constructor
        var hoodDataList = new $.hood.DataList(this, options);
        // Store plugin object in this element's data
        element.data('hoodDataList', hoodDataList);
    });
};
$(document).on('ready', function () {
    $('.hood-data').each(function () {
        $(this).hoodDataList({
            url: $(this).data('url'),
            params: $(this).data('filters') ? $(this).data('filters') : {},
            pageSize: $(this).data('pagesize') ? $(this).data('pagesize') : 12,
            refreshOnChange: $(this).data('onchange') ? $(this).data('onchange') : "",
            refreshOnClick: $(this).data('onclick') ? $(this).data('onclick') : "",
            pagers: $(this).data('pagers') ? $(this).data('pagers') : {},
            template: $(this).data('template') ? $(this).data('template') : {},
            dataBound: $(this).data('databound') ? $(this).data('databound') : function () { }
        });
    });
});

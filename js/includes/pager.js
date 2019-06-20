if (!$.hood)
    $.hood = {}
$.hood.Pager = function (element, options) {
    this.Options = $.extend({
        element: element,
        pageCount: 0,
        dataSource: null,
        selectedClass: 'active',
        defaultClass: '',
        textClass: '',
        baseClass: 'pagination pagination-sm',
        noItemTemplate: '<span class="no-items">No items to display.</span>'
    }, options || {});
    if (this.Options.dataSource != null)
        this.Options.dataSource.bind("change", $.proxy(function (e) {
            // get page count
            var data = e.items;
            this.Options.pageCount = Math.ceil(this.Options.dataSource._total / this.Options.dataSource._pageSize);
            if (data.length / this.Options.dataSource._pageSize > this.Options.pageCount)
                this.Options.pageCount++;

            if (data.length == 0) {
                $(this.Options.element).empty()
                .append(this.Options.noItemTemplate);
                return;
            }
            // add page info
            startItem = this.Options.dataSource._pageSize * (this.Options.dataSource._page - 1);
            ts = (startItem + this.Options.dataSource._pageSize);
            if (ts > this.Options.dataSource._total)
                ts = this.Options.dataSource._total;
            $(this.Options.element).empty()
                .append('<ul class="' + this.Options.baseClass + ' page-numbers"></ul>');

            // get page bounds
            min = this.Options.dataSource._page - 2;
            if (min < 1) min = 1;
            max = this.Options.dataSource._page + 2;
            if (max > this.Options.pageCount) max = this.Options.pageCount;
            prev = this.Options.dataSource._page - 1;
            if (prev < 1) prev = 1;
            next = this.Options.dataSource._page + 1;

            // construct links
            if (next > this.Options.pageCount) next = this.Options.pageCount;
            $(this.Options.element).find('.page-numbers').append('<li><a href="javascript:void(0);" class="page-link ' + this.Options.defaultClass + ' " data-page="' + prev + '"><i class="fa fa-angle-left"></i></a></li>');
            for (i = min; i <= max; i++) {
                active = this.Options.defaultClass;
                if (this.Options.dataSource._page == i) {
                    active = this.Options.selectedClass;
                }
                $(this.Options.element).find('.page-numbers').append('<li class="' + active + '"><a href="javascript:void(0);" class="page-link" data-page="' + i + '">' + i + '</a></li>');
            }
            $(this.Options.element).find('.page-numbers').append('<li><a href="javascript:void(0);" class="page-link ' + this.Options.defaultClass + '" data-page="' + next + '"><i class="fa fa-angle-right"></i></a></li>');

            // bind click functions
            $(this.Options.element).find('.page-link').click($.proxy(function (e) {
                this.Options.dataSource.page($(e.target).data('page'));
            }, this));
        }, this));
}

$.fn.hoodPager = function (options) {
    return this.each(function () {
        var element = $(this);
        // Return early if this element already has a plugin instance
        if (element.data('hoodPager')) return;
        // pass options to plugin constructor
        var hoodPager = new $.hood.Pager(this, options);
        // Store plugin object in this element's data
        element.data('hoodPager', hoodPager);
    });
};

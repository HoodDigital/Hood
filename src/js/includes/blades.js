if (!$.hood)
    $.hood = {};
$.hood.Blades = {
    Init: function () {
        $('body').on('click', '.blade', $.hood.Blades.Open);
        $('body').on('click', '.close-blade', $.hood.Blades.Close);
    },
    Loading: false,
    Open: function (e) {
        e.preventDefault();
        if ($.hood.Blades.Loading)
            return;
        $.hood.Blades.Loading = true;
        $(this).data('temp', $(this).html());
        $(this).addClass('loading').append('<i class="fa fa-refresh fa-spin margin-left-5"></i>');
        // get in the create user blade content
        $.hood.Blades.OpenBlade($(this).attr('href'), $.proxy(function () {
            eval($(this).data('complete'));
            $(this).removeClass('loading').html($(this).data('temp'));
        }, this));
    },
    OpenWithLoader: function (button, url, complete) {
        if ($.hood.Blades.Loading)
            return;
        $.hood.Blades.Loading = true;
        $(button).data('temp', $(button).html());
        $(button).addClass('loading').append('<i class="fa fa-refresh fa-spin m-l-sm"></i>');
        // get in the create user blade content
        this.OpenBlade(url, function () {
            if (complete !== null)
                complete();
            $(button).removeClass('loading').html($(button).data('temp'));
        });
    },
    OpenBlade: function (url, complete) {
        $('#right-sidebar').removeClass('animate-all sidebar-open');
        $('#right-sidebar').addClass('animate-all');
        // get in the create user blade content
        $.get(url, null, function (data) {
            // load and slide the blade
            $('#right-sidebar').empty();
            $('#right-sidebar').html(data);
            $('#right-sidebar').addClass('sidebar-open');
            $.hood.Helpers.ResetSidebarScroll();  
            if (!$.hood.Helpers.IsNullOrUndefined(complete)) {
                if ($.hood.Helpers.IsFunction(complete))
                    complete(data);
                else
                    eval(complete);
            }
        })
        .fail(function (data) {
            $.hood.Alerts.Error("There was an error loading the inline panel's URL:<br/><strong>" + urlLoad + "</strong>");
        })
        .always(function (data) {
            $.hood.Blades.Loading = false;
        });
    },
    Close: function (e) {
        if (!$.hood.Helpers.IsNullOrUndefined(e)) 
            e.preventDefault();
        if ($.hood.Blades.Loading)
            return;
        $('#right-sidebar').removeClass('sidebar-open');
        setTimeout(function () {
            $('#right-sidebar').empty();
            $.hood.Blades.Loading = false;
        }, 1000);
    }, 
    Reload: function () {
        if ($('#right-sidebar').hasClass('sidebar-open'))
        {
            // if blade is open, then refresh it.
            $.hood.Blades.OpenBlade($('#right-sidebar').data('url'), $('#right-sidebar').data('complete'));
        }
    }
};
$.hood.Blades.Init();
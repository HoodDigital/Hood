if (!$.hood)
    $.hood = {}
$.hood.Observable = function (element, options) {
    this.Options = $.extend({
        element: element,
        load: '/',
        save: '/',
        syncFunction: function () { },
        changeFunction: function () {
            this.modelSource.options.change();
        },
        evalSynced: false
    }, options || {});
    this.syncFunction = this.Options.syncFunction,
    this.evalSynced = this.Options.evalSynced,
    this.viewModel = kendo.observable({
        modelSource: new kendo.data.DataSource({
            transport: {
                read: {
                    url: this.Options.load,
                    dataType: "json"
                },
                update: {
                    url: this.Options.save,
                    type: "POST",
                    contentType: "application/x-www-form-urlencoded"
                },
                parameterMap: function (options, operation) {
                    if (operation !== "read" && options.models) {
                        return {
                            models: options.models
                        };
                        return options;
                    }
                }
            },
            batch: true,
            schema: {
                model: {
                    id: "Id"
                }
            },
            change: $.proxy(function () {
                this.viewModel.selectedModel = this.viewModel.modelSource.view()[0];
                kendo.bind(this.Options.element, this.viewModel);
            }, this),
            requestEnd: $.proxy(function (e) {
                if (e.response.Success != null)
                    if (e.response.Success) {
                        $.hood.Alerts.Success('Saved.');
                        if (this.evalSynced) {
                            eval(this.syncFunction);
                        } else {
                            this.syncFunction();
                        }
                    }
                    else
                        $.hood.Alerts.Error(e.response.Errors, "An error occurred...");
            }, this)
        }),
        selectedModel: null,
        getUrl: function () {
            return "/" + this.selectedModel.Slug;
        },
        hasChanges: false,
        save: function () {
            this.changeFunction();
            this.modelSource.sync();
            this.set("hasChanges", false);
        },
        showForm: function () {
            return this.get("selectedModel") !== null;
        },
        change: function (e) {
            this.set("hasChanges", true);
        },
        changeFunction: this.Options.changeFunction
    });
    this.viewModel.modelSource.read();
    this.Change = function () {
        this.viewModel.changeFunction();
    }
}
$.fn.hoodObservable = function (options) {
    return this.each(function () {
        var element = $(this);
        // Return early if this element already has a plugin instance
        if (element.data('hoodObservable')) return;
        // pass options to plugin constructor
        var hoodObservable = new $.hood.Observable(this, options);
        // Store plugin object in this element's data
        element.data('hoodObservable', hoodObservable);
    });
};
$(document).ready(function () {
    $('.hood-observable').each(function () {
        $(this).hoodObservable({
            load: $(this).data('load'),
            save: $(this).data('save'),
            synced: $(this).data('synced'),
            evalSynced: true
        });
    });
});


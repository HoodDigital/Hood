var hood = (function (exports, google_maps, Swal, bootstrap) {
    'use strict';

    function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

    var Swal__default = /*#__PURE__*/_interopDefaultLegacy(Swal);

    /*! *****************************************************************************
    Copyright (c) Microsoft Corporation.

    Permission to use, copy, modify, and/or distribute this software for any
    purpose with or without fee is hereby granted.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
    REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
    AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
    INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
    LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
    OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
    PERFORMANCE OF THIS SOFTWARE.
    ***************************************************************************** */
    /* global Reflect, Promise */

    var extendStatics = function(d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };

    function __extends(d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    }

    var __assign = function() {
        __assign = Object.assign || function __assign(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };

    "function"!=typeof Object.create&&(Object.create=function(t){function o(){}return o.prototype=t,new o}),function(t,o,i,s){var n={_positionClasses:["bottom-left","bottom-right","top-right","top-left","bottom-center","top-center","mid-center"],_defaultIcons:["success","error","info","warning"],init:function(o,i){this.prepareOptions(o,t.toast.options),this.process();},prepareOptions:function(o,i){var s={};"string"==typeof o||o instanceof Array?s.text=o:s=o,this.options=t.extend({},i,s);},process:function(){this.setup(),this.addToDom(),this.position(),this.bindToast(),this.animate();},setup:function(){var o="";if(this._toastEl=this._toastEl||t("<div></div>",{"class":"jq-toast-single"}),o+='<span class="jq-toast-loader"></span>',this.options.allowToastClose&&(o+='<span class="close-jq-toast-single">&times;</span>'),this.options.text instanceof Array){this.options.heading&&(o+='<h2 class="jq-toast-heading">'+this.options.heading+"</h2>"),o+='<ul class="jq-toast-ul">';for(var i=0;i<this.options.text.length;i++)o+='<li class="jq-toast-li" id="jq-toast-item-'+i+'">'+this.options.text[i]+"</li>";o+="</ul>";}else this.options.heading&&(o+='<h2 class="jq-toast-heading">'+this.options.heading+"</h2>"),o+=this.options.text;this._toastEl.html(o),this.options.bgColor!==!1&&this._toastEl.css("background-color",this.options.bgColor),this.options.textColor!==!1&&this._toastEl.css("color",this.options.textColor),this.options.textAlign&&this._toastEl.css("text-align",this.options.textAlign),this.options.icon!==!1&&(this._toastEl.addClass("jq-has-icon"),-1!==t.inArray(this.options.icon,this._defaultIcons)&&this._toastEl.addClass("jq-icon-"+this.options.icon)),this.options["class"]!==!1&&this._toastEl.addClass(this.options["class"]);},position:function(){"string"==typeof this.options.position&&-1!==t.inArray(this.options.position,this._positionClasses)?"bottom-center"===this.options.position?this._container.css({left:t(o).outerWidth()/2-this._container.outerWidth()/2,bottom:20}):"top-center"===this.options.position?this._container.css({left:t(o).outerWidth()/2-this._container.outerWidth()/2,top:20}):"mid-center"===this.options.position?this._container.css({left:t(o).outerWidth()/2-this._container.outerWidth()/2,top:t(o).outerHeight()/2-this._container.outerHeight()/2}):this._container.addClass(this.options.position):"object"==typeof this.options.position?this._container.css({top:this.options.position.top?this.options.position.top:"auto",bottom:this.options.position.bottom?this.options.position.bottom:"auto",left:this.options.position.left?this.options.position.left:"auto",right:this.options.position.right?this.options.position.right:"auto"}):this._container.addClass("bottom-left");},bindToast:function(){var t=this;this._toastEl.on("afterShown",function(){t.processLoader();}),this._toastEl.find(".close-jq-toast-single").on("click",function(o){o.preventDefault(),"fade"===t.options.showHideTransition?(t._toastEl.trigger("beforeHide"),t._toastEl.fadeOut(function(){t._toastEl.trigger("afterHidden");})):"slide"===t.options.showHideTransition?(t._toastEl.trigger("beforeHide"),t._toastEl.slideUp(function(){t._toastEl.trigger("afterHidden");})):(t._toastEl.trigger("beforeHide"),t._toastEl.hide(function(){t._toastEl.trigger("afterHidden");}));}),"function"==typeof this.options.beforeShow&&this._toastEl.on("beforeShow",function(){t.options.beforeShow();}),"function"==typeof this.options.afterShown&&this._toastEl.on("afterShown",function(){t.options.afterShown();}),"function"==typeof this.options.beforeHide&&this._toastEl.on("beforeHide",function(){t.options.beforeHide();}),"function"==typeof this.options.afterHidden&&this._toastEl.on("afterHidden",function(){t.options.afterHidden();});},addToDom:function(){var o=t(".jq-toast-wrap");if(0===o.length?(o=t("<div></div>",{"class":"jq-toast-wrap"}),t("body").append(o)):(!this.options.stack||isNaN(parseInt(this.options.stack,10)))&&o.empty(),o.find(".jq-toast-single:hidden").remove(),o.append(this._toastEl),this.options.stack&&!isNaN(parseInt(this.options.stack),10)){var i=o.find(".jq-toast-single").length,s=i-this.options.stack;s>0&&t(".jq-toast-wrap").find(".jq-toast-single").slice(0,s).remove();}this._container=o;},canAutoHide:function(){return this.options.hideAfter!==!1&&!isNaN(parseInt(this.options.hideAfter,10))},processLoader:function(){if(!this.canAutoHide()||this.options.loader===!1)return !1;var t=this._toastEl.find(".jq-toast-loader"),o=(this.options.hideAfter-400)/1e3+"s",i=this.options.loaderBg,s=t.attr("style")||"";s=s.substring(0,s.indexOf("-webkit-transition")),s+="-webkit-transition: width "+o+" ease-in;                       -o-transition: width "+o+" ease-in;                       transition: width "+o+" ease-in;                       background-color: "+i+";",t.attr("style",s).addClass("jq-toast-loaded");},animate:function(){var t=this;if(this._toastEl.hide(),this._toastEl.trigger("beforeShow"),"fade"===this.options.showHideTransition.toLowerCase()?this._toastEl.fadeIn(function(){t._toastEl.trigger("afterShown");}):"slide"===this.options.showHideTransition.toLowerCase()?this._toastEl.slideDown(function(){t._toastEl.trigger("afterShown");}):this._toastEl.show(function(){t._toastEl.trigger("afterShown");}),this.canAutoHide()){var t=this;o.setTimeout(function(){"fade"===t.options.showHideTransition.toLowerCase()?(t._toastEl.trigger("beforeHide"),t._toastEl.fadeOut(function(){t._toastEl.trigger("afterHidden");})):"slide"===t.options.showHideTransition.toLowerCase()?(t._toastEl.trigger("beforeHide"),t._toastEl.slideUp(function(){t._toastEl.trigger("afterHidden");})):(t._toastEl.trigger("beforeHide"),t._toastEl.hide(function(){t._toastEl.trigger("afterHidden");}));},this.options.hideAfter);}},reset:function(o){"all"===o?t(".jq-toast-wrap").remove():this._toastEl.remove();},update:function(t){this.prepareOptions(t,this.options),this.setup(),this.bindToast();}};t.toast=function(t){var o=Object.create(n);return o.init(t,this),{reset:function(t){o.reset(t);},update:function(t){o.update(t);}}},t.toast.options={text:"",heading:"",showHideTransition:"fade",allowToastClose:!0,hideAfter:3e3,loader:!0,loaderBg:"#9EC600",stack:5,position:"bottom-left",bgColor:!1,textColor:!1,textAlign:"left",icon:!1,beforeShow:function(){},afterShown:function(){},beforeHide:function(){},afterHidden:function(){}};}(jQuery,window);

    var BootstrapSwal = Swal__default['default'].mixin({
        customClass: {
            confirmButton: 'btn btn-success btn-lg m-1 pl-4 pr-4',
            cancelButton: 'btn btn-danger btn-lg m-1'
        },
        buttonsStyling: false
    });
    var Alerts = /** @class */ (function () {
        function Alerts() {
        }
        Alerts.log = function (message, type) {
            if (type === void 0) { type = 'message'; }
            if (!document.body.classList.contains('dev-mode')) {
                return;
            }
            switch (type) {
                case 'error':
                    console.error(message);
                    break;
                case 'message':
                    console.log(message);
                    break;
                case 'warning':
                    console.warn(message);
                    break;
                case 'info':
                    console.info(message);
                    break;
            }
        };
        Alerts.error = function (message, title, hideAfter) {
            if (title === void 0) { title = null; }
            if (hideAfter === void 0) { hideAfter = null; }
            $.toast({
                heading: title,
                text: message,
                icon: 'error',
                position: 'bottom-left',
                loader: false,
                bgColor: '#d0100b',
                textColor: 'white',
                hideAfter: hideAfter
            });
        };
        Alerts.warning = function (message, title, hideAfter) {
            if (title === void 0) { title = null; }
            if (hideAfter === void 0) { hideAfter = null; }
            $.toast({
                heading: title,
                text: message,
                icon: 'error',
                position: 'bottom-left',
                loader: false,
                bgColor: '#ef9007',
                textColor: 'white',
                hideAfter: hideAfter
            });
        };
        Alerts.message = function (message, title, hideAfter) {
            if (title === void 0) { title = null; }
            if (hideAfter === void 0) { hideAfter = null; }
            $.toast({
                heading: title,
                text: message,
                icon: 'error',
                position: 'bottom-left',
                loader: false,
                bgColor: '#222222',
                textColor: 'white',
                hideAfter: hideAfter
            });
        };
        Alerts.success = function (message, title, hideAfter) {
            if (title === void 0) { title = null; }
            if (hideAfter === void 0) { hideAfter = null; }
            $.toast({
                heading: title,
                text: message,
                icon: 'error',
                position: 'bottom-left',
                loader: false,
                bgColor: '#28a745',
                textColor: 'white',
                hideAfter: hideAfter
            });
        };
        Alerts.alert = function (message, title, icon, hideAfter) {
            if (title === void 0) { title = null; }
            if (icon === void 0) { icon = 'info'; }
            if (hideAfter === void 0) { hideAfter = 10000; }
            switch (icon) {
                case 'error':
                    Alerts.error(message, title, hideAfter);
                    break;
                case 'warning':
                    Alerts.warning(message, title, hideAfter);
                    break;
                case 'info':
                    Alerts.message(message, title, hideAfter);
                    break;
                case 'success':
                    Alerts.success(message, title, hideAfter);
                    break;
            }
        };
        Alerts.sweetAlert = function (options, callback) {
            BootstrapSwal.fire(options).then(function (result) {
                callback(result);
            });
        };
        Alerts.confirm = function (options, callback) {
            var baseOptions = {
                showCancelButton: true,
                footer: '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>',
                title: 'Are you sure?',
                html: 'Are you sure you want to do this?',
                confirmButtonText: 'Ok',
                cancelButtonText: 'Cancel'
            };
            Alerts.sweetAlert(__assign(__assign({}, baseOptions), options), callback);
        };
        Alerts.prompt = function (options, callback) {
            var baseOptions = {
                input: 'text',
                inputAttributes: {
                    autocapitalize: 'off'
                },
                showCancelButton: true,
                icon: 'info',
                footer: '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>',
                confirmButtonText: 'Ok',
                cancelButtonText: 'Cancel'
            };
            Alerts.sweetAlert(__assign(__assign({}, baseOptions), options), callback);
        };
        return Alerts;
    }());

    var Handlers = /** @class */ (function () {
        function Handlers() {
        }
        Handlers.prototype.initDefaultHandlers = function () {
            this.checkboxToCsvInput();
            this.iconSelector();
            this.initSelectValues();
            this.scrollHandlers();
            this.selectText();
            this.setValueOnClick();
            this.submitOnChange();
        };
        /**
         * Sets values of any selects that have the value set in data-selected, useful for
         */
        Handlers.prototype.initSelectValues = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).find('select[data-selected]').each(this.initSelectValuesHandler);
        };
        Handlers.prototype.initSelectValuesHandler = function (index, element) {
            var sel = $(this).data('selected');
            if (sel !== 'undefined' && sel !== '') {
                var selected = String(sel);
                $(this).val(selected);
            }
        };
        /**
         * Sets up any Hood Icon selector fields, requires the correct HTML setup.
         */
        Handlers.prototype.iconSelector = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).find('[data-hood-icon]').each(this.iconSelectorHandler);
        };
        Handlers.prototype.iconSelectorHandler = function (index, element) {
            var input = $(this).find('input[data-hood-icon-input]');
            var display = $(this).find('[data-hood-icon-display]');
            var collapse = $(this).find('.collapse');
            $(this).find('[data-hood-icon-key][data-hood-icon-value]').on('click', function () {
                var key = $(this).data('hoodIconKey');
                var value = $(this).data('hoodIconValue');
                display.html(value);
                input.val(key);
                if (collapse) {
                    collapse.removeClass('show');
                }
            });
        };
        /**
         * Submits the form when input is changed, mark inputs with .submit-on-change class.
         */
        Handlers.prototype.selectText = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).on('click', '.select-text', this.selectTextHandler);
        };
        Handlers.prototype.selectTextHandler = function () {
            var $this = $(this);
            $this.select();
            // Work around Chrome's little problem
            $this.mouseup(function () {
                // Prevent further mouseup intervention
                $this.unbind("mouseup");
                return false;
            });
        };
        /**
         * Attaches handlers for default scrolling functions, scroll to top, scroll to target (with header.header offset calculated)
         * and scroll to target direct (with no calculated offset).
         */
        Handlers.prototype.scrollHandlers = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).on('click', '.scroll-top, .scroll-to-top', this.scrollTop);
            $(tag).on('click', '.scroll-target, .scroll-to-target', this.scrollTarget);
            $(tag).on('click', '.scroll-target-direct, .scroll-to-target-direct', this.scrollTargetDirect);
        };
        Handlers.prototype.scrollTop = function (e) {
            if (e)
                e.preventDefault();
            $('html, body').animate({ scrollTop: 0 }, 800);
            return false;
        };
        Handlers.prototype.scrollTarget = function (e) {
            if (e)
                e.preventDefault();
            var url = $(this).attr('href').split('#')[0];
            if (url !== window.location.pathname && url !== "") {
                return;
            }
            var target = this.hash;
            var $target = $(target);
            var $header = $('header.header');
            var headerOffset = 0;
            if ($header) {
                headerOffset = $header.height();
            }
            if ($(this).data('offset'))
                $('html, body').stop().animate({
                    'scrollTop': $target.offset().top - $(this).data('offset')
                }, 900, 'swing');
            else
                $('html, body').stop().animate({
                    'scrollTop': $target.offset().top - headerOffset
                }, 900, 'swing');
        };
        Handlers.prototype.scrollTargetDirect = function () {
            var scrollTop = $('body').scrollTop();
            var top = $($(this).attr('href')).offset().top;
            $('html, body').animate({
                scrollTop: top
            }, Math.abs(top - scrollTop));
            return false;
        };
        /**
        * Compiles any selected checkboxes with matching data-hood-csv-input tags,
        * then saves the CSV list of the values to the input given in the tag.
        */
        Handlers.prototype.checkboxToCsvInput = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).on('change', 'input[type=checkbox][data-hood-csv-input]', this.checkboxToCsvInputHandler);
        };
        Handlers.prototype.checkboxToCsvInputHandler = function (e) {
            if (e)
                e.preventDefault();
            // when i change - create an array, with any other checked of the same data-input checkboxes. and add to the data-input referenced tag.
            var items = new Array();
            $('input[data-hood-csv-input="' + $(this).data('hoodCsvInput') + '"]').each(function () {
                if ($(this).is(":checked"))
                    items.push($(this).val());
            });
            var id = '#' + $(this).data('input');
            var vals = JSON.stringify(items);
            $(id).val(vals);
        };
        /**
        * Submits the form when input is changed, mark inputs with .submit-on-change class.
        */
        Handlers.prototype.submitOnChange = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).on('change', '.submit-on-change', this.submitOnChangeHandler);
        };
        Handlers.prototype.submitOnChangeHandler = function (e) {
            if (e)
                e.preventDefault();
            $(this).parents('form').submit();
        };
        /**
        * Sets the value of the input [data-target] when clicked to the value in [data-value]
        */
        Handlers.prototype.setValueOnClick = function (tag) {
            if (tag === void 0) { tag = 'body'; }
            $(tag).on('click', '.click-select[data-target][data-value]', this.setValueOnClickHandler);
        };
        Handlers.prototype.setValueOnClickHandler = function () {
            var $this = $(this);
            var targetId = '#' + $this.data('target');
            $(targetId).val($this.data('value'));
            $(targetId).trigger('change');
            $('.click-select.clean[data-target="' + $this.data('target') + '"]').each(function () { $(this).removeClass('active'); });
            $('.click-select.clean[data-target="' + $this.data('target') + '"][data-value="' + $this.data('value') + '"]').each(function () { $(this).addClass('active'); });
        };
        return Handlers;
    }());

    var Response = /** @class */ (function () {
        function Response() {
        }
        Response.process = function (data, autoHide) {
            if (autoHide === void 0) { autoHide = 5000; }
            if (data.success) {
                Alerts.success(data.message, data.title, autoHide);
            }
            else {
                Alerts.error(data.errors, data.title, autoHide);
            }
        };
        return Response;
    }());

    var Inline = /** @class */ (function () {
        function Inline() {
        }
        Inline.load = function (tag, options) {
            var _a;
            var $tag = $(tag);
            $tag.addClass('loading');
            if (options.onLoad) {
                options.onLoad(tag);
            }
            var url = $tag.data('url');
            $.get(url, function (data) {
                if (options.onRender) {
                    data = options.onRender(data, tag);
                }
                $tag.html(data);
                $tag.removeClass('loading');
                if (options.onComplete) {
                    options.onComplete(data, tag);
                }
            })
                .fail((_a = options.onError) !== null && _a !== void 0 ? _a : Inline.handleError);
        };
        Inline.task = function (url, sender, complete, error) {
            if (complete === void 0) { complete = null; }
            if (error === void 0) { error = null; }
            if (sender) {
                sender.classList.add('loading');
            }
            $.get(url, function (response) {
                if (sender) {
                    sender.classList.remove('loading');
                }
                if (complete) {
                    complete(response, sender);
                }
            })
                .fail(error !== null && error !== void 0 ? error : Inline.handleError);
        };
        Inline.post = function (url, sender, complete, error) {
            if (complete === void 0) { complete = null; }
            if (error === void 0) { error = null; }
            if (sender) {
                sender.classList.add('loading');
            }
            $.post(url, function (response) {
                if (sender) {
                    sender.classList.remove('loading');
                }
                if (complete) {
                    complete(response, sender);
                }
            })
                .fail(error !== null && error !== void 0 ? error : Inline.handleError);
        };
        Inline.handleError = function (xhr) {
            if (xhr.status === 500) {
                Alerts.error("There was an error processing the content, please contact an administrator if this continues.", "Error " + xhr.status);
            }
            else if (xhr.status === 404) {
                Alerts.error("The content could not be found.", "Error " + xhr.status);
            }
            else if (xhr.status === 401) {
                Alerts.error("You are not allowed to view this resource, are you logged in correctly?", "Error " + xhr.status);
                window.location = window.location;
            }
        };
        return Inline;
    }());

    /**
      * Attach a data list feed to an HTML element. The element must have a data-url attribute to connect to a feed.
      */
    var DataList = /** @class */ (function () {
        /**
          * @param element The datalist element. The element must have a data-url attribute to connect to a feed.
          */
        function DataList(element, options) {
            this.element = element;
            this.element.hoodDataList = this;
            if (typeof (element) == 'undefined' || element == null) {
                Alerts.log('Could not DataList to element, element does not exist.', 'error');
                return;
            }
            this.options = __assign(__assign({}, this.options), options);
            if ($(this.element).hasClass('query')) {
                var pageUrl = $(this.element).data('url') + window.location.search;
                $(this.element).attr('data-url', pageUrl);
                $(this.element).data('url', pageUrl);
            }
            if (!$(this.element).hasClass('refresh-only')) {
                var listUrl = document.createElement('a');
                listUrl.href = $(this.element).data('url');
                this.reload(new URL(listUrl.href));
            }
            $(this.element).on('click', '.pagination a, a.hood-inline-list-target', function (e) {
                e.preventDefault();
                var url = document.createElement('a');
                url.href = e.currentTarget.href;
                var listUrl = document.createElement('a');
                listUrl.href = $(this.element).data('url');
                listUrl.search = url.search;
                this.reload(new URL(listUrl.href));
            }.bind(this));
            $('body').on('submit', "form.inline[data-target=\"#" + this.element.id + "\"]", function (e) {
                e.preventDefault();
                var $form = $(e.currentTarget);
                var listUrl = document.createElement('a');
                listUrl.href = $(this.element).data('url');
                listUrl.search = "?" + $form.serialize();
                this.reload(new URL(listUrl.href));
            }.bind(this));
            //    $('body').on('submit', '.hood-inline-list form', function (e) {
            //        e.preventDefault();
            //        $.hood.Loader(true);
            //        let $form = $(this);
            //        let $list = $form.parents('.hood-inline-list');
            //        var url = document.createElement('a');
            //        url.href = $list.data('url');
            //        url.search = "?" + $form.serialize();
            //        $.hood.Inline.DataList.Reload($list, url);
            //    });
        }
        DataList.prototype.reload = function (url) {
            if (url === void 0) { url = null; }
            if (url) {
                if (history.pushState && $(this.element).hasClass('query')) {
                    var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + (url.href.contains('?') ? "?" + url.href.substring(url.href.indexOf('?') + 1) : '');
                    window.history.pushState({ path: newurl }, '', newurl);
                }
                $(this.element).data('url', url);
            }
            Inline.load(this.element, __assign({}, this.options));
        };
        return DataList;
    }());

    var ModalController = /** @class */ (function () {
        function ModalController(options) {
            if (options === void 0) { options = null; }
            this.options = {
                closePrevious: true
            };
            this.options = __assign(__assign({}, this.options), options);
        }
        ModalController.prototype.show = function (url, sender) {
            var _a;
            if (this.options.onLoad) {
                this.options.onLoad(this.element);
            }
            $.get(url, function (data) {
                if (this.modal && this.options.closePrevious) {
                    this.close();
                }
                if (this.options.onRender) {
                    data = this.options.onRender(this.element, data);
                }
                this.element = this.createElementFromHTML(data);
                this.element.classList.add('hood-inline-modal');
                $('body').append(this.element);
                this.modal = new bootstrap.Modal(this.element, {});
                this.modal.show();
                // Workaround for sweetalert popups.
                this.element.addEventListener('shown.bs.modal', function () {
                    $(document).off('focusin.modal');
                }.bind(this));
                this.element.addEventListener('hidden.bs.modal', function () {
                    this.dispose();
                }.bind(this));
                if (this.options.onComplete) {
                    this.options.onComplete(this.element);
                }
            }.bind(this))
                .fail((_a = this.options.onError) !== null && _a !== void 0 ? _a : Inline.handleError);
        };
        ModalController.prototype.close = function () {
            if (this.modal) {
                this.modal.hide();
            }
        };
        ModalController.prototype.dispose = function () {
            this.modal.dispose();
            this.element.remove();
        };
        ModalController.prototype.createElementFromHTML = function (htmlString) {
            var div = document.createElement('div');
            div.innerHTML = htmlString.trim();
            // Change this to div.childNodes to support multiple top-level nodes
            return div.firstChild;
        };
        return ModalController;
    }());

    var Validator = /** @class */ (function () {
        /**
          * @param element The datalist element. The element must have a data-url attribute to connect to a feed.
          */
        function Validator(element, options) {
            this.options = {
                errorAlert: 'There are errors, please check the form.'
            };
            this.element = element;
            if (!this.element) {
                return;
            }
            this.options.serializationFunction = function () {
                var rtn = $(this.element).serialize();
                return rtn;
            }.bind(this);
            this.options = __assign(__assign({}, this.options), options);
            this.element.addEventListener('submit', function (e) {
                e.preventDefault();
                e.stopImmediatePropagation();
                this.submitForm();
            }.bind(this));
        }
        Validator.prototype.submitForm = function () {
            var _a;
            this.element.classList.add('was-validated');
            if (this.element.checkValidity()) {
                this.element.classList.add('loading');
                var checkboxes = this.element.querySelector('input[type=checkbox]');
                if (checkboxes) {
                    Array.prototype.slice.call(checkboxes)
                        .forEach(function (checkbox) {
                        if ($(this).is(':checked')) {
                            $(this).val('true');
                        }
                    });
                }
                if (this.options.onSubmit) {
                    this.options.onSubmit(this);
                }
                var formData = this.options.serializationFunction();
                $.post(this.element.action, formData, function (data) {
                    if (this.options.onComplete) {
                        this.options.onComplete(data, this);
                    }
                }.bind(this))
                    .fail((_a = this.options.onError) !== null && _a !== void 0 ? _a : Inline.handleError);
            }
            else {
                if (this.options.errorAlert) {
                    Alerts.error(this.options.errorAlert, null, 5000);
                }
            }
        };
        return Validator;
    }());

    var MediaService = /** @class */ (function () {
        function MediaService(element, options) {
            this.options = {
                action: 'show',
                size: 'large'
            };
            this.galleryInitialised = false;
            this.selectedMedia = new Array();
            this.element = element;
            if (!this.element) {
                return;
            }
            this.options = __assign(__assign({}, this.options), options);
            $('body').off('click', '.media-delete', this.delete.bind(this));
            $('body').on('click', '.media-delete', this.delete.bind(this));
            $(this.element).on('click', '.media-item', this.action.bind(this));
            $(this.element).on('click', '.media-create-directory', this.createDirectory.bind(this));
            $(this.element).on('click', '.media-delete-directory', this.deleteDirectory.bind(this));
            this.media = new DataList(this.element, {
                onLoad: this.options.onListLoad,
                onError: this.options.onError,
                onRender: this.options.onListRender,
                onComplete: function (html, sender) {
                    this.initUploader();
                    // if this is gallery type, add the "Add to gallery button" and hook it to the add function
                    if (this.options.action == 'gallery' && !this.galleryInitialised) {
                        $('#media-select-modal .modal-footer').removeClass('d-none');
                        $('#media-select-modal .modal-footer').on('click', this.galleryAdd.bind(this));
                        this.galleryInitialised = true;
                    }
                    if (this.options.onListComplete) {
                        this.options.onListComplete(html, sender);
                    }
                }.bind(this)
            });
        }
        MediaService.prototype.initUploader = function () {
            this.uploadButton = document.getElementById('media-add');
            this.uploader = document.getElementById('media-upload');
            if (!this.uploadButton || !this.uploader)
                return;
            this.progressArea = document.getElementById('media-progress');
            this.progressText = $('<div class="progress-text text-muted mb-3"><i class="fa fa-info-circle me-2"></i>Uploading file <span></span>...</div>');
            this.progress = $('<div class="progress"><div class= "progress-bar progress-bar-striped" role="progressbar" style="width:0%" aria-valuenow="10" aria-valuemin="0" aria-valuemax="100" ></div></div>');
            this.progressText.appendTo(this.progressArea);
            this.progress.appendTo(this.progressArea);
            var dz = null;
            $("#media-upload").dropzone({
                url: $("#media-upload").data('url') + "?directoryId=" + $("#media-list > #upload-directory-id").val(),
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                paramName: 'files',
                acceptedFiles: $("#media-upload").data('types') || ".png,.jpg,.jpeg,.gif",
                autoProcessQueue: true,
                previewsContainer: false,
                clickable: "#media-add",
                dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
                dictResponseError: 'Error while uploading file!',
                init: function () {
                    dz = this;
                }
            });
            dz.on("success", function (file, data) {
                Response.process(data);
            }.bind(this));
            dz.on("addedfile", function (file) {
                this.progress.find('.progress-bar').css({ width: 0 + "%" });
                this.progressText.find('span').html(0 + "%");
            }.bind(this));
            // Update the total progress bar
            dz.on("totaluploadprogress", function (totalProgress, totalBytes, totalBytesSent) {
                this.progress.find('.progress-bar').css({ width: totalProgress + "%" });
                this.progressText.find('span').html(Math.round(totalProgress) + "% - " + totalBytesSent.formatKilobytes() + " / " + totalBytes.formatKilobytes());
            }.bind(this));
            dz.on("sending", function (file) {
                // Show the total progress bar when upload starts
                this.progressArea.classList.remove('collapse');
                this.progress.find('.progress-bar').css({ width: 0 + "%" });
                this.progressText.find('span').html(0 + "%");
            }.bind(this));
            // Hide the total progress bar when nothing's uploading anymore
            dz.on("complete", function (file) {
                this.media.reload();
            }.bind(this));
            // Hide the total progress bar when nothing's uploading anymore
            dz.on("queuecomplete", function () {
                this.progressArea.classList.add('collapse');
                this.media.reload();
            }.bind(this));
        };
        MediaService.prototype.createDirectory = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createDirectoryModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('content-directories-edit-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (response.success) {
                                this.media.reload();
                                createDirectoryModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createDirectoryModal.show($(e.currentTarget).attr('href'), this.element);
        };
        MediaService.prototype.deleteDirectory = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({}, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (sender, response) {
                        // Refresh the list, using the parent directory id - stored in the response's data array.
                        Response.process(response, 5000);
                        if (response.data.length > 0) {
                            var listUrl = document.createElement('a');
                            listUrl.href = $(this.element).data('url');
                            listUrl.search = "?dir=" + response.data[0];
                            this.media.reload(new URL(listUrl.href));
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        MediaService.prototype.uploadUrl = function () {
            return $("#media-upload").data('url') + "?directoryId=" + $("#media-list > #upload-directory-id").val();
        };
        MediaService.prototype.action = function (e) {
            e.preventDefault();
            e.stopPropagation();
            // Load media object from clicked item in the media list
            var mediaObject = $(e.currentTarget).data('json');
            // Perform the chosen action, which is set on the service's options when loaded.
            switch (this.options.action) {
                case 'select':
                    this.select(mediaObject, e);
                    break;
                case 'insert':
                    this.insert(mediaObject, e);
                    break;
                case 'attach':
                    this.attach(mediaObject, e);
                    break;
                case 'gallery':
                    this.galleryClick(mediaObject, e);
                    break;
                default:
                    this.show(mediaObject, e);
                    break;
            }
        };
        MediaService.prototype.show = function (mediaObject, sender) {
            // Load the media as a new blade to display.
            this.currentBlade = new ModalController();
            this.currentBlade.show($(sender.target).data('blade'), sender.target);
            // TODO: On close, reload the list and reinstate the modal??
        };
        MediaService.prototype.select = function (mediaObject, e) {
            Alerts.log("[MediaService.select] Selecting media object id " + mediaObject.id + " - " + mediaObject.filename + " and inserting " + this.options.size + " url to target: " + this.options.target);
            if (this.options.target) {
                var target = $(this.options.target);
                switch (this.options.size) {
                    case 'thumb':
                        target.val(mediaObject.thumbUrl);
                        break;
                    case 'small':
                        target.val(mediaObject.smallUrl);
                        break;
                    case 'medium':
                        target.val(mediaObject.mediumUrl);
                        break;
                    case 'large':
                        target.val(mediaObject.largeUrl);
                        break;
                    case 'full':
                        target.val(mediaObject.url);
                        break;
                }
            }
            if (this.options.refresh) {
                MediaService.refresh(mediaObject, this.options.refresh);
            }
            // Run the callback for onAction
            if (this.options.onAction) {
                this.options.onAction(mediaObject);
            }
        };
        MediaService.prototype.insert = function (mediaObject, e) {
            // basic functionality to insert the correct string from the media response (from uploader) to given input element. 
            Alerts.log("[MediaService.insert] Selecting media object id " + mediaObject.id + " - " + mediaObject.filename + " and inserting " + this.options.size + " image to target editor: " + this.options.target);
            this.options.targetEditor.insertContent('<img alt="' + mediaObject.filename + '" src="' + mediaObject.url + '" class="img-fluid" />');
            // Run the callback for onAction
            if (this.options.onAction) {
                this.options.onAction(mediaObject);
            }
        };
        MediaService.prototype.attach = function (mediaObject, e) {
            // once file is uploaded to given directory, send media id to the given attach endpoint.
            Alerts.log("[MediaService.attach] Attaching media object id " + mediaObject.id + " - " + mediaObject.filename + " to url: " + this.options.url);
            $.post(this.options.url, { mediaId: mediaObject.id }, function (response) {
                Response.process(response, 5000);
                MediaService.refresh(response.media, this.options.refresh);
                // Run the callback for onAction
                if (this.options.onAction) {
                    this.options.onAction(mediaObject);
                }
            }.bind(this));
        };
        MediaService.prototype.galleryClick = function (mediaObject, e) {
            // once file is uploaded to given directory, send media id to the given attach endpoint.
            if (!this.isMediaSelected(mediaObject)) {
                Alerts.log("[MediaService.galleryClick] Adding to selected media objects - id " + mediaObject.id + " - " + mediaObject.filename + ".");
                this.selectedMedia.push(mediaObject);
                $(e.currentTarget).parents('.media-item').addClass('active');
            }
            else {
                Alerts.log("[MediaService.galleryClick] Removing media from selection - id " + mediaObject.id + " - " + mediaObject.filename + ".}");
                this.selectedMedia = this.selectedMedia.filter(function (obj) {
                    return obj.id !== mediaObject.id;
                });
                $(e.currentTarget).parents('.media-item').removeClass('active');
            }
        };
        MediaService.prototype.galleryAdd = function (e) {
            e.preventDefault();
            e.stopPropagation();
            // once file is uploaded to given directory, send media id to the given attach endpoint.
            Alerts.log("[MediaService.galleryAdd] Adding " + this.selectedMedia.length + " selected media objects  to url: " + this.options.url);
            var mediaIds = this.selectedMedia.map(function (v) {
                return v.id;
            });
            // create the url to send to (add media id's to it as query params)
            $.post(this.options.url, { media: mediaIds }, function (data) {
                Response.process(data);
                // refresh the gallery - - 
                var galleryEl = document.getElementById(this.options.target);
                var gallery = galleryEl.hoodDataList;
                gallery.reload();
                // Run the callback for onAction
                if (this.options.onAction) {
                    this.options.onAction(data.media);
                }
            }.bind(this));
        };
        MediaService.prototype.isMediaSelected = function (mediaObject) {
            var added = false;
            this.selectedMedia.forEach(function (value, index, array) {
                if (value.id == mediaObject.id) {
                    added = true;
                }
            });
            return added;
        };
        MediaService.refresh = function (media, refresh) {
            var icon = media.icon;
            if (media.genericFileType === "Image") {
                icon = media.mediumUrl;
            }
            if (refresh) {
                var $image = $(refresh);
                $image.css({
                    'background-image': 'url(' + icon + ')'
                });
                $image.find('img').attr('src', icon);
                $image.removeClass('loading');
            }
        };
        MediaService.prototype.delete = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                html: 'This file will be permanently deleted, are you sure?'
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (response) {
                        Response.process(response, 5000);
                        this.media.reload();
                        if (this.currentBlade) {
                            this.currentBlade.close();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        return MediaService;
    }());
    var MediaModal = /** @class */ (function () {
        function MediaModal() {
            $('body').on('click', '[data-hood-media=attach],[data-hood-media=select],[data-hood-media=gallery]', this.load.bind(this));
            $('body').on('click', '[data-hood-media=clear]', this.clear.bind(this));
            $('[data-hood-media=gallery]').each(this.initGallery.bind(this));
        }
        MediaModal.prototype.initGallery = function (index, element) {
            // setup the gallery list also, just a simple list jobby, and attach it to the 
            var el = document.getElementById(element.dataset.hoodMediaTarget);
            if (el) {
                new DataList(el, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading gallery media list.', 'info');
                    }.bind(this)
                });
            }
        };
        MediaModal.prototype.load = function (e) {
            e.preventDefault();
            e.stopPropagation();
            this.element = e.currentTarget;
            this.modal = new ModalController({
                onComplete: function (sender) {
                    this.list = document.getElementById('media-list');
                    this.service = new MediaService(this.list, {
                        action: this.element.dataset.hoodMedia,
                        url: this.element.dataset.hoodMediaUrl,
                        refresh: this.element.dataset.hoodMediaRefresh,
                        target: this.element.dataset.hoodMediaTarget,
                        size: this.element.dataset.hoodMediaSize,
                        beforeAction: function (sender, mediaObject) {
                        }.bind(this),
                        onAction: function (sender, mediaObject) {
                            this.modal.close();
                        }.bind(this),
                        onListLoad: function (sender) {
                        },
                        onListRender: function (data) {
                            return data;
                        },
                        onListComplete: function (data) {
                        },
                        onError: function (jqXHR, textStatus, errorThrown) {
                        },
                    });
                }.bind(this)
            });
            this.modal.show($(e.currentTarget).data('hood-media-list'), e.currentTarget);
        };
        MediaModal.prototype.clear = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({}, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (response) {
                        Response.process(response, 5000);
                        MediaService.refresh(response.media, e.currentTarget.dataset.hoodMediaRefresh);
                    }.bind(this));
                }
            }.bind(this));
        };
        return MediaModal;
    }());

    var Uploader = /** @class */ (function () {
        function Uploader() {
            if ($('.image-uploader').length || $('.gallery-uploader').length) {
                $(".upload-progress-bar").hide();
                $('.image-uploader').each(this.singleImage);
                $('.gallery-uploader').each(this.gallery);
            }
        }
        Uploader.prototype.refreshImage = function (sender, data) {
            $(sender.data('preview')).css({
                'background-image': 'url(' + data.media.smallUrl + ')'
            });
            $(sender.data('preview')).find('img').attr('src', data.media.smallUrl);
        };
        Uploader.prototype.singleImage = function () {
            var tag = '#' + $(this).attr('id');
            var $tag = $(tag);
            var jsontag = '#' + $(this).attr('json');
            var avatarDropzone = null;
            $tag.dropzone({
                url: $tag.data('url'),
                maxFiles: 1,
                paramName: 'file',
                parallelUploads: 1,
                acceptedFiles: $tag.data('types') || ".png,.jpg,.jpeg,.gif",
                autoProcessQueue: true,
                previewsContainer: false,
                clickable: tag,
                init: function () {
                    avatarDropzone = this;
                }
            });
            avatarDropzone.on("addedfile", function () {
            });
            avatarDropzone.on("totaluploadprogress", function (progress) {
                $(".upload-progress-bar." + tag.replace('#', '') + " .progress-bar").css({ width: progress + "%" });
            });
            avatarDropzone.on("sending", function (file) {
                $(".upload-progress-bar." + tag.replace('#', '')).show();
                $($tag.data('preview')).addClass('loading');
            });
            avatarDropzone.on("queuecomplete", function (progress) {
                $(".upload-progress-bar." + tag.replace('#', '')).hide();
            });
            avatarDropzone.on("success", function (file, response) {
                if (response.success) {
                    if (response.media) {
                        $(jsontag).val(JSON.stringify(response.media));
                        $($tag.data('preview')).css({
                            'background-image': 'url(' + response.media.smallUrl + ')'
                        });
                        $($tag.data('preview')).find('img').attr('src', response.media.smallUrl);
                    }
                    Alerts.success("New image added!");
                }
                else {
                    Alerts.error("There was a problem adding the image: " + response.error);
                }
                avatarDropzone.removeFile(file);
                $($tag.data('preview')).removeClass('loading');
            });
        };
        Uploader.prototype.gallery = function () {
            var tag = '#' + $(this).attr('id');
            var $tag = $(tag);
            var previewNode = document.querySelector(tag + "-template");
            previewNode.id = "";
            var previewTemplate = previewNode.parentNode.innerHTML;
            previewNode.parentNode.removeChild(previewNode);
            var galleryDropzone = null;
            $tag.dropzone({
                url: $tag.data('url'),
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                parallelUploads: 5,
                previewTemplate: previewTemplate,
                paramName: 'files',
                acceptedFiles: $tag.data('types') || ".png,.jpg,.jpeg,.gif",
                autoProcessQueue: true,
                previewsContainer: "#previews",
                clickable: ".fileinput-button",
                init: function () {
                    galleryDropzone = this;
                }
            });
            $(tag + " .cancel").hide();
            galleryDropzone.on("addedfile", function (file) {
                $(file.previewElement.querySelector(".complete")).hide();
                $(file.previewElement.querySelector(".cancel")).show();
                $(tag + " .cancel").show();
            });
            // Update the total progress bar
            galleryDropzone.on("totaluploadprogress", function (totalProgress, totalBytes, totalBytesSent) {
                var progressBar = document.querySelector("#total-progress .progress-bar");
                progressBar.style.width = totalProgress + "%";
            });
            galleryDropzone.on("sending", function (file) {
                // Show the total progress bar when upload starts
                var progressBar = document.querySelector("#total-progress");
                progressBar.style.opacity = "1";
            });
            // Hide the total progress bar when nothing's uploading anymore
            galleryDropzone.on("complete", function (file) {
                $(file.previewElement.querySelector(".cancel")).hide();
                $(file.previewElement.querySelector(".progress")).hide();
                $(file.previewElement.querySelector(".complete")).show();
                console.error("Uploader.Gallery.Dropzone.OnComplete - Inline.Refresh('.gallery') is not implemented.");
                //Inline.Refresh('.gallery');
            });
            // Hide the total progress bar when nothing's uploading anymore
            galleryDropzone.on("queuecomplete", function (progress) {
                var totalProgress = document.querySelector("#total-progress");
                totalProgress.style.opacity = "0";
                $(tag + " .cancel").hide();
            });
            galleryDropzone.on("success", function (file, response) {
                console.error("Uploader.Gallery.Dropzone.OnSuccess - Inline.Refresh('.gallery') is not implemented.");
                //Inline.Refresh('.gallery');
                if (response.success) {
                    Alerts.success("New images added!");
                }
                else {
                    Alerts.error("There was a problem adding the profile image: " + response.error);
                }
            });
            // Setup the buttons for all transfers
            // The "add files" button doesn't need to be setup because the config
            // `clickable` has already been specified.
            $(".actions .cancel").click(function () {
                galleryDropzone.removeAllFiles(true);
            });
        };
        return Uploader;
    }());

    var BaseController = /** @class */ (function () {
        function BaseController() {
            // Global Services
            this.uploader = new Uploader();
            this.handlers = new Handlers();
            // Global Handlers
            this.setupLoaders();
            // Media Modal Service
            this.mediaModal = new MediaModal();
        }
        BaseController.prototype.setupLoaders = function () {
            $('body').on('loader-show', function () { Alerts.success("Showing loader..."); });
            $('body').on('loader-hide', function () { Alerts.error("Hiding loader..."); });
        };
        return BaseController;
    }());

    var PropertyController = /** @class */ (function () {
        function PropertyController() {
            this.map = null;
            this.center = { lat: 30, lng: -110 };
            this.initList();
        }
        PropertyController.prototype.initList = function () {
            this.element = document.getElementById('property-list');
            if (!this.element) {
                return;
            }
            this.list = new DataList(this.element, {
                onComplete: function (data, sender) {
                    Alerts.log('Finished loading property list.', 'info');
                }.bind(this)
            });
        };
        PropertyController.prototype.initMapList = function () {
            this.mapListElement = document.getElementById('property-map-list');
            if (!this.mapElement) {
                return;
            }
            this.mapList = new DataList(this.mapListElement, {
                onComplete: function (data, sender) {
                    Alerts.log('Finished loading map list.', 'info');
                    this.reloadMarkers();
                }.bind(this)
            });
        };
        PropertyController.prototype.initMap = function (mapElementId) {
            if (mapElementId === void 0) { mapElementId = 'property-map'; }
            this.mapElement = document.getElementById(mapElementId);
            if (!this.mapElement) {
                return;
            }
            this.center = { lat: +this.mapElement.dataset.lat, lng: +this.mapElement.dataset.long };
            this.map = new google.maps.Map(this.mapElement, {
                zoom: +this.mapElement.dataset.zoom || 15,
                center: this.center,
                scrollwheel: false
            });
            $(window).resize(function () {
                google.maps.event.trigger(this.map, 'resize');
            }.bind(this));
            google.maps.event.trigger(this.map, 'resize');
            this.initMapList();
        };
        PropertyController.prototype.reloadMarkers = function () {
            var infowindow = null;
            if (!this.mapElement) {
                return;
            }
            var map = this.map;
            if (this.markers) {
                for (var i = 0; i < this.markers.length; i++) {
                    this.markers[i].setMap(null);
                }
            }
            this.markers = [];
            var locations = $("#property-map-locations").data('locations');
            locations.map(function (location, i) {
                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(+location.Latitude, +location.Longitude),
                    map: this.map,
                    optimized: true // makes SVG icons work in IE
                });
                //marker.setIcon({
                //    url: '/images/marker.png',
                //    size: new google.maps.Size(30, 41),
                //    scaledSize: new google.maps.Size(30, 41)
                //});
                marker.info = "<div class=\"card border-0\" style=\"max-width:300px\">\n    <div style=\"background-image:url(" + location.ImageUrl + ")\" class=\"rounded img-full img img-wide\"></div>\n    <div class=\"card-body border-0\">\n        <p style=\"overflow: hidden;text-overflow: ellipsis;white-space: nowrap;\">\n            <strong>" + location.Address1 + ", " + location.Postcode + "</strong>\n        </p>\n        <p>" + location.Description + "</p>\n        <a href=\"" + location.MarkerUrl + "\" class=\"btn btn-block btn-primary\">Find out more...</a>\n    </div>\n</div>";
                google.maps.event.addListener(marker, 'click', function () {
                    if (infowindow) {
                        infowindow.close();
                    }
                    infowindow = new google.maps.InfoWindow({
                        content: this.info
                    });
                    infowindow.open(map, this);
                }.bind(this));
                this.markers.push(marker);
            }.bind(this));
        };
        return PropertyController;
    }());

    $.fn.exists = function () {
        return $(this).length;
    };
    $.fn.restrictToSlug = function (restrictPattern) {
        if (restrictPattern === void 0) { restrictPattern = /[^0-9a-zA-Z]*/g; }
        var targets = $(this);
        // The characters inside this pattern are accepted
        // and everything else will be 'cleaned'
        // For example 'ABCdEfGhI5' become 'ABCEGI5'
        var restrictHandler = function () {
            var val = $(this).val();
            var newVal = val.replace(restrictPattern, '');
            // This condition is to prevent selection and keyboard navigation issues
            if (val !== newVal) {
                $(this).val(newVal);
            }
        };
        targets.on('keyup', restrictHandler);
        targets.on('paste', restrictHandler);
        targets.on('change', restrictHandler);
    };
    $.fn.restrictToPageSlug = function (restrictPattern) {
        if (restrictPattern === void 0) { restrictPattern = /[^0-9a-zA-Z-//]*/g; }
        var targets = $(this);
        // The characters inside this pattern are accepted
        // and everything else will be 'cleaned'
        var restrictHandler = function () {
            var val = $(this).val();
            var newVal = val.replace(restrictPattern, '');
            if ((newVal.match(new RegExp("/", "g")) || []).length > 4) {
                var pos = newVal.lastIndexOf('/');
                newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
                Alerts.warning("You can only have up to 4 '/' characters in a url slug.");
            }
            // This condition is to prevent selection and keyboard navigation issues
            if (val !== newVal) {
                $(this).val(newVal);
            }
        };
        targets.on('keyup', restrictHandler);
        targets.on('paste', restrictHandler);
        targets.on('change', restrictHandler);
    };
    $.fn.restrictToMetaSlug = function (restrictPattern) {
        if (restrictPattern === void 0) { restrictPattern = /[^0-9a-zA-Z.]*/g; }
        var targets = $(this);
        // The characters inside this pattern are accepted
        // and everything else will be 'cleaned'
        var restrictHandler = function () {
            var val = $(this).val();
            var newVal = val.replace(restrictPattern, '');
            if ((newVal.match(new RegExp(".", "g")) || []).length > 1) {
                var pos = newVal.lastIndexOf('.');
                newVal = newVal.substring(0, pos) + newVal.substring(pos + 1);
                Alerts.warning("You can only have up to 1 '.' characters in a meta slug.");
            }
            // This condition is to prevent selection and keyboard navigation issues
            if (val !== newVal) {
                $(this).val(newVal);
            }
        };
        targets.on('keyup', restrictHandler);
        targets.on('paste', restrictHandler);
        targets.on('change', restrictHandler);
    };
    $.fn.characterCounter = function () {
        var targets = $(this);
        var characterCounterHandler = function () {
            var counter = $(this).data('counter');
            var max = Number($(this).attr('maxlength'));
            var len = $(this).val().length;
            $(counter).text(max - len);
            var cls = "text-success";
            if (max - len < max / 10)
                cls = "text-danger";
            $(counter).parent().removeClass('text-success').removeClass('text-danger').addClass(cls);
        };
        targets.on('keyup', characterCounterHandler);
        targets.on('paste', characterCounterHandler);
        targets.on('change', characterCounterHandler);
    };
    $.fn.warningAlert = function () {
        var targets = $(this);
        var warningAlertHandler = function (e) {
            e.preventDefault();
            var warningAlertCallback = function (result) {
                if (result.isConfirmed) {
                    var url = $(e.currentTarget).attr('href');
                    window.location.href = url;
                }
            };
            Alerts.confirm({
                title: $(e.currentTarget).data('title'),
                html: $(e.currentTarget).data('warning'),
                footer: $(e.currentTarget).data('footer'),
                icon: 'warning'
            }, warningAlertCallback);
            return false;
        };
        targets.on('click', warningAlertHandler);
    };

    Number.prototype.formatCurrency = function (currency) {
        return currency + " " + this.toFixed(2).replace(/./g, function (c, i, a) {
            return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
        });
    };
    Number.prototype.formatKilobytes = function () {
        var n = this / 1024;
        return n.toFixed(0).replace(/./g, function (c, i, a) {
            return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
        }) + "Kb";
    };
    Number.prototype.formatMegabytes = function () {
        var n = this / 1024;
        n = n / 1024;
        return n.toFixed(0).replace(/./g, function (c, i, a) {
            return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
        }) + "Mb";
    };

    String.prototype.htmlEncode = function () {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out.  The div never exists on the page.
        return $('<div/>').text(this).html();
    };
    String.prototype.htmlDecode = function () {
        return $('<div/>').html(this).text();
    };
    String.prototype.contains = function (it) {
        return this.indexOf(it) !== -1;
    };
    String.prototype.pick = function (min, max) {
        var n, chars = '';
        if (typeof max === 'undefined') {
            n = min;
        }
        else {
            n = min + Math.floor(Math.random() * (max - min));
        }
        for (var i = 0; i < n; i++) {
            chars += this.charAt(Math.floor(Math.random() * this.length));
        }
        return chars;
    };
    // Credit to @Christoph: http://stackoverflow.com/a/962890/464744
    String.prototype.shuffle = function () {
        var array = this.split('');
        var tmp, current, top = array.length;
        if (top)
            while (--top) {
                current = Math.floor(Math.random() * (top + 1));
                tmp = array[current];
                array[current] = array[top];
                array[top] = tmp;
            }
        return array.join('');
    };
    String.prototype.toSeoUrl = function () {
        var output = this.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, "-").toLowerCase();
        /* remove first dash */
        if (output.charAt(0) === '-')
            output = output.substring(1);
        /* remove last dash */
        var last = output.length - 1;
        if (output.charAt(last) === '-')
            output = output.substring(0, last);
        return output;
    };

    var App = /** @class */ (function (_super) {
        __extends(App, _super);
        function App() {
            var _this = _super.call(this) || this;
            // Hook up default handlers.
            _this.handlers.initDefaultHandlers();
            // Admin Controllers
            _this.propertyController = new PropertyController();
            return _this;
        }
        App.prototype.initGoogleMaps = function (tag) {
            if (tag === void 0) { tag = '.google-map'; }
            $(tag).each(function () {
                var myLatLng = new google.maps.LatLng($(this).data('lat'), $(this).data('long'));
                console.log('Loading map at: ' + $(this).data('lat') + ', ' + $(this).data('long'));
                var map = new google.maps.Map(this, {
                    zoom: $(this).data('zoom') || 15,
                    center: myLatLng,
                    scrollwheel: false
                });
                new google.maps.Marker({
                    position: myLatLng,
                    map: map,
                    title: $(this).data('marker')
                });
                $(window).resize(function () {
                    google.maps.event.trigger(map, 'resize');
                });
                google.maps.event.trigger(map, 'resize');
            });
        };
        App.prototype.initContactForms = function (tag) {
            if (tag === void 0) { tag = '.contact-form'; }
            var $form = $(tag);
            $form.find('.thank-you').hide();
            $form.find('.form-content').show();
            $('body').on('submit', tag, this.submitContactForm);
            var form = $(tag)[0];
            new Validator(form, {
                onComplete: function (response) {
                    Response.process(response, 5000);
                    if (response.success) {
                        if ($form.attr('data-redirect'))
                            window.location.href = $form.attr('data-redirect');
                        if ($form.attr('data-alert-message'))
                            Alerts.success($form.attr('data-alert-message'), "Success");
                        $form.find('.form').hide();
                        $form.find('.thank-you').show();
                    }
                    else {
                        if ($form.attr('data-alert-error'))
                            Alerts.error($form.attr('data-alert-error'), "Error");
                        else
                            Alerts.error("There was an error sending the message: " + response.errors, "Error");
                    }
                }.bind(this)
            });
        };
        App.prototype.submitContactForm = function (e) {
            e.preventDefault();
            $(this).addClass('loading');
            var $form = $(this);
            if ($form.valid()) {
                $.post($form.attr('action'), $form.serialize(), function (data) {
                });
            }
            return false;
        };
        return App;
    }(BaseController));
    var app = new App();

    exports.Alerts = Alerts;
    exports.App = App;
    exports.app = app;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

}({}, null, Swal, bootstrap));
//# sourceMappingURL=app.js.map

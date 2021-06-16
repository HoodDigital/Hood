var hood = (function (exports, swal, bootstrap, chart_js, tinymce, Pickr) {
    'use strict';

    function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

    function _interopNamespace(e) {
        if (e && e.__esModule) return e;
        var n = Object.create(null);
        if (e) {
            Object.keys(e).forEach(function (k) {
                if (k !== 'default') {
                    var d = Object.getOwnPropertyDescriptor(e, k);
                    Object.defineProperty(n, k, d.get ? d : {
                        enumerable: true,
                        get: function () {
                            return e[k];
                        }
                    });
                }
            });
        }
        n['default'] = e;
        return Object.freeze(n);
    }

    var swal__default = /*#__PURE__*/_interopDefaultLegacy(swal);
    var bootstrap__namespace = /*#__PURE__*/_interopNamespace(bootstrap);
    var tinymce__default = /*#__PURE__*/_interopDefaultLegacy(tinymce);
    var Pickr__default = /*#__PURE__*/_interopDefaultLegacy(Pickr);

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

    /*! Copyright (c) 2011 Piotr Rochala (http://rocha.la)
     * Dual licensed under the MIT (http://www.opensource.org/licenses/mit-license.php)
     * and GPL (http://www.opensource.org/licenses/gpl-license.php) licenses.
     *
     * Version: 1.3.8
     *
     */
    (function($) {

      $.fn.extend({
        slimScroll: function(options) {

          var defaults = {

            // width in pixels of the visible scroll area
            width : 'auto',

            // height in pixels of the visible scroll area
            height : '250px',

            // width in pixels of the scrollbar and rail
            size : '7px',

            // scrollbar color, accepts any hex/color value
            color: '#000',

            // scrollbar position - left/right
            position : 'right',

            // distance in pixels between the side edge and the scrollbar
            distance : '1px',

            // default scroll position on load - top / bottom / $('selector')
            start : 'top',

            // sets scrollbar opacity
            opacity : .4,

            // enables always-on mode for the scrollbar
            alwaysVisible : false,

            // check if we should hide the scrollbar when user is hovering over
            disableFadeOut : false,

            // sets visibility of the rail
            railVisible : false,

            // sets rail color
            railColor : '#333',

            // sets rail opacity
            railOpacity : .2,

            // whether  we should use jQuery UI Draggable to enable bar dragging
            railDraggable : true,

            // defautlt CSS class of the slimscroll rail
            railClass : 'slimScrollRail',

            // defautlt CSS class of the slimscroll bar
            barClass : 'slimScrollBar',

            // defautlt CSS class of the slimscroll wrapper
            wrapperClass : 'slimScrollDiv',

            // check if mousewheel should scroll the window if we reach top/bottom
            allowPageScroll : false,

            // scroll amount applied to each mouse wheel step
            wheelStep : 20,

            // scroll amount applied when user is using gestures
            touchScrollStep : 200,

            // sets border radius
            borderRadius: '7px',

            // sets border radius of the rail
            railBorderRadius : '7px'
          };

          var o = $.extend(defaults, options);

          // do it for every element that matches selector
          this.each(function(){

          var isOverPanel, isOverBar, isDragg, queueHide, touchDif,
            barHeight, percentScroll, lastScroll,
            divS = '<div></div>',
            minBarHeight = 30,
            releaseScroll = false;

            // used in event handlers and for better minification
            var me = $(this);

            // ensure we are not binding it again
            if (me.parent().hasClass(o.wrapperClass))
            {
                // start from last bar position
                var offset = me.scrollTop();

                // find bar and rail
                bar = me.siblings('.' + o.barClass);
                rail = me.siblings('.' + o.railClass);

                getBarHeight();

                // check if we should scroll existing instance
                if ($.isPlainObject(options))
                {
                  // Pass height: auto to an existing slimscroll object to force a resize after contents have changed
                  if ( 'height' in options && options.height == 'auto' ) {
                    me.parent().css('height', 'auto');
                    me.css('height', 'auto');
                    var height = me.parent().parent().height();
                    me.parent().css('height', height);
                    me.css('height', height);
                  } else if ('height' in options) {
                    var h = options.height;
                    me.parent().css('height', h);
                    me.css('height', h);
                  }

                  if ('scrollTo' in options)
                  {
                    // jump to a static point
                    offset = parseInt(o.scrollTo);
                  }
                  else if ('scrollBy' in options)
                  {
                    // jump by value pixels
                    offset += parseInt(o.scrollBy);
                  }
                  else if ('destroy' in options)
                  {
                    // remove slimscroll elements
                    bar.remove();
                    rail.remove();
                    me.unwrap();
                    return;
                  }

                  // scroll content by the given offset
                  scrollContent(offset, false, true);
                }

                return;
            }
            else if ($.isPlainObject(options))
            {
                if ('destroy' in options)
                {
                	return;
                }
            }

            // optionally set height to the parent's height
            o.height = (o.height == 'auto') ? me.parent().height() : o.height;

            // wrap content
            var wrapper = $(divS)
              .addClass(o.wrapperClass)
              .css({
                position: 'relative',
                overflow: 'hidden',
                width: o.width,
                height: o.height
              });

            // update style for the div
            me.css({
              overflow: 'hidden',
              width: o.width,
              height: o.height
            });

            // create scrollbar rail
            var rail = $(divS)
              .addClass(o.railClass)
              .css({
                width: o.size,
                height: '100%',
                position: 'absolute',
                top: 0,
                display: (o.alwaysVisible && o.railVisible) ? 'block' : 'none',
                'border-radius': o.railBorderRadius,
                background: o.railColor,
                opacity: o.railOpacity,
                zIndex: 90
              });

            // create scrollbar
            var bar = $(divS)
              .addClass(o.barClass)
              .css({
                background: o.color,
                width: o.size,
                position: 'absolute',
                top: 0,
                opacity: o.opacity,
                display: o.alwaysVisible ? 'block' : 'none',
                'border-radius' : o.borderRadius,
                BorderRadius: o.borderRadius,
                MozBorderRadius: o.borderRadius,
                WebkitBorderRadius: o.borderRadius,
                zIndex: 99
              });

            // set position
            var posCss = (o.position == 'right') ? { right: o.distance } : { left: o.distance };
            rail.css(posCss);
            bar.css(posCss);

            // wrap it
            me.wrap(wrapper);

            // append to parent div
            me.parent().append(bar);
            me.parent().append(rail);

            // make it draggable and no longer dependent on the jqueryUI
            if (o.railDraggable){
              bar.bind("mousedown", function(e) {
                var $doc = $(document);
                isDragg = true;
                t = parseFloat(bar.css('top'));
                pageY = e.pageY;

                $doc.bind("mousemove.slimscroll", function(e){
                  currTop = t + e.pageY - pageY;
                  bar.css('top', currTop);
                  scrollContent(0, bar.position().top, false);// scroll content
                });

                $doc.bind("mouseup.slimscroll", function(e) {
                  isDragg = false;hideBar();
                  $doc.unbind('.slimscroll');
                });
                return false;
              }).bind("selectstart.slimscroll", function(e){
                e.stopPropagation();
                e.preventDefault();
                return false;
              });
            }

            // on rail over
            rail.hover(function(){
              showBar();
            }, function(){
              hideBar();
            });

            // on bar over
            bar.hover(function(){
              isOverBar = true;
            }, function(){
              isOverBar = false;
            });

            // show on parent mouseover
            me.hover(function(){
              isOverPanel = true;
              showBar();
              hideBar();
            }, function(){
              isOverPanel = false;
              hideBar();
            });

            // support for mobile
            me.bind('touchstart', function(e,b){
              if (e.originalEvent.touches.length)
              {
                // record where touch started
                touchDif = e.originalEvent.touches[0].pageY;
              }
            });

            me.bind('touchmove', function(e){
              // prevent scrolling the page if necessary
              if(!releaseScroll)
              {
      		      e.originalEvent.preventDefault();
    		      }
              if (e.originalEvent.touches.length)
              {
                // see how far user swiped
                var diff = (touchDif - e.originalEvent.touches[0].pageY) / o.touchScrollStep;
                // scroll content
                scrollContent(diff, true);
                touchDif = e.originalEvent.touches[0].pageY;
              }
            });

            // set up initial height
            getBarHeight();

            // check start position
            if (o.start === 'bottom')
            {
              // scroll content to bottom
              bar.css({ top: me.outerHeight() - bar.outerHeight() });
              scrollContent(0, true);
            }
            else if (o.start !== 'top')
            {
              // assume jQuery selector
              scrollContent($(o.start).position().top, null, true);

              // make sure bar stays hidden
              if (!o.alwaysVisible) { bar.hide(); }
            }

            // attach scroll events
            attachWheel(this);

            function _onWheel(e)
            {
              // use mouse wheel only when mouse is over
              if (!isOverPanel) { return; }

              var e = e || window.event;

              var delta = 0;
              if (e.wheelDelta) { delta = -e.wheelDelta/120; }
              if (e.detail) { delta = e.detail / 3; }

              var target = e.target || e.srcTarget || e.srcElement;
              if ($(target).closest('.' + o.wrapperClass).is(me.parent())) {
                // scroll content
                scrollContent(delta, true);
              }

              // stop window scroll
              if (e.preventDefault && !releaseScroll) { e.preventDefault(); }
              if (!releaseScroll) { e.returnValue = false; }
            }

            function scrollContent(y, isWheel, isJump)
            {
              releaseScroll = false;
              var delta = y;
              var maxTop = me.outerHeight() - bar.outerHeight();

              if (isWheel)
              {
                // move bar with mouse wheel
                delta = parseInt(bar.css('top')) + y * parseInt(o.wheelStep) / 100 * bar.outerHeight();

                // move bar, make sure it doesn't go out
                delta = Math.min(Math.max(delta, 0), maxTop);

                // if scrolling down, make sure a fractional change to the
                // scroll position isn't rounded away when the scrollbar's CSS is set
                // this flooring of delta would happened automatically when
                // bar.css is set below, but we floor here for clarity
                delta = (y > 0) ? Math.ceil(delta) : Math.floor(delta);

                // scroll the scrollbar
                bar.css({ top: delta + 'px' });
              }

              // calculate actual scroll amount
              percentScroll = parseInt(bar.css('top')) / (me.outerHeight() - bar.outerHeight());
              delta = percentScroll * (me[0].scrollHeight - me.outerHeight());

              if (isJump)
              {
                delta = y;
                var offsetTop = delta / me[0].scrollHeight * me.outerHeight();
                offsetTop = Math.min(Math.max(offsetTop, 0), maxTop);
                bar.css({ top: offsetTop + 'px' });
              }

              // scroll content
              me.scrollTop(delta);

              // fire scrolling event
              me.trigger('slimscrolling', ~~delta);

              // ensure bar is visible
              showBar();

              // trigger hide when scroll is stopped
              hideBar();
            }

            function attachWheel(target)
            {
              if (window.addEventListener)
              {
                target.addEventListener('DOMMouseScroll', _onWheel, false );
                target.addEventListener('mousewheel', _onWheel, false );
              }
              else
              {
                document.attachEvent("onmousewheel", _onWheel);
              }
            }

            function getBarHeight()
            {
              // calculate scrollbar height and make sure it is not too small
              barHeight = Math.max((me.outerHeight() / me[0].scrollHeight) * me.outerHeight(), minBarHeight);
              bar.css({ height: barHeight + 'px' });

              // hide scrollbar if content is not long enough
              var display = barHeight == me.outerHeight() ? 'none' : 'block';
              bar.css({ display: display });
            }

            function showBar()
            {
              // recalculate bar height
              getBarHeight();
              clearTimeout(queueHide);

              // when bar reached top or bottom
              if (percentScroll == ~~percentScroll)
              {
                //release wheel
                releaseScroll = o.allowPageScroll;

                // publish approporiate event
                if (lastScroll != percentScroll)
                {
                    var msg = (~~percentScroll == 0) ? 'top' : 'bottom';
                    me.trigger('slimscroll', msg);
                }
              }
              else
              {
                releaseScroll = false;
              }
              lastScroll = percentScroll;

              // show only when required
              if(barHeight >= me.outerHeight()) {
                //allow window scroll
                releaseScroll = true;
                return;
              }
              bar.stop(true,true).fadeIn('fast');
              if (o.railVisible) { rail.stop(true,true).fadeIn('fast'); }
            }

            function hideBar()
            {
              // only hide when options allow it
              if (!o.alwaysVisible)
              {
                queueHide = setTimeout(function(){
                  if (!(o.disableFadeOut && isOverPanel) && !isOverBar && !isDragg)
                  {
                    bar.fadeOut('slow');
                    rail.fadeOut('slow');
                  }
                }, 1000);
              }
            }

          });

          // maintain chainability
          return this;
        }
      });

      $.fn.extend({
        slimscroll: $.fn.slimScroll
      });

    })(jQuery);

    "function"!=typeof Object.create&&(Object.create=function(t){function o(){}return o.prototype=t,new o}),function(t,o,i,s){var n={_positionClasses:["bottom-left","bottom-right","top-right","top-left","bottom-center","top-center","mid-center"],_defaultIcons:["success","error","info","warning"],init:function(o,i){this.prepareOptions(o,t.toast.options),this.process();},prepareOptions:function(o,i){var s={};"string"==typeof o||o instanceof Array?s.text=o:s=o,this.options=t.extend({},i,s);},process:function(){this.setup(),this.addToDom(),this.position(),this.bindToast(),this.animate();},setup:function(){var o="";if(this._toastEl=this._toastEl||t("<div></div>",{"class":"jq-toast-single"}),o+='<span class="jq-toast-loader"></span>',this.options.allowToastClose&&(o+='<span class="close-jq-toast-single">&times;</span>'),this.options.text instanceof Array){this.options.heading&&(o+='<h2 class="jq-toast-heading">'+this.options.heading+"</h2>"),o+='<ul class="jq-toast-ul">';for(var i=0;i<this.options.text.length;i++)o+='<li class="jq-toast-li" id="jq-toast-item-'+i+'">'+this.options.text[i]+"</li>";o+="</ul>";}else this.options.heading&&(o+='<h2 class="jq-toast-heading">'+this.options.heading+"</h2>"),o+=this.options.text;this._toastEl.html(o),this.options.bgColor!==!1&&this._toastEl.css("background-color",this.options.bgColor),this.options.textColor!==!1&&this._toastEl.css("color",this.options.textColor),this.options.textAlign&&this._toastEl.css("text-align",this.options.textAlign),this.options.icon!==!1&&(this._toastEl.addClass("jq-has-icon"),-1!==t.inArray(this.options.icon,this._defaultIcons)&&this._toastEl.addClass("jq-icon-"+this.options.icon)),this.options["class"]!==!1&&this._toastEl.addClass(this.options["class"]);},position:function(){"string"==typeof this.options.position&&-1!==t.inArray(this.options.position,this._positionClasses)?"bottom-center"===this.options.position?this._container.css({left:t(o).outerWidth()/2-this._container.outerWidth()/2,bottom:20}):"top-center"===this.options.position?this._container.css({left:t(o).outerWidth()/2-this._container.outerWidth()/2,top:20}):"mid-center"===this.options.position?this._container.css({left:t(o).outerWidth()/2-this._container.outerWidth()/2,top:t(o).outerHeight()/2-this._container.outerHeight()/2}):this._container.addClass(this.options.position):"object"==typeof this.options.position?this._container.css({top:this.options.position.top?this.options.position.top:"auto",bottom:this.options.position.bottom?this.options.position.bottom:"auto",left:this.options.position.left?this.options.position.left:"auto",right:this.options.position.right?this.options.position.right:"auto"}):this._container.addClass("bottom-left");},bindToast:function(){var t=this;this._toastEl.on("afterShown",function(){t.processLoader();}),this._toastEl.find(".close-jq-toast-single").on("click",function(o){o.preventDefault(),"fade"===t.options.showHideTransition?(t._toastEl.trigger("beforeHide"),t._toastEl.fadeOut(function(){t._toastEl.trigger("afterHidden");})):"slide"===t.options.showHideTransition?(t._toastEl.trigger("beforeHide"),t._toastEl.slideUp(function(){t._toastEl.trigger("afterHidden");})):(t._toastEl.trigger("beforeHide"),t._toastEl.hide(function(){t._toastEl.trigger("afterHidden");}));}),"function"==typeof this.options.beforeShow&&this._toastEl.on("beforeShow",function(){t.options.beforeShow();}),"function"==typeof this.options.afterShown&&this._toastEl.on("afterShown",function(){t.options.afterShown();}),"function"==typeof this.options.beforeHide&&this._toastEl.on("beforeHide",function(){t.options.beforeHide();}),"function"==typeof this.options.afterHidden&&this._toastEl.on("afterHidden",function(){t.options.afterHidden();});},addToDom:function(){var o=t(".jq-toast-wrap");if(0===o.length?(o=t("<div></div>",{"class":"jq-toast-wrap"}),t("body").append(o)):(!this.options.stack||isNaN(parseInt(this.options.stack,10)))&&o.empty(),o.find(".jq-toast-single:hidden").remove(),o.append(this._toastEl),this.options.stack&&!isNaN(parseInt(this.options.stack),10)){var i=o.find(".jq-toast-single").length,s=i-this.options.stack;s>0&&t(".jq-toast-wrap").find(".jq-toast-single").slice(0,s).remove();}this._container=o;},canAutoHide:function(){return this.options.hideAfter!==!1&&!isNaN(parseInt(this.options.hideAfter,10))},processLoader:function(){if(!this.canAutoHide()||this.options.loader===!1)return !1;var t=this._toastEl.find(".jq-toast-loader"),o=(this.options.hideAfter-400)/1e3+"s",i=this.options.loaderBg,s=t.attr("style")||"";s=s.substring(0,s.indexOf("-webkit-transition")),s+="-webkit-transition: width "+o+" ease-in;                       -o-transition: width "+o+" ease-in;                       transition: width "+o+" ease-in;                       background-color: "+i+";",t.attr("style",s).addClass("jq-toast-loaded");},animate:function(){var t=this;if(this._toastEl.hide(),this._toastEl.trigger("beforeShow"),"fade"===this.options.showHideTransition.toLowerCase()?this._toastEl.fadeIn(function(){t._toastEl.trigger("afterShown");}):"slide"===this.options.showHideTransition.toLowerCase()?this._toastEl.slideDown(function(){t._toastEl.trigger("afterShown");}):this._toastEl.show(function(){t._toastEl.trigger("afterShown");}),this.canAutoHide()){var t=this;o.setTimeout(function(){"fade"===t.options.showHideTransition.toLowerCase()?(t._toastEl.trigger("beforeHide"),t._toastEl.fadeOut(function(){t._toastEl.trigger("afterHidden");})):"slide"===t.options.showHideTransition.toLowerCase()?(t._toastEl.trigger("beforeHide"),t._toastEl.slideUp(function(){t._toastEl.trigger("afterHidden");})):(t._toastEl.trigger("beforeHide"),t._toastEl.hide(function(){t._toastEl.trigger("afterHidden");}));},this.options.hideAfter);}},reset:function(o){"all"===o?t(".jq-toast-wrap").remove():this._toastEl.remove();},update:function(t){this.prepareOptions(t,this.options),this.setup(),this.bindToast();}};t.toast=function(t){var o=Object.create(n);return o.init(t,this),{reset:function(t){o.reset(t);},update:function(t){o.update(t);}}},t.toast.options={text:"",heading:"",showHideTransition:"fade",allowToastClose:!0,hideAfter:3e3,loader:!0,loaderBg:"#9EC600",stack:5,position:"bottom-left",bgColor:!1,textColor:!1,textAlign:"left",icon:!1,beforeShow:function(){},afterShown:function(){},beforeHide:function(){},afterHidden:function(){}};}(jQuery,window);

    var BootstrapSwal = swal__default['default'].mixin({
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

    var Handlers = /** @class */ (function () {
        function Handlers() {
        }
        Handlers.prototype.defaults = function () {
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

    var HomeController = /** @class */ (function () {
        function HomeController() {
            if ($('#admin-chart-area').length) {
                var $this_1 = this;
                $this_1.LoadChart();
                $('body').on('change', '#history', function () {
                    $this_1.LoadChart();
                });
            }
        }
        HomeController.prototype.LoadChart = function () {
            var $this = this;
            $.get('/admin/stats/', function (data) {
                $('#admin-chart-area').empty();
                $('#admin-chart-area').html('<canvas id="admin-stats-chart"></canvas>');
                $this.LoadStats(data);
                $this.DrawChart(data);
            });
        };
        HomeController.prototype.DrawChart = function (data) {
            var datasets = [];
            var dataLabels = [];
            var contentColour = '#fabd07';
            var propertyColour = '#20c997';
            var userColour = '#fd7e14';
            // 12 months
            var labels = [];
            // Content by day
            var contentSet = [];
            data.content.months.forEach(function (element) {
                contentSet.push(element.value);
                labels.push(element.key);
            });
            datasets.push({
                label: 'New Content',
                data: contentSet,
                borderColor: contentColour,
                backgroundColor: contentColour,
                pointBackgroundColor: contentColour,
                pointBorderColor: '#ffffff'
            });
            // users by day
            contentSet = [];
            data.users.months.forEach(function (element) {
                contentSet.push(element.value);
            });
            datasets.push({
                label: 'New Users',
                data: contentSet,
                borderColor: userColour,
                backgroundColor: userColour,
                pointBackgroundColor: userColour,
                pointBorderColor: '#ffffff'
            });
            // Properties by day
            contentSet = [];
            data.properties.months.forEach(function (element) {
                contentSet.push(element.value);
            });
            datasets.push({
                label: 'New Properties',
                data: contentSet,
                borderColor: propertyColour,
                backgroundColor: propertyColour,
                pointBackgroundColor: propertyColour,
                pointBorderColor: '#ffffff'
            });
            dataLabels = labels;
            this.canvas = document.getElementById("admin-stats-chart");
            this.ctx = this.canvas.getContext('2d');
            this.chart = new chart_js.Chart(this.ctx, {
                type: 'bar',
                data: {
                    labels: dataLabels,
                    datasets: datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        };
        HomeController.prototype.LoadStats = function (data) {
            if (data.content) {
                $('.content-totalPosts').text(data.content.totalPosts);
                $('.content-totalPublished').text(data.content.totalPublished);
                if (data.content.byType) {
                    for (var i = 0; i < data.content.byType.length; i++) {
                        $('.content-' + data.content.byType[i].name + '-total').text(data.content.byType[i].total);
                    }
                }
            }
            if (data.users) {
                $('.users-totalUsers').text(data.users.totalUsers);
                $('.users-totalAdmins').text(data.users.totalAdmins);
            }
            if (data.properties) {
                $('.properties-totalPosts').text(data.properties.totalProperties);
                $('.properties-totalPublished').text(data.properties.totalPublished);
            }
        };
        return HomeController;
    }());

    var MediaController = /** @class */ (function () {
        function MediaController() {
            this.manage();
        }
        MediaController.prototype.manage = function () {
            this.list = document.getElementById('media-list');
            this.service = new MediaService(this.list, {
                action: 'show',
                onAction: function (mediaObject) {
                    Alerts.log("Showing media object id " + mediaObject.id + " - " + mediaObject.filename);
                }.bind(this),
                onListLoad: function () {
                    Alerts.log('Commencing media list fetch.');
                },
                onError: function (jqXHR, textStatus, errorThrown) {
                    Alerts.log("Error loading media list: " + textStatus);
                },
                onListRender: function (data) {
                    Alerts.log('Fetched media list data.');
                    return data;
                },
                onListComplete: function () {
                    Alerts.log('Finished loading media list...', 'info');
                }
            });
        };
        return MediaController;
    }());

    var ContentController = /** @class */ (function () {
        function ContentController() {
            this.manage();
            $('body').on('click', '.content-create', this.create.bind(this));
            $('body').on('click', '.content-delete', this.delete.bind(this));
            $('body').on('click', '.content-set-status', this.setStatus.bind(this));
            $('body').on('click', '.content-categories-delete', this.deleteCategory.bind(this));
            $('body').on('click', '.content-categories-create,.content-categories-edit', this.createOrEditCategory.bind(this));
            $('body').on('change', '.content-categories-check', this.toggleCategory.bind(this));
            $('body').on('click', '.content-media-delete', this.removeMedia.bind(this));
            $('body').on('keyup', '#Slug', function () {
                var slugValue = $(this).val();
                $('.slug-display').html(slugValue);
            });
        }
        ContentController.prototype.manage = function () {
            this.element = document.getElementById('content-list');
            if (this.element) {
                this.list = new DataList(this.element, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading content list.', 'info');
                    }.bind(this)
                });
            }
            this.categoryElement = document.getElementById('content-categories-list');
            if (this.categoryElement) {
                this.categoryList = new DataList(this.categoryElement, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading category list.', 'info');
                    }.bind(this)
                });
            }
        };
        ContentController.prototype.create = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createContentModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('content-create-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (this.list) {
                                this.list.reload();
                            }
                            if (response.success) {
                                createContentModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createContentModal.show($(e.currentTarget).attr('href'), this.element);
        };
        ContentController.prototype.delete = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The content will be permanently removed."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                        if (e.currentTarget.dataset.redirect) {
                            Alerts.message('Just taking you back to the content list.', 'Redirecting...');
                            setTimeout(function () {
                                window.location = e.currentTarget.dataset.redirect;
                            }, 1500);
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        ContentController.prototype.setStatus = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The change will happen immediately."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        ContentController.prototype.clone = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "This will duplicate the content and everything inside it."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        ContentController.prototype.createOrEditCategory = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createCategoryModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('content-categories-edit-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (this.list) {
                                this.list.reload();
                            }
                            if (response.success) {
                                this.categoryList.reload();
                                createCategoryModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createCategoryModal.show($(e.currentTarget).attr('href'), this.element);
        };
        ContentController.prototype.deleteCategory = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
            // Confirm options...
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        // category deleted...
                        Response.process(data, 5000);
                        this.categoryList.reload();
                    }.bind(this));
                }
            }.bind(this));
        };
        ContentController.prototype.toggleCategory = function (e) {
            e.preventDefault();
            e.stopPropagation();
            $.post($(e.currentTarget).data('url'), { categoryId: $(e.currentTarget).val(), add: $(e.currentTarget).is(':checked') }, function (response) {
                Response.process(response, 5000);
                if (this.categoryList) {
                    this.categoryList.reload();
                }
            }.bind(this));
        };
        ContentController.prototype.removeMedia = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The media will be removed from the property, but will still be in the media collection."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        // reload the property media gallery, should be attached to #property-gallery-list
                        var mediaGalleryEl = document.getElementById('content-gallery-list');
                        if (mediaGalleryEl.hoodDataList) {
                            mediaGalleryEl.hoodDataList.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        return ContentController;
    }());

    var PropertyController = /** @class */ (function () {
        function PropertyController() {
            this.manage();
            $('body').on('click', '.property-create', this.create.bind(this));
            $('body').on('click', '.property-delete', this.delete.bind(this));
            $('body').on('click', '.property-set-status', this.setStatus.bind(this));
            $('body').on('click', '.property-media-delete', this.removeMedia.bind(this));
            $('body').on('click', '.property-floorplan-delete', this.removeMedia.bind(this));
            $('body').on('keyup', '#Slug', function () {
                var slugValue = $(this).val();
                $('.slug-display').html(slugValue);
            });
        }
        PropertyController.prototype.manage = function () {
            this.element = document.getElementById('property-list');
            if (this.element) {
                this.list = new DataList(this.element, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading property list.', 'info');
                    }.bind(this)
                });
            }
        };
        PropertyController.prototype.create = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createPropertyModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('property-create-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (this.list) {
                                this.list.reload();
                            }
                            if (response.success) {
                                createPropertyModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createPropertyModal.show($(e.currentTarget).attr('href'), this.element);
        };
        PropertyController.prototype.delete = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The property will be permanently removed."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                        if (e.currentTarget.dataset.redirect) {
                            Alerts.message('Just taking you back to the property list.', 'Redirecting...');
                            setTimeout(function () {
                                window.location = e.currentTarget.dataset.redirect;
                            }, 1500);
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        PropertyController.prototype.setStatus = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The change will happen immediately."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        PropertyController.prototype.removeMedia = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The media will be removed from the property, but will still be in the media collection."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        // reload the property media gallery, should be attached to #property-gallery-list
                        var mediaGalleryEl = document.getElementById('property-gallery-list');
                        if (mediaGalleryEl.hoodDataList) {
                            mediaGalleryEl.hoodDataList.reload();
                        }
                        // reload the property media gallery, should be attached to #property-gallery-list
                        var mfpGalleryEl = document.getElementById('property-floorplan-list');
                        if (mfpGalleryEl && mfpGalleryEl.hoodDataList) {
                            mfpGalleryEl.hoodDataList.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        return PropertyController;
    }());

    var RandomStringGenerator = /** @class */ (function () {
        function RandomStringGenerator(options) {
            this.options = {
                specials: '!@#$&*',
                alpha: 'abcdefghijklmnopqrstuvwxyz',
                numeric: '0123456789',
                numSpecial: 2
            };
            this.options = __assign(__assign({}, this.options), options);
        }
        RandomStringGenerator.prototype.generate = function (length) {
            var password = '';
            var len = Math.ceil((length - this.options.numSpecial) / 2);
            for (var i = 0; i < len; i++) {
                password += this.options.alpha.charAt(Math.floor(Math.random() * this.options.alpha.length));
                password += this.options.numeric.charAt(Math.floor(Math.random() * this.options.numeric.length));
            }
            for (var j = 0; j < this.options.numSpecial; j++)
                password += this.options.specials.charAt(Math.floor(Math.random() * this.options.specials.length));
            password = password.split('').sort(function () { return 0.5 - Math.random(); }).join('');
            return password;
        };
        return RandomStringGenerator;
    }());

    var UsersController = /** @class */ (function () {
        function UsersController() {
            this.initLists();
            $('body').on('click', '.user-create', this.create.bind(this));
            $('body').on('click', '.user-delete', this.delete.bind(this));
            $('body').on('change', '#user-create-form #GeneratePassword', this.generatePassword);
            $('body').on('change', '.user-role-check', this.toggleUserRole);
            $('body').on('click', '.user-reset-password', this.resetPassword);
            $('body').on('click', '.user-notes-add', this.addNote.bind(this));
            $('body').on('click', '.user-notes-delete', this.deleteNote.bind(this));
        }
        UsersController.prototype.initLists = function () {
            this.element = document.getElementById('user-list');
            if (this.element) {
                this.list = new DataList(this.element, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading users list.', 'info');
                    }.bind(this)
                });
            }
            this.notesEl = document.getElementById('user-notes');
            if (this.notesEl) {
                this.notesList = new DataList(this.notesEl, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading user notes list.', 'info');
                    }.bind(this)
                });
            }
        };
        UsersController.prototype.create = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createUserModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('user-create-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (this.list) {
                                this.list.reload();
                            }
                            if (response.success) {
                                createUserModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createUserModal.show($(e.currentTarget).attr('href'), this.element);
        };
        UsersController.prototype.delete = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The user will be permanently removed."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                        if (e.currentTarget.dataset.redirect) {
                            Alerts.message('Just taking you back to the content list.', 'Redirecting...');
                            setTimeout(function () {
                                window.location = e.currentTarget.dataset.redirect;
                            }, 1500);
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        UsersController.prototype.generatePassword = function () {
            if ($(this).is(':checked')) {
                var generator = new RandomStringGenerator({ numSpecial: 1 });
                $('#user-create-form #Password').val(generator.generate(8));
                $('#user-create-form #Password').attr('type', 'text');
            }
            else {
                $('#user-create-form #Password').val('');
                $('#user-create-form #Password').attr('type', 'password');
            }
        };
        UsersController.prototype.toggleUserRole = function () {
            if ($(this).is(':checked')) {
                $.post($(this).data('url'), { role: $(this).val(), add: true }, function (data) {
                    Response.process(data);
                });
            }
            else {
                $.post($(this).data('url'), { role: $(this).val(), add: false }, function (data) {
                    Response.process(data);
                });
            }
        };
        UsersController.prototype.resetPassword = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.prompt({
                // Confirm options...
                title: "Reset Password",
                html: "Please enter a new password for the user...",
                preConfirm: function (inputValue) {
                    if (inputValue === false)
                        return false;
                    if (inputValue === "") {
                        swal__default['default'].showValidationMessage("You didn't supply a new password, we can't reset the password without it!");
                        return false;
                    }
                }
            }, function (result) {
                if (result.isDismissed) {
                    return false;
                }
                $.post(e.currentTarget.href, { password: result.value }, function (data) {
                    Response.process(data, 5000);
                });
            }.bind(this));
        };
        UsersController.prototype.addNote = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.prompt({
                // Confirm options...
                title: "Add a note",
                html: "Enter and store a note about this user.",
                input: 'textarea',
                preConfirm: function (inputValue) {
                    if (inputValue === false)
                        return false;
                    if (inputValue === "") {
                        swal__default['default'].showValidationMessage("You didn't enter a note!");
                        return false;
                    }
                }
            }, function (result) {
                if (result.isDismissed) {
                    return false;
                }
                $.post(e.currentTarget.href, { note: result.value }, function (data) {
                    Response.process(data, 5000);
                    if (this.notesList) {
                        this.notesList.reload();
                    }
                }.bind(this));
            }.bind(this));
        };
        UsersController.prototype.deleteNote = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The note will be permanently removed."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.notesList) {
                            this.notesList.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        return UsersController;
    }());

    var ThemesController = /** @class */ (function () {
        function ThemesController() {
            $('body').on('click', '.activate-theme', this.activate.bind(this));
            this.element = document.getElementById('themes-list');
            if (this.element) {
                this.list = new DataList(this.element, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading users list.', 'info');
                    }.bind(this)
                });
            }
        }
        ThemesController.prototype.activate = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The site will change themes, and the selected theme will be live right away."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        setTimeout(function () {
                            if (this.list) {
                                this.list.reload();
                            }
                        }.bind(this), 2000);
                    }.bind(this));
                }
            }.bind(this));
        };
        return ThemesController;
    }());

    var ContentTypeController = /** @class */ (function () {
        function ContentTypeController() {
            this.manage();
            $('body').on('click', '.content-type-create', this.create.bind(this));
            $('body').on('click', '.content-type-delete', this.delete.bind(this));
            $('body').on('click', '.content-type-set-status', this.setStatus.bind(this));
            $('body').on('click', '.content-custom-field-create', this.createField.bind(this));
            $('body').on('click', '.content-custom-field-delete', this.deleteField.bind(this));
            $('body').on('keyup', '#Slug', function () {
                var slugValue = $(this).val();
                $('.slug-display').html(slugValue);
            });
        }
        ContentTypeController.prototype.manage = function () {
            this.element = document.getElementById('content-type-list');
            if (this.element) {
                this.list = new DataList(this.element, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading content type list.', 'info');
                    }.bind(this)
                });
            }
            this.fieldsElement = document.getElementById('content-custom-field-list');
            if (this.fieldsElement) {
                this.fieldsList = new DataList(this.fieldsElement, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading content type list.', 'info');
                    }.bind(this)
                });
            }
        };
        ContentTypeController.prototype.create = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createContentModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('content-type-create-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (this.list) {
                                this.list.reload();
                            }
                            if (response.success) {
                                createContentModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createContentModal.show($(e.currentTarget).attr('href'), this.element);
        };
        ContentTypeController.prototype.delete = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The content type will be permanently removed. Content will remain, but will be unusable and marked as the old content type."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                        if (e.currentTarget.dataset.redirect) {
                            Alerts.message('Just taking you back to the content list.', 'Redirecting...');
                            setTimeout(function () {
                                window.location = e.currentTarget.dataset.redirect;
                            }, 1500);
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        ContentTypeController.prototype.setStatus = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The change will happen immediately."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.list) {
                            this.list.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        ContentTypeController.prototype.createField = function (e) {
            e.preventDefault();
            e.stopPropagation();
            var createContentModal = new ModalController({
                onComplete: function () {
                    var form = document.getElementById('content-custom-field-create-form');
                    new Validator(form, {
                        onComplete: function (response) {
                            Response.process(response, 5000);
                            if (this.fieldsList) {
                                this.fieldsList.reload();
                            }
                            if (response.success) {
                                createContentModal.close();
                            }
                        }.bind(this)
                    });
                }.bind(this)
            });
            createContentModal.show($(e.currentTarget).attr('href'), this.element);
        };
        ContentTypeController.prototype.deleteField = function (e) {
            e.preventDefault();
            e.stopPropagation();
            Alerts.confirm({
                // Confirm options...
                title: "Are you sure?",
                html: "The field will be permanently removed. However fields will still be attached to content."
            }, function (result) {
                if (result.isConfirmed) {
                    Inline.post(e.currentTarget.href, e.currentTarget, function (data) {
                        Response.process(data, 5000);
                        if (this.fieldsList) {
                            this.fieldsList.reload();
                        }
                    }.bind(this));
                }
            }.bind(this));
        };
        return ContentTypeController;
    }());

    var LogsController = /** @class */ (function () {
        function LogsController() {
            this.element = document.getElementById('log-list');
            if (this.element) {
                this.list = new DataList(this.element, {
                    onComplete: function (data, sender) {
                        Alerts.log('Finished loading logs list.', 'info');
                    }.bind(this)
                });
            }
        }
        return LogsController;
    }());

    var PropertyImporter = /** @class */ (function () {
        function PropertyImporter() {
            if ($('#import-property-start').length > 0) {
                this.update();
                $('#import-property-start').click(function () {
                    $.ajax({
                        url: $('#import-property-start').data('url'),
                        type: "POST",
                        error: Inline.handleError,
                        success: function () {
                            this.update();
                        }.bind(this)
                    });
                }.bind(this));
                $('#import-property-cancel').click(function () {
                    $.ajax({
                        url: $('#import-property-cancel').data('url'),
                        type: "POST",
                        error: Inline.handleError,
                        success: function () {
                            this.update();
                        }.bind(this)
                    });
                }.bind(this));
            }
        }
        PropertyImporter.prototype.update = function () {
            $.ajax({
                url: $('#import-property-status').data('url'),
                type: "POST",
                error: Inline.handleError,
                success: function (result) {
                    if (result.importer.running) {
                        this.showInfo();
                        clearInterval(this.updateInterval);
                        this.updateInterval = setTimeout(this.update, 250);
                    }
                    else {
                        clearInterval(this.updateInterval);
                        this.hideInfo();
                    }
                    $('.tp').html(result.importer.total.toString());
                    $('#pu').html(result.importer.updated.toString());
                    $('#pa').html(result.importer.added.toString());
                    $('#pp').html(result.importer.processed.toString());
                    $('#pd').html(result.importer.deleted.toString());
                    $('#ToAdd').html(result.importer.toAdd.toString());
                    $('#ToUpdate').html(result.importer.toUpdate.toString());
                    $('#ToDelete').html(result.importer.toDelete.toString());
                    $('#pt').html(result.importer.statusMessage.toString());
                    var ftpPercentComplete = Math.round(result.ftp.complete * 100) / 100;
                    $('#fp').html(ftpPercentComplete.toString());
                    $('#ft').html(result.ftp.statusMessage);
                    var percentComplete = Math.round(result.importer.complete * 100) / 100;
                    $('.pc').html(percentComplete.toString());
                    $('#progressbar').css({
                        width: result.importer.complete + "%"
                    });
                    if (result.importer.errors.length) {
                        var errorHtml = "";
                        for (var i = result.importer.errors.length - 1; i >= 0; i--) {
                            errorHtml += '<div class="text-danger">' + result.importer.errors[i] + '</div>';
                        }
                        $('#import-property-errors').html(errorHtml);
                    }
                    else {
                        $('#import-property-errors').html("<div>No errors reported.</div>");
                    }
                    if (result.importer.warnings.length) {
                        var warningHtml = "";
                        for (var j = result.importer.warnings.length - 1; j >= 0; j--) {
                            warningHtml += '<div class="text-warning">' + result.importer.warnings[j] + '</div>';
                        }
                        $('#import-property-warnings').html(warningHtml);
                    }
                    else {
                        $('#import-property-warnings').html("<div>No warnings reported.</div>");
                    }
                }.bind(this)
            });
        };
        PropertyImporter.prototype.hideInfo = function () {
            $('#import-property-start').removeAttr('disabled');
            $('#import-property-cancel').attr('disabled', 'disabled');
            $('#import-property-progress').removeClass('d-block');
            $('#import-property-progress').addClass('d-none');
        };
        PropertyImporter.prototype.showInfo = function () {
            $('#import-property-cancel').removeAttr('disabled');
            $('#import-property-start').attr('disabled', 'disabled');
            $('#import-property-progress').addClass('d-block');
            $('#import-property-progress').removeClass('d-none');
        };
        return PropertyImporter;
    }());

    var Editors = /** @class */ (function () {
        function Editors(options) {
            this.options = {
                linkClasses: [
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
                imageClasses: [
                    { title: 'None', value: '' },
                    { title: 'Full Width', value: 'user-image full' },
                    { title: 'Left Aligned', value: 'user-image left' },
                    { title: 'Centered', value: 'user-image center' },
                    { title: 'Right Aligned', value: 'user-image right' },
                    { title: 'Inline with text, top aligned', value: 'user-image inline top' },
                    { title: 'Inline with text, middle aligned', value: 'user-image inline' },
                    { title: 'Inline with text, bottom aligned', value: 'user-image inline bottom' },
                    { title: 'Pulled Left', value: 'user-image pull-left' },
                    { title: 'Pulled Right', value: 'user-image pull-right' },
                ]
            };
            this.options = __assign(__assign({}, this.options), options);
            this.richTextEditors();
        }
        Editors.prototype.richTextEditors = function () {
            tinymce__default['default'].init({
                selector: '.tinymce-full',
                height: 150,
                menubar: false,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
                link_class_list: this.options.linkClasses,
                image_class_list: this.options.imageClasses,
                setup: this.setupCommands.bind(this),
                image_dimensions: false,
                content_css: [
                    '/dist/css/editor.css'
                ],
            });
            tinymce__default['default'].init({
                selector: '.tinymce-simple',
                height: 150,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                menubar: false,
                toolbar: 'bold italic | bullist numlist | undo redo | link',
                link_class_list: this.options.linkClasses,
                image_class_list: this.options.imageClasses,
                setup: this.setupCommands.bind(this),
                image_dimensions: false
            });
            tinymce__default['default'].init({
                selector: '.tinymce-full-content',
                height: 500,
                menubar: false,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
                link_class_list: this.options.linkClasses,
                image_class_list: this.options.imageClasses,
                setup: this.setupCommands.bind(this),
                image_dimensions: false
            });
            tinymce__default['default'].init({
                selector: '.tinymce-simple-content',
                height: 500,
                plugins: [
                    'advlist autolink lists link image charmap print preview anchor media',
                    'searchreplace visualblocks code fullscreen',
                    'insertdatetime media contextmenu paste code'
                ],
                menubar: false,
                toolbar: 'bold italic | bullist numlist | undo redo | link',
                link_class_list: this.options.linkClasses,
                image_class_list: this.options.imageClasses,
                image_dimensions: false
            });
        };
        Editors.prototype.setupCommands = function (editor) {
            this.currentEditor = $('#' + editor.id);
            if (this.currentEditor.data('hoodMediaList')) {
                editor.addButton('hoodimage', {
                    text: 'Insert image...',
                    icon: false,
                    onclick: function (e) {
                        this.mediaModal = new ModalController({
                            onComplete: function (sender) {
                                this.list = document.getElementById('media-list');
                                this.service = new MediaService(this.list, {
                                    action: 'insert',
                                    url: this.currentEditor.data('hoodMediaList'),
                                    targetEditor: editor,
                                    size: this.currentEditor.data('hoodMediaSize'),
                                    beforeAction: function (sender, mediaObject) {
                                    }.bind(this),
                                    onAction: function (sender, mediaObject) {
                                        this.mediaModal.close();
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
                        this.mediaModal.show(this.currentEditor.data('hoodMediaList'), e.currentTarget);
                    }.bind(this)
                });
            }
        };
        return Editors;
    }());

    var ColorPickers = /** @class */ (function () {
        function ColorPickers(tag) {
            if (tag === void 0) { tag = '.color-picker'; }
            var updateColorFieldValue = function (color, eventSource, instance) {
                var elemId = $(instance._root.button).parent().data('target');
                $(instance._root.button).parent().css({ 'background-color': color.toHEXA().toString() });
                var colorHex = instance.getColor().toHEXA();
                var result = "";
                for (var i = colorHex.length - 1; i >= 0; i--) {
                    result = colorHex[i] + result;
                }
                $(elemId).val('#' + result);
                $(elemId).change();
            };
            // Simple example, see optional options for more configuration.
            $(tag).each(function (index, elem) {
                var lockOpacity = true;
                if ($(this).data('opacity') == 'true') {
                    lockOpacity = false;
                }
                Pickr__default['default'].create({
                    el: elem.children[0],
                    appClass: 'custom-class',
                    theme: 'monolith',
                    useAsButton: true,
                    default: $(this).data('default') || 'none',
                    lockOpacity: lockOpacity,
                    defaultRepresentation: 'HEXA',
                    position: 'bottom-end',
                    components: {
                        opacity: true,
                        hue: true,
                        interaction: {
                            hex: false,
                            rgba: false,
                            hsva: false,
                            input: true,
                            clear: true
                        }
                    }
                })
                    .on('init', function (instance) {
                    var elemId = $(instance._root.button).parent().data('target');
                    var value = $(elemId).val();
                    $(instance._root.button).on('click', $.proxy(function () {
                        this.show();
                    }, instance));
                    $(elemId).on('click', $.proxy(function () {
                        this.show();
                    }, instance));
                    if (value) {
                        instance.setColor(value);
                        updateColorFieldValue(instance.getColor(), null, instance);
                    }
                })
                    .on('clear', function (instance) {
                    var elemId = $(instance._root.button).parent().data('target');
                    instance.setColor('transparent');
                    updateColorFieldValue(instance.getColor(), null, instance);
                    $(elemId).val('');
                    $(elemId).change();
                })
                    .on('change', updateColorFieldValue);
            });
        }
        return ColorPickers;
    }());

    var Admin = /** @class */ (function (_super) {
        __extends(Admin, _super);
        function Admin() {
            var _this = _super.call(this) || this;
            // Admin Controllers
            _this.home = new HomeController();
            _this.media = new MediaController();
            _this.content = new ContentController();
            _this.contentType = new ContentTypeController();
            _this.logs = new LogsController();
            _this.property = new PropertyController();
            _this.propertyImporter = new PropertyImporter();
            _this.themes = new ThemesController();
            _this.users = new UsersController();
            new Editors();
            new ColorPickers();
            if ($('#page-tabs').length > 0) {
                _this.checkAndLoadTabs('#page-tabs');
            }
            // Admin Handlers
            $('.restrict-to-slug').restrictToSlug();
            $('.restrict-to-page-slug').restrictToPageSlug();
            $('.restrict-to-meta-slug').restrictToMetaSlug();
            $('.character-counter').characterCounter();
            $('.character-counter').trigger('change');
            $('.warning-alert').warningAlert();
            $('.mobile-sidebar-toggle').on('click', function () {
                $('nav.sidebar').toggleClass('open');
            });
            $('.right-sidebar-toggle').on('click', function () {
                $('#right-sidebar').toggleClass('sidebar-open');
            });
            $(".alert.auto-dismiss").fadeTo(5000, 500).slideUp(500, function () {
                $(".alert.auto-dismiss").slideUp(500);
            });
            $('.sidebar-scroll').slimScroll({
                height: '100%',
                railOpacity: 0.4,
                wheelStep: 10
            });
            return _this;
        }
        Admin.prototype.checkAndLoadTabs = function (tag) {
            $(tag + ' a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
                var store = JSON.parse(localStorage.getItem('tabs-' + tag)) || {};
                store['tab-' + $(tag).data('hoodTabs')] = $(e.target).attr('href');
                localStorage.setItem('tabs-' + tag, JSON.stringify(store));
            });
            var store = JSON.parse(localStorage.getItem('tabs-' + tag)) || {};
            var activeTab = store['tab-' + $(tag).data('hoodTabs')];
            var triggerEl = $(tag + ' a[data-bs-toggle="tab"]')[0];
            if (activeTab) {
                triggerEl = document.querySelector(tag + ' a[href="' + activeTab + '"]');
            }
            var tabTrigger = new bootstrap__namespace.Tab(triggerEl);
            tabTrigger.show();
        };
        return Admin;
    }(BaseController));
    var app = new Admin();

    exports.Alerts = Alerts;
    exports.app = app;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

}({}, Swal, bootstrap, Chart, tinymce, Pickr));
//# sourceMappingURL=admin.js.map

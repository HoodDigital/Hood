!function(t){"function"==typeof define&&define.amd?define(["jquery"],t):t(jQuery)}(function(t){t.extend(t.fn,{validate:function(e){if(!this.length)return void(e&&e.debug&&window.console&&console.warn("Nothing selected, can't validate, returning nothing."));var i=t.data(this[0],"validator");return i?i:(this.attr("novalidate","novalidate"),i=new t.validator(e,this[0]),t.data(this[0],"validator",i),i.settings.onsubmit&&(this.on("click.validate",":submit",function(e){i.settings.submitHandler&&(i.submitButton=e.target),t(this).hasClass("cancel")&&(i.cancelSubmit=!0),void 0!==t(this).attr("formnovalidate")&&(i.cancelSubmit=!0)}),this.on("submit.validate",function(e){function n(){var n,r;return!i.settings.submitHandler||(i.submitButton&&(n=t("<input type='hidden'/>").attr("name",i.submitButton.name).val(t(i.submitButton).val()).appendTo(i.currentForm)),r=i.settings.submitHandler.call(i,i.currentForm,e),i.submitButton&&n.remove(),void 0!==r&&r)}return i.settings.debug&&e.preventDefault(),i.cancelSubmit?(i.cancelSubmit=!1,n()):i.form()?i.pendingRequest?(i.formSubmitted=!0,!1):n():(i.focusInvalid(),!1)})),i)},valid:function(){var e,i,n;return t(this[0]).is("form")?e=this.validate().form():(n=[],e=!0,i=t(this[0].form).validate(),this.each(function(){e=i.element(this)&&e,n=n.concat(i.errorList)}),i.errorList=n),e},rules:function(e,i){var n,r,a,s,o,l,d=this[0];if(e)switch(n=t.data(d.form,"validator").settings,r=n.rules,a=t.validator.staticRules(d),e){case"add":t.extend(a,t.validator.normalizeRule(i)),delete a.messages,r[d.name]=a,i.messages&&(n.messages[d.name]=t.extend(n.messages[d.name],i.messages));break;case"remove":return i?(l={},t.each(i.split(/\s/),function(e,i){l[i]=a[i],delete a[i],"required"===i&&t(d).removeAttr("aria-required")}),l):(delete r[d.name],a)}return s=t.validator.normalizeRules(t.extend({},t.validator.classRules(d),t.validator.attributeRules(d),t.validator.dataRules(d),t.validator.staticRules(d)),d),s.required&&(o=s.required,delete s.required,s=t.extend({required:o},s),t(d).attr("aria-required","true")),s.remote&&(o=s.remote,delete s.remote,s=t.extend(s,{remote:o})),s}}),t.extend(t.expr[":"],{blank:function(e){return!t.trim(""+t(e).val())},filled:function(e){return!!t.trim(""+t(e).val())},unchecked:function(e){return!t(e).prop("checked")}}),t.validator=function(e,i){this.settings=t.extend(!0,{},t.validator.defaults,e),this.currentForm=i,this.init()},t.validator.format=function(e,i){return 1===arguments.length?function(){var i=t.makeArray(arguments);return i.unshift(e),t.validator.format.apply(this,i)}:(arguments.length>2&&i.constructor!==Array&&(i=t.makeArray(arguments).slice(1)),i.constructor!==Array&&(i=[i]),t.each(i,function(t,i){e=e.replace(new RegExp("\\{"+t+"\\}","g"),function(){return i})}),e)},t.extend(t.validator,{defaults:{messages:{},groups:{},rules:{},errorClass:"error",validClass:"valid",errorElement:"label",focusCleanup:!1,focusInvalid:!0,errorContainer:t([]),errorLabelContainer:t([]),onsubmit:!0,ignore:":hidden",ignoreTitle:!1,onfocusin:function(t){this.lastActive=t,this.settings.focusCleanup&&(this.settings.unhighlight&&this.settings.unhighlight.call(this,t,this.settings.errorClass,this.settings.validClass),this.hideThese(this.errorsFor(t)))},onfocusout:function(t){this.checkable(t)||!(t.name in this.submitted)&&this.optional(t)||this.element(t)},onkeyup:function(e,i){var n=[16,17,18,20,35,36,37,38,39,40,45,144,225];9===i.which&&""===this.elementValue(e)||-1!==t.inArray(i.keyCode,n)||(e.name in this.submitted||e===this.lastElement)&&this.element(e)},onclick:function(t){t.name in this.submitted?this.element(t):t.parentNode.name in this.submitted&&this.element(t.parentNode)},highlight:function(e,i,n){"radio"===e.type?this.findByName(e.name).addClass(i).removeClass(n):t(e).addClass(i).removeClass(n)},unhighlight:function(e,i,n){"radio"===e.type?this.findByName(e.name).removeClass(i).addClass(n):t(e).removeClass(i).addClass(n)}},setDefaults:function(e){t.extend(t.validator.defaults,e)},messages:{required:"This field is required.",remote:"Please fix this field.",email:"Please enter a valid email address.",url:"Please enter a valid URL.",date:"Please enter a valid date.",dateISO:"Please enter a valid date ( ISO ).",number:"Please enter a valid number.",digits:"Please enter only digits.",creditcard:"Please enter a valid credit card number.",equalTo:"Please enter the same value again.",maxlength:t.validator.format("Please enter no more than {0} characters."),minlength:t.validator.format("Please enter at least {0} characters."),rangelength:t.validator.format("Please enter a value between {0} and {1} characters long."),range:t.validator.format("Please enter a value between {0} and {1}."),max:t.validator.format("Please enter a value less than or equal to {0}."),min:t.validator.format("Please enter a value greater than or equal to {0}.")},autoCreateRanges:!1,prototype:{init:function(){function e(e){var i=t.data(this.form,"validator"),n="on"+e.type.replace(/^validate/,""),r=i.settings;r[n]&&!t(this).is(r.ignore)&&r[n].call(i,this,e)}this.labelContainer=t(this.settings.errorLabelContainer),this.errorContext=this.labelContainer.length&&this.labelContainer||t(this.currentForm),this.containers=t(this.settings.errorContainer).add(this.settings.errorLabelContainer),this.submitted={},this.valueCache={},this.pendingRequest=0,this.pending={},this.invalid={},this.reset();var i,n=this.groups={};t.each(this.settings.groups,function(e,i){"string"==typeof i&&(i=i.split(/\s/)),t.each(i,function(t,i){n[i]=e})}),i=this.settings.rules,t.each(i,function(e,n){i[e]=t.validator.normalizeRule(n)}),t(this.currentForm).on("focusin.validate focusout.validate keyup.validate",":text, [type='password'], [type='file'], select, textarea, [type='number'], [type='search'], [type='tel'], [type='url'], [type='email'], [type='datetime'], [type='date'], [type='month'], [type='week'], [type='time'], [type='datetime-local'], [type='range'], [type='color'], [type='radio'], [type='checkbox']",e).on("click.validate","select, option, [type='radio'], [type='checkbox']",e),this.settings.invalidHandler&&t(this.currentForm).on("invalid-form.validate",this.settings.invalidHandler),t(this.currentForm).find("[required], [data-rule-required], .required").attr("aria-required","true")},form:function(){return this.checkForm(),t.extend(this.submitted,this.errorMap),this.invalid=t.extend({},this.errorMap),this.valid()||t(this.currentForm).triggerHandler("invalid-form",[this]),this.showErrors(),this.valid()},checkForm:function(){this.prepareForm();for(var t=0,e=this.currentElements=this.elements();e[t];t++)this.check(e[t]);return this.valid()},element:function(e){var i=this.clean(e),n=this.validationTargetFor(i),r=!0;return this.lastElement=n,void 0===n?delete this.invalid[i.name]:(this.prepareElement(n),this.currentElements=t(n),r=this.check(n)!==!1,r?delete this.invalid[n.name]:this.invalid[n.name]=!0),t(e).attr("aria-invalid",!r),this.numberOfInvalids()||(this.toHide=this.toHide.add(this.containers)),this.showErrors(),r},showErrors:function(e){if(e){t.extend(this.errorMap,e),this.errorList=[];for(var i in e)this.errorList.push({message:e[i],element:this.findByName(i)[0]});this.successList=t.grep(this.successList,function(t){return!(t.name in e)})}this.settings.showErrors?this.settings.showErrors.call(this,this.errorMap,this.errorList):this.defaultShowErrors()},resetForm:function(){t.fn.resetForm&&t(this.currentForm).resetForm(),this.submitted={},this.lastElement=null,this.prepareForm(),this.hideErrors();var e,i=this.elements().removeData("previousValue").removeAttr("aria-invalid");if(this.settings.unhighlight)for(e=0;i[e];e++)this.settings.unhighlight.call(this,i[e],this.settings.errorClass,"");else i.removeClass(this.settings.errorClass)},numberOfInvalids:function(){return this.objectLength(this.invalid)},objectLength:function(t){var e,i=0;for(e in t)i++;return i},hideErrors:function(){this.hideThese(this.toHide)},hideThese:function(t){t.not(this.containers).text(""),this.addWrapper(t).hide()},valid:function(){return 0===this.size()},size:function(){return this.errorList.length},focusInvalid:function(){if(this.settings.focusInvalid)try{t(this.findLastActive()||this.errorList.length&&this.errorList[0].element||[]).filter(":visible").focus().trigger("focusin")}catch(t){}},findLastActive:function(){var e=this.lastActive;return e&&1===t.grep(this.errorList,function(t){return t.element.name===e.name}).length&&e},elements:function(){var e=this,i={};return t(this.currentForm).find("input, select, textarea").not(":submit, :reset, :image, :disabled").not(this.settings.ignore).filter(function(){return!this.name&&e.settings.debug&&window.console&&console.error("%o has no name assigned",this),!(this.name in i||!e.objectLength(t(this).rules()))&&(i[this.name]=!0,!0)})},clean:function(e){return t(e)[0]},errors:function(){var e=this.settings.errorClass.split(" ").join(".");return t(this.settings.errorElement+"."+e,this.errorContext)},reset:function(){this.successList=[],this.errorList=[],this.errorMap={},this.toShow=t([]),this.toHide=t([]),this.currentElements=t([])},prepareForm:function(){this.reset(),this.toHide=this.errors().add(this.containers)},prepareElement:function(t){this.reset(),this.toHide=this.errorsFor(t)},elementValue:function(e){var i,n=t(e),r=e.type;return"radio"===r||"checkbox"===r?this.findByName(e.name).filter(":checked").val():"number"===r&&"undefined"!=typeof e.validity?!e.validity.badInput&&n.val():(i=n.val(),"string"==typeof i?i.replace(/\r/g,""):i)},check:function(e){e=this.validationTargetFor(this.clean(e));var i,n,r,a=t(e).rules(),s=t.map(a,function(t,e){return e}).length,o=!1,l=this.elementValue(e);for(n in a){r={method:n,parameters:a[n]};try{if(i=t.validator.methods[n].call(this,l,e,r.parameters),"dependency-mismatch"===i&&1===s){o=!0;continue}if(o=!1,"pending"===i)return void(this.toHide=this.toHide.not(this.errorsFor(e)));if(!i)return this.formatAndAdd(e,r),!1}catch(t){throw this.settings.debug&&window.console&&console.log("Exception occurred when checking element "+e.id+", check the '"+r.method+"' method.",t),t instanceof TypeError&&(t.message+=".  Exception occurred when checking element "+e.id+", check the '"+r.method+"' method."),t}}if(!o)return this.objectLength(a)&&this.successList.push(e),!0},customDataMessage:function(e,i){return t(e).data("msg"+i.charAt(0).toUpperCase()+i.substring(1).toLowerCase())||t(e).data("msg")},customMessage:function(t,e){var i=this.settings.messages[t];return i&&(i.constructor===String?i:i[e])},findDefined:function(){for(var t=0;t<arguments.length;t++)if(void 0!==arguments[t])return arguments[t]},defaultMessage:function(e,i){return this.findDefined(this.customMessage(e.name,i),this.customDataMessage(e,i),!this.settings.ignoreTitle&&e.title||void 0,t.validator.messages[i],"<strong>Warning: No message defined for "+e.name+"</strong>")},formatAndAdd:function(e,i){var n=this.defaultMessage(e,i.method),r=/\$?\{(\d+)\}/g;"function"==typeof n?n=n.call(this,i.parameters,e):r.test(n)&&(n=t.validator.format(n.replace(r,"{$1}"),i.parameters)),this.errorList.push({message:n,element:e,method:i.method}),this.errorMap[e.name]=n,this.submitted[e.name]=n},addWrapper:function(t){return this.settings.wrapper&&(t=t.add(t.parent(this.settings.wrapper))),t},defaultShowErrors:function(){var t,e,i;for(t=0;this.errorList[t];t++)i=this.errorList[t],this.settings.highlight&&this.settings.highlight.call(this,i.element,this.settings.errorClass,this.settings.validClass),this.showLabel(i.element,i.message);if(this.errorList.length&&(this.toShow=this.toShow.add(this.containers)),this.settings.success)for(t=0;this.successList[t];t++)this.showLabel(this.successList[t]);if(this.settings.unhighlight)for(t=0,e=this.validElements();e[t];t++)this.settings.unhighlight.call(this,e[t],this.settings.errorClass,this.settings.validClass);this.toHide=this.toHide.not(this.toShow),this.hideErrors(),this.addWrapper(this.toShow).show()},validElements:function(){return this.currentElements.not(this.invalidElements())},invalidElements:function(){return t(this.errorList).map(function(){return this.element})},showLabel:function(e,i){var n,r,a,s=this.errorsFor(e),o=this.idOrName(e),l=t(e).attr("aria-describedby");s.length?(s.removeClass(this.settings.validClass).addClass(this.settings.errorClass),s.html(i)):(s=t("<"+this.settings.errorElement+">").attr("id",o+"-error").addClass(this.settings.errorClass).html(i||""),n=s,this.settings.wrapper&&(n=s.hide().show().wrap("<"+this.settings.wrapper+"/>").parent()),this.labelContainer.length?this.labelContainer.append(n):this.settings.errorPlacement?this.settings.errorPlacement(n,t(e)):n.insertAfter(e),s.is("label")?s.attr("for",o):0===s.parents("label[for='"+o+"']").length&&(a=s.attr("id").replace(/(:|\.|\[|\]|\$)/g,"\\$1"),l?l.match(new RegExp("\\b"+a+"\\b"))||(l+=" "+a):l=a,t(e).attr("aria-describedby",l),r=this.groups[e.name],r&&t.each(this.groups,function(e,i){i===r&&t("[name='"+e+"']",this.currentForm).attr("aria-describedby",s.attr("id"))}))),!i&&this.settings.success&&(s.text(""),"string"==typeof this.settings.success?s.addClass(this.settings.success):this.settings.success(s,e)),this.toShow=this.toShow.add(s)},errorsFor:function(e){var i=this.idOrName(e),n=t(e).attr("aria-describedby"),r="label[for='"+i+"'], label[for='"+i+"'] *";return n&&(r=r+", #"+n.replace(/\s+/g,", #")),this.errors().filter(r)},idOrName:function(t){return this.groups[t.name]||(this.checkable(t)?t.name:t.id||t.name)},validationTargetFor:function(e){return this.checkable(e)&&(e=this.findByName(e.name)),t(e).not(this.settings.ignore)[0]},checkable:function(t){return/radio|checkbox/i.test(t.type)},findByName:function(e){return t(this.currentForm).find("[name='"+e+"']")},getLength:function(e,i){switch(i.nodeName.toLowerCase()){case"select":return t("option:selected",i).length;case"input":if(this.checkable(i))return this.findByName(i.name).filter(":checked").length}return e.length},depend:function(t,e){return!this.dependTypes[typeof t]||this.dependTypes[typeof t](t,e)},dependTypes:{boolean:function(t){return t},string:function(e,i){return!!t(e,i.form).length},function:function(t,e){return t(e)}},optional:function(e){var i=this.elementValue(e);return!t.validator.methods.required.call(this,i,e)&&"dependency-mismatch"},startRequest:function(t){this.pending[t.name]||(this.pendingRequest++,this.pending[t.name]=!0)},stopRequest:function(e,i){this.pendingRequest--,this.pendingRequest<0&&(this.pendingRequest=0),delete this.pending[e.name],i&&0===this.pendingRequest&&this.formSubmitted&&this.form()?(t(this.currentForm).submit(),this.formSubmitted=!1):!i&&0===this.pendingRequest&&this.formSubmitted&&(t(this.currentForm).triggerHandler("invalid-form",[this]),this.formSubmitted=!1)},previousValue:function(e){return t.data(e,"previousValue")||t.data(e,"previousValue",{old:null,valid:!0,message:this.defaultMessage(e,"remote")})},destroy:function(){this.resetForm(),t(this.currentForm).off(".validate").removeData("validator")}},classRuleSettings:{required:{required:!0},email:{email:!0},url:{url:!0},date:{date:!0},dateISO:{dateISO:!0},number:{number:!0},digits:{digits:!0},creditcard:{creditcard:!0}},addClassRules:function(e,i){e.constructor===String?this.classRuleSettings[e]=i:t.extend(this.classRuleSettings,e)},classRules:function(e){var i={},n=t(e).attr("class");return n&&t.each(n.split(" "),function(){this in t.validator.classRuleSettings&&t.extend(i,t.validator.classRuleSettings[this])}),i},normalizeAttributeRule:function(t,e,i,n){/min|max/.test(i)&&(null===e||/number|range|text/.test(e))&&(n=Number(n),isNaN(n)&&(n=void 0)),n||0===n?t[i]=n:e===i&&"range"!==e&&(t[i]=!0)},attributeRules:function(e){var i,n,r={},a=t(e),s=e.getAttribute("type");for(i in t.validator.methods)"required"===i?(n=e.getAttribute(i),""===n&&(n=!0),n=!!n):n=a.attr(i),this.normalizeAttributeRule(r,s,i,n);return r.maxlength&&/-1|2147483647|524288/.test(r.maxlength)&&delete r.maxlength,r},dataRules:function(e){var i,n,r={},a=t(e),s=e.getAttribute("type");for(i in t.validator.methods)n=a.data("rule"+i.charAt(0).toUpperCase()+i.substring(1).toLowerCase()),this.normalizeAttributeRule(r,s,i,n);return r},staticRules:function(e){var i={},n=t.data(e.form,"validator");return n.settings.rules&&(i=t.validator.normalizeRule(n.settings.rules[e.name])||{}),i},normalizeRules:function(e,i){return t.each(e,function(n,r){if(r===!1)return void delete e[n];if(r.param||r.depends){var a=!0;switch(typeof r.depends){case"string":a=!!t(r.depends,i.form).length;break;case"function":a=r.depends.call(i,i)}a?e[n]=void 0===r.param||r.param:delete e[n]}}),t.each(e,function(n,r){e[n]=t.isFunction(r)?r(i):r}),t.each(["minlength","maxlength"],function(){e[this]&&(e[this]=Number(e[this]))}),t.each(["rangelength","range"],function(){var i;e[this]&&(t.isArray(e[this])?e[this]=[Number(e[this][0]),Number(e[this][1])]:"string"==typeof e[this]&&(i=e[this].replace(/[\[\]]/g,"").split(/[\s,]+/),e[this]=[Number(i[0]),Number(i[1])]))}),t.validator.autoCreateRanges&&(null!=e.min&&null!=e.max&&(e.range=[e.min,e.max],delete e.min,delete e.max),null!=e.minlength&&null!=e.maxlength&&(e.rangelength=[e.minlength,e.maxlength],delete e.minlength,delete e.maxlength)),e},normalizeRule:function(e){if("string"==typeof e){var i={};t.each(e.split(/\s/),function(){i[this]=!0}),e=i}return e},addMethod:function(e,i,n){t.validator.methods[e]=i,t.validator.messages[e]=void 0!==n?n:t.validator.messages[e],i.length<3&&t.validator.addClassRules(e,t.validator.normalizeRule(e))},methods:{required:function(e,i,n){if(!this.depend(n,i))return"dependency-mismatch";if("select"===i.nodeName.toLowerCase()){var r=t(i).val();return r&&r.length>0}return this.checkable(i)?this.getLength(e,i)>0:e.length>0},email:function(t,e){return this.optional(e)||/^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/.test(t)},url:function(t,e){return this.optional(e)||/^(?:(?:(?:https?|ftp):)?\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[\/?#]\S*)?$/i.test(t)},date:function(t,e){return this.optional(e)||!/Invalid|NaN/.test(new Date(t).toString())},dateISO:function(t,e){return this.optional(e)||/^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$/.test(t)},number:function(t,e){return this.optional(e)||/^(?:-?\d+|-?\d{1,3}(?:,\d{3})+)?(?:\.\d+)?$/.test(t)},digits:function(t,e){return this.optional(e)||/^\d+$/.test(t)},creditcard:function(t,e){if(this.optional(e))return"dependency-mismatch";if(/[^0-9 \-]+/.test(t))return!1;var i,n,r=0,a=0,s=!1;if(t=t.replace(/\D/g,""),t.length<13||t.length>19)return!1;for(i=t.length-1;i>=0;i--)n=t.charAt(i),a=parseInt(n,10),s&&(a*=2)>9&&(a-=9),r+=a,s=!s;return r%10===0},minlength:function(e,i,n){var r=t.isArray(e)?e.length:this.getLength(e,i);return this.optional(i)||r>=n},maxlength:function(e,i,n){var r=t.isArray(e)?e.length:this.getLength(e,i);return this.optional(i)||n>=r},rangelength:function(e,i,n){var r=t.isArray(e)?e.length:this.getLength(e,i);return this.optional(i)||r>=n[0]&&r<=n[1]},min:function(t,e,i){return this.optional(e)||t>=i},max:function(t,e,i){return this.optional(e)||i>=t},range:function(t,e,i){return this.optional(e)||t>=i[0]&&t<=i[1]},equalTo:function(e,i,n){var r=t(n);return this.settings.onfocusout&&r.off(".validate-equalTo").on("blur.validate-equalTo",function(){t(i).valid()}),e===r.val()},remote:function(e,i,n){if(this.optional(i))return"dependency-mismatch";var r,a,s=this.previousValue(i);return this.settings.messages[i.name]||(this.settings.messages[i.name]={}),s.originalMessage=this.settings.messages[i.name].remote,this.settings.messages[i.name].remote=s.message,n="string"==typeof n&&{url:n}||n,s.old===e?s.valid:(s.old=e,r=this,this.startRequest(i),a={},a[i.name]=e,t.ajax(t.extend(!0,{mode:"abort",port:"validate"+i.name,dataType:"json",data:a,context:r.currentForm,success:function(n){var a,o,l,d=n===!0||"true"===n;r.settings.messages[i.name].remote=s.originalMessage,d?(l=r.formSubmitted,r.prepareElement(i),r.formSubmitted=l,r.successList.push(i),delete r.invalid[i.name],r.showErrors()):(a={},o=n||r.defaultMessage(i,"remote"),a[i.name]=s.message=t.isFunction(o)?o(e):o,r.invalid[i.name]=!0,r.showErrors(a)),s.valid=d,r.stopRequest(i,d)}},n)),"pending")}}});var e,i={};t.ajaxPrefilter?t.ajaxPrefilter(function(t,e,n){var r=t.port;"abort"===t.mode&&(i[r]&&i[r].abort(),i[r]=n)}):(e=t.ajax,t.ajax=function(n){var r=("mode"in n?n:t.ajaxSettings).mode,a=("port"in n?n:t.ajaxSettings).port;return"abort"===r?(i[a]&&i[a].abort(),i[a]=e.apply(this,arguments),i[a]):e.apply(this,arguments)})}),!function(t){function e(t,e,i){t.rules[e]=i,t.message&&(t.messages[e]=t.message)}function i(t){return t.replace(/^\s+|\s+$/g,"").split(/\s*,\s*/g)}function n(t){return t.replace(/([!"#$%&'()*+,.\/:;<=>?@\[\\\]^`{|}~])/g,"\\$1")}function r(t){return t.substr(0,t.lastIndexOf(".")+1)}function a(t,e){return 0===t.indexOf("*.")&&(t=t.replace("*.",e)),t}function s(e,i){var r=t(this).find("[data-valmsg-for='"+n(i[0].name)+"']"),a=r.attr("data-valmsg-replace"),s=a?t.parseJSON(a)!==!1:null;r.removeClass("field-validation-valid").addClass("field-validation-error"),e.data("unobtrusiveContainer",r),s?(r.empty(),e.removeClass("input-validation-error").appendTo(r)):e.hide()}function o(e,i){var n=t(this).find("[data-valmsg-summary=true]"),r=n.find("ul");r&&r.length&&i.errorList.length&&(r.empty(),n.addClass("validation-summary-errors").removeClass("validation-summary-valid"),t.each(i.errorList,function(){t("<li />").html(this.message).appendTo(r)}))}function l(e){var i=e.data("unobtrusiveContainer");if(i){var n=i.attr("data-valmsg-replace"),r=n?t.parseJSON(n):null;i.addClass("field-validation-valid").removeClass("field-validation-error"),e.removeData("unobtrusiveContainer"),r&&i.empty()}}function d(e){var i=t(this),n="__jquery_unobtrusive_validation_form_reset";if(!i.data(n)){i.data(n,!0);try{i.data("validator").resetForm()}finally{i.removeData(n)}i.find(".validation-summary-errors").addClass("validation-summary-valid").removeClass("validation-summary-errors"),i.find(".field-validation-error").addClass("field-validation-valid").removeClass("field-validation-error").removeData("unobtrusiveContainer").find(">*").removeData("unobtrusiveContainer")}}function h(e){var i=t(e),n=i.data(m),r=t.proxy(d,e),a=c.unobtrusive.options||{},h=function(i,n){var r=a[i];r&&t.isFunction(r)&&r.apply(e,n)};return n||(n={options:{errorClass:a.errorClass||"input-validation-error",errorElement:a.errorElement||"span",errorPlacement:function(){s.apply(e,arguments),h("errorPlacement",arguments)},invalidHandler:function(){o.apply(e,arguments),h("invalidHandler",arguments)},messages:{},rules:{},success:function(){l.apply(e,arguments),h("success",arguments)}},attachValidation:function(){i.off("reset."+m,r).on("reset."+m,r).validate(this.options)},validate:function(){return i.validate(),i.valid()}},i.data(m,n)),n}var u,c=t.validator,m="unobtrusiveValidation";c.unobtrusive={adapters:[],parseElement:function(e,i){var n,r,a,s=t(e),o=s.parents("form")[0];o&&(n=h(o),n.options.rules[e.name]=r={},n.options.messages[e.name]=a={},t.each(this.adapters,function(){var i="data-val-"+this.name,n=s.attr(i),l={};void 0!==n&&(i+="-",t.each(this.params,function(){l[this]=s.attr(i+this)}),this.adapt({element:e,form:o,message:n,params:l,rules:r,messages:a}))}),t.extend(r,{__dummy__:!0}),i||n.attachValidation())},parse:function(e){var i=t(e),n=i.parents().addBack().filter("form").add(i.find("form")).has("[data-val=true]");i.find("[data-val=true]").each(function(){c.unobtrusive.parseElement(this,!0)}),n.each(function(){var t=h(this);t&&t.attachValidation()})}},u=c.unobtrusive.adapters,u.add=function(t,e,i){return i||(i=e,e=[]),this.push({name:t,params:e,adapt:i}),this},u.addBool=function(t,i){return this.add(t,function(n){e(n,i||t,!0)})},u.addMinMax=function(t,i,n,r,a,s){return this.add(t,[a||"min",s||"max"],function(t){var a=t.params.min,s=t.params.max;a&&s?e(t,r,[a,s]):a?e(t,i,a):s&&e(t,n,s)})},u.addSingleVal=function(t,i,n){return this.add(t,[i||"val"],function(r){e(r,n||t,r.params[i])})},c.addMethod("__dummy__",function(t,e,i){return!0}),c.addMethod("regex",function(t,e,i){var n;return!!this.optional(e)||(n=new RegExp(i).exec(t),n&&0===n.index&&n[0].length===t.length)}),c.addMethod("nonalphamin",function(t,e,i){var n;return i&&(n=t.match(/\W/g),n=n&&n.length>=i),n}),c.methods.extension?(u.addSingleVal("accept","mimtype"),u.addSingleVal("extension","extension")):u.addSingleVal("extension","extension","accept"),u.addSingleVal("regex","pattern"),u.addBool("creditcard").addBool("date").addBool("digits").addBool("email").addBool("number").addBool("url"),u.addMinMax("length","minlength","maxlength","rangelength").addMinMax("range","min","max","range"),u.addMinMax("minlength","minlength").addMinMax("maxlength","minlength","maxlength"),u.add("equalto",["other"],function(i){var s=r(i.element.name),o=i.params.other,l=a(o,s),d=t(i.form).find(":input").filter("[name='"+n(l)+"']")[0];e(i,"equalTo",d)}),u.add("required",function(t){("INPUT"!==t.element.tagName.toUpperCase()||"CHECKBOX"!==t.element.type.toUpperCase())&&e(t,"required",!0)}),u.add("remote",["url","type","additionalfields"],function(s){var o={url:s.params.url,type:s.params.type||"GET",data:{}},l=r(s.element.name);t.each(i(s.params.additionalfields||s.element.name),function(e,i){var r=a(i,l);o.data[r]=function(){var e=t(s.form).find(":input").filter("[name='"+n(r)+"']");return e.is(":checkbox")?e.filter(":checked").val()||e.filter(":hidden").val()||"":e.is(":radio")?e.filter(":checked").val()||"":e.val()}}),e(s,"remote",o)}),u.add("password",["min","nonalphamin","regex"],function(t){t.params.min&&e(t,"minlength",t.params.min),t.params.nonalphamin&&e(t,"nonalphamin",t.params.nonalphamin),t.params.regex&&e(t,"regex",t.params.regex)}),t(function(){c.unobtrusive.parse(document)})}(jQuery);var console={};console.log=function(){},window.console=console,function(t,e,i){t.fn.backstretch=function(n,r){return(n===i||0===n.length)&&t.error("No images were supplied for Backstretch"),0===t(e).scrollTop()&&e.scrollTo(0,0),this.each(function(){var e=t(this),i=e.data("backstretch");if(i){if("string"==typeof n&&"function"==typeof i[n])return void i[n](r);r=t.extend(i.options,r),i.destroy(!0)}i=new a(this,n,r),e.data("backstretch",i)})},t.backstretch=function(e,i){return t("body").backstretch(e,i).data("backstretch")},t.expr[":"].backstretch=function(e){return t(e).data("backstretch")!==i},t.fn.backstretch.defaults={centeredX:!0,centeredY:!0,duration:5e3,fade:0};var n={left:0,top:0,overflow:"hidden",margin:0,padding:0,height:"100%",width:"100%",zIndex:-999999},r={position:"absolute",display:"none",margin:0,padding:0,border:"none",width:"auto",height:"auto",maxHeight:"none",maxWidth:"none",zIndex:-999999},a=function(i,r,a){this.options=t.extend({},t.fn.backstretch.defaults,a||{}),this.images=t.isArray(r)?r:[r],t.each(this.images,function(){t("<img />")[0].src=this}),this.isBody=i===document.body,this.$container=t(i),this.$root=this.isBody?t(s?e:document):this.$container,i=this.$container.children(".backstretch").first(),this.$wrap=i.length?i:t('<div class="backstretch"></div>').css(n).appendTo(this.$container),this.isBody||(i=this.$container.css("position"),r=this.$container.css("zIndex"),this.$container.css({position:"static"===i?"relative":i,zIndex:"auto"===r?0:r,background:"none"}),this.$wrap.css({zIndex:-999998})),this.$wrap.css({position:this.isBody&&s?"fixed":"absolute"}),this.index=0,this.show(this.index),t(e).on("resize.backstretch",t.proxy(this.resize,this)).on("orientationchange.backstretch",t.proxy(function(){this.isBody&&0===e.pageYOffset&&(e.scrollTo(0,1),this.resize())},this))};a.prototype={resize:function(){try{var t,i={left:0,top:0},n=this.isBody?this.$root.width():this.$root.innerWidth(),r=n,a=this.isBody?e.innerHeight?e.innerHeight:this.$root.height():this.$root.innerHeight(),s=r/this.$img.data("ratio");s>=a?(t=(s-a)/2,this.options.centeredY&&(i.top="-"+t+"px")):(s=a,r=s*this.$img.data("ratio"),t=(r-n)/2,this.options.centeredX&&(i.left="-"+t+"px")),this.$wrap.css({width:n,height:a}).find("img:not(.deleteable)").css({width:r,height:s}).css(i)}catch(t){}return this},show:function(e){if(!(Math.abs(e)>this.images.length-1)){var i=this,n=i.$wrap.find("img").addClass("deleteable"),a={relatedTarget:i.$container[0]};return i.$container.trigger(t.Event("backstretch.before",a),[i,e]),this.index=e,clearInterval(i.interval),i.$img=t("<img />").css(r).bind("load",function(r){var s=this.width||t(r.target).width();r=this.height||t(r.target).height(),t(this).data("ratio",s/r),t(this).fadeIn(i.options.speed||i.options.fade,function(){n.remove(),i.paused||i.cycle(),t(["after","show"]).each(function(){i.$container.trigger(t.Event("backstretch."+this,a),[i,e])})}),i.resize()}).appendTo(i.$wrap),i.$img.attr("src",i.images[e]),i}},next:function(){return this.show(this.index<this.images.length-1?this.index+1:0)},prev:function(){return this.show(0===this.index?this.images.length-1:this.index-1)},pause:function(){return this.paused=!0,this},resume:function(){return this.paused=!1,this.next(),this},cycle:function(){return 1<this.images.length&&(clearInterval(this.interval),this.interval=setInterval(t.proxy(function(){this.paused||this.next()},this),this.options.duration)),this},destroy:function(i){t(e).off("resize.backstretch orientationchange.backstretch"),clearInterval(this.interval),i||this.$wrap.remove(),this.$container.removeData("backstretch")}};var s,o=navigator.userAgent,l=navigator.platform,d=o.match(/AppleWebKit\/([0-9]+)/),d=!!d&&d[1],h=o.match(/Fennec\/([0-9]+)/),h=!!h&&h[1],u=o.match(/Opera Mobi\/([0-9]+)/),c=!!u&&u[1],m=o.match(/MSIE ([0-9]+)/),m=!!m&&m[1];s=!((-1<l.indexOf("iPhone")||-1<l.indexOf("iPad")||-1<l.indexOf("iPod"))&&d&&534>d||e.operamini&&"[object OperaMini]"==={}.toString.call(e.operamini)||u&&7458>c||-1<o.indexOf("Android")&&d&&533>d||h&&6>h||"palmGetResource"in e&&d&&534>d||-1<o.indexOf("MeeGo")&&-1<o.indexOf("NokiaBrowser/8.5.0")||m&&6>=m)}(jQuery,window),$.backstretch(["/lib/hood/images/bg/1.jpg","/lib/hood/images/bg/3.jpg","/lib/hood/images/bg/4.jpg","/lib/hood/images/bg/5.jpg","/lib/hood/images/bg/6.jpg","/lib/hood/images/bg/7.jpg","/lib/hood/images/bg/8.jpg","/lib/hood/images/bg/9.jpg","/lib/hood/images/bg/10.jpg","/lib/hood/images/bg/11.jpg","/lib/hood/images/bg/12.jpg","/lib/hood/images/bg/13.jpg"],{fade:1e3,duration:3e3});
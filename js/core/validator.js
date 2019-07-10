"use strict";

if (!$.hood) $.hood = {};

$.hood.FormValidator = function (element, options) {
  this.Options = $.extend({
    formTag: element,
    validationRules: null,
    validationMessages: {},
    placeBelow: true,
    submitButtonTag: null,
    submitUrl: null,
    submitFunction: null,
    serializationFunction: function serializationFunction() {
      rtn = $(this.formTag).serialize();
      return rtn;
    }
  }, options || {});

  this.LoadValidation = function () {
    if ($.hood.Helpers.IsNullOrUndefined(this.Options.formTag)) return;
    $(this.Options.formTag).find('input, textarea, select').keypress($.proxy(function (e) {
      if (e.which === 13) {
        $.proxy(this.submitForm(), this);
        e.preventDefault();
        return false;
      }
    }, this));
    $(this.Options.formTag).validate({
      submitHandler: function submitHandler(e) {
        e.preventDefault();
      },
      errorClass: this.Options.errorClass,
      focusInvalid: false,
      rules: this.Options.validationRules,
      messages: this.Options.validationMessages
    });
    if ($.hood.Helpers.IsNullOrUndefined(this.Options.submitButtonTag)) return;
    $(this.Options.submitButtonTag).click($.proxy(this.submitForm, this));
  };

  this.submitForm = function () {
    if ($(this.Options.formTag).valid()) {
      this.TempButtonContent = $(this.Options.submitButtonTag).removeClass('btn-primary').addClass('btn-default').html();
      $(this.Options.submitButtonTag).removeClass('btn-primary').addClass('btn-default').html('<i class="fa fa-refresh fa-spin"></i>&nbsp;Loading...');
      $(this.Options.formTag).find('input[type=checkbox]').each(function () {
        if ($(this).is(':checked')) {
          $(this).val('true');
        }
      });
      $.post(this.Options.submitUrl, this.Options.serializationFunction(), $.proxy(function (data) {
        $(this.Options.submitButtonTag).removeClass('btn-default').addClass('btn-primary').html(this.TempButtonContent);
        this.Options.submitFunction(data);
      }, this));
    }
  };

  this.LoadValidation();
  if (this.Options.placeBelow) $(this.Options.formTag).addClass("validation-below");
};

$.fn.hoodValidator = function (options) {
  return this.each(function () {
    var element = $(this); // Return early if this element already has a plugin instance

    if (element.data('hoodValidator')) return; // pass options to plugin constructor

    var hoodValidator = new $.hood.FormValidator(this, options); // Store plugin object in this element's data

    element.data('hoodValidator', hoodValidator);
  });
};
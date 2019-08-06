"use strict";

var swalWithBootstrapButtons = Swal.mixin({
  customClass: {
    confirmButton: 'btn btn-success btn-lg m-1 pl-4 pr-4',
    cancelButton: 'btn btn-danger btn-lg m-1'
  },
  buttonsStyling: false
});
var Toast = Swal.mixin({
  toast: true,
  position: 'top-end',
  showConfirmButton: true
});
if (!$.hood) $.hood = {};
$.hood.Alerts = {
  Error: function Error(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'error', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'error');
  },
  Warning: function Warning(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'warning', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'warning');
  },
  Message: function Message(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'info', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'info');
  },
  Success: function Success(msg, title, sweetAlert, footer, showConfirmButton, timer, callback) {
    if (sweetAlert === true) this.SweetAlert(msg, title, 'success', footer, showConfirmButton, timer, callback);else this.Alert(msg, title, 'success');
  },
  Alert: function Alert(msg, title, type) {
    Toast.fire({
      type: type || 'info',
      html: msg,
      title: title
    });
  },
  SweetAlert: function SweetAlert(msg, title, type, footer, showConfirmButton, timer, callback) {
    swalWithBootstrapButtons.fire({
      title: title,
      html: msg,
      type: type || 'info',
      footer: footer,
      showConfirmButton: showConfirmButton,
      timer: timer
    }).then(function (result) {
      if (!result.dismiss) callback(result);
    });
  },
  Confirm: function Confirm(msg, title, callback, type, footer, confirmButtonText, cancelButtonText) {
    swalWithBootstrapButtons.fire({
      title: title || 'Woah!',
      html: msg || 'Are you sure you want to do this?',
      type: type || 'warning',
      footer: footer || '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>',
      showCancelButton: true,
      confirmButtonText: confirmButtonText || 'Ok',
      cancelButtonText: cancelButtonText || 'Cancel'
    }).then(function (result) {
      if (!result.dismiss) callback(result.value);
    });
  },
  Prompt: function Prompt(msg, title, callback, inputType, type, footer, confirmButtonText, cancelButtonText, inputAttributes) {
    swalWithBootstrapButtons.fire({
      input: inputType || 'text',
      inputAttributes: inputAttributes || {
        autocapitalize: 'off'
      },
      title: title || 'Enter a value',
      html: msg || 'Fill in the field and press Ok to continue.',
      type: type || 'info',
      footer: footer,
      showCancelButton: true,
      confirmButtonText: confirmButtonText || 'Ok',
      cancelButtonText: cancelButtonText || 'Cancel'
    }).then(function (result) {
      if (!result.dismiss) callback(result.value);
    });
  }
};
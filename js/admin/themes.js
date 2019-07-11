"use strict";

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

if (!$.hood) $.hood = {};
$.hood.Themes = {
  Init: function Init() {
    $('body').on('click', '.activate-theme', $.hood.Themes.Activate);
  },
  Activate: function Activate(e) {
    var _swal;

    var $this = $(this);
    swal((_swal = {
      title: "Are you sure?",
      text: "The site will change themes, and the selected theme will be live right away.",
      type: "warning",
      showCancelButton: true,
      confirmButtonColor: "#DD6B55",
      confirmButtonText: "Yes, go ahead.",
      cancelButtonText: "No, cancel!"
    }, _defineProperty(_swal, "showCancelButton", true), _defineProperty(_swal, "closeOnConfirm", false), _defineProperty(_swal, "showLoaderOnConfirm", true), _defineProperty(_swal, "closeOnCancel", false), _swal), function (isConfirm) {
      if (isConfirm) {
        // delete functionality
        $.post('/admin/themes/activate', {
          name: $this.data('name')
        }, function (data) {
          if (data.Success) {
            window.location = '/admin/theme';
          } else {
            swal("Error", "There was a problem activating the theme:\n\n" + data.Errors, "error");
          }
        });
      } else {
        swal("Cancelled", "It's all good in the hood!", "error");
      }
    });
  }
};
$(document).ready($.hood.Themes.Init);
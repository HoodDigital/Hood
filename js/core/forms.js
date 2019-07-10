"use strict";

if (!$.hood) $.hood = {};
$.hood.Forms = {
  Init: function Init() {
    $('.floating-label > label').each(function () {
      var $me = $(this);
      $me.parent().append($me);
    });
  },
  GetAllowedExtensions: function GetAllowedExtensions(section) {
    switch (section) {
      case "Image":
        return ['png', 'jpg', 'jpeg', 'bmp', 'gif'];

      case "Document":
        return ['doc', 'docx', 'pdf', 'rtf'];

      case "All":
        return '';
    }
  },
  GetAllowedFiles: function GetAllowedFiles(section) {
    switch (section) {
      case "Image":
        return 'image/png,jpg,image/jpeg,image/gif';

      case "Document":
        return 'application/msword,application/pdf,text/rtf';

      case "All":
        return '';
    }
  }
};
$.hood.Forms.Init();
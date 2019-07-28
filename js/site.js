"use strict";

if (!$.hood) $.hood = {};
$.hood.Site = {
  Init: function Init() {// Add any init time functionality here - when the code is first loaded.
  },
  Ready: function Ready() {
    $.hood.Site.Resize(); // Add any ready time functionality here - when the document is ready.
  },
  Load: function Load() {// Add any load time functionality here - when the document is loaded.
  },
  Resize: function Resize() {// Add any resize functionality here - whenever the window is resized.
  }
}; // Initialise

$(function () {
  $.hood.Site.Ready();
});
$(window).on('load', $.hood.Site.Load);
$(window).on('resize', $.hood.Site.Resize);
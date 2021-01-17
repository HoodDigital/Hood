"use strict";

if (!$.hood) $.hood = {};
$.hood.Admin = {
  Init: function Init() {
    $('.navbar-nav .collapse').on("show.bs.collapse", function () {
      !function (t) {
        var e = t.closest(".navbar-nav, .navbar-nav .nav").querySelectorAll(".collapse");
        [].forEach.call(e, function (e) {
          e !== t && $(e).collapse("hide");
        });
      }(this);
    });
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
    if ($('[data-plugin="counter"]') && $.counterUp) $('[data-plugin="counter"]').counterUp({
      delay: 10,
      time: 800
    });
    $.hood.Admin.Editors.Init();
    $.hood.Admin.Stats.Init();
  },
  Editors: {
    Init: function Init() {
      // Load any tinymce editors.
      tinymce.init({
        selector: '.tinymce-full',
        height: 150,
        menubar: false,
        plugins: ['advlist autolink lists link image charmap print preview anchor media', 'searchreplace visualblocks code fullscreen', 'insertdatetime media contextmenu paste code'],
        toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
        link_class_list: $.hood.LinkClasses,
        image_class_list: $.hood.ImageClasses,
        setup: $.hood.Media.Actions.Load.Insert,
        image_dimensions: false
      });
      tinymce.init({
        selector: '.tinymce-simple',
        height: 150,
        plugins: ['advlist autolink lists link image charmap print preview anchor media', 'searchreplace visualblocks code fullscreen', 'insertdatetime media contextmenu paste code'],
        menubar: false,
        toolbar: 'bold italic | bullist numlist | undo redo | link',
        link_class_list: $.hood.LinkClasses,
        image_class_list: $.hood.ImageClasses,
        image_dimensions: false
      });
      tinymce.init({
        selector: '.tinymce-full-content',
        height: 500,
        menubar: false,
        plugins: ['advlist autolink lists link image charmap print preview anchor media', 'searchreplace visualblocks code fullscreen', 'insertdatetime media contextmenu paste code'],
        toolbar: 'styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link image media hoodimage | code',
        link_class_list: $.hood.LinkClasses,
        image_class_list: $.hood.ImageClasses,
        setup: $.hood.Media.Actions.Load.Insert,
        image_dimensions: false
      });
      tinymce.init({
        selector: '.tinymce-simple-content',
        height: 500,
        plugins: ['advlist autolink lists link image charmap print preview anchor media', 'searchreplace visualblocks code fullscreen', 'insertdatetime media contextmenu paste code'],
        menubar: false,
        toolbar: 'bold italic | bullist numlist | undo redo | link',
        link_class_list: $.hood.LinkClasses,
        image_class_list: $.hood.ImageClasses,
        image_dimensions: false
      });
    }
  },
  Load: function Load() {
    $.hood.Admin.Resize();
  },
  Ready: function Ready() {
    $.hood.Admin.CheckAndLoadTabs('#page-tabs');
    $.hood.Admin.Resize();
  },
  Resize: function Resize() {
    if ($(window).width() > 768) {
      $('.content-body').css({
        'min-height': $(window).height() - 216
      });
    }
  },
  Stats: {
    Init: function Init() {
      if ($('#admin-chart-area').exists()) {
        $.hood.Admin.Stats.LoadChart();
        $('body').on('change', '#history', function () {
          $.hood.Admin.Stats.LoadChart();
        });
      }
    },
    LoadChart: function LoadChart() {
      $.get('/admin/stats/', function (data) {
        $('#admin-chart-area').empty();
        ;
        $('#admin-chart-area').html('<canvas id="admin-stats-chart"></canvas>');
        $.hood.Admin.Stats.LoadStats(data);
        $.hood.Admin.Stats.DoCharts(data);
      });
    },
    SetupCharts: function SetupCharts() {
      Chart.defaults.global.responsive = true;
      Chart.defaults.global.maintainAspectRatio = false;
      Chart.defaults.global.defaultColor = "#6E84A3";
      Chart.defaults.global.defaultFontColor = "#6E84A3";
      Chart.defaults.global.defaultFontFamily = '';
      Chart.defaults.global.defaultFontSize = 16;
      Chart.defaults.global.layout.padding = 0;
      Chart.defaults.global.legend.display = true;
      Chart.defaults.global.legend.position = "bottom";
      Chart.defaults.global.legend.labels.usePointStyle = true;
      Chart.defaults.global.legend.labels.padding = 25;
      Chart.defaults.global.elements.point.radius = 5;
      Chart.defaults.global.elements.point.backgroundColor = "#000000";
      Chart.defaults.global.elements.line.tension = .4;
      Chart.defaults.global.elements.line.borderWidth = 3;
      Chart.defaults.global.elements.line.borderColor = '#aaaaaa';
      Chart.defaults.global.elements.line.backgroundColor = "transparent";
      Chart.defaults.global.elements.line.borderCapStyle = "round";
      Chart.defaults.global.elements.rectangle.backgroundColor = '#aaaaaa';
      Chart.defaults.global.elements.arc.backgroundColor = "#2C7BE5";
      Chart.defaults.global.elements.arc.borderColor = "#152E4D";
      Chart.defaults.global.elements.arc.borderWidth = 4;
      Chart.defaults.global.elements.arc.hoverBorderColor = "#152E4D";
      Chart.defaults.global.tooltips.enabled = true;
      Chart.defaults.global.tooltips.mode = "nearest";
      Chart.defaults.global.tooltips.intersect = false;
      Chart.scaleService.updateScaleDefaults("linear", {
        gridLines: {
          borderDash: [2],
          borderDashOffset: [2],
          color: "#dedede",
          drawBorder: !1,
          drawTicks: !1,
          zeroLineColor: "#dedede",
          zeroLineBorderDash: [2],
          zeroLineBorderDashOffset: [2]
        },
        ticks: {
          beginAtZero: !0,
          padding: 2,
          callback: function callback(e) {
            return e;
          }
        }
      });
      Chart.scaleService.updateScaleDefaults("category", {
        gridLines: {
          drawBorder: !1,
          drawOnChartArea: !1,
          drawTicks: !1
        },
        ticks: {
          padding: 5
        }
      });
    },
    DoCharts: function DoCharts(data) {
      var datasets = [];
      var dataLabels = [];
      var contentColour = '#fabd07';
      var propertyColour = '#20c997';
      var userColour = '#fd7e14';
      var subColour = '#17a2b8'; // 12 months

      var labels = []; // Content by day

      var contentSet = [];
      data.content.months.forEach(function (element) {
        contentSet.push(element.Value);
        labels.push(element.Key);
      });
      datasets.push({
        label: 'New Content',
        data: contentSet,
        borderColor: contentColour,
        backgroundColor: contentColour,
        pointBackgroundColor: contentColour,
        pointBorderColor: '#ffffff'
      }); // users by day

      contentSet = [];
      data.users.months.forEach(function (element) {
        contentSet.push(element.Value);
      });
      datasets.push({
        label: 'New Users',
        data: contentSet,
        borderColor: userColour,
        backgroundColor: userColour,
        pointBackgroundColor: userColour,
        pointBorderColor: '#ffffff'
      }); // Properties by day

      contentSet = [];
      data.properties.months.forEach(function (element) {
        contentSet.push(element.Value);
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
      var ctx = document.getElementById("admin-stats-chart");
      var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: dataLabels,
          datasets: datasets
        },
        responsive: true,
        maintainAspectRatio: false,
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            yAxes: [{
              ticks: {
                beginAtZero: true
              }
            }]
          }
        }
      });
    },
    LoadStats: function LoadStats(data) {
      if (data.content) {
        $('.content-totalPosts').text(data.content.totalPosts);
        $('.content-totalPublished').text(data.content.totalPublished);

        if (data.content.byType) {
          for (var i = 0; i < data.content.byType.length; i++) {
            $('.content-' + data.content.byType[i].typeName + '-total').text(data.content.byType[i].total);
          }
        }
      }

      if (data.users) {
        $('.users-totalUsers').text(data.users.totalUsers);
        $('.users-totalAdmins').text(data.users.totalAdmins);
      }

      if (data.properties) {
        $('.properties-totalPosts').text(data.properties.totalPosts);
        $('.properties-totalPublished').text(data.properties.totalPublished);
      }
    }
  },
  CheckAndLoadTabs: function CheckAndLoadTabs(tag) {
    $(tag).each(function () {
      // build the mobile jobber
      var button = $('<li class="nav-item d-inline d-lg-none"></li>');
      var dropdown = $('<div class="dropdown-menu"></ul>');
      var nav = $('<ul class="navbar-nav"></ul>');
      $(this).find('> li').each(function () {
        var link = $(this).children('a:first-child');
        var item = $('<li class="nav-item"><a href="' + link.attr('href') + '" data-toggle="tab" class="nav-link tab-switch">' + link.html() + '</a></li>');
        nav.append(item);
        $(this).addClass('d-none d-lg-inline');
      });
      button.append('<a class="nav-link dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-eye mr-2"></i><span>Choose View</span></button>');
      dropdown.append(nav);
      button.append(dropdown);
      $(this).prepend(button);
    });
    $(tag + ' a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
      localStorage.setItem('tab-' + $(tag).data('id'), $(e.target).attr('href'));
    });
    var activeTab = localStorage.getItem('tab-' + $(tag).data('id'));

    if (activeTab) {
      $(tag + ' a[href="' + activeTab + '"]').tab('show');
    } else {
      $(tag + ' a[data-toggle="tab"]').first().tab('show');
    }
  }
};
$(document).ready($.hood.Admin.Ready);
$(window).on('load', $.hood.Admin.Load);
$(window).on('resize', $.hood.Admin.Resize);
$.hood.Admin.Init();
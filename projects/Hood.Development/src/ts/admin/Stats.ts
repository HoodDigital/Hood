export class Stats {
    constructor() {
        if ($('#admin-chart-area').length) {
            this.LoadChart();
            $('body').on('change', '#history', function () {
                this.LoadChart();
            });
        }
    }

    LoadChart() {
        console.error(" is not implemented.");
    //    $.get('/admin/stats/', function (data) {
    //        $('#admin-chart-area').empty();;
    //        $('#admin-chart-area').html('<canvas id="admin-stats-chart"></canvas>');
    //        $.hood.Admin.Stats.LoadStats(data);
    //        $.hood.Admin.Stats.DoCharts(data);
    //    });
    }
    SetupCharts() {
        console.error("Hood.Stats.SetupCharts() is not implemented.");
    //    Chart.defaults.global.responsive = true;
    //    Chart.defaults.global.maintainAspectRatio = false;

    //    Chart.defaults.global.defaultColor = "#6E84A3";
    //    Chart.defaults.global.defaultFontColor = "#6E84A3";
    //    Chart.defaults.global.defaultFontFamily = '';
    //    Chart.defaults.global.defaultFontSize = 16;

    //    Chart.defaults.global.layout.padding = 0;

    //    Chart.defaults.global.legend.display = true;
    //    Chart.defaults.global.legend.position = "bottom";
    //    Chart.defaults.global.legend.labels.usePointStyle = true;
    //    Chart.defaults.global.legend.labels.padding = 25;

    //    Chart.defaults.global.elements.point.radius = 5;
    //    Chart.defaults.global.elements.point.backgroundColor = "#000000";

    //    Chart.defaults.global.elements.line.tension = .4;
    //    Chart.defaults.global.elements.line.borderWidth = 3;
    //    Chart.defaults.global.elements.line.borderColor = '#aaaaaa';
    //    Chart.defaults.global.elements.line.backgroundColor = "transparent";
    //    Chart.defaults.global.elements.line.borderCapStyle = "round";

    //    Chart.defaults.global.elements.rectangle.backgroundColor = '#aaaaaa';

    //    Chart.defaults.global.elements.arc.backgroundColor = "#2C7BE5";
    //    Chart.defaults.global.elements.arc.borderColor = "#152E4D";
    //    Chart.defaults.global.elements.arc.borderWidth = 4;
    //    Chart.defaults.global.elements.arc.hoverBorderColor = "#152E4D";

    //    Chart.defaults.global.tooltips.enabled = true;
    //    Chart.defaults.global.tooltips.mode = "nearest";
    //    Chart.defaults.global.tooltips.intersect = false;

    //    Chart.scaleService.updateScaleDefaults("linear", {
    //        gridLines: {

    //            borderDash: [2],
    //            borderDashOffset: [2],
    //            color: "#dedede",
    //            drawBorder: !1,
    //            drawTicks: !1,
    //            zeroLineColor: "#dedede",
    //            zeroLineBorderDash: [2],
    //            zeroLineBorderDashOffset: [2]
    //        },
    //        ticks: {
    //            beginAtZero: !0,
    //            padding: 2,
    //            callback: function (e) {
    //                return e;
    //            }
    //        }
    //    });
    //    Chart.scaleService.updateScaleDefaults("category", {
    //        gridLines: {
    //            drawBorder: !1,
    //            drawOnChartArea: !1,
    //            drawTicks: !1
    //        },
    //        ticks: {
    //            padding: 5
    //        }
    //    });
    }
    DoCharts(data: any) {
        console.error("Hood.Stats.DoCharts() is not implemented.");
    //    let datasets = [];
    //    let dataLabels = [];
    //    let contentColour = '#fabd07';
    //    let propertyColour = '#20c997';
    //    let userColour = '#fd7e14';
    //    let subColour = '#17a2b8';
    //    // 12 months
    //    let labels = [];

    //    // Content by day
    //    let contentSet = [];
    //    data.content.months.forEach(function (element) {
    //        contentSet.push(element.Value);
    //        labels.push(element.Key);
    //    });

    //    datasets.push({
    //        label: 'New Content',
    //        data: contentSet,
    //        borderColor: contentColour,
    //        backgroundColor: contentColour,
    //        pointBackgroundColor: contentColour,
    //        pointBorderColor: '#ffffff'
    //    });

    //    // users by day
    //    contentSet = [];
    //    data.users.months.forEach(function (element) {
    //        contentSet.push(element.Value);
    //    });

    //    datasets.push({
    //        label: 'New Users',
    //        data: contentSet,
    //        borderColor: userColour,
    //        backgroundColor: userColour,
    //        pointBackgroundColor: userColour,
    //        pointBorderColor: '#ffffff'
    //    });

    //    // Properties by day
    //    contentSet = [];
    //    data.properties.months.forEach(function (element) {
    //        contentSet.push(element.Value);
    //    });

    //    datasets.push({
    //        label: 'New Properties',
    //        data: contentSet,
    //        borderColor: propertyColour,
    //        backgroundColor: propertyColour,
    //        pointBackgroundColor: propertyColour,
    //        pointBorderColor: '#ffffff'
    //    });

    //    dataLabels = labels;
    //    var ctx = document.getElementById("admin-stats-chart");
    //    var myChart = new Chart(ctx, {
    //        type: 'bar',
    //        data: {
    //            labels: dataLabels,
    //            datasets: datasets
    //        },
    //        responsive: true,
    //        maintainAspectRatio: false,
    //        options: {
    //            responsive: true,
    //            maintainAspectRatio: false,
    //            scales: {
    //                yAxes: [{
    //                    ticks: {
    //                        beginAtZero: true
    //                    }
    //                }]
    //            }
    //        }
    //    });
    }
    LoadStats(data: any) {
        console.error("Hood.Stats.LoadStats() is not implemented.");
    //  if (data.content) {
    //        $('.content-totalPosts').text(data.content.totalPosts);
    //        $('.content-totalPublished').text(data.content.totalPublished);
    //        if (data.content.byType) {
    //            for (let i = 0; i < data.content.byType.length; i++) {
    //                $('.content-' + data.content.byType[i].typeName + '-total').text(data.content.byType[i].total);
    //            }
    //        }
    //    }
    //    if (data.users) {
    //        $('.users-totalUsers').text(data.users.totalUsers);
    //        $('.users-totalAdmins').text(data.users.totalAdmins);
    //    }
    //    if (data.properties) {
    //        $('.properties-totalPosts').text(data.properties.totalPosts);
    //        $('.properties-totalPublished').text(data.properties.totalPublished);
    //    }
    }
}
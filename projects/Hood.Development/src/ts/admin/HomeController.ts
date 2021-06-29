import { KeyValue } from "../interfaces/KeyValue";
import { ContentStatistics } from "../models/Content";
import { PropertyStatistics } from "../models/Property";
import { UserStatistics } from "../models/Users";
import { Chart } from 'chart.js';

export class HomeController {
    canvas: HTMLCanvasElement;
    ctx: CanvasRenderingContext2D;
    chart: any;

    constructor() {

        if ($('#admin-chart-area').length) {
            let $this = this;
            $this.LoadChart();
            $('body').on('change', '#history', function () {
                $this.LoadChart();
            });
        }

    }

    LoadChart(): void {
        let $this = this;
        $.get('/admin/stats/', function (data) {
            $('#admin-chart-area').empty();;
            $('#admin-chart-area').html('<canvas id="admin-stats-chart"></canvas>');
            $this.LoadStats(data);
            $this.DrawChart(data);
        });
    }

    DrawChart(data: Statistics): void {

        let datasets = [];
        let dataLabels = [];

        let contentColour = '#fabd07';
        let propertyColour = '#20c997';
        let userColour = '#fd7e14';

        // 12 months
        let labels: string[] = [];

        // Content by day
        let contentSet: number[] = [];

        data.content.months.forEach(function (element: KeyValue<string, number>) {
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
        data.users.months.forEach(function (element: KeyValue<string, number>) {
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
        data.properties.months.forEach(function (element: KeyValue<string, number>) {
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

        this.canvas = <HTMLCanvasElement>document.getElementById("admin-stats-chart");
        this.ctx = this.canvas.getContext('2d');
        this.chart = new Chart(this.ctx, {
            type: 'bar',
            data: {
                labels: dataLabels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    ticks: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    LoadStats(data: Statistics): void {
        if (data.content) {
            $('.content-totalPosts').text(data.content.totalPosts);
            $('.content-totalPublished').text(data.content.totalPublished);
            if (data.content.byType) {
                for (let i = 0; i < data.content.byType.length; i++) {
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
    }
}

export class Statistics {
    content: ContentStatistics;
    users: UserStatistics;
    properties: PropertyStatistics;
}

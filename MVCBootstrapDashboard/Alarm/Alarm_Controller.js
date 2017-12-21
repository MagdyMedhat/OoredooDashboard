App.controller('AlarmController', function ($scope, $sce, $filter, AlarmApi, TablesConfig, CommonMethods) {
    $scope.TableRecords = {};
    $scope.TableHeaders = {};

    $scope.CurrentPage = 1;
    $scope.dtStartDate = "";
    $scope.searchDate = "";
    $scope.dtEndDate = "";
    $scope.RowsPerPage = TablesConfig.Page_Size;
    $scope.Pages_Max_Size = TablesConfig.Pages_Max_Size;
    $scope.ReportName = "";

    $scope.chartLevel = true;
    $scope.datewiseLevel = false;
    $scope.detailsLevel = false;

    $scope.datewiseCharts = []; //used in chart click to check whether to drilldown or not to datewise level
    $scope.detailsCharts = []; //used in getdetails to check whether to drilldown or not to details level


    $scope.ViewReport = function () {
        $scope.dtStartDate = $("#bs-datepicker-range-start").val();
        $scope.dtEndDate = $("#bs-datepicker-range-end").val();
        
        if ($scope.chartLevel)
            $scope.GetSummary();
        else
            $scope.GetDetails();
    }

    $scope.ChartClick = function (ReportName) {
        if ($scope.datewiseCharts.includes(ReportName)) {
            $scope.chartLevel = false;
            $scope.detailsLevel = false;
            $scope.datewiseLevel = true;
            $scope.ReportName = ReportName;
            $scope.GetDetails();
        }
    }

    $scope.DayClick = function (date) {
        $scope.CurrentPage = 1;
        $scope.searchDate = date;
        $scope.chartLevel = false;
        $scope.detailsLevel = true;
        $scope.datewiseLevel = false;
        $scope.GetDetails();
    }

    $scope.backToChartLevel = function () {
        $scope.chartLevel = true;
        $scope.detailsLevel = false;
        $scope.datewiseLevel = false;
        $scope.ViewReport();
    }

    $scope.BackToDaywise = function () {
        $scope.CurrentPage = 1;
        $scope.ChartClick($scope.ReportName);
    }

    $scope.GetDetails = function () {

        $scope.hasMoreDetails = false;

        if ($scope.detailsLevel == false) {
            if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
        $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
                return;
            }
            if ($scope.ReportName == "")
                return;
            AlarmApi.GetDetails($scope.ReportName, $scope.dtStartDate, $scope.dtEndDate, $scope.CurrentPage, $scope.RowsPerPage)
        .success(function (data) {
            $scope.hasMoreDetails = $scope.detailsCharts.includes($scope.ReportName);
            $scope.TotalItemsCount = data[0];
            $scope.TableRecords = data[1];
            $scope.TableHeaders = data[2];
            $("#focus_here").focus();
        }).error(function (data) { console.log(data); });

        }
        else {
            AlarmApi.GetDetails("MORE", $scope.searchDate, $scope.searchDate, $scope.CurrentPage, $scope.RowsPerPage)
             .success(function (data) {
                 $scope.TotalItemsCount = data[0];
                 $scope.TableRecords = data[1];
                 $scope.TableHeaders = data[2];
                 $("#focus_here").focus();
             }).error(function (data) { console.log(data); });
        }
    }


    $scope.DrawChart = function (DivName, ChartText, Series, LabelText) {

        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
                type: 'column',
            },
            title: {
                text: ChartText
            },
            legend: {
                verticalAlign: 'top',
                y: 20
            },
            xAxis: {
                type: 'datetime',
                tickInterval: 24 * 3600 * 1000, // one day
                labels: {
                    formatter: function () {
                        return moment(this.value).format("YYYY-MM-DD");
                    }, rotation: -45
                },
                crosshair: true,
                title: {
                    enabled: true,
                    text: 'Time(day)'
                }
            },
            yAxis: {
                labels: {
                    format: '{value:,.0f}'
                },
                min: 0,
                title: {
                    enabled: true,
                    text: LabelText
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y:,.0f}</b></td></tr>',
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function (event) { $scope.ChartClick(DivName); }
                        }
                    }
                },
                column: {
                    pointPadding: 0.2,
                    borderWidth: 0
                }
            },
            series: Series
        });
    }

    $scope.DrawChartVariance = function (DivName, ChartText, Series, LabelText) {
        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
                zoomType: 'xy',
            },
            title: {
                text: ChartText
            },
            legend: {
                verticalAlign: 'top',
                y: 20
            },
            xAxis: {                
                type: 'datetime',
                tickInterval: 24 * 3600 * 1000, // one day
                labels: {
                    formatter: function () {
                        return moment(this.value).format("YYYY-MM-DD");
                    }, rotation: -45
                },
                crosshair: true,
                title: {
                    enabled: true,
                    text: 'Time(day)'
                }
            },
            yAxis: [{ // Primary yAxis
                title: {
                    enabled: true,
                    text: LabelText
                },
                labels: {
                    format: '{value:,.0f}',
                    style: {
                        color: Highcharts.getOptions().colors[0]
                    }
                }
            },
        { // secondary yAxis
            labels: {
                format: '{value} %',
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            min: 0,
            title: {
                text: '',
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            opposite: true
        }]
            ,
            tooltip: {
                shared: true,
                useHTML: true,
                formatter: function () {                   
                    var points = '';
                    points += '<span style="color:' + this.points[0].series.color + ';padding:0">' + this.points[0].series.name + ': </span><span>' +
                       $filter('number')(this.points[0].y) + '</span>';
                    points += '<br/><span style="color:' + this.points[1].series.color + ';padding:0">' + this.points[1].series.name + ': </span><span>' +
                       $filter('number')(this.points[1].y) + '%</span>';

                    points += '</table>'

                    return '<span style="font-size:10px">' + Highcharts.dateFormat('%A, %b %e, %Y', this.x, false) + '</span><br/>' + points;
                }
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function (event) { $scope.ChartClick(DivName); }
                        }
                    }
                },
                column: {
                    pointPadding: 0.2,
                    borderWidth: 0
                }
            },
            series: Series
        });
    }

    $scope.DrawPieChart = function (DivName, ChartText, Series, LabelText) {
        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                type: 'pie',
                events: {
                    click: function (event) { $scope.ChartClick(DivName); }
                }
            },
            labels: {
                items: [{
                    html: LabelText,
                    style: {
                        top: '10px',
                        left: '10px'
                    }
                }]
            },
            legend: {
                enables: true,
                align: 'center',
                verticalAlign: 'bottom',
                layout: 'horizontal',
                x: 0,
                y: 0
            },
            title: {
                text: ChartText
            },
            tooltip: {
                pointFormat: '<b>{point.y:,.0f}</b>'
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function (event) { $scope.ChartClick(DivName); }
                        }
                    }
                },
                pie: {
                    allowPointSelect: false,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true
                }

            },
            series: [{
                colorByPoint: true,
                data: Series
            }]
        });
    }

    $scope.drawScatterChart = function (DivName, ChartText, Categories, Series, LabelText) {
        if (DivName == 'CHART_15A')
            console.debug(Categories);
        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
            },
            title: {
                text: ChartText
            },
            legend: {
                verticalAlign: 'top',
                y: 20
            },
            xAxis: {
                //  categories: Categories,
                type: 'datetime',
                tickInterval: 24 * 3600 * 1000, // one day
                labels: {
                    formatter: function () {
                        return moment(this.value).format("YYYY-MM-DD");
                    }, rotation: -45
                },
                crosshair: true,
                title: {
                    enabled: true,
                    text: 'Time(day)'
                }
            },


            yAxis: {
                labels: {
                    //formatter: function () {
                    //    return this.value;
                    //},
                    format: '{value:,.0f}'
                },
                min: 0,
                title: {
                    enabled: true,
                    text: LabelText
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y}</b></td></tr>',
                footerFormat: '</table>',
                //formatter: function () {
                //    return '<span style="font-size:10px">' + this.x + '</span><br/><span style="color:' + this.series.color + '">'+ this.series.name +': </span>' + $filter('number')(this.y);
                //    //var tooltip = '<span style="font-size:10px">' + this.x + '</span>';
                //    //$.each(this.points, function(index, point){
                //    //    tooltip += '<br/><span style="color:' + point.series.color + ';padding:0">' + point.series.name + ': </span><span>' +
                //    //       $filter('number')(point.y) + '</span>';
                    
                //    //});
                //    //return tooltip;
                //},
                shared: true,
                useHTML: true
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function (event) { $scope.ChartClick(DivName); }
                        }
                    }
                }
            },
            series: Series
        });
    }

    $scope.drawScatterChart_Weekly = function (DivName, ChartText, Categories, Series, LabelText) {
        if (DivName == 'CHART_15B')
        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
            },
            title: {
                text: ChartText
            },
            legend: {
                verticalAlign: 'top',
                y: 20
            },
            xAxis: {
                categories: Categories,
                labels: { rotation: -45 },
                crosshair: true,
                title: {
                    enabled: true,
                    text: 'Time(Week)'
                }
            },


            yAxis: {
                type: 'datetime',
                tickInterval: 24 * 3600 * 1000, // one day
                labels: {
                    formatter: function () {
                        return moment(this.value).format("YYYY-MM-DD");
                    }
                    //, rotation: -45
                },
                //   min: 0,
                title: {
                    enabled: true,
                    text: 'Time(Day)'
                }
            },
            tooltip: {
                //headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                //pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                //    '<td style="padding:0"><b>{point.y}</b></td></tr>',
                //footerFormat: '</table>',
                shared: true,
                formatter: function () {
                    return '<span style="font-size:10px">' + this.x + '</span><br/><span style="color:' + this.series.color + '">' + this.series.name + ': </span>' + moment(this.y).format("YYYY-MM-DD");
                    //var tooltip = '<span style="font-size:10px">' + this.x + '</span>';
                    //$.each(this.points, function(index, point){
                    //    tooltip += '<br/><span style="color:' + point.series.color + ';padding:0">' + point.series.name + ': </span><span>' +
                    //       $filter('number')(point.y) + '</span>';
                    
                    //});
                    //    return tooltip;
                    }
                    
                
                
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function (event) { $scope.ChartClick(DivName); }
                        }
                    }
                }
            },
            series: Series
        });
    }

    $scope.GetSummary = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }

        AlarmApi.GetSummary("COUNT_CLOSED_ALARMS_FRD_NFR", $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("COUNT_CLOSED_ALARMS_FRD_NFR", "Count of Closed Alarms - Fraud vs. Non Fraud", retData[0], "Count(Number)");
                $scope.datewiseCharts.push("COUNT_CLOSED_ALARMS_FRD_NFR");
                $scope.detailsCharts.push("COUNT_CLOSED_ALARMS_FRD_NFR");
            }).error(function () {
            });
        AlarmApi.GetSummary("AMOUNT_CLOSED_ALARMS_FRD_NFR", $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("AMOUNT_CLOSED_ALARMS_FRD_NFR", "Amount of Closed Alarms - Fraud vs. Non Fraud", retData[0], "Amount(QAR)");
                $scope.datewiseCharts.push("AMOUNT_CLOSED_ALARMS_FRD_NFR");
                $scope.detailsCharts.push("AMOUNT_CLOSED_ALARMS_FRD_NFR");
            }).error(function () {
            });
        AlarmApi.GetSummary("COUNT_FRD_NFR_PIE", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawPieChart("COUNT_FRD_NFR_PIE", retData[0].name, retData[0].data, "Count(Number)");
               $scope.datewiseCharts.push("COUNT_FRD_NFR_PIE");
           }).error(function () {
           });
        AlarmApi.GetSummary("AMOUNT_FRD_NFR_PIE", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawPieChart("AMOUNT_FRD_NFR_PIE", retData[0].name, retData[0].data, "Amount(QAR)");
               $scope.datewiseCharts.push("AMOUNT_FRD_NFR_PIE");
           }).error(function () {
           });
        AlarmApi.GetSummary("COUNT_9_PIE", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawPieChart("COUNT_9_PIE", retData[0].name, retData[0].data, "Count(Number)");
           }).error(function () {
           });
        AlarmApi.GetSummary("COUNT_CLOSED_ALARMS_FRD_IMEI", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawChartVariance("COUNT_CLOSED_ALARMS_FRD_IMEI", "Count of Closed Alarms - Fraud - IMEI Invoice Rules", retData[0], "Count(Number)");
               $scope.datewiseCharts.push("COUNT_CLOSED_ALARMS_FRD_IMEI");
               $scope.detailsCharts.push("COUNT_CLOSED_ALARMS_FRD_IMEI");
           }).error(function () {
           });
        AlarmApi.GetSummary("COUNT_CLOSED_ALARMS_FRD_IRSF", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawChartVariance("COUNT_CLOSED_ALARMS_FRD_IRSF", "Count of Closed Alarms - Fraud - ISRF Rules", retData[0], "Count(Number)");
           }).error(function () {
           });
        AlarmApi.GetSummary("COUNT_CLOSED_ALARMS_FRD_INTERNATIONAL", $scope.dtStartDate, $scope.dtEndDate)
          .success(function (retData) {
              $scope.DrawChartVariance("COUNT_CLOSED_ALARMS_FRD_INTERNATIONAL", "Count of Closed Alarms - Fraud - International Rules", retData[0], "Count(Number)");
          }).error(function () {
          });
        AlarmApi.GetSummary("COUNT_12_PIE", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawPieChart("COUNT_12_PIE", retData[0].name, retData[0].data, "Count(Number)");
           }).error(function () {
           });
        AlarmApi.GetSummary("COUNT_13_PIE", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawPieChart("COUNT_13_PIE", retData[0].name, retData[0].data, "Count(Number)");
           }).error(function () {
           });
        AlarmApi.GetSummary("COUNT_13B_PIE", $scope.dtStartDate, $scope.dtEndDate)
           .success(function (retData) {
               $scope.DrawPieChart("COUNT_13B_PIE", retData[0].name, retData[0].data, "Count(Number)");
           }).error(function () {
           });
        AlarmApi.GetSummary("CHART_15A", $scope.dtStartDate, $scope.dtEndDate)
             .success(function (retData) {
                 $scope.drawScatterChart("CHART_15A", "Daily Trend of Hot Hour of Day", retData[1], retData[0], "Time(Hour)");
                 $scope.datewiseCharts.push("CHART_15A");
             }).error(function () {
             });
        AlarmApi.GetSummary("CHART_15B", $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.drawScatterChart_Weekly("CHART_15B", "Weekly Trend of Hot Day of Week", retData[1], retData[0], "Time(Week)");
                //$scope.datewiseCharts.push("CHART_15B");
            }).error(function () {
            });
    }

    $scope.PageChanged = function () {
        $scope.GetDetails();
    }

    $scope.ExportPage = function () {
        CommonMethods.ExportToExcel("myTable01", "Overview");
    }

    $scope.ExportAll = function () {
        AlarmApi.GetDetails($scope.ReportName, $scope.dtStartDate, $scope.dtEndDate, 1, $scope.TotalItemsCount)
            .success(function (CDRS) {
                CDRS[1].splice(0, 0, CDRS[2]);
                alasql('SELECT * INTO XLSX("Overview_report.xlsx",{headers:false}) FROM ?', [CDRS[1]]);
            }).error(function () { });
    }

    $scope.filterItems = function (seriesData) {
        for (var i = 0; i < seriesData.length; i++) {
            seriesData.y = $filter('number')(seriesData.y, 0);
        }
        return seriesData;
    }
});

App.factory("AlarmApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "../api/";
    var AIR_REFUNDControllerUrl = ApiPath + "Alarm/";

    function ExportALL(dtStartDate,
    dtEndDate) {
        var params = {
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate
        }
        return api.Get(AIR_REFUNDControllerUrl + "ExportALL", params);
    }


    function GetSummary(ReportName,
        dtStartDate,
        dtEndDate) {
        var params = {
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate,
        }

        return api.Get(AIR_REFUNDControllerUrl + ReportName, params);
    }

    function GetDetails(ReportName,
        dtStartDate,
        dtEndDate,
        PageNumber, RowsPerPage) {

        var params = {
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate,
            PageNumber: PageNumber,
            RowsPerPage: RowsPerPage
        }

        return api.Get(AIR_REFUNDControllerUrl + ReportName + "_DETAILS", params);
    }

    return {
        GetSummary: GetSummary,
        GetDetails: GetDetails,
        ExportALL: ExportALL
    };
}]);
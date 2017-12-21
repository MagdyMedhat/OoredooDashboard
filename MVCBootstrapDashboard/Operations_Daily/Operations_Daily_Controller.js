//'use strict';
App.controller('OperationsDailyController', function ($scope, $sce, $filter, OperationsDailyApi, TablesConfig, CommonMethods) {
    $scope.TableRecords = {};
    $scope.TableHeaders = {};
    $scope.CurrentPage = 1;
    $scope.dtStartDate = "";
    $scope.dtEndDate = "";
    $scope.ORIGINHOSTNAMEs = {};
    $scope.ORIGINHOSTNAMEsSelected = {};
    $scope.RowsPerPage = TablesConfig.Page_Size;
    $scope.Pages_Max_Size = TablesConfig.Pages_Max_Size;
    $scope.flag = true;
    $scope.ReportName = "";

    $scope.TotalItemsCount = 0;
    


    //thousands separator
    $scope.filterItem = function (value, index) {
        if (index > 2 && index < 5)
            return $filter('number')(value, 0);
        else
            return value;
    }

    $scope.GetORIGINHOSTNAMEs = function () {
        OperationsDailyApi.GetORIGINHOSTNAMEs().success(function (retData) {
            retData.unshift('');
            $scope.ORIGINHOSTNAMEs = retData;
            $("#ORIGINHOSTNAME").select2({ placeholder: "Report" });
        }).error(function () {
            $("#ORIGINHOSTNAME").select2({ placeholder: "Error While Loading !!" });
        });
    }

    $scope.ChartClick = function (ReportName) {
        $scope.flag = false;
        $scope.ReportName = ReportName;
        $scope.GetDetails();
        $("#ORIGINHOSTNAME").focus();
        $scope.$apply();
    }

    $scope.GetDetails = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }

        if ($scope.ReportName == "")
            return;

        OperationsDailyApi.GetDetails($scope.ReportName, $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, $scope.CurrentPage, $scope.RowsPerPage)
            .success(function (CDRS) {
                $scope.TotalItemsCount = CDRS[0];
                $scope.TableRecords = CDRS[1];
                $scope.TableHeaders = CDRS[2];
                window.scrollTo(0, 0);
            }).error(function () { });
    }

    $scope.back = function () {
        $scope.flag = true;
    }

    $scope.DrawChart = function (DivName, ChartText, Series) {

        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
                type: (DivName == 'INMAD_VS_INMAC_VS_COST' || DivName == 'INDAD_VS_INDAC_VS_COST' || DivName == 'INMAD_VS_INDAD') ? 'line' : 'column',
                //events: {
                //    click: function (event) { $scope.ChartClick(DivName); }
                //}
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
                    },
                    rotation: -45
                },
                crosshair: true,
                title: {
                    text: "Time(Day)"
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
                    text: 'CDRs(Number)'
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y}</b></td></tr>',
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

    $scope.DrawChartVariance = function (DivName, ChartText, Series) {
        var x = "#" + DivName;
        Highcharts.setOptions({
            colors: ['#7cb5ec', '#434348', 'red']
        });
        $(x).highcharts({
            chart: {
                zoomType: 'xy',
                //events: {
                //    click: function (event) { $scope.ChartClick(DivName); }
                //}
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
                    },
                    rotation: -45
                },
                crosshair: true,
                title: {
                    text: 'Time(Day)'
                }
            },
            yAxis: [{ // Primary yAxis
                title: {
                    text: 'CDRs(Number)',
                    style: {
                        color: Highcharts.getOptions().colors[0]
                    }
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
                    color: Highcharts.getOptions().colors[2]
                }
            },
            title: {
                text: '',
                style: {
                    color: Highcharts.getOptions().colors[2]
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
                       $filter('number')(this.points[1].y) + '</span>';
                    points += '<br/><span style="color:' + this.points[2].series.color + ';padding:0">' + this.points[2].series.name + ': </span><span>' +
                       $filter('number', '2')(this.points[2].y) + '%</span>';

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

    $scope.GetSummary = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null ||
            $scope.ORIGINHOSTNAMEsSelected == '' || $scope.ORIGINHOSTNAMEsSelected == undefined || $scope.ORIGINHOSTNAMEsSelected == null) {
            return;
        }

        OperationsDailyApi.GetSummary("MSC_COUNT_DURATION", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("MSC_COUNT_DURATION", "MSC Count & Duration Trends",retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("IN_TOTALCOUNT_DURATION", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("IN_TOTALCOUNT_DURATION", "IN Total count & duration Trends", retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("MSC_VS_IN_CWITHV", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                console.debug(retData);
                $scope.DrawChartVariance("MSC_VS_IN_CWITHV", "MSC Count vs. IN count with variance", retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("MSC_VS_IN_DWITHV", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                console.debug(retData);
                $scope.DrawChartVariance("MSC_VS_IN_DWITHV", "MSC duration vs. IN duration with variance", retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("INMAD_VS_INMAC_VS_COST", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("INMAD_VS_INMAC_VS_COST", "IN MA Duration vs. IN MA Count vs. Cost", retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("INDAD_VS_INDAC_VS_COST", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("INDAD_VS_INDAC_VS_COST", "IN DA Duration vs. IN DA Count vs. Cost", retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("INMAD_VS_INDAD", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("INMAD_VS_INDAD", "IN MA Duration vs. IN DA duration Trends", retData[0]);
            }).error(function () {
            });

        OperationsDailyApi.GetSummary("FILTER_OUT", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChart("FILTER_OUT", "Filter Out Trends", retData[0]);
            }).error(function () {
            });
    }

    $scope.ViewReport = function () {
        $scope.dtStartDate = $("#bs-datepicker-range-start").val();
        $scope.dtEndDate = $("#bs-datepicker-range-end").val();
        $scope.ORIGINHOSTNAMEsSelected = $("#ORIGINHOSTNAME").select2("val");

        $scope.GetSummary();

        $scope.GetDetails();
    }

    $scope.PageChanged = function () {
        $scope.GetDetails();
    }

    $scope.ExportPage = function () {
        CommonMethods.ExportToExcel("myTable01", "Operations_Daily");
    }

    $scope.ExportAll = function () {
        OperationsDailyApi.GetDetails($scope.ReportName, $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, 1, $scope.TotalItemsCount)
            .success(function (CDRS) {
                CDRS[1].splice(0, 0, CDRS[2]);
                alasql('SELECT * INTO XLSX("Daily_report.xlsx",{headers:false}) FROM ?', [CDRS[1]]);
            }).error(function () { });
    }

    $scope.GetORIGINHOSTNAMEs();

});



App.factory("OperationsDailyApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "../api/";
    var AIR_REFUNDControllerUrl = ApiPath + "Operations_Daily/";


    function GetORIGINHOSTNAMEs() {
        return api.Get(AIR_REFUNDControllerUrl + "GetORIGINHOSTNAMEs");
    }


    function ExportALL(ORIGINHOSTNAMEs,
    dtStartDate,
    dtEndDate) {
        var params = {
            ORIGINHOSTNAMEs: ORIGINHOSTNAMEs,
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate
        }
        return api.Get(AIR_REFUNDControllerUrl + "ExportALL", params);
    }


    function GetSummary(ReportName, ORIGINHOSTNAMEs,
        dtStartDate,
        dtEndDate) {
        var params = {
            ORIGINHOSTNAMEs: ORIGINHOSTNAMEs,
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate,
        }

        return api.Get(AIR_REFUNDControllerUrl + ReportName, params);
    }

    function GetDetails(ReportName, ORIGINHOSTNAMEs,
        dtStartDate,
        dtEndDate,
        PageNumber, RowsPerPage) {
        var params = {
            ORIGINHOSTNAMEs: ORIGINHOSTNAMEs,
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
        GetORIGINHOSTNAMEs: GetORIGINHOSTNAMEs,
        ExportALL: ExportALL
    };
}]);


//'use strict';
App.controller('AIR_REFUNDController', function ($scope, $sce, $filter, AIR_REFUNDApi, TablesConfig, CommonMethods) {
    $scope.AIR_Refund_CDRS = {};
    $scope.CurrentPage = 1;
    $scope.dtStartDate = "";
    $scope.dtEndDate = "";
    $scope.ORIGINHOSTNAMEs = {};
    $scope.ORIGINHOSTNAMEsSelected = {};
    $scope.RowsPerPage = TablesConfig.Page_Size;
    $scope.Pages_Max_Size = TablesConfig.Pages_Max_Size;
    $scope.flag = true;
    $scope.chartType = "";

    $scope.TotalItemsCount = 0;

    $scope.GetORIGINHOSTNAMEs = function () {
        AIR_REFUNDApi.GetORIGINHOSTNAMEs().success(function (retData) {

            retData.unshift('');
            $scope.ORIGINHOSTNAMEs = retData;
            $("#ORIGINHOSTNAME").select2({ placeholder: "Report" });
        }).error(function () {
            $("#ORIGINHOSTNAME").select2({ placeholder: "Error While Loading !!" });
        });
    }

    $scope.SelectAllORIGINHOSTNAMEs = function () {
        var selectstring = '';
        var stringVal = [];
        $('#ORIGINHOSTNAME').find('option').each(function () {
            //if (stringVal.length <= 30)
            stringVal.push($(this).val());
        });
        $('#ORIGINHOSTNAME').val(stringVal).trigger("change");
    }

    $scope.UnSelectAllORIGINHOSTNAMEs = function () {
        $('#ORIGINHOSTNAME').val([]).trigger("change");
    }

    $scope.ViewReport = function () {
        $scope.dtStartDate = $("#bs-datepicker-range-start").val();
        $scope.dtEndDate = $("#bs-datepicker-range-end").val();
        $scope.ORIGINHOSTNAMEsSelected = $("#ORIGINHOSTNAME").select2("val");

        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
            $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null ||
            $scope.ORIGINHOSTNAMEsSelected == '' || $scope.ORIGINHOSTNAMEsSelected == undefined || $scope.ORIGINHOSTNAMEsSelected == null) {
            return;
        }

        AIR_REFUNDApi.Get_TAPIN($scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                console.debug(retData);
                $('#TAPINChart').highcharts({
                    chart: {
                        type: 'column'
                        //events: {
                        //    click: function (event) { $scope.ChartClick("TAPIN"); }
                        //}
                    },
                    title: {
                        text: 'TAPIN Trends'
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
                            text: "Time(Days)"
                        }
                    },
                    yAxis: {
                        //stackLabels: {
                        //    rotation: '-25',
                        //    verticalAlign: 'top',
                        //    enabled: true,
                        //    style: {
                        //        fontWeight: 'bold',
                        //        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        //    }
                        //},
                        labels: {
                            //formatter: function () {
                            //    return this.value;
                            //}
                            format: '{value:,.0f}',

                        },
                        min: 0,
                        title: {
                            text: 'CDRs(Number)'
                        }
                    },
                    tooltip: {
                        shared: true,
                        formatter: function () {
                            var points = '<table class="tip"><caption>' + Highcharts.dateFormat('%A, %b %e, %Y', this.x, false) + '</caption><tbody>';
                            //loop each point in this.points
                            for (var i = 0; i < this.points.length; i++) {
                                points += '<br/><span style="color: ' + this.points[i].series.color + '">' + this.points[i].series.name + ': </span>'
                                      + '<span style="text-align: right">' + $filter('number')(this.points[i].y) + '</span>'
                            }
                            points += '<br/><tr>'
                            + '<td style="text-align:right"><b></b></td></tr>'
                            + '</tbody></table>';
                            return points;
                        }
                    },
                    plotOptions: {
                        series: {
                            cursor: 'pointer',
                            point: {
                                events: {
                                    click: function (event) { $scope.ChartClick("TAPIN"); }
                                }
                            }
                        },
                        column: {
                            pointPadding: '0.3',
                            stacking: 'normal',
                            //dataLabels: {
                            //    enabled: true,
                            //    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                            //    style: {
                            //        textShadow: '0 0 3px black'
                            //    }
                            //}
                        }
                    },
                    series: retData[0]
                });

            }).error(function () {
            });


        AIR_REFUNDApi.Get_BILLING($scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
         .success(function (retData) {

             $('#BILLINGChart').highcharts({
                 chart: {
                     type: 'column'
                     //events: {
                     //    click: function (event) { $scope.ChartClick("BILLING"); }
                     //}
                 },
                 title: {
                     text: 'BILLING Trends',
                     useHTML: true
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
                         text: "Time(Days)"
                     }
                 },
                 yAxis: {
                     //stackLabels: {
                     //    rotation: '-25',
                     //    verticalAlign: 'top',
                     //    enabled: true,
                     //    style: {
                     //        fontWeight: 'bold',
                     //        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                     //    }
                     //},
                     labels: {
                         //formatter: function () {
                         //    return this.value;
                         //}
                         format: '{value:,.0f}'
                     },
                     min: 0,
                     title: {
                         text: "CDRs(Number)"
                     }
                 },
                 //tooltip: {
                 //    headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                 //    pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                 //        '<td style="padding:0"><b>{point.y}</b></td></tr>',
                 //    footerFormat: '</table>',
                 //    shared: true,
                 //    useHTML: true
                 //},
                 tooltip: {
                     shared: true,
                     formatter: function () {
                         var points = '<table class="tip"><caption>' + Highcharts.dateFormat('%A, %b %e, %Y', this.x, false) + '</caption></br><tbody>';
                         //loop each point in this.points
                         for (var i = 0; i < this.points.length; i++) {
                             points += '<br/><span style="color: ' + this.points[i].series.color + '">' + this.points[i].series.name + ': </span>'
                                   + '<span style="text-align: right">' + $filter('number')(this.points[i].y) + '</span>'
                         }
                         /*total
                        <th>Total: </th>
                         ' + $filter('number')(this.points[0].total) + '*/
                         points += '<br/><tr>'
                         + '<td style="text-align:right"><b></b></td></tr>'
                         + '</tbody></table>';
                         return points;
                     }
                 },
                 plotOptions: {
                     series: {
                         cursor: 'pointer',
                         point: {
                             events: {
                                 click: function (event) { $scope.ChartClick("BILLING"); }
                             }
                         }
                     },
                     column: {
                         pointPadding: '0.3',
                         stacking: 'normal'
                     }
                 },
                 series: retData[0]
             });

         }).error(function () {
         });

        $scope.GetDetails();
    }

    $scope.ChartClick = function (chartType) {
        $scope.flag = false;
        $scope.chartType = chartType;
        $scope.$apply();
        $scope.GetDetails();
    }

    $scope.back = function () {
        $scope.flag = true;
    }

    $scope.GetDetails = function () {
        if ($scope.chartType == "TAPIN") {
            $scope.Get_TAPIN_Details();
        }

        if ($scope.chartType == "BILLING") {
            $scope.Get_BILLING_Details();
        }
    }

    $scope.Get_TAPIN_Details = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
            $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }
        AIR_REFUNDApi.Get_TAPIN_Details($scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, $scope.CurrentPage, $scope.RowsPerPage)
            .success(function (CDRS) {
                $scope.TotalItemsCount = CDRS[0];
                $scope.AIR_Refund_CDRS = CDRS[1];
            }).error(function () { });
    }

    $scope.Get_BILLING_Details = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
            $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }
        AIR_REFUNDApi.Get_BILLING_Details($scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, $scope.CurrentPage, $scope.RowsPerPage)
            .success(function (CDRS) {
                $scope.TotalItemsCount = CDRS[0];
                $scope.AIR_Refund_CDRS = CDRS[1];
            }).error(function () { });
    }

    $scope.PageChanged = function () {
        $scope.GetDetails();
    }

    $scope.ExportPage = function () {
        CommonMethods.ExportToExcel("myTable01", "Operations_TAPIN_BILLING");
    }

    $scope.ExportAll = function () {

        if ($scope.chartType == "TAPIN") {
            AIR_REFUNDApi.Get_TAPIN_Details($scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, 1, $scope.TotalItemsCount)
    .success(function (CDRS) {
        alasql('SELECT * INTO XLSX("History_TAPIN_report.xlsx",{headers:false}) FROM ?', [CDRS[1]]);
    }).error(function () { });
        }

        if ($scope.chartType == "BILLING") {
            AIR_REFUNDApi.Get_BILLING_Details($scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, 1, $scope.TotalItemsCount)
       .success(function (CDRS) {
           CDRS[1].splice(0, 0, CDRS[2]);
           alasql('SELECT * INTO XLSX("History_BILLING_report.xlsx",{headers:false}) FROM ?', [CDRS[1]]);
       }).error(function () { });
        }

    }

    $scope.GetORIGINHOSTNAMEs();

});

App.factory("AIR_REFUNDApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "../api/";
    var AIR_REFUNDControllerUrl = ApiPath + "Operations_History/";


    function Get_TAPIN_Details(ORIGINHOSTNAMEs,
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
        return api.Get(AIR_REFUNDControllerUrl + "Get_TAPIN_Details", params);
    }

    function Get_BILLING_Details(ORIGINHOSTNAMEs,
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
        return api.Get(AIR_REFUNDControllerUrl + "Get_BILLING_Details", params);
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

    function Get_TAPIN(ORIGINHOSTNAMEs,
        dtStartDate,
        dtEndDate) {
        var params = {
            ORIGINHOSTNAMEs: ORIGINHOSTNAMEs,
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate,
        }
        return api.Get(AIR_REFUNDControllerUrl + "Get_TAPIN", params);
    }

    function Get_BILLING(ORIGINHOSTNAMEs,
    dtStartDate,
    dtEndDate) {
        var params = {
            ORIGINHOSTNAMEs: ORIGINHOSTNAMEs,
            dtStartDate: dtStartDate,
            dtEndDate: dtEndDate,
        }
        return api.Get(AIR_REFUNDControllerUrl + "Get_BILLING", params);
    }

    function GetORIGINHOSTNAMEs() {
        return api.Get(AIR_REFUNDControllerUrl + "GetORIGINHOSTNAMEs");
    }

    return {
        Get_TAPIN_Details: Get_TAPIN_Details,
        Get_BILLING_Details: Get_BILLING_Details,
        Get_TAPIN: Get_TAPIN,
        Get_BILLING: Get_BILLING,
        GetORIGINHOSTNAMEs: GetORIGINHOSTNAMEs,
        ExportALL: ExportALL
    };
}]);

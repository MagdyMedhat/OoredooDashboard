//'use strict';
App.controller('AnalystController', function ($scope, $sce, $filter, AnalystApi, TablesConfig, CommonMethods) {
    $scope.TableRecords = {};
    $scope.TableHeaders = {};
    $scope.CurrentPage = 1;
    $scope.dtStartDate = "";
    $scope.dtEndDate = "";
    $scope.Analysts = [];
    $scope.Days = [];
    $scope.Hours = [];
    $scope.analystSelected = {};
    $scope.daySelected = {};
    $scope.hourSelected = {};
    $scope.ORIGINHOSTNAMEs = {};
    $scope.ORIGINHOSTNAMEsSelected = {};
    $scope.RowsPerPage = TablesConfig.Page_Size;
    $scope.Pages_Max_Size = TablesConfig.Pages_Max_Size;
    $scope.flag = true;
    $scope.ReportName = "";
    $scope.detailsCharts = [];
    $scope.TotalItemsCount = 0;

    //thousands separator
    //$scope.filterItem = function (value, index) {
    //    if (index > 2 && index < 5)
    //        return $filter('number')(value, 0);
    //    else
    //        return value;
    //}

    $scope.GetORIGINHOSTNAMEs = function () {
        AnalystApi.GetORIGINHOSTNAMEs().success(function (retData) {
            retData.unshift('');
            $scope.ORIGINHOSTNAMEs = retData;
            $("#ORIGINHOSTNAME").select2({ placeholder: "Analyst" });
        }).error(function () {
            $("#ORIGINHOSTNAME").select2({ placeholder: "Error While Loading !!" });
        });
    }

    $scope.GetAnalysts = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }
        AnalystApi.GetComboBoxData("GetAnalysts", $scope.dtStartDate, $scope.dtEndDate).success(function (retData) {
            $scope.Analysts = retData;
            $('#Analysts').select2({ placeholder: "Analyst" });
        }).error(function () {
            $('#Analysts').select2({ placeholder: "Error while loading !!" });
        });
    }

    $scope.GetDays = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }
        AnalystApi.GetComboBoxData("GetDays", $scope.dtStartDate, $scope.dtEndDate).success(function (retData) {
            $scope.Days = retData;
            $('#Days').select2({ placeholder: "Day" });
        }).error(function () {
            $('#Days').select2({ placeholder: "Error while loading !!" });
        });
    }

    $scope.GetHours = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }
        AnalystApi.GetComboBoxData("GetHours", $scope.dtStartDate, $scope.dtEndDate).success(function (retData) {
            $scope.Hours = retData;
            $('#Hours').select2({ placeholder: "Hour" });
        }).error(function () {
            $('#Hours').select2({ placeholder: "Error while loading !!" });
        });
    }

    $scope.InitializeAxes = function (series) {
        var yAxes = [];
        //for (var i = 0; i < series.length; i++) {
            yAxes.push(
                {
                    labels: {
                        formatter: function () {
                            return labels[this.value];
                        }
                    },
                    title: {
                        text: series.name
                    }
                });
        //}
        return yAxes;
    }

    $scope.ViewPanelChart_Analysts = function () {
        $scope.analystSelected = $('#Analysts').select2("val");

        AnalystApi.GetSummary("CHART_14_A", $scope.analystSelected, $scope.dtStartDate, $scope.dtEndDate, "Count(number)")
            .success(function (retData) {
                $scope.DrawChart("CHART_14_A", "Daily Trend of Hot Hour for a Selected Fraud Analyst", retData[1], retData[0], "Time(Hour)", "Time(Day)");
            }).error(function (error) {
                console.log(error);
            });
    }

    $scope.ViewPanelChart_Days = function () {
        $scope.daySelected = $('#Days').select2("val");

        AnalystApi.GetSummary("CHART_14_B", $scope.daySelected, $scope.dtStartDate, $scope.dtEndDate, "Count(number)")
            .success(function (retData) {
                
                $scope.DrawChart_Days("CHART_14_B", "Top Fraud Analyst Every Hour of a Selected Day", retData[1], retData[0], "Analysts(Names)", "Time(Hour)", retData[1]);
            }).error(function (error) {
                console.log(error);
            });
    }

    $scope.ViewPanelChart_Hours = function () {
        $scope.hourSelected = $('#Hours').select2("val");

        AnalystApi.GetSummary("CHART_14_C", $scope.hourSelected, $scope.dtStartDate, $scope.dtEndDate, "Count(number)")
            .success(function (retData) {
                $scope.DrawChart("CHART_14_C", "Daily Trend for Top Fraud Analyst for a Selected Hour of Day", retData[1], retData[0], "Analysts(Names)", "Time(Day)", retData[1]);
            }).error(function (error) {
                console.log(error);
            });
    }

    $scope.ChartClick = function (ReportName) {
        if ($scope.detailsCharts.includes(ReportName)) {
            $scope.flag = false;
            $scope.ReportName = ReportName;
            $scope.GetDetails();
            $("#ORIGINHOSTNAME").focus();
            $scope.$apply();
        }
    }

    $scope.GetDetails = function () {
        if ($scope.dtStartDate == '' || $scope.dtStartDate == undefined || $scope.dtStartDate == null ||
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }

        if ($scope.ReportName == "")
            return;

        AnalystApi.GetDetails($scope.ReportName, $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, $scope.CurrentPage, $scope.RowsPerPage)
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
    
    $scope.DrawChart = function (DivName, ChartText, Categories, Series, LabelText, xAxisLabel, labels) {
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
                //categories: Categories,
                type: 'datetime',
                tickInterval: 24 * 3600 * 1000, // one day
                labels: { formatter: function() {
                    return moment(this.value).format("YYYY-MM-DD");
                } ,rotation: -45 },
                crosshair: true,
                title: {
                    enabled: true,
                    text: xAxisLabel
                }
            },
            yAxis: labels != undefined ? {
                labels: {
                    formatter: function () {
                        //alert("hello");
                        return labels[this.value];
                    },

                },
                min: 0,
                startOnTick: true,
                endOnTick: true,
                minPadding: 0,
                maxPadding: 0,
                tickInterval: 1,
                title: {
                    enabled: true,
                    text: LabelText
                }
            } :
                {
                    labels: {
                        formatter: function () {
                            //alert("hello");
                            return this.value;
                        }
                    },

                    min: 0,
                    title: {
                        enabled: true,
                        text: LabelText
                    }
                },
            tooltip: labels == undefined ? {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y} </b>  </td></tr>'
                    ,
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            } :
            {
                formatter: function () {
                    return '<span style="font-size:10px">' + Highcharts.dateFormat('%A, %b %e, %Y', this.x, false) + '</span><br/><span style="color:' + this.series.color + '">'+ this.series.name +': </span>' + labels[this.y];
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

    $scope.DrawChart_Days = function (DivName, ChartText, Categories, Series, LabelText, xAxisLabel, labels) {
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
                //categories: Categories,
                //type: 'datetime',
                tickInterval: 1,
                labels: {
                    //formatter: function () {
                    //    return moment(this.value).format("YYYY-MM-DD");
                    //},
                    rotation: -45
                },
                crosshair: true,
                title: {
                    enabled: true,
                    text: xAxisLabel
                }
            },
            yAxis: labels != undefined ? {
                labels: {
                    formatter: function () {
                        //alert("hello");
                        return labels[this.value];
                    },

                },
                //showFirstLabel: false,
                min: 0,
                startOnTick: true,
                endOnTick: true,
                minPadding: 0,
                maxPadding: 0,
                 tickInterval: 1,
                title: {
                    enabled: true,
                    text: LabelText
                }
            } :
                {
                    labels: {
                        formatter: function () {
                            //alert("hello");
                            return this.value;
                        }
                    },

                    min: 1,
                   // showFirstLabel: false,
                    title: {
                        enabled: true,
                        text: LabelText
                    }
                },
            tooltip: labels == undefined ? {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">Hour: </td>' +
                    '<td style="padding:0"><b>{point.y} </b>  </td></tr>'
                    ,
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            } :
            {
                formatter: function () {
                    return '<span style="font-size:10px">' + this.x + '</span><br/><span style="color:' + this.series.color + '">'+ this.series.name +': </span>' + labels[this.y];
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

    $scope.DrawPieChart = function (DivName, ChartText, Series,LabelText,Unit) {
        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                type: 'pie',
                title:Unit,
                events: {
                    click: function (event) { $scope.ChartClick(DivName); }
                }
            },
            labels: {
                items: [{
                    html: LabelText,
                    style: {
                        top: '50',
                        left: '50'
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
    
    $scope.DrawChartVariance = function (DivName, ChartText, Series, LabelText) {
        var x = "#" + DivName;
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
                //categories: Categories,
                type: 'datetime',
                tickInterval: 24 * 3600 * 1000, // one day
                labels: {
                    formatter: function () {
                    return moment(this.value).format("YYYY-MM-DD");
                },
                    rotation: -45
                },
                crosshair: true,
               // labels: {
                    //formatter: function(){
                    //    return Highcharts.dateFormat('%d-%m-%Y', this.value);
                    //}
                //},
                //tickInterval: 1,
                title: {
                    enabled:true,
                    text: "Time(day)"
                }
            },
            yAxis: [{ // Primary yAxis
                title: {
                    enabled: true,
                    text: LabelText
                },
                min: 0,
                labels: {
                    format: '{value:,.0f}',
                    style: {
                        color: Highcharts.getOptions().colors[0]
                    }
                }
            },
        { // secondary yAxis
            labels: {
                format: DivName == 'CHART_14_A' ? '{value:,.0f}' :
                        '{value} %',
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            title: {
                text: '',
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            min:0,
            opposite: true
        }]
            ,
            tooltip: {
                formatter: function () {
                    var points = '';
                    points += '<span style="color:' + this.points[0].series.color + ';padding:0">' + this.points[0].series.name + ': </span><span>' +
                       $filter('number')(this.points[0].y) + '</span>';
                    points += '<br/><span style="color:' + this.points[1].series.color + ';padding:0">' + this.points[1].series.name + ': </span><span>' +
                       $filter('number')(this.points[1].y) + '%</span>';

                    points += '</table>'

                    return '<span style="font-size:10px">' + Highcharts.dateFormat('%A, %b %e, %Y', this.x, false) + '</span><br/>' + points;
                },
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
    
    $scope.DrawChartNames = function (DivName, ChartText, Categories, Series, LabelText, xAxisLabel) {

        var x = "#" + DivName;
        $(x).highcharts({
            chart: {
                type: 'column',
                events: {
                    click: function (event) { $scope.ChartClick(DivName); }
                }
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
                    text: xAxisLabel
                }
            },
            yAxis: {
                type: 'datetime',
                labels: {
                    formatter:function(){
                        return Highcharts.dateFormat('%m/%d/%Y', this.value);
                    },
                    //format: '{value:,.0f}'
                },
                min: new Date("2015").getTime(),
                title: {
                    enabled: true,
                    text: LabelText
                }
            },
            tooltip: {
                formatter: function () {
                    return Highcharts.dateFormat('%m/%d/%Y', this.y);
                },
                //headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                //pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                //    '<td style="padding:0"><b>{point.y}</b></td></tr>',
                //footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            plotOptions: {
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
    $scope.dtEndDate == '' || $scope.dtEndDate == undefined || $scope.dtEndDate == null) {
            return;
        }

        $scope.panelFlag = true;

        AnalystApi.GetSummary("COUNT_CLOSED_ALARMS_ANALYST", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, "Count(number)")
            .success(function (retData) {
                $scope.DrawChartVariance("COUNT_CLOSED_ALARMS_ANALYST", "Count of Closed Alarms per Analyst - Total Count", retData[0]);
            }).error(function () {
            });

        AnalystApi.GetSummary("COUNT_CLOSED_ALARMS_FRD_ANALYST", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChartVariance("COUNT_CLOSED_ALARMS_FRD_ANALYST", "Count of Closed Alarms per Analyst - Fraud Count", retData[0], "Count(number)");
            }).error(function () {
            });

        AnalystApi.GetSummary("COUNT_CLOSED_ALARMS_NFR_ANALYST", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate)
            .success(function (retData) {
                $scope.DrawChartVariance("COUNT_CLOSED_ALARMS_NFR_ANALYST", "Count of Closed Alarms per Analyst - Non Fraud Count", retData[0], "Count(number)");
            }).error(function () {
            });

        AnalystApi.GetSummary("COUNT_PIE", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, "Count(number)")
            .success(function (retData) {
                $scope.DrawPieChart("COUNT_PIE", 'Count of Closed Alarms - Fraud - Analyst Distribution', retData[0].data, 'Count(Number)');
            }).error(function () {
            });

        AnalystApi.GetSummary("AMOUNT_PIE", $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, "Amount(QAR)")
           .success(function (retData) {
               $scope.DrawPieChart("AMOUNT_PIE" , 'Count of Closed Alarms - Non Fraud - Analyst Distribution', retData[0].data,'Count(Number)');
           }).error(function () {
           });

        //AnalystApi.GetSummary("CHART_14_A", $scope.Analysts[0], $scope.dtStartDate, $scope.dtEndDate, "Count(number)")
        //    .success(function (retData) {                
        //        $scope.DrawChart("CHART_14_A", "Daily Trend of Hot Hour for a Selected Fraud Analyst", retData[1], retData[0], "Time(Hours)", "Time(Days)");
        //    }).error(function (error) {
        //        console.log(error);
        //    });
    }

    $scope.ViewReport = function () {
        $scope.dtStartDate = $("#bs-datepicker-range-start").val();
        $scope.dtEndDate = $("#bs-datepicker-range-end").val();
        $scope.ORIGINHOSTNAMEsSelected = $("#ORIGINHOSTNAME").select2("val");

        $scope.GetSummary();

        $scope.GetDetails();

        $scope.GetAnalysts();

        $scope.GetDays();

        $scope.GetHours();
    }

    $scope.PageChanged = function () {
        $scope.GetDetails();
    }

    $scope.ExportPage = function () {
        CommonMethods.ExportToExcel("myTable01", "Analysts (" + $scope.ORIGINHOSTNAMEsSelected + ")");
    }

    $scope.ExportAll = function () {
        AnalystApi.GetDetails($scope.ReportName, $scope.ORIGINHOSTNAMEsSelected, $scope.dtStartDate, $scope.dtEndDate, 1, $scope.TotalItemsCount)
            .success(function (CDRS) {
                CDRS[1].splice(0, 0, CDRS[2]);
                alasql('SELECT * INTO XLSX("Overview_report.xlsx",{headers:false}) FROM ?', [CDRS[1]]);
            }).error(function () { });
    }

    $scope.GetORIGINHOSTNAMEs();
    

});



App.factory("AnalystApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "../api/";
    var AIR_REFUNDControllerUrl = ApiPath + "Analyst/";


    function GetORIGINHOSTNAMEs() {
        return api.Get(AIR_REFUNDControllerUrl + "GetORIGINHOSTNAMEs");
    }

    function GetComboBoxData(method, startDate, endDate) {
        return api.Get(AIR_REFUNDControllerUrl + method , {dtStartDate: startDate, dtEndDate: endDate});
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
        ExportALL: ExportALL,
        GetComboBoxData: GetComboBoxData
    };
}]);

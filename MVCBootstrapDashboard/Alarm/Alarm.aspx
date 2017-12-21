<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html>

<html data-ng-app="App">
<head id="Head1" runat="server">
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0">
    <title></title>
    <script src="../Scripts/jquery-1.7.1.min.js"></script>
    <script src="../Scripts/jquery.btechco.excelexport.js"></script>
    <script src="../Scripts/jquery.base64.js"></script>
    <script src="../Scripts/alasql.min.js"></script>
    <script src="../Scripts/xlsx.core.min.js"></script>
    <script src="../Scripts/AnjularJS/angular.js"></script>
    <script src="../Scripts/AnjularJS/angular-sanitize.js"></script>
    <script src="../Scripts/AnjularJS/angular-route.js"></script>
    <script src="../Scripts/AnjularJS/ui-bootstrap-tpls-0.12.0.min.js"></script>
    <script src="../Scripts/AnjularJS/angular-webstorage.min.js"></script>
    <script src="../Scripts/AnjularJS/angular-webstorage-utils.js"></script>
    <script src="../HighChartsJS/highcharts.js"></script>
    <script src="../HighChartsJS/modules/exporting.js"></script>
    <script src="../HighChartsJS/modules/export-csv.js"></script>
    <script src="../Scripts/app.js"></script>
    <script src="Alarm_Controller.js"></script>

    <!-- Open Sans font from Google CDN -->
    <link href="http://fonts.googleapis.com/css?family=Open+Sans:300italic,400italic,600italic,700italic,400,600,700,300&subset=latin" rel="stylesheet" type="text/css">

    <!-- Pixel Admin's stylesheets -->
    <link href="../assets/stylesheets/bootstrap.min.css" rel="stylesheet" />
    <link href="../assets/stylesheets/pixel-admin.css" rel="stylesheet" />
    <link href="../assets/stylesheets/widgets.min.css" rel="stylesheet" />
    <link href="../assets/stylesheets/rtl.min.css" rel="stylesheet" />
    <link href="../assets/stylesheets/themes.min.css" rel="stylesheet" />

    <style>
        #switcher-examples .switcher {
            vertical-align: middle;
        }
    </style>
</head>

<body data-ng-controller="AlarmController" class="theme-default main-menu-animated">
    <script>var init = [];</script>

    <script>
        init.push(function () {
            var options2 = {
                orientation: $('body').hasClass('right-to-left') ? "auto right" : 'auto auto'
            }
            $('#bs-datepicker-range').datepicker(options2);

            $("#input-form").validate({
                ignore: '.ignore, .select2-input',
                focusInvalid: false,
                rules: {
                    'bs-datepicker-range-start': {
                        required: true
                    },
                    'bs-datepicker-range-end': {
                        required: true
                    }
                }
            });
        });
    </script>

    <form id='input-form' class="panel form-horizontal">
        <div class="panel-body">
            <div class="row form-group">
                <div class="input-daterange input-group" id="bs-datepicker-range">
                    <span class="input-group-addon" ng-show="detailsLevel">Date</span>
                    <input type="text" class="input-sm form-control" id='bs-datepicker-range-start' name="bs-datepicker-range-start" placeholder="Start date">
                    <span class="input-group-addon" ng-hide="detailsLevel">to</span>
                    <input type="text" ng-hide="detailsLevel" class="input-sm form-control" id='bs-datepicker-range-end' name="bs-datepicker-range-end" placeholder="End date">
                </div>
            </div>
        </div>
        <div class="panel-footer text-right">
            <button id="focus_here" class="btn btn-primary" data-ng-click="ViewReport();">View Report</button>
        </div>
    </form>

    <!---------------------------------------------Charts------------------------------------------------->
    <div class="chart" id="COUNT_CLOSED_ALARMS_FRD_NFR" style="height: 400px;" ng-show="chartLevel"></div>
    <div class="chart"id="AMOUNT_CLOSED_ALARMS_FRD_NFR" style="height: 400px;" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_FRD_NFR_PIE"  style="height: 450px; width: 50%; float: left;" ng-show="chartLevel"></div>
    <div class="chart"id="AMOUNT_FRD_NFR_PIE"  style="height: 450px; width: 50%; float: left;" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_9_PIE" style="height: 450px; width: 50%; float: left;" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_12_PIE"  style="height: 450px; width: 50%; float: left;" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_13_PIE"  style="height: 450px; width: 50%; float: left;" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_13B_PIE"  style="height: 450px; width: 50%; float: left;" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_CLOSED_ALARMS_FRD_INTERNATIONAL" style="height: 400px;float: left;width:100%" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_CLOSED_ALARMS_FRD_IRSF" style="height: 400px;float: left;width:100%" ng-show="chartLevel"></div>
    <div class="chart"id="COUNT_CLOSED_ALARMS_FRD_IMEI" style="height: 400px;float: left;width:100%" ng-show="chartLevel"></div>
    <div class="chart" id="CHART_15A" style="height: 400px;float: left;width:100%" ng-show="chartLevel"></div>
    <div class="chart" id="CHART_15B" style="height: 400px;float: left;width:100%" ng-show="chartLevel"></div>

    <!---------------------------------------------Details Table------------------------------------------------->
    <div class="row" ng-show="datewiseLevel || detailsLevel" style="margin-left: 15px">
        <h2>Details:</h2>
        <div class="table-header">
            <pagination total-items="TotalItemsCount" ng-model="CurrentPage" max-size="Pages_Max_Size" rotate="false" boundary-links="true"
                items-per-page="RowsPerPage" ng-change="PageChanged()"></pagination>

            <div class="btn dropdown" style="margin-top: -26px;" title="Export">
                <a class="dropdown-toggle" data-toggle="dropdown"><i class="fa fa-bars"></i>&nbsp;&nbsp;Export&nbsp;&nbsp;<i class="fa fa-caret-down"></i>
                </a>
                <ul class="dropdown-menu">
                    <li><a data-ng-click="ExportPage()">Current Page</a></li>
                    <li><a data-ng-click="ExportAll()">All</a></li>
                </ul>
            </div>
            <div class="btn" style="margin-top: -26px;" title="Back" ng-click="backToChartLevel()" ng-show="datewiseLevel"> Back </div>
            <div class="btn" style="margin-top: -26px;" title="Back" ng-click="BackToDaywise()" ng-show="detailsLevel"> Back </div>
        </div>

        <div class="table-primary">
            <table class="table table-bordered" id="myTable01">
                <thead>
                    <tr>
                        <th ng-repeat="header in TableHeaders">{{header}} </th>
                    </tr>
                </thead>
                <tbody>
                    <tr class="gradeU" ng-repeat="Row in TableRecords">
                        <th ng-repeat="item in Row track by $index" > 
                            <div ng-if="hasMoreDetails != true || $index != 0">{{item}}</div> 
                            <div ng-if="hasMoreDetails == true && $index == 0"><a href ng-click="DayClick(item)"> {{item}} </a></div> 
                        </th>
                        <%--<th style ="cursor:pointer;" data-ng-click="DayClick(Row[0])">{{Row[0]}} </th>
                        <th>{{Row[1]}}</th>
                        <th>{{Row[2]}}</th>--%>
                    </tr>

                </tbody>
            </table>
        </div>

        <div class="table-footer">
            <p class="text-center text-success" data-ng-show="TableRecords.length == 0">No Data Found</p>
            <pagination total-items="TotalItemsCount" ng-model="CurrentPage" max-size="Pages_Max_Size" rotate="false" boundary-links="true"
                items-per-page="RowsPerPage" ng-change="PageChanged()"></pagination>
        </div>
    </div>

    

    <!-- Pixel Admin's javascripts -->
    <script src="../assets/javascripts/bootstrap.min.js"></script>
    <script src="../assets/javascripts/pixel-admin.min.js"></script>

    <!-- Used for X-Editable demo only. You can remove this lines -->
    <script src="../assets/javascripts/jquery.mockjax.js"></script>
    <script src="../assets/javascripts/demo-mock.js"></script>
    <!---->

    <script type="text/javascript">
        init.push(function () {
            // Javascript code here
        })
        window.PixelAdmin.start(init);
    </script>

</body>
</html>

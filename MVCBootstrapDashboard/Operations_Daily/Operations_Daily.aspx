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
    <script src="Operations_Daily_Controller.js"></script>

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
<body data-ng-controller="OperationsDailyController" class="theme-default main-menu-animated">
    <script>var init = [];</script>

    <script>
        init.push(function () {
            //Multiselect
            $("#ORIGINHOSTNAME").select2({
                placeholder: "Loading ..."
            });
        })
    </script>

    <script>
        init.push(function () {
            var options2 = {
                orientation: $('body').hasClass('right-to-left') ? "auto right" : 'auto auto'
            }
            $('#bs-datepicker-range').datepicker(options2);

            $("#input-form").validate({
                ignore: '.ignore',
                focusInvalid: false,
                rules: {
                    'bs-datepicker-range-start': {
                        required: true
                    },
                    'bs-datepicker-range-end': {
                        required: true
                    },
                    'ORIGINHOSTNAME': {
                        required: true
                    }
                }
            });
        });
    </script>

    <form id='input-form' class="panel form-horizontal">
        <div class="panel-body">
            <div class="row form-group">
                <label class="col-sm-1 control-label">Report:</label>
                <div class="col-sm-10">
                    <select id="ORIGINHOSTNAME" name="ORIGINHOSTNAME" class="form-control">
                        <option data-ng-repeat="indentifier in ORIGINHOSTNAMEs" value="{{indentifier}}">{{indentifier}}</option>
                    </select>
                </div>
                <div class="col-sm-1" ng-show="false">
                    <span style="cursor: pointer; width: 100%" class="label" data-ng-click="SelectAllORIGINHOSTNAMEs();">Select all</span>
                    <div style="height: 1px"></div>
                    <span style="cursor: pointer; width: 100%" class="label label-danger" data-ng-click="UnSelectAllORIGINHOSTNAMEs();">Unselect all</span>
                </div>
            </div>
            <div class="row form-group">
                <div class="input-daterange input-group" id="bs-datepicker-range">
                    <input type="text" class="input-sm form-control" id='bs-datepicker-range-start' name="bs-datepicker-range-start" placeholder="Start date">
                    <span class="input-group-addon">to</span>
                    <input type="text" class="input-sm form-control" id='bs-datepicker-range-end' name="bs-datepicker-range-end" placeholder="End date">
                </div>
            </div>
        </div>
        <div class="panel-footer text-right">
            <button class="btn btn-primary" data-ng-click="ViewReport();">View Report</button>
        </div>
    </form>

    <!---------------------------------------------Charts------------------------------------------------->
    <div id="MSC_COUNT_DURATION" style="height: 400px;" ng-show="flag"></div>
    <div id="IN_TOTALCOUNT_DURATION" style="height: 400px;" ng-show="flag"></div>
    <div id="MSC_VS_IN_CWITHV" style="height: 400px;" ng-show="flag"></div>
    <div id="MSC_VS_IN_DWITHV" style="height: 400px;" ng-show="flag"></div>
    <div id="INMAD_VS_INMAC_VS_COST" style="height: 400px;" ng-show="flag"></div>
    <div id="INDAD_VS_INDAC_VS_COST" style="height: 400px;" ng-show="flag"></div>
    <div id="INMAD_VS_INDAD" style="height: 400px;" ng-show="flag"></div>
    <div id="FILTER_OUT" style="height: 400px;" ng-show="flag"></div>
    <!---------------------------------------------Details Table------------------------------------------------->
    <div class="row" ng-show="!flag" style="margin-left: 15px">
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
            <div class="btn" style="margin-top: -26px;" title="Back">
                <a href ng-click="back()">Back</a>
            </div>
        </div>

        <div class="table-primary">
            <table class="table table-bordered" id="myTable01">
                <thead>
                    <tr>
                        <th ng-repeat="header in TableHeaders"> {{header}} </th>
                    </tr>
                </thead>
                <tbody>
                    <tr class="gradeU" data-ng-repeat="Row in TableRecords">
                        <th ng-repeat="item in Row track by $index"> {{filterItem(item, $index)}} </th>
                        <%--<th>{{Row[0]}}</th>
                        <th>{{Row[1]}}</th>
                        <th>{{Row[2]}}</th>
                        <th>{{Row[3] | number:0}}</th>
                        <th>{{Row[4] | number:0}}</th>
                        <th>{{Row[5] | number:0}}</th>--%>
                    </tr>

                </tbody>
            </table>
            <%--<div class="table-footer">--%>

            <%-- </div>--%>
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

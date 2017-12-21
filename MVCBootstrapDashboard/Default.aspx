<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Javascript -->
    <script>
        init.push(function () {
            $('ul.bs-tabdrop-example').tabdrop();
        });
    </script>
    <!-- / Javascript -->

    <div class="panel">
        <div id="ui-tabs-demo">
            <ul class="nav nav-tabs bs-tabdrop-example">
                <li id='{{Report.ID}}' data-ng-repeat="Report in Reports" ng-click="ActiveReportTab(Report.ID, Report.TabID);" class="{{Report.Class}}">
                    <a style="cursor:pointer" data-toggle="tab">
                        <button type="button" class="close" title="Close Report" ng-click="CloseReportTab(Report.ID)">×</button>{{Report.Name}}</a>

                </li>
            </ul>

            <!--  .tab-content -->
            <div class="tab-content tab-content">
                <div id='{{Report.ID}}' data-ng-repeat="Report in Reports" class="tab-pane{{Report.Class}}">
                    <iframe src='{{Report.URL}}' height="600" style="width:100%; border:none;" onload="resizeIframe(this)"></iframe>
                </div>
            </div>

            <!-- / .tab-content -->
        </div>
    </div>
</asp:Content>



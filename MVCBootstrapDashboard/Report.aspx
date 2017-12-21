<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        init.push(function () {
            Morris.Bar({
                element: 'hero-bar',
                data: [
                    { device: 'iPhone', geekbench: 136 },
                    { device: 'iPhone 3G', geekbench: 137 },
                    { device: 'iPhone 3GS', geekbench: 275 },
                    { device: 'iPhone 4', geekbench: 380 },
                    { device: 'iPhone 4S', geekbench: 655 },
                    { device: 'iPhone 5', geekbench: 1571 }
                ],
                xkey: 'device',
                ykeys: ['geekbench'],
                labels: ['Geekbench'],
                barRatio: 0.4,
                xLabelAngle: 35,
                hideHover: 'auto',
                barColors: PixelAdmin.settings.consts.COLORS,
                gridLineColor: '#cfcfcf',
                resize: true
            });
        });
    </script>
    <!-- / Javascript -->

    <div class="panel">
        <div class="panel-heading">
            <span class="panel-title">KoKo Report</span>
        </div>
        <div class="panel-body">
            <div class="graph-container">
                <div id="hero-bar" class="graph"></div>
            </div>
        </div>
    </div>
</asp:Content>

var App = angular.module('App', ['ui.bootstrap', 'webStorageModule', 'ngRoute']);

App.controller('TabsMenusReportsController', function ($scope, $rootScope, $sce, $location, TabsApi, MenusApi, LoginApi, LogApi, webStorage) {
    $scope.Tabs = [];
    $scope.Menus = [];
    $scope.Reports = [];

    $scope.UserID = webStorage.local.get('UserID');
    $scope.Name = webStorage.local.get('Name');
    $scope.Username = webStorage.local.get('Username');
    $scope.RoleID = webStorage.local.get('RoleID');
    $scope.LastLoginTime = new Date(webStorage.local.get('DateTime'));
    $scope.DateNow = new Date();


    $scope.FreeStorage = function () {
        webStorage.local.add('Username', null);
        webStorage.local.add('Name', null);
        webStorage.local.add('UserID', null);
        webStorage.local.add('RoleID', null);
        webStorage.local.add('DateTime', null);

        $scope.UserID = null;
        $scope.Name = null;
        $scope.Username = null;
        $scope.RoleID = null;
    }


    if ($scope.LastLoginTime == null) {
        $scope.FreeStorage();
    }
    else {
        var hours = Math.abs($scope.DateNow - $scope.LastLoginTime) / 3600000;

        if (hours > 1) {
            $scope.FreeStorage();
        }
    }

    $scope.loginfailed = '';

    var CurrentTabID = 1;
    var ActivatedReportTabID = -1;
    var ActivatedID = -1;

    $scope.MainTabs = function () {
        var roleID = $scope.RoleID[$scope.RoleID.length - 1];
        TabsApi.GetMainTabs(roleID).success(function (mainTabs) {
            $scope.Tabs = mainTabs;
            if ($scope.Tabs.length != 0)
                $scope.changeTabs($scope.Tabs[0].ID);
        }).error(function () {
            $scope.Logout();
        });
    }

    var path = $location.absUrl().split('/');

    if ($scope.RoleID == null && path[path.length - 1] != "Login.aspx") {
        window.location = "Login.aspx";
    }

    if (path[path.length - 1] != "Login.aspx") {
        $scope.MainTabs();
    }
    else if (path[path.length - 1] == "Login.aspx") {
        if ($scope.RoleID != null)
            window.location = "Default.aspx";
    }

    $scope.changeTabs = function (id) {
        var roleID = $scope.RoleID[$scope.RoleID.length - 1];
        MenusApi.GetMenus(id, roleID).success(function (menus) {
            $scope.Menus = menus;
            CurrentTabID = id;
            ActivateMenuItem(ActivatedID, id);
        }).error(function () {
            //$scope.Logout();
        });
    }

    $scope.OpenReportInTab = function (ID, URL, Title) {
        if (URL == '#') {
            ActivatedID = ID;
            return;
        }
        var my_array = URL.split('/');
        var href = '#' + my_array[my_array.length - 1].split('.')[0];

        var Report = { "ID": ID, "URL": $sce.trustAsResourceUrl(URL), "Name": Title, "href": href, "TabID": CurrentTabID, "Class": "Active" };

        // check if the report is already opened
        for (var i = 0; i < $scope.Reports.length; i++) {
            if ($scope.Reports[i].ID == ID) {
                $scope.ActiveReportTab(ID, CurrentTabID);
                return;
            }
        }

        $scope.Reports.push(Report);
        $scope.ActiveReportTab(ID, CurrentTabID);

        
        LogApi.LogOperation($scope.UserID, "Open " + Title)
            .success()
            .error();

        return;
    }

    $scope.ActiveReportTab = function (ID, TabID) {

        ActivateMenuItem(ID, TabID);

        for (var i = 0; i < $scope.Reports.length; i++) {
            if ($scope.Reports[i].ID == ID) {
                $scope.Reports[i].Class = "active";
                ActivatedReportTabID = i;
                ActivatedID = $scope.Reports[i].ID;
            }
            else {
                $scope.Reports[i].Class = "";
            }
        }
    }

    $scope.CloseReportTab = function (ID) {

        // check if the report is already opened
        for (var i = 0; i < $scope.Reports.length; i++) {
            if ($scope.Reports[i].ID == ID) {
                $scope.Reports.splice(i, 1);
                if (i != 0 && ActivatedReportTabID == i) {
                    $scope.ActiveReportTab($scope.Reports[i - 1].ID, $scope.Reports[i - 1].TabID);
                }
                else if (i == 0 && ActivatedReportTabID == i && $scope.Reports.length > 0) {
                    $scope.ActiveReportTab($scope.Reports[0].ID, $scope.Reports[0].TabID);
                }
                else if (i == 0 && $scope.Reports.length == 0) {
                    ActivatedReportTabID = -1;
                    ActivatedID = -1;

                    ActivateMenuItem(ActivatedID, CurrentTabID);
                }
                return;
            }
        }
    }

    function ActivateMenuItem(ID, TabID) {
        if (TabID != CurrentTabID) {
            // if the activated Menu item is no in the curent menu, so we need to update the menu then activate the report in the new menu.
            // changeTabs() working async so we call ActivateMenuItem() again inside success function of changeTabs(), so we can be sure the Menus array is filled with new menu
            $scope.changeTabs(TabID);
            return;
        }

        // activate the menu item of opened report
        for (var i = 0; i < $scope.Menus.length; i++) {
            var child = $scope.Menus[i].Children;
            if (child != null) {
                $scope.Menus[i].LIClass = "mm-dropdown ng-scope mm-dropdown-root";
                for (var j = 0; j < child.length; j++) {
                    if (child[j].ID == ID) {
                        child[j].LIClass = "active";
                        $scope.Menus[i].LIClass = "mm-dropdown ng-scope mm-dropdown-root open";
                    }
                    else
                        child[j].LIClass = "";
                }
            }
        }
    }

    $scope.Login = function () {
        var username = $("#username_id").val();
        var password = $("#password_id").val();
        var time = new Date();

        LoginApi.ISLogin(username, password).success(function (retData) {
            $scope.loginfailed = '';

            if (retData == 'null') {
                $scope.loginfailed = 'Your login attempt was not successful. Please try again';
                return;
            }

            //LogApi.LogOperation(retData.ID, "Login")
            //    .success()
            //    .error();

            webStorage.local.add('Username', retData.Username);
            webStorage.local.add('Name', retData.Name);
            webStorage.local.add('UserID', retData.ID);
            webStorage.local.add('RoleID', '3258887b-9bc5-449a-84d8-b49a6cc81f73' + retData.RoleID);
            webStorage.local.add('DateTime', time);

            window.location = "Default.aspx";
        }).error();
    }

    $scope.Logout = function () {
        $scope.FreeStorage();
        window.location = "Login.aspx";
    }
});

App.factory("api", ["$http", "$rootScope", "$q", function ($http, $rootScope, $q) {
    function send(method, url, params, data) {
        return $http({
            method: method,
            url: url,
            params: params,
            data: data
        });
    }

    function Get(url, params, data) {
        return send("GET", url, params, data);
    }

    function Post(url, params, data) {
        return send("POST", url, params, data);
    }

    function Delete(url, params, data) {
        return send("Delete", url, params, data);
    }

    return {
        Get: Get,
        Post: Post,
        Delete: Delete
    };
}]);

App.factory("TabsApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "api/";
    var TabsControllerUrl = ApiPath + "Tabs/";
     

    function GetMainTabs(iRoleID) {
        var params = {
            'iRoleID': iRoleID
        }
        return api.Get(TabsControllerUrl + "GetMainTabs", params);
    }
    return {
        GetMainTabs: GetMainTabs
    };
}]);

App.factory("MenusApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "api/";
    var MenusControllerUrl = ApiPath + "Menus/";


    function GetMenus(ID, iRoleID) {
        var params = {
            'p_iTabID': ID,
            'iRoleID': iRoleID
        }
        return api.Get(MenusControllerUrl + "GetMenus", params);
    }
    return {
        GetMenus: GetMenus
    };
}]);

App.factory("LoginApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "api/";
    var LoginControllerUrl = ApiPath + "Login/";

    function ISLogin(username, password) {
        var params = {
            'username': username,
            'password': password
        }
        return api.Post(LoginControllerUrl + "ISLogin", params);
    }
    return {
        ISLogin: ISLogin
    };
}]);

App.factory("ChartsApi", ["api", "$rootScope", function (api, $rootScope) {
    var ApiPath = "../api/";
    var ControllerUrl = ApiPath + "Charts/";

    function DownloadReport(data_to_send) {
        return api.Post(ControllerUrl + "DownloadReport", null, data_to_send);
    }
    return {
        DownloadReport: DownloadReport
    };
}]);

App.factory("LogApi", ["api", "$rootScope", function (api, $rootScope, $location) {
    var ApiPath = "../api/";
    var ControllerUrl = ApiPath + "Log/";

    function LogOperation(iUserID, strOperation) {
        var params = {
            'iUserID': iUserID,
            'strOperation': strOperation
        }
        return api.Post(ControllerUrl + "LogOperation", params);
    }
    return {
        LogOperation: LogOperation
    };
}]);


App.factory("CommonMethods", function ($http, $rootScope) {
    
    function ExportToExcel(TableName, FileName) {
        $("#" + TableName).btechco_excelexport({
            containerid: TableName,
            datatype: $datatype.Table,
            filename: FileName
        });
    }

    function ForceEndDate(EndDate, backDayes) {
        var EndDateObject = new Date(EndDate);

        var LastAllowedEndDate = new Date();
        LastAllowedEndDate.setDate(LastAllowedEndDate.getDate() - backDayes);

        var hours = (EndDateObject - LastAllowedEndDate) / 86400000;

        if (hours > 0) {
            var dd = LastAllowedEndDate.getDate();
            var mm = LastAllowedEndDate.getMonth() + 1; //January is 0!
            var yyyy = LastAllowedEndDate.getFullYear();

            if (dd < 10) {
                dd = '0' + dd
            }

            if (mm < 10) {
                mm = '0' + mm
            }

            return mm + '/' + dd + '/' + yyyy;
        }
        else {
            var dd = EndDateObject.getDate();
            var mm = EndDateObject.getMonth() + 1; //January is 0!
            var yyyy = EndDateObject.getFullYear();

            if (dd < 10) {
                dd = '0' + dd
            }

            if (mm < 10) {
                mm = '0' + mm
            }

            return mm + '/' + dd + '/' + yyyy;
        }
    }

    function Formate_Date(today, formate) {
        var _date = new Date(today);

        var dd = _date.getDate();
        var mm = _date.getMonth() + 1; //January is 0!

        var yyyy = _date.getFullYear();
        if (dd < 10) {
            dd = '0' + dd
        }
        if (mm < 10) {
            mm = '0' + mm
        }

        if(formate == 'dm')
            today = dd + '/' + mm + '/' + yyyy;
        else if (formate == 'md')
            today = mm + '/' + dd + '/' + yyyy;
        return today;
    }
    return {
        ExportToExcel: ExportToExcel,
        ForceEndDate: ForceEndDate,
        Formate_Date: Formate_Date
    };
});

App.constant("TablesConfig", {
    'Page_Size': 10,
    'Pages_Max_Size': 7
});
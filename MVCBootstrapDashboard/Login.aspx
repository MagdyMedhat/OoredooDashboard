<%@ Page Title="" Language="C#" MasterPageFile="~/Login.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!DOCTYPE html>
    <!--[if IE 8]>         <html class="ie8"> <![endif]-->
    <!--[if IE 9]>         <html class="ie9 gt-ie8"> <![endif]-->
    <!--[if gt IE 9]><!-->
    <html class="gt-ie8 gt-ie9 not-ie">
    <!--<![endif]-->
    <head>
        <meta charset="utf-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
        <title>Sign In - iView 360</title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0">

        <!-- Open Sans font from Google CDN -->
        <link href="http://fonts.googleapis.com/css?family=Open+Sans:300italic,400italic,600italic,700italic,400,600,700,300&subset=latin" rel="stylesheet" type="text/css">


        <!-- Pixel Admin's stylesheets -->
        <link href="assets/stylesheets/bootstrap.min.css" rel="stylesheet" type="text/css">
        <link href="assets/stylesheets/pixel-admin.min.css" rel="stylesheet" type="text/css">
        <link href="assets/stylesheets/pages.min.css" rel="stylesheet" type="text/css">
        <link href="assets/stylesheets/rtl.min.css" rel="stylesheet" type="text/css">
        <link href="assets/stylesheets/themes.min.css" rel="stylesheet" type="text/css">

        <!--[if lt IE 9]>
		<script src="assets/javascripts/ie.min.js"></script>
	<![endif]-->


        <!-- $DEMO =========================================================================================

	Remove this section on production
-->
        <style>
            #signin-demo
            {
                position: fixed;
                right: 0;
                bottom: 0;
                z-index: 10000;
                background: rgba(0,0,0,.6);
                padding: 6px;
                border-radius: 3px;
            }

                #signin-demo img
                {
                    cursor: pointer;
                    height: 40px;
                }

                    #signin-demo img:hover
                    {
                        opacity: .5;
                    }

                #signin-demo div
                {
                    color: #fff;
                    font-size: 10px;
                    font-weight: 600;
                    padding-bottom: 6px;
                }
        </style>
        <!-- / $DEMO -->

    </head>


    <!-- 1. $BODY ======================================================================================
	
	Body

	Classes:
	* 'theme-{THEME NAME}'
	* 'right-to-left'     - Sets text direction to right-to-left
-->
    <body class="theme-default page-signin">

        <script>
            var init = [];
            init.push(function () {
                var $div = $('<div id="signin-demo" class="hidden-xs"><div>PAGE BACKGROUND</div></div>'),
                    bgs = ['assets/demo/signin-bg-1.jpg', 'assets/demo/signin-bg-2.jpg', 'assets/demo/signin-bg-3.jpg',
                             'assets/demo/signin-bg-4.jpg', 'assets/demo/signin-bg-5.jpg', 'assets/demo/signin-bg-6.jpg',
                             'assets/demo/signin-bg-7.jpg', 'assets/demo/signin-bg-8.jpg', 'assets/demo/signin-bg-9.jpg'];
                for (var i = 0, l = bgs.length; i < l; i++) $div.append($('<img src="' + bgs[i] + '">'));
                $div.find('img').click(function () {
                    var img = new Image();
                    img.onload = function () {
                        $('#page-signin-bg > img').attr('src', img.src);
                        $(window).resize();
                    }
                    img.src = $(this).attr('src');
                });
                $('body').append($div);
            });
        </script>
        <!-- Demo script -->
        <script src="assets/demo/demo.js"></script>
        <!-- / Demo script -->

        <!-- Page background -->
        <div id="page-signin-bg">
            <!-- Background overlay -->
            <div class="overlay"></div>
            <!-- Replace this with your bg image -->
            <img src="assets/demo/signin-bg-1.jpg" alt="">
        </div>
        <!-- / Page background -->


        <!-- Container -->
        <div class="signin-container">

            <!-- Left side -->
            <div class="signin-info">
                <a href="index.html" class="logo">
                    <img src="assets/demo/logo-big.png" alt="" style="margin-top: -5px;">&nbsp;
				iView 360
                </a>
                <!-- / .logo -->
                <div class="slogan">
                    Simple. Flexible. Powerful.
                </div>
                <!-- / .slogan -->
                <ul>
<%--                    <li><i class="fa fa-sitemap signin-icon"></i>Flexible modular structure</li>--%>
<%--                    <li><i class="fa fa-file-text-o signin-icon"></i>LESS &amp; SCSS source files</li>--%>
<%--                    <li><i class="fa fa-outdent signin-icon"></i>RTL direction support</li>--%>
                   
                </ul>
                <!-- / Info list -->
            </div>
            <!-- / Left side -->

            <!-- Right side -->
            <form class="signin-form">
                <img src="StingLogo.png" alt="" style="margin-top: -5px;">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
           
                <img src="zain-logo.png" alt="" style="width:150px;height:30px;" style="margin-top: -5px;">
                <br>
                <br>

                    <div class="signin-text">
                        <span>Sign In to your account</span>
                    </div>
                    <!-- / .signin-text -->

                    <div class="form-group w-icon">
                        <input type="text" name="signin_username" id="username_id" class="form-control input-lg" placeholder="Username">
                        <span class="fa fa-user signin-form-icon"></span>
                    </div>
                    <!-- / Username -->

                    <div class="form-group w-icon">
                        <input type="password" name="signin_password" id="password_id" class="form-control input-lg" placeholder="Password">
                        <span class="fa fa-lock signin-form-icon"></span>
                    </div>
                    <!-- / Password -->

                    <div class="form-actions">
                        <button type="submit" data-ng-click="Login()" class="signin-btn bg-primary">SIGN IN</button>
                    </div>
            </form>
            <!-- Right side -->
        </div>
        <!-- / Container -->

        <div class="not-a-member" id="loginfailed">
            {{loginfailed}}
        </div>

        <!-- Get jQuery from Google CDN -->
        <!--[if !IE]> -->
        
                
        <script type="text/javascript"> window.jQuery || document.write('<script src="Scripts/jquery-1.7.1.min.js">' + "<" + "/script>"); </script>

        
    <%--<script type="text/javascript"> window.jQuery || document.write('<script src="https://ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js">' + "<" + "/script>"); </script>--%>
        <!-- <![endif]-->
        <!--[if lte IE 9]>
	<script type="text/javascript"> window.jQuery || document.write('<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js">'+"<"+"/script>"); </script>
<![endif]-->


        <!-- Pixel Admin's javascripts -->
        <script src="assets/javascripts/bootstrap.min.js"></script>
        <script src="assets/javascripts/pixel-admin.min.js"></script>

        <script type="text/javascript">
            // Resize BG
            init.push(function () {
                var $ph = $('#page-signin-bg'),
                    $img = $ph.find('> img');

                $(window).on('resize', function () {
                    $img.attr('style', '');
                    if ($img.height() < $ph.height()) {
                        $img.css({
                            height: '100%',
                            width: 'auto'
                        });
                    }
                });
            });

            // Show/Hide password reset form on click
            init.push(function () {
                $('#forgot-password-link').click(function () {
                    $('#password-reset-form').fadeIn(400);
                    return false;
                });
                $('#password-reset-form .close').click(function () {
                    $('#password-reset-form').fadeOut(400);
                    return false;
                });
            });

            // Setup Sign In form validation
            init.push(function () {
                $("#signin-form_id").validate({ focusInvalid: true, errorPlacement: function () { } });

                // Validate username
                $("#username_id").rules("add", {
                    required: true,
                    minlength: 3
                });

                // Validate password
                $("#password_id").rules("add", {
                    required: true,
                    minlength: 6
                });
            });

            // Setup Password Reset form validation
            init.push(function () {
                $("#password-reset-form_id").validate({ focusInvalid: true, errorPlacement: function () { } });

                // Validate email
                $("#p_email_id").rules("add", {
                    required: true,
                    email: true
                });
            });

            function Login() {
                //alert('Inside Login');

                var username = document.getElementById('username_id').value;
                var password = document.getElementById('password_id').value;
                //alert(username);
                //alert(password);

                $.ajax({
                    type: "POST",
                    url: "/api/Login/ISLogin?username=" + username + "&password=" + password,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (LoginState) {

                        if (LoginState == true)
                            window.location = "Default.aspx";
                        else
                        {
                            var loginfailed = document.getElementById('loginfailed');
                            loginfailed.innerText = 'Your login attempt was not successful. Please try again';
                        }
                    }
                });

                //$.ajax({
                //    type: "Get",
                //    url: "/api/Menus/GetMenus?p_iTabID=" + 1,
                //    contentType: "application/json; charset=utf-8",
                //    dataType: "json",
                //    success: function (lstMenus) {
                //        alert(lstMenus);
                //    }
                //});
            }


            window.PixelAdmin.start(init);
        </script>

    </body>
    </html>
</asp:Content>

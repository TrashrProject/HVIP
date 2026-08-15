<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

require_once "app/init.pz.php";

if(isset($_GET['fb_reg'])):
    // Code and return of check username
    $U = ucwords(strtolower(AppFunctions::GeneralClean($_POST['uname'])));
    $R = $USession->ValidateUserName($U);
    echo $R;
    exit;
endif;

if(isset($_GET['fb_submit'])):
    // Code and return of check username
    $U = AppFunctions::GeneralClean($_POST['fb_u']);
    $E = AppFunctions::GeneralClean($_POST['fb_e']);
    $ID = AppFunctions::GeneralClean($_POST['fb_id']);
    $N = AppFunctions::GeneralClean($_POST['fb_name']);


    $R = $USession->FBRegister($E, $N, $ID, $U);
    $R_ = json_decode($R);

    echo $R;
    exit;



endif;


if(isset($_GET['code'])):

    $Token = json_decode($FBManager->ExchangeCode($_GET['code']));
    $Data = json_decode($FBManager->GetFBData($Token->access_token));
    if($Data->id == null):
        echo "<b>Error [5000] : Contacta a un administrador en nuestro <a href='https://discord.gg/4M5f4aQwNb'>Discord</a></b>";
        exit;
    endif;

    if($FBManager->FBExists($Data->email, $Data->id)):
        // We login the user
        if($USession->FBLogin($Data->email, $Data->id)):
            echo "<script>window.opener.location.href = \"/me\"; self.close();</script>";
            exit;
        else:
            echo "<script>window.opener.location.href = \"/?bannedw\"; self.close();</script>";
        endif;
    else:
        // Ask user to insert username desired to play
        require_once HEADER . "fb_login.php";
        require_once BODY . "fb_login.php";

    endif;

endif;



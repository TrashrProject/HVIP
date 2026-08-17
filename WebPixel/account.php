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

// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;

if(isset($_GET['chanpass'])):

    if(isset($_POST['current_password']) && isset($_POST['new_password']) && isset($_POST['new_password_confirmation'])):
        $CP = AppFunctions::GeneralClean($_POST['current_password']);
        $NP = AppFunctions::GeneralClean($_POST['new_password']);
        $CNP = AppFunctions::GeneralClean($_POST['new_password_confirmation']);
        if($NP == $CNP):
            $R = json_decode($UserMG->ChangeOldPass($UData['username'], $CP, $NP));
            $_SESSION['E'] = $R->result;
            $_SESSION['M'] = $R->msg;
        else:
            $_SESSION['E'] = false;
            $_SESSION['M'] = "Les mots de passe ne correspondent pas.";
        endif;
    endif;

endif;

//Page Name
$PageName = "Ajustes de Cuenta";

// Load Pages
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'profile_settings.php';
require_once FOOTER . 'main.php';


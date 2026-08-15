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

if(!isset($_GET['id'])):
    header("Location: /corporations");
    exit;
endif;

// Initialize Corporation Manager
$CID = AppFunctions::GeneralClean($_GET['id']);
require_once "app/Controller/Business.class.php";
$Corp = new Business($DB, $CID);

// Corp construct verifies if Corp ID is valid
if(!$Corp->status):
    header("Location: /corporations");
    exit;
endif;

//Page Name
$PageName = "Corporaciones";

// Load Pages
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'corporation.php';
require_once FOOTER . 'main.php';



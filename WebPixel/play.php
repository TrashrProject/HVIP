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

// Redirect if not logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;

//Page Name
$PageName = "Play";

// Load Pages
//require_once HEADER . 'main.php';
//require_once NAVBAR . 'navbar.php';
//require_once CLIENT . 'client.php';
require_once CLIENT . 'client.php';
//require_once FOOTER . 'main.php';

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
if($Session->Exist(Config::$SessionName)):
    header("Location: /me");
    exit;
endif;

// Load Pages
require_once HEADER . 'index.php';
require_once BODY . 'index.php';

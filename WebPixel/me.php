<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 */

require_once "app/init.pz.php";

// Redirect if not logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

$PageName = "Me";

require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'me.php';
require_once FOOTER . 'main.php';

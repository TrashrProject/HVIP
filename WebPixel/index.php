<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 */

require_once "app/init.pz.php";

// Redirect if logged in
if($Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/me.php");
    exit;
endif;

require_once HEADER . 'index.php';
require_once BODY . 'index.php';

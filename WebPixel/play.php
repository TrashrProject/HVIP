<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * Localhost/XAMPP entry point for the client.
 */

require_once "app/init.pz.php";

// Redirect if not logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

$PageName = "Play";
require_once CLIENT . 'client.php';

<?php
/**
 * Velora RP - citizen dashboard.
 */

require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

$PageName = "Mon espace";

require_once HEADER . 'me.php';
require_once BODY . 'me.php';
require_once FOOTER . 'me.php';

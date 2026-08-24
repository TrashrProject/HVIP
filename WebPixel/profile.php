<?php
require_once "app/init.pz.php";
if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;
$PageName = "Profil utilisateur";
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'profile.php';
require_once FOOTER . 'main.php';

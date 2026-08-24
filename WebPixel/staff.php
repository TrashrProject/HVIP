<?php
require_once "app/init.pz.php";
if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/"); exit;
endif;
$PageName = "Équipe";
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'staffs.php';
require_once FOOTER . 'main.php';

<?php
require_once "app/init.pz.php";
if($Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/me.php");
    exit;
endif;
require_once HEADER . 'index.php';
require_once BODY . 'index.php';

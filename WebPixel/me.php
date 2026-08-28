<?php
require_once "app/init.pz.php";
require_once "app/Controller/ArticleService.class.php";
if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;
$PageName = "Me";
$ArticleService = new ArticleService($DB->Con());
$LatestArticles = $ArticleService->latest(4);
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'me.php';
require_once FOOTER . 'main.php';

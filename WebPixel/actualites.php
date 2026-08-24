<?php
require_once 'app/init.pz.php';require_once 'app/Controller/ArticleService.class.php';
if(!$Session->Exist(Config::$SessionName)){header('Location: '.Config::$URL.'/');exit;}
$PageName='Actualités';$ArticleService=new ArticleService($DB->Con());$NewsPage=max(1,(int)($_GET['page']??1));$NewsRows=$ArticleService->latest(9,($NewsPage-1)*9);
require_once HEADER.'main.php';require_once NAVBAR.'navbar.php';require_once BODY.'actualites.php';require_once FOOTER.'main.php';

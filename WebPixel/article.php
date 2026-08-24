<?php
require_once 'app/init.pz.php';require_once 'app/Controller/ArticleService.class.php';
if(!$Session->Exist(Config::$SessionName)){header('Location: '.Config::$URL.'/');exit;}
$ArticleService=new ArticleService($DB->Con());$Article=$ArticleService->findBySlug(trim((string)($_GET['slug']??'')));
if(!$Article){http_response_code(404);$PageName='Article introuvable';require_once HEADER.'main.php';require_once NAVBAR.'navbar.php';echo '<div class="content"><div class="container"><div class="content-box"><div class="title">Article introuvable</div><div class="box-content p-4">Cette actualité n’existe pas ou n’est pas publiée.</div></div></div></div>';require_once FOOTER.'main.php';exit;}
$PageName=$Article['title'];require_once HEADER.'main.php';require_once NAVBAR.'navbar.php';require_once BODY.'article.php';require_once FOOTER.'main.php';

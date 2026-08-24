<?php
$PageName='Boutique';require_once 'app/init.pz.php';
if(!$Session->Exist(Config::$SessionName)){header('Location: '.Config::$URL.'/');exit;}
require_once 'app/Controller/ShopService.class.php';$Shop=new ShopService($DB->Con());$ShopNotice='';$ShopError='';
if(empty($_SESSION['shop_csrf']))$_SESSION['shop_csrf']=bin2hex(random_bytes(32));if(empty($_SESSION['shop_request_token']))$_SESSION['shop_request_token']=bin2hex(random_bytes(16));
if($_SERVER['REQUEST_METHOD']==='POST'&&($_POST['action']??'')==='buy'){try{if(!hash_equals($_SESSION['shop_csrf'],(string)($_POST['csrf']??'')))throw new RuntimeException('Formulaire expiré.');$result=$Shop->buy((int)$UData['id'],(int)($_POST['product_id']??0),(string)($_POST['request_token']??''));$_SESSION['shop_request_token']=bin2hex(random_bytes(16));$ShopNotice='Achat effectué : '.$result['product'].'.';$User=new User($DB,$Session);$UData=$User->UData;$UPData=$User->UPData;}catch(Throwable $e){$ShopError=$e instanceof RuntimeException?$e->getMessage():'Achat impossible.';}}
$category=max(0,(int)($_GET['category']??0));$ShopCategories=$Shop->categories();$ShopProducts=$Shop->products($category);$ShopHistory=$Shop->history((int)$UData['id']);
require_once HEADER.'main.php';require_once NAVBAR.'navbar.php';require_once BODY.'store.php';require_once FOOTER.'main.php';

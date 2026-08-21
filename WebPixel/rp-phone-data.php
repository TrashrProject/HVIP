<?php
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

function pr_phone_json(array $data,int $status=200):void{http_response_code($status);echo json_encode($data,JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);exit;}
try{
 require_once __DIR__.'/app/init.pz.php';
 require_once __DIR__.'/paradise-phone-lib.php';
 if(!isset($Session,$DB)||!class_exists('Config'))pr_phone_json(['ok'=>false,'reason'=>'bootstrap_unavailable']);
 $username=trim((string)$Session->Read(Config::$SessionName));
 if($username==='')pr_phone_json(['ok'=>false,'reason'=>'not_connected']);
 $con=$DB->Con(); if(!($con instanceof mysqli))pr_phone_json(['ok'=>false,'reason'=>'database_unavailable']);
 $safe=mysqli_real_escape_string($con,$username);$r=mysqli_query($con,"SELECT id,username FROM users WHERE username='{$safe}' LIMIT 1");$user=$r?(mysqli_fetch_assoc($r)?:null):null;if($r)mysqli_free_result($r);if(!$user)pr_phone_json(['ok'=>false,'reason'=>'user_not_found']);
 foreach(['rp_phones','rp_phone_contacts','rp_phone_messages','rp_phone_calls'] as $t)if(!pr_phone_table_exists($con,$t))pr_phone_json(['ok'=>false,'reason'=>'phone_migration_required']);
 $snapshot=pr_phone_snapshot($con,(int)$user['id']);
 pr_phone_json(['ok'=>true,'user_id'=>(int)$user['id'],'username'=>(string)$user['username'],'phone'=>$snapshot,'server_time'=>date(DATE_ATOM)]);
}catch(Throwable $e){error_log('[ParadisePhone:data] '.$e->getMessage());pr_phone_json(['ok'=>false,'reason'=>'phone_unavailable']);}

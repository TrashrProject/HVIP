<?php
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

function out(array $d,int $s=200):void{http_response_code($s);echo json_encode($d,JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);exit;}
function clean_name($v):string{return mb_substr(trim(preg_replace('/\s+/u',' ',strip_tags((string)$v))),0,64);}
function clean_body($v):string{return trim(strip_tags((string)$v));}
function phone_notification(mysqli $con,int $phoneId,string $type,string $title,string $body,array $meta=[]):void{
    $stmt=mysqli_prepare($con,'INSERT INTO rp_phone_notifications (phone_id,notification_type,title,body,metadata) VALUES (?,?,?,?,?)');
    if(!$stmt)return;$metadata=$meta?json_encode($meta,JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES):null;mysqli_stmt_bind_param($stmt,'issss',$phoneId,$type,$title,$body,$metadata);@mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);
}

if(($_SERVER['REQUEST_METHOD']??'GET')!=='POST')out(['ok'=>false,'reason'=>'method_not_allowed'],405);
if(($_SERVER['HTTP_X_PARADISE_ACTION']??'')!=='phase4')out(['ok'=>false,'reason'=>'missing_action_header'],403);
if(!empty($_SERVER['HTTP_ORIGIN'])){
    $originHost=strtolower((string)parse_url($_SERVER['HTTP_ORIGIN'],PHP_URL_HOST));
    $requestHost=strtolower(preg_replace('/:\d+$/','',(string)($_SERVER['HTTP_HOST']??'')));
    if($originHost!==''&&$requestHost!==''&&$originHost!==$requestHost)out(['ok'=>false,'reason'=>'origin_rejected'],403);
}

try{
 require_once __DIR__.'/app/init.pz.php'; require_once __DIR__.'/paradise-phone-lib.php';
 if(!isset($Session,$DB)||!class_exists('Config'))out(['ok'=>false,'reason'=>'bootstrap_unavailable'],503);
 $username=trim((string)$Session->Read(Config::$SessionName)); if($username==='')out(['ok'=>false,'reason'=>'not_connected'],401);
 $con=$DB->Con(); if(!($con instanceof mysqli))out(['ok'=>false,'reason'=>'database_unavailable'],503);
 if(!mysqli_set_charset($con,'utf8mb4'))out(['ok'=>false,'reason'=>'database_charset_unavailable'],503);
 $safe=mysqli_real_escape_string($con,$username);$r=mysqli_query($con,"SELECT id,username FROM users WHERE username='{$safe}' LIMIT 1");$user=$r?(mysqli_fetch_assoc($r)?:null):null;if($r)mysqli_free_result($r);if(!$user)out(['ok'=>false,'reason'=>'user_not_found'],404);
 $userId=(int)$user['id'];
 $raw=file_get_contents('php://input');if($raw===false||strlen($raw)>12000)out(['ok'=>false,'reason'=>'invalid_payload'],400);
 $input=json_decode($raw!==''?$raw:'{}',true);if(!is_array($input))out(['ok'=>false,'reason'=>'invalid_json'],400);
 $action=strtolower(trim((string)($input['action']??'')));
 $phone=pr_phone_ensure($con,$userId); if(!$phone)out(['ok'=>false,'reason'=>'no_phone_item','message'=>'Vous ne possédez pas de téléphone.'],403);$pid=(int)$phone['id'];

 if($action==='add_contact'){
   $number=trim((string)($input['number']??''));$name=clean_name($input['name']??'');
   if($name===''||!preg_match('/^555-\d{4}$/',$number))out(['ok'=>false,'reason'=>'invalid_contact','message'=>'Nom ou numéro invalide.'],422);
   if($number===$phone['phone_number'])out(['ok'=>false,'reason'=>'self_contact','message'=>'Vous ne pouvez pas vous ajouter vous-même.'],422);
   if(!pr_phone_row_by_number($con,$number))out(['ok'=>false,'reason'=>'unknown_number','message'=>'Ce numéro est indisponible.'],404);
   $stmt=mysqli_prepare($con,'INSERT INTO rp_phone_contacts (phone_id,contact_phone_number,display_name) VALUES (?,?,?) ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),updated_at=CURRENT_TIMESTAMP');mysqli_stmt_bind_param($stmt,'iss',$pid,$number,$name);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);
   out(['ok'=>true,'message'=>$name.' a été ajouté à vos contacts.','phone'=>pr_phone_snapshot($con,$userId)]);
 }

 if($action==='delete_contact'){
   $id=(int)($input['contact_id']??0);$stmt=mysqli_prepare($con,'DELETE FROM rp_phone_contacts WHERE id=? AND phone_id=? LIMIT 1');mysqli_stmt_bind_param($stmt,'ii',$id,$pid);mysqli_stmt_execute($stmt);$ok=mysqli_stmt_affected_rows($stmt)>0;mysqli_stmt_close($stmt);
   out(['ok'=>$ok,'reason'=>$ok?null:'contact_not_found','message'=>$ok?'Contact supprimé.':null,'phone'=>pr_phone_snapshot($con,$userId)],$ok?200:404);
 }

 if($action==='send_message'){
   $targetToken=trim((string)($input['target']??''));$body=clean_body($input['body']??'');
   if($body===''||mb_strlen($body)>500)out(['ok'=>false,'reason'=>'invalid_message','message'=>'Le message doit contenir entre 1 et 500 caractères.'],422);
   if(pr_phone_rate_limited($con,$pid,'SMS',10,8))out(['ok'=>false,'reason'=>'rate_limited','message'=>'Vous envoyez des messages trop rapidement.'],429);
   $target=pr_phone_resolve($con,$phone,$targetToken);if(!$target)out(['ok'=>false,'reason'=>'target_not_found','message'=>'Ce numéro/contact est indisponible.'],404);$tid=(int)$target['id'];
   if($tid===$pid)out(['ok'=>false,'reason'=>'self_message','message'=>'Vous ne pouvez pas vous envoyer un SMS à vous-même.'],422);
   $receiverUserId=(int)$target['user_id'];$senderIdentity=pr_phone_identity($con,$userId);$targetIdentity=pr_phone_identity($con,$receiverUserId);
   $senderName=$senderIdentity['name']?:$username;$receiverName=$targetIdentity['name']?:($targetIdentity['username']?:$target['phone_number']);
   $stmt=mysqli_prepare($con,"INSERT INTO play_phone_chats (type,emisor_id,emisor_name,receptor_id,receptor_name,msg,timestamp,status,read_at) VALUES (1,?,?,?,?,?,NOW(),'SENT',NULL)");
   if(!$stmt)out(['ok'=>false,'reason'=>'message_store_unavailable'],503);
   mysqli_stmt_bind_param($stmt,'isiss',$userId,$senderName,$receiverUserId,$receiverName,$body);mysqli_stmt_execute($stmt);$messageId=(int)mysqli_insert_id($con);mysqli_stmt_close($stmt);
   pr_phone_log_action($con,$pid,'SMS',$tid);phone_notification($con,$tid,'MESSAGE','Nouveau message',$senderName.': '.mb_substr($body,0,90),['chat_id'=>$messageId,'sender_phone_id'=>$pid]);
   out(['ok'=>true,'message'=>'Message envoyé à '.$receiverName.'.','phone'=>pr_phone_snapshot($con,$userId)]);
 }

 if($action==='read_conversation'){
   $other=(int)($input['other_phone_id']??0);if($other<=0)out(['ok'=>false,'reason'=>'invalid_conversation'],422);
   if(!pr_phone_mark_read($con,$phone,$other))out(['ok'=>false,'reason'=>'conversation_not_found'],404);
   out(['ok'=>true,'messages'=>pr_phone_messages($con,$pid,$other,30),'phone'=>pr_phone_snapshot($con,$userId)]);
 }

 if($action==='conversation'){
   $other=(int)($input['other_phone_id']??0);if($other<=0||!pr_phone_row_by_id($con,$other))out(['ok'=>false,'reason'=>'conversation_not_found'],404);
   out(['ok'=>true,'messages'=>pr_phone_messages($con,$pid,$other,30)]);
 }

 if($action==='call'){
   if(pr_phone_rate_limited($con,$pid,'CALL',20,4))out(['ok'=>false,'reason'=>'rate_limited','message'=>'Trop de tentatives d’appel.'],429);
   $target=pr_phone_resolve($con,$phone,trim((string)($input['target']??'')));if(!$target)out(['ok'=>false,'reason'=>'target_not_found','message'=>'Ce correspondant est indisponible.'],404);$tid=(int)$target['id'];
   if($tid===$pid)out(['ok'=>false,'reason'=>'self_call','message'=>'Vous ne pouvez pas vous appeler vous-même.'],422);
   if(!pr_phone_is_online($con,(int)$target['user_id']))out(['ok'=>false,'reason'=>'offline','message'=>'Ce correspondant est actuellement indisponible.'],409);
   if(pr_phone_active_call($con,$pid)||pr_phone_active_call($con,$tid))out(['ok'=>false,'reason'=>'busy','message'=>'Ligne occupée.'],409);
   $stmt=mysqli_prepare($con,"INSERT INTO rp_phone_calls (caller_phone_id,receiver_phone_id,status) VALUES (?,?,'RINGING')");mysqli_stmt_bind_param($stmt,'ii',$pid,$tid);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);pr_phone_log_action($con,$pid,'CALL',$tid);
   $sender=pr_phone_identity($con,$userId);$targetName=pr_phone_identity($con,(int)$target['user_id']);phone_notification($con,$tid,'CALL','Appel entrant',($sender['name']?:$username).' vous appelle.',['caller_phone_id'=>$pid]);
   out(['ok'=>true,'message'=>'Vous appelez '.($targetName['name']?:$target['phone_number']).'...','phone'=>pr_phone_snapshot($con,$userId)]);
 }

 if(in_array($action,['answer','decline','hangup'],true)){
   $call=pr_phone_active_call($con,$pid);if(!$call)out(['ok'=>false,'reason'=>'no_active_call','message'=>'Aucun appel en cours.'],409);$callId=(int)$call['id'];$isReceiver=(int)$call['receiver_phone_id']===$pid;$status=(string)$call['status'];
   if($action==='answer'){
     if(!$isReceiver||$status!=='RINGING')out(['ok'=>false,'reason'=>'cannot_answer','message'=>'Cet appel ne peut pas être décroché.'],409);
     $stmt=mysqli_prepare($con,"UPDATE rp_phone_calls SET status='CONNECTED',answered_at=NOW() WHERE id=? AND status='RINGING'");mysqli_stmt_bind_param($stmt,'i',$callId);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);$msg='Appel connecté.';
   }elseif($action==='decline'){
     if(!$isReceiver||$status!=='RINGING')out(['ok'=>false,'reason'=>'cannot_decline','message'=>'Cet appel ne peut pas être refusé.'],409);
     $stmt=mysqli_prepare($con,"UPDATE rp_phone_calls SET status='DECLINED',ended_at=NOW() WHERE id=? AND status='RINGING'");mysqli_stmt_bind_param($stmt,'i',$callId);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);$msg='Appel refusé.';
   }else{
     $final=$status==='RINGING'?'CANCELLED':'COMPLETED';$stmt=mysqli_prepare($con,"UPDATE rp_phone_calls SET status=?,ended_at=NOW() WHERE id=? AND status IN ('RINGING','CONNECTED')");mysqli_stmt_bind_param($stmt,'si',$final,$callId);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);$msg='Appel terminé.';
   }
   $otherId=(int)$call['caller_phone_id']===$pid?(int)$call['receiver_phone_id']:(int)$call['caller_phone_id'];phone_notification($con,$otherId,'CALL','Téléphone',$msg,['call_id'=>$callId]);
   out(['ok'=>true,'message'=>$msg,'phone'=>pr_phone_snapshot($con,$userId)]);
 }

 if($action==='settings'){
   $silent=!empty($input['silent'])?1:0;$notif=array_key_exists('notifications',$input)?(!empty($input['notifications'])?1:0):(int)$phone['notifications_enabled'];$sounds=array_key_exists('sounds',$input)?(!empty($input['sounds'])?1:0):(int)$phone['sounds_enabled'];
   $stmt=mysqli_prepare($con,'UPDATE rp_phones SET silent_mode=?,notifications_enabled=?,sounds_enabled=? WHERE id=?');mysqli_stmt_bind_param($stmt,'iiii',$silent,$notif,$sounds,$pid);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);
   out(['ok'=>true,'phone'=>pr_phone_snapshot($con,$userId)]);
 }

 out(['ok'=>false,'reason'=>'unknown_action'],400);
}catch(Throwable $e){error_log('[ParadisePhone:action] '.$e->getMessage());out(['ok'=>false,'reason'=>'phone_action_failed'],500);}

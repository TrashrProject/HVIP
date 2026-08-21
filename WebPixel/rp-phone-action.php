<?php
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

function out(array $d,int $s=200):void{http_response_code($s);echo json_encode($d,JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);exit;}
function clean_name($v):string{return mb_substr(trim(preg_replace('/\s+/u',' ',strip_tags((string)$v))),0,64);}
function clean_body($v):string{return trim(strip_tags((string)$v));}
try{
 require_once __DIR__.'/app/init.pz.php'; require_once __DIR__.'/paradise-phone-lib.php';
 if(!isset($Session,$DB)||!class_exists('Config'))out(['ok'=>false,'reason'=>'bootstrap_unavailable'],503);
 $username=trim((string)$Session->Read(Config::$SessionName)); if($username==='')out(['ok'=>false,'reason'=>'not_connected'],401);
 $con=$DB->Con(); if(!($con instanceof mysqli))out(['ok'=>false,'reason'=>'database_unavailable'],503);
 $safe=mysqli_real_escape_string($con,$username);$r=mysqli_query($con,"SELECT id,username FROM users WHERE username='{$safe}' LIMIT 1");$user=$r?(mysqli_fetch_assoc($r)?:null):null;if($r)mysqli_free_result($r);if(!$user)out(['ok'=>false,'reason'=>'user_not_found'],404);
 $userId=(int)$user['id'];$input=json_decode(file_get_contents('php://input'),true);if(!is_array($input))$input=$_POST;$action=strtolower(trim((string)($input['action']??'')));
 $phone=pr_phone_ensure($con,$userId); if(!$phone)out(['ok'=>false,'reason'=>'no_phone_item','message'=>'Vous ne possédez pas de téléphone.'],403);$pid=(int)$phone['id'];

 if($action==='add_contact'){
   $number=trim((string)($input['number']??''));$name=clean_name($input['name']??'');if($name===''||!preg_match('/^555-\d{4}$/',$number))out(['ok'=>false,'reason'=>'invalid_contact'],422);
   if($number===$phone['phone_number'])out(['ok'=>false,'reason'=>'self_contact'],422);
   if(!pr_phone_row_by_number($con,$number))out(['ok'=>false,'reason'=>'unknown_number','message'=>'Ce numéro est indisponible.'],404);
   $stmt=mysqli_prepare($con,'INSERT INTO rp_phone_contacts (phone_id,contact_phone_number,display_name) VALUES (?,?,?) ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),updated_at=CURRENT_TIMESTAMP');mysqli_stmt_bind_param($stmt,'iss',$pid,$number,$name);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);out(['ok'=>true,'message'=>$name.' a été ajouté à vos contacts.','phone'=>pr_phone_snapshot($con,$userId)]);
 }
 if($action==='delete_contact'){
   $id=(int)($input['contact_id']??0);$stmt=mysqli_prepare($con,'DELETE FROM rp_phone_contacts WHERE id=? AND phone_id=? LIMIT 1');mysqli_stmt_bind_param($stmt,'ii',$id,$pid);mysqli_stmt_execute($stmt);$ok=mysqli_stmt_affected_rows($stmt)>0;mysqli_stmt_close($stmt);out(['ok'=>$ok,'reason'=>$ok?null:'contact_not_found','message'=>$ok?'Contact supprimé.':null,'phone'=>pr_phone_snapshot($con,$userId)],$ok?200:404);
 }
 if($action==='send_message'){
   $targetToken=trim((string)($input['target']??''));$body=clean_body($input['body']??'');if($body===''||mb_strlen($body)>500)out(['ok'=>false,'reason'=>'invalid_message','message'=>'Le message doit contenir entre 1 et 500 caractères.'],422);
   if(pr_phone_rate_limited($con,$pid,'SMS',10,8))out(['ok'=>false,'reason'=>'rate_limited','message'=>'Vous envoyez des messages trop rapidement.'],429);
   $target=pr_phone_resolve($con,$phone,$targetToken);if(!$target)out(['ok'=>false,'reason'=>'target_not_found','message'=>'Ce numéro/contact est indisponible.'],404);$tid=(int)$target['id'];if($tid===$pid)out(['ok'=>false,'reason'=>'self_message'],422);
   $stmt=mysqli_prepare($con,"INSERT INTO rp_phone_messages (sender_phone_id,receiver_phone_id,body,status) VALUES (?,?,?,'SENT')");mysqli_stmt_bind_param($stmt,'iis',$pid,$tid,$body);mysqli_stmt_execute($stmt);$messageId=mysqli_insert_id($con);mysqli_stmt_close($stmt);pr_phone_log_action($con,$pid,'SMS',$tid);
   $ident=pr_phone_identity($con,(int)$target['user_id']);$sender=pr_phone_identity($con,$userId);
   $title='Nouveau message';$senderName=$sender['name']?:$username;$preview=mb_substr($body,0,90);$stmt=mysqli_prepare($con,"INSERT INTO rp_phone_notifications (phone_id,notification_type,title,body,metadata) VALUES (?,'MESSAGE',?,?,?)");$meta=json_encode(['message_id'=>$messageId,'sender_phone_id'=>$pid],JSON_UNESCAPED_UNICODE);mysqli_stmt_bind_param($stmt,'isss',$tid,$title,$senderName.': '.$preview,$meta);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);
   out(['ok'=>true,'message'=>'Message envoyé à '.($ident['name']?:$target['phone_number']).'.','phone'=>pr_phone_snapshot($con,$userId)]);
 }
 if($action==='read_conversation'){
   $other=(int)($input['other_phone_id']??0);if($other<=0)out(['ok'=>false,'reason'=>'invalid_conversation'],422);$stmt=mysqli_prepare($con,'UPDATE rp_phone_messages SET read_at=COALESCE(read_at,NOW()),status=\'READ\' WHERE receiver_phone_id=? AND sender_phone_id=? AND read_at IS NULL');mysqli_stmt_bind_param($stmt,'ii',$pid,$other);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);out(['ok'=>true,'messages'=>pr_phone_messages($con,$pid,$other,30),'phone'=>pr_phone_snapshot($con,$userId)]);
 }
 if($action==='conversation'){
   $other=(int)($input['other_phone_id']??0);$target=null;if($other>0){$stmt=mysqli_prepare($con,'SELECT id FROM rp_phones WHERE id=? AND status=\'ACTIVE\' LIMIT 1');mysqli_stmt_bind_param($stmt,'i',$other);mysqli_stmt_execute($stmt);$rr=mysqli_stmt_get_result($stmt);$target=$rr?mysqli_fetch_assoc($rr):null;if($rr)mysqli_free_result($rr);mysqli_stmt_close($stmt);}if(!$target)out(['ok'=>false,'reason'=>'conversation_not_found'],404);out(['ok'=>true,'messages'=>pr_phone_messages($con,$pid,$other,30)]);
 }
 if($action==='call'){
   if(pr_phone_rate_limited($con,$pid,'CALL',20,4))out(['ok'=>false,'reason'=>'rate_limited','message'=>'Trop de tentatives d’appel.'],429);$target=pr_phone_resolve($con,$phone,trim((string)($input['target']??'')));if(!$target)out(['ok'=>false,'reason'=>'target_not_found','message'=>'Ce correspondant est indisponible.'],404);$tid=(int)$target['id'];if($tid===$pid)out(['ok'=>false,'reason'=>'self_call','message'=>'Vous ne pouvez pas vous appeler vous-même.'],422);if(!pr_phone_is_online($con,(int)$target['user_id']))out(['ok'=>false,'reason'=>'offline','message'=>'Ce correspondant est actuellement indisponible.'],409);if(pr_phone_active_call($con,$pid)||pr_phone_active_call($con,$tid))out(['ok'=>false,'reason'=>'busy','message'=>'Ligne occupée.'],409);
   $stmt=mysqli_prepare($con,"INSERT INTO rp_phone_calls (caller_phone_id,receiver_phone_id,status) VALUES (?,?,'RINGING')");mysqli_stmt_bind_param($stmt,'ii',$pid,$tid);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);pr_phone_log_action($con,$pid,'CALL',$tid);$sender=pr_phone_identity($con,$userId);$stmt=mysqli_prepare($con,"INSERT INTO rp_phone_notifications (phone_id,notification_type,title,body) VALUES (?,'CALL','Appel entrant',?)");$callBody=($sender['name']?:$username).' vous appelle.';mysqli_stmt_bind_param($stmt,'is',$tid,$callBody);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);out(['ok'=>true,'message'=>'Vous appelez '.(pr_phone_identity($con,(int)$target['user_id'])['name']?:$target['phone_number']).'...','phone'=>pr_phone_snapshot($con,$userId)]);
 }
 if(in_array($action,['answer','decline','hangup'],true)){
   $call=pr_phone_active_call($con,$pid);if(!$call)out(['ok'=>false,'reason'=>'no_active_call','message'=>'Aucun appel en cours.'],409);$callId=(int)$call['id'];$isReceiver=(int)$call['receiver_phone_id']===$pid;$status=(string)$call['status'];
   if($action==='answer'){if(!$isReceiver||$status!=='RINGING')out(['ok'=>false,'reason'=>'cannot_answer'],409);$stmt=mysqli_prepare($con,"UPDATE rp_phone_calls SET status='CONNECTED',answered_at=NOW() WHERE id=? AND status='RINGING'");mysqli_stmt_bind_param($stmt,'i',$callId);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);$msg='Appel connecté.';}
   elseif($action==='decline'){if(!$isReceiver||$status!=='RINGING')out(['ok'=>false,'reason'=>'cannot_decline'],409);$stmt=mysqli_prepare($con,"UPDATE rp_phone_calls SET status='DECLINED',ended_at=NOW() WHERE id=? AND status='RINGING'");mysqli_stmt_bind_param($stmt,'i',$callId);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);$msg='Appel refusé.';}
   else{$final=$status==='RINGING'?'CANCELLED':'COMPLETED';$stmt=mysqli_prepare($con,'UPDATE rp_phone_calls SET status=?,ended_at=NOW() WHERE id=? AND status IN (\'RINGING\',\'CONNECTED\')');mysqli_stmt_bind_param($stmt,'si',$final,$callId);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);$msg='Appel terminé.';}
   out(['ok'=>true,'message'=>$msg,'phone'=>pr_phone_snapshot($con,$userId)]);
 }
 if($action==='settings'){
   $silent=!empty($input['silent'])?1:0;$notif=array_key_exists('notifications',$input)?(!empty($input['notifications'])?1:0):(int)$phone['notifications_enabled'];$sounds=array_key_exists('sounds',$input)?(!empty($input['sounds'])?1:0):(int)$phone['sounds_enabled'];$stmt=mysqli_prepare($con,'UPDATE rp_phones SET silent_mode=?,notifications_enabled=?,sounds_enabled=? WHERE id=?');mysqli_stmt_bind_param($stmt,'iiii',$silent,$notif,$sounds,$pid);mysqli_stmt_execute($stmt);mysqli_stmt_close($stmt);out(['ok'=>true,'phone'=>pr_phone_snapshot($con,$userId)]);
 }
 out(['ok'=>false,'reason'=>'unknown_action'],400);
}catch(Throwable $e){error_log('[ParadisePhone:action] '.$e->getMessage());out(['ok'=>false,'reason'=>'phone_action_failed'],500);}

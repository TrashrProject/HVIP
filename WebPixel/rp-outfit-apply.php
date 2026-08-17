<?php
require_once __DIR__ . '/app/init.pz.php';
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
function fail_json($code,$message){http_response_code($code);echo json_encode(['ok'=>false,'error'=>$message],JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);exit;}
function clean_figure($value){$value=trim((string)$value);if($value===''||stripos($value,'undefined')!==false)return '';return preg_match('/^[a-z0-9.\-]+$/i',$value)?$value:'';}
if(!$Session->Exist(Config::$SessionName)||!isset($UData['id']))fail_json(401,'Session expirée');
$data=json_decode(file_get_contents('php://input')?:'{}',true);
$outfitId=isset($data['id'])?(string)$data['id']:'';
$uid=(int)$UData['id'];$gender=strtoupper((string)($UData['gender']??'M'))==='F'?'F':'M';$userRank=(int)($UData['rank']??1);
if(!preg_match('/^job-(\d+)-rank-(\d+)$/',$outfitId,$m))fail_json(400,'Tenue invalide');
$jobId=(int)$m[1];$rank=(int)$m[2];if($jobId<1||$rank<1)fail_json(400,'Tenue invalide');
$isManager=$userRank>=6;
$staffQuery=$DB->Query("SELECT g.name FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE gm.user_id='".$uid."'");
if($staffQuery){while($s=mysqli_fetch_assoc($staffQuery)){if(preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i',trim((string)$s['name']))){$isManager=true;break;}}}
if(!$isManager){
 $membership=$DB->Query("SELECT gm.rank AS member_rank,g.name FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE gm.user_id='".$uid."' AND gm.group_id='".$jobId."' LIMIT 1");
 if(!$membership||mysqli_num_rows($membership)===0)fail_json(403,'Tu ne fais pas partie de ce métier');
 $member=mysqli_fetch_assoc($membership);$groupName=trim((string)$member['name']);
 if(preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i',$groupName))fail_json(403,'Les groupes staff n’ont pas de tenues RP');
 if($rank>(int)$member['member_rank'])fail_json(403,'Cette tenue dépasse ton grade');
}
$rows=$DB->Query("SELECT pjr.name,pjr.male_figure,pjr.female_figure,g.name AS job_name FROM play_jobs_ranks pjr INNER JOIN groups g ON g.id=pjr.job WHERE pjr.job='".$jobId."' AND pjr.rank='".$rank."' LIMIT 1");
if(!$rows||mysqli_num_rows($rows)===0)fail_json(404,'Tenue métier introuvable');
$row=mysqli_fetch_assoc($rows);$jobName=trim((string)$row['job_name']);
if(preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i',$jobName))fail_json(403,'Les groupes staff n’ont pas de tenues RP');
$figure=clean_figure($gender==='F'?$row['female_figure']:$row['male_figure']);$name=(string)$row['name'];
if($figure==='')fail_json(422,'Cette tenue métier n’est pas configurée pour ton genre');
$result=$DB->Query("UPDATE users SET look='".$figure."', gender='".$gender."' WHERE id='".$uid."' LIMIT 1");
if($result===false)fail_json(500,'Impossible de sauvegarder la tenue');
echo json_encode(['ok'=>true,'id'=>$outfitId,'name'=>$name,'job'=>$jobName,'figure'=>$figure,'gender'=>$gender,'manager_override'=>$isManager,'reload'=>true],JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);

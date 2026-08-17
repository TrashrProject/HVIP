<?php
require_once __DIR__ . '/app/init.pz.php';
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

function fail_json($code,$message){http_response_code($code);echo json_encode(['ok'=>false,'error'=>$message],JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);exit;}
function clean_figure($value){$value=trim((string)$value);if($value===''||stripos($value,'undefined')!==false)return '';return preg_match('/^[a-z0-9.\-]+$/i',$value)?$value:'';}
function is_staff_name($name){return (bool)preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i',(string)$name);}
function get_figure_part($look,$type){if(preg_match('/(?:^|\.)'.preg_quote($type,'/').'-\d+(?:-\d+)*/i',(string)$look,$m))return ltrim($m[0],'.');return '';}
function set_figure_part($look,$type,$part){if($part==='')return $look;$pattern='/(^|\.)'.preg_quote($type,'/').'-\d+(?:-\d+)*/i';if(preg_match($pattern,$look)){return preg_replace_callback($pattern,function($m)use($part){return ($m[1]==='.'?'.':'').$part;},$look,1);}return $look===''?$part:($look.'.'.$part);}

if(!$Session->Exist(Config::$SessionName)||!isset($UData['id']))fail_json(401,'Session expirée');

$data=json_decode(file_get_contents('php://input')?:'{}',true);
$outfitId=isset($data['id'])?(string)$data['id']:'';
$uid=(int)$UData['id'];
$gender=strtoupper((string)($UData['gender']??'M'))==='F'?'F':'M';
$userRank=(int)($UData['rank']??1);
$currentLook=clean_figure($UData['look']??'');
$isManager=$userRank>=6;

$staffQuery=$DB->Query("SELECT g.name FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE gm.user_id='".$uid."'");
if($staffQuery){while($s=mysqli_fetch_assoc($staffQuery)){if(is_staff_name(trim((string)$s['name']))){$isManager=true;break;}}}

$jobId=0;$rank=0;$figure='';$name='Tenue RP';$jobName='';

if(preg_match('/^job-(\d+)-rank-(\d+)$/',$outfitId,$m)){
    $jobId=(int)$m[1];$rank=(int)$m[2];
    $rows=$DB->Query("SELECT r.name,r.male_figure,r.female_figure,g.name AS job_name FROM play_jobs_ranks r INNER JOIN groups g ON g.id=r.job WHERE r.job='".$jobId."' AND r.rank='".$rank."' LIMIT 1");
    if(!$rows||mysqli_num_rows($rows)===0)fail_json(404,'Tenue métier introuvable');
    $row=mysqli_fetch_assoc($rows);
    $jobName=trim((string)$row['job_name']);
    $name=(string)$row['name'].' — Standard';
    $figure=clean_figure($gender==='F'?$row['female_figure']:$row['male_figure']);
} elseif(preg_match('/^outfit-(\d+)$/',$outfitId,$m)) {
    $variantId=(int)$m[1];
    $rows=$DB->Query("SELECT o.job_id,o.rank_id,o.name,o.male_figure,o.female_figure,g.name AS job_name FROM play_jobs_outfits o INNER JOIN groups g ON g.id=o.job_id WHERE o.id='".$variantId."' AND o.enabled=1 LIMIT 1");
    if(!$rows||mysqli_num_rows($rows)===0)fail_json(404,'Variante de tenue introuvable');
    $row=mysqli_fetch_assoc($rows);
    $jobId=(int)$row['job_id'];$rank=(int)$row['rank_id'];
    $jobName=trim((string)$row['job_name']);
    $name=(string)$row['name'];
    $figure=clean_figure($gender==='F'?$row['female_figure']:$row['male_figure']);
} else {
    fail_json(400,'Tenue invalide');
}

if($jobId<1||$rank<1)fail_json(400,'Tenue invalide');
if(is_staff_name($jobName))fail_json(403,'Les groupes staff n’ont pas de tenues RP');

if(!$isManager){
    $membership=$DB->Query("SELECT gm.rank AS member_rank FROM group_memberships gm WHERE gm.user_id='".$uid."' AND gm.group_id='".$jobId."' LIMIT 1");
    if(!$membership||mysqli_num_rows($membership)===0)fail_json(403,'Tu ne fais pas partie de ce métier');
    $member=mysqli_fetch_assoc($membership);
    if($rank>(int)$member['member_rank'])fail_json(403,'Cette tenue dépasse ton grade');
}

if($figure==='')fail_json(422,'Cette tenue n’est pas configurée pour ton genre');

// Conserve l'identité visuelle du joueur : coupe/couleur de cheveux + tête/teint.
// La tenue RP remplace les vêtements/accessoires, pas le visage choisi dans l'éditeur.
$hairPart=get_figure_part($currentLook,'hr');
$headPart=get_figure_part($currentLook,'hd');
if($hairPart!=='')$figure=set_figure_part($figure,'hr',$hairPart);
if($headPart!=='')$figure=set_figure_part($figure,'hd',$headPart);
$figure=clean_figure($figure);
if($figure==='')fail_json(422,'Look final invalide');

$result=$DB->Query("UPDATE users SET look='".$figure."', gender='".$gender."' WHERE id='".$uid."' LIMIT 1");
if($result===false)fail_json(500,'Impossible de sauvegarder la tenue');

echo json_encode([
    'ok'=>true,'id'=>$outfitId,'name'=>$name,'job'=>$jobName,'rank'=>$rank,
    'figure'=>$figure,'gender'=>$gender,'manager_override'=>$isManager,'preserved_identity'=>true,'reload'=>true
],JSON_UNESCAPED_UNICODE|JSON_UNESCAPED_SLASHES);

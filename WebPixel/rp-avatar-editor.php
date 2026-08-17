<?php
require_once __DIR__ . '/app/init.pz.php';
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

function out_json($data, $code = 200) { http_response_code($code); echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES); exit; }
function clean_look($value) { $value=trim((string)$value); if($value===''||stripos($value,'undefined')!==false)return ''; return preg_match('/^[a-z0-9.\-]+$/i',$value)?$value:''; }
function get_part($look,$type){ if(preg_match('/(?:^|\.)'.preg_quote($type,'/').'-(\d+)(?:-([0-9]+))?/i',$look,$m)) return ['set'=>(int)$m[1],'color'=>isset($m[2])?(int)$m[2]:0]; return ['set'=>0,'color'=>0]; }
function replace_part($look,$type,$setId,$colorId){ $part=$type.'-'.(int)$setId.((int)$colorId>0?'-'.(int)$colorId:''); $pattern='/(^|\.)'.preg_quote($type,'/').'-\d+(?:-\d+)*/i'; if(preg_match($pattern,$look)) return preg_replace_callback($pattern,function($m)use($part){return ($m[1]==='.'?'.':'').$part;},$look,1); return $look===''?$part:($look.'.'.$part); }
function remove_part($look,$type){ $parts=array_values(array_filter(explode('.',(string)$look),function($part)use($type){return !preg_match('/^'.preg_quote($type,'/').'-/i',$part);})); return implode('.',$parts); }
function find_set_groups_recursive($node,&$groups){ if(!is_array($node))return; if(isset($node['type'])&&isset($node['sets'])&&is_array($node['sets'])){$type=strtolower((string)$node['type']);if($type!==''&&!isset($groups[$type]))$groups[$type]=$node;} foreach($node as $child)if(is_array($child))find_set_groups_recursive($child,$groups); }
function find_palettes_recursive($node,&$palettes){ if(!is_array($node))return; if(isset($node['id'])&&isset($node['colors'])&&is_array($node['colors'])){$id=(int)$node['id'];if($id>0&&!isset($palettes[$id]))$palettes[$id]=$node['colors'];} foreach($node as $child)if(is_array($child))find_palettes_recursive($child,$palettes); }
function flatten_sets($sets){$out=[];foreach($sets as $k=>$set){if(!is_array($set))continue;if(!isset($set['id'])&&is_numeric($k))$set['id']=(int)$k;$out[]=$set;}return $out;}
function flatten_parts($parts){$out=[];if(!is_array($parts))return $out;foreach($parts as $part)if(is_array($part))$out[]=$part;return $out;}
function is_pure_hair_set($set){$parts=flatten_parts($set['parts']??[]);if(!$parts)return true;foreach($parts as $part){$type=strtolower(trim((string)($part['type']??'hr')));if($type!==''&&$type!=='hr')return false;}return true;}
function is_realistic_skin_hex($hex){ if(!preg_match('/^[0-9A-F]{6}$/i',$hex))return false; $r=hexdec(substr($hex,0,2));$g=hexdec(substr($hex,2,2));$b=hexdec(substr($hex,4,2));$max=max($r,$g,$b);$min=min($r,$g,$b);$delta=$max-$min;$brightness=($max+$min)/2; if($brightness<35||$brightness>248)return false;if($b>$r+18)return false;if($g>$r+12)return false;if($delta>150)return false;if($r>=$g-8&&$g>=$b-16)return true;if($r>=90&&abs($r-$g)<=55&&abs($g-$b)<=55)return true;return false; }

if(!$Session->Exist(Config::$SessionName)||!isset($UData['id']))out_json(['ok'=>false,'error'=>'Session expirée'],401);
$uid=(int)$UData['id'];$gender=strtoupper((string)($UData['gender']??'M'))==='F'?'F':'M';
$figureFile=dirname(__DIR__).DIRECTORY_SEPARATOR.'swf_pz'.DIRECTORY_SEPARATOR.'V5-0-2'.DIRECTORY_SEPARATOR.'gamedata'.DIRECTORY_SEPARATOR.'json'.DIRECTORY_SEPARATOR.'FigureData.json';
if(!is_file($figureFile))out_json(['ok'=>false,'error'=>'FigureData.json introuvable'],503);
$fig=json_decode(file_get_contents($figureFile),true);if(!is_array($fig))out_json(['ok'=>false,'error'=>'FigureData.json invalide'],503);
$groups=[];find_set_groups_recursive($fig,$groups);$paletteMap=[];find_palettes_recursive($fig,$paletteMap);
$hairGroup=$groups['hr']??null;$headGroup=$groups['hd']??null;if(!$hairGroup||!$headGroup)out_json(['ok'=>false,'error'=>'Données avatar incomplètes'],503);
$currentLook=clean_look($UData['look']??'');$currentHair=get_part($currentLook,'hr');$currentHead=get_part($currentLook,'hd');

$validHairSets=[];$hairOptions=[];$rejectedAccessorySets=0;
foreach(flatten_sets($hairGroup['sets']??[]) as $set){
    $id=(int)($set['id']??0);if($id<=0)continue;if(array_key_exists('selectable',$set)&&!$set['selectable'])continue;
    $g=strtoupper((string)($set['gender']??'U'));if($g!=='U'&&$g!==$gender)continue;
    if(!is_pure_hair_set($set)){$rejectedAccessorySets++;continue;}
    $validHairSets[$id]=true;
    $previewLook=$currentLook;foreach(['ha','he','ea','fa'] as $accessoryType)$previewLook=remove_part($previewLook,$accessoryType);
    $previewColor=$currentHair['color']>0?$currentHair['color']:40;$previewLook=replace_part($previewLook,'hr',$id,$previewColor);
    $hairOptions[]=['id'=>$id,'gender'=>$g,'preview_figure'=>$previewLook];
}
usort($hairOptions,function($a,$b){return $a['id']<=>$b['id'];});

$hairPaletteId=(int)($hairGroup['paletteid']??$hairGroup['paletteId']??2);$skinPaletteId=(int)($headGroup['paletteid']??$headGroup['paletteId']??1);
$hairColors=[];$validHairColors=[];foreach(($paletteMap[$hairPaletteId]??[]) as $color){if(!is_array($color))continue;if(array_key_exists('selectable',$color)&&!$color['selectable'])continue;$id=(int)($color['id']??0);if($id<=0)continue;$hex=strtoupper(trim((string)($color['hexCode']??$color['hexcode']??'')));if(!preg_match('/^[0-9A-F]{6}$/',$hex))continue;$validHairColors[$id]=true;$hairColors[]=['id'=>$id,'hex'=>$hex];if(count($hairColors)>=64)break;}
$skinColors=[];foreach(($paletteMap[$skinPaletteId]??[]) as $color){if(!is_array($color))continue;if(array_key_exists('selectable',$color)&&!$color['selectable'])continue;$id=(int)($color['id']??0);$index=(int)($color['index']??0);if($id<=0)continue;$hex=strtoupper(trim((string)($color['hexCode']??$color['hexcode']??'')));if(!is_realistic_skin_hex($hex))continue;$skinColors[]=['id'=>$id,'hex'=>$hex,'index'=>$index];}
usort($skinColors,function($a,$b){$la=hexdec(substr($a['hex'],0,2))+hexdec(substr($a['hex'],2,2))+hexdec(substr($a['hex'],4,2));$lb=hexdec(substr($b['hex'],0,2))+hexdec(substr($b['hex'],2,2))+hexdec(substr($b['hex'],4,2));return $lb<=>$la;});$seen=[];$compact=[];foreach($skinColors as $c){if(isset($seen[$c['hex']]))continue;$seen[$c['hex']]=true;$compact[]=$c;if(count($compact)>=40)break;}$skinColors=$compact;$validSkinColors=[];foreach($skinColors as $c)$validSkinColors[(int)$c['id']]=true;

if($_SERVER['REQUEST_METHOD']==='GET')out_json(['ok'=>true,'gender'=>$gender,'look'=>$currentLook,'current'=>['hair_set'=>$currentHair['set'],'hair_color'=>$currentHair['color'],'skin_color'=>$currentHead['color']],'hair_sets'=>$hairOptions,'hair_colors'=>$hairColors,'skin_colors'=>$skinColors,'debug'=>['hair_group_sets'=>count($hairOptions),'hair_accessory_sets_rejected'=>$rejectedAccessorySets,'hair_palette'=>$hairPaletteId,'skin_palette'=>$skinPaletteId,'skin_tones'=>count($skinColors)]]);
if($_SERVER['REQUEST_METHOD']!=='POST')out_json(['ok'=>false,'error'=>'Méthode invalide'],405);
$data=json_decode(file_get_contents('php://input')?:'{}',true);$hairSet=isset($data['hair_set'])?(int)$data['hair_set']:$currentHair['set'];$hairColor=isset($data['hair_color'])?(int)$data['hair_color']:$currentHair['color'];$skinColor=isset($data['skin_color'])?(int)$data['skin_color']:$currentHead['color'];
if($hairSet<=0||!isset($validHairSets[$hairSet]))out_json(['ok'=>false,'error'=>'Cette coupe contient un accessoire ou n’est pas une coupe pure'],422);if($hairColor<=0||!isset($validHairColors[$hairColor]))out_json(['ok'=>false,'error'=>'Couleur de cheveux invalide'],422);if($skinColor<=0||!isset($validSkinColors[$skinColor]))out_json(['ok'=>false,'error'=>'Teint de peau invalide'],422);
// Ne touche qu'aux parties hr et hd. Aucun accessoire de tête n'est ajouté ici.
$look=$currentLook;$look=replace_part($look,'hr',$hairSet,$hairColor);$headSet=$currentHead['set']>0?$currentHead['set']:180;$look=replace_part($look,'hd',$headSet,$skinColor);$look=clean_look($look);if($look==='')out_json(['ok'=>false,'error'=>'Look généré invalide'],422);
$result=$DB->Query("UPDATE users SET look='".$look."' WHERE id='".$uid."' LIMIT 1");if($result===false)out_json(['ok'=>false,'error'=>'Impossible de sauvegarder le look'],500);out_json(['ok'=>true,'look'=>$look,'hair_set'=>$hairSet,'hair_color'=>$hairColor,'skin_color'=>$skinColor,'reload'=>true]);

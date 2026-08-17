<?php
// Usage: C:\xampp\php\php.exe C:\HVIP\tools\enhance-all-jobs-outfits.php
// Recrée play_jobs_outfits avec une sélection plus intelligente et des variantes réellement orientées métier.

$root = dirname(__DIR__);
$catalogFile = $root . DIRECTORY_SEPARATOR . 'WebPixel' . DIRECTORY_SEPARATOR . 'nitro-last' . DIRECTORY_SEPARATOR . 'rp-outfits.json';
$db = @new mysqli('127.0.0.1','root','','hv_rp');
if ($db->connect_errno) { fwrite(STDERR,"MariaDB: {$db->connect_error}\n"); exit(1); }
$db->set_charset('utf8mb4');

$catalog=[];
if(is_file($catalogFile)){
    $j=json_decode(file_get_contents($catalogFile),true);
    if(is_array($j))$catalog=$j['outfits']??[];
}
function cleanFig($v){$v=trim((string)$v);if($v===''||stripos($v,'undefined')!==false)return '';return preg_match('/^[a-z0-9.\-]+$/i',$v)?$v:'';}
function txt($o){return strtolower(trim(($o['name']??'').' '.($o['category']??'').' '.($o['categoryLabel']??'').' '.($o['source']??'')));}
function scorePreset($o,$base,$variant){$t=txt($o);$s=0;foreach($base as $k)if($k!==''&&strpos($t,strtolower($k))!==false)$s+=3;foreach($variant as $k)if($k!==''&&strpos($t,strtolower($k))!==false)$s+=8;return $s;}
function collectCandidates($catalog,$gender,$base){$out=[];foreach($catalog as $o){$g=strtoupper((string)($o['gender']??'M'))==='F'?'F':'M';if($g!==$gender)continue;$f=cleanFig($o['figure']??'');if($f==='')continue;$s=scorePreset($o,$base,[]);if($s<=0)continue;$out[$f]=['figure'=>$f,'text'=>txt($o),'raw'=>$o];}return array_values($out);}
function pickBest($items,$base,$variant,$used){$best=null;$bestScore=-1;foreach($items as $it){if(isset($used[$it['figure']]))continue;$s=scorePreset($it['raw'],$base,$variant);if($s>$bestScore){$bestScore=$s;$best=$it;}}return $best;}

$P=[
1=>['Hospital',['medical','medic','doctor','hospital','nurse','ambulance','samu','surgeon','infirm'],[
['Consultation',['doctor','medical']],['Urgences',['emergency','ambulance','samu']],['Bloc opératoire',['surgeon','surgery']],['Réanimation',['medical','hospital']],['SAMU / Ambulance',['ambulance','samu']],['Garde de nuit',['nurse','medical']],['Chef de service',['doctor','formal','chief']],['Direction médicale',['formal','business','doctor']],['Chirurgie spécialisée',['surgeon','mask']],['Visite officielle',['formal','medical']]]],
2=>['Basurero',['worker','garbage','trash','clean','maintenance','utility','construction'],[['Collecte urbaine',['garbage','trash']],['Haute visibilité',['worker','construction']],['Collecte de nuit',['worker','dark']],['Maintenance',['maintenance','utility']],['Nettoyage spécial',['clean','cleaner']],['Chef d’équipe',['chief','worker']],['Inspection',['supervisor','worker']],['Direction service urbain',['formal','worker']]]],
3=>['Taller Mecánico',['mechanic','mecano','garage','worker','repair','engineer','maintenance'],[['Atelier standard',['mechanic','garage']],['Diagnostic',['engineer','repair']],['Réparation lourde',['mechanic','worker']],['Dépannage',['maintenance','worker']],['Essais véhicule',['mechanic','driver']],['Chef d’atelier',['chief','mechanic']],['Responsable technique',['engineer','formal']],['Direction garage',['formal','business']]]],
4=>['Fábrica de Armas',['military','army','security','tactical','factory','industrial','worker'],[['Production',['factory','worker']],['Contrôle qualité',['industrial','worker']],['Entrepôt',['worker','security']],['Sécurité interne',['security','guard']],['Zone tactique',['tactical','military']],['Superviseur',['chief','military']],['Responsable usine',['formal','industrial']],['Direction',['formal','military']]]],
6=>['Camioneros',['driver','truck','camion','transport','delivery','logistic','worker'],[['Route standard',['driver','truck']],['Long trajet',['driver','road']],['Livraison',['delivery','worker']],['Entrepôt',['logistic','worker']],['Convoi',['driver','security']],['Chef de convoi',['chief','driver']],['Responsable logistique',['logistic','formal']],['Direction transport',['formal','business']]]],
7=>['Guardaespaldas',['security','guard','bodyguard','garde','tactical','agent','suit','formal'],[['Protection rapprochée',['bodyguard','security']],['Service discret',['suit','formal']],['Intervention',['tactical','security']],['Escorte VIP',['bodyguard','formal']],['Protection armée',['military','guard']],['Chef sécurité',['chief','security']],['Coordinateur VIP',['agent','formal']],['Direction protection',['formal','business']]]],
8=>['Mineros',['miner','mine','worker','construction','industrial','helmet'],[['Extraction',['miner','mine']],['Galerie',['worker','helmet']],['Sécurité mine',['security','worker']],['Maintenance',['maintenance','industrial']],['Exploration',['construction','worker']],['Chef d’équipe',['chief','worker']],['Superviseur',['supervisor','industrial']],['Direction minière',['formal','business']]]],
9=>['Policía',['police','policia','swat','security','sheriff','officer','tactical','riot','fbi'],[['Patrouille',['police','officer']],['Patrouille nuit',['police','dark']],['Intervention',['riot','security']],['SWAT',['swat','tactical']],['Anti-émeute',['riot','helmet']],['Unité spéciale',['fbi','tactical']],['Cérémonie',['formal','police']],['Inspecteur',['detective','officer']],['Commandement',['chief','officer','formal']],['Haute direction',['formal','suit','police']]]],
10=>["McDonald's",['restaurant','fastfood','cook','chef','waiter','server','cashier','kitchen','uniform'],[['Comptoir',['server','uniform']],['Caisse',['cashier']],['Cuisine',['cook','kitchen']],['Rush',['fastfood','uniform']],['Livraison',['delivery','food']],['Supervision',['supervisor','uniform']],['Manager',['manager','formal']],['Direction',['formal','business']]]],
11=>['Gobierno Central',['government','gouvernement','justice','formal','suit','business','office','president','senator','diplomat'],[['Bureau officiel',['office','formal']],['Réunion',['business','formal']],['Cérémonie',['formal','prestige']],['Diplomatie',['diplomat','suit']],['Déplacement officiel',['formal','security']],['Protocole',['government','formal']],['Parlement',['senator','formal']],['Cabinet',['office','business']],['Prestige',['suit','prestige']],['Direction d’État',['president','formal']]]],
12=>['Gobierno Federal',['government','gouvernement','justice','formal','suit','business','office','president','senator','diplomat'],[['Bureau fédéral',['office','formal']],['Réunion fédérale',['business','formal']],['Cérémonie',['formal','prestige']],['Diplomatie',['diplomat','suit']],['Déplacement officiel',['formal','security']],['Protocole',['government','formal']],['Conseil fédéral',['senator','formal']],['Cabinet',['office','business']],['Prestige',['suit','prestige']],['Haute direction',['president','formal']]]],
13=>['Cafetería Bobba Ball',['restaurant','cafe','food','cook','chef','waiter','server','cashier','uniform'],[['Salle',['waiter','server']],['Caisse',['cashier']],['Cuisine',['cook','chef']],['Service premium',['formal','server']],['Terrasse',['cafe','server']],['Supervision',['supervisor']],['Manager',['manager','formal']],['Direction',['formal','business']]]],
14=>['Subway',['restaurant','food','fastfood','cook','waiter','server','cashier','uniform'],[['Comptoir',['server','uniform']],['Caisse',['cashier']],['Préparation',['cook','food']],['Rush',['fastfood','uniform']],['Livraison',['delivery','food']],['Supervision',['supervisor']],['Manager',['manager','formal']],['Direction',['formal','business']]]],
15=>['Heladería',['restaurant','ice','glace','food','waiter','server','cashier','uniform','shop'],[['Service glace',['ice','server']],['Caisse',['cashier']],['Préparation',['food','uniform']],['Terrasse',['server','shop']],['Service premium',['formal','server']],['Supervision',['supervisor']],['Manager',['manager','formal']],['Direction',['formal','business']]]],
16=>['Bubble Juice',['restaurant','drink','juice','bubble','food','waiter','server','cashier','uniform','shop'],[['Bar',['drink','server']],['Caisse',['cashier']],['Préparation boissons',['drink','food']],['Rush',['uniform','server']],['Service premium',['formal','server']],['Supervision',['supervisor']],['Manager',['manager','formal']],['Direction',['formal','business']]]]
];

$total=0;$jobs=0;
foreach($P as $jobId=>$p){[$label,$baseKw,$variants]=$p;$r=[];$q=$db->query('SELECT rank,name,male_figure,female_figure FROM play_jobs_ranks WHERE job='.(int)$jobId.' ORDER BY rank');if(!$q||!$q->num_rows){echo "[SKIP] {$label}\n";continue;}while($x=$q->fetch_assoc())$r[]=$x;
 $fallback=[];if($jobId===12){$fq=$db->query('SELECT rank,male_figure,female_figure FROM play_jobs_ranks WHERE job=11');if($fq)while($x=$fq->fetch_assoc())$fallback[(int)$x['rank']]=$x;}
 $mCand=collectCandidates($catalog,'M',$baseKw);$fCand=collectCandidates($catalog,'F',$baseKw);$db->query('DELETE FROM play_jobs_outfits WHERE job_id='.(int)$jobId);
 $st=$db->prepare('INSERT INTO play_jobs_outfits(job_id,rank_id,name,male_figure,female_figure,sort_order,enabled) VALUES(?,?,?,?,?,?,1)');$ins=0;
 foreach($r as $rr){$rank=(int)$rr['rank'];$rankName=trim((string)$rr['name']);$bm=cleanFig($rr['male_figure']);$bf=cleanFig($rr['female_figure']);if($jobId===12&&isset($fallback[$rank])){if($bm==='')$bm=cleanFig($fallback[$rank]['male_figure']);if($bf==='')$bf=cleanFig($fallback[$rank]['female_figure']);}
  $usedM=[];$usedF=[];foreach($variants as $i=>$v){[$vName,$vKw]=$v;$pm=pickBest($mCand,$baseKw,$vKw,$usedM);$pf=pickBest($fCand,$baseKw,$vKw,$usedF);$m=$pm['figure']??$bm;$f=$pf['figure']??$bf;if($m==='')$m=$bm;if($f==='')$f=$bf;if($m===''&&$f==='')continue;if($pm)$usedM[$m]=true;if($pf)$usedF[$f]=true;$name=$rankName.' — '.$vName;$sort=$rank*100+$i+1;$jid=(int)$jobId;$st->bind_param('iisssi',$jid,$rank,$name,$m,$f,$sort);if($st->execute())$ins++;}
 }
 $st->close();$total+=$ins;$jobs++;echo "[OK] Job {$jobId} {$label}: ".count($r)." grades, {$ins} tenues | candidats H:".count($mCand)." F:".count($fCand)."\n";
}
echo "\n=== Amélioration globale terminée ===\nMétiers: {$jobs}\nTenues: {$total}\n";
$db->close();

<?php
// Usage: C:\xampp\php\php.exe C:\HVIP\tools\seed-government-outfits.php
// Remplit play_jobs_outfits pour Gobierno Central (11) et Gobierno Federal (12).

$root = dirname(__DIR__);
$catalogFile = $root . DIRECTORY_SEPARATOR . 'WebPixel' . DIRECTORY_SEPARATOR . 'nitro-last' . DIRECTORY_SEPARATOR . 'rp-outfits.json';
if (!is_file($catalogFile)) { fwrite(STDERR, "rp-outfits.json introuvable: {$catalogFile}\n"); exit(1); }

$db = @new mysqli('127.0.0.1', 'root', '', 'hv_rp');
if ($db->connect_errno) { fwrite(STDERR, "Connexion MariaDB impossible: {$db->connect_error}\n"); exit(2); }
$db->set_charset('utf8mb4');

$catalog = json_decode(file_get_contents($catalogFile), true);
if (!is_array($catalog)) { fwrite(STDERR, "Catalogue RP JSON invalide.\n"); exit(3); }

function cleanFigure($value) {
    $value = trim((string)$value);
    if ($value === '' || stripos($value, 'undefined') !== false) return '';
    return preg_match('/^[a-z0-9.\-]+$/i', $value) ? $value : '';
}
function normText($o) {
    return strtolower(trim((string)($o['name'] ?? '') . ' ' . (string)($o['category'] ?? '') . ' ' . (string)($o['categoryLabel'] ?? '') . ' ' . (string)($o['source'] ?? '')));
}

$keywords = ['government','gouvernement','gobierno','justice','judicial','judge','juez','senator','senador','president','presidente','politic','politique','diplomat','diplomatie','formal','ceremony','ceremonie','office','bureau','suit','costume','luxury','luxe'];
$male=[]; $female=[];
foreach (($catalog['outfits'] ?? []) as $o) {
    $text = normText($o); $match=false;
    foreach ($keywords as $kw) if (strpos($text,$kw)!==false) { $match=true; break; }
    if (!$match) continue;
    $figure = cleanFigure($o['figure'] ?? ''); if ($figure==='') continue;
    $entry=['name'=>trim((string)($o['name'] ?? 'Tenue Gouvernement')),'figure'=>$figure];
    $gender = strtoupper((string)($o['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
    if ($gender==='F') $female[]=$entry; else $male[]=$entry;
}
$dedupe=function(array $items){$seen=[];$out=[];foreach($items as $it){if(isset($seen[$it['figure']]))continue;$seen[$it['figure']]=true;$out[]=$it;}return $out;};
$male=$dedupe($male); $female=$dedupe($female);

// Fallbacks issus de Gobierno Central par grade, utiles notamment pour job 12 dont les figures sont souvent vides.
$fallbackCentral=[];
$res=$db->query("SELECT rank,male_figure,female_figure FROM play_jobs_ranks WHERE job=11 ORDER BY rank");
if ($res) while($r=$res->fetch_assoc()) $fallbackCentral[(int)$r['rank']]=['M'=>cleanFigure($r['male_figure']),'F'=>cleanFigure($r['female_figure'])];

$variantNames=[
    'Tenue officielle',
    'Tenue réunion gouvernementale',
    'Tenue cérémonie',
    'Tenue diplomatique',
    'Tenue déplacement officiel',
    'Tenue protocole',
    'Tenue prestige',
    'Tenue direction'
];

$totalInserted=0;
foreach ([11=>'Gobierno Central',12=>'Gobierno Federal'] as $jobId=>$jobLabel) {
    $ranks=[];
    $res=$db->query("SELECT rank,name,male_figure,female_figure FROM play_jobs_ranks WHERE job={$jobId} ORDER BY rank ASC");
    if (!$res || !$res->num_rows) { echo "Aucun grade pour {$jobLabel} (job {$jobId}).\n"; continue; }
    while($row=$res->fetch_assoc()) $ranks[]=$row;

    $db->query("DELETE FROM play_jobs_outfits WHERE job_id={$jobId}");
    $stmt=$db->prepare("INSERT INTO play_jobs_outfits (job_id,rank_id,name,male_figure,female_figure,sort_order,enabled) VALUES (?, ?, ?, ?, ?, ?, 1)");
    if (!$stmt) { fwrite(STDERR,"Prepare SQL impossible: {$db->error}\n"); exit(4); }

    $mi=0; $fi=0; $inserted=0;
    foreach($ranks as $rankRow){
        $rank=(int)$rankRow['rank']; $rankName=trim((string)$rankRow['name']);
        $baseM=cleanFigure($rankRow['male_figure']); $baseF=cleanFigure($rankRow['female_figure']);
        if ($baseM==='' && isset($fallbackCentral[$rank]['M'])) $baseM=$fallbackCentral[$rank]['M'];
        if ($baseF==='' && isset($fallbackCentral[$rank]['F'])) $baseF=$fallbackCentral[$rank]['F'];

        for($i=0;$i<count($variantNames);$i++){
            $m = count($male) ? $male[$mi % count($male)]['figure'] : $baseM;
            $f = count($female) ? $female[$fi % count($female)]['figure'] : $baseF;
            if($m==='')$m=$baseM; if($f==='')$f=$baseF;
            if($m==='' && $f==='') continue;
            $name=$rankName.' — '.$variantNames[$i];
            $sort=($rank*100)+($i+1);
            $stmt->bind_param('iisssi',$jobId,$rank,$name,$m,$f,$sort);
            if($stmt->execute()){ $inserted++; $totalInserted++; }
            if(count($male))$mi++; if(count($female))$fi++;
        }
    }
    $stmt->close();
    echo "=== {$jobLabel} (job {$jobId}) ===\n";
    echo "Grades détectés : ".count($ranks)."\n";
    echo "Tenues ajoutées : {$inserted}\n";
    $check=$db->query("SELECT rank_id,COUNT(*) total FROM play_jobs_outfits WHERE job_id={$jobId} GROUP BY rank_id ORDER BY rank_id");
    if($check) while($r=$check->fetch_assoc()) echo " - Grade {$r['rank_id']} : {$r['total']} variantes\n";
    echo "\n";
}

echo "Presets gouvernement homme détectés : ".count($male)."\n";
echo "Presets gouvernement femme détectés : ".count($female)."\n";
echo "Total tenues gouvernement ajoutées : {$totalInserted}\n";
if(!$male && !$female) echo "ATTENTION: aucun preset gouvernemental détecté; utilisation des looks standards/fallbacks par grade.\n";
$db->close();

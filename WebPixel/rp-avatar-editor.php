<?php
require_once __DIR__ . '/app/init.pz.php';
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

function out_json($data, $code = 200) {
    http_response_code($code);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}
function clean_look($value) {
    $value = trim((string)$value);
    if ($value === '' || stripos($value, 'undefined') !== false) return '';
    return preg_match('/^[a-z0-9.\-]+$/i', $value) ? $value : '';
}
function get_part($look, $type) {
    if (preg_match('/(?:^|\.)' . preg_quote($type, '/') . '-(\d+)(?:-([0-9]+))?/i', $look, $m)) {
        return ['set' => (int)$m[1], 'color' => isset($m[2]) ? (int)$m[2] : 0];
    }
    return ['set' => 0, 'color' => 0];
}
function replace_part($look, $type, $setId, $colorId) {
    $part = $type . '-' . (int)$setId . ((int)$colorId > 0 ? '-' . (int)$colorId : '');
    $pattern = '/(^|\.)' . preg_quote($type, '/') . '-\d+(?:-\d+)*/i';
    if (preg_match($pattern, $look)) {
        return preg_replace_callback($pattern, function($m) use ($part) { return ($m[1] === '.' ? '.' : '') . $part; }, $look, 1);
    }
    return $look === '' ? $part : ($look . '.' . $part);
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) out_json(['ok'=>false,'error'=>'Session expirée'], 401);
$uid = (int)$UData['id'];
$gender = strtoupper((string)($UData['gender'] ?? 'M')) === 'F' ? 'F' : 'M';

$figureFile = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'swf_pz' . DIRECTORY_SEPARATOR . 'V5-0-2' . DIRECTORY_SEPARATOR . 'gamedata' . DIRECTORY_SEPARATOR . 'json' . DIRECTORY_SEPARATOR . 'FigureData.json';
if (!is_file($figureFile)) out_json(['ok'=>false,'error'=>'FigureData.json introuvable'], 503);
$fig = json_decode(file_get_contents($figureFile), true);
if (!is_array($fig)) out_json(['ok'=>false,'error'=>'FigureData.json invalide'], 503);

$groups = [];
foreach (($fig['sets'] ?? []) as $group) {
    if (!isset($group['type'])) continue;
    $groups[(string)$group['type']] = $group;
}
$paletteMap = [];
foreach (($fig['palettes'] ?? []) as $palette) {
    $paletteMap[(int)($palette['id'] ?? 0)] = $palette['colors'] ?? [];
}

$hairGroup = $groups['hr'] ?? null;
$headGroup = $groups['hd'] ?? null;
if (!$hairGroup || !$headGroup) out_json(['ok'=>false,'error'=>'Données avatar incomplètes'], 503);

$validHairSets = [];
$hairOptions = [];
foreach (($hairGroup['sets'] ?? []) as $set) {
    $id = (int)($set['id'] ?? 0);
    if ($id <= 0 || isset($set['selectable']) && !$set['selectable']) continue;
    $g = strtoupper((string)($set['gender'] ?? 'U'));
    if ($g !== 'U' && $g !== $gender) continue;
    $validHairSets[$id] = true;
    $hairOptions[] = ['id'=>$id, 'gender'=>$g];
}
usort($hairOptions, function($a,$b){ return $a['id'] <=> $b['id']; });
if (count($hairOptions) > 220) $hairOptions = array_slice($hairOptions, 0, 220);

$hairPaletteId = (int)($hairGroup['paletteid'] ?? $hairGroup['paletteId'] ?? 0);
$skinPaletteId = (int)($headGroup['paletteid'] ?? $headGroup['paletteId'] ?? 1);

$hairColors = [];
$validHairColors = [];
foreach (($paletteMap[$hairPaletteId] ?? []) as $color) {
    if (isset($color['selectable']) && !$color['selectable']) continue;
    $id = (int)($color['id'] ?? 0);
    if ($id <= 0) continue;
    $hex = strtoupper(trim((string)($color['hexCode'] ?? '')));
    if (!preg_match('/^[0-9A-F]{6}$/', $hex)) continue;
    $validHairColors[$id] = true;
    $hairColors[] = ['id'=>$id,'hex'=>$hex];
    if (count($hairColors) >= 48) break;
}

$skinColors = [];
$validSkinColors = [];
foreach (($paletteMap[$skinPaletteId] ?? []) as $color) {
    if (isset($color['selectable']) && !$color['selectable']) continue;
    $id = (int)($color['id'] ?? 0);
    $index = (int)($color['index'] ?? 0);
    if ($id <= 0 || $id > 2000) continue;
    $hex = strtoupper(trim((string)($color['hexCode'] ?? '')));
    if (!preg_match('/^[0-9A-F]{6}$/', $hex)) continue;
    // Garde les teintes de peau classiques/custom raisonnables et écarte les énormes palettes fantaisie.
    if ($index > 100) continue;
    $validSkinColors[$id] = true;
    $skinColors[] = ['id'=>$id,'hex'=>$hex,'index'=>$index];
}
usort($skinColors, function($a,$b){ return $a['index'] <=> $b['index']; });
if (count($skinColors) > 96) $skinColors = array_slice($skinColors, 0, 96);

$currentLook = clean_look($UData['look'] ?? '');
$currentHair = get_part($currentLook, 'hr');
$currentHead = get_part($currentLook, 'hd');

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    out_json([
        'ok'=>true,
        'gender'=>$gender,
        'look'=>$currentLook,
        'current'=>['hair_set'=>$currentHair['set'],'hair_color'=>$currentHair['color'],'skin_color'=>$currentHead['color']],
        'hair_sets'=>$hairOptions,
        'hair_colors'=>$hairColors,
        'skin_colors'=>$skinColors
    ]);
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') out_json(['ok'=>false,'error'=>'Méthode invalide'], 405);
$data = json_decode(file_get_contents('php://input') ?: '{}', true);
$hairSet = isset($data['hair_set']) ? (int)$data['hair_set'] : $currentHair['set'];
$hairColor = isset($data['hair_color']) ? (int)$data['hair_color'] : $currentHair['color'];
$skinColor = isset($data['skin_color']) ? (int)$data['skin_color'] : $currentHead['color'];

if ($hairSet <= 0 || !isset($validHairSets[$hairSet])) out_json(['ok'=>false,'error'=>'Coupe de cheveux invalide'], 422);
if ($hairColor <= 0 || !isset($validHairColors[$hairColor])) out_json(['ok'=>false,'error'=>'Couleur de cheveux invalide'], 422);
if ($skinColor <= 0 || !isset($validSkinColors[$skinColor])) out_json(['ok'=>false,'error'=>'Teint de peau invalide'], 422);

$look = $currentLook;
$look = replace_part($look, 'hr', $hairSet, $hairColor);
$headSet = $currentHead['set'] > 0 ? $currentHead['set'] : 180;
$look = replace_part($look, 'hd', $headSet, $skinColor);
$look = clean_look($look);
if ($look === '') out_json(['ok'=>false,'error'=>'Look généré invalide'], 422);

$result = $DB->Query("UPDATE users SET look='" . $look . "' WHERE id='" . $uid . "' LIMIT 1");
if ($result === false) out_json(['ok'=>false,'error'=>'Impossible de sauvegarder le look'], 500);
out_json(['ok'=>true,'look'=>$look,'hair_set'=>$hairSet,'hair_color'=>$hairColor,'skin_color'=>$skinColor,'reload'=>true]);

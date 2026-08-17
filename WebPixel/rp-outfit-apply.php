<?php
require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) {
    http_response_code(401);
    echo json_encode(['ok' => false, 'error' => 'Session expirée']);
    exit;
}

$raw = file_get_contents('php://input');
$data = json_decode($raw ?: '{}', true);
$outfitId = isset($data['id']) ? (string)$data['id'] : '';

if (!preg_match('/^rp-\d+$/', $outfitId)) {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'Tenue invalide']);
    exit;
}

$file = __DIR__ . '/nitro-last/rp-outfits.json';
if (!is_file($file)) {
    http_response_code(503);
    echo json_encode(['ok' => false, 'error' => 'Catalogue RP non généré']);
    exit;
}

$catalog = json_decode(file_get_contents($file), true);
$selected = null;
foreach (($catalog['outfits'] ?? []) as $outfit) {
    if (($outfit['id'] ?? '') === $outfitId) {
        $selected = $outfit;
        break;
    }
}

if (!$selected) {
    http_response_code(404);
    echo json_encode(['ok' => false, 'error' => 'Tenue introuvable']);
    exit;
}

$figure = (string)($selected['figure'] ?? '');
$gender = strtoupper((string)($selected['gender'] ?? 'M'));
if (!preg_match('/^[a-z0-9.\-]+$/i', $figure) || !in_array($gender, ['M', 'F'], true)) {
    http_response_code(422);
    echo json_encode(['ok' => false, 'error' => 'Données de tenue invalides']);
    exit;
}

$uid = (int)$UData['id'];
$sql = "UPDATE `users` SET `look`='" . $figure . "', `gender`='" . $gender . "' WHERE `id`='" . $uid . "' LIMIT 1";
$result = $DB->Query($sql);

if ($result === false) {
    http_response_code(500);
    echo json_encode(['ok' => false, 'error' => 'Impossible de sauvegarder la tenue']);
    exit;
}

echo json_encode([
    'ok' => true,
    'id' => $outfitId,
    'name' => (string)($selected['name'] ?? 'Tenue RP'),
    'figure' => $figure,
    'gender' => $gender,
    'reload' => true
]);

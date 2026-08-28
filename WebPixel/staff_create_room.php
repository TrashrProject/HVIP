<?php
require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');

if (!$Session->Exist(Config::$SessionName) || !isset($UData['rank']) || (int) $UData['rank'] < 4) {
    http_response_code(403);
    echo json_encode(array('success' => false, 'message' => 'Accès réservé au staff.'));
    exit;
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(array('success' => false, 'message' => 'Méthode non autorisée.'));
    exit;
}

$name = trim((string) ($_POST['name'] ?? ''));
$description = trim((string) ($_POST['description'] ?? ''));
$model = trim((string) ($_POST['model'] ?? 'model_a'));
$visitors = max(10, min(25, (int) ($_POST['visitors'] ?? 25)));
$allowedModels = array('model_a', 'model_b', 'model_c', 'model_d', 'model_e', 'model_f', 'model_i', 'model_j');

if (mb_strlen($name) < 3 || mb_strlen($name) > 50) {
    echo json_encode(array('success' => false, 'message' => 'Le nom doit contenir entre 3 et 50 caractères.'));
    exit;
}
if (mb_strlen($description) > 255) $description = mb_substr($description, 0, 255);
if (!in_array($model, $allowedModels, true)) $model = 'model_a';

$connection = $DB->Con();
$modelStatement = mysqli_prepare($connection, "SELECT id FROM room_models WHERE id = ? AND custom = '0' LIMIT 1");
mysqli_stmt_bind_param($modelStatement, 's', $model);
mysqli_stmt_execute($modelStatement);
$modelResult = mysqli_stmt_get_result($modelStatement);
if (!$modelResult || mysqli_num_rows($modelResult) !== 1) {
    echo json_encode(array('success' => false, 'message' => 'Le modèle sélectionné est indisponible.'));
    exit;
}

mysqli_begin_transaction($connection);
try {
    $owner = (int) $UData['id'];
    $category = 36;
    $tradeSettings = 2;
    $roomStatement = mysqli_prepare($connection, "INSERT INTO rooms (roomtype, caption, description, owner, model_name, category, users_max, trade_settings) VALUES ('private', ?, ?, ?, ?, ?, ?, ?)");
    mysqli_stmt_bind_param($roomStatement, 'ssisiii', $name, $description, $owner, $model, $category, $visitors, $tradeSettings);
    if (!mysqli_stmt_execute($roomStatement)) throw new RuntimeException(mysqli_stmt_error($roomStatement));
    $roomId = (int) mysqli_insert_id($connection);

    $city = 'pixelzone';
    $roleplayStatement = mysqli_prepare($connection, "INSERT INTO play_rooms (id, city, safezone_enabled, wardrobe_enabled) VALUES (?, ?, '1', '1')");
    mysqli_stmt_bind_param($roleplayStatement, 'is', $roomId, $city);
    if (!mysqli_stmt_execute($roleplayStatement)) throw new RuntimeException(mysqli_stmt_error($roleplayStatement));

    mysqli_commit($connection);
    echo json_encode(array('success' => true, 'roomId' => $roomId, 'message' => 'Appartement créé avec succès.'));
} catch (Throwable $exception) {
    mysqli_rollback($connection);
    http_response_code(500);
    echo json_encode(array('success' => false, 'message' => 'Impossible de créer l’appartement.'));
}

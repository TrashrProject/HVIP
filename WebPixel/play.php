<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * Localhost/XAMPP Nitro client entry point.
 */

require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

$ClientAUTH = $UserMG->GenerateAUTH($UData['id']);
$UserMG->GenerateMachineId($UData['id']);
$UserMG->CheckVIPStatus($UData['id']);

$nitroUrl = Config::$URL . "/nitro/index.html?sso=" . rawurlencode($ClientAUTH);
?>
<!doctype html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?php echo Config::$WName; ?> ~ Client</title>
    <style>
        html, body { margin:0; padding:0; width:100%; height:100%; overflow:hidden; background:#111; }
        iframe { border:0; width:100%; height:100%; display:block; }
    </style>
</head>
<body>
    <iframe src="<?php echo htmlspecialchars($nitroUrl, ENT_QUOTES, 'UTF-8'); ?>" allow="camera; microphone; fullscreen" allowfullscreen></iframe>
</body>
</html>

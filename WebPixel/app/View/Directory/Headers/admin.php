<?php
if (!isset($PCC)) { http_response_code(500); exit; }
?>
<!doctype html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="robots" content="noindex,nofollow,noarchive">
    <title><?php echo htmlspecialchars(Config::$WName . ' — ' . $PageName, ENT_QUOTES, 'UTF-8'); ?></title>
    <link rel="shortcut icon" href="<?php echo IMG; ?>/favicon.ico?v2" type="image/vnd.microsoft.icon">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&family=Ubuntu:wght@700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css" crossorigin="anonymous">
    <link rel="stylesheet" href="<?php echo CSS; ?>/admin-control-center.css?v=<?php echo rawurlencode(Config::$V); ?>">
    <link rel="stylesheet" href="<?php echo CSS; ?>/admin-control-center-hotfix.css?v=<?php echo time(); ?>">
</head>
<body class="pcc-body">

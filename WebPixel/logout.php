<?php
require_once __DIR__ . '/app/init.pz.php';
$Session->Delete(Config::$SessionName);
$_SESSION = [];
if(ini_get('session.use_cookies')):
    $params = session_get_cookie_params();
    setcookie(session_name(), '', time() - 42000, $params['path'], $params['domain'] ?? '', (bool)$params['secure'], (bool)$params['httponly']);
endif;
$Session->Destroy();
header('Location: ' . Config::$URL . '/?logout=success');
exit;

<?php
require_once "app/init.pz.php";
if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;
if(isset($_GET['chanpass']) && $_SERVER['REQUEST_METHOD'] === 'POST'):
    if(!isset($_SESSION['account_csrf']) || !hash_equals($_SESSION['account_csrf'], $_POST['csrf'] ?? '')):
        http_response_code(419); exit('Formulaire expiré.');
    endif;
    $CP = (string)($_POST['current_password'] ?? '');
    $NP = (string)($_POST['new_password'] ?? '');
    $CNP = (string)($_POST['new_password_confirmation'] ?? '');
    if(strlen($NP) < 8):
        $_SESSION['E'] = false; $_SESSION['M'] = "Le nouveau mot de passe doit contenir au moins 8 caractères.";
    elseif($NP === $CNP):
        $R = json_decode($UserMG->ChangeOldPass($UData['username'], $CP, $NP));
        $_SESSION['E'] = $R->result; $_SESSION['M'] = $R->msg;
    else:
        $_SESSION['E'] = false; $_SESSION['M'] = "Les mots de passe ne correspondent pas.";
    endif;
endif;
if(empty($_SESSION['account_csrf'])) $_SESSION['account_csrf'] = bin2hex(random_bytes(32));
$PageName = "Paramètres du compte";
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'profile_settings.php';
require_once FOOTER . 'main.php';

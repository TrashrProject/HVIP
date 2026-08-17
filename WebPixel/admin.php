<?php
require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)) {
    header("Location: " . Config::$URL . "/");
    exit;
}

if((int)$UData['rank'] < 3) {
    header("Location: " . Config::$URL . "/me");
    exit;
}

if(empty($_SESSION['cms_admin_csrf'])) $_SESSION['cms_admin_csrf'] = bin2hex(random_bytes(32));
$AdminNotice = '';
if($_SERVER['REQUEST_METHOD'] === 'POST') {
    if((int)$UData['rank'] < 5 || !hash_equals($_SESSION['cms_admin_csrf'], $_POST['csrf'] ?? '')) {
        http_response_code(403);
        exit('Action non autorisee.');
    }
    $db = $DB->Con();
    $action = $_POST['action'] ?? '';
    $username = trim($_POST['username'] ?? '');
    $safeUser = mysqli_real_escape_string($db, $username);
    if($action === 'catalogue') {
        $offer = max(1, (int)($_POST['offer_id'] ?? 0));
        $price = max(0, min(999999, (int)($_POST['price'] ?? 0)));
        $active = empty($_POST['active']) ? 0 : 1;
        $DB->Query("UPDATE catalog_items SET cost_credits=$price, offer_active='$active' WHERE id=$offer LIMIT 1");
        $AdminNotice = 'Offre catalogue mise a jour.';
    } elseif($action === 'badge') {
        $badge = strtoupper(trim($_POST['badge'] ?? ''));
        $slot = max(0, min(4, (int)($_POST['slot'] ?? 0)));
        $user = mysqli_fetch_assoc($DB->Query("SELECT id FROM users WHERE username='$safeUser' LIMIT 1"));
        if(!$user || !preg_match('/^[A-Z0-9_]{1,100}$/', $badge)) throw new RuntimeException('Joueur ou badge invalide.');
        $uid = (int)$user['id']; $safeBadge = mysqli_real_escape_string($db, $badge);
        $DB->Query("DELETE FROM user_badges WHERE user_id=$uid AND badge_slot=$slot");
        $DB->Query("INSERT INTO user_badges (user_id,badge_id,badge_slot) VALUES ($uid,'$safeBadge',$slot)");
        $AdminNotice = 'Badge attribue.';
    } elseif($action === 'look') {
        $look = substr(trim($_POST['look'] ?? ''), 0, 700);
        if($username === '' || $look === '') throw new RuntimeException('Look ou joueur invalide.');
        $safeLook = mysqli_real_escape_string($db, $look);
        $DB->Query("UPDATE users SET look='$safeLook' WHERE username='$safeUser' LIMIT 1");
        $AdminNotice = 'Look du joueur mis a jour.';
    } elseif($action === 'ban' || $action === 'unban') {
        if($username === '') throw new RuntimeException('Joueur invalide.');
        if($action === 'unban') {
            $DB->Query("DELETE FROM bans WHERE bantype='user' AND value='$safeUser'");
            $AdminNotice = 'Joueur debanni. Il peut se reconnecter.';
        } else {
            $days = max(1, min(3650, (int)($_POST['days'] ?? 1)));
            $reason = substr(trim($_POST['reason'] ?? 'Sanction staff'), 0, 250);
            $safeReason = mysqli_real_escape_string($db, $reason);
            $by = mysqli_real_escape_string($db, $UData['username']);
            $expire = time() + ($days * 86400);
            $DB->Query("DELETE FROM bans WHERE bantype='user' AND value='$safeUser'");
            $DB->Query("INSERT INTO bans (bantype,value,reason,expire,added_by,added_date) VALUES ('user','$safeUser','$safeReason',$expire,'$by','" . time() . "')");
            $AdminNotice = 'Joueur banni pour ' . $days . ' jour(s).';
        }
    } elseif($action === 'maintenance' || $action === 'maintenance_on' || $action === 'maintenance_off') {
        $enabled = ($action === 'maintenance_on') ? true : (($action === 'maintenance_off') ? false : !empty($_POST['enabled']));
        file_put_contents(__DIR__ . '/app/runtime-settings.json', json_encode(array('maintenance' => $enabled), JSON_PRETTY_PRINT), LOCK_EX);
        $AdminNotice = $enabled ? 'Maintenance CMS activee.' : 'Maintenance CMS desactivee.';
    }
}

$PageName = "Espace staff";
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'admin.php';
require_once FOOTER . 'main.php';

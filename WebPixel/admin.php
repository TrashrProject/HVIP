<?php
require_once "app/init.pz.php";

if (!$Session->Exist(Config::$SessionName) || !isset($UData)) {
    header("Location: " . Config::$URL . "/");
    exit;
}

require_once __DIR__ . '/app/Controller/AdminControlCenter.class.php';
$PCC = new AdminControlCenter($DB, $UData);

if (!$PCC->can('admin.access')) {
    http_response_code(403);
    header("Location: " . Config::$URL . "/me");
    exit;
}

header('X-Frame-Options: SAMEORIGIN');
header('X-Content-Type-Options: nosniff');
header('Referrer-Policy: same-origin');
header('Cache-Control: no-store, max-age=0');

$allowedPages = array('dashboard', 'players', 'player', 'businesses', 'sanctions', 'logs', 'staff', 'permissions', 'settings', 'tools');
$AdminPage = isset($_GET['page']) ? strtolower(trim((string)$_GET['page'])) : 'dashboard';
if (!in_array($AdminPage, $allowedPages, true)) $AdminPage = 'dashboard';

$pageCapabilities = array(
    'dashboard' => 'admin.dashboard.view',
    'players' => 'admin.players.view',
    'player' => 'admin.players.view',
    'businesses' => 'admin.businesses.view',
    'sanctions' => 'admin.sanctions.view',
    'logs' => 'admin.logs.view',
    'staff' => 'admin.staff.view',
    'permissions' => 'admin.permissions.view',
    'settings' => 'admin.settings.view',
    'tools' => 'admin.tools.view'
);
if (!$PCC->can($pageCapabilities[$AdminPage])) {
    http_response_code(403);
    $PCC->setFlash('error', 'Vous n’avez pas la permission d’ouvrir ce module.');
    header('Location: ' . Config::$URL . '/admin.php?page=dashboard');
    exit;
}

// Lightweight global search. Only real, verified user fields are searched.
if (isset($_GET['ajax']) && $_GET['ajax'] === 'global-search') {
    try {
        $PCC->requireCapability('admin.players.view');
        $q = trim((string)($_GET['q'] ?? ''));
        if (mb_strlen($q) < 2) $PCC->jsonResponse(array('groups' => array('players' => array())));
        $like = '%' . mb_substr($q, 0, 64) . '%';
        $id = ctype_digit($q) ? (int)$q : -1;
        $result = $DB->PreparedResult(
            'SELECT id, username, rank, online, look FROM users WHERE username LIKE ? OR id = ? ORDER BY online DESC, username ASC LIMIT 8',
            'si',
            array($like, $id)
        );
        $players = array();
        while ($row = mysqli_fetch_assoc($result)) {
            $players[] = array(
                'id' => (int)$row['id'],
                'username' => $row['username'],
                'role' => $PCC->roleName($row['rank']),
                'online' => (bool)$row['online'],
                'avatar' => $PCC->avatarUrl($row['look'], 's'),
                'url' => Config::$URL . '/admin.php?page=player&id=' . (int)$row['id']
            );
        }
        $PCC->jsonResponse(array('groups' => array('players' => $players)));
    } catch (Throwable $e) {
        $PCC->jsonResponse(array('error' => 'Recherche indisponible.'), 500);
    }
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $redirectPage = isset($_POST['return_page']) ? (string)$_POST['return_page'] : $AdminPage;
    if (!in_array($redirectPage, $allowedPages, true)) $redirectPage = 'dashboard';
    $redirect = Config::$URL . '/admin.php?page=' . rawurlencode($redirectPage);
    if (!empty($_POST['return_id']) && ctype_digit((string)$_POST['return_id'])) {
        $redirect .= '&id=' . (int)$_POST['return_id'];
    }

    try {
        $PCC->requirePostSecurity($_POST);
        $action = isset($_POST['action']) ? (string)$_POST['action'] : '';

        if ($action === 'logout') {
            $Session->Destroy();
            header('Location: ' . Config::$URL . '/');
            exit;
        }

        if ($action === 'economy_adjust') {
            $PCC->requireCapability('admin.economy.adjust');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $wallet = ($_POST['wallet'] ?? '') === 'bank' ? 'bank' : 'credits';
            $operation = in_array($_POST['operation'] ?? '', array('add', 'remove', 'set'), true) ? $_POST['operation'] : '';
            $amount = filter_var($_POST['amount'] ?? null, FILTER_VALIDATE_INT, array('options' => array('min_range' => 0, 'max_range' => 2000000000)));
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            if ($userId < 0 || $operation === '' || $amount === false) throw new RuntimeException('Paramètres économiques invalides.');

            $DB->BeginTransaction();
            try {
                $result = $DB->PreparedResult(
                    'SELECT u.id, u.username, u.online, u.credits, p.bank FROM users u INNER JOIN play_stats p ON p.id = u.id WHERE u.id = ? LIMIT 1 FOR UPDATE',
                    'i',
                    array($userId)
                );
                $player = mysqli_fetch_assoc($result);
                if (!$player) throw new RuntimeException('Joueur introuvable ou données RP incomplètes.');
                if ((int)$player['online'] === 1) {
                    throw new RuntimeException('Ce joueur est connecté. L’ajustement est bloqué tant qu’aucun pont ÉMU sécurisé n’a été détecté, afin d’éviter une désynchronisation du HUD.');
                }
                $current = (int)$player[$wallet];
                if ($operation === 'add') $next = $current + (int)$amount;
                elseif ($operation === 'remove') $next = $current - (int)$amount;
                else $next = (int)$amount;
                if ($next < 0 || $next > 2000000000) throw new RuntimeException('Le solde final est invalide.');

                if ($wallet === 'credits') {
                    $DB->PreparedAffect('UPDATE users SET credits = ? WHERE id = ? LIMIT 1', 'ii', array($next, $userId));
                } else {
                    $DB->PreparedAffect('UPDATE play_stats SET bank = ? WHERE id = ? LIMIT 1', 'ii', array($next, $userId));
                }
                $PCC->audit('PLAYER_MONEY_ADJUSTED', 'player', $userId,
                    array('wallet' => $wallet, 'balance' => $current),
                    array('wallet' => $wallet, 'balance' => $next, 'operation' => $operation, 'amount' => (int)$amount),
                    $reason
                );
                $DB->Commit();
                $PCC->setFlash('success', 'Solde de ' . $player['username'] . ' mis à jour et journalisé.');
            } catch (Throwable $e) {
                $DB->Rollback();
                throw $e;
            }
        } elseif ($action === 'ban') {
            $PCC->requireCapability('admin.sanctions.manage');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $days = filter_var($_POST['days'] ?? null, FILTER_VALIDATE_INT, array('options' => array('min_range' => 1, 'max_range' => 3650)));
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            if ($days === false) throw new RuntimeException('Durée de bannissement invalide.');
            $userResult = $DB->PreparedResult('SELECT id, username FROM users WHERE id = ? LIMIT 1', 'i', array($userId));
            $user = mysqli_fetch_assoc($userResult);
            if (!$user) throw new RuntimeException('Joueur introuvable.');
            $beforeResult = $DB->PreparedResult("SELECT reason, expire, added_by, added_date FROM bans WHERE bantype='user' AND value=? AND expire >= ? ORDER BY expire DESC LIMIT 1", 'si', array($user['username'], time()));
            $before = mysqli_fetch_assoc($beforeResult) ?: null;
            $expire = time() + ((int)$days * 86400);

            $DB->BeginTransaction();
            try {
                $DB->PreparedAffect("DELETE FROM bans WHERE bantype='user' AND value=?", 's', array($user['username']));
                $DB->PreparedAffect("INSERT INTO bans (bantype,value,reason,expire,added_by,added_date) VALUES ('user',?,?,?,?,?)", 'ssisi', array($user['username'], $reason, $expire, $UData['username'], time()));
                $PCC->audit('PLAYER_BANNED', 'player', $userId, $before, array('reason' => $reason, 'expire' => $expire, 'days' => (int)$days), $reason);
                $DB->Commit();
                $PCC->setFlash('success', $user['username'] . ' a été banni et l’action est auditée.');
            } catch (Throwable $e) {
                $DB->Rollback();
                throw $e;
            }
        } elseif ($action === 'unban') {
            $PCC->requireCapability('admin.sanctions.manage');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $userResult = $DB->PreparedResult('SELECT id, username FROM users WHERE id = ? LIMIT 1', 'i', array($userId));
            $user = mysqli_fetch_assoc($userResult);
            if (!$user) throw new RuntimeException('Joueur introuvable.');
            $banResult = $DB->PreparedResult("SELECT reason, expire, added_by, added_date FROM bans WHERE bantype='user' AND value=? ORDER BY expire DESC LIMIT 1", 's', array($user['username']));
            $before = mysqli_fetch_assoc($banResult);
            if (!$before) throw new RuntimeException('Aucun bannissement à retirer.');

            $DB->BeginTransaction();
            try {
                $DB->PreparedAffect("DELETE FROM bans WHERE bantype='user' AND value=?", 's', array($user['username']));
                $PCC->audit('PLAYER_UNBANNED', 'player', $userId, $before, array('status' => 'unbanned'), $reason);
                $DB->Commit();
                $PCC->setFlash('success', $user['username'] . ' a été débanni et l’action est auditée.');
            } catch (Throwable $e) {
                $DB->Rollback();
                throw $e;
            }
        } elseif ($action === 'look') {
            $PCC->requireCapability('admin.players.appearance.edit');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $look = mb_substr(trim((string)($_POST['look'] ?? '')), 0, 700);
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            if ($look === '') throw new RuntimeException('Look invalide.');
            $result = $DB->PreparedResult('SELECT id, username, look FROM users WHERE id = ? LIMIT 1', 'i', array($userId));
            $user = mysqli_fetch_assoc($result);
            if (!$user) throw new RuntimeException('Joueur introuvable.');
            $DB->BeginTransaction();
            try {
                $DB->PreparedAffect('UPDATE users SET look = ? WHERE id = ? LIMIT 1', 'si', array($look, $userId));
                $PCC->audit('PLAYER_LOOK_CHANGED', 'player', $userId, array('look' => $user['look']), array('look' => $look), $reason);
                $DB->Commit();
                $PCC->setFlash('success', 'Apparence de ' . $user['username'] . ' mise à jour.');
            } catch (Throwable $e) {
                $DB->Rollback();
                throw $e;
            }
        } elseif ($action === 'badge') {
            $PCC->requireCapability('admin.players.badges.edit');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $badge = strtoupper(trim((string)($_POST['badge'] ?? '')));
            $slot = filter_var($_POST['slot'] ?? 0, FILTER_VALIDATE_INT, array('options' => array('min_range' => 0, 'max_range' => 4)));
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            if ($slot === false || !preg_match('/^[A-Z0-9_]{1,100}$/', $badge)) throw new RuntimeException('Badge ou emplacement invalide.');
            $userResult = $DB->PreparedResult('SELECT id, username FROM users WHERE id = ? LIMIT 1', 'i', array($userId));
            $user = mysqli_fetch_assoc($userResult);
            if (!$user) throw new RuntimeException('Joueur introuvable.');
            $beforeResult = $DB->PreparedResult('SELECT badge_id, badge_slot FROM user_badges WHERE user_id = ? AND badge_slot = ? LIMIT 1', 'ii', array($userId, $slot));
            $before = mysqli_fetch_assoc($beforeResult) ?: null;
            $DB->BeginTransaction();
            try {
                $DB->PreparedAffect('DELETE FROM user_badges WHERE user_id = ? AND badge_slot = ?', 'ii', array($userId, $slot));
                $DB->PreparedAffect('INSERT INTO user_badges (user_id,badge_id,badge_slot) VALUES (?,?,?)', 'isi', array($userId, $badge, $slot));
                $PCC->audit('PLAYER_BADGE_ASSIGNED', 'player', $userId, $before, array('badge_id' => $badge, 'badge_slot' => (int)$slot), $reason);
                $DB->Commit();
                $PCC->setFlash('success', 'Badge attribué à ' . $user['username'] . '.');
            } catch (Throwable $e) {
                $DB->Rollback();
                throw $e;
            }
        } elseif ($action === 'catalogue') {
            $PCC->requireCapability('admin.catalogue.edit');
            $PCC->requireAuditReady();
            $offerId = (int)($_POST['offer_id'] ?? 0);
            $price = filter_var($_POST['price'] ?? null, FILTER_VALIDATE_INT, array('options' => array('min_range' => 0, 'max_range' => 999999)));
            $active = empty($_POST['active']) ? 0 : 1;
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            if ($offerId < 1 || $price === false) throw new RuntimeException('Offre ou prix invalide.');
            $result = $DB->PreparedResult('SELECT id, cost_credits, offer_active FROM catalog_items WHERE id = ? LIMIT 1', 'i', array($offerId));
            $before = mysqli_fetch_assoc($result);
            if (!$before) throw new RuntimeException('Offre catalogue introuvable.');
            $DB->BeginTransaction();
            try {
                $DB->PreparedAffect('UPDATE catalog_items SET cost_credits = ?, offer_active = ? WHERE id = ? LIMIT 1', 'iii', array($price, $active, $offerId));
                $PCC->audit('CATALOGUE_OFFER_CHANGED', 'catalogue_offer', $offerId, $before, array('cost_credits' => (int)$price, 'offer_active' => $active), $reason);
                $DB->Commit();
                $PCC->setFlash('success', 'Offre catalogue mise à jour.');
            } catch (Throwable $e) {
                $DB->Rollback();
                throw $e;
            }
        } elseif ($action === 'maintenance_on' || $action === 'maintenance_off') {
            $PCC->requireCapability('admin.settings.maintenance');
            $PCC->requireAuditReady();
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $enabled = $action === 'maintenance_on';
            $runtimePath = __DIR__ . '/app/runtime-settings.json';
            $oldContent = is_file($runtimePath) ? (string)file_get_contents($runtimePath) : null;
            $before = array('maintenance' => (bool)Config::$_MANT);
            $payload = json_encode(array('maintenance' => $enabled), JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES);
            if (file_put_contents($runtimePath, $payload, LOCK_EX) === false) throw new RuntimeException('Impossible d’écrire la configuration de maintenance.');
            try {
                $PCC->audit('CMS_MAINTENANCE_CHANGED', 'cms', 'maintenance', $before, array('maintenance' => $enabled), $reason);
            } catch (Throwable $e) {
                if ($oldContent !== null) file_put_contents($runtimePath, $oldContent, LOCK_EX);
                else @unlink($runtimePath);
                throw $e;
            }
            $PCC->setFlash('success', $enabled ? 'Maintenance CMS activée.' : 'Maintenance CMS désactivée.');
        } else {
            throw new RuntimeException('Action administrative inconnue.');
        }
    } catch (Throwable $e) {
        $PCC->setFlash('error', $e->getMessage());
    }

    header('Location: ' . $redirect);
    exit;
}

$AdminFlash = $PCC->pullFlash();
$PageName = 'Paradise Control Center';
require_once HEADER . 'admin.php';
require_once BODY . 'admin.php';
require_once FOOTER . 'admin.php';

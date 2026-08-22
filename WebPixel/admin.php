<?php
require_once 'app/init.pz.php';

if (!$Session->Exist(Config::$SessionName)) {
    header('Location: ' . Config::$URL . '/');
    exit;
}
if (!isset($UData) || (int)$UData['rank'] < 3) {
    header('Location: ' . Config::$URL . '/me');
    exit;
}

require_once __DIR__ . '/app/Controller/AdminControlCenter.class.php';
$PCC = new AdminControlCenter($DB, $UData);
$PCC->requireCapability('admin.access');

// Reuse the real ParadiseRP projections already used by the player client.
$characterLib = __DIR__ . '/paradise-character-lib.php';
$inventoryLib = __DIR__ . '/paradise-inventory-lib.php';
$phoneLib = __DIR__ . '/paradise-phone-lib.php';
if (is_file($characterLib)) require_once $characterLib;
if (is_file($inventoryLib)) require_once $inventoryLib;
if (is_file($phoneLib)) require_once $phoneLib;

$pages = array(
    'dashboard' => 'dashboard.view',
    'players' => 'players.view',
    'player' => 'players.view',
    'characters' => 'character.view',
    'sessions' => 'players.view',
    'economy' => 'economy.view',
    'documents' => 'documents.view',
    'inventory' => 'inventory.view',
    'items' => 'items.view',
    'phones' => 'phone.view',
    'catalogue' => 'catalog.view',
    'badges' => 'badges.view',
    'businesses' => 'business.view',
    'sanctions' => 'sanctions.view',
    'staff' => 'staff.view',
    'commands' => 'admin.access',
    'logs' => 'logs.view',
    'permissions' => 'permissions.view',
    'settings' => 'config.manage',
    'health' => 'health.view'
);

$AdminPage = isset($_GET['page']) ? strtolower(trim((string)$_GET['page'])) : 'dashboard';
if (!isset($pages[$AdminPage])) $AdminPage = 'dashboard';
if (!$PCC->can($pages[$AdminPage])) {
    http_response_code(403);
    exit('Accès refusé.');
}

function pcc_return_page($value, $pages)
{
    $value = strtolower(trim((string)$value));
    return isset($pages[$value]) ? $value : 'dashboard';
}

// Powerful global search: username/user id + RP identity + citizen id + phone,
// and businesses when those real systems are present.
if (isset($_GET['ajax']) && $_GET['ajax'] === 'global-search') {
    $PCC->requireCapability('players.view');
    $q = trim((string)($_GET['q'] ?? ''));
    if (mb_strlen($q) < 2) $PCC->jsonResponse(array('groups' => array('players' => array(), 'businesses' => array())));
    $like = '%' . mb_substr($q, 0, 64) . '%';

    $joins = '';
    $select = "u.id,u.username,u.look,u.rank,u.online";
    $where = array('u.username LIKE ?', 'CAST(u.id AS CHAR)=?');
    $types = 'ss';
    $params = array($like, $q);

    if ($PCC->tableExists('rp_characters')) {
        $joins .= ' LEFT JOIN rp_characters c ON c.user_id=u.id';
        $select .= ',c.citizen_id,c.first_name,c.last_name';
        $where[] = 'c.citizen_id LIKE ?';
        $where[] = "CONCAT(COALESCE(c.first_name,''),' ',COALESCE(c.last_name,'')) LIKE ?";
        $types .= 'ss';
        $params[] = $like;
        $params[] = $like;
    } else {
        $select .= ",NULL citizen_id,NULL first_name,NULL last_name";
    }

    if ($PCC->tableExists('rp_phones')) {
        $joins .= ' LEFT JOIN rp_phones p ON p.user_id=u.id';
        $select .= ',p.phone_number';
        $where[] = 'p.phone_number LIKE ?';
        $types .= 's';
        $params[] = $like;
    } else {
        $select .= ',NULL phone_number';
    }

    $players = $DB->PreparedAll(
        'SELECT ' . $select . ' FROM users u' . $joins . ' WHERE ' . implode(' OR ', $where) . ' ORDER BY u.online DESC,u.username ASC LIMIT 12',
        $types,
        $params
    );
    foreach ($players as &$row) {
        $row['id'] = (int)$row['id'];
        $row['rank'] = (int)$row['rank'];
        $row['online'] = (int)$row['online'] === 1;
        $row['role'] = $PCC->roleName($row['rank']);
        $row['rp_name'] = trim((string)$row['first_name'] . ' ' . (string)$row['last_name']);
        $row['avatar'] = $PCC->avatarUrl($row['look'], 's');
        $row['url'] = Config::$URL . '/admin.php?page=player&id=' . $row['id'];
    }
    unset($row);

    $businesses = array();
    if ($PCC->tableExists('groups')) {
        try {
            $businesses = $DB->PreparedAll("SELECT id,name,type FROM groups WHERE (type='1' OR type='2') AND name LIKE ? ORDER BY name LIMIT 8", 's', array($like));
            foreach ($businesses as &$business) {
                $business['id'] = (int)$business['id'];
                $business['url'] = Config::$URL . '/admin.php?page=businesses&q=' . rawurlencode((string)$business['name']);
            }
            unset($business);
        } catch (Throwable $e) { $businesses = array(); }
    }

    $PCC->jsonResponse(array('groups' => array('players' => $players, 'businesses' => $businesses)));
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $returnPage = pcc_return_page($_POST['return_page'] ?? 'dashboard', $pages);
    $redirect = Config::$URL . '/admin.php?page=' . rawurlencode($returnPage);
    if (!empty($_POST['return_id'])) $redirect .= '&id=' . (int)$_POST['return_id'];
    if (!empty($_POST['return_tab'])) $redirect .= '&tab=' . rawurlencode((string)$_POST['return_tab']);

    try {
        $PCC->requirePostSecurity($_POST);
        $action = (string)($_POST['action'] ?? '');

        if ($action === 'logout') {
            $Session->Destroy();
            header('Location: ' . Config::$URL . '/');
            exit;
        }

        if ($action === 'character_update') {
            $PCC->requireCapability('character.edit');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $PCC->requireOfflineUser($userId, 'La modification du personnage');
            if (!$PCC->tableExists('rp_characters') || !function_exists('pr_character_validate_identity')) throw new RuntimeException('Character V2 n’est pas disponible.');
            $before = function_exists('pr_character_row') ? pr_character_row($DB->Con(), $userId) : null;
            if (!$before) throw new RuntimeException('Ce joueur n’a pas encore de personnage RP.');
            $errors = array();
            $identity = pr_character_validate_identity($_POST, $errors);
            if (!$identity) throw new RuntimeException($errors ? implode(' ', $errors) : 'Identité RP invalide.');
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $DB->Begin();
            try {
                $DB->PreparedAffect('UPDATE rp_characters SET first_name=?,last_name=?,birth_date=?,gender=?,nationality=?,biography=? WHERE user_id=? LIMIT 1', 'ssssssi', array(
                    $identity['first_name'], $identity['last_name'], $identity['birth_date'], $identity['gender'], $identity['nationality'], $identity['biography'], $userId
                ));
                $after = function_exists('pr_character_row') ? pr_character_row($DB->Con(), $userId) : $identity;
                $PCC->audit('CHARACTER_EDIT', 'character', 'user', $userId, $before, $after, $reason);
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }
            $PCC->setFlash('success', 'Personnage RP mis à jour.');

        } elseif ($action === 'economy_adjust') {
            $PCC->requireCapability('economy.adjust');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $PCC->requireOfflineUser($userId, 'L’ajustement économique');
            $wallet = strtolower((string)($_POST['wallet'] ?? ''));
            $operation = strtolower((string)($_POST['operation'] ?? ''));
            $amount = (int)($_POST['amount'] ?? -1);
            if (!in_array($wallet, array('cash','bank'), true)) throw new RuntimeException('Compte économique invalide.');
            if (!in_array($operation, array('add','remove','set'), true)) throw new RuntimeException('Opération économique invalide.');
            if ($amount < 0 || $amount > 2000000000) throw new RuntimeException('Montant invalide.');
            if ($operation !== 'set' && $amount === 0) throw new RuntimeException('Le montant doit être supérieur à zéro.');
            if ($wallet === 'bank' && !$PCC->tableExists('play_stats')) throw new RuntimeException('Compte bancaire RP indisponible.');
            $reason = $PCC->requireReason($_POST['reason'] ?? '');

            $DB->Begin();
            try {
                if ($wallet === 'cash') {
                    $row = $DB->PreparedRow('SELECT credits AS balance FROM users WHERE id=? LIMIT 1 FOR UPDATE', 'i', array($userId));
                } else {
                    $row = $DB->PreparedRow('SELECT bank AS balance FROM play_stats WHERE id=? LIMIT 1 FOR UPDATE', 'i', array($userId));
                }
                if (!$row) throw new RuntimeException('Compte économique introuvable.');
                $old = (int)$row['balance'];
                $new = $operation === 'set' ? $amount : ($operation === 'add' ? $old + $amount : $old - $amount);
                if ($new < 0 || $new > 2000000000) throw new RuntimeException('Le solde final est invalide.');

                if ($wallet === 'cash') $DB->PreparedAffect('UPDATE users SET credits=? WHERE id=? LIMIT 1', 'ii', array($new, $userId));
                else $DB->PreparedAffect('UPDATE play_stats SET bank=? WHERE id=? LIMIT 1', 'ii', array($new, $userId));

                $actionName = 'PLAYER_MONEY_' . strtoupper($operation);
                $PCC->audit($actionName, 'economy', 'user', $userId, array($wallet => $old), array($wallet => $new, 'amount' => $amount, 'operation' => $operation), $reason);
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }
            $PCC->setFlash('success', 'Solde ' . ($wallet === 'cash' ? 'cash' : 'bancaire') . ' mis à jour.');

        } elseif ($action === 'document_status') {
            $PCC->requireCapability('documents.manage');
            $PCC->requireAuditReady();
            $documentId = (int)($_POST['document_id'] ?? 0);
            $statusAction = strtolower((string)($_POST['status_action'] ?? ''));
            $map = array('suspend' => 'SUSPENDED', 'reactivate' => 'VALID', 'revoke' => 'REVOKED');
            if (!isset($map[$statusAction])) throw new RuntimeException('Action documentaire invalide.');
            $before = $DB->PreparedRow('SELECT d.*,t.code,t.name FROM rp_player_documents d INNER JOIN rp_document_types t ON t.id=d.document_type_id WHERE d.id=? LIMIT 1', 'i', array($documentId));
            if (!$before) throw new RuntimeException('Document introuvable.');
            $userId = (int)$before['user_id'];
            $PCC->requireOfflineUser($userId, 'La modification d’un document');
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $newStatus = $map[$statusAction];
            $DB->Begin();
            try {
                $DB->PreparedAffect('UPDATE rp_player_documents SET status=? WHERE id=? LIMIT 1', 'si', array($newStatus, $documentId));
                $after = $before;
                $after['status'] = $newStatus;
                $PCC->audit('DOCUMENT_' . strtoupper($statusAction), 'documents', 'document', $documentId, $before, $after, $reason);
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }
            $PCC->setFlash('success', 'Statut du document mis à jour.');

        } elseif ($action === 'ban' || $action === 'unban') {
            $PCC->requireCapability('sanctions.issue');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $user = $PCC->requireUser($userId);
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $username = (string)$user['username'];
            $before = $PCC->tableExists('bans') ? $DB->PreparedAll("SELECT * FROM bans WHERE bantype='user' AND value=?", 's', array($username)) : array();
            if (!$PCC->tableExists('bans')) throw new RuntimeException('Table de sanctions indisponible.');
            $DB->Begin();
            try {
                $DB->PreparedAffect("DELETE FROM bans WHERE bantype='user' AND value=?", 's', array($username));
                if ($action === 'ban') {
                    $days = max(1, min(3650, (int)($_POST['days'] ?? 1)));
                    $expire = time() + ($days * 86400);
                    $DB->PreparedAffect("INSERT INTO bans (bantype,value,reason,expire,added_by,added_date) VALUES ('user',?,?,?,?,?)", 'ssisi', array($username, $reason, $expire, (string)$UData['username'], time()));
                    $after = array('username' => $username, 'reason' => $reason, 'expire' => $expire);
                    $PCC->audit('BAN_CREATE', 'sanctions', 'user', $userId, $before, $after, $reason);
                    $PCC->setFlash('success', $username . ' a été banni.');
                } else {
                    $PCC->audit('BAN_REVOKE', 'sanctions', 'user', $userId, $before, array(), $reason);
                    $PCC->setFlash('success', 'Sanction de ' . $username . ' levée.');
                }
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }

        } elseif ($action === 'appearance_update') {
            $PCC->requireCapability('appearance.edit');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $user = $PCC->requireOfflineUser($userId, 'La modification de l’apparence');
            $look = trim((string)($_POST['look'] ?? ''));
            if ($look === '' || strlen($look) > 700 || !preg_match('/^[A-Za-z0-9.\-]+$/', $look)) throw new RuntimeException('Figure Habbo invalide.');
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $DB->Begin();
            try {
                $DB->PreparedAffect('UPDATE users SET look=? WHERE id=? LIMIT 1', 'si', array($look, $userId));
                $PCC->audit('PLAYER_APPEARANCE_EDIT', 'appearance', 'user', $userId, array('look' => $user['look']), array('look' => $look), $reason);
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }
            $PCC->setFlash('success', 'Apparence mise à jour.');

        } elseif ($action === 'badge_assign' || $action === 'badge_remove') {
            $PCC->requireCapability('badges.manage');
            $PCC->requireAuditReady();
            $userId = (int)($_POST['user_id'] ?? 0);
            $user = $PCC->requireUser($userId);
            $badge = strtoupper(trim((string)($_POST['badge'] ?? '')));
            if (!preg_match('/^[A-Z0-9_]{1,100}$/', $badge)) throw new RuntimeException('Code badge invalide.');
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $slot = max(0, min(4, (int)($_POST['slot'] ?? 0)));
            $before = $DB->PreparedAll('SELECT * FROM user_badges WHERE user_id=? AND badge_id=?', 'is', array($userId, $badge));
            $DB->Begin();
            try {
                if ($action === 'badge_assign') {
                    $DB->PreparedAffect('DELETE FROM user_badges WHERE user_id=? AND badge_slot=?', 'ii', array($userId, $slot));
                    $DB->PreparedAffect('INSERT INTO user_badges (user_id,badge_id,badge_slot) VALUES (?,?,?)', 'isi', array($userId, $badge, $slot));
                    $after = array('badge' => $badge, 'slot' => $slot);
                    $auditAction = 'BADGE_ASSIGN';
                } else {
                    $DB->PreparedAffect('DELETE FROM user_badges WHERE user_id=? AND badge_id=?', 'is', array($userId, $badge));
                    $after = array();
                    $auditAction = 'BADGE_REMOVE';
                }
                $PCC->audit($auditAction, 'badges', 'user', $userId, $before, $after, $reason);
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }
            $PCC->setFlash('success', 'Badges du joueur mis à jour.');

        } elseif ($action === 'catalogue_update') {
            $PCC->requireCapability('catalog.manage');
            $PCC->requireAuditReady();
            if (!$PCC->tableExists('catalog_items')) throw new RuntimeException('Catalogue indisponible.');
            $offerId = max(1, (int)($_POST['offer_id'] ?? 0));
            $price = max(0, min(999999, (int)($_POST['price'] ?? 0)));
            $active = !empty($_POST['active']) ? 1 : 0;
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $before = $DB->PreparedRow('SELECT id,cost_credits,offer_active FROM catalog_items WHERE id=? LIMIT 1', 'i', array($offerId));
            if (!$before) throw new RuntimeException('Offre catalogue introuvable.');
            $DB->Begin();
            try {
                $DB->PreparedAffect('UPDATE catalog_items SET cost_credits=?,offer_active=? WHERE id=? LIMIT 1', 'iii', array($price, $active, $offerId));
                $after = array('id' => $offerId, 'cost_credits' => $price, 'offer_active' => $active);
                $PCC->audit('CATALOGUE_UPDATE', 'catalogue', 'catalog_item', $offerId, $before, $after, $reason);
                $DB->Commit();
            } catch (Throwable $e) { $DB->Rollback(); throw $e; }
            $PCC->setFlash('success', 'Offre catalogue mise à jour.');

        } elseif ($action === 'maintenance_on' || $action === 'maintenance_off') {
            $PCC->requireCapability('config.manage');
            $PCC->requireAuditReady();
            $reason = $PCC->requireReason($_POST['reason'] ?? '');
            $enabled = $action === 'maintenance_on';
            $runtimePath = __DIR__ . '/app/runtime-settings.json';
            $before = array('maintenance' => (bool)Config::$_MANT);
            $previous = is_file($runtimePath) ? (string)file_get_contents($runtimePath) : null;
            $payload = json_encode(array('maintenance' => $enabled), JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES);
            if (file_put_contents($runtimePath, $payload, LOCK_EX) === false) throw new RuntimeException('Impossible d’écrire la configuration CMS.');
            try {
                $PCC->audit('CONFIG_MAINTENANCE', 'configuration', 'cms', 'maintenance', $before, array('maintenance' => $enabled), $reason);
            } catch (Throwable $e) {
                if ($previous !== null) file_put_contents($runtimePath, $previous, LOCK_EX); else @unlink($runtimePath);
                throw $e;
            }
            $PCC->setFlash('success', $enabled ? 'Maintenance activée.' : 'Maintenance désactivée.');

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

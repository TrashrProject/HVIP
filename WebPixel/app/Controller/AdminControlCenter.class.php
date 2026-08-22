<?php
/**
 * Paradise Control Center V3
 * Central security/RBAC/audit layer for the existing PixelZone CMS.
 * Habbo ranks remain the identity source; capabilities are mapped here so
 * every endpoint validates permissions server-side.
 */
class AdminControlCenter
{
    private $DB;
    private $staff;
    private $tableCache = array();
    private $columnCache = array();

    private $capabilities = array(
        'admin.access' => 3,
        'dashboard.view' => 3,
        'players.view' => 3,
        'character.view' => 3,
        'economy.view' => 3,
        'documents.view' => 3,
        'inventory.view' => 3,
        'items.view' => 3,
        'phone.view' => 3,
        'catalog.view' => 3,
        'badges.view' => 3,
        'business.view' => 3,
        'sanctions.view' => 3,
        'health.view' => 3,

        'sanctions.issue' => 4,

        'character.edit' => 5,
        'documents.manage' => 5,
        'inventory.adjust' => 5,
        'phone.manage' => 5,
        'appearance.edit' => 5,
        'badges.manage' => 5,
        'staff.view' => 5,
        'logs.view' => 5,

        'economy.adjust' => 6,
        'items.manage' => 6,
        'catalog.manage' => 6,
        'config.manage' => 6,
        'permissions.view' => 6,

        'staff.manage' => 7,
        'phone.messages.read' => 7
    );

    public function __construct(DBManager $DB, array $staff)
    {
        $this->DB = $DB;
        $this->staff = $staff;
        $this->ensureSecuritySession();
    }

    public function staff() { return $this->staff; }

    public function can($capability)
    {
        return isset($this->capabilities[$capability]) && (int)$this->staff['rank'] >= (int)$this->capabilities[$capability];
    }

    public function requireCapability($capability)
    {
        if (!$this->can($capability)) {
            http_response_code(403);
            throw new RuntimeException('Permission insuffisante pour cette action.');
        }
    }

    public function capabilityMap() { return $this->capabilities; }

    public function roleName($rank)
    {
        $rank = (int)$rank;
        if ($rank >= 7) return 'Fondateur';
        if ($rank === 6) return 'Développeur';
        if ($rank === 5) return 'Administrateur';
        if ($rank === 4) return 'Modérateur';
        if ($rank === 3) return 'Assistant';
        if ($rank === 2) return 'VIP';
        return 'Joueur';
    }

    public function csrfToken() { return $_SESSION['pcc_csrf']; }

    public function issueNonce()
    {
        $this->pruneNonces();
        $nonce = bin2hex(random_bytes(24));
        $_SESSION['pcc_nonces'][$nonce] = time() + 1800;
        return $nonce;
    }

    public function requirePostSecurity(array $post)
    {
        $token = isset($post['csrf']) ? (string)$post['csrf'] : '';
        if ($token === '' || !hash_equals($_SESSION['pcc_csrf'], $token)) {
            throw new RuntimeException('Jeton de sécurité invalide. Recharge la page puis réessaie.');
        }

        $nonce = isset($post['action_nonce']) ? (string)$post['action_nonce'] : '';
        $this->pruneNonces();
        if ($nonce === '' || !isset($_SESSION['pcc_nonces'][$nonce])) {
            throw new RuntimeException('Action déjà envoyée ou expirée. Recharge la page.');
        }
        unset($_SESSION['pcc_nonces'][$nonce]);
    }

    public function tableExists($table)
    {
        if (!preg_match('/^[A-Za-z0-9_]+$/', $table)) return false;
        if (array_key_exists($table, $this->tableCache)) return $this->tableCache[$table];
        $result = $this->DB->PreparedResult(
            'SELECT 1 FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name=? LIMIT 1',
            's', array($table)
        );
        $exists = mysqli_num_rows($result) === 1;
        mysqli_free_result($result);
        $this->tableCache[$table] = $exists;
        return $exists;
    }

    public function columnExists($table, $column)
    {
        if (!preg_match('/^[A-Za-z0-9_]+$/', $table) || !preg_match('/^[A-Za-z0-9_]+$/', $column)) return false;
        $key = $table . '.' . $column;
        if (array_key_exists($key, $this->columnCache)) return $this->columnCache[$key];
        $result = $this->DB->PreparedResult(
            'SELECT 1 FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name=? LIMIT 1',
            'ss', array($table, $column)
        );
        $exists = mysqli_num_rows($result) === 1;
        mysqli_free_result($result);
        $this->columnCache[$key] = $exists;
        return $exists;
    }

    public function auditReady() { return $this->tableExists('cms_admin_audit_log'); }

    public function requireAuditReady()
    {
        if (!$this->auditReady()) {
            throw new RuntimeException('Migration Control Center V3 non appliquée : les écritures sensibles restent bloquées.');
        }
    }

    public function audit($action, $module, $targetType, $targetId, $before, $after, $reason)
    {
        $this->requireAuditReady();
        $reason = $this->requireReason($reason);
        $beforeJson = $before === null ? null : json_encode($before, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        $afterJson = $after === null ? null : json_encode($after, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        $ip = isset($_SERVER['REMOTE_ADDR']) ? substr((string)$_SERVER['REMOTE_ADDR'], 0, 45) : null;
        $target = $targetId === null ? null : substr((string)$targetId, 0, 96);

        $this->DB->PreparedAffect(
            'INSERT INTO cms_admin_audit_log (staff_id,staff_username,action,module,target_type,target_id,before_data,after_data,reason,ip_address,created_at) VALUES (?,?,?,?,?,?,?,?,?,?,?)',
            'isssssssssi',
            array(
                (int)$this->staff['id'],
                substr((string)$this->staff['username'], 0, 64),
                substr((string)$action, 0, 64),
                substr((string)$module, 0, 40),
                substr((string)$targetType, 0, 40),
                $target,
                $beforeJson,
                $afterJson,
                substr($reason, 0, 500),
                $ip,
                time()
            )
        );
    }

    public function userById($userId)
    {
        return $this->DB->PreparedRow('SELECT id,username,look,rank,online,credits,account_created FROM users WHERE id=? LIMIT 1', 'i', array((int)$userId));
    }

    public function requireUser($userId)
    {
        $row = $this->userById($userId);
        if (!$row) throw new RuntimeException('Joueur introuvable.');
        return $row;
    }

    public function requireOfflineUser($userId, $operation = 'Cette modification')
    {
        $user = $this->requireUser($userId);
        if ((int)$user['online'] === 1) {
            throw new RuntimeException($operation . ' est bloquée tant que le joueur est connecté afin d’éviter une désynchronisation avec l’ÉMU.');
        }
        return $user;
    }

    public function requireReason($value, $minLength = 3)
    {
        $reason = trim((string)$value);
        if (mb_strlen($reason) < $minLength) throw new RuntimeException('Une raison administrative est obligatoire.');
        return mb_substr($reason, 0, 500);
    }

    public function avatarUrl($look, $size = 'm')
    {
        $size = in_array($size, array('s','m','l'), true) ? $size : 'm';
        return 'https://www.habbo.es/habbo-imaging/avatarimage?figure=' . rawurlencode((string)$look) . '&size=' . $size . '&direction=2&head_direction=3&gesture=sml';
    }

    public function formatDate($value)
    {
        if ($value === null || $value === '') return '—';
        if (is_numeric($value)) return date('d/m/Y H:i', (int)$value);
        $ts = strtotime((string)$value);
        return $ts ? date('d/m/Y H:i', $ts) : '—';
    }

    public function setFlash($type, $message)
    {
        $_SESSION['pcc_flash'] = array(
            'type' => in_array($type, array('success','error','warning','info'), true) ? $type : 'info',
            'message' => mb_substr((string)$message, 0, 600)
        );
    }

    public function pullFlash()
    {
        $flash = isset($_SESSION['pcc_flash']) ? $_SESSION['pcc_flash'] : null;
        unset($_SESSION['pcc_flash']);
        return $flash;
    }

    public function jsonResponse($payload, $status = 200)
    {
        http_response_code((int)$status);
        header('Content-Type: application/json; charset=utf-8');
        header('Cache-Control: no-store, max-age=0');
        echo json_encode($payload, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        exit;
    }

    private function ensureSecuritySession()
    {
        if (empty($_SESSION['pcc_csrf'])) $_SESSION['pcc_csrf'] = bin2hex(random_bytes(32));
        if (!isset($_SESSION['pcc_nonces']) || !is_array($_SESSION['pcc_nonces'])) $_SESSION['pcc_nonces'] = array();
        if (empty($_SESSION['pcc_rotated_at']) || (time() - (int)$_SESSION['pcc_rotated_at']) > 1800) {
            session_regenerate_id(true);
            $_SESSION['pcc_rotated_at'] = time();
        }
    }

    private function pruneNonces()
    {
        $now = time();
        foreach ($_SESSION['pcc_nonces'] as $nonce => $expires) {
            if ((int)$expires < $now) unset($_SESSION['pcc_nonces'][$nonce]);
        }
        if (count($_SESSION['pcc_nonces']) > 100) {
            $_SESSION['pcc_nonces'] = array_slice($_SESSION['pcc_nonces'], -100, null, true);
        }
    }
}

<?php
/**
 * Paradise Control Center
 * Security and data-access helpers for the existing PixelZone PHP stack.
 * Keeps Habbo ranks as the source of staff identity while centralising the
 * permissions used by the administration.
 */
class AdminControlCenter
{
    private $DB;
    private $staff;
    private $tableCache = array();
    private $columnCache = array();
    private $capabilities = array(
        'admin.access' => 3,
        'admin.dashboard.view' => 3,
        'admin.players.view' => 3,
        'admin.staff.view' => 3,
        'admin.businesses.view' => 3,
        'admin.sanctions.view' => 3,
        'admin.sanctions.manage' => 5,
        'admin.players.appearance.edit' => 5,
        'admin.players.badges.edit' => 5,
        'admin.economy.view' => 3,
        'admin.economy.adjust' => 6,
        'admin.logs.view' => 5,
        'admin.permissions.view' => 5,
        'admin.tools.view' => 5,
        'admin.catalogue.edit' => 6,
        'admin.settings.view' => 5,
        'admin.settings.maintenance' => 6
    );

    public function __construct(DBManager $DB, array $staff)
    {
        $this->DB = $DB;
        $this->staff = $staff;
        $this->ensureSecuritySession();
    }

    public function staff()
    {
        return $this->staff;
    }

    public function can($capability)
    {
        if (!isset($this->capabilities[$capability])) return false;
        return (int)$this->staff['rank'] >= (int)$this->capabilities[$capability];
    }

    public function capabilityMap()
    {
        return $this->capabilities;
    }

    public function requireCapability($capability)
    {
        if (!$this->can($capability)) {
            http_response_code(403);
            throw new RuntimeException('Vous n\'avez pas la permission requise pour cette action.');
        }
    }

    public function roleName($rank)
    {
        $rank = (int)$rank;
        if ($rank >= 7) return 'Fondateur';
        if ($rank === 6) return 'Développeur';
        if ($rank === 5) return 'Administrateur';
        if ($rank === 4) return 'Modérateur';
        if ($rank === 3) return 'Assistant';
        return 'Joueur';
    }

    public function csrfToken()
    {
        return $_SESSION['pcc_csrf'];
    }

    public function validateCsrf($token)
    {
        return is_string($token) && hash_equals($_SESSION['pcc_csrf'], $token);
    }

    public function issueNonce()
    {
        $this->pruneNonces();
        $nonce = bin2hex(random_bytes(24));
        $_SESSION['pcc_nonces'][$nonce] = time() + 1800;
        return $nonce;
    }

    public function consumeNonce($nonce)
    {
        $this->pruneNonces();
        if (!is_string($nonce) || !isset($_SESSION['pcc_nonces'][$nonce])) return false;
        unset($_SESSION['pcc_nonces'][$nonce]);
        return true;
    }

    public function requirePostSecurity(array $post)
    {
        if (!$this->validateCsrf(isset($post['csrf']) ? $post['csrf'] : null)) {
            throw new RuntimeException('Jeton CSRF invalide. Rechargez la page puis réessayez.');
        }
        if (!$this->consumeNonce(isset($post['action_nonce']) ? $post['action_nonce'] : null)) {
            throw new RuntimeException('Cette action a déjà été envoyée ou a expiré. Rechargez la page.');
        }
    }

    public function setFlash($type, $message)
    {
        $_SESSION['pcc_flash'] = array(
            'type' => in_array($type, array('success', 'error', 'warning', 'info'), true) ? $type : 'info',
            'message' => substr((string)$message, 0, 500)
        );
    }

    public function pullFlash()
    {
        $flash = isset($_SESSION['pcc_flash']) ? $_SESSION['pcc_flash'] : null;
        unset($_SESSION['pcc_flash']);
        return $flash;
    }

    public function tableExists($table)
    {
        if (!preg_match('/^[a-zA-Z0-9_]+$/', $table)) return false;
        if (array_key_exists($table, $this->tableCache)) return $this->tableCache[$table];
        $result = $this->DB->PreparedResult(
            'SELECT 1 FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = ? LIMIT 1',
            's',
            array($table)
        );
        $this->tableCache[$table] = mysqli_num_rows($result) === 1;
        return $this->tableCache[$table];
    }

    public function columnExists($table, $column)
    {
        if (!preg_match('/^[a-zA-Z0-9_]+$/', $table) || !preg_match('/^[a-zA-Z0-9_]+$/', $column)) return false;
        $key = $table . '.' . $column;
        if (array_key_exists($key, $this->columnCache)) return $this->columnCache[$key];
        $result = $this->DB->PreparedResult(
            'SELECT 1 FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = ? AND column_name = ? LIMIT 1',
            'ss',
            array($table, $column)
        );
        $this->columnCache[$key] = mysqli_num_rows($result) === 1;
        return $this->columnCache[$key];
    }

    public function auditReady()
    {
        return $this->tableExists('cms_admin_audit_log');
    }

    public function requireAuditReady()
    {
        if (!$this->auditReady()) {
            throw new RuntimeException('Migration Paradise Control Center non appliquée : les actions sensibles restent volontairement bloquées tant que le journal d’audit n’est pas disponible.');
        }
    }

    public function audit($action, $targetType, $targetId, $before, $after, $reason)
    {
        $this->requireAuditReady();
        $reason = trim((string)$reason);
        if ($reason === '') $reason = 'Action administrative';

        $beforeJson = $before === null ? null : json_encode($before, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        $afterJson = $after === null ? null : json_encode($after, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        $ip = isset($_SERVER['REMOTE_ADDR']) ? substr((string)$_SERVER['REMOTE_ADDR'], 0, 45) : null;
        $staffId = isset($this->staff['id']) ? (int)$this->staff['id'] : 0;
        $staffUsername = substr((string)$this->staff['username'], 0, 64);

        $this->DB->PreparedAffect(
            'INSERT INTO cms_admin_audit_log (staff_id, staff_username, action, target_type, target_id, before_data, after_data, reason, ip_address, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)',
            'issssssssi',
            array(
                $staffId,
                $staffUsername,
                substr((string)$action, 0, 64),
                substr((string)$targetType, 0, 40),
                $targetId === null ? null : substr((string)$targetId, 0, 80),
                $beforeJson,
                $afterJson,
                substr($reason, 0, 500),
                $ip,
                time()
            )
        );
    }

    public function avatarUrl($look, $size = 'm')
    {
        $size = in_array($size, array('s', 'm', 'l'), true) ? $size : 'm';
        return 'https://www.habbo.es/habbo-imaging/avatarimage?figure=' . rawurlencode((string)$look) . '&size=' . $size . '&direction=2&head_direction=3&gesture=sml';
    }

    public function formatDate($timestamp)
    {
        if (!$timestamp || !is_numeric($timestamp)) return '—';
        return date('d/m/Y H:i', (int)$timestamp);
    }

    public function requireReason($value, $minLength = 3)
    {
        $reason = trim((string)$value);
        if (mb_strlen($reason) < $minLength) {
            throw new RuntimeException('Une raison administrative est obligatoire.');
        }
        return mb_substr($reason, 0, 500);
    }

    public function jsonResponse($data, $status = 200)
    {
        http_response_code((int)$status);
        header('Content-Type: application/json; charset=utf-8');
        header('Cache-Control: no-store, max-age=0');
        echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        exit;
    }

    private function ensureSecuritySession()
    {
        if (empty($_SESSION['pcc_csrf'])) {
            $_SESSION['pcc_csrf'] = bin2hex(random_bytes(32));
        }
        if (!isset($_SESSION['pcc_nonces']) || !is_array($_SESSION['pcc_nonces'])) {
            $_SESSION['pcc_nonces'] = array();
        }
        if (empty($_SESSION['pcc_session_rotated'])) {
            session_regenerate_id(true);
            $_SESSION['pcc_session_rotated'] = time();
        }
    }

    private function pruneNonces()
    {
        $now = time();
        foreach ($_SESSION['pcc_nonces'] as $nonce => $expiresAt) {
            if ((int)$expiresAt < $now) unset($_SESSION['pcc_nonces'][$nonce]);
        }
        if (count($_SESSION['pcc_nonces']) > 80) {
            $_SESSION['pcc_nonces'] = array_slice($_SESSION['pcc_nonces'], -80, null, true);
        }
    }
}

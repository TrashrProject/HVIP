<?php
/** ParadiseRP Phase 4 — server-authoritative ParadisePhone helpers.
 * SMS persistence deliberately reuses the existing play_phone_chats table used
 * by the emulator PhoneChatManager. No second SMS store is introduced.
 */

function pr_phone_table_exists(mysqli $con, string $table): bool
{
    if (!preg_match('/^[a-z0-9_]+$/i', $table)) return false;
    $safe = mysqli_real_escape_string($con, $table);
    $r = @mysqli_query($con, "SHOW TABLES LIKE '{$safe}'");
    $ok = $r && mysqli_num_rows($r) > 0;
    if ($r) mysqli_free_result($r);
    return $ok;
}

function pr_phone_has_column(mysqli $con, string $table, string $column): bool
{
    $t = mysqli_real_escape_string($con, $table);
    $c = mysqli_real_escape_string($con, $column);
    $r = @mysqli_query($con, "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='{$t}' AND COLUMN_NAME='{$c}' LIMIT 1");
    $ok = $r && mysqli_num_rows($r) > 0;
    if ($r) mysqli_free_result($r);
    return $ok;
}

function pr_phone_has_device(mysqli $con, int $userId): bool
{
    if (!pr_phone_table_exists($con, 'rp_inventory_items') || !pr_phone_table_exists($con, 'rp_item_definitions')) return false;
    $sql = "SELECT 1 FROM rp_inventory_items i INNER JOIN rp_item_definitions d ON d.id=i.item_definition_id WHERE i.owner_user_id=? AND i.quantity>0 AND (UPPER(d.effect_type)='PHONE' OR UPPER(d.code)='PHONE_BASIC') LIMIT 1";
    $stmt = mysqli_prepare($con, $sql);
    if (!$stmt) return false;
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $ok = $res && mysqli_num_rows($res) > 0;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $ok;
}

function pr_phone_number_candidate(): string
{
    try { $n = random_int(0, 9999); }
    catch (Throwable $e) { $n = mt_rand(0, 9999); }
    return '555-' . str_pad((string)$n, 4, '0', STR_PAD_LEFT);
}

function pr_phone_row_by_user(mysqli $con, int $userId): ?array
{
    if (!pr_phone_table_exists($con, 'rp_phones')) return null;
    $stmt = mysqli_prepare($con, "SELECT * FROM rp_phones WHERE user_id=? AND status='ACTIVE' LIMIT 1");
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row;
}

function pr_phone_row_by_id(mysqli $con, int $phoneId): ?array
{
    $stmt = mysqli_prepare($con, "SELECT * FROM rp_phones WHERE id=? AND status='ACTIVE' LIMIT 1");
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'i', $phoneId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row;
}

function pr_phone_row_by_number(mysqli $con, string $number): ?array
{
    $number = trim($number);
    $stmt = mysqli_prepare($con, "SELECT * FROM rp_phones WHERE phone_number=? AND status='ACTIVE' LIMIT 1");
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 's', $number);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row;
}

function pr_phone_ensure(mysqli $con, int $userId): ?array
{
    $existing = pr_phone_row_by_user($con, $userId);
    if ($existing) return $existing;
    if (!pr_phone_has_device($con, $userId)) return null;

    for ($attempt = 0; $attempt < 50; $attempt++) {
        $number = pr_phone_number_candidate();
        $device = 'PI-' . strtoupper(substr(hash('sha256', $userId . '|' . microtime(true) . '|' . $attempt), 0, 20));
        $stmt = mysqli_prepare($con, "INSERT IGNORE INTO rp_phones (user_id,phone_number,device_identifier,status) VALUES (?,?,?,'ACTIVE')");
        if (!$stmt) return null;
        mysqli_stmt_bind_param($stmt, 'iss', $userId, $number, $device);
        mysqli_stmt_execute($stmt);
        $created = mysqli_stmt_affected_rows($stmt) > 0;
        mysqli_stmt_close($stmt);
        if ($created) return pr_phone_row_by_user($con, $userId);
        $existing = pr_phone_row_by_user($con, $userId);
        if ($existing) return $existing;
    }
    return null;
}

function pr_phone_identity(mysqli $con, int $userId): array
{
    $stmt = mysqli_prepare($con, 'SELECT u.username,u.look,c.first_name,c.last_name FROM users u LEFT JOIN rp_characters c ON c.user_id=u.id WHERE u.id=? LIMIT 1');
    if (!$stmt) return ['username'=>null,'name'=>null,'look'=>null];
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    if (!$row) return ['username'=>null,'name'=>null,'look'=>null];
    $name = trim((string)($row['first_name'] ?? '') . ' ' . (string)($row['last_name'] ?? ''));
    return ['username'=>(string)$row['username'],'name'=>$name !== '' ? $name : (string)$row['username'],'look'=>(string)$row['look']];
}

function pr_phone_is_online(mysqli $con, int $userId): bool
{
    if (!pr_phone_has_column($con, 'users', 'online')) return false;
    $stmt = mysqli_prepare($con, 'SELECT online FROM users WHERE id=? LIMIT 1');
    if (!$stmt) return false;
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row ? ((int)$row['online'] === 1) : false;
}

function pr_phone_resolve(mysqli $con, array $ownerPhone, string $token): ?array
{
    $token = trim($token);
    if ($token === '') return null;
    $byNumber = pr_phone_row_by_number($con, $token);
    if ($byNumber) return $byNumber;
    $stmt = mysqli_prepare($con, "SELECT p.* FROM rp_phone_contacts c INNER JOIN rp_phones p ON p.phone_number=c.contact_phone_number WHERE c.phone_id=? AND LOWER(c.display_name)=LOWER(?) AND p.status='ACTIVE' LIMIT 1");
    if (!$stmt) return null;
    $phoneId = (int)$ownerPhone['id'];
    mysqli_stmt_bind_param($stmt, 'is', $phoneId, $token);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row;
}

function pr_phone_rate_limited(mysqli $con, int $phoneId, string $action, int $seconds, int $max): bool
{
    $stmt = mysqli_prepare($con, 'SELECT COUNT(*) AS c FROM rp_phone_action_log WHERE phone_id=? AND action_type=? AND created_at >= DATE_SUB(NOW(), INTERVAL ? SECOND)');
    if (!$stmt) return false;
    mysqli_stmt_bind_param($stmt, 'isi', $phoneId, $action, $seconds);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? mysqli_fetch_assoc($res) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row && (int)$row['c'] >= $max;
}

function pr_phone_log_action(mysqli $con, int $phoneId, string $action, ?int $targetPhoneId=null): void
{
    $stmt = mysqli_prepare($con, 'INSERT INTO rp_phone_action_log (phone_id,action_type,target_phone_id) VALUES (?,?,?)');
    if (!$stmt) return;
    mysqli_stmt_bind_param($stmt, 'isi', $phoneId, $action, $targetPhoneId);
    @mysqli_stmt_execute($stmt);
    mysqli_stmt_close($stmt);
}

function pr_phone_contacts(mysqli $con, array $phone): array
{
    $sql = "SELECT c.id,c.display_name,c.contact_phone_number,p.user_id FROM rp_phone_contacts c LEFT JOIN rp_phones p ON p.phone_number=c.contact_phone_number AND p.status='ACTIVE' WHERE c.phone_id=? ORDER BY c.display_name";
    $stmt = mysqli_prepare($con, $sql);
    if (!$stmt) return [];
    $phoneId = (int)$phone['id'];
    mysqli_stmt_bind_param($stmt, 'i', $phoneId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $out = [];
    while ($res && ($row = mysqli_fetch_assoc($res))) {
        $identity = $row['user_id'] ? pr_phone_identity($con, (int)$row['user_id']) : ['username'=>null,'name'=>null,'look'=>null];
        $out[] = [
            'id'=>(int)$row['id'], 'name'=>(string)$row['display_name'], 'number'=>(string)$row['contact_phone_number'],
            'user_id'=>$row['user_id'] ? (int)$row['user_id'] : null, 'username'=>$identity['username'], 'look'=>$identity['look'],
            'online'=>$row['user_id'] ? pr_phone_is_online($con, (int)$row['user_id']) : false
        ];
    }
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $out;
}

function pr_phone_contact_name(mysqli $con, int $phoneId, string $number): ?string
{
    $stmt = mysqli_prepare($con, 'SELECT display_name FROM rp_phone_contacts WHERE phone_id=? AND contact_phone_number=? LIMIT 1');
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'is', $phoneId, $number);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row ? (string)$row['display_name'] : null;
}

function pr_phone_conversations(mysqli $con, array $phone): array
{
    $selfUserId = (int)$phone['user_id'];
    $selfPhoneId = (int)$phone['id'];
    $sql = "SELECT x.other_user_id,x.last_id,m.msg,m.timestamp,COALESCE(u.unread,0) unread
            FROM (
              SELECT CASE WHEN emisor_id=? THEN receptor_id ELSE emisor_id END other_user_id, MAX(id) last_id
              FROM play_phone_chats
              WHERE type=1 AND (emisor_id=? OR receptor_id=?)
              GROUP BY other_user_id
            ) x
            INNER JOIN play_phone_chats m ON m.id=x.last_id
            LEFT JOIN (
              SELECT emisor_id,COUNT(*) unread FROM play_phone_chats
              WHERE type=1 AND receptor_id=? AND read_at IS NULL GROUP BY emisor_id
            ) u ON u.emisor_id=x.other_user_id
            ORDER BY m.timestamp DESC LIMIT 50";
    $stmt = mysqli_prepare($con, $sql);
    if (!$stmt) return [];
    mysqli_stmt_bind_param($stmt, 'iiii', $selfUserId, $selfUserId, $selfUserId, $selfUserId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $out = [];
    while ($res && ($row = mysqli_fetch_assoc($res))) {
        $otherPhone = pr_phone_row_by_user($con, (int)$row['other_user_id']);
        if (!$otherPhone) continue;
        $identity = pr_phone_identity($con, (int)$row['other_user_id']);
        $contactName = pr_phone_contact_name($con, $selfPhoneId, (string)$otherPhone['phone_number']);
        $out[] = [
            'phone_id'=>(int)$otherPhone['id'], 'number'=>(string)$otherPhone['phone_number'],
            'name'=>$contactName ?: ($identity['name'] ?: (string)$otherPhone['phone_number']), 'username'=>$identity['username'], 'look'=>$identity['look'],
            'last_message'=>(string)$row['msg'], 'last_at'=>(string)$row['timestamp'], 'unread'=>(int)$row['unread']
        ];
    }
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $out;
}

function pr_phone_messages(mysqli $con, int $selfPhoneId, int $otherPhoneId, int $limit=30): array
{
    $self = pr_phone_row_by_id($con, $selfPhoneId);
    $other = pr_phone_row_by_id($con, $otherPhoneId);
    if (!$self || !$other) return [];
    $selfUser = (int)$self['user_id'];
    $otherUser = (int)$other['user_id'];
    $limit = max(1, min(50, $limit));
    $sql = "SELECT id,emisor_id,receptor_id,msg,timestamp,read_at FROM play_phone_chats
            WHERE type=1 AND ((emisor_id=? AND receptor_id=?) OR (emisor_id=? AND receptor_id=?))
            ORDER BY id DESC LIMIT {$limit}";
    $stmt = mysqli_prepare($con, $sql);
    if (!$stmt) return [];
    mysqli_stmt_bind_param($stmt, 'iiii', $selfUser, $otherUser, $otherUser, $selfUser);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $out = [];
    while ($res && ($row = mysqli_fetch_assoc($res))) {
        $out[] = [
            'id'=>(int)$row['id'], 'mine'=>(int)$row['emisor_id'] === $selfUser,
            'body'=>(string)$row['msg'], 'sent_at'=>(string)$row['timestamp'], 'read_at'=>$row['read_at']
        ];
    }
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return array_reverse($out);
}

function pr_phone_mark_read(mysqli $con, array $selfPhone, int $otherPhoneId): bool
{
    $other = pr_phone_row_by_id($con, $otherPhoneId);
    if (!$other) return false;
    $selfUser = (int)$selfPhone['user_id'];
    $otherUser = (int)$other['user_id'];
    $stmt = mysqli_prepare($con, "UPDATE play_phone_chats SET read_at=COALESCE(read_at,NOW()),status='READ' WHERE type=1 AND receptor_id=? AND emisor_id=? AND read_at IS NULL");
    if (!$stmt) return false;
    mysqli_stmt_bind_param($stmt, 'ii', $selfUser, $otherUser);
    mysqli_stmt_execute($stmt);
    mysqli_stmt_close($stmt);
    return true;
}

function pr_phone_active_call(mysqli $con, int $phoneId): ?array
{
    $stmt = mysqli_prepare($con, "SELECT * FROM rp_phone_calls WHERE (caller_phone_id=? OR receiver_phone_id=?) AND status IN ('RINGING','CONNECTED') ORDER BY id DESC LIMIT 1");
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'ii', $phoneId, $phoneId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $row = $res ? (mysqli_fetch_assoc($res) ?: null) : null;
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $row;
}

function pr_phone_notifications(mysqli $con, int $phoneId): array
{
    $stmt = mysqli_prepare($con, 'SELECT id,notification_type,title,body,created_at,read_at FROM rp_phone_notifications WHERE phone_id=? ORDER BY id DESC LIMIT 30');
    if (!$stmt) return [];
    mysqli_stmt_bind_param($stmt, 'i', $phoneId);
    mysqli_stmt_execute($stmt);
    $res = mysqli_stmt_get_result($stmt);
    $out = [];
    while ($res && ($row = mysqli_fetch_assoc($res))) {
        $out[] = ['id'=>(int)$row['id'],'type'=>(string)$row['notification_type'],'title'=>(string)$row['title'],'body'=>(string)$row['body'],'created_at'=>(string)$row['created_at'],'read'=>!is_null($row['read_at'])];
    }
    if ($res) mysqli_free_result($res);
    mysqli_stmt_close($stmt);
    return $out;
}

function pr_phone_snapshot(mysqli $con, int $userId): array
{
    $hasDevice = pr_phone_has_device($con, $userId);
    $phone = $hasDevice ? pr_phone_ensure($con, $userId) : pr_phone_row_by_user($con, $userId);
    if (!$hasDevice || !$phone) {
        return ['available'=>false,'has_device'=>$hasDevice,'number'=>null,'contacts'=>[],'conversations'=>[],'unread_count'=>0,'active_call'=>null,'notifications'=>[],'settings'=>['silent'=>false,'notifications'=>true,'sounds'=>true]];
    }

    $phoneId = (int)$phone['id'];
    $stmt = mysqli_prepare($con, 'SELECT COUNT(*) c FROM play_phone_chats WHERE type=1 AND receptor_id=? AND read_at IS NULL');
    $unread = 0;
    if ($stmt) {
        mysqli_stmt_bind_param($stmt, 'i', $userId);
        mysqli_stmt_execute($stmt);
        $res = mysqli_stmt_get_result($stmt);
        $row = $res ? mysqli_fetch_assoc($res) : null;
        $unread = $row ? (int)$row['c'] : 0;
        if ($res) mysqli_free_result($res);
        mysqli_stmt_close($stmt);
    }

    $call = pr_phone_active_call($con, $phoneId);
    $callView = null;
    if ($call) {
        $otherId = (int)$call['caller_phone_id'] === $phoneId ? (int)$call['receiver_phone_id'] : (int)$call['caller_phone_id'];
        $other = pr_phone_row_by_id($con, $otherId);
        $identity = $other ? pr_phone_identity($con, (int)$other['user_id']) : ['name'=>null,'look'=>null,'username'=>null];
        $callView = [
            'id'=>(int)$call['id'], 'status'=>(string)$call['status'],
            'direction'=>(int)$call['caller_phone_id'] === $phoneId ? 'outgoing' : 'incoming',
            'other_phone_id'=>$otherId, 'other_number'=>$other ? (string)$other['phone_number'] : null,
            'other_name'=>$identity['name'], 'other_username'=>$identity['username'], 'other_look'=>$identity['look'],
            'started_at'=>(string)$call['started_at'], 'answered_at'=>$call['answered_at']
        ];
    }

    return [
        'available'=>true,'has_device'=>true,'id'=>$phoneId,'number'=>(string)$phone['phone_number'],
        'contacts'=>pr_phone_contacts($con,$phone),'conversations'=>pr_phone_conversations($con,$phone),'unread_count'=>$unread,
        'active_call'=>$callView,'notifications'=>pr_phone_notifications($con,$phoneId),
        'settings'=>['silent'=>(bool)$phone['silent_mode'],'notifications'=>(bool)$phone['notifications_enabled'],'sounds'=>(bool)$phone['sounds_enabled']]
    ];
}

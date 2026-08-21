<?php
/**
 * ParadiseRP Phase 2 shared Character/Document helpers.
 * This file never trusts client-provided user ids: callers pass the authenticated
 * account id resolved from the CMS session.
 */

function pr_character_table_exists(mysqli $con, string $table): bool
{
    if (!preg_match('/^[a-z0-9_]+$/i', $table)) return false;
    $safe = mysqli_real_escape_string($con, $table);
    $result = @mysqli_query($con, "SHOW TABLES LIKE '{$safe}'");
    $exists = $result && mysqli_num_rows($result) > 0;
    if ($result) mysqli_free_result($result);
    return $exists;
}

function pr_character_age(?string $birthDate): ?int
{
    if (!$birthDate) return null;
    try {
        $birth = new DateTimeImmutable($birthDate);
        $today = new DateTimeImmutable('today');
        return (int)$birth->diff($today)->y;
    } catch (Throwable $e) {
        return null;
    }
}

function pr_character_clean_name($value): string
{
    $value = trim(preg_replace('/\s+/u', ' ', (string)$value));
    return $value;
}

function pr_character_clean_bio($value): string
{
    $value = trim(preg_replace('/\s+/u', ' ', strip_tags((string)$value)));
    return $value;
}

function pr_character_generate_number(string $prefix, int $bytes = 4): string
{
    try {
        return strtoupper($prefix . '-' . bin2hex(random_bytes($bytes)));
    } catch (Throwable $e) {
        return strtoupper($prefix . '-' . substr(sha1(uniqid('', true) . mt_rand()), 0, $bytes * 2));
    }
}

function pr_character_row(mysqli $con, int $userId): ?array
{
    if (!pr_character_table_exists($con, 'rp_characters')) return null;
    $stmt = mysqli_prepare($con, 'SELECT `id`,`user_id`,`citizen_id`,`first_name`,`last_name`,`birth_date`,`gender`,`nationality`,`biography`,`reputation`,`created_at`,`updated_at` FROM `rp_characters` WHERE `user_id` = ? LIMIT 1');
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $result = mysqli_stmt_get_result($stmt);
    $row = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
    if ($result) mysqli_free_result($result);
    mysqli_stmt_close($stmt);
    return $row;
}

function pr_character_documents(mysqli $con, int $userId): array
{
    if (!pr_character_table_exists($con, 'rp_player_documents') || !pr_character_table_exists($con, 'rp_document_types')) return [];
    $stmt = mysqli_prepare($con, 'SELECT d.`id`,d.`document_number`,d.`issued_at`,d.`expires_at`,d.`status`,d.`metadata`,t.`code`,t.`name`,t.`category`,t.`expires` FROM `rp_player_documents` d INNER JOIN `rp_document_types` t ON t.`id` = d.`document_type_id` WHERE d.`user_id` = ? ORDER BY d.`issued_at` ASC');
    if (!$stmt) return [];
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $result = mysqli_stmt_get_result($stmt);
    $documents = [];
    if ($result) {
        while ($row = mysqli_fetch_assoc($result)) {
            $documents[] = [
                'id' => (int)$row['id'],
                'type' => (string)$row['code'],
                'name' => (string)$row['name'],
                'category' => (string)$row['category'],
                'number' => (string)$row['document_number'],
                'status' => strtoupper((string)$row['status']),
                'issued_at' => (string)$row['issued_at'],
                'expires_at' => $row['expires_at'] !== null ? (string)$row['expires_at'] : null,
                'can_expire' => (bool)$row['expires'],
                'metadata' => $row['metadata'] !== null ? (string)$row['metadata'] : null,
            ];
        }
        mysqli_free_result($result);
    }
    mysqli_stmt_close($stmt);
    return $documents;
}

function pr_character_offer(mysqli $con, int $targetUserId): ?array
{
    if (!pr_character_table_exists($con, 'rp_document_shares') || !pr_character_table_exists($con, 'rp_player_documents') || !pr_character_table_exists($con, 'rp_document_types')) return null;
    if (!pr_character_table_exists($con, 'rp_characters') || !pr_character_table_exists($con, 'users')) return null;

    $sql = "SELECT s.`id` AS share_id,s.`sender_user_id`,s.`created_at`,s.`expires_at`,d.`document_number`,d.`issued_at`,d.`expires_at` AS document_expires_at,d.`status` AS document_status,t.`code` AS document_type,t.`name` AS document_name,c.`first_name`,c.`last_name`,c.`birth_date`,c.`nationality`,c.`citizen_id`,u.`username`,u.`look` FROM `rp_document_shares` s INNER JOIN `rp_player_documents` d ON d.`id` = s.`player_document_id` INNER JOIN `rp_document_types` t ON t.`id` = d.`document_type_id` INNER JOIN `rp_characters` c ON c.`user_id` = s.`sender_user_id` INNER JOIN `users` u ON u.`id` = s.`sender_user_id` WHERE s.`target_user_id` = ? AND s.`status` = 'PENDING' AND s.`expires_at` > NOW() ORDER BY s.`id` DESC LIMIT 1";
    $stmt = mysqli_prepare($con, $sql);
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'i', $targetUserId);
    mysqli_stmt_execute($stmt);
    $result = mysqli_stmt_get_result($stmt);
    $row = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
    if ($result) mysqli_free_result($result);
    mysqli_stmt_close($stmt);
    if (!$row) return null;

    return [
        'id' => (int)$row['share_id'],
        'sender' => [
            'username' => (string)$row['username'],
            'name' => trim((string)$row['first_name'] . ' ' . (string)$row['last_name']),
            'look' => (string)$row['look'],
        ],
        'document' => [
            'type' => (string)$row['document_type'],
            'name' => (string)$row['document_name'],
            'number' => (string)$row['document_number'],
            'status' => strtoupper((string)$row['document_status']),
            'issued_at' => (string)$row['issued_at'],
            'expires_at' => $row['document_expires_at'] !== null ? (string)$row['document_expires_at'] : null,
        ],
        'identity' => [
            'first_name' => (string)$row['first_name'],
            'last_name' => (string)$row['last_name'],
            'birth_date' => (string)$row['birth_date'],
            'age' => pr_character_age((string)$row['birth_date']),
            'nationality' => (string)$row['nationality'],
            'citizen_id' => (string)$row['citizen_id'],
        ],
        'created_at' => (string)$row['created_at'],
        'expires_at' => (string)$row['expires_at'],
    ];
}

function pr_character_ui_event(mysqli $con, int $userId): ?array
{
    if (!pr_character_table_exists($con, 'rp_ui_events')) return null;
    $stmt = mysqli_prepare($con, "SELECT `id`,`event_type`,`payload`,`created_at` FROM `rp_ui_events` WHERE `user_id` = ? AND `status` = 'PENDING' AND `expires_at` > NOW() ORDER BY `id` DESC LIMIT 1");
    if (!$stmt) return null;
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $result = mysqli_stmt_get_result($stmt);
    $row = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
    if ($result) mysqli_free_result($result);
    mysqli_stmt_close($stmt);
    if (!$row) return null;
    parse_str((string)$row['payload'], $payload);
    return [
        'id' => (int)$row['id'],
        'type' => (string)$row['event_type'],
        'payload' => is_array($payload) ? $payload : [],
        'created_at' => (string)$row['created_at'],
    ];
}

function pr_character_snapshot(mysqli $con, int $userId): array
{
    $row = pr_character_row($con, $userId);
    if (!$row) {
        return [
            'exists' => false,
            'first_name' => null,
            'last_name' => null,
            'full_name' => null,
            'birth_date' => null,
            'age' => null,
            'gender' => null,
            'nationality' => null,
            'citizen_id' => null,
            'biography' => null,
            'reputation' => 0,
            'created_at' => null,
            'updated_at' => null,
        ];
    }

    return [
        'exists' => true,
        'first_name' => (string)$row['first_name'],
        'last_name' => (string)$row['last_name'],
        'full_name' => trim((string)$row['first_name'] . ' ' . (string)$row['last_name']),
        'birth_date' => (string)$row['birth_date'],
        'age' => pr_character_age((string)$row['birth_date']),
        'gender' => $row['gender'] !== null ? (string)$row['gender'] : null,
        'nationality' => (string)$row['nationality'],
        'citizen_id' => (string)$row['citizen_id'],
        'biography' => $row['biography'] !== null ? (string)$row['biography'] : '',
        'reputation' => (int)$row['reputation'],
        'created_at' => (string)$row['created_at'],
        'updated_at' => (string)$row['updated_at'],
    ];
}

function pr_character_validate_identity(array $input, array &$errors): ?array
{
    $first = pr_character_clean_name($input['first_name'] ?? '');
    $last = pr_character_clean_name($input['last_name'] ?? '');
    $nationality = pr_character_clean_name($input['nationality'] ?? '');
    $gender = pr_character_clean_name($input['gender'] ?? '');
    $bio = pr_character_clean_bio($input['biography'] ?? '');
    $birthRaw = trim((string)($input['birth_date'] ?? ''));

    if (!preg_match("/^[\\p{L}' -]{2,32}$/u", $first)) $errors[] = 'Prénom invalide (2 à 32 caractères).';
    if (!preg_match("/^[\\p{L}' -]{2,32}$/u", $last)) $errors[] = 'Nom invalide (2 à 32 caractères).';
    if (mb_strlen($nationality) < 2 || mb_strlen($nationality) > 48) $errors[] = 'Nationalité / origine invalide.';
    if (mb_strlen($gender) > 24) $errors[] = 'Genre RP trop long.';
    if (mb_strlen($bio) > 400) $errors[] = 'La biographie est limitée à 400 caractères.';

    $birth = DateTimeImmutable::createFromFormat('!Y-m-d', $birthRaw);
    $birthValid = $birth && $birth->format('Y-m-d') === $birthRaw;
    if (!$birthValid) {
        $errors[] = 'Date de naissance invalide.';
    } else {
        $today = new DateTimeImmutable('today');
        $age = (int)$birth->diff($today)->y;
        if ($birth > $today || $age < 16 || $age > 100) $errors[] = 'Date de naissance incohérente pour un personnage RP.';
    }

    if ($errors) return null;
    return [
        'first_name' => $first,
        'last_name' => $last,
        'birth_date' => $birthRaw,
        'gender' => $gender !== '' ? $gender : null,
        'nationality' => $nationality,
        'biography' => $bio !== '' ? $bio : null,
    ];
}

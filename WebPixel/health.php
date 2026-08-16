<?php
require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)):
    if(isset($_POST['action'])) {
        header('Content-Type: application/json; charset=utf-8');
        http_response_code(401);
        echo json_encode(['ok' => false, 'message' => 'Session expirée.']);
        exit;
    }
    header("Location: " . Config::$URL . "/");
    exit;
endif;

$username = isset($UData['username']) ? $UData['username'] : 'Citoyen';
$userId = isset($UData['id']) ? (int)$UData['id'] : 0;
$look = isset($UData['look']) ? $UData['look'] : '';
$health = isset($UPData['health']) ? max(0, min(100, (int)$UPData['health'])) : 100;
$fatigue = isset($UPData['fatigue']) ? max(0, min(100, (int)$UPData['fatigue'])) : 20;
$energy = isset($UPData['energy']) ? max(0, min(100, (int)$UPData['energy'])) : 58;
$hygiene = isset($UPData['hygiene']) ? max(0, min(100, (int)$UPData['hygiene'])) : 92;
$avatar = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=m&head_direction=3&gesture=sml';

function healthStatusLabel($health) {
    if($health >= 80) return ['Très bonne', 'good'];
    if($health >= 55) return ['Stable', 'okay'];
    if($health >= 30) return ['Fragile', 'warning'];
    return ['Critique', 'critical'];
}

if(isset($_POST['action']) && $_POST['action'] === 'call911'):
    header('Content-Type: application/json; charset=utf-8');

    if($userId <= 0) {
        http_response_code(400);
        echo json_encode(['ok' => false, 'message' => 'Utilisateur introuvable.']);
        exit;
    }

    try {
        $con = $DB->Con();
        $DB->Query("CREATE TABLE IF NOT EXISTS rp_emergency_calls (
            id INT UNSIGNED NOT NULL AUTO_INCREMENT,
            user_id INT NOT NULL,
            username VARCHAR(64) NOT NULL,
            health TINYINT UNSIGNED NOT NULL DEFAULT 100,
            fatigue TINYINT UNSIGNED NOT NULL DEFAULT 0,
            energy TINYINT UNSIGNED NOT NULL DEFAULT 100,
            hygiene TINYINT UNSIGNED NOT NULL DEFAULT 100,
            status ENUM('pending','accepted','closed','cancelled') NOT NULL DEFAULT 'pending',
            created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY idx_user_status (user_id, status),
            KEY idx_status_created (status, created_at)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

        $stmt = mysqli_prepare($con, "SELECT id, created_at FROM rp_emergency_calls WHERE user_id = ? AND status = 'pending' ORDER BY id DESC LIMIT 1");
        mysqli_stmt_bind_param($stmt, 'i', $userId);
        mysqli_stmt_execute($stmt);
        $result = mysqli_stmt_get_result($stmt);
        $existing = mysqli_fetch_assoc($result);
        mysqli_stmt_close($stmt);

        if($existing) {
            echo json_encode([
                'ok' => true,
                'alreadyPending' => true,
                'callId' => (int)$existing['id'],
                'message' => 'Un appel 911 est déjà en attente. Les secours ont été prévenus.'
            ]);
            exit;
        }

        $stmt = mysqli_prepare($con, "INSERT INTO rp_emergency_calls (user_id, username, health, fatigue, energy, hygiene, status) VALUES (?, ?, ?, ?, ?, ?, 'pending')");
        mysqli_stmt_bind_param($stmt, 'isiiii', $userId, $username, $health, $fatigue, $energy, $hygiene);
        mysqli_stmt_execute($stmt);
        $callId = mysqli_insert_id($con);
        mysqli_stmt_close($stmt);

        echo json_encode([
            'ok' => true,
            'callId' => $callId,
            'message' => 'Appel 911 envoyé. Reste sur place : les secours ont reçu ta demande.'
        ]);
        exit;
    } catch (Throwable $e) {
        http_response_code(500);
        echo json_encode(['ok' => false, 'message' => 'Impossible de joindre le 911 pour le moment.']);
        exit;
    }
endif;

$status = healthStatusLabel($health);
$wellness = (int)round(($health + (100 - $fatigue) + $energy + $hygiene) / 4);
?>
<!doctype html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
    <meta name="theme-color" content="#0a1019">
    <title><?php echo Config::$WName; ?> — Santé</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.8.2/css/all.css" crossorigin="anonymous">
    <link rel="stylesheet" href="<?php echo CSS; ?>/phone/health.css?<?php echo time(); ?>">
</head>
<body class="health-app-page">
<div class="health-shell">
    <header class="health-topbar">
        <a class="health-back" href="<?php echo URL; ?>/me" aria-label="Retour"><i class="fas fa-chevron-left"></i></a>
        <div class="health-brand"><span class="health-brand-icon"><i class="fas fa-heartbeat"></i></span><div><strong>Santé</strong><small>VELORA CITY PHONE</small></div></div>
        <span class="health-secure"><i class="fas fa-shield-alt"></i> Données privées</span>
    </header>

    <main class="health-main">
        <section class="health-hero">
            <div class="health-avatar-wrap"><img src="<?php echo htmlspecialchars($avatar, ENT_QUOTES, 'UTF-8'); ?>" alt=""></div>
            <div class="health-hero-copy">
                <span class="health-kicker"><i class="fas fa-circle"></i> DOSSIER SANTÉ ACTIF</span>
                <h1>Bonjour, <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>.</h1>
                <p>Voici ton état actuel. Les valeurs sont synchronisées avec ton personnage RP.</p>
            </div>
            <div class="health-score <?php echo $status[1]; ?>">
                <span>ÉTAT GÉNÉRAL</span>
                <strong><?php echo $wellness; ?></strong>
                <small>/ 100 · <?php echo $status[0]; ?></small>
            </div>
        </section>

        <section class="health-grid">
            <article class="health-metric metric-health">
                <div class="metric-head"><span class="metric-icon"><i class="fas fa-heart"></i></span><div><small>SANTÉ</small><strong><?php echo $health; ?> / 100</strong></div></div>
                <div class="metric-track"><i style="width:<?php echo $health; ?>%"></i></div>
                <p><?php echo $health >= 70 ? 'Tes constantes principales sont bonnes.' : ($health >= 35 ? 'Ton état mérite de la prudence.' : 'Ton état est critique. Demande de l’aide rapidement.'); ?></p>
            </article>
            <article class="health-metric metric-fatigue">
                <div class="metric-head"><span class="metric-icon"><i class="fas fa-bed"></i></span><div><small>FATIGUE</small><strong><?php echo $fatigue; ?> / 100</strong></div></div>
                <div class="metric-track"><i style="width:<?php echo $fatigue; ?>%"></i></div>
                <p><?php echo $fatigue <= 35 ? 'Tu es suffisamment reposé.' : 'Pense à ralentir et à te reposer.'; ?></p>
            </article>
            <article class="health-metric metric-energy">
                <div class="metric-head"><span class="metric-icon"><i class="fas fa-bolt"></i></span><div><small>ÉNERGIE</small><strong><?php echo $energy; ?> / 100</strong></div></div>
                <div class="metric-track"><i style="width:<?php echo $energy; ?>%"></i></div>
                <p><?php echo $energy >= 50 ? 'Ton niveau d’énergie est correct.' : 'Ton niveau d’énergie est faible.'; ?></p>
            </article>
            <article class="health-metric metric-hygiene">
                <div class="metric-head"><span class="metric-icon"><i class="fas fa-tint"></i></span><div><small>HYGIÈNE</small><strong><?php echo $hygiene; ?> / 100</strong></div></div>
                <div class="metric-track"><i style="width:<?php echo $hygiene; ?>%"></i></div>
                <p><?php echo $hygiene >= 60 ? 'Ton hygiène est satisfaisante.' : 'Une douche te ferait du bien.'; ?></p>
            </article>
        </section>

        <section class="emergency-card">
            <div class="emergency-copy">
                <span class="emergency-icon"><i class="fas fa-ambulance"></i></span>
                <div><small>URGENCE MÉDICALE RP</small><h2>Tu ne te sens pas bien ?</h2><p>Appuie sur le bouton pour envoyer immédiatement une demande au 911 de Velora avec tes constantes actuelles.</p></div>
            </div>
            <button id="call-911" class="call-911" type="button"><i class="fas fa-phone"></i><span><small>APPEL D'URGENCE</small><strong>APPELER LE 911</strong></span></button>
        </section>

        <section class="health-advice">
            <div class="health-section-title"><span>CONSEILS</span><strong>Prends soin de ton personnage</strong></div>
            <div class="advice-grid">
                <div><i class="fas fa-utensils"></i><strong>Mange régulièrement</strong><small>Maintiens ton énergie avant une longue activité.</small></div>
                <div><i class="fas fa-moon"></i><strong>Repose-toi</strong><small>Une fatigue élevée peut limiter tes actions RP.</small></div>
                <div><i class="fas fa-shower"></i><strong>Reste propre</strong><small>L'hygiène fait partie de la vie quotidienne à Velora.</small></div>
            </div>
        </section>
    </main>
</div>

<div class="health-modal" id="health-911-modal" aria-hidden="true">
    <div class="health-modal-card" role="dialog" aria-modal="true" aria-labelledby="health-modal-title">
        <button class="health-modal-close" id="health-modal-close" type="button" aria-label="Fermer"><i class="fas fa-times"></i></button>
        <span class="modal-emergency-icon"><i class="fas fa-phone"></i></span>
        <small>VELORA 911</small>
        <h2 id="health-modal-title">Confirmer l'appel d'urgence ?</h2>
        <p>Une demande médicale RP sera créée et ton état actuel sera transmis aux services de secours.</p>
        <div class="modal-actions"><button id="health-modal-cancel" type="button">Annuler</button><button id="health-modal-confirm" class="confirm" type="button"><i class="fas fa-ambulance"></i> Envoyer l'appel</button></div>
    </div>
</div>

<div class="health-toast" id="health-toast" role="status"></div>
<script>window.VELORA_HEALTH_ENDPOINT = <?php echo json_encode(URL . '/health'); ?>;</script>
<script src="<?php echo JS; ?>/health.js?<?php echo time(); ?>"></script>
</body>
</html>

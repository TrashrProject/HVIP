<?php
$h = function ($value) { return htmlspecialchars((string)$value, ENT_QUOTES, 'UTF-8'); };
$number = function ($value) { return number_format((int)$value, 0, ',', ' '); };

$pageTitles = array(
    'dashboard' => array('Vue d’ensemble', 'La situation réelle de ParadiseRP, sans KPI inventé.'),
    'players' => array('Joueurs', 'Comptes Habbo, identité RP, économie et état de connexion.'),
    'player' => array('Fiche joueur', 'Administrer la vie ParadiseRP d’un joueur depuis une fiche centrale.'),
    'documents' => array('Documents', 'Documents RP réellement stockés par Character / Documents V2.'),
    'inventory' => array('Inventaires', 'Inventaires V2, capacité, poids et objets détenus.'),
    'items' => array('Objets', 'Définitions d’objets utilisées par Inventory V2.'),
    'phones' => array('Téléphones', 'ParadisePhone V1 : numéros, appareils, activité et statistiques.'),
    'catalogue' => array('Catalogue', 'Offres réellement présentes dans le catalogue Habbo.'),
    'badges' => array('Badges', 'Badges Habbo et détenteurs.'),
    'businesses' => array('Entreprises', 'Corporations et memberships existants dans le core RP.'),
    'sanctions' => array('Sanctions', 'Bannissements actifs et historique disponible.'),
    'staff' => array('Staff', 'Comptes staff, ranks Habbo et état de connexion.'),
    'logs' => array('Audit Logs', 'Traçabilité avant / après des actions du Control Center.'),
    'permissions' => array('Permissions', 'Correspondance ranks Habbo → capacités administratives.'),
    'settings' => array('Configuration', 'Réglages CMS exposables sans secrets.'),
    'health' => array('Santé système', 'État observable du CMS, de la DB et des signaux ÉMU disponibles.')
);
$currentTitle = $pageTitles[$AdminPage] ?? $pageTitles['dashboard'];
$onlineCount = 0;
try {
    $row = $DB->PreparedRow("SELECT COUNT(*) total FROM users WHERE online='1'");
    $onlineCount = $row ? (int)$row['total'] : 0;
} catch (Throwable $e) {}

$emuSignal = null;
if ($PCC->tableExists('server_status')) {
    try { $emuSignal = $DB->PreparedRow('SELECT * FROM server_status LIMIT 1'); } catch (Throwable $e) { $emuSignal = null; }
}
$staffAvatar = $PCC->avatarUrl($UData['look'] ?? '', 's');
$adminViewDir = VIEW . 'Admin' . DS;
$adminViewFile = $adminViewDir . $AdminPage . '.php';
?>
<div class="pcc-app" data-pcc-app>
    <aside class="pcc-sidebar" data-sidebar>
        <div class="pcc-brand">
            <div class="pcc-brand-mark">P</div>
            <div class="pcc-brand-copy"><strong>PARADISE</strong><span>CONTROL CENTER</span></div>
            <button class="pcc-icon-button pcc-sidebar-toggle" type="button" data-sidebar-toggle aria-label="Réduire la navigation"><i class="fas fa-bars"></i></button>
        </div>

        <nav class="pcc-nav" aria-label="Navigation administration">
            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Vue d’ensemble</span>
                <a class="pcc-nav-item <?php echo $AdminPage === 'dashboard' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=dashboard"><i class="fas fa-th-large"></i><span>Dashboard</span></a>
            </div>

            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Joueurs</span>
                <?php if($PCC->can('players.view')): ?><a class="pcc-nav-item <?php echo in_array($AdminPage, array('players','player'), true) ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=players"><i class="fas fa-users"></i><span>Joueurs</span></a><?php endif; ?>
                <?php if($PCC->can('sanctions.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'sanctions' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=sanctions"><i class="fas fa-gavel"></i><span>Sanctions</span></a><?php endif; ?>
            </div>

            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Roleplay</span>
                <?php if($PCC->can('documents.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'documents' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=documents"><i class="fas fa-id-card"></i><span>Documents</span></a><?php endif; ?>
                <?php if($PCC->can('inventory.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'inventory' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=inventory"><i class="fas fa-box-open"></i><span>Inventaires</span></a><?php endif; ?>
                <?php if($PCC->can('items.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'items' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=items"><i class="fas fa-cubes"></i><span>Objets</span></a><?php endif; ?>
                <?php if($PCC->can('phone.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'phones' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=phones"><i class="fas fa-mobile-alt"></i><span>Téléphones</span></a><?php endif; ?>
                <?php if($PCC->can('business.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'businesses' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=businesses"><i class="fas fa-building"></i><span>Entreprises</span></a><?php endif; ?>
            </div>

            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Économie & contenu</span>
                <?php if($PCC->can('catalog.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'catalogue' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=catalogue"><i class="fas fa-shopping-bag"></i><span>Catalogue</span></a><?php endif; ?>
                <?php if($PCC->can('badges.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'badges' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=badges"><i class="fas fa-certificate"></i><span>Badges</span></a><?php endif; ?>
            </div>

            <div class="pcc-nav-group">
                <span class="pcc-nav-label">Administration</span>
                <?php if($PCC->can('staff.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'staff' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=staff"><i class="fas fa-user-shield"></i><span>Staff</span></a><?php endif; ?>
                <?php if($PCC->can('logs.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'logs' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=logs"><i class="fas fa-stream"></i><span>Logs</span></a><?php endif; ?>
                <?php if($PCC->can('permissions.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'permissions' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=permissions"><i class="fas fa-key"></i><span>Permissions</span></a><?php endif; ?>
                <?php if($PCC->can('config.manage')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'settings' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=settings"><i class="fas fa-sliders-h"></i><span>Configuration</span></a><?php endif; ?>
                <?php if($PCC->can('health.view')): ?><a class="pcc-nav-item <?php echo $AdminPage === 'health' ? 'is-active' : ''; ?>" href="<?php echo URL; ?>/admin.php?page=health"><i class="fas fa-heartbeat"></i><span>Santé système</span></a><?php endif; ?>
            </div>
        </nav>

        <div class="pcc-sidebar-foot">
            <div class="pcc-runtime"><span class="pcc-runtime-dot <?php echo $emuSignal && isset($emuSignal['status']) && (int)$emuSignal['status'] === 1 ? 'ok' : 'neutral'; ?>"></span><div><strong>ÉMU</strong><small><?php echo $emuSignal ? 'signal DB ' . (((int)($emuSignal['status'] ?? 0) === 1) ? 'actif' : 'inactif') : 'signal indisponible'; ?></small></div></div>
            <div class="pcc-version">CMS <?php echo $h(Config::$V); ?> · PCC V3</div>
        </div>
    </aside>

    <section class="pcc-workspace">
        <header class="pcc-topbar">
            <div class="pcc-global-search" data-global-search>
                <i class="fas fa-search"></i>
                <input type="search" placeholder="Username, Citizen ID, téléphone, entreprise…" autocomplete="off" aria-label="Recherche globale" data-global-search-input>
                <kbd>Ctrl K</kbd>
                <div class="pcc-search-results" data-global-search-results hidden></div>
            </div>
            <div class="pcc-topbar-state">
                <span class="pcc-status-pill"><i class="fas fa-user-friends"></i><?php echo $number($onlineCount); ?> en ligne</span>
                <span class="pcc-status-pill neutral"><i class="fas fa-server"></i><?php echo $emuSignal ? (((int)($emuSignal['status'] ?? 0) === 1) ? 'ÉMU signal actif' : 'ÉMU signal inactif') : 'ÉMU non exposé'; ?></span>
            </div>
            <div class="pcc-account">
                <img src="<?php echo $h($staffAvatar); ?>" alt="">
                <div><strong><?php echo $h($UData['username']); ?></strong><span><?php echo $h($PCC->roleName($UData['rank'])); ?></span></div>
                <a class="pcc-icon-button" href="<?php echo URL; ?>/me" title="Retour au CMS"><i class="fas fa-external-link-alt"></i></a>
                <form method="post" class="pcc-inline-form">
                    <input type="hidden" name="csrf" value="<?php echo $h($PCC->csrfToken()); ?>">
                    <input type="hidden" name="action_nonce" value="<?php echo $h($PCC->issueNonce()); ?>">
                    <input type="hidden" name="action" value="logout">
                    <button class="pcc-icon-button" type="submit" title="Déconnexion"><i class="fas fa-sign-out-alt"></i></button>
                </form>
            </div>
        </header>

        <main class="pcc-main">
            <div class="pcc-page-head">
                <div><span class="pcc-eyebrow">Paradise Control Center</span><h1><?php echo $h($currentTitle[0]); ?></h1><p><?php echo $h($currentTitle[1]); ?></p></div>
                <div class="pcc-page-head-actions"><button class="pcc-button ghost" type="button" data-refresh><i class="fas fa-sync-alt"></i> Actualiser</button></div>
            </div>

            <?php if($AdminFlash): ?>
                <div class="pcc-toast is-<?php echo $h($AdminFlash['type']); ?>" data-toast><i class="fas fa-info-circle"></i><span><?php echo $h($AdminFlash['message']); ?></span><button type="button" data-toast-close aria-label="Fermer">×</button></div>
            <?php endif; ?>

            <?php if(!$PCC->auditReady()): ?>
                <section class="pcc-alert warning"><i class="fas fa-shield-alt"></i><div><strong>Audit V3 non installé</strong><p>Applique <code>migrations/2026_08_22_paradise_control_center_v3.sql</code>. Les lectures fonctionnent, les écritures sensibles restent bloquées volontairement.</p></div></section>
            <?php endif; ?>

            <?php
            if (is_file($adminViewFile)) require $adminViewFile;
            else echo '<section class="pcc-panel"><div class="pcc-empty"><strong>Module indisponible</strong><span>La vue demandée n’existe pas.</span></div></section>';
            ?>
        </main>
    </section>
</div>

<div class="pcc-modal-backdrop" data-confirm-modal hidden>
    <div class="pcc-modal" role="dialog" aria-modal="true" aria-labelledby="pcc-confirm-title">
        <div class="pcc-modal-icon"><i class="fas fa-exclamation-triangle"></i></div>
        <div><span class="pcc-eyebrow">Confirmation requise</span><h2 id="pcc-confirm-title" data-confirm-modal-title>Confirmer l’action</h2><p data-confirm-modal-message>Cette action sera enregistrée.</p></div>
        <div class="pcc-modal-actions"><button class="pcc-button ghost" type="button" data-confirm-cancel>Annuler</button><button class="pcc-button danger" type="button" data-confirm-accept>Confirmer</button></div>
    </div>
</div>

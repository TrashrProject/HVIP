<?php
/** Velora RP - citizen dashboard */

$username = isset($UData['username']) ? $UData['username'] : 'Citoyen';
$look = isset($UData['look']) ? $UData['look'] : '';
$motto = isset($UData['motto']) && trim($UData['motto']) !== '' ? $UData['motto'] : 'Citoyen de Velora';
$credits = isset($UData['credits']) ? (int)$UData['credits'] : 0;
$vipPoints = isset($UData['vip_points']) ? (int)$UData['vip_points'] : 0;
$rank = isset($UData['rank']) ? (int)$UData['rank'] : 1;
$bank = isset($UPData['bank']) ? (int)$UPData['bank'] : 0;
$level = isset($UPData['level']) ? (int)$UPData['level'] : 1;
$onlineUsers = 0;
$registeredUsers = 0;

try {
    $onlineUsers = (int)$UserMG->GetStatData('users_online');
    $registeredUsers = (int)$UserMG->GetStatData('users_registered');
} catch (Throwable $e) {
    $onlineUsers = 0;
    $registeredUsers = 0;
}

$avatarUrl = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=l&head_direction=3&gesture=sml&action=wav';
$avatarMiniUrl = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=m&head_direction=3&gesture=sml';
$citizenType = $rank >= 5 ? 'Équipe Velora' : 'Citoyen';
?>

<div class="me-ambient" aria-hidden="true">
    <div class="me-ambient-city"></div>
</div>
<div class="sidebar-overlay"></div>

<div class="me-app">
    <aside class="me-sidebar">
        <a class="me-brand" href="<?php echo URL; ?>/me">
            <span class="me-brand-logo">V</span>
            <span class="me-brand-copy"><strong>VELORA</strong><small>ROLEPLAY</small></span>
        </a>

        <div class="me-profile-mini">
            <div class="me-profile-row">
                <div class="me-avatar-mini"><img src="<?php echo htmlspecialchars($avatarMiniUrl, ENT_QUOTES, 'UTF-8'); ?>" alt=""></div>
                <div class="me-profile-copy">
                    <strong><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong>
                    <span><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?> · niveau <?php echo $level; ?></span>
                    <span class="mini-status"><i class="fas fa-circle"></i> Session active</span>
                </div>
            </div>
        </div>

        <nav class="me-nav" aria-label="Navigation principale">
            <span class="me-nav-section">MON ESPACE</span>
            <a class="active" href="<?php echo URL; ?>/me"><i class="fas fa-home"></i><span>Accueil</span></a>
            <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="fas fa-user"></i><span>Mon profil</span></a>
            <a href="<?php echo URL; ?>/account"><i class="fas fa-sliders-h"></i><span>Paramètres</span></a>

            <span class="me-nav-section">VELORA</span>
            <a href="<?php echo URL; ?>/map"><i class="fas fa-map-marked-alt"></i><span>Carte de la ville</span></a>
            <a href="<?php echo URL; ?>/corporations"><i class="fas fa-building"></i><span>Entreprises</span></a>
            <a href="<?php echo URL; ?>/gangs"><i class="fas fa-users"></i><span>Organisations</span></a>
            <a href="<?php echo URL; ?>/online"><i class="fas fa-user-friends"></i><span>Citoyens en ligne</span></a>
            <a href="<?php echo URL; ?>/store"><i class="fas fa-shopping-bag"></i><span>Boutique</span></a>

            <a class="danger" href="<?php echo URL; ?>/logout"><i class="fas fa-sign-out-alt"></i><span>Déconnexion</span></a>
        </nav>

        <div class="me-sidebar-bottom">
            <strong>VELORA CITY NETWORK</strong>
            Ton espace citoyen centralise ta progression et les accès importants du serveur.
        </div>
    </aside>

    <main class="me-main">
        <header class="me-topbar">
            <div class="me-topbar-left">
                <button id="me-menu-toggle" class="menu-toggle" type="button" aria-label="Ouvrir le menu"><i class="fas fa-bars"></i></button>
                <div class="me-page-title">
                    <span>ESPACE CITOYEN</span>
                    <strong>Bienvenue, <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong>
                </div>
            </div>
            <div class="me-topbar-right">
                <div class="me-live-pill"><i class="fas fa-circle"></i><strong><?php echo number_format($onlineUsers); ?></strong> citoyens connectés</div>
                <a class="enter-city" href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener"><i class="fas fa-play"></i><span>ENTRER EN VILLE</span></a>
            </div>
        </header>

        <div class="me-content">
            <section class="welcome-card">
                <div class="welcome-copy">
                    <span class="welcome-kicker"><i class="fas fa-city"></i> VELORA CITY · SAISON 01</span>
                    <h1>Ta vie commence ici.</h1>
                    <p><?php echo htmlspecialchars($motto, ENT_QUOTES, 'UTF-8'); ?>. Retrouve ton argent, ta progression et tous les accès essentiels avant de rejoindre la ville.</p>
                    <div class="welcome-actions">
                        <a class="primary" href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener"><i class="fas fa-door-open"></i> Entrer dans Velora</a>
                        <a class="secondary" href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="far fa-id-card"></i> Voir mon profil</a>
                    </div>
                </div>
                <div class="welcome-avatar"><img src="<?php echo htmlspecialchars($avatarUrl, ENT_QUOTES, 'UTF-8'); ?>" alt="Avatar de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>"></div>
            </section>

            <section class="stat-grid" aria-label="Statistiques du personnage">
                <article class="stat-card wallet">
                    <div class="stat-card-top"><span class="stat-icon"><i class="fas fa-wallet"></i></span><span class="stat-label">PORTEFEUILLE</span></div>
                    <strong>$<?php echo number_format($credits); ?></strong><small>Argent disponible sur toi</small>
                </article>
                <article class="stat-card bank">
                    <div class="stat-card-top"><span class="stat-icon"><i class="fas fa-university"></i></span><span class="stat-label">BANQUE</span></div>
                    <strong>$<?php echo number_format($bank); ?></strong><small>Épargne actuellement en banque</small>
                </article>
                <article class="stat-card vip">
                    <div class="stat-card-top"><span class="stat-icon"><i class="fas fa-gem"></i></span><span class="stat-label">PLATINOS</span></div>
                    <strong><?php echo number_format($vipPoints); ?></strong><small>Monnaie premium du compte</small>
                </article>
                <article class="stat-card level">
                    <div class="stat-card-top"><span class="stat-icon"><i class="fas fa-chart-line"></i></span><span class="stat-label">PROGRESSION</span></div>
                    <strong>Niveau <?php echo $level; ?></strong><small>Progression globale RP</small>
                </article>
            </section>

            <section class="dashboard-grid">
                <div class="dash-column">
                    <article class="dash-card">
                        <header class="dash-head">
                            <div class="dash-title"><i class="fas fa-id-badge"></i><div><strong>Ma vie à Velora</strong><span>Résumé de ton personnage</span></div></div>
                            <a class="dash-link" href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>">Profil →</a>
                        </header>
                        <div class="dash-body life-grid">
                            <div class="life-card">
                                <div class="life-top"><i class="fas fa-briefcase"></i><span class="life-badge">RP</span></div>
                                <span>ACTIVITÉ</span><strong>Citoyen de Velora</strong><small>Choisis ton métier et construis ta carrière directement en ville.</small>
                            </div>
                            <div class="life-card">
                                <div class="life-top"><i class="fas fa-home"></i><span class="life-badge">VILLE</span></div>
                                <span>LOGEMENT</span><strong>Ton point de départ</strong><small>Entre en ville pour rejoindre ton logement et gérer ta vie quotidienne.</small>
                            </div>
                            <div class="life-card">
                                <div class="life-top"><i class="fas fa-star"></i><span class="life-badge">NIV. <?php echo $level; ?></span></div>
                                <span>RÉPUTATION</span><strong><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?></strong><small>Ta progression reflète peu à peu la place que tu prends dans Velora.</small>
                            </div>
                        </div>
                    </article>

                    <article class="dash-card">
                        <header class="dash-head">
                            <div class="dash-title"><i class="fas fa-bolt"></i><div><strong>Accès rapides</strong><span>Tout ce qu'il faut sans chercher</span></div></div>
                        </header>
                        <div class="dash-body quick-grid">
                            <a class="quick-link" href="<?php echo URL; ?>/map"><span class="quick-icon"><i class="fas fa-map"></i></span><span class="quick-copy"><strong>Carte de Velora</strong><span>Repère les zones et lieux importants.</span></span></a>
                            <a class="quick-link" href="<?php echo URL; ?>/corporations"><span class="quick-icon"><i class="fas fa-building"></i></span><span class="quick-copy"><strong>Entreprises</strong><span>Découvre l'économie et les corporations.</span></span></a>
                            <a class="quick-link" href="<?php echo URL; ?>/gangs"><span class="quick-icon"><i class="fas fa-user-secret"></i></span><span class="quick-copy"><strong>Organisations</strong><span>Consulte les groupes actifs de la ville.</span></span></a>
                            <a class="quick-link" href="<?php echo URL; ?>/store"><span class="quick-icon"><i class="fas fa-shopping-bag"></i></span><span class="quick-copy"><strong>Boutique</strong><span>Retrouve les éléments disponibles sur le CMS.</span></span></a>
                        </div>
                    </article>
                </div>

                <div class="dash-column">
                    <article class="dash-card">
                        <header class="dash-head">
                            <div class="dash-title"><i class="fas fa-broadcast-tower"></i><div><strong>Velora Live</strong><span>État actuel de la ville</span></div></div>
                            <span class="dash-link"><?php echo number_format($onlineUsers); ?> online</span>
                        </header>
                        <div class="dash-body city-feed">
                            <div class="feed-item"><span class="feed-icon"><i class="fas fa-user-friends"></i></span><span class="feed-copy"><strong>La ville est ouverte</strong><span><?php echo number_format($onlineUsers); ?> citoyens sont actuellement connectés.</span></span><span class="feed-time">LIVE</span></div>
                            <div class="feed-item"><span class="feed-icon"><i class="fas fa-address-card"></i></span><span class="feed-copy"><strong>Communauté</strong><span><?php echo number_format($registeredUsers); ?> comptes ont rejoint Velora.</span></span><span class="feed-time">RP</span></div>
                            <div class="feed-item"><span class="feed-icon"><i class="fas fa-briefcase"></i></span><span class="feed-copy"><strong>Construis ton histoire</strong><span>Métier, entreprise, police ou rue : ta place dépend de tes choix.</span></span><span class="feed-time">CITY</span></div>
                        </div>
                    </article>

                    <article class="dash-card objective-card">
                        <span class="objective-kicker">OBJECTIF DE DÉPART</span>
                        <h3>Fais-toi un nom.</h3>
                        <p>Entre dans Velora, rencontre les habitants et commence à construire une vraie identité RP.</p>
                        <div class="objective-progress"><span></span></div>
                    </article>
                </div>
            </section>

            <footer class="me-footer">
                <span><strong><?php echo Config::$WName; ?></strong> · Tableau de bord citoyen</span>
                <span>Session de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?> · <span id="me-clock" class="me-clock">--:--</span></span>
            </footer>
        </div>
    </main>
</div>

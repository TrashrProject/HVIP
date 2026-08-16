<?php
/** Velora RP - new citizen launcher */

$username = isset($UData['username']) ? $UData['username'] : 'Citoyen';
$look = isset($UData['look']) ? $UData['look'] : '';
$motto = isset($UData['motto']) && trim($UData['motto']) !== '' ? $UData['motto'] : 'Écris ton histoire. Prends ta place.';
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

$avatar = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=l&head_direction=3&gesture=sml&action=wav';
$avatarSmall = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=m&head_direction=3&gesture=sml';
$citizenType = $rank >= 5 ? 'Équipe Velora' : 'Citoyen';
?>

<div class="vx-bg" aria-hidden="true">
    <div class="vx-moon"></div>
    <div class="vx-haze"></div>
    <div class="vx-stars"></div>
    <div class="vx-city vx-city-back"></div>
    <div class="vx-city vx-city-front"></div>
    <div class="vx-street"></div>
</div>

<div class="vx-page">
    <header class="vx-header">
        <a class="vx-brand" href="<?php echo URL; ?>/me">
            <span class="vx-brand-mark">V</span>
            <span class="vx-brand-text"><strong>VELORA</strong><small>ROLEPLAY</small></span>
        </a>

        <nav class="vx-nav" id="vx-nav">
            <a class="active" href="<?php echo URL; ?>/me">Accueil</a>
            <a href="<?php echo URL; ?>/map">Ville</a>
            <a href="<?php echo URL; ?>/corporations">Business</a>
            <a href="<?php echo URL; ?>/gangs">Organisations</a>
            <a href="<?php echo URL; ?>/online">Citoyens</a>
        </nav>

        <div class="vx-header-actions">
            <span class="vx-online"><i></i><?php echo number_format($onlineUsers); ?> en ligne</span>
            <a class="vx-launch-small" href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener"><i class="fas fa-play"></i> Jouer</a>
            <div class="vx-account">
                <button id="vx-account-toggle" type="button" class="vx-account-btn" aria-expanded="false">
                    <span class="vx-account-avatar"><img src="<?php echo htmlspecialchars($avatarSmall, ENT_QUOTES, 'UTF-8'); ?>" alt=""></span>
                    <span><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></span>
                    <i class="fas fa-chevron-down"></i>
                </button>
                <div id="vx-account-menu" class="vx-account-menu">
                    <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="far fa-id-card"></i> Mon profil</a>
                    <a href="<?php echo URL; ?>/account"><i class="fas fa-cog"></i> Paramètres</a>
                    <a class="danger" href="<?php echo URL; ?>/logout"><i class="fas fa-sign-out-alt"></i> Déconnexion</a>
                </div>
            </div>
            <button id="vx-mobile-toggle" class="vx-mobile-toggle" type="button"><i class="fas fa-bars"></i></button>
        </div>
    </header>

    <main class="vx-main">
        <section class="vx-hero">
            <div class="vx-hero-copy">
                <div class="vx-eyebrow"><span></span> VELORA CITY · SAISON 01</div>
                <h1>BON RETOUR<br><em><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></em></h1>
                <p><?php echo htmlspecialchars($motto, ENT_QUOTES, 'UTF-8'); ?></p>

                <div class="vx-hero-meta">
                    <span><b><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?></b> Statut</span>
                    <span><b>Niveau <?php echo $level; ?></b> Progression</span>
                    <span><b><?php echo number_format($onlineUsers); ?></b> En ville</span>
                </div>

                <div class="vx-hero-actions">
                    <a class="vx-play" href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener"><span class="vx-play-icon"><i class="fas fa-play"></i></span><span><small>REJOINDRE LE SERVEUR</small><strong>ENTRER EN VILLE</strong></span></a>
                    <a class="vx-secondary" href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="far fa-user"></i> Mon profil</a>
                </div>
            </div>

            <div class="vx-character-stage">
                <div class="vx-character-ring ring-one"></div>
                <div class="vx-character-ring ring-two"></div>
                <div class="vx-pass-card">
                    <div class="vx-pass-top"><span>VELORA</span><b>VC-01</b></div>
                    <div class="vx-pass-name"><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></div>
                    <div class="vx-pass-role"><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?> · LEVEL <?php echo $level; ?></div>
                    <div class="vx-pass-bars"><i></i><i></i><i></i><i></i><i></i><i></i></div>
                </div>
                <img class="vx-character" src="<?php echo htmlspecialchars($avatar, ENT_QUOTES, 'UTF-8'); ?>" alt="Avatar de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>">
                <div class="vx-character-floor"></div>
            </div>

            <div class="vx-hero-status">
                <div><span class="vx-status-icon"><i class="fas fa-wallet"></i></span><p>PORTEFEUILLE<strong>$<?php echo number_format($credits); ?></strong></p></div>
                <div><span class="vx-status-icon"><i class="fas fa-university"></i></span><p>BANQUE<strong>$<?php echo number_format($bank); ?></strong></p></div>
                <div><span class="vx-status-icon"><i class="fas fa-gem"></i></span><p>PLATINOS<strong><?php echo number_format($vipPoints); ?></strong></p></div>
                <div><span class="vx-status-icon"><i class="fas fa-users"></i></span><p>CITOYENS<strong><?php echo number_format($registeredUsers); ?></strong></p></div>
            </div>
        </section>

        <section class="vx-dock" aria-label="Accès rapides">
            <a href="<?php echo URL; ?>/map"><span class="vx-dock-icon blue"><i class="fas fa-map-marked-alt"></i></span><p><strong>VILLE</strong><small>Carte & quartiers</small></p><i class="fas fa-arrow-right"></i></a>
            <a href="<?php echo URL; ?>/corporations"><span class="vx-dock-icon gold"><i class="fas fa-briefcase"></i></span><p><strong>BUSINESS</strong><small>Entreprises & métiers</small></p><i class="fas fa-arrow-right"></i></a>
            <a href="<?php echo URL; ?>/gangs"><span class="vx-dock-icon red"><i class="fas fa-users"></i></span><p><strong>ORGANISATIONS</strong><small>Influence & réseaux</small></p><i class="fas fa-arrow-right"></i></a>
            <a href="<?php echo URL; ?>/store"><span class="vx-dock-icon purple"><i class="fas fa-shopping-bag"></i></span><p><strong>BOUTIQUE</strong><small>Contenu & extras</small></p><i class="fas fa-arrow-right"></i></a>
        </section>

        <section class="vx-lower-grid">
            <article class="vx-panel vx-live-panel">
                <div class="vx-panel-head">
                    <div><small>VELORA LIVE</small><strong>La ville en direct</strong></div>
                    <span class="vx-live"><i></i> LIVE</span>
                </div>
                <div class="vx-live-list">
                    <div class="vx-live-row"><span class="vx-live-icon"><i class="fas fa-city"></i></span><div><strong>Velora est ouverte</strong><small>Ta session est prête, tu peux rejoindre la ville.</small></div><b>MAINTENANT</b></div>
                    <div class="vx-live-row"><span class="vx-live-icon"><i class="fas fa-briefcase"></i></span><div><strong>Économie RP active</strong><small>Les joueurs font vivre les métiers, commerces et entreprises.</small></div><b>ACTIF</b></div>
                    <div class="vx-live-row"><span class="vx-live-icon"><i class="fas fa-user-friends"></i></span><div><strong><?php echo number_format($onlineUsers); ?> citoyens connectés</strong><small>Retrouve les joueurs déjà présents dans la ville.</small></div><b>EN LIGNE</b></div>
                </div>
            </article>

            <aside class="vx-panel vx-citizen-panel">
                <div class="vx-panel-head">
                    <div><small>MON IDENTITÉ</small><strong>Carte citoyenne</strong></div>
                    <span class="vx-card-chip"><i class="fas fa-id-card"></i></span>
                </div>
                <div class="vx-citizen-card">
                    <div class="vx-citizen-avatar"><img src="<?php echo htmlspecialchars($avatarSmall, ENT_QUOTES, 'UTF-8'); ?>" alt=""></div>
                    <div class="vx-citizen-copy"><strong><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong><span><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?></span><small>Niveau <?php echo $level; ?> · Velora City</small></div>
                </div>
                <div class="vx-citizen-links">
                    <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="fas fa-user"></i> Profil <span>›</span></a>
                    <a href="<?php echo URL; ?>/account"><i class="fas fa-sliders-h"></i> Paramètres <span>›</span></a>
                    <a href="<?php echo URL; ?>/leaderboards"><i class="fas fa-trophy"></i> Classements <span>›</span></a>
                </div>
            </aside>
        </section>
    </main>

    <footer class="vx-footer"><span>VELORA CITY NETWORK</span><span id="vx-clock">00:00</span><span>© <?php echo date('Y'); ?> <?php echo strtoupper(Config::$WName); ?></span></footer>
</div>

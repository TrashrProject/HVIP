<?php
/** Velora RP - immersive citizen hub */

$username = isset($UData['username']) ? $UData['username'] : 'Citoyen';
$look = isset($UData['look']) ? $UData['look'] : '';
$motto = isset($UData['motto']) && trim($UData['motto']) !== '' ? $UData['motto'] : 'Ma place se construit à Velora.';
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

<div class="hub-scene" aria-hidden="true">
    <div class="hub-sky-glow"></div>
    <span class="hub-cloud hub-cloud-a"></span>
    <span class="hub-cloud hub-cloud-b"></span>
    <div class="hub-skyline"></div>
    <div class="hub-street-row">
        <span class="hub-building b1"></span>
        <span class="hub-building b2"></span>
        <span class="hub-building b3"></span>
        <span class="hub-building b4"></span>
    </div>
    <div class="hub-fence"></div>
    <div class="hub-sidewalk"></div>
    <div class="hub-road"></div>
</div>

<div class="hub-shell">
    <header class="hub-topbar">
        <a class="hub-brand" href="<?php echo URL; ?>/me">
            <span class="hub-brand-mark">V</span>
            <span class="hub-brand-copy"><strong>VELORA</strong><small>ROLEPLAY</small></span>
        </a>

        <nav class="hub-nav" aria-label="Navigation principale">
            <a class="active" href="<?php echo URL; ?>/me"><i class="fas fa-home"></i><span>Accueil</span></a>
            <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="fas fa-user"></i><span>Profil</span></a>
            <a href="<?php echo URL; ?>/map"><i class="fas fa-map-marked-alt"></i><span>Ville</span></a>
            <a href="<?php echo URL; ?>/corporations"><i class="fas fa-building"></i><span>Business</span></a>
            <a href="<?php echo URL; ?>/gangs"><i class="fas fa-users"></i><span>Organisations</span></a>
            <a href="<?php echo URL; ?>/store"><i class="fas fa-shopping-bag"></i><span>Boutique</span></a>
        </nav>

        <div class="hub-actions">
            <div class="hub-online"><i class="fas fa-circle"></i><strong><?php echo number_format($onlineUsers); ?></strong><span>en ligne</span></div>
            <div class="hub-user-menu">
                <button id="hub-user-toggle" type="button" class="hub-user-button" aria-expanded="false">
                    <span class="hub-user-avatar"><img src="<?php echo htmlspecialchars($avatarMiniUrl, ENT_QUOTES, 'UTF-8'); ?>" alt=""></span>
                    <span class="hub-user-name"><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></span>
                    <i class="fas fa-chevron-down"></i>
                </button>
                <div id="hub-user-dropdown" class="hub-user-dropdown">
                    <a href="<?php echo URL; ?>/account"><i class="fas fa-cog"></i> Paramètres</a>
                    <a href="<?php echo URL; ?>/logout" class="logout"><i class="fas fa-sign-out-alt"></i> Déconnexion</a>
                </div>
            </div>
            <button id="hub-menu-toggle" class="hub-menu-toggle" type="button" aria-label="Menu"><i class="fas fa-bars"></i></button>
        </div>
    </header>

    <main class="hub-main">
        <section class="citizen-hero">
            <div class="citizen-copy">
                <div class="hero-kicker"><span class="live-dot"></span> IDENTITÉ CITOYENNE ACTIVE</div>
                <h1>Bienvenue à Velora,<br><span><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></span>.</h1>
                <p><?php echo htmlspecialchars($motto, ENT_QUOTES, 'UTF-8'); ?></p>

                <div class="hero-tags">
                    <span><i class="fas fa-id-badge"></i><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?></span>
                    <span><i class="fas fa-chart-line"></i>Niveau <?php echo $level; ?></span>
                    <span><i class="fas fa-map-marker-alt"></i>Velora City</span>
                </div>

                <div class="hero-actions">
                    <a class="hero-play" href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener"><i class="fas fa-play"></i><span>ENTRER EN VILLE</span></a>
                    <a class="hero-profile" href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="far fa-id-card"></i><span>Voir mon profil</span></a>
                </div>
            </div>

            <div class="citizen-visual">
                <div class="visual-glow"></div>
                <div class="citizen-id-card">
                    <div class="id-card-top"><span>VELORA CITY</span><strong>V·01</strong></div>
                    <div class="id-card-name"><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></div>
                    <div class="id-card-role"><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?> · Niveau <?php echo $level; ?></div>
                    <div class="id-card-code"><span></span><span></span><span></span><span></span><span></span><span></span><span></span></div>
                </div>
                <img class="citizen-avatar" src="<?php echo htmlspecialchars($avatarUrl, ENT_QUOTES, 'UTF-8'); ?>" alt="Avatar de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>">
            </div>
        </section>

        <section class="money-strip" aria-label="Économie du personnage">
            <article>
                <span class="money-icon wallet"><i class="fas fa-wallet"></i></span>
                <div><small>PORTEFEUILLE</small><strong>$<?php echo number_format($credits); ?></strong></div>
            </article>
            <article>
                <span class="money-icon bank"><i class="fas fa-university"></i></span>
                <div><small>BANQUE</small><strong>$<?php echo number_format($bank); ?></strong></div>
            </article>
            <article>
                <span class="money-icon platinum"><i class="fas fa-gem"></i></span>
                <div><small>PLATINOS</small><strong><?php echo number_format($vipPoints); ?></strong></div>
            </article>
            <article>
                <span class="money-icon citizens"><i class="fas fa-user-friends"></i></span>
                <div><small>CITOYENS</small><strong><?php echo number_format($registeredUsers); ?></strong></div>
            </article>
        </section>

        <section class="hub-grid">
            <div class="hub-left-column">
                <article class="city-panel city-life-panel">
                    <div class="panel-head">
                        <div><span>TA VIE À VELORA</span><strong>Choisis où tu veux aller</strong></div>
                        <i class="fas fa-compass"></i>
                    </div>

                    <div class="city-actions-grid">
                        <a class="city-action map" href="<?php echo URL; ?>/map">
                            <span class="city-action-icon"><i class="fas fa-map-marked-alt"></i></span>
                            <span class="city-action-copy"><strong>Explorer la ville</strong><small>Quartiers, lieux et points d'intérêt</small></span>
                            <i class="fas fa-arrow-right arrow"></i>
                        </a>
                        <a class="city-action business" href="<?php echo URL; ?>/corporations">
                            <span class="city-action-icon"><i class="fas fa-briefcase"></i></span>
                            <span class="city-action-copy"><strong>Faire du business</strong><small>Entreprises et opportunités RP</small></span>
                            <i class="fas fa-arrow-right arrow"></i>
                        </a>
                        <a class="city-action org" href="<?php echo URL; ?>/gangs">
                            <span class="city-action-icon"><i class="fas fa-users"></i></span>
                            <span class="city-action-copy"><strong>Organisations</strong><small>Groupes, réseaux et influence</small></span>
                            <i class="fas fa-arrow-right arrow"></i>
                        </a>
                        <a class="city-action people" href="<?php echo URL; ?>/online">
                            <span class="city-action-icon"><i class="fas fa-user-friends"></i></span>
                            <span class="city-action-copy"><strong>Voir les citoyens</strong><small><?php echo number_format($onlineUsers); ?> joueurs actuellement connectés</small></span>
                            <i class="fas fa-arrow-right arrow"></i>
                        </a>
                    </div>
                </article>

                <article class="city-panel city-news-panel">
                    <div class="panel-head compact">
                        <div><span>VELORA LIVE</span><strong>Ce qui se passe en ville</strong></div>
                        <div class="panel-live"><i class="fas fa-circle"></i> LIVE</div>
                    </div>
                    <div class="live-feed">
                        <div class="live-item"><span class="live-icon"><i class="fas fa-city"></i></span><div><strong>La ville est ouverte</strong><small>Entre en jeu et fais évoluer ton histoire RP.</small></div><time>maintenant</time></div>
                        <div class="live-item"><span class="live-icon"><i class="fas fa-briefcase"></i></span><div><strong>Économie active</strong><small>Les entreprises et métiers façonnent la ville.</small></div><time>RP</time></div>
                        <div class="live-item"><span class="live-icon"><i class="fas fa-users"></i></span><div><strong><?php echo number_format($onlineUsers); ?> citoyens connectés</strong><small>Rejoins les joueurs déjà présents à Velora.</small></div><time>live</time></div>
                    </div>
                </article>
            </div>

            <aside class="hub-right-column">
                <article class="phone-card">
                    <div class="phone-shell-mini">
                        <div class="phone-notch-mini"></div>
                        <div class="phone-status-mini"><span id="hub-clock">00:00</span><span><i class="fas fa-signal"></i><i class="fas fa-wifi"></i></span></div>
                        <div class="phone-screen-mini">
                            <div class="phone-title"><span>VELORA</span><strong>City Phone</strong></div>
                            <div class="phone-apps">
                                <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="fas fa-user"></i><span>Profil</span></a>
                                <a href="<?php echo URL; ?>/map"><i class="fas fa-map"></i><span>Carte</span></a>
                                <a href="<?php echo URL; ?>/corporations"><i class="fas fa-building"></i><span>Jobs</span></a>
                                <a href="<?php echo URL; ?>/store"><i class="fas fa-shopping-bag"></i><span>Shop</span></a>
                            </div>
                            <div class="phone-notice"><i class="fas fa-bell"></i><div><strong>Prêt pour le RP ?</strong><small>Ta session est active.</small></div></div>
                        </div>
                    </div>
                </article>

                <article class="quick-panel">
                    <div class="quick-head"><span>ACCÈS RAPIDES</span><i class="fas fa-bolt"></i></div>
                    <a href="<?php echo URL; ?>/account"><span><i class="fas fa-cog"></i>Paramètres du compte</span><i class="fas fa-chevron-right"></i></a>
                    <a href="<?php echo URL; ?>/staff"><span><i class="fas fa-shield-alt"></i>Équipe Velora</span><i class="fas fa-chevron-right"></i></a>
                    <a href="<?php echo URL; ?>/leaderboards"><span><i class="fas fa-trophy"></i>Classements</span><i class="fas fa-chevron-right"></i></a>
                </article>
            </aside>
        </section>
    </main>

    <footer class="hub-footer">
        <span>© <?php echo date('Y'); ?> <?php echo strtoupper(Config::$WName); ?></span>
        <span>VELORA CITY NETWORK · SESSION ACTIVE</span>
    </footer>
</div>

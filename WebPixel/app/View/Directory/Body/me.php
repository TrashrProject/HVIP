<?php
/** Velora RP - immersive RP citizen dashboard */

$username = isset($UData['username']) ? $UData['username'] : 'Citoyen';
$look = isset($UData['look']) ? $UData['look'] : '';
$motto = isset($UData['motto']) && trim($UData['motto']) !== '' ? $UData['motto'] : 'Bienvenue dans ta ville, fais ta légende.';
$userId = isset($UData['id']) ? (int)$UData['id'] : 1;
$rank = isset($UData['rank']) ? (int)$UData['rank'] : 1;
$credits = isset($UData['credits']) ? (int)$UData['credits'] : 0;
$vipPoints = isset($UData['vip_points']) ? (int)$UData['vip_points'] : 0;

$bank = isset($UPData['bank']) ? (int)$UPData['bank'] : 0;
$level = isset($UPData['level']) ? max(1, (int)$UPData['level']) : 1;
$xp = isset($UPData['xp']) ? max(0, (int)$UPData['xp']) : (isset($UPData['experience']) ? max(0, (int)$UPData['experience']) : 0);
$xpGoal = max(1000, $level * 1000);
$xpPercent = min(100, (int)round(($xp / $xpGoal) * 100));

$health = isset($UPData['health']) ? max(0, min(100, (int)$UPData['health'])) : 100;
$fatigue = isset($UPData['fatigue']) ? max(0, min(100, (int)$UPData['fatigue'])) : 20;
$energy = isset($UPData['energy']) ? max(0, min(100, (int)$UPData['energy'])) : 58;
$hygiene = isset($UPData['hygiene']) ? max(0, min(100, (int)$UPData['hygiene'])) : 92;

$jobName = isset($UPData['job_name']) && trim($UPData['job_name']) !== '' ? $UPData['job_name'] : 'Sans emploi';
$jobRole = isset($UPData['job_rank']) && trim($UPData['job_rank']) !== '' ? $UPData['job_rank'] : 'Chômeur';
$citizenType = $rank >= 5 ? 'Équipe Velora' : 'Citoyen de Velora';

$onlineUsers = 0;
$registeredUsers = 0;
try {
    $onlineUsers = (int)$UserMG->GetStatData('users_online');
    $registeredUsers = (int)$UserMG->GetStatData('users_registered');
} catch (Throwable $e) {
    $onlineUsers = 0;
    $registeredUsers = 0;
}

$createdAt = date('d/m/Y');
if (isset($UData['account_created']) && is_numeric($UData['account_created']) && (int)$UData['account_created'] > 0) {
    $createdAt = date('d/m/Y', (int)$UData['account_created']);
}

$avatarLarge = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=l&head_direction=3&gesture=sml&action=wav';
$avatarSmall = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=m&head_direction=3&gesture=sml';
$avatarHead = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=s&head_direction=3&headonly=1&gesture=sml';

$onlinePreviewFigures = [
    'hr-100-61.hd-180-1.ch-210-66.lg-270-82.sh-290-80',
    'hr-515-33.hd-600-1.ch-635-70.lg-716-73.sh-907-64',
    'hr-165-45.hd-180-7.ch-255-64.lg-280-64.sh-300-64',
    'hr-890-37.hd-600-2.ch-665-64.lg-700-64.sh-730-64',
    'hr-110-61.hd-180-1.ch-255-70.lg-280-82.sh-290-80'
];

$eventEnd = time() + (2 * 86400) + (14 * 3600) + (37 * 60) + 52;
?>

<div class="rp-dashboard">
    <aside class="rp-sidebar" id="rp-sidebar">
        <a class="rp-sidebar-brand" href="<?php echo URL; ?>/me">
            <span class="brand-main">VELORA</span>
            <span class="brand-sub">RP</span>
            <i class="fas fa-city"></i>
        </a>

        <section class="rp-side-profile">
            <span class="profile-status"><i></i> En ligne</span>
            <div class="profile-avatar-orbit">
                <div class="profile-avatar-glow"></div>
                <img src="<?php echo htmlspecialchars($avatarLarge, ENT_QUOTES, 'UTF-8'); ?>" alt="Avatar de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>">
            </div>
            <strong><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong>
            <span><?php echo htmlspecialchars($citizenType, ENT_QUOTES, 'UTF-8'); ?></span>
        </section>

        <section class="rp-vitals">
            <div class="vital-row health">
                <div class="vital-top"><span><i class="fas fa-heart"></i> Santé</span><strong><?php echo $health; ?> / 100</strong></div>
                <div class="vital-track"><span style="width:<?php echo $health; ?>%"></span></div>
            </div>
            <div class="vital-row fatigue">
                <div class="vital-top"><span><i class="fas fa-bed"></i> Fatigue</span><strong><?php echo $fatigue; ?> / 100</strong></div>
                <div class="vital-track"><span style="width:<?php echo $fatigue; ?>%"></span></div>
            </div>
            <div class="vital-row energy">
                <div class="vital-top"><span><i class="fas fa-bolt"></i> Énergie</span><strong><?php echo $energy; ?> / 100</strong></div>
                <div class="vital-track"><span style="width:<?php echo $energy; ?>%"></span></div>
            </div>
            <div class="vital-row hygiene">
                <div class="vital-top"><span><i class="fas fa-tint"></i> Hygiène</span><strong><?php echo $hygiene; ?> / 100</strong></div>
                <div class="vital-track"><span style="width:<?php echo $hygiene; ?>%"></span></div>
            </div>
        </section>

        <section class="rp-side-section">
            <h3><span></span> MON TRAVAIL</h3>
            <div class="side-job">
                <span class="side-icon"><i class="fas fa-briefcase"></i></span>
                <div><strong><?php echo htmlspecialchars($jobName, ENT_QUOTES, 'UTF-8'); ?></strong><small><?php echo htmlspecialchars($jobRole, ENT_QUOTES, 'UTF-8'); ?></small></div>
            </div>
        </section>

        <section class="rp-side-section">
            <h3><span></span> MES DOCUMENTS</h3>
            <div class="side-document-grid">
                <button type="button" class="side-document"><i class="fas fa-id-card"></i><small>Carte d'identité</small></button>
                <button type="button" class="side-document"><i class="fas fa-address-card"></i><small>Permis</small></button>
            </div>
        </section>

        <section class="rp-side-section">
            <h3><span></span> MES VÉHICULES</h3>
            <div class="side-inventory vehicles">
                <button type="button" class="inventory-slot white"><i class="fas fa-car-side"></i></button>
                <button type="button" class="inventory-slot blue"><i class="fas fa-car"></i></button>
                <button type="button" class="inventory-slot cyan"><i class="fas fa-truck-pickup"></i></button>
                <button type="button" class="inventory-slot red"><i class="fas fa-car-side"></i></button>
                <button type="button" class="inventory-slot white"><i class="fas fa-shuttle-van"></i></button>
                <button type="button" class="inventory-slot orange"><i class="fas fa-car"></i></button>
                <button type="button" class="inventory-slot empty"><i class="fas fa-plus"></i></button>
            </div>
        </section>

        <section class="rp-side-section rp-objects">
            <h3><span></span> MES OBJETS</h3>
            <div class="side-inventory objects">
                <button type="button" class="inventory-slot violet"><i class="fas fa-gavel"></i><em>1</em></button>
                <button type="button" class="inventory-slot violet"><i class="fas fa-utensils"></i><em>1</em></button>
                <button type="button" class="inventory-slot violet"><i class="fas fa-crosshairs"></i><b>14</b></button>
                <button type="button" class="inventory-slot violet"><i class="fas fa-fire"></i><b>4</b></button>
            </div>
        </section>

        <a class="rp-logout" href="<?php echo URL; ?>/logout"><i class="fas fa-sign-out-alt"></i> DÉCONNEXION</a>
    </aside>

    <div class="rp-sidebar-overlay" id="rp-sidebar-overlay"></div>

    <div class="rp-workspace">
        <header class="rp-topbar">
            <button class="rp-menu-button" id="rp-menu-button" type="button" aria-label="Ouvrir le profil"><i class="fas fa-bars"></i></button>

            <nav class="rp-global-nav" aria-label="Navigation principale">
                <a class="active" href="<?php echo URL; ?>/me"><i class="fas fa-home"></i> ACCUEIL</a>
                <a href="<?php echo URL; ?>/online"><i class="fas fa-users"></i> COMMUNAUTÉ</a>
                <a href="<?php echo URL; ?>/store"><i class="fas fa-shopping-cart"></i> BOUTIQUE</a>
                <a href="<?php echo URL; ?>/leaderboards"><i class="fas fa-medal"></i> CLASSEMENT</a>
            </nav>

            <div class="rp-top-actions">
                <button type="button" class="rp-notification" aria-label="Notifications"><i class="far fa-bell"></i><b>3</b></button>
                <div class="rp-user-menu">
                    <button id="rp-user-toggle" type="button" class="rp-user-toggle" aria-expanded="false">
                        <span class="rp-user-head"><img src="<?php echo htmlspecialchars($avatarHead, ENT_QUOTES, 'UTF-8'); ?>" alt=""></span>
                        <span class="rp-user-copy"><strong><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong><small>Niveau <?php echo $level; ?></small></span>
                        <i class="fas fa-chevron-down"></i>
                    </button>
                    <div id="rp-user-dropdown" class="rp-user-dropdown">
                        <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="fas fa-user"></i> Mon profil</a>
                        <a href="<?php echo URL; ?>/account"><i class="fas fa-cog"></i> Paramètres</a>
                        <a class="danger" href="<?php echo URL; ?>/logout"><i class="fas fa-sign-out-alt"></i> Déconnexion</a>
                    </div>
                </div>
            </div>
        </header>

        <div class="rp-content-grid">
            <main class="rp-main-column">
                <section class="rp-hero-card">
                    <div class="rp-city-art" aria-hidden="true">
                        <span class="art-building a1"></span>
                        <span class="art-building a2"></span>
                        <span class="art-building a3"></span>
                        <span class="art-road"></span>
                        <span class="art-light l1"></span>
                        <span class="art-light l2"></span>
                        <span class="art-sign s1">VELORA</span>
                        <span class="art-sign s2">RP</span>
                    </div>
                    <div class="rp-hero-shade"></div>

                    <div class="rp-hero-copy">
                        <h1><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></h1>
                        <p><?php echo htmlspecialchars($motto, ENT_QUOTES, 'UTF-8'); ?></p>
                        <div class="rp-hero-meta">
                            <span><i class="far fa-calendar-alt"></i> Inscrit le <?php echo $createdAt; ?></span>
                            <span><i class="fas fa-tag"></i> Citoyen #<?php echo str_pad((string)$userId, 4, '0', STR_PAD_LEFT); ?></span>
                        </div>
                        <span class="rp-online-badge"><i></i> EN LIGNE</span>
                    </div>

                    <div class="rp-hero-avatar">
                        <div class="avatar-aura"></div>
                        <img src="<?php echo htmlspecialchars($avatarLarge, ENT_QUOTES, 'UTF-8'); ?>" alt="Avatar de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>">
                    </div>
                </section>

                <nav class="rp-profile-tabs" aria-label="Sections du profil">
                    <a class="active" href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="far fa-user"></i> MON PROFIL</a>
                    <a href="#"><i class="fas fa-home"></i> MA MAISON</a>
                    <a href="<?php echo URL; ?>/corporations"><i class="far fa-building"></i> MES ENTREPRISES</a>
                    <a href="<?php echo URL; ?>/gangs"><i class="far fa-flag"></i> MES FACTIONS</a>
                    <a href="#rp-activities"><i class="fas fa-history"></i> HISTORIQUE</a>
                </nav>

                <section class="rp-economy-grid">
                    <article class="economy-card money">
                        <span class="economy-icon"><i class="fas fa-wallet"></i></span>
                        <div><small>ARGENT EN POCHE</small><strong>$ <?php echo number_format($credits, 0, ',', ' '); ?></strong><span>Disponible</span></div>
                    </article>
                    <article class="economy-card bank">
                        <span class="economy-icon"><i class="fas fa-university"></i></span>
                        <div><small>ARGENT BANQUE</small><strong>$ <?php echo number_format($bank, 0, ',', ' '); ?></strong><span>En banque</span></div>
                    </article>
                    <article class="economy-card platinum">
                        <span class="economy-icon"><i class="fas fa-coins"></i></span>
                        <div><small>PLATINOS</small><strong><?php echo number_format($vipPoints, 0, ',', ' '); ?></strong><span>Platinos</span></div>
                    </article>
                    <article class="economy-card level">
                        <span class="economy-icon"><i class="fas fa-shield-alt"></i></span>
                        <div class="level-copy"><small>NIVEAU</small><strong><?php echo $level; ?></strong><span><?php echo number_format($xp); ?> / <?php echo number_format($xpGoal); ?> XP</span><div class="xp-track"><i style="width:<?php echo $xpPercent; ?>%"></i></div></div>
                    </article>
                </section>

                <section class="rp-panel rp-life-panel">
                    <div class="rp-panel-title"><i></i><strong>MA VIE À VELORA</strong></div>
                    <div class="rp-life-grid">
                        <a href="<?php echo URL; ?>/map"><span class="life-icon blue"><i class="fas fa-map-marker-alt"></i></span><div><strong>Carte</strong><small>Explore la ville</small></div></a>
                        <a href="<?php echo URL; ?>/corporations"><span class="life-icon cyan"><i class="fas fa-building"></i></span><div><strong>Entreprises</strong><small>Gère ton business</small></div></a>
                        <a href="<?php echo URL; ?>/gangs"><span class="life-icon aqua"><i class="fas fa-users"></i></span><div><strong>Organisations</strong><small>Rejoins une faction</small></div></a>
                        <a href="<?php echo URL; ?>/store"><span class="life-icon gold"><i class="fas fa-shopping-bag"></i></span><div><strong>Boutique</strong><small>Achète des objets</small></div></a>
                    </div>

                    <a class="rp-enter-city" href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener">
                        <div class="enter-city-art" aria-hidden="true"><span></span><span></span><span></span></div>
                        <div class="enter-city-shade"></div>
                        <div><strong>ENTRER EN VILLE</strong><small>Rejoins les autres citoyens dès maintenant</small></div>
                        <span class="enter-city-arrow"><i class="fas fa-arrow-right"></i></span>
                    </a>
                </section>

                <section class="rp-bottom-grid" id="rp-activities">
                    <article class="rp-panel activity-panel">
                        <div class="rp-panel-title plain"><strong>ACTIVITÉS RÉCENTES</strong></div>
                        <div class="activity-list">
                            <div><span class="activity-icon rent"><i class="fas fa-house-damage"></i></span><strong>Paiement de loyer</strong><small>Il y a 2 heures</small><b class="minus">- $250</b></div>
                            <div><span class="activity-icon salary"><i class="fas fa-briefcase"></i></span><strong>Salaire reçu · <?php echo htmlspecialchars($jobName, ENT_QUOTES, 'UTF-8'); ?></strong><small>Il y a 6 heures</small><b class="plus">+ $450</b></div>
                            <div><span class="activity-icon shop"><i class="fas fa-shopping-bag"></i></span><strong>Achat à la boutique</strong><small>Il y a 1 jour</small><b class="minus">- $120</b></div>
                        </div>
                        <a class="panel-footer-link" href="#">VOIR TOUTES LES ACTIVITÉS</a>
                    </article>

                    <article class="rp-panel server-panel">
                        <div class="rp-panel-title plain"><strong>INFOS SERVER</strong></div>
                        <div class="server-list">
                            <div><span><i class="far fa-user"></i> Citoyens en ligne</span><strong><?php echo number_format($onlineUsers); ?> / 500</strong></div>
                            <div><span><i class="fas fa-user-plus"></i> Inscriptions totales</span><strong><?php echo number_format($registeredUsers); ?></strong></div>
                            <div><span><i class="far fa-building"></i> Entreprises créées</span><strong>—</strong></div>
                            <div><span><i class="far fa-flag"></i> Factions actives</span><strong>—</strong></div>
                        </div>
                        <a class="panel-footer-link" href="<?php echo URL; ?>/leaderboards">VOIR LES STATISTIQUES</a>
                    </article>
                </section>
            </main>

            <aside class="rp-right-rail">
                <section class="rp-panel rp-event-panel" data-event-end="<?php echo $eventEnd * 1000; ?>">
                    <div class="rp-panel-title"><i></i><strong>PROCHAIN ÉVÉNEMENT</strong></div>
                    <div class="next-event-info">
                        <span class="next-event-icon"><i class="fas fa-crosshairs"></i></span>
                        <div><small>GUERRE DE TERRITOIRE</small><strong>Ghetto vs Downtown</strong></div>
                    </div>
                    <div class="event-countdown">
                        <div><strong id="rp-days">02</strong><span>JOURS</span></div>
                        <div><strong id="rp-hours">14</strong><span>HEURES</span></div>
                        <div><strong id="rp-minutes">37</strong><span>MINUTES</span></div>
                        <div><strong id="rp-seconds">52</strong><span>SECONDES</span></div>
                    </div>
                    <a class="rail-button" href="#">VOIR PLUS D'ÉVÉNEMENTS</a>
                </section>

                <section class="rp-panel rp-phone-panel">
                    <div class="rp-panel-title"><i></i><strong>CITY PHONE</strong></div>
                    <div class="phone-layout">
                        <div class="phone-app-grid">
                            <a href="#"><span class="app-icon green"><i class="fas fa-comment-dots"></i></span><small>Messages</small></a>
                            <a href="<?php echo URL; ?>/online"><span class="app-icon orange"><i class="fas fa-user"></i></span><small>Contacts</small></a>
                            <a href="#"><span class="app-icon teal"><i class="fas fa-university"></i></span><small>Banque</small></a>
                            <a href="<?php echo URL; ?>/corporations"><span class="app-icon purple"><i class="fas fa-briefcase"></i></span><small>Emplois</small></a>
                            <a href="<?php echo URL; ?>/map"><span class="app-icon blue"><i class="fas fa-map-marker-alt"></i></span><small>GPS</small></a>
                            <a href="<?php echo URL; ?>/account"><span class="app-icon gray"><i class="fas fa-cog"></i></span><small>Paramètres</small></a>
                        </div>
                        <div class="phone-device" aria-hidden="true">
                            <div class="phone-speaker"></div>
                            <div class="phone-screen"><span>VELORA</span><strong id="rp-phone-clock">00:00</strong><i class="fas fa-map-marked-alt"></i></div>
                            <div class="phone-home"></div>
                        </div>
                    </div>
                </section>

                <section class="rp-panel rp-discord-panel">
                    <div class="rp-panel-title"><i></i><strong>DISCORD OFFICIEL</strong></div>
                    <div class="discord-card">
                        <span class="discord-big"><i class="fab fa-discord"></i></span>
                        <div><strong>Rejoins notre communauté !</strong><small>+35 membres en ligne</small></div>
                    </div>
                    <a class="discord-join" href="<?php echo Config::$DiscordInvite; ?>" target="_blank" rel="noopener"><i class="fab fa-discord"></i> REJOINDRE</a>
                </section>

                <section class="rp-panel rp-online-panel">
                    <div class="rp-panel-title"><i></i><strong>JOUEURS EN LIGNE</strong><span><?php echo number_format($onlineUsers); ?></span></div>
                    <div class="online-avatar-row">
                        <?php foreach($onlinePreviewFigures as $figure):
                            $previewAvatar = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($figure) . '&size=s&head_direction=3&headonly=1&gesture=sml';
                        ?>
                            <span><img src="<?php echo htmlspecialchars($previewAvatar, ENT_QUOTES, 'UTF-8'); ?>" alt=""></span>
                        <?php endforeach; ?>
                        <?php if($onlineUsers > count($onlinePreviewFigures)): ?><b>+<?php echo $onlineUsers - count($onlinePreviewFigures); ?></b><?php endif; ?>
                    </div>
                    <a class="rail-button" href="<?php echo URL; ?>/online">VOIR TOUS LES JOUEURS</a>
                </section>
            </aside>
        </div>
    </div>
</div>

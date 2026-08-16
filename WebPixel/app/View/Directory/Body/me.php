<?php
/** Velora RP - screenshot-inspired RP dashboard */

$username = isset($UData['username']) ? $UData['username'] : 'Citoyen';
$look = isset($UData['look']) ? $UData['look'] : '';
$credits = isset($UData['credits']) ? (int)$UData['credits'] : 0;
$vipPoints = isset($UData['vip_points']) ? (int)$UData['vip_points'] : 0;
$rank = isset($UData['rank']) ? (int)$UData['rank'] : 1;
$bank = isset($UPData['bank']) ? (int)$UPData['bank'] : 0;
$level = isset($UPData['level']) ? (int)$UPData['level'] : 1;

$health = isset($UPData['health']) ? max(0, min(100, (int)$UPData['health'])) : 100;
$fatigue = isset($UPData['fatigue']) ? max(0, min(100, (int)$UPData['fatigue'])) : 0;
$energy = isset($UPData['energy']) ? max(0, min(100, (int)$UPData['energy'])) : 58;
$hygiene = isset($UPData['hygiene']) ? max(0, min(100, (int)$UPData['hygiene'])) : 92;

$jobName = isset($UPData['job_name']) && trim($UPData['job_name']) !== '' ? $UPData['job_name'] : 'Sans emploi';
$jobRole = isset($UPData['job_rank']) && trim($UPData['job_rank']) !== '' ? $UPData['job_rank'] : 'Citoyen';

$onlineUsers = 0;
$registeredUsers = 0;
try {
    $onlineUsers = (int)$UserMG->GetStatData('users_online');
    $registeredUsers = (int)$UserMG->GetStatData('users_registered');
} catch (Throwable $e) {
    $onlineUsers = 0;
    $registeredUsers = 0;
}

$avatarLarge = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=l&head_direction=3&gesture=sml&action=wav';
$avatarHead = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($look) . '&size=s&head_direction=3&headonly=1&gesture=sml';

$wanted = [
    ['name' => 'Lucas_92', 'reward' => 500, 'figure' => 'hr-100-61.hd-180-1.ch-210-66.lg-270-82.sh-290-80'],
    ['name' => 'Emma_RP', 'reward' => 400, 'figure' => 'hr-515-33.hd-600-1.ch-635-70.lg-716-73.sh-907-64'],
    ['name' => 'Yanis_13', 'reward' => 600, 'figure' => 'hr-165-45.hd-180-7.ch-255-64.lg-280-64.sh-300-64'],
    ['name' => 'Ninaaa', 'reward' => 450, 'figure' => 'hr-890-37.hd-600-2.ch-665-64.lg-700-64.sh-730-64']
];
?>

<div class="rp-layout">
    <aside class="rp-sidebar" id="rp-sidebar">
        <div class="sidebar-user">
            <img class="sidebar-head" src="<?php echo htmlspecialchars($avatarHead, ENT_QUOTES, 'UTF-8'); ?>" alt="">
            <div>
                <strong><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong>
                <span>ID #<?php echo isset($UData['id']) ? str_pad((string)$UData['id'], 4, '0', STR_PAD_LEFT) : '0001'; ?> · <?php echo $rank >= 5 ? 'Équipe' : 'Citoyen'; ?></span>
            </div>
            <span class="sidebar-online-dot"></span>
        </div>

        <div class="sidebar-profile-zone">
            <div class="sidebar-avatar-stage">
                <img src="<?php echo htmlspecialchars($avatarLarge, ENT_QUOTES, 'UTF-8'); ?>" alt="Avatar de <?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?>">
            </div>

            <div class="sidebar-vitals">
                <div class="vital vital-health">
                    <span class="vital-icon"><i class="fas fa-heart"></i></span>
                    <div class="vital-main"><div class="vital-label"><span>Santé</span><b><?php echo $health; ?>/100</b></div><div class="vital-track"><span style="width:<?php echo $health; ?>%"></span></div></div>
                </div>
                <div class="vital vital-fatigue">
                    <span class="vital-icon"><i class="fas fa-bed"></i></span>
                    <div class="vital-main"><div class="vital-label"><span>Fatigue</span><b><?php echo $fatigue; ?>/100</b></div><div class="vital-track"><span style="width:<?php echo $fatigue; ?>%"></span></div></div>
                </div>
                <div class="vital vital-energy">
                    <span class="vital-icon"><i class="fas fa-bolt"></i></span>
                    <div class="vital-main"><div class="vital-label"><span>Énergie</span><b><?php echo $energy; ?>/100</b></div><div class="vital-track"><span style="width:<?php echo $energy; ?>%"></span></div></div>
                </div>
                <div class="vital vital-hygiene">
                    <span class="vital-icon"><i class="fas fa-tint"></i></span>
                    <div class="vital-main"><div class="vital-label"><span>Hygiène</span><b><?php echo $hygiene; ?>/100</b></div><div class="vital-track"><span style="width:<?php echo $hygiene; ?>%"></span></div></div>
                </div>
            </div>
        </div>

        <section class="sidebar-section">
            <div class="sidebar-section-title"><span></span><strong>MON TRAVAIL</strong><span></span></div>
            <div class="job-card">
                <div><strong><?php echo htmlspecialchars($jobName, ENT_QUOTES, 'UTF-8'); ?></strong><span><?php echo htmlspecialchars($jobRole, ENT_QUOTES, 'UTF-8'); ?></span></div>
                <span class="job-logo"><i class="fas fa-briefcase"></i></span>
            </div>
        </section>

        <section class="sidebar-section">
            <div class="sidebar-section-title"><span></span><strong>MES DOCUMENTS</strong><span></span></div>
            <div class="document-grid">
                <button type="button" class="inventory-tile doc"><i class="fas fa-id-card"></i><small>IDENTITÉ</small></button>
                <button type="button" class="inventory-tile doc"><i class="fas fa-address-card"></i><small>PERMIS</small></button>
            </div>
        </section>

        <section class="sidebar-section">
            <div class="sidebar-section-title"><span></span><strong>MES VÉHICULES</strong><span></span></div>
            <div class="inventory-grid vehicles">
                <button type="button" class="inventory-tile red"><i class="fas fa-motorcycle"></i></button>
                <button type="button" class="inventory-tile blue"><i class="fas fa-car-side"></i></button>
                <button type="button" class="inventory-tile cyan"><i class="fas fa-car"></i></button>
                <button type="button" class="inventory-tile orange"><i class="fas fa-car-side"></i></button>
                <button type="button" class="inventory-tile light"><i class="fas fa-shuttle-van"></i></button>
                <button type="button" class="inventory-tile light"><i class="fas fa-car"></i></button>
                <button type="button" class="inventory-tile orange"><i class="fas fa-truck-pickup"></i></button>
                <button type="button" class="inventory-tile purple"><i class="fas fa-car-side"></i></button>
            </div>
        </section>

        <section class="sidebar-section sidebar-weapons">
            <div class="sidebar-section-title"><span></span><strong>MES OBJETS</strong><span></span></div>
            <div class="inventory-grid weapons">
                <button type="button" class="inventory-tile violet"><i class="fas fa-gavel"></i></button>
                <button type="button" class="inventory-tile violet"><i class="fas fa-utensils"></i></button>
                <button type="button" class="inventory-tile violet"><i class="fas fa-crosshairs"></i><em>74</em></button>
                <button type="button" class="inventory-tile violet"><i class="fas fa-fire"></i><em>4</em></button>
            </div>
        </section>

        <div class="sidebar-wallet">
            <div><small>PORTEFEUILLE</small><strong>$<?php echo number_format($credits); ?></strong></div>
            <div><small>BANQUE</small><strong>$<?php echo number_format($bank); ?></strong></div>
        </div>
    </aside>

    <div class="rp-main-shell">
        <header class="rp-header">
            <div class="header-left-actions">
                <a href="<?php echo URL; ?>/" class="round-action back"><i class="fas fa-chevron-left"></i></a>
                <button type="button" class="round-action help"><i class="fas fa-question"></i></button>
            </div>

            <a href="<?php echo URL; ?>/me" class="rp-logo">
                <span class="rp-logo-main">VELORA<span>RP</span></span>
                <small>ROLEPLAY</small>
            </a>

            <div class="header-online-count"><i class="fas fa-user-friends"></i><strong><?php echo number_format($onlineUsers); ?></strong><span>civils dans la ville</span></div>

            <div class="header-round-actions">
                <a href="<?php echo URL; ?>/store" class="header-orb gold"><i class="fas fa-coins"></i></a>
                <a href="<?php echo URL; ?>/online" class="header-orb"><i class="fas fa-comment-alt"></i></a>
                <a href="<?php echo URL; ?>/corporations" class="header-orb orange"><i class="fas fa-briefcase"></i></a>
                <a href="<?php echo URL; ?>/account" class="header-orb green"><i class="fas fa-gift"></i></a>
            </div>

            <div class="header-city-art" aria-hidden="true">
                <span class="city-building cb1"></span>
                <span class="city-building cb2"></span>
                <span class="city-building cb3"></span>
                <span class="city-tree ct1"></span>
                <span class="city-tree ct2"></span>
                <span class="city-road"></span>
            </div>
        </header>

        <nav class="rp-tabs" id="rp-tabs">
            <a href="<?php echo URL; ?>/profile/<?php echo rawurlencode($username); ?>"><i class="fas fa-user-circle"></i><span>MOI</span></a>
            <a class="active" href="<?php echo URL; ?>/me"><i class="fas fa-home"></i><span>ACCUEIL</span></a>
            <a href="<?php echo URL; ?>/account"><i class="fas fa-cog"></i><span>PARAMÈTRES</span></a>
            <a href="<?php echo URL; ?>/corporations"><i class="fas fa-box"></i><span>TRAVAUX</span></a>
            <button id="rp-mobile-sidebar" type="button" class="rp-mobile-sidebar"><i class="fas fa-bars"></i><span>PROFIL</span></button>
        </nav>

        <main class="rp-content">
            <section class="event-zone">
                <div class="section-line-title"><span></span><strong><i class="fas fa-coins"></i> TIRAGE VELORA</strong><span></span></div>

                <div class="event-title-row">
                    <div class="event-character"><img src="<?php echo htmlspecialchars($avatarLarge, ENT_QUOTES, 'UTF-8'); ?>" alt=""></div>
                    <div class="event-title-copy">
                        <small>ÉVÉNEMENT COMMUNAUTAIRE</small>
                        <h1>15 CRÉDITS <span>À GAGNER</span></h1>
                    </div>
                </div>

                <div class="event-meta-row">
                    <div><small>Dernier gagnant</small><strong><?php echo htmlspecialchars($username, ENT_QUOTES, 'UTF-8'); ?></strong></div>
                    <div><small>Participants</small><strong><?php echo max(2, $onlineUsers); ?></strong></div>
                </div>

                <div class="countdown-card">
                    <div class="countdown-scene" aria-hidden="true">
                        <span class="scene-wall wall-a"></span>
                        <span class="scene-wall wall-b"></span>
                        <span class="scene-plant plant-a"></span>
                        <span class="scene-plant plant-b"></span>
                        <span class="scene-sofa"></span>
                        <span class="scene-table"></span>
                    </div>
                    <div class="countdown-shade"></div>
                    <div class="countdown-grid">
                        <div><strong id="count-days">02</strong><span>JOURS</span></div>
                        <div><strong id="count-hours">01</strong><span>HEURES</span></div>
                        <div><strong id="count-minutes">27</strong><span>MINUTES</span></div>
                        <div><strong id="count-seconds">27</strong><span>SECONDES</span></div>
                    </div>
                </div>
            </section>

            <section class="bottom-grid">
                <article class="wanted-panel">
                    <div class="lower-heading"><span></span><strong><i class="fas fa-search"></i> QUELQUES CIVILS RECHERCHÉS</strong><span></span></div>
                    <div class="wanted-grid">
                        <?php foreach($wanted as $person):
                            $wantedImg = 'https://nitro-imager.kubbo.ch/?figure=' . rawurlencode($person['figure']) . '&size=l&head_direction=3&gesture=sml';
                        ?>
                            <div class="wanted-card">
                                <div class="wanted-avatar"><img src="<?php echo htmlspecialchars($wantedImg, ENT_QUOTES, 'UTF-8'); ?>" onerror="this.src='<?php echo htmlspecialchars($avatarLarge, ENT_QUOTES, 'UTF-8'); ?>'" alt=""></div>
                                <strong><?php echo htmlspecialchars($person['name'], ENT_QUOTES, 'UTF-8'); ?></strong>
                                <span><i class="fas fa-money-bill-wave"></i> <?php echo number_format($person['reward']); ?> $</span>
                            </div>
                        <?php endforeach; ?>
                    </div>
                </article>

                <article class="discord-panel">
                    <div class="lower-heading"><span></span><strong><i class="fab fa-discord"></i> NOTRE DISCORD</strong><span></span></div>
                    <div class="discord-widget">
                        <div class="discord-top"><span class="discord-logo"><i class="fab fa-discord"></i></span><div><strong>Velora RP · Communauté</strong><small><i></i> Communauté active</small></div></div>
                        <p>Rejoins le Discord pour suivre les annonces, participer aux événements et organiser ton RP avec les autres citoyens.</p>
                        <a href="<?php echo Config::$DiscordInvite; ?>" target="_blank" rel="noopener"><i class="fab fa-discord"></i> REJOINDRE LE DISCORD</a>
                    </div>
                </article>
            </section>
        </main>

        <footer class="rp-footer">
            <span>© <?php echo date('Y'); ?> <?php echo strtoupper(Config::$WName); ?></span>
            <span><i class="fas fa-circle"></i> SESSION ACTIVE · NIVEAU <?php echo $level; ?> · <?php echo number_format($registeredUsers); ?> CITOYENS</span>
        </footer>
    </div>
</div>
<div class="sidebar-mobile-overlay" id="sidebar-mobile-overlay"></div>

<?php
/**
 * ParadiseRP dashboard.
 */
?>

<div class="content">
    <div class="container">
        <section class="dashboard-hero">
            <div class="dashboard-welcome">
                <img
                    src="https://www.habbo.es/habbo-imaging/avatarimage?figure=<?php echo rawurlencode($UData['look']); ?>&amp;size=l&amp;direction=2&amp;head_direction=3&amp;gesture=sml"
                    alt="<?php echo htmlspecialchars($UData['username'], ENT_QUOTES, 'UTF-8'); ?>"
                    loading="eager"
                    referrerpolicy="no-referrer"
                >
                <div>
                    <span>TABLEAU DE BORD</span>
                    <h1>Salut, <?php echo htmlspecialchars($UData['username'], ENT_QUOTES, 'UTF-8'); ?> !</h1>
                    <p>Bienvenue sur <?php echo htmlspecialchars(Config::$WName, ENT_QUOTES, 'UTF-8'); ?>. Gère ton personnage et retrouve toutes les activités de la ville.</p>
                </div>
            </div>
            <div class="dashboard-cta">
                <strong><?php echo htmlspecialchars(Config::$WName, ENT_QUOTES, 'UTF-8'); ?></strong>
                <span><i class="fas fa-circle"></i> <?php echo $UserMG->GetStatData('users_online'); ?> citoyens en ligne</span>
                <a href="<?php echo Config::$URL; ?>/play" target="_blank" rel="noopener">Entrer dans le jeu <i class="fas fa-arrow-right"></i></a>
            </div>
        </section>

        <section class="dashboard-metrics">
            <div><img class="metric-icon metric-wallet" src="<?php echo DY; ?>/img/logos/icon/portefeuille.png" alt=""><span><strong>$<?php echo number_format($UData['credits']); ?></strong>Portefeuille</span></div>
            <div><i class="fas fa-university"></i><span><strong>$<?php echo number_format($UPData['bank']); ?></strong>Banque</span></div>
            <div><i class="fas fa-star"></i><span><strong><?php echo $UPData['level']; ?></strong>Niveau</span></div>
            <div><i class="fas fa-users"></i><span><strong><?php echo $UserMG->GetStatData('users_online'); ?></strong>Connectés</span></div>
        </section>

        <div class="row">
            <div class="col-6">
                <div class="content-box">
                    <div class="title"><i class="fas fa-id-card text-secondary"></i> Statistiques de jeu</div>
                    <div class="box-content">
                        <div class="user-info-pz">
                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center"><div><div class="user-pz-dinero"></div></div></div>
                                    <div class="post-info mr-auto ml-2"><b>Portefeuille :</b> $<?php echo number_format($UData['credits']); ?></div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center"><div><div class="user-pz-banco"></div></div></div>
                                    <div class="post-info mr-auto ml-2"><b>Banque :</b> $<?php echo number_format($UPData['bank']); ?></div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center"><div><div class="user-pz-platinos"></div></div></div>
                                    <div class="post-info mr-auto ml-2"><b>Platinos :</b> <?php echo $UData['vip_points']; ?></div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center"><div><div class="user-pz-level"></div></div></div>
                                    <div class="post-info mr-auto ml-2"><b>Niveau :</b> <?php echo $UPData['level']; ?></div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="content-box">
                    <div class="title"><img class="title-custom-icon" src="<?php echo DY; ?>/img/logos/icon/speaker.png" alt=""> Informations</div>
                    <div class="box-content p-3">
                        <p class="mb-0">Bienvenue sur ParadiseRP. Les annonces et activités de la ville apparaîtront ici.</p>
                    </div>
                </div>

                <div class="content-box">
                    <iframe
                        src="https://discord.com/widget?id=857370545111826452&theme=dark"
                        width="540"
                        height="415"
                        allowtransparency="true"
                        frameborder="0"
                        loading="lazy"
                        title="Discord ParadiseRP"
                    ></iframe>
                </div>
            </div>

            <div class="col-6">
                <?php require_once WIDGETS . 'Top3Money.php'; ?>
                <?php require_once WIDGETS . 'ServerStats.php'; ?>
            </div>
        </div>
    </div>
</div>

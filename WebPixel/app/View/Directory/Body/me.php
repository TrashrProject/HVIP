<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

 $GameStats = $UserMG->GetUserGameStats($UData['id']);
 ?>

<div class="content">

    <div class="container">

        <section class="dashboard-hero">
            <div class="dashboard-welcome">
                <img src="<?php echo URL; ?>/avatar.php?figure=<?php echo rawurlencode($UData['look']); ?>&amp;size=l&amp;direction=3&amp;head_direction=3&amp;gesture=sml&amp;v=<?php echo rawurlencode($UData['look']); ?>" alt="<?php echo htmlspecialchars($UData['username'], ENT_QUOTES, 'UTF-8'); ?>">
                <div>
                    <span>TABLEAU DE BORD</span>
                    <h1>Salut, <?php echo htmlspecialchars($UData['username'], ENT_QUOTES, 'UTF-8'); ?> !</h1>
                    <p>Bienvenue sur <?php echo htmlspecialchars(Config::$WName, ENT_QUOTES, 'UTF-8'); ?>. Gère ton personnage et retrouve toutes les activités de la ville.</p>
                </div>
            </div>
            <div class="dashboard-cta">
                <strong><?php echo htmlspecialchars(Config::$WName, ENT_QUOTES, 'UTF-8'); ?></strong>
                <span><i class="fas fa-circle"></i> <?php echo $UserMG->GetStatData('users_online'); ?> citoyens en ligne</span>
                <a href="<?php echo Config::$URL; ?>/play" target="_blank">Entrer dans le jeu <i class="fas fa-arrow-right"></i></a>
            </div>
        </section>

        <section class="dashboard-metrics">
            <div><img class="metric-icon metric-wallet" src="<?php echo DY; ?>/img/logos/icon/portefeuille.png" alt=""><span><strong>$<?php echo number_format($GameStats['credits']); ?></strong>Portefeuille</span></div>
            <div><i class="fas fa-university"></i><span><strong>$<?php echo number_format($GameStats['bank']); ?></strong>Banque RP</span></div>
            <div><i class="fas fa-star"></i><span><strong><?php echo number_format($GameStats['experience']); ?></strong>Expérience RP</span></div>
            <div><i class="fas fa-users"></i><span><strong><?php echo $UserMG->GetStatData('users_online'); ?></strong>Connectés</span></div>
        </section>


        <section class="cms-news">
            <div class="cms-news-head"><div><span>LE JOURNAL DE LA VILLE</span><h2>Dernières actualités</h2></div><a href="<?php echo URL; ?>/actualites">Toutes les actualités <i class="fas fa-arrow-right"></i></a></div>
            <div class="cms-news-grid">
                <?php if(mysqli_num_rows($LatestArticles)): while($ArticleRow=mysqli_fetch_assoc($LatestArticles)):
                    $cover=!empty($ArticleRow['image_url'])?URL.'/'.$ArticleRow['image_url']:DY.'/img/backgrounds/bg.jpg'; ?>
                <article class="cms-news-card"><a class="cms-news-cover" href="<?php echo URL; ?>/article/<?php echo rawurlencode($ArticleRow['slug']); ?>" style="background-image:url('<?php echo htmlspecialchars($cover,ENT_QUOTES,'UTF-8'); ?>')"><span>ACTUALITÉ</span></a><div class="cms-news-body"><small>Par <?php echo htmlspecialchars($ArticleRow['author_name']?:'Équipe ParadiseRP',ENT_QUOTES,'UTF-8'); ?> · <?php echo date('d/m/Y',(int)$ArticleRow['published_at']); ?></small><h3><?php echo htmlspecialchars($ArticleRow['title'],ENT_QUOTES,'UTF-8'); ?></h3><p><?php echo htmlspecialchars($ArticleRow['summary'],ENT_QUOTES,'UTF-8'); ?></p><a href="<?php echo URL; ?>/article/<?php echo rawurlencode($ArticleRow['slug']); ?>">Lire l’article <i class="fas fa-chevron-right"></i></a></div></article>
                <?php endwhile; else: ?><div class="cms-news-empty">Aucune actualité publiée pour le moment.</div><?php endif; ?>
            </div>
        </section>

        <div class="row">
            <div class="col-6">
                <div class="content-box">
                    <div class="title">
                        <i class="fas fa-id-card text-secondary"></i> Statistiques de jeu
                    </div>
                    <div class="box-content">
                        <div class="user-info-pz">
                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-dinero"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Portefeuille :</b> $<?php echo number_format($GameStats['credits']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-banco"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Banque RP :</b> $<?php echo number_format($GameStats['bank']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-platinos"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Diamants :</b> <?php echo number_format($GameStats['vip_points']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-level"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Expérience RP :</b> <?php echo number_format($GameStats['experience']); ?> XP
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>

                <div class="content-box">
                    <div class="title"><img class="title-custom-icon" src="<?php echo DY; ?>/img/logos/icon/speaker.png" alt=""> Informations</div>
                    <div class="box-content p-3">
                        <p class="mb-0">Bienvenue sur ParadiseRP. Les annonces et activit&eacute;s de la ville appara&icirc;tront ici.</p>
                    </div>
                </div>

                

                <div class="content-box">
                    <iframe src="https://discord.com/widget?id=857370545111826452&theme=dark" width="540" height="415" allowtransparency="true" frameborder="0"></iframe>
                </div>
                <!--<div class="peakrp-news d-flex justify-content-center align-items-center">
                    <div class="mr-auto">Noticías <?php echo Config::$WName; ?> ...</div>
                    <div><img src="https://peakrp.com/img/website/icons/news_list.gif"></div>
                </div>
                <div class="d-flex flex-column mt-2">
                    <div class="flex-fill pb-2">
                        <a class="news-item d-flex flex-column justify-content-between" href="https://forums.peakrp.com/threads/.1300" style="background-image: url('https://forums.peakrp.com/attachments/1248');">
                            <div class="d-flex justify-content-end">
                                <div class="news-stats">
                                    <span class="pr-2"><img src="https://peakrp.com/img/website/icons/likes.gif"> 18</span>
                                    <img src="https://peakrp.com/img/website/icons/comments.gif"> 44
                                </div>
                            </div>
                            <div class="news-title d-flex justify-content-center">
                                <div class="ml-3 flex-grow-1">Nouvelle entreprise - Candidatures de responsables</div>
                                <div class="mr-3"><i class="fas fa-certificate"></i></div>
                            </div>
                        </a>
                    </div>
                    <div class="flex-fill pb-2">
                        <a class="news-item d-flex flex-column justify-content-between" href="https://forums.peakrp.com/threads/.829" style="background-image: url('https://forums.peakrp.com/attachments/864');">
                            <div class="d-flex justify-content-end">
                                <div class="news-stats">
                                    <span class="pr-2"><img src="https://peakrp.com/img/website/icons/likes.gif"> 24</span>
                                    <img src="https://peakrp.com/img/website/icons/comments.gif"> 26
                                </div>
                            </div>
                            <div class="news-title d-flex justify-content-center">
                                <div class="ml-3 flex-grow-1">PeakRP Update 2</div>
                                <div class="mr-3"><i class="fas fa-certificate"></i></div>
                            </div>
                        </a>
                    </div>
                    <div class="flex-fill pb-2">
                        <a class="news-item d-flex flex-column justify-content-between" href="https://forums.peakrp.com/threads/.751" style="background-image: url('https://forums.peakrp.com/attachments/835');">
                            <div class="d-flex justify-content-end">
                                <div class="news-stats">
                                    <span class="pr-2"><img src="https://peakrp.com/img/website/icons/likes.gif"> 7</span>
                                    <img src="https://peakrp.com/img/website/icons/comments.gif"> 8
                                </div>
                            </div>
                            <div class="news-title d-flex justify-content-center">
                                <div class="ml-3 flex-grow-1">Scheduled maintenance</div>
                                <div class="mr-3"><i class="fas fa-certificate"></i></div>
                            </div>
                        </a>
                    </div>
                </div>
                <div class="d-flex mb-2">
                    <a href="https://forums.peakrp.com/" class="twitter jumbo-link d-flex justify-content-center align-items-center">
                        <div class="mr-auto">Forum</div>
                        <div class=""><i class="fas fa-edit"></i></div>
                    </a>
                    <a href="https://wiki.peakrp.com/" class="forum jumbo-link d-flex justify-content-center align-items-center">
                        <div class="mr-auto">Wiki</div>
                        <div class=""><i class="fas fa-info"></i></div>
                    </a>
                    <a href="https://peakrp.com/account" class="discord jumbo-link d-flex justify-content-center align-items-center">
                        <div class="mr-auto">Discord Link</div>
                        <div class=""><i class="fab fa-discord font-weight-normal"></i></div>
                    </a>
                </div>-->
            </div>
            <div class="col-6">
                <?php require_once WIDGETS . 'Top3Money.php'; ?>
                <?php require_once WIDGETS . 'ServerStats.php'; ?>
            </div>
        </div>


    </div>
    <center><script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
<!-- Responsive -->
<ins class="adsbygoogle"
     style="display:block"
     data-ad-client="ca-pub-5384077970237124"
     data-ad-slot="7246095666"
     data-ad-format="auto"
     data-full-width-responsive="true"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script></center>
</div>

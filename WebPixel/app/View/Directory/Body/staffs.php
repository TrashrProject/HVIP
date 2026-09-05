<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */
 ?>
<div class="content">

    <div class="container">


        <div class="container">
            <div class="row">
                <div class="col-8">
                    <div class="staff-grid">
                        <?php
                        $R_ = $UserMG->GetStaffs();
                        $CurrentStaffRole = null;
                        while($Row = mysqli_fetch_assoc($R_)):
                            $RawRankName = trim((string)($Row['rank_name'] ?? ('Rang '.$Row['rank'])));
                            $RankKey = strtolower($RawRankName);

                            // Affichage canonique ParadiseRP : on ne sépare jamais Owner et Fondateur.
                            // Les variantes historiques/anglaises restent reconnues pour les anciennes DB.
                            if(strpos($RankKey, 'owner') !== false || strpos($RankKey, 'founder') !== false || strpos($RankKey, 'fondateur') !== false):
                                $RankName = 'Fondateur';
                                $RankRoleKey = 'fondateur';
                                $BadgeFile = 'FOND.png';
                            elseif(strpos($RankKey, 'developer') !== false || strpos($RankKey, 'développeur') !== false || strpos($RankKey, 'developpeur') !== false):
                                $RankName = 'Développeur';
                                $RankRoleKey = 'developpeur';
                                $BadgeFile = 'DEV.png';
                            elseif(strpos($RankKey, 'administrator') !== false || strpos($RankKey, 'administrateur') !== false || strpos($RankKey, 'manager') !== false):
                                $RankName = 'Administrateur';
                                $RankRoleKey = 'administrateur';
                                $BadgeFile = 'ADM.png';
                            elseif(strpos($RankKey, 'moderator') !== false || strpos($RankKey, 'modérateur') !== false || strpos($RankKey, 'moderateur') !== false):
                                $RankName = 'Modérateur';
                                $RankRoleKey = 'moderateur';
                                $BadgeFile = 'MOD.png';
                            else:
                                $RankName = 'Staff';
                                $RankRoleKey = 'staff';
                                $BadgeFile = 'STAFF.png';
                            endif;

                            $BadgePath = dirname(__DIR__, 4) . '/Dynamics/img/staff/normalized/' . $BadgeFile;
                            if($CurrentStaffRole !== $RankRoleKey):
                                $CurrentStaffRole = $RankRoleKey;
                        ?>
                        <div class="staff-rank-heading"><span><?php echo htmlspecialchars($RankName, ENT_QUOTES, 'UTF-8'); ?></span></div>
                        <?php endif; ?>
                        <a href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" class="content-box mb-0 no-link-styling" style="<?php echo ($Row['online'] == '1')? "border-bottom: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="title d-flex align-items-center" style="">
                                <div class="mr-auto" style="padding-left: 115px;"><?php echo htmlspecialchars($Row['username'], ENT_QUOTES, 'UTF-8'); ?></div>
                                <div><span class="float-right pr-3"></span></div>
                            </div>
                            <div class="d-flex align-items-center">
                                <div class="staff-member-avatar">
                                    <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo urlencode($Row['look']); ?>&amp;direction=3&amp;size=l&amp;gesture=sml">
                                </div>
                                <div class="mr-auto">
                                    <div class="staff-role text-white">
                                        <?php echo htmlspecialchars($RankName, ENT_QUOTES, 'UTF-8'); ?>

                                    </div>
                                </div>
                                <div class="mr-3"><img class="staff-rank-badge" src="<?php echo DY; ?>/img/staff/normalized/<?php echo $BadgeFile; ?>?v=<?php echo is_file($BadgePath) ? filemtime($BadgePath) : time(); ?>" alt="<?php echo htmlspecialchars($RankName, ENT_QUOTES, 'UTF-8'); ?>"></div>
                            </div>
                        </a>

                        <?php endwhile; ?>

                    </div>
                </div>
                <div class="col-4 pl-0">
                    <div class="help-grid">
                        <div class="content-box">
                            <div class="title">Besoin d'aide ?</div>
                            <div class="p-3">
                                Le moyen le plus simple de nous contacter est notre Discord officiel : clique <a href="<?php echo Config::$DiscordInvite; ?>" target="_blank">ICI</a> pour le rejoindre.<br>
                                Tu peux aussi contacter l'&eacute;quipe staff pour tes questions. Utilise <b>:n [question]</b> pour envoyer une demande d'aide g&eacute;n&eacute;rale.
                                <center><img class="pt-3" src="<?php echo IMG; ?>/extras/frank_signs.gif"></center>
                                <hr>

                                Si tu ne peux pas entrer dans le jeu, utilise notre <a class="text-white font-weight-bold text-decoration-none" href="<?php echo Config::$DiscordInvite; ?>" target="_blank">Discord</a> pour nous envoyer un message.
                            </div>
                        </div>
                        <script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
<!-- Responsive -->
<ins class="adsbygoogle"
     style="display:block"
     data-ad-client="ca-pub-5384077970237124"
     data-ad-slot="7246095666"
     data-ad-format="auto"
     data-full-width-responsive="true"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script>
                    </div>
                </div>
            </div>
        </div>


    </div>
</div>

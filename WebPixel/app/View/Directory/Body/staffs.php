<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * ParadiseRP staff page.
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

                        // Les niveaux correspondent aux groupes de permissions de la DB.
                        // Le nom affiche reste donc coherent meme si une ancienne DB contient
                        // encore un libelle historique (Owner, Moderator, etc.).
                        $StaffRanks = array(
                            7 => array('name' => 'Fondateur',      'key' => 'fondateur',      'badge' => 'FOND.png'),
                            6 => array('name' => 'Développeur',    'key' => 'developpeur',    'badge' => 'DEV.png'),
                            5 => array('name' => 'Administrateur', 'key' => 'administrateur', 'badge' => 'ADM.png'),
                            4 => array('name' => 'Modérateur',     'key' => 'moderateur',     'badge' => 'MOD.png'),
                            3 => array('name' => 'Staff',          'key' => 'staff',          'badge' => 'STAFF.png')
                        );

                        while($Row = mysqli_fetch_assoc($R_)):
                            $RankLevel = (int)($Row['rank_level'] ?? $Row['rank']);
                            if(!isset($StaffRanks[$RankLevel])):
                                continue;
                            endif;

                            $Rank = $StaffRanks[$RankLevel];
                            $RankName = $Rank['name'];
                            $RankRoleKey = $Rank['key'];
                            $BadgeFile = $Rank['badge'];
                            $BadgePath = dirname(__DIR__, 4) . '/Dynamics/img/staff/normalized/' . $BadgeFile;

                            if($CurrentStaffRole !== $RankRoleKey):
                                $CurrentStaffRole = $RankRoleKey;
                        ?>
                        <div class="staff-rank-heading"><span><?php echo htmlspecialchars($RankName, ENT_QUOTES, 'UTF-8'); ?></span></div>
                        <?php endif; ?>

                        <a href="<?php echo Config::$URL; ?>/profile/<?php echo (int)$Row['id']; ?>" class="content-box mb-0 no-link-styling" style="<?php echo ($Row['online'] == '1') ? 'border-bottom: 3px solid #1dc40e;' : ''; ?>">
                            <div class="title d-flex align-items-center">
                                <div class="mr-auto" style="padding-left: 115px;"><?php echo htmlspecialchars($Row['username'], ENT_QUOTES, 'UTF-8'); ?></div>
                                <div><span class="float-right pr-3"></span></div>
                            </div>
                            <div class="d-flex align-items-center">
                                <div class="staff-member-avatar">
                                    <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo urlencode($Row['look']); ?>&amp;direction=3&amp;size=l&amp;gesture=sml">
                                </div>
                                <div class="mr-auto">
                                    <div class="staff-role text-white"><?php echo htmlspecialchars($RankName, ENT_QUOTES, 'UTF-8'); ?></div>
                                </div>
                                <div class="mr-3">
                                    <img class="staff-rank-badge" src="<?php echo DY; ?>/img/staff/normalized/<?php echo $BadgeFile; ?>?v=<?php echo is_file($BadgePath) ? filemtime($BadgePath) : time(); ?>" alt="<?php echo htmlspecialchars($RankName, ENT_QUOTES, 'UTF-8'); ?>">
                                </div>
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
                        <ins class="adsbygoogle" style="display:block" data-ad-client="ca-pub-5384077970237124" data-ad-slot="7246095666" data-ad-format="auto" data-full-width-responsive="true"></ins>
                        <script>(adsbygoogle = window.adsbygoogle || []).push({});</script>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<?php
/**
 * ParadiseRP server statistics widget.
 */
?>

<div class="content-box">
    <div class="title">
        <i class="fas fa-chart-bar text-secondary"></i> Statistiques de <?php echo Config::$WName; ?>
    </div>
    <div class="box-content">
        <div class="user-info-pz">
            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-onlines"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Citoyens connect&eacute;s :</b> <?php echo $UserMG->GetStatData("users_online"); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>

            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-users"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Citoyens inscrits :</b> <?php echo $UserMG->GetStatData("users_registered"); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>

            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-money"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Argent total :</b> $<?php echo $UserMG->GetStatData(); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>

            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-apts"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Appartements :</b> <?php echo $UserMG->GetStatData("apts_total"); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>

            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-business"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Entreprises :</b> <?php echo $UserMG->GetStatData("business_count"); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>

            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-gangs"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Gangs :</b> <?php echo $UserMG->GetStatData("gangs_total"); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>

            <div class="server-pz-stats">
                <div class="d-flex justify-content-center align-items-center">
                    <div class="author-avatar text-center">
                        <div><div class="stats-pz-build"></div></div>
                    </div>
                    <div class="post-info mr-auto ml-2">
                        <b>Version actuelle :</b> <?php echo $UserMG->GetStatData("edit_version"); ?>
                    </div>
                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                </div>
            </div>
        </div>
    </div>
</div>

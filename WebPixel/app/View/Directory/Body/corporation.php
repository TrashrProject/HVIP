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



        <div class="row">
            <div class="col-3">
                <div class="content-box" style="margin-bottom: 10px;">
                    <div class="title text-center"><?php echo $Corp->CorpData['name']; ?></div>
                    <div class="box-content text-center">
                        <div><img class="corporation-badge profile-level-box" style="padding: 15px;" src="<?php echo Config::$SWF; ?>/habbo-imaging/corp/<?php echo $Corp->CorpData['badge']; ?>.gif" ></div>
                        <div></div>
                        <div class="text-center p-2">
                            <ul class="list-group list-group-flush">
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Employés
                                    <span class="badge badge-secondary badge-pill peak"><?php echo $Corp->GetCorpEmployeesCount(); ?></span>
                                </li>
                                <?php if($Corp->CorpData['type'] == "1"): ?>
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Valeur totale
                                    <span class="badge badge-secondary badge-pill peak">$<?php echo number_format($Corp->CorpData['bank']); ?></span>
                                </li>
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Services terminés
                                    <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Corp->CorpData['shifts']); ?></span>
                                </li>

                                <?php endif; ?>

                            </ul>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col">
                <?php
                $CorpRanks_ = $Corp->GetCorpRanks();
                while($Ranks = mysqli_fetch_assoc($CorpRanks_)): ?>
                <div class="content-box">
                    <div class="title"><?php echo $Ranks['name']; ?></div>
                    <div class="box-content">
                        <div class="corporation-employee-grid">
                            <?php
                                $R = $Corp->GetCorpEmployeesByRank($Ranks['rank']);
                                if(mysqli_num_rows($R) > 0):
                                while ($E = mysqli_fetch_assoc($R)):
                                    $EU = mysqli_fetch_assoc($DB->Query("SELECT id, username, look, online FROM users WHERE id = ".$E['user_id']." LIMIT 1"));
                            ?>
                            <a href="<?php echo Config::$URL; ?>/profile/<?php echo $EU['id']; ?>" class="employee d-flex justify-content-center align-items-center no-link-styling rank-">
                                <div class="employee-pixel <?php echo ($EU['online'] == '0')? "grayscale" :  "" ; ?>">
                                    <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $EU['look']; ?>&amp;head_direction=3&amp;gesture=sml">
                                </div>
                                <div>
                                    <span class="font-weight-bold"><?php echo $EU['username']; ?></span>
                                    <div class="employee-stats" style="font-size: 11px;">

                                    </div>
                                </div>
                                <div class="d-flex align-items-center justify-content-center flex-fill">
                                    <img src="<?php echo DY; ?>/img/icons/<?php echo ($EU['online'] == '1')? "online" : "offline" ; ?>.gif">
                                </div>
                            </a>
                            <?php endwhile; else: ?>
                            <center><b>Aucun citoyen n’occupe ce poste.</b></center>
                            <?php endif; ?>


                        </div>
                    </div>
                    
                </div>
                <?php endwhile; ?>

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

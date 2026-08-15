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
        <div class="container d-flex">
            <div class="col-3">
                <div class="content-box">
                    <div class="title">Buscar Banda</div>
                    <div class="box-content">
                        <div class="form-group">
                            <label></label>
                            <center><input type="text" placeholder="Escribir nombre a buscar" class="form-control" name="gang-lookup" style="max-width: 85%;"></center>
                        </div>
                    </div>
                </div>
                <div class="content-box blue leaderboards mb-0">
                    <div class="title">M&aacute;s dinero</div>
                    <div class="box-content">
                        <?php $G = $GangMG->GetLeaderBoard("groups", "bank");
                        if(mysqli_num_rows($G) >= 1):
                            while ($Gang = mysqli_fetch_assoc($G)): ?>
                                <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                    <div class="gang-colours p-2">
                                        <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                    </div>
                                    <div class="leaderboard-user-info mr-auto">
                                        <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                        <div class="leaderboard-user-stat">Riqueza $<?php echo number_format($Gang['bank']); ?></div>
                                    </div>
                                </a>
                            <?php endwhile; else: echo "No hay bandas en esta categoria"; endif; ?>

                    </div>
                </div>
            </div>
            <div class="col-9" id="gangs-results">
                <?php $Gangs = $DB->Query("SELECT * FROM groups WHERE type = '3'");
                if(mysqli_num_rows($Gangs) >= 1): ?>
            <div class="corporation-index-grid" >


               <?php while ($Gang = mysqli_fetch_assoc($Gangs)):
                    $ECount = mysqli_num_rows($DB->Query("SELECT null FROM group_memberships WHERE group_id = " . $Gang['id'] . " "));
                    ?>
                    <div class="corp">
                        <div class="content-box">
                            <div class="title"><?php echo utf8_encode($Gang['name']); ?> <span
                                        class="title-small float-right"><small><?php echo $ECount; ?> Miembros</small></span>
                            </div>
                            <div class="box-content d-flex justify-content-center align-items-center p-0">
                                <div class="" style="margin-left: -15px;">
                                    <div class="manager-wrapper"
                                         style="display: block;overflow: hidden;width: 110px;margin-left: 50px; margin-top: 0px; height: 40px;">
                                        <img class="corporation-badge"
                                             src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>">
                                    </div>
                                </div>
                                <div class="mr-auto">

                                </div>
                                <div class="pr-3 d-flex">
                                    <div class="mr-1"><a class="button blue pr-1"
                                                         href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">Ver <i class="far fa-arrow-alt-circle-right"></i></a></div>
                                </div>
                            </div>
                        </div>
                    </div>
                <?php endwhile;  ?>

                </div>
                <?php else: ?>
                    <div class="content-box">
                        <div class="title">Oops!</div>
                        <div class="box-content">
                            <center><b>Hasta el momento no hay ninguna banda creada...</b></center>
                        </div>
                    </div>
                <?php endif; ?>

            </div>
        </div>


    </div>
</div>


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


        <div class="leaderboard-grid">


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
                    <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Asesinatos</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_kills");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_kills']); ?> asesinatos</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Muertes</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_deaths");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_deaths']); ?> muertes</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Polic&iacute;as</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_cop_kills");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_cop_kills']); ?> polic&iacute;as asesinados</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <!-- Second line -->


            <div class="content-box blue leaderboards mb-0">
                <div class="title">Ataques Ganados</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_turfs_taken");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_turfs_taken']); ?> ataques</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">Ataques Defendidos</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_turfs_defend");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_turfs_defend']); ?> defensas</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">Coca&iacute;na</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_farm_cocaine");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_farm_cocaine']); ?> gramos</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">Marihuana</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_farm_weed");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_farm_weed']); ?> gramos</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">Medicamentos</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_farm_medicines");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_farm_medicines']); ?> medicamentos</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">Armas Fabricadas</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_fab_guns");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_fab_guns']); ?> armas</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Atracos</div>
                <div class="box-content">
                    <?php $G = $GangMG->GetLeaderBoard("groups", "gang_heists");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['gang_heists']); ?> atracos</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>

            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Territorios</div>
                <div class="box-content">
                    <?php $G = $DB->Query("SELECT COUNT(*) AS CantTurfs, groups.*, rooms.* FROM groups, rooms WHERE groups.id = rooms.group_id AND groups.type = 3 GROUP BY groups.id ORDER BY CantTurfs DESC");
                    if(mysqli_num_rows($G) >= 1):
                        while ($Gang = mysqli_fetch_assoc($G)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang['name']); ?></div>
                                    <div class="leaderboard-user-stat"><?php echo number_format($Gang['CantTurfs']); ?> territorios</div>
                                </div>
                            </a>
                        <?php endwhile; else: echo "No hay bandas en esta categor&iacute;a"; endif; ?>

                </div>
            </div>




        </div>


    </div>
</div>

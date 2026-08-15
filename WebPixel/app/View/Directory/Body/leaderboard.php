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
            <!-- Richest Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">Usuarios m&aacute;s Ricos</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("richest_user");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                    <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                        <div class="leaderboard-pixel">
                            <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                        </div>
                        <div class="leaderboard-user-info mr-auto">
                            <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                            <div class="leaderboard-user-stat">

                                $<?php echo number_format($Row['bank'] + $Row['credits']); ?>

                            </div>
                        </div>
                    </a>
                    <?php endwhile; ?>


                </div>
            </div>

            <!-- Earned the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s dinero ganado</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "money_earned");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    $<?php echo number_format($Row['money_earned']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>


                </div>
            </div>

            <!-- Richest (PL) Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Platinos</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("richest_pl_user");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                    <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                        <div class="leaderboard-pixel">
                            <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                        </div>
                        <div class="leaderboard-user-info mr-auto">
                            <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                            <div class="leaderboard-user-stat">

                                <?php echo number_format($Row['vip_points']); ?> PL

                            </div>
                        </div>
                    </a>
                    <?php endwhile; ?>


                </div>
            </div>

            <!-- Earned PL the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s platinos ganados</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "pl_earned");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['pl_earned']); ?> PL

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>


                </div>
            </div>

            <!-- Fist Hits the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s golpes dados (Puñetazos)</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "punches");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['punches']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Killed the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">m&aacute;s Asesinatos cometidos</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "kills");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['kills']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Died the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s ocasiones muert@</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "deaths");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['deaths']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Hit Kills the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Asesinatos (Puños)</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "hitkills");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['hitkills']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Gun Kills the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Asesinatos (Armas)</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "gunkills");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['gunkills']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Cops Killed the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Policías Asesinados</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "copkills");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['copkills']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Cops Died the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Muertes por Policías</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "copdeaths");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['copdeaths']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Arrested the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s ocasiones arrestad@</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "arrested");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['arrested']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Arrested the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s arrestos como Policía</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "arrests");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['arrests']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Arrested the most Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">m&aacute;s Evasiones a la Policía</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "evasions");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['evasions']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Got the most Cocacine Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Cocaína Conseguida</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "cocaine_taken");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['cocaine_taken']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Got the most Weed Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Marihuana Conseguida</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "weed_taken");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['weed_taken']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Got the most Weed Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Medicamentos Conseguidos</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "medicines_taken");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['medicines_taken']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Got the most Drug consumed Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Droga consumida (Todas)</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "drugs_taken");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['drugs_taken']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Most Weapons created Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s Arm&aacute;s Fabricadas (Armeros)</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "guns_fab");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['guns_fab']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

            <!-- Most Shifts completed Users -->
            <div class="content-box blue leaderboards mb-0">
                <div class="title">M&aacute;s turnos realizados (Empresas)</div>
                <div class="box-content">
                    <?php
                    $R_ = $UserMG->GetLeaderBoard("play_stats", "total_shifts");
                    while($Row = mysqli_fetch_assoc($R_)): ?>
                        <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="leaderboard-pixel">
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                            </div>
                            <div class="leaderboard-user-info mr-auto">
                                <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                <div class="leaderboard-user-stat">

                                    <?php echo number_format($Row['total_shifts']); ?>

                                </div>
                            </div>
                        </a>
                    <?php endwhile; ?>
                </div>
            </div>

        </div>

    </div>
</div>


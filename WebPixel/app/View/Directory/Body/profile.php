<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

if(isset($_GET['id']) && is_numeric($_GET['id']) && $_GET['id'] != ""):
    $ID = $_GET['id'];
    $User_ = $UserMG->GetUserDataByID($ID);
    $Gang_ = $GangMG->GetUserGang($User_['id']);
    $PBusiness = $BusinessMG->GetUserPrimaryBusiness($User_['id']);
    $VIP = $UserMG->isVIP($User_['id']);
    $SBusiness = $BusinessMG->GetUserSecondaryBusiness($User_['id']);

else:
    $User_ = $UserMG->GetUserDataByID($UData['id']);
    $Gang_ = $GangMG->GetUserGang($UData['id']);
    $PBusiness = $BusinessMG->GetUserPrimaryBusiness($UData['id']);
    $VIP = $UserMG->isVIP($UData['id']);
    $SBusiness = $BusinessMG->GetUserSecondaryBusiness($UData['id']);

endif;
?>
<div class="content">

    <div class="container">
    <?php if($User_ != null): ?>

        <div class="row">
            <div class="col-4">
                <div class="content-box mb-4">
                    <div class="title">
                        <i class="fas fa-user text-primary"></i> <?php echo $User_['username']; ?>
                    </div>
                    <div class="box-content p-2">
                        <div class="d-flex justify-content-center align-items-center text-center pb-3 profile-box">
                            <div class=""><img class="profile-pixel" src="https://nitro-imager.kubbo.ch/?figure=<?php echo $User_['look']; ?>&amp;direction=2&amp;gesture=sml&amp;size=l"></div>
                            <div class="pr-4"><img src="<?php echo DY; ?>/img/extras/bio.gif"> <b>Rango</b> <br> <?php if($User_['rank'] == 6): echo "Desarrollador / Dueño"; elseif($User_['rank'] == 5): echo "Administrador(a)"; elseif($User_['rank'] == 4): echo "Moderador(a)"; elseif($User_['rank'] == 3): echo "Ayudante"; endif; ?></div>
                        </div>
                        <div class="d-flex justify-content-center align-items-center text-center">
                            <div class="flex-fill p-2 mr-1 profile-level-box"><b><?php echo $User_['level']; ?></b><br>Nivel</div>
                            <div class="flex-fill p-2 ml-2 profile-level-box"><b><?php echo $User_['curxp']; ?> / <?php echo $User_['needxp']; ?></b><br>Reputaci&oacute;n (XP)</div>
                        </div>
                        <hr>
                        <div><span class="float-right font-weight-bold"><?php echo date('d/m/Y', $User_['account_created']); ?></span><img src="<?php echo DY; ?>/img/extras/account_created.gif"> Registro</div>
                        <div>
                            <img src="<?php echo DY; ?>/img/extras/logout.gif"> <span class="text-success font-weight-bold">&Uacute;ltima conexi&oacute;n</span>
                            <span class="float-right font-weight-bold"><?php echo AppFunctions::GetTime($User_['last_online']); ?></span>
                        </div>
                        <div><span class="float-right font-weight-bold">$<?php echo number_format($User_['credits'] + $User_['bank']); ?></span><img src="<?php echo DY; ?>/img/extras/credit.png"> Valor de la cuenta</div>
                        <hr>
                    </div>
                </div>
                <?php if($Gang_ != null): ?>
                    <div class="content-box mb-4">
                        <div class="title">
                            <i class="fas fa-adjust text-success"></i> Banda
                        </div>
                        <div class="box-content">
                            <a class="leaderboard-gang no-link-styling justify-content-center align-items-center p-1" href="<?php echo Config::$URL; ?>/gang/<?php echo $Gang_['id']; ?>">
                                <div class="gang-colours p-2">
                                    <img src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang_['badge']; ?>" width="35" height="35">
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo utf8_encode($Gang_['name']); ?></div>
                                    <div class="leaderboard-user-stat"><b>K/D</b>: <?php $C_ = ($Gang_['gang_deaths'] == 0)? $Gang_['gang_kills'] : ($Gang_['gang_kills'] / $Gang_['gang_deaths']); echo number_format($C_, 2); ?></div>
                                </div>
                            </a>
                            <ul class="list-group list-group-flush">
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    L&iacute;der
                                    <span class="badge badge-secondary badge-pill peak"><?php echo $GangMG->GetGangLeader($Gang_['id']); ?></span>
                                </li>
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Asesinatos
                                    <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang_['gang_kills']); ?></span>
                                </li>
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Muertes
                                    <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang_['gang_deaths']); ?></span>
                                </li>
                            </ul>
                        </div>
                    </div>
                <?php endif; ?>
            </div>
            <div class="col-4">
                <div class="content-box mb-4">
                    <div class="title">
                        <i class="fas fa-star text-warning"></i> Estad&iacute;sticas Criminales
                    </div>
                    <div class="box-content p-2">
                        <ul class="list-group list-group-flush">

                            <li class="list-group-item d-flex justify-content-between align-items-center">
                               Asesinatos
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['kills']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Muertes
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['deaths']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                K/D
                                <span class="badge badge-secondary badge-pill peak"><?php $C_ = ($User_['deaths'] == 0)? $User_['kills'] : ($User_['kills'] / $User_['deaths']); echo number_format($C_, 2); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Golpes
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['punches']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Asesinatos a Golpes
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['hitkills']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Asesinatos con Armas
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['gunkills']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Polic&iacute;as Asesinados
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['copkills']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Muerto por Pol&iacute;cias
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['copdeaths']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Arrestos (como Polic&iacute;a)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['arrests']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Arrestad@
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['arrested']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Evasiones policiacas
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['evasions']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Coca&iacute;na conseguida
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['cocaine_taken']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Medicamentos conseguidos
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['medicines_taken']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Marihuana conseguida
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['weed_taken']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Drogas consumidas
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['drugs_taken']); ?></span>
                            </li>
                        </ul>
                    </div>
                </div>
                <div class="content-box mb-4">
                    <div class="title">
                        <i class="fas fa-money-bill-wave text-warning"></i> Estad&iacute;sticas Econ&oacute;micas
                    </div>
                    <div class="box-content p-2">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Dinero ganado
                                <span class="badge badge-secondary badge-pill peak">$<?php echo number_format($User_['money_earned']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Platinos ganados
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['pl_earned']); ?> PL</span>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
            <div class="col-4">
                <div class="content-box mb-4">
                    <div class="title">
                        <i class="fas fa-user-md text-info"></i> Estad&iacute;sticas de Trabajo (Primario)                   </div>
                    <div class="box-content p-2">
                        <?php if($PBusiness == null): ?>
                        <div class="profile-corp d-flex align-items-center justify-content-center p-2">
                            <div class="corp-badge">
                                <img src="<?php echo Config::$SWF;?>/habbo-imaging/corp/poor.gif">
                            </div>
                            <div class="corp text-center ml-auto pr-2">Desempleado</div>
                        </div>
                        <?php else: ?>
                        <a href="<?php echo Config::$URL;?>/corporation/<?php echo $PBusiness['id']; ?>" target="" style="color:white;">
                            <div class="profile-corp d-flex align-items-center justify-content-center p-2">
                            <div class="corp-badge">
                                <img src="<?php echo Config::$SWF;?>/habbo-imaging/corp/<?php echo $PBusiness['badge']; ?>.gif">
                            </div>
                            <div class="corp text-center ml-auto pr-2"><?php echo utf8_encode($PBusiness['name']); ?> (<?php echo utf8_encode($PBusiness['RankName']); ?>)</div>
                            </div>
                        </a>
                        <?php endif; ?>
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Minutos trabajando
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['time_worked']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Turnos completados (totales)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($User_['total_shifts']); ?></span>
                            </li>
                        </ul>
                    </div>
                </div>

                <div class="content-box mb-4">
                    <div class="title">
                        <i class="fas fa-user-md text-info"></i> Estad&iacute;sticas de Trabajo (Secundario)                  </div>
                    <div class="box-content p-2">
                        <?php if($SBusiness == null): ?>
                            <div class="profile-corp d-flex align-items-center justify-content-center p-2">
                                <div class="corp-badge">
                                    <img src="<?php echo Config::$SWF;?>/habbo-imaging/corp/poor.gif">
                                </div>
                                <div class="corp text-center ml-auto pr-2">Desempleado</div>
                            </div>
                        <?php else: ?>
                        <a href="<?php echo Config::$URL;?>/corporation/<?php echo $SBusiness['id']; ?>" target="" style="color:white;">
                            <div class="profile-corp d-flex align-items-center justify-content-center p-2">
                                <div class="corp-badge">
                                    <img src="<?php echo Config::$SWF;?>/habbo-imaging/corp/<?php echo $SBusiness['badge']; ?>.gif">
                                </div>
                                <div class="corp text-center ml-auto pr-2"><?php echo utf8_encode($SBusiness['name']); ?></div>
                            </div>
                        </a>
                        <?php if($UserMG->GetUserSecondJobLvl($User_['id'], $SBusiness['id']) > 0): ?>
                            <ul class="list-group list-group-flush">
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Nivel
                                    <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], $SBusiness['id'])); ?></span>
                                </li>
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    Habilidad
                                    <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], $SBusiness['id'])); ?> / 50</span>
                                </li>
                            </ul>
                        <?php endif; ?>
                        <?php endif; ?>
                    </div>
                </div>

                <div class="content-box mb-4">
                    <div class="title">
                        <i class="fa fa-trophy text-info"></i> Habilidades
                    </div>
                    <div class="box-content p-2">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Basurero (Nivel <?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], 2)); ?>)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], 2)); ?> / 50</span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Mec&aacute;nico (Nivel <?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], 3)); ?>)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], 3)); ?> / 50</span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Fabricante de Armas (Nivel <?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], 4)); ?>)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], 4)); ?> / 50</span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Camionero (Nivel <?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], 6)); ?>)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], 6)); ?> / 50</span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Minero (Nivel <?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], 8)); ?>)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], 8)); ?> / 50</span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Ladr&oacute;n (Nivel <?php echo number_format($UserMG->GetUserSecondJobLvl($User_['id'], 0)); ?>)
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($UserMG->GetUserSecondJobXP($User_['id'], 0)); ?> / 50</span>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    <?php else:   endif; ?>

    </div>
</div>


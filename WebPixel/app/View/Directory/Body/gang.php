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

<style type="text/css">
    /* MODAL
    –––––––––––––––––––––––––––––––––––––––––––––––––– */
    .modal {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 1rem;
      background: #00000075;
      cursor: pointer;
      visibility: hidden;
      opacity: 0;
      transition: all 0.35s ease-in;
    }

    .modal.is-visible {
      visibility: visible;
      opacity: 1;
    }

    .modal-dialog {
      position: relative;
      max-width: 800px;
      max-height: 100vh;
      border-radius: 5px;
      background: #069;
      overflow: auto;
      cursor: default;
    }

    .modal-dialog > * {
      padding: 1rem;
    }

    .modal-header,
    .modal-footer {
        border-radius: 5px 5px 0 0;
        text-transform: uppercase;
        font-weight: bold;
        font-size: 115%;
        color: #ffffff;
        text-shadow: 1px 1px 1px #000;
        background-color: #092a3f;
        padding: 8px 14px;
        letter-spacing: 0.2px;
        text-align: center;
    }

    .modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .modal-header .close-modal {
      font-size: 1.5rem;
      background: transparent;
      border: 0;
      color: white;
    }

    .modal p + p {
      margin-top: 1rem;
    }

    .modal-content {
        color: white;
    }

    /* ANIMATIONS
    –––––––––––––––––––––––––––––––––––––––––––––––––– */
    [data-animation] .modal-dialog {
      opacity: 0;
      transition: all 0.5s var(--bounceEasing);
    }

    [data-animation].is-visible .modal-dialog {
      opacity: 1;
      transition-delay: 0.2s;
    }

    [data-animation="slideInOutDown"] .modal-dialog {
      transform: translateY(100%);
    }

    [data-animation="slideInOutTop"] .modal-dialog {
      transform: translateY(-100%);
    }

    [data-animation="slideInOutLeft"] .modal-dialog {
      transform: translateX(-100%);
    }

    [data-animation="slideInOutRight"] .modal-dialog {
      transform: translateX(100%);
    }

    [data-animation="zoomInOut"] .modal-dialog {
      transform: scale(0.2);
    }

    [data-animation="rotateInOutDown"] .modal-dialog {
      transform-origin: top left;
      transform: rotate(-1turn);
    }

    [data-animation="mixInAnimations"].is-visible .modal-dialog {
      animation: mixInAnimations 2s 0.2s linear forwards;
    }

    [data-animation="slideInOutDown"].is-visible .modal-dialog,
    [data-animation="slideInOutTop"].is-visible .modal-dialog,
    [data-animation="slideInOutLeft"].is-visible .modal-dialog,
    [data-animation="slideInOutRight"].is-visible .modal-dialog,
    [data-animation="zoomInOut"].is-visible .modal-dialog,
    [data-animation="rotateInOutDown"].is-visible .modal-dialog {
      transform: none;
    }

    @keyframes mixInAnimations {
      0% {
        transform: translateX(-100%);
      }

      10% {
        transform: translateX(0);
      }

      20% {
        transform: rotate(20deg);
      }

      30% {
        transform: rotate(-20deg);
      }

      40% {
        transform: rotate(15deg);
      }

      50% {
        transform: rotate(-15deg);
      }

      60% {
        transform: rotate(10deg);
      }

      70% {
        transform: rotate(-10deg);
      }

      80% {
        transform: rotate(5deg);
      }

      90% {
        transform: rotate(-5deg);
      }

      100% {
        transform: rotate(0deg);
      }
    }
</style>

<style type="text/css">
    div#cont_placagrupo {
        width: 632px;
    }

    div#cont_placagrupo div.cont_btx {
        height: auto !important;
        width: 308px !important;
    }

    div#cont_placagrupo div#pgrupo {
        width: auto !important;
        height: auto !important;
    }

    div#cont_placagrupo .imgprev {
        width: 42px !important;
        height: 42px !important;
    }
</style>

<script type="text/javascript" src="http://code.jquery.com/jquery-1.9.1.js"></script>
<script type="text/javascript" src="<?php echo Config::$SWF; ?>/habbogrupos/js/general.js?p=<?php echo time(); ?>"></script>
<link rel="stylesheet" href="<?php echo Config::$SWF; ?>/habbogrupos/css/style.css?p=<?php echo time(); ?>"/>

<div class="content">

    <div class="container">
        <div class="container d-flex">
            <!-- Side -->
            <div class="col-3">
                <div class="content-box">
                    <div class="title"><?php echo utf8_encode($Gang->Data['name']); ?></div>
                    <div class="box-content text-center">
                        <?php if ($Gang->Data['owner_id'] == $UData['id']): ?>
                            <button type="button" class="open-modal button green" data-open="modal1" style="font-size: 13px;padding: 3px;position: absolute;left: 58%;" title="Editar emblema">
                                    <i class="fas fa-edit"></i>
                            </button>
                        <?php endif; ?>
                        <img class="corporation-badge profile-level-box" style="padding: 15px;"
                             src="https://swf.habbovip.us/group-badge/badge/<?php echo $Gang->Data['badge']; ?>">
                    </div>
                </div>
                <div class="content-box ">
                    <div class="title">INFORMACI&Oacute;N</div>
                    <div class="box-content text-center">

                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Creador
                                <span class="badge badge-secondary badge-pill peak"><?php echo $Gang->GangOwner['username']; ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Fundada el
                                <span class="badge badge-secondary badge-pill peak"><?php echo date('d/m/Y', $Gang->Data['created']); ?></span>
                            </li>

                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Valor
                                <span class="badge badge-secondary badge-pill peak">$<?php echo number_format($Gang->Data['bank']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Miembros
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->MemberCount); ?></span>
                            </li>


                        </ul>

                    </div>
                </div>

                <div class="content-box ">
                    <div class="title">Combate</div>
                    <div class="box-content text-center">

                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Asesinatos
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_kills']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Muertes
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_deaths']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Polic&iacute;as Asesinados
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_cop_kills']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                K/D
                                <span class="badge badge-secondary badge-pill peak"><?php $C_ = ($Gang->Data['gang_deaths'] == 0) ? $Gang->Data['gang_kills'] : ($Gang->Data['gang_kills'] / $Gang->Data['gang_deaths']);
                                    echo number_format($C_, 2); ?></span>
                            </li>

                        </ul>


                    </div>
                </div>

                <div class="content-box ">
                    <div class="title">Territorios</div>
                    <div class="box-content text-center">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Controlados
                                <span class="badge badge-secondary badge-pill peak"><?php echo $Gang->TurfCount; ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Capturados
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_turfs_taken']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Defendidos
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_turfs_defend']); ?></span>
                            </li>

                        </ul>
                    </div>
                </div>

                <div class="content-box ">
                    <div class="title">Records</div>
                    <div class="box-content text-center">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Coca&iacute;na
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_farm_cocaine']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Marihuana
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_farm_weed']); ?></span>
                            </li>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                Armas
                                <span class="badge badge-secondary badge-pill peak"><?php echo number_format($Gang->Data['gang_fab_guns']); ?></span>
                            </li>

                        </ul>
                    </div>
                </div>


            </div>

            <!-- Center -->
            <div class="col-9">
                <!-- Ranks Start -->

                <?php
                $GR = $DB->Query("SELECT * FROM play_jobs_ranks WHERE job = " . $Gang->GangID . " ORDER BY rank DESC");
                while ($GRank = mysqli_fetch_assoc($GR)): ?>
                    <div class="content-box">
                        <div class="title"><?php echo utf8_encode($GRank['name']); ?></div>
                        <div class="box-content">
                            <?php $GU = $DB->Query("SELECT * FROM group_memberships WHERE rank = " . $GRank['rank'] . " AND group_id = " . $GRank['job'] . " ");
                            if (mysqli_num_rows($GU) >= 1):
                            ?>
                            <div class="gang-member-grid">
                                <?php
                                while ($GUsers = mysqli_fetch_assoc($GU)):
                                    $RankUserQ = $DB->Query("SELECT username, look, online, id FROM users WHERE id = ". $GUsers['user_id'] . " LIMIT 1");
                                    $RankUser = mysqli_fetch_assoc($RankUserQ);
                                    ?>
                                    <a href="<?php echo Config::$URL; ?>/profile/<?php echo $RankUser['id']; ?>"
                                       class="gang-member d-flex justify-content-center align-items-center no-link-styling">
                                        <div class="gang-member-pixel <?php echo ($RankUser['online'] == '1')? "" : "grayscale" ; ?>">
                                            <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $RankUser['look']; ?>&amp;head_direction=3&amp;gesture=sml">
                                        </div>
                                        <div>
                                            <span class="font-weight-bold"><?php echo $RankUser['username']; ?></span>
                                            <div class="gang-member-stats" style="font-size: 11px;">
                                            </div>
                                        </div>
                                        <div class="d-flex align-items-center justify-content-center flex-fill">
                                            <img src="<?php echo DY; ?>/img/icons/<?php echo ($RankUser['online'] == '1')? "online" : "offline" ; ?>.gif">
                                        </div>
                                    </a>

                                <?php endwhile; ?>
                            </div>
                            <?php else: echo "No hay usuarios en este rango"; endif; ?>
                        </div>
                    </div>
                <?php endwhile; ?>

            </div>
        </div>

        <?php if ($Gang->Data['owner_id'] == $UData['id']): ?>
            <div class="modal" id="modal1" data-animation="slideInOutLeft">
              <div class="modal-dialog">
                <header class="modal-header">
                  Edita el emblema de tu Banda
                  <button class="close-modal" aria-label="close modal" data-close>
                    ✕  
                  </button>
                </header>
                <section class="modal-content" style="height: 88vh;max-height: 88vh; overflow:auto;">

                    <?php if ($UData['online'] == 1): ?>
                        <p><strong style="color:red;text-shadow: -1px 0 black, 0 1px black, 1px 0 black, 0 -1px black;">Debes estar offline para editar el emblema de tu Banda</strong></p>
                    <?php else: ?>

                        <p><center><strong id="ErrorMsg" style="color:red;text-shadow: -1px 0 black, 0 1px black, 1px 0 black, 0 -1px black;"></strong></center></p>

                        <div class="flex">
                            <div id="cont_placagrupo">
                              <div class="cont_btx setcolors">
                                <div id="contg" style="display: inline-flex;text-align: center;">
                                  <div>
                                    <center>
                                      <p style="margin-bottom: 0;">Color primario</p>
                                      <div id="ColorPrim" class="boxcolor2" style="background-Color:#FFD500;float:none;"></div>
                                    </center>
                                  </div>
                                  <div style="margin-left: 48px;">
                                    <center>
                                      <p style="margin-bottom: 0;">Color secundario</p>
                                      <div id="ColorSec" class="boxcolor2" style="background-Color:#FFD500;float:none;"></div>
                                    </center>
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx setcolors">
                                <div id="contg">
                                  <div class="prevplaca_box">
                                    Vista previa
                                    <!--<img id="placafinal">-->
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx colors">
                                <div id="contg">
                                  <div id="contgrupocolores" style="margin-right: 23px;">
                                    <div id="primcolor-01" class="boxcolor" onClick="colorPrim('01')" style="background-Color:#FFD500;"></div>
                                    <div id="primcolor-02" class="boxcolor" onClick="colorPrim('02')" style="background-Color:#EB7500;"></div>
                                    <div id="primcolor-03" class="boxcolor" onClick="colorPrim('03')" style="background-Color:#83DD00;"></div>
                                    <div id="primcolor-04" class="boxcolor" onClick="colorPrim('04')" style="background-Color:#579900;"></div>
                                    <div id="primcolor-05" class="boxcolor" onClick="colorPrim('05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="primcolor-06" class="boxcolor" onClick="colorPrim('06')" style="background-Color:#006ECE;"></div>
                                    <div id="primcolor-07" class="boxcolor" onClick="colorPrim('07')" style="background-Color:#FF97E2;"></div>
                                    <div id="primcolor-08" class="boxcolor" onClick="colorPrim('08')" style="background-Color:#F233BE;"></div>
                                    <div id="primcolor-09" class="boxcolor" onClick="colorPrim('09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="primcolor-10" class="boxcolor" onClick="colorPrim('10')" style="background-Color:#AE0909;"></div>
                                    <div id="primcolor-11" class="boxcolor" onClick="colorPrim('11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="primcolor-12" class="boxcolor" onClick="colorPrim('12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="primcolor-13" class="boxcolor" onClick="colorPrim('13')" style="background-Color:#363636;"></div>
                                    <div id="primcolor-14" class="boxcolor" onClick="colorPrim('14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="primcolor-15" class="boxcolor" onClick="colorPrim('15')" style="background-Color:#967540;"></div>
                                    <div id="primcolor-16" class="boxcolor" onClick="colorPrim('16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="primcolor-17" class="boxcolor" onClick="colorPrim('17')" style="background-Color:#FFF064;"></div>
                                    <div id="primcolor-18" class="boxcolor" onClick="colorPrim('18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                  <div id="contgrupocolores">
                                    <div id="seccolor-01" class="boxcolor" onClick="colorSec('01')" style="background-Color:#FFD500;"></div>
                                    <div id="seccolor-02" class="boxcolor" onClick="colorSec('02')" style="background-Color:#EB7500;"></div>
                                    <div id="seccolor-03" class="boxcolor" onClick="colorSec('03')" style="background-Color:#83DD00;"></div>
                                    <div id="seccolor-04" class="boxcolor" onClick="colorSec('04')" style="background-Color:#579900;"></div>
                                    <div id="seccolor-05" class="boxcolor" onClick="colorSec('05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="seccolor-06" class="boxcolor" onClick="colorSec('06')" style="background-Color:#006ECE;"></div>
                                    <div id="seccolor-07" class="boxcolor" onClick="colorSec('07')" style="background-Color:#FF97E2;"></div>
                                    <div id="seccolor-08" class="boxcolor" onClick="colorSec('08')" style="background-Color:#F233BE;"></div>
                                    <div id="seccolor-09" class="boxcolor" onClick="colorSec('09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="seccolor-10" class="boxcolor" onClick="colorSec('10')" style="background-Color:#AE0909;"></div>
                                    <div id="seccolor-11" class="boxcolor" onClick="colorSec('11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="seccolor-12" class="boxcolor" onClick="colorSec('12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="seccolor-13" class="boxcolor" onClick="colorSec('13')" style="background-Color:#363636;"></div>
                                    <div id="seccolor-14" class="boxcolor" onClick="colorSec('14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="seccolor-15" class="boxcolor" onClick="colorSec('15')" style="background-Color:#967540;"></div>
                                    <div id="seccolor-16" class="boxcolor" onClick="colorSec('16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="seccolor-17" class="boxcolor" onClick="colorSec('17')" style="background-Color:#FFF064;"></div>
                                    <div id="seccolor-18" class="boxcolor" onClick="colorSec('18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx base">
                                <div id="contg">
                                  <div id="pgrupo">
                                    <div class="flecha izq" onClick="cambiarBase(0)"></div>
                                    <div class="flecha der" onClick="cambiarBase(1)"></div>
                                    <div id="baseimg" class="imgprev"></div>
                                  </div>
                                  <div id="contgrupocolores">
                                    <div id="bascolor-01" class="boxcolor" onClick="colorBase('01')" style="background-Color:#FFD500;"></div>
                                    <div id="bascolor-02" class="boxcolor" onClick="colorBase('02')" style="background-Color:#EB7500;"></div>
                                    <div id="bascolor-03" class="boxcolor" onClick="colorBase('03')" style="background-Color:#83DD00;"></div>
                                    <div id="bascolor-04" class="boxcolor" onClick="colorBase('04')" style="background-Color:#579900;"></div>
                                    <div id="bascolor-05" class="boxcolor" onClick="colorBase('05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="bascolor-06" class="boxcolor" onClick="colorBase('06')" style="background-Color:#006ECE;"></div>
                                    <div id="bascolor-07" class="boxcolor" onClick="colorBase('07')" style="background-Color:#FF97E2;"></div>
                                    <div id="bascolor-08" class="boxcolor" onClick="colorBase('08')" style="background-Color:#F233BE;"></div>
                                    <div id="bascolor-09" class="boxcolor" onClick="colorBase('09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="bascolor-10" class="boxcolor" onClick="colorBase('10')" style="background-Color:#AE0909;"></div>
                                    <div id="bascolor-11" class="boxcolor" onClick="colorBase('11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="bascolor-12" class="boxcolor" onClick="colorBase('12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="bascolor-13" class="boxcolor" onClick="colorBase('13')" style="background-Color:#363636;"></div>
                                    <div id="bascolor-14" class="boxcolor" onClick="colorBase('14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="bascolor-15" class="boxcolor" onClick="colorBase('15')" style="background-Color:#967540;"></div>
                                    <div id="bascolor-16" class="boxcolor" onClick="colorBase('16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="bascolor-17" class="boxcolor" onClick="colorBase('17')" style="background-Color:#FFF064;"></div>
                                    <div id="bascolor-18" class="boxcolor" onClick="colorBase('18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx comp">
                                <div id="contg">
                                  <div id="pgrupo">
                                    <div class="flecha izq" onClick="cambiarObjeto(0,0)"></div>
                                    <div class="flecha der" onClick="cambiarObjeto(1,0)"></div>
                                    <div id="accimg-0" class="imgprev obj" obj="0"></div>
                                  </div>
                                  <div id="contgrupopos">
                                    <div id="pos-0-0" class="boxpos deselect" onClick="cambiarPos(0, 0)"></div>
                                    <div id="pos-0-1" class="boxpos deselect" onClick="cambiarPos(0, 1)"></div>
                                    <div id="pos-0-2" class="boxpos deselect" onClick="cambiarPos(0, 2)"></div>
                                    <div id="pos-0-3" class="boxpos deselect" onClick="cambiarPos(0, 3)"></div>
                                    <div id="pos-0-4" class="boxpos deselect" onClick="cambiarPos(0, 4)"></div>
                                    <div id="pos-0-5" class="boxpos deselect" onClick="cambiarPos(0, 5)"></div>
                                    <div id="pos-0-6" class="boxpos deselect" onClick="cambiarPos(0, 6)"></div>
                                    <div id="pos-0-7" class="boxpos deselect" onClick="cambiarPos(0, 7)"></div>
                                    <div id="pos-0-8" class="boxpos deselect" onClick="cambiarPos(0, 8)"></div>
                                  </div>
                                  <div id="contgrupocolores">
                                    <div id="bcolor-0-01" class="boxcolor" onClick="cambiarColor(0, '01')" style="background-Color:#FFD500;"></div>
                                    <div id="bcolor-0-02" class="boxcolor" onClick="cambiarColor(0, '02')" style="background-Color:#EB7500;"></div>
                                    <div id="bcolor-0-03" class="boxcolor" onClick="cambiarColor(0, '03')" style="background-Color:#83DD00;"></div>
                                    <div id="bcolor-0-04" class="boxcolor" onClick="cambiarColor(0, '04')" style="background-Color:#579900;"></div>
                                    <div id="bcolor-0-05" class="boxcolor" onClick="cambiarColor(0, '05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="bcolor-0-06" class="boxcolor" onClick="cambiarColor(0, '06')" style="background-Color:#006ECE;"></div>
                                    <div id="bcolor-0-07" class="boxcolor" onClick="cambiarColor(0, '07')" style="background-Color:#FF97E2;"></div>
                                    <div id="bcolor-0-08" class="boxcolor" onClick="cambiarColor(0, '08')" style="background-Color:#F233BE;"></div>
                                    <div id="bcolor-0-09" class="boxcolor" onClick="cambiarColor(0, '09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="bcolor-0-10" class="boxcolor" onClick="cambiarColor(0, '10')" style="background-Color:#AE0909;"></div>
                                    <div id="bcolor-0-11" class="boxcolor" onClick="cambiarColor(0, '11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="bcolor-0-12" class="boxcolor" onClick="cambiarColor(0, '12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="bcolor-0-13" class="boxcolor" onClick="cambiarColor(0, '13')" style="background-Color:#363636;"></div>
                                    <div id="bcolor-0-14" class="boxcolor" onClick="cambiarColor(0, '14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="bcolor-0-15" class="boxcolor" onClick="cambiarColor(0, '15')" style="background-Color:#967540;"></div>
                                    <div id="bcolor-0-16" class="boxcolor" onClick="cambiarColor(0, '16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="bcolor-0-17" class="boxcolor" onClick="cambiarColor(0, '17')" style="background-Color:#FFF064;"></div>
                                    <div id="bcolor-0-18" class="boxcolor" onClick="cambiarColor(0, '18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx comp">
                                <div id="contg">
                                  <div id="pgrupo">
                                    <div class="flecha izq" onClick="cambiarObjeto(0,1)"></div>
                                    <div class="flecha der" onClick="cambiarObjeto(1,1)"></div>
                                    <div id="accimg-1" class="imgprev obj" obj="1"></div>
                                  </div>
                                  <div id="contgrupopos">
                                    <div id="pos-1-0" class="boxpos deselect" onClick="cambiarPos(1, 0)"></div>
                                    <div id="pos-1-1" class="boxpos deselect" onClick="cambiarPos(1, 1)"></div>
                                    <div id="pos-1-2" class="boxpos deselect" onClick="cambiarPos(1, 2)"></div>
                                    <div id="pos-1-3" class="boxpos deselect" onClick="cambiarPos(1, 3)"></div>
                                    <div id="pos-1-4" class="boxpos deselect" onClick="cambiarPos(1, 4)"></div>
                                    <div id="pos-1-5" class="boxpos deselect" onClick="cambiarPos(1, 5)"></div>
                                    <div id="pos-1-6" class="boxpos deselect" onClick="cambiarPos(1, 6)"></div>
                                    <div id="pos-1-7" class="boxpos deselect" onClick="cambiarPos(1, 7)"></div>
                                    <div id="pos-1-8" class="boxpos deselect" onClick="cambiarPos(1, 8)"></div>
                                  </div>
                                  <div id="contgrupocolores">
                                    <div id="bcolor-1-01" class="boxcolor" onClick="cambiarColor(1, '01')" style="background-Color:#FFD500;"></div>
                                    <div id="bcolor-1-02" class="boxcolor" onClick="cambiarColor(1, '02')" style="background-Color:#EB7500;"></div>
                                    <div id="bcolor-1-03" class="boxcolor" onClick="cambiarColor(1, '03')" style="background-Color:#83DD00;"></div>
                                    <div id="bcolor-1-04" class="boxcolor" onClick="cambiarColor(1, '04')" style="background-Color:#579900;"></div>
                                    <div id="bcolor-1-05" class="boxcolor" onClick="cambiarColor(1, '05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="bcolor-1-06" class="boxcolor" onClick="cambiarColor(1, '06')" style="background-Color:#006ECE;"></div>
                                    <div id="bcolor-1-07" class="boxcolor" onClick="cambiarColor(1, '07')" style="background-Color:#FF97E2;"></div>
                                    <div id="bcolor-1-08" class="boxcolor" onClick="cambiarColor(1, '08')" style="background-Color:#F233BE;"></div>
                                    <div id="bcolor-1-09" class="boxcolor" onClick="cambiarColor(1, '09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="bcolor-1-10" class="boxcolor" onClick="cambiarColor(1, '10')" style="background-Color:#AE0909;"></div>
                                    <div id="bcolor-1-11" class="boxcolor" onClick="cambiarColor(1, '11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="bcolor-1-12" class="boxcolor" onClick="cambiarColor(1, '12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="bcolor-1-13" class="boxcolor" onClick="cambiarColor(1, '13')" style="background-Color:#363636;"></div>
                                    <div id="bcolor-1-14" class="boxcolor" onClick="cambiarColor(1, '14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="bcolor-1-15" class="boxcolor" onClick="cambiarColor(1, '15')" style="background-Color:#967540;"></div>
                                    <div id="bcolor-1-16" class="boxcolor" onClick="cambiarColor(1, '16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="bcolor-1-17" class="boxcolor" onClick="cambiarColor(1, '17')" style="background-Color:#FFF064;"></div>
                                    <div id="bcolor-1-18" class="boxcolor" onClick="cambiarColor(1, '18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx comp">
                                <div id="contg">
                                  <div id="pgrupo">
                                    <div class="flecha izq" onClick="cambiarObjeto(0,2)"></div>
                                    <div class="flecha der" onClick="cambiarObjeto(1,2)"></div>
                                    <div id="accimg-2" class="imgprev obj" obj="2"></div>
                                  </div>
                                  <div id="contgrupopos">
                                    <div id="pos-2-0" class="boxpos deselect" onClick="cambiarPos(2, 0)"></div>
                                    <div id="pos-2-1" class="boxpos deselect" onClick="cambiarPos(2, 1)"></div>
                                    <div id="pos-2-2" class="boxpos deselect" onClick="cambiarPos(2, 2)"></div>
                                    <div id="pos-2-3" class="boxpos deselect" onClick="cambiarPos(2, 3)"></div>
                                    <div id="pos-2-4" class="boxpos deselect" onClick="cambiarPos(2, 4)"></div>
                                    <div id="pos-2-5" class="boxpos deselect" onClick="cambiarPos(2, 5)"></div>
                                    <div id="pos-2-6" class="boxpos deselect" onClick="cambiarPos(2, 6)"></div>
                                    <div id="pos-2-7" class="boxpos deselect" onClick="cambiarPos(2, 7)"></div>
                                    <div id="pos-2-8" class="boxpos deselect" onClick="cambiarPos(2, 8)"></div>
                                  </div>
                                  <div id="contgrupocolores">
                                    <div id="bcolor-2-01" class="boxcolor" onClick="cambiarColor(2, '01')" style="background-Color:#FFD500;"></div>
                                    <div id="bcolor-2-02" class="boxcolor" onClick="cambiarColor(2, '02')" style="background-Color:#EB7500;"></div>
                                    <div id="bcolor-2-03" class="boxcolor" onClick="cambiarColor(2, '03')" style="background-Color:#83DD00;"></div>
                                    <div id="bcolor-2-04" class="boxcolor" onClick="cambiarColor(2, '04')" style="background-Color:#579900;"></div>
                                    <div id="bcolor-2-05" class="boxcolor" onClick="cambiarColor(2, '05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="bcolor-2-06" class="boxcolor" onClick="cambiarColor(2, '06')" style="background-Color:#006ECE;"></div>
                                    <div id="bcolor-2-07" class="boxcolor" onClick="cambiarColor(2, '07')" style="background-Color:#FF97E2;"></div>
                                    <div id="bcolor-2-08" class="boxcolor" onClick="cambiarColor(2, '08')" style="background-Color:#F233BE;"></div>
                                    <div id="bcolor-2-09" class="boxcolor" onClick="cambiarColor(2, '09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="bcolor-2-10" class="boxcolor" onClick="cambiarColor(2, '10')" style="background-Color:#AE0909;"></div>
                                    <div id="bcolor-2-11" class="boxcolor" onClick="cambiarColor(2, '11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="bcolor-2-12" class="boxcolor" onClick="cambiarColor(2, '12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="bcolor-2-13" class="boxcolor" onClick="cambiarColor(2, '13')" style="background-Color:#363636;"></div>
                                    <div id="bcolor-2-14" class="boxcolor" onClick="cambiarColor(2, '14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="bcolor-2-15" class="boxcolor" onClick="cambiarColor(2, '15')" style="background-Color:#967540;"></div>
                                    <div id="bcolor-2-16" class="boxcolor" onClick="cambiarColor(2, '16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="bcolor-2-17" class="boxcolor" onClick="cambiarColor(2, '17')" style="background-Color:#FFF064;"></div>
                                    <div id="bcolor-2-18" class="boxcolor" onClick="cambiarColor(2, '18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                </div>
                              </div>

                              <div class="cont_btx comp">
                                <div id="contg">
                                  <div id="pgrupo">
                                    <div class="flecha izq" onClick="cambiarObjeto(0,3)"></div>
                                    <div class="flecha der" onClick="cambiarObjeto(1,3)"></div>
                                    <div id="accimg-3" class="imgprev obj" obj="3"></div>
                                  </div>
                                  <div id="contgrupopos">
                                    <div id="pos-3-0" class="boxpos deselect" onClick="cambiarPos(3, 0)"></div>
                                    <div id="pos-3-1" class="boxpos deselect" onClick="cambiarPos(3, 1)"></div>
                                    <div id="pos-3-2" class="boxpos deselect" onClick="cambiarPos(3, 2)"></div>
                                    <div id="pos-3-3" class="boxpos deselect" onClick="cambiarPos(3, 3)"></div>
                                    <div id="pos-3-4" class="boxpos deselect" onClick="cambiarPos(3, 4)"></div>
                                    <div id="pos-3-5" class="boxpos deselect" onClick="cambiarPos(3, 5)"></div>
                                    <div id="pos-3-6" class="boxpos deselect" onClick="cambiarPos(3, 6)"></div>
                                    <div id="pos-3-7" class="boxpos deselect" onClick="cambiarPos(3, 7)"></div>
                                    <div id="pos-3-8" class="boxpos deselect" onClick="cambiarPos(3, 8)"></div>
                                  </div>
                                  <div id="contgrupocolores">
                                    <div id="bcolor-3-01" class="boxcolor" onClick="cambiarColor(3, '01')" style="background-Color:#FFD500;"></div>
                                    <div id="bcolor-3-02" class="boxcolor" onClick="cambiarColor(3, '02')" style="background-Color:#EB7500;"></div>
                                    <div id="bcolor-3-03" class="boxcolor" onClick="cambiarColor(3, '03')" style="background-Color:#83DD00;"></div>
                                    <div id="bcolor-3-04" class="boxcolor" onClick="cambiarColor(3, '04')" style="background-Color:#579900;"></div>
                                    <div id="bcolor-3-05" class="boxcolor" onClick="cambiarColor(3, '05')" style="background-Color:#4FC0FA;"></div>
                                    <div id="bcolor-3-06" class="boxcolor" onClick="cambiarColor(3, '06')" style="background-Color:#006ECE;"></div>
                                    <div id="bcolor-3-07" class="boxcolor" onClick="cambiarColor(3, '07')" style="background-Color:#FF97E2;"></div>
                                    <div id="bcolor-3-08" class="boxcolor" onClick="cambiarColor(3, '08')" style="background-Color:#F233BE;"></div>
                                    <div id="bcolor-3-09" class="boxcolor" onClick="cambiarColor(3, '09')" style="background-Color:#FF2C2C;"></div>
                                    <div id="bcolor-3-10" class="boxcolor" onClick="cambiarColor(3, '10')" style="background-Color:#AE0909;"></div>
                                    <div id="bcolor-3-11" class="boxcolor" onClick="cambiarColor(3, '11')" style="background-Color:#FFFFFF;"></div>
                                    <div id="bcolor-3-12" class="boxcolor" onClick="cambiarColor(3, '12')" style="background-Color:#BFBFBF;"></div>
                                    <div id="bcolor-3-13" class="boxcolor" onClick="cambiarColor(3, '13')" style="background-Color:#363636;"></div>
                                    <div id="bcolor-3-14" class="boxcolor" onClick="cambiarColor(3, '14')" style="background-Color:#FAE6AB;"></div>
                                    <div id="bcolor-3-15" class="boxcolor" onClick="cambiarColor(3, '15')" style="background-Color:#967540;"></div>
                                    <div id="bcolor-3-16" class="boxcolor" onClick="cambiarColor(3, '16')" style="background-Color:#C1E9FF;"></div>
                                    <div id="bcolor-3-17" class="boxcolor" onClick="cambiarColor(3, '17')" style="background-Color:#FFF064;"></div>
                                    <div id="bcolor-3-18" class="boxcolor" onClick="cambiarColor(3, '18')" style="background-Color:#A9FF7C;"></div>
                                  </div>
                                </div>
                              </div>
                            </div>

                          </div>

                        <p>
                            Te queda(n) <b><?php echo $Gang->Data['badge_changes']; ?></b> cambio(s) gratuito(s)
                            <center>
                                <button id="BuyGangBadge" onclick="return ChangeBadge();" type="button" class="button green">Actualizar emblema <?php echo ($Gang->Data['badge_changes'] > 0) ? "(GRATIS)" : "($10,000)"?></button>
                            </center>
                        </p>
                    <?php endif; ?>              

                </section>
              </div>
            </div>
        <?php endif; ?>

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

<script type="text/javascript">
    const openEls = document.querySelectorAll("[data-open]");
    const closeEls = document.querySelectorAll("[data-close]");
    const isVisible = "is-visible";

    for (const el of openEls) {
      el.addEventListener("click", function() {
        const modalId = this.dataset.open;
        document.getElementById(modalId).classList.add(isVisible);
      });
    }

    for (const el of closeEls) {
      el.addEventListener("click", function() {
        this.parentElement.parentElement.parentElement.classList.remove(isVisible);
      });
    }

    document.addEventListener("click", e => {
      if (e.target == document.querySelector(".modal.is-visible")) {
        document.querySelector(".modal.is-visible").classList.remove(isVisible);
      }
    });

    document.addEventListener("keyup", e => {
      // if we press the ESC
      if (e.key == "Escape" && document.querySelector(".modal.is-visible")) {
        document.querySelector(".modal.is-visible").classList.remove(isVisible);
      }
    });
</script>

<script type="text/javascript">
    function ChangeBadge() {
        var primCol = rgb2hex($("#ColorPrim").css('background-color')).split('#')[1];
        var secCol = rgb2hex($("#ColorSec").css('background-color')).split('#')[1];
        var badge = milink;
        //var data = "badge=" + badge + "&primCol=" + primCol + "&secCol=" + secCol;

        $.post("/gang.php",
        {
            badge: badge,
            primCol: primCol,
            secCol: secCol
        },
        function(data){
            if(data.result == true){
                $("#BuyGangBadge").attr('disabled', true);
                $("#BuyGangBadge").html("Cargando...");

                $("#ErrorMsg").html("<b style='color:green'>" + data.msg + " Refescando...</b>");
                setTimeout(function(){ location.reload(); }, 3000);
            } else {
                $("#ErrorMsg").html(data.msg);
            }
        });

        return false;
    }
</script>
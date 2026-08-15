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
    .calendar_off {
        background-color: #393939; border-radius: 4px; opacity: 0.6;
    }
    .calendar_active {
        background-color: #384708; border-radius: 4px; border: 4px solid #84f24e
    }
    .calendar_event {
        background-color: #384708; border-radius: 4px;
    }
</style>
<?php
    $Ev1 = "calendar_event";
    $Ev2 = "calendar_event";
    $Ev3 = "calendar_event";

    // Current 31
    if(time() >= 1604016000 && time() <= 1604102340){
        $Ev1 = "calendar_active";
    }

    // Current 31
    if(time() >= 1604102400 && time() <= 1604188740){
        $Ev1 = "calendar_off";
        $Ev2 = "calendar_active";
    }

    // Current 1
    if(time() >= 1604188800 && time() <= 1604275140){
        $Ev1 = "calendar_off";
        $Ev2 = "calendar_off";
        $Ev2 = "calendar_active";
    }

    if(time() > 1604275140) {
        $Ev1 = "calendar_off";
        $Ev2 = "calendar_off";
        $Ev2 = "calendar_off";
    }
?>
<div class="content-box mb-2">
    <div class="title">
        <i class="fas fa-skull text-success"></i> Eventos Halloween
    </div>
    <div class="box-content">
        <div class="current-potw p-2 text-center">
            <small>¡Halloween ya está en HabboVIP! Este es el calendario de eventos que tenemos preparado.</small><br>
            <div class="<?php echo $Ev1; ?> d-flex justify-content-center align-items-center text-center p-2 mt-3">
                <div class="pr-2 pl-2">30 de Octubre<br><small>Por <a class="text-white" href="https://habbovip.us/profile/2887">Warrior</a></small></div>
                <div class="ml-auto mr-5 font-weight-bolder text-right">Fiesta de disfraces<br><small>Inicia 23:30 PM GMT+1</small><br></div>
                <div class="mr-4"><img src="https://game.peakrp.com/c_images/album1584/FI045.gif"></div>
            </div>
            
            <div class="<?php echo $Ev2; ?> d-flex justify-content-center align-items-center text-center p-2 mt-1">
                <div class="pr-2 pl-2">31 de Octubre</div>
                <div class="ml-auto mr-5 font-weight-bolder text-right">Noche de Purga<br><small>Inicia 23:59 PM GMT-6</small><br></div>
                <div class="mr-4"><img src="https://game.peakrp.com/c_images/album1584/HBQNO.gif"></div>
            </div>

            <div class="<?php echo $Ev3; ?> d-flex justify-content-center align-items-center text-center p-2 mt-2 mb-1">
                <div class="pr-2 pl-2">1 de Noviembre<br><small>Por <a class="text-white" href="https://habbovip.us/profile/2149">Screamer</a></small></div>
                <div class="ml-auto mr-5 font-weight-bolder text-right">Games Halloween<br><small>Inicia 18:00 PM GMT-5</small></div>
                <div class="mr-4"><img src="https://habboo-a.akamaihd.net/c_images/album1584/NL798.png"></div>
            </div>
        </div>
    </div>
</div>
<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */ ?>
<body>

<img src="<?php echo IMG; ?>/backgrounds/bg.jpg" id="bg" alt="">

<div class="d-flex flex-column justify-content-center align-items-center mt-5">
    <div class="mb-2">
        <img id="main-logo" src="<?php echo IMG; ?>/logos/hv_logo_p.png" style="width: 265px;">
    </div>
    <div class="login mt-4">
        <div class="alert alert-info" style="max-width: 380px;">
            <strong><i class="fas fa-alert-circle"></i> Parfait ! &nbsp;</strong> Il ne te reste plus qu’à choisir un pseudo pour jouer.
        </div>
        <!--<div class="text-center mb-3"><b>254</b> citizens online</div>-->
        <div id="e-fb-message" class="alert alert-danger" style="display: none; max-width: 380px;">
            <strong><i class="fas fa-alert-circle"></i> Oops: &nbsp;</strong><div id="e-fb-msg" style="float: right;"></div>
        </div>
        <div id="fb-message" class="alert alert-success" style="display: none; max-width: 380px;">
            <strong><i class="fas fa-alert-circle"></i> ¡Perfecto! &nbsp;</strong> <div id="fb-msg"></div>
        </div>
            <div class="form-group">
                <input id="pz-fb-uname" type="text" class="form-control username-input" name="pz-fb-uname" placeholder="Pseudo" value="" required="true" autofocus="true" autocomplete="on">
            </div>
            <input type="hidden" name="pz-fb-email" value="<?php echo $Data->email; ?>" />
            <input type="hidden" name="pz-fb-id" value="<?php echo $Data->id; ?>" />
            <input type="hidden" name="pz-fb-name" value="<?php echo $Data->name; ?>" />

            <div class="text-center mb-3"></div>
            <button id="submit-fb-username" type="submit" class="button blue w-100">Créer mon pseudo</button>

    </div>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" integrity="sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1" crossorigin="anonymous" type="text/javascript"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" integrity="sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM" crossorigin="anonymous" type="text/javascript"></script>

<script src="<?php echo DY; ?>/js/fb_login.js?<?php echo time(); ?>" type="text/javascript"></script>

</body>



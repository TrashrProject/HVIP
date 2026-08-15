<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */ ?>

<body onclick="UnMute()" style="background: url(<?php echo IMG;?>/backgrounds/index3.png) no-repeat center center fixed;
    -webkit-background-size: cover;
    -moz-background-size: cover;
    -o-background-size: cover;
    background-size: cover;
    background-position-y: 100%;">

<!--<img src="php echo IMG; /backgrounds/bg.jpg" id="bg" alt="">-->
<div class="d-flex flex-column justify-content-center align-items-center mt-5">
    <div class="mb-2 ">
        <a href="/"><img id="main-logo" src="<?php echo IMG; ?>/logos/hv_logo_p.png" style="width: 265px;"></a>
    </div>
</div>
<div class="d-flex justify-content-center mr-6" >
    <center style="display:flex;">
    <div id="login-box" class="login mt-4" style="">
        <!--<div class="text-center mb-3"><b>254</b> citizens online</div>-->
        <div id="e-login-message" class="alert alert-danger" style="display: none">
            <strong><i class="fas fa-alert-circle"></i> Oops:   &nbsp;</strong><div id="e-login-msg" style="float: right;"></div>
        </div>
        <div id="login-message" class="alert alert-success" style="display: none">
            <strong><i class="fas fa-alert-circle"></i> ¡Perfecto! &nbsp;</strong> <div id="login-msg"></div>
        </div>
        <?php if(isset($_GET['logout']) && $_GET['logout'] == "success"): ?>
            <div id="login-message" class="alert alert-success">
                <strong><i class="fas fa-alert-circle"></i> ¡Hecho! &nbsp;</strong> Has cerrado sesi&oacute;n correctamente.
            </div>
        <?php endif; ?>
        <div class="form-group">
            <input id="pz-login-uname" type="text" class="form-control username-input" name="pz-login-uname" placeholder="Nombre de usuario" value="" required="" autofocus="" autocomplete="on">
        </div>
        <div class="form-group">
            <input id="pz-login-pass" type="password" class="form-control password-input" name="pz-login-pass" placeholder="Contraseña" required="" autocomplete="on">
        </div>
        <div class="form-check text-center mb-4">
            <input class="form-check-input" type="checkbox" name="pz_remember" value="1">
            <label class="form-check-label" for="remember">
                Mantener conectado
            </label>
        </div>
        <div class="text-center mb-3"></div>
        <button id="subrmit-login" type="submit" class="button blue w-100">Iniciar sesi&oacute;n</button>

        <!--<center>&Oacute;</center>-->
        <button id="fb-login" type="submit" class="button blue w-100" style=""> ¡Entrar con Facebook! </button>
        <hr>
        <a id="show-register" class="button green text-center w-100">¡Regístrate Gratis!</a>
    </div>
    <div id="register-box" class="login mt-4" style="display: none;">

        <div id="e-register-message" class="alert alert-danger" style="display: none">
            <strong><i class="fas fa-alert-circle"></i> Oops:   &nbsp;</strong><div id="e-register-msg" style="float: right;"></div>
        </div>
        <div id="register-message" class="alert alert-success" style="display: none">
            <strong><i class="fas fa-alert-circle"></i> Perfecto! &nbsp;</strong> <div id="register-msg"></div>
        </div>

        <div class="form-group">
            <input id="register-username" type="text" class="form-control username-input" name="username" value="" placeholder="Nombre de usuario" required="" autofocus="">
        </div>
        <div class="create-password">
            <div class="form-group">
                <input id="register-password" type="password" class="form-control password-input" name="password" placeholder="Contraseña" autocomplete="new-password" required="">
            </div>
            <div class="form-group">
                <input id="register-password-confirm" type="password" class="form-control password-input" name="password_confirmation" placeholder="Repite tu contraseña" required="">
            </div>
        </div>
        <div class="form-group d-flex">
            <div class="col text-right">
                <img style="padding-right: 10px" src="<?php echo IMG; ?>/male.gif"><input id="genre-m" type="radio" name="gender" value="M" checked="" required=""> Hombre<br>
            </div>
            <div class="col text-left">
                <img style="padding-right: 10px" src="<?php echo IMG; ?>/female.gif"><input id="genre-f" type="radio" name="gender" value="F" required=""> Mujer
            </div>
        </div>

        <button id="subrmit-register" type="submit" class="button green w-100">Registrar</button>

        <hr>
        <a id="show-login" class="button blue w-100 text-center"><i class="fas fa-long-arrow-alt-left"></i> Ya tengo Cuenta</a>
    </div>


</center>

</div>
<div class="d-flex flex-column justify-content-center align-items-center mt-1">
    <div class="mb-2 " style="color: white; ">
    <style>
    a {
        color: #ffffff;
    }
    </style>
       <a href="/privacy.html">Política de Privacidad </a> - <a href="/terms.html">Términos y Condiciones </a> - <a href="https://discord.gg/4M5f4aQwNb">Contacto</a>
    </div>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" integrity="sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1" crossorigin="anonymous" type="text/javascript"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" integrity="sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM" crossorigin="anonymous" type="text/javascript"></script>

<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
<script>var autoPlayVideo = document.getElementById("ocScreencapVideo");
    autoPlayVideo.oncanplaythrough = function() {
        autoPlayVideo.muted = true;
        autoPlayVideo.play();
        autoPlayVideo.pause();
        autoPlayVideo.play();
    }

    function UnMute() {
        autoPlayVideo.muted = false;
    }
</script>
</body>




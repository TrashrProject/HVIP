<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * Localhost development view.
 */ ?>

<body style="background: url(<?php echo IMG;?>/backgrounds/index3.png) no-repeat center center fixed;
    -webkit-background-size: cover;
    -moz-background-size: cover;
    -o-background-size: cover;
    background-size: cover;
    background-position-y: 100%;">

<div class="d-flex flex-column justify-content-center align-items-center mt-5">
    <div class="mb-2">
        <a href="<?php echo URL; ?>/"><img id="main-logo" src="<?php echo IMG; ?>/logos/hv_logo_p.png" style="width: 265px;"></a>
    </div>
</div>

<div class="d-flex justify-content-center mr-6">
    <center style="display:flex;">
        <div id="login-box" class="login mt-4">
            <div id="e-login-message" class="alert alert-danger" style="display:none">
                <strong><i class="fas fa-alert-circle"></i> Oops: &nbsp;</strong><div id="e-login-msg" style="float:right;"></div>
            </div>
            <div id="login-message" class="alert alert-success" style="display:none">
                <strong><i class="fas fa-alert-circle"></i> ¡Perfecto! &nbsp;</strong><div id="login-msg"></div>
            </div>

            <?php if(isset($_GET['logout']) && $_GET['logout'] === "success"): ?>
                <div class="alert alert-success">
                    <strong><i class="fas fa-alert-circle"></i> ¡Hecho! &nbsp;</strong> Has cerrado sesi&oacute;n correctamente.
                </div>
            <?php endif; ?>

            <div class="form-group">
                <input id="pz-login-uname" type="text" class="form-control username-input" placeholder="Nombre de usuario" required autofocus autocomplete="username">
            </div>
            <div class="form-group">
                <input id="pz-login-pass" type="password" class="form-control password-input" placeholder="Contraseña" required autocomplete="current-password">
            </div>
            <div class="form-check text-center mb-4">
                <input class="form-check-input" type="checkbox" name="pz_remember" value="1">
                <label class="form-check-label">Mantener conectado</label>
            </div>

            <button id="subrmit-login" type="button" class="button blue w-100">Iniciar sesi&oacute;n</button>
            <button id="fb-login" type="button" class="button blue w-100" style="display:none">¡Entrar con Facebook!</button>
            <hr>
            <a id="show-register" class="button green text-center w-100">¡Regístrate Gratis!</a>
        </div>

        <div id="register-box" class="login mt-4" style="display:none;">
            <div id="e-register-message" class="alert alert-danger" style="display:none">
                <strong><i class="fas fa-alert-circle"></i> Oops: &nbsp;</strong><div id="e-register-msg" style="float:right;"></div>
            </div>
            <div id="register-message" class="alert alert-success" style="display:none">
                <strong><i class="fas fa-alert-circle"></i> Perfecto! &nbsp;</strong><div id="register-msg"></div>
            </div>

            <div class="form-group">
                <input id="register-username" type="text" class="form-control username-input" placeholder="Nombre de usuario" required autocomplete="username">
            </div>
            <div class="form-group">
                <input id="email" type="email" class="form-control" placeholder="Adresse e-mail" required autocomplete="email">
            </div>
            <div class="create-password">
                <div class="form-group">
                    <input id="register-password" type="password" class="form-control password-input" placeholder="Contraseña" autocomplete="new-password" required>
                </div>
                <div class="form-group">
                    <input id="register-password-confirm" type="password" class="form-control password-input" placeholder="Repite tu contraseña" autocomplete="new-password" required>
                </div>
            </div>

            <div class="form-group d-flex">
                <div class="col text-right">
                    <img style="padding-right:10px" src="<?php echo IMG; ?>/male.gif"><input id="genre-m" type="radio" name="gender" value="M" checked required> Hombre<br>
                </div>
                <div class="col text-left">
                    <img style="padding-right:10px" src="<?php echo IMG; ?>/female.gif"><input id="genre-f" type="radio" name="gender" value="F" required> Mujer
                </div>
            </div>

            <button id="subrmit-register" type="button" class="button green w-100">Registrar</button>
            <hr>
            <a id="show-login" class="button blue w-100 text-center"><i class="fas fa-long-arrow-alt-left"></i> Ya tengo Cuenta</a>
        </div>
    </center>
</div>

<div class="d-flex flex-column justify-content-center align-items-center mt-1">
    <div class="mb-2" style="color:white;">
        <style>a { color:#ffffff; }</style>
        <a href="<?php echo URL; ?>/privacy.html">Política de Privacidad</a> -
        <a href="<?php echo URL; ?>/terms.html">Términos y Condiciones</a>
    </div>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" crossorigin="anonymous"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" crossorigin="anonymous"></script>
<script src="<?php echo DY; ?>/js/index.js?<?php echo time(); ?>" type="text/javascript"></script>
</body>

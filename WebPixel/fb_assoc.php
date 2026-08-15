<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

require_once "app/init.pz.php";


// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;


if(isset($_GET['code'])):

    $Token = json_decode($FBManager->ExchangeCodeAssoc($_GET['code']));
    $Data = json_decode($FBManager->GetFBData($Token->access_token));
    if($Data->id == null):
        echo "<b>Error [5001] : Contacta a un administrador en nuestro <a href='https://discord.gg/4M5f4aQwNb'>Discord</a></b>";
        exit;
    endif;

    $R = json_decode($FBManager->FBAssoc($UData['username'], $Data->email, $Data->id));

    require_once HEADER . "fb_login.php";
    ?>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" integrity="sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1" crossorigin="anonymous" type="text/javascript"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" integrity="sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM" crossorigin="anonymous" type="text/javascript"></script>

    <?php

    if($R->result == true):
        echo "<script>setTimeout(function(){window.opener.location.href = \"/account?fb_assoc=true\";self.close();}, 3000);</script>";

    else:
        echo "<script> setTimeout(function(){window.opener.location.href = \"/account?fb_assoc=false\";self.close();}, 3000);</script>";

    endif;

    require_once BODY . "fb_assoc.php";

endif;



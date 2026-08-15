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


        <div class="container">
            <div class="row">
                <div class="col-8">
                    <div class="staff-grid">
                        <?php
                        $R_ = $UserMG->GetStaffs();
                        while($Row = mysqli_fetch_assoc($R_)): 
                            if($Row['rank'] == 6): $Badge = "DEV"; elseif($Row['rank'] == 5): $Badge = "ADM"; elseif($Row['rank'] == 4): $Badge = "MOD"; elseif($Row['rank'] == 3): $Badge = "STAFF"; endif;
                        ?>
                        <a href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" class="content-box mb-0 no-link-styling" style="<?php echo ($Row['online'] == '1')? "border-bottom: 3px solid #1dc40e;" : "" ; ?>">
                            <div class="title d-flex align-items-center" style="">
                                <div class="mr-auto" style="padding-left: 115px;"><?php echo $Row['username']; ?></div>
                                <div><span class="float-right pr-3"></span></div>
                            </div>
                            <div class="d-flex align-items-center">
                                <div class="staff-member-avatar">
                                    <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3&amp;size=l&amp;gesture=sml">
                                </div>
                                <div class="mr-auto">
                                    <div class="staff-role text-white">
                                        <?php if($Row['rank'] == 6): echo "Desarrollador / Dueño"; elseif($Row['rank'] == 5): echo "Administrador"; elseif($Row['rank'] == 4): echo "Moderador"; elseif($Row['rank'] == 3): echo "Ayudante"; endif; ?>

                                    </div>
                                </div>
                                <div class="mr-3"><img src="<?php echo DY; ?>/img/staff/<?php echo $Badge; ?>.gif"></div>
                            </div>
                        </a>

                        <?php endwhile; ?>

                    </div>
                </div>
                <div class="col-4 pl-0">
                    <div class="help-grid">
                        <div class="content-box">
                            <div class="title">¿Necesitas ayuda?</div>
                            <div class="p-3">
                                La manera más fácil de contactarnos es a través de nuestro discord official, ¡click <a href="<?php echo Config::$DiscordInvite; ?>" target="_blank"> AQU&Iacute; </a>para ingresar!<br>
                                También puedes contactar a nuestro Equipo Staff para preguntas y pedidos de ayuda... No olvides que puedes usar el comando <b>:n [pregunta]</b> para mandar una duda general.
                                <center><img class="pt-3" src="<?php echo IMG; ?>/extras/frank_signs.gif"></center>
                                <hr>

                                Si no puedes entrar a jugar, Por favor usa nuestro <a class="text-white font-weight-bold text-decoration-none" href="<?php echo Config::$DiscordInvite; ?>" target="_blank">Discord</a> para enviarnos un mensaje.
                            </div>
                        </div>
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
            </div>
        </div>


    </div>
</div>
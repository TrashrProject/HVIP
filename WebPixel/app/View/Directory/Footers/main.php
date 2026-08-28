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


<div class="footer">
    <div class="container d-flex justify-content-center align-items-center text-center">
        <div class="cms">
            ParadiseCMS 1.0.0
        </div>
        <div class="footer-links mr-auto ml-auto">
            <a href="<?php echo Config::$URL; ?>/rules" class="footer-link no-link-styling">R&egrave;gles</a>
            <a href="<?php echo Config::$DiscordInvite; ?>" target="_blank" class="footer-link no-link-styling">Discord officiel</a>
            <a href="<?php echo Config::$URL; ?>/play" target="_blank" class="footer-link no-link-styling">Entrer dans le jeu</a>
        </div>
        <div class="footer-logo">
            <?php echo Config::$WName; ?> © 2016-<?php echo date("Y"); ?>
        </div>
    </div>
</div>
</div>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js" integrity="sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1" crossorigin="anonymous" type="text/javascript"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js" integrity="sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM" crossorigin="anonymous" type="text/javascript"></script>

<script src="<?php echo DY; ?>/js/rdp_main.js?<?php echo time(); ?>"  type="text/javascript"></script>
<script src="<?php echo DY; ?>/js/cms-fr.js?<?php echo time(); ?>" type="text/javascript"></script>

<script type="text/javascript">
    $(function () {
        $('[data-toggle="tooltip"]').tooltip()
    })

    $(function () {
        $('[data-toggle="popover"]').popover()
    })
</script>


<?php if($PageName == "Ajustes de Cuenta"): ?>
<script>
    // Facebook Login
    $(document).ready(function(){
        $("#fb-link").click(function(){
            var left = (screen.width ) / 3 ;
            var top = (screen.height ) / 5 ;
            let params = `scrollbars=no,resizable=no,status=no,location=no,toolbar=no,menubar=no,
        width=600,height=600,left= ` + left + ` ,top=` + top + ``;
            window.open('<?php echo Config::$FB_ASSOC_API_LINK; ?>', '', params);
        });
    });
</script>

<?php endif; ?>

<?php
// Calls Paypals Javascript
if($PageName == "Tienda"):
    $StoreMG->GetPaypalJavascript();
endif;

?>




</body>
</html>

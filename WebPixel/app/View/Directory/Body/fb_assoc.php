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

    </div>
    <div class="login mt-4">

        <!--<div class="text-center mb-3"><b>254</b> citizens online</div>-->

        <?php if($R->result == true): ?>
        <div id="fb-message" class="alert alert-success" style="max-width: 380px;">
            <strong><i class="fas fa-alert-circle"></i> ¡Perfecto! &nbsp;</strong> <div id="fb-msg" ><?php echo $R->msg;?></div>
        </div>
        <?php else: ?>
            <div id="e-fb-message" class="alert alert-danger" style="max-width: 380px;">
                <strong><i class="fas fa-alert-circle"></i> Oops: &nbsp;</strong><div id="e-fb-msg"><?php echo $R->msg;?></div>
            </div>
        <?php endif; ?>

    </div>
</div>

</body>




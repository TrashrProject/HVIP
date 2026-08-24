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

        <div class="container d-flex">
            <div class="col-6">
                <div class="content-box">
                    <div class="title">Modifier le mot de passe</div>
                    <div class="box-content">
                        <form role="form" method="POST" action="<?php echo URL; ?>/account.php?chanpass" class="form-block p-2">
                            <input type="hidden" name="csrf" value="<?= htmlspecialchars($_SESSION['account_csrf'], ENT_QUOTES, 'UTF-8') ?>">
                            <?php
                            if(isset($_SESSION['E'])):
                                if($_SESSION['E'] == true): ?>

                            <div id="fb-message" class="alert alert-success" >
                                <strong> ¡Perfecto! &nbsp;</strong> <?php echo $_SESSION['M']; ?>
                            </div>
                            <?php else: ?>
                                    <div id="fb-message" class="alert alert-danger" >
                                        <strong> ¡Oops! &nbsp;</strong> <?php echo $_SESSION['M']; ?>
                                    </div>
                               <?php endif;
                                unset($_SESSION['E']);
                               endif; ?>
                            <div class="form-group">
                                <label for="current-password">Saisis ton mot de passe actuel pour continuer</label>
                                <input id="current-password" type="password" class="form-control" name="current_password" placeholder="Mot de passe actuel" required="true">
                            </div>
                            <div class="form-group">
                                <label for="current-password"><strong>Nouveau mot de passe</strong><br>Choisis un mot de passe unique et sécurisé</label>
                                <input id="new-password" type="password" class="form-control" name="new_password" placeholder="Nouveau mot de passe" required="true">
                            </div>
                            <div class="form-group">
                                <label for="current-password"><strong>Confirme le mot de passe</strong><br>Saisis-le une seconde fois</label>
                                <input id="new-password-confirm" type="password" class="form-control" name="new_password_confirmation" placeholder="Confirmer le mot de passe" required="true">
                            </div>
                            <div class="text-right"><button type="submit" class="button green d-inline">Modifier le mot de passe</button></div>
                        </form>
                    </div>
                </div>
            </div>
            <div class="col-6">

                <div class="content-box">
                    <div class="title">Vincular Cuenta a Facebook</div>
                    <div class="box-content d-flex justify-content-center align-items-center">
                        <?php if(!$UserMG->IsLinkedFB($UData['id'])): ?>
                        <div class="ml-3 mr-auto">

                            Actualmente tu cuenta no está vinculada a tu <b>cuenta de Facebook</b>. Si deseas vincular una cuenta, simplemente haz clic en Vincular.

                        </div>
                        <div class="mr-3">

                            <button class="button green" id="fb-link" >Vincular</button>

                        </div>
                        <?php else: ?>
                        <div class="ml-3 mr-auto">
                            Tu cuenta ya está vinculada con una cuenta de Facebook.
                        </div>
                        <?php endif; ?>
                    </div>
                </div>
            </div>
        </div>

    </div>
</div>

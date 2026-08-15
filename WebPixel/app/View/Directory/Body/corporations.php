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


        <div class="corporation-index-grid">

            <?php $Corporations = $DB->Query("SELECT * FROM groups WHERE type = '1' OR type = '2'");
            while ($Cor = mysqli_fetch_assoc($Corporations)):
                $ECount = mysqli_num_rows($DB->Query("SELECT null FROM group_memberships WHERE group_id = ". $Cor['id'] ." "));
                ?>
            <div class="corp">
                <div class="content-box">
                    <div class="title"><?php echo utf8_encode($Cor['name']); ?> <span class="title-small float-right"><small><?php echo $ECount; ?> Empleados</small></span></div>
                    <div class="box-content d-flex justify-content-center align-items-center p-0">
                        <div class="" style="margin-left: -15px;">
                            <div class="manager-wrapper" style="display: block;overflow: hidden;width: 110px;margin-left: 50px; margin-top: 0px; height: 40px;">
                                <img class="corporation-badge" src="<?php echo Config::$SWF; ?>/habbo-imaging/corp/<?php echo $Cor['badge']; ?>.gif">
                            </div>
                        </div>
                        <div class="mr-auto">

                        </div>
                        <div class="pr-3 d-flex">
                            <div class="mr-1"><a class="button blue pr-1" href="<?php echo Config::$URL; ?>/corporation/<?php echo $Cor['id']; ?>">Ver Negocio <i class="far fa-arrow-alt-circle-right"></i></a></div>
                        </div>
                    </div>
                </div>
            </div>
            <?php endwhile; ?>

        </div>


    </div>
</div>

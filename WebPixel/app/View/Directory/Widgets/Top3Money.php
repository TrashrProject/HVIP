<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

// Loop Count
$C = 1;

?>
<div class="content-box mb-2">
    <div class="title">
        <i class="fas fa-star text-warning"></i> Ciudadanos m&aacute;s ricos
    </div>
    <div class="box-content">
        <div class="current-potw p-2 text-center">
            Estos ciudadanos son los que m&aacute;s dinero tienen en la ciudad. Trabaja duro, roba, y haz buenos negocios para poder llegar aqu&iacute;.

            <div class="mt-2 text-center font-weight-bold">Top 3 m&aacute;s Ricos</div>

            <div class="d-flex justify-content-center align-items-center text-center">

                <?php
                $Top = $UserMG->GetTopThree();

                while($Row = mysqli_fetch_assoc($Top)): ?>
                    <div class="flex-fill m-1" style="border-bottom: 2px solid <?php if($C == 1): echo "gold"; elseif($C == 2): echo "silver";  else: echo "#cd7f32;"; endif; ?>">
                        <div class="d-flex justify-content-center align-items-center pb-2">
                            <div>
                                <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=2&amp;gesture=sml&amp;head_direction=3&amp;action=wav">
                            </div>
                            <div class="mr-auto">

                                <img src="<?php echo DY; ?>/img/extras/leader_<?php if($C == 1): echo "1st"; elseif($C == 2): echo "2nd";  else: echo "3rd"; endif; ?>.gif">
                                <br>
                                <strong><?php echo $Row['username']; ?></strong><br> $<?php echo number_format($Row['credits'] + $Row['bank']); ?>
                            </div>
                        </div>
                    </div>
                <?php $C++; endwhile; ?>
            </div>
        </div>
    </div>
</div>


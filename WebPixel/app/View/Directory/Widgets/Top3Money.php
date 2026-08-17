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
        <img class="title-custom-icon" src="<?php echo DY; ?>/img/logos/icon/niveau.png" alt=""> Citoyens les plus riches
    </div>
    <div class="box-content">
        <div class="current-potw p-2 text-center">
            Ces citoyens poss&egrave;dent le plus d'argent en ville. Travaille et d&eacute;veloppe tes affaires pour atteindre le sommet.

            <div class="mt-2 text-center font-weight-bold">Top 3 des plus riches</div>

            <div class="d-flex justify-content-center align-items-center text-center">

                <?php
                $Top = $UserMG->GetTopThree();

                if ($Top instanceof mysqli_result && mysqli_num_rows($Top) > 0):
                while($Row = mysqli_fetch_assoc($Top)): ?>
                    <div class="flex-fill m-1" style="border-bottom: 2px solid <?php if($C == 1): echo "gold"; elseif($C == 2): echo "silver";  else: echo "#cd7f32;"; endif; ?>">
                        <div class="d-flex justify-content-center align-items-center pb-2">
                            <div>
                                <img src="https://www.habbo.es/habbo-imaging/avatarimage?figure=<?php echo rawurlencode($Row['look']); ?>&amp;direction=2&amp;gesture=sml&amp;head_direction=3&amp;action=wav" alt="<?php echo htmlspecialchars($Row['username'], ENT_QUOTES, 'UTF-8'); ?>">
                            </div>
                            <div class="mr-auto">

                                <img src="<?php echo DY; ?>/img/extras/leader_<?php if($C == 1): echo "1st"; elseif($C == 2): echo "2nd";  else: echo "3rd"; endif; ?>.gif">
                                <br>
                                <strong><?php echo $Row['username']; ?></strong><br> $<?php echo number_format($Row['credits'] + $Row['bank']); ?>
                            </div>
                        </div>
                    </div>
                <?php $C++; endwhile; else: ?>
                    <p class="mb-0 text-muted">Aucun citoyen n'est encore class&eacute;.</p>
                <?php endif; ?>
            </div>
        </div>
    </div>
</div>

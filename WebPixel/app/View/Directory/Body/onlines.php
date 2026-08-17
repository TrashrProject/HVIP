<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */
$R_ = $UserMG->GetOnlines();
$T = mysqli_num_rows($R_);
?>
<div class="content">

    <div class="container">

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

        <div class="row">
            <div class="col">
                <div class="content-box">
                    <div class="title">Citoyens connect&eacute;s (<?php echo number_format($T); ?>)</div>
                    <div class="box-content">
                        <?php
                        if($T != 0): ?>
                        <div class="online-grid">


                            <?php while($Row = mysqli_fetch_assoc($R_)): ?>
                            <a href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" class="online-user no-link-styling <?php echo ($UserMG->isVIP($Row['id']))? "online-user-vip " : ""; echo ($Row['rank'] >= 3)? "online-user-staff " : "" ; ?>justify-content-center align-items-center">
                                <div class="online-pixel">
                                    <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3">
                                </div>
                                <div class="username mr-auto" style="white-space: nowrap; overflow: hidden;">
                                    <span><?php echo $Row['username']; ?></span>
                                </div>
                            </a>
                            <?php endwhile; ?>


                        </div>
                           <?php else: ?>
                            <center><b>Aucun citoyen n'est connect&eacute; pour le moment...</b></center>
                        <?php endif; ?>
                    </div>
                </div>
            </div>
        </div>


    </div>
</div>

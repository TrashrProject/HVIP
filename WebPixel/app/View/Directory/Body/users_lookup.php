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
            <div class="col-3">
                <div class="content-box">
                    <div class="title">Buscar Usuarios</div>
                    <div class="box-content">
                            <div class="form-group">
                                <label></label>
                               <center> <input type="text" placeholder="Escribir nombre a buscar" class="form-control" name="username-lookup" style="max-width: 85%;"></center>
                            </div>
                    </div>
                </div>
                <div class="content-box blue leaderboards mb-0">
                    <div class="title">Usuarios al azar</div>
                    <div class="box-content">
                        <?php $US = $DB->Query("SELECT username, id, look, rank FROM users WHERE rank < 3 ORDER BY RAND() LIMIT 5");
                        if(mysqli_num_rows($US) >= 1):
                            while ($US_ = mysqli_fetch_assoc($US)): ?>
                                <a href="<?php echo Config::$URL .'/profile/'. $US_['id']; ?>" style="margin-bottom: 5px;" class="online-user no-link-styling <?php echo ($US_['rank_vip'] >= 1)? "online-user-vip" : "" ; echo ($US_['rank'] >= 3)? "online-user-staff" : "" ; ?> justify-content-center align-items-center">
                                    <div class="online-pixel"><img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $US_['look']; ?>&amp;direction=3"></div>
                                    <div class="username mr-auto" style="white-space: nowrap; overflow: hidden;"><span><?php echo $US_['username']; ?></span></div></a>
                            <?php endwhile; else: echo "No hay usuarios en esta categoria"; endif; ?>

                    </div>
                </div>
            </div>
            <div class="col-9">
                <div class="content-box">
                    <div class="title">Usuarios encontrados</div>
                    <div class="box-content">
                        <div class="online-grid" id="lookup-results">
                            <center><b>Escribe el nombre del usuario para buscar...</b></center>

                        </div>
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


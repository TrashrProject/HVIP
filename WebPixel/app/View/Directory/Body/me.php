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


        <div class="row">
            <div class="col-6">
                <div class="content-box">
                    <div class="title">
                        <i class="fas fa-id-card text-secondary"></i> Estad&iacute;sticas de Juego
                    </div>
                    <div class="box-content">
                        <div class="user-info-pz">
                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-dinero"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Cartera:</b> $<?php echo number_format($UData['credits']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-banco"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Banco:</b> $<?php echo number_format($UPData['bank']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-platinos"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Platinos:</b> <?php echo $UData['vip_points']; ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-level"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Nivel:</b> <?php echo $UPData['level']; ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>

                <div class="content-box">
                    <div id="ms" >
                        <?php
                        $html = file_get_contents('https://swf.habbovip.us/V5-0-2/MPU/pz_v2/image_loader.php'); 
                        echo $html;
                        ?>
                    </div>
                </div>

                

                <div class="content-box">
                    <iframe src="https://discord.com/widget?id=857370545111826452&theme=dark" width="540" height="415" allowtransparency="true" frameborder="0"></iframe>
                </div>
                <!--<div class="peakrp-news d-flex justify-content-center align-items-center">
                    <div class="mr-auto">Noticías <?php echo Config::$WName; ?> ...</div>
                    <div><img src="https://peakrp.com/img/website/icons/news_list.gif"></div>
                </div>
                <div class="d-flex flex-column mt-2">
                    <div class="flex-fill pb-2">
                        <a class="news-item d-flex flex-column justify-content-between" href="https://forums.peakrp.com/threads/.1300" style="background-image: url('https://forums.peakrp.com/attachments/1248');">
                            <div class="d-flex justify-content-end">
                                <div class="news-stats">
                                    <span class="pr-2"><img src="https://peakrp.com/img/website/icons/likes.gif"> 18</span>
                                    <img src="https://peakrp.com/img/website/icons/comments.gif"> 44
                                </div>
                            </div>
                            <div class="news-title d-flex justify-content-center">
                                <div class="ml-3 flex-grow-1">New Corporation - Manager Applications</div>
                                <div class="mr-3"><i class="fas fa-certificate"></i></div>
                            </div>
                        </a>
                    </div>
                    <div class="flex-fill pb-2">
                        <a class="news-item d-flex flex-column justify-content-between" href="https://forums.peakrp.com/threads/.829" style="background-image: url('https://forums.peakrp.com/attachments/864');">
                            <div class="d-flex justify-content-end">
                                <div class="news-stats">
                                    <span class="pr-2"><img src="https://peakrp.com/img/website/icons/likes.gif"> 24</span>
                                    <img src="https://peakrp.com/img/website/icons/comments.gif"> 26
                                </div>
                            </div>
                            <div class="news-title d-flex justify-content-center">
                                <div class="ml-3 flex-grow-1">PeakRP Update 2</div>
                                <div class="mr-3"><i class="fas fa-certificate"></i></div>
                            </div>
                        </a>
                    </div>
                    <div class="flex-fill pb-2">
                        <a class="news-item d-flex flex-column justify-content-between" href="https://forums.peakrp.com/threads/.751" style="background-image: url('https://forums.peakrp.com/attachments/835');">
                            <div class="d-flex justify-content-end">
                                <div class="news-stats">
                                    <span class="pr-2"><img src="https://peakrp.com/img/website/icons/likes.gif"> 7</span>
                                    <img src="https://peakrp.com/img/website/icons/comments.gif"> 8
                                </div>
                            </div>
                            <div class="news-title d-flex justify-content-center">
                                <div class="ml-3 flex-grow-1">Scheduled maintenance</div>
                                <div class="mr-3"><i class="fas fa-certificate"></i></div>
                            </div>
                        </a>
                    </div>
                </div>
                <div class="d-flex mb-2">
                    <a href="https://forums.peakrp.com/" class="twitter jumbo-link d-flex justify-content-center align-items-center">
                        <div class="mr-auto">Forum</div>
                        <div class=""><i class="fas fa-edit"></i></div>
                    </a>
                    <a href="https://wiki.peakrp.com/" class="forum jumbo-link d-flex justify-content-center align-items-center">
                        <div class="mr-auto">Wiki</div>
                        <div class=""><i class="fas fa-info"></i></div>
                    </a>
                    <a href="https://peakrp.com/account" class="discord jumbo-link d-flex justify-content-center align-items-center">
                        <div class="mr-auto">Discord Link</div>
                        <div class=""><i class="fab fa-discord font-weight-normal"></i></div>
                    </a>
                </div>-->
            </div>
            <div class="col-6">
                <?php require_once WIDGETS . 'Top3Money.php'; ?>
                <?php require_once WIDGETS . 'ServerStats.php'; ?>
            </div>
        </div>


    </div>
    <center><script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
<!-- Responsive -->
<ins class="adsbygoogle"
     style="display:block"
     data-ad-client="ca-pub-5384077970237124"
     data-ad-slot="7246095666"
     data-ad-format="auto"
     data-full-width-responsive="true"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script></center>
</div>


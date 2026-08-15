<?php
$reporterAccess = true;
include_once "./Header.php";
?>

<div class="main_content_iner ">
    <div class="container-fluid p-0">
        <div class="row justify-content-center">
            <div class="col-lg-12">
                <div class="single_element">
                    <div class="quick_activity">
                        <div class="row">
                            <div class="col-12">
                                <div class="quick_activity_wrap">
                                    <div class="single_quick_activity d-flex">
                                        <div class="icon">
                                            <i class="fas fa-users" style="color:#3eb93e"></i>
                                        </div>
                                        <div class="count_content">
                                            <h3><span class="counter"><?php echo $UserMG->GetStatData("users_online"); ?></span> </h3>
                                            <p>Usuarios conectados</p>
                                        </div>
                                    </div>
                                    <div class="single_quick_activity d-flex">
                                        <div class="icon">
                                            <i class="fas fa-user-plus" style="color:#2daab8"></i>
                                        </div>
                                        <div class="count_content">
                                            <h3><span class="counter"><?php echo $UserMG->GetStatData("users_registered"); ?></span> </h3>
                                            <p>Usuarios registrados</p>
                                        </div>
                                    </div>
                                    <div class="single_quick_activity d-flex">
                                        <div class="icon">
                                            <i class="fas fa-user-times" style="color:#cb2a2a"></i>
                                        </div>
                                        <div class="count_content">
                                            <h3><span class="counter"><?php echo $UserMG->GetStatData("users_bans"); ?></span> </h3>
                                            <p>Usuarios baneados</p>
                                        </div>
                                    </div>
                                    <div class="single_quick_activity d-flex">
                                        <div class="icon">
                                            <i class="fas fa-money-bill-alt" style="color:green"></i>
                                        </div>
                                        <div class="count_content">
                                            <h3>$<span class="counter"><?php echo $UserMG->GetStatData("money"); ?></span> </h3>
                                            <p>Dinero Total</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<?php
include_once "./Footer.php";
?>
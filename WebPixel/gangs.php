<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

require_once "app/init.pz.php";


if(isset($_POST['gname'])):

    $G = AppFunctions::GeneralClean($_POST['gname']);
    $LookUP = $DB->Query("SELECT name, id, badge, type FROM groups WHERE name LIKE '%". $G ."%' AND type = '3' LIMIT 20");
    if(mysqli_num_rows($LookUP) <= 0): ?>
        <div class="content-box">
            <div class="title">Oops!</div>
            <div class="box-content">
                <center><b>Aucun gang ne correspond à ce nom.</b></center>
            </div>
        </div>
<?php
    exit;
    endif;
    echo '<div class="corporation-index-grid" >';
    // Show results
    while ($Results = mysqli_fetch_assoc($LookUP)):
        $EGCount = mysqli_num_rows($DB->Query("SELECT null FROM group_memberships WHERE group_id = " . $Results['id'] . " "));
        ?>
        <div class="corp">
            <div class="content-box">
                <div class="title"><?php echo utf8_encode($Results['name']); ?> <span
                        class="title-small float-right"><small><?php echo $EGCount; ?> Miembros</small></span>
                </div>
                <div class="box-content d-flex justify-content-center align-items-center p-0">
                    <div class="" style="margin-left: -15px;">
                        <div class="manager-wrapper"
                             style="display: block;overflow: hidden;width: 110px;margin-left: 50px; margin-top: 0px; height: 40px;">
                            <img class="corporation-badge"
                                 src="https://swf.habbovip.us/group-badge/badge/<?php echo $Results['badge']; ?>">
                        </div>
                    </div>
                    <div class="mr-auto">

                    </div>
                    <div class="pr-3 d-flex">
                        <div class="mr-1"><a class="button blue pr-1"
                                             href="<?php echo Config::$URL; ?>/gang/<?php echo $Results['id']; ?>">Ver <i class="far fa-arrow-alt-circle-right"></i></a></div>
                    </div>
                </div>
            </div>
        </div>
    <?php
    endwhile;
    echo "</div>";
    exit;
endif;

// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;

//Page Name
$PageName = "Bandas";

// Load Pages
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'gangs.php';
require_once FOOTER . 'main.php';

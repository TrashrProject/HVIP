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

// La recherche rapide de l'en-tete va directement sur le profil lorsqu'un
// pseudo exact existe. Les recherches approximatives restent disponibles sur
// cette page pour ne pas bloquer l'utilisateur en cas de faute de frappe.
if(isset($_GET['q'])):
    $QuickSearch = trim(AppFunctions::GeneralClean($_GET['q']));
    if($QuickSearch !== ''):
        $ExactUser = $DB->Query("SELECT id FROM `users` WHERE `username` = '". $QuickSearch ."' LIMIT 1");
        if($ExactUser && mysqli_num_rows($ExactUser) === 1):
            $ExactUserData = mysqli_fetch_assoc($ExactUser);
            header("Location: " . Config::$URL . "/profile/" . (int)$ExactUserData['id']);
            exit;
        endif;
    endif;
endif;

if(isset($_POST['uname'])):

    $U = AppFunctions::GeneralClean($_POST['uname']);
    $LookUP = $DB->Query("SELECT id, username, look, online, rank, rank_vip FROM `users` WHERE `username` LIKE '%". $U ."%' LIMIT 20");
    if(mysqli_num_rows($LookUP) <= 0):
        echo "<center><b>Aucun citoyen ne correspond à cette recherche.</b></center>";
        exit;
    endif;

    // Show results
    while ($Results = mysqli_fetch_assoc($LookUP)): ?>
        <a href="<?php echo Config::$URL .'/profile/'. $Results['id']; ?>" class="online-user no-link-styling <?php echo ($Results['rank_vip'] >= 1)? "online-user-vip" : "" ; echo ($Results['rank'] >= 3)? "online-user-staff" : "" ; ?> justify-content-center align-items-center">
        <div class="online-pixel"><img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Results['look']; ?>&amp;direction=3"></div>
        <div class="username mr-auto" style="white-space: nowrap; overflow: hidden;"><span><?php echo $Results['username']; ?></span></div></a>
    <?php
    endwhile;

    exit;
endif;


// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;

//Page Name
$PageName = "Buscar Usuario";

// Load Pages
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'users_lookup.php';
require_once FOOTER . 'main.php';

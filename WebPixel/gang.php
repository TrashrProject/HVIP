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

if (isset($_POST['badge'])):
    $B = AppFunctions::GeneralClean($_POST['badge']);
    $C1 = AppFunctions::GeneralClean($_POST['primCol']);
    $C2 = AppFunctions::GeneralClean($_POST['secCol']);

    $Response['result'] = false;
    $Response['msg'] = "";

    $LookUP = $DB->Query("SELECT * FROM groups WHERE owner_id = ".$UData['id']." AND type = '3'");
    if(mysqli_num_rows($LookUP) > 0):
        while ($Results = mysqli_fetch_assoc($LookUP)):
            $Cost = ($Results['badge_changes'] > 0) ? 0 : 10000;

            if ($Cost > $UData['credits']):
                $Response['result'] = false;
                $Response['msg'] = "Necesitas $" . number_format($Cost) . " para editar el emblema de tu banda.";
            else:
                if ($DB->Query("UPDATE groups SET badge = '".$B."', colour1 = '".$C1."', colour2 = '".$C2."' WHERE owner_id = ".$UData['id']." AND type = '3'")):

                    // Restamos cambio de emblema gratuito
                    if ($Results['badge_changes'] > 0)
                        $DB->Query("UPDATE groups SET badge_changes = badge_changes - 1 WHERE owner_id = ".$UData['id']." AND type = '3'");
                    else
                        // Restamos dinero
                        $DB->Query("UPDATE users SET credits = credits - ".$Cost." WHERE id = ".$UData['id']);

                    $Response['result'] = true;
                    $Response['msg'] = "¡Éxito! Emblema de tu banda actualizado exitosamente.";
                endif;
            endif;

        endwhile;
    endif;

    // Mandamos respuesta AJAX
    header('Content-type: application/json');
    echo json_encode($Response);
    exit;
endif;

// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;

if(!isset($_GET['gang_id'])):
    header("Location: /gangs");
    exit;
endif;

if(!$Gang->Status):
    header("Location: /gangs");
    exit;
endif;

//Page Name
$PageName = "Perfil de Banda";

// Load Pages
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'gang.php';
require_once FOOTER . 'main.php';



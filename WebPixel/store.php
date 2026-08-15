<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

//Page Name
$PageName = "Tienda";

require_once "app/init.pz.php";

// Redirect if logged in
if(!$Session->Exist(Config::$SessionName)):
    header("Location: /");
    exit;
endif;


require_once 'app/Controller/StoreManager.class.php';
$StoreMG = new StoreManager($DB);

// If store checkout requested
if(isset($_POST['orderID']) && isset($_POST['itemID'])):
    $OrderID = AppFunctions::InjectionCleaner($_POST['orderID']);
    $ItemID = AppFunctions::InjectionCleaner($_POST['itemID']);

    require_once 'app/Controller/PayPalManager.php';
    $_P = new PayPalManager($DB, $OrderID, $ItemID, $UData);

    // Execute Transaction
    echo $_P->ProcessTransaction();
    exit;
endif;

if(isset($_POST['vipType'])):
	$VIPType = AppFunctions::InjectionCleaner($_POST['vipType']);
	echo $UserMG->BuyVIP($VIPType, $UData["id"]);
    exit;
endif;

// Load Pages
require_once HEADER . 'main.php';
require_once NAVBAR . 'navbar.php';
require_once BODY . 'Store.php';
require_once FOOTER . 'main.php';
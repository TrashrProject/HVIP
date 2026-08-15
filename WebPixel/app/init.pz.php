<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

require_once 'Controller/Config.class.php';
require_once 'Controller/DBManager.class.php';
require_once 'Modal/SessionMG.class.php';

// Declare constants Variables
define('DS', DIRECTORY_SEPARATOR);
define('API', dirname(__FILE__) . DS);
define('VIEW', API . 'View' . DS . 'Directory' . DS);
define('HEADER', VIEW . 'Headers' . DS);
define('FOOTER', VIEW . 'Footers' . DS);
define('BODY', VIEW . 'Body' . DS);
define('NAVBAR', VIEW . 'Navigation' . DS);
define('WIDGETS', VIEW . 'Widgets' . DS);
define('CLIENT', VIEW . 'Client' . DS);
define('WS_DY', Config::$URL . '/app/View/Directory/Client/websockets');
define('DY', Config::$WEB_DY);
define('CSS', DY . '/css');
define('IMG', DY . '/img');
define('JS', DY . '/js');
define('URL', Config::$URL);

// Initialize DB Manager
$DB = new DBManager();
// Initialize Session Manager
$Session = new SessionMG();
require_once 'Modal/AppFunctions.php';
require_once 'Modal/UserSessionMG.class.php';



// Initialize Site Session
$USession = new UserSessionMG($DB, $Session);

// If Login requested
if(isset($_POST['login_username']) && isset($_POST['login_password'])):
    echo $USession->Login($_POST['login_username'], $_POST['login_password']);
    exit;
endif;

// If Registration requested
if(isset($_POST['reg_username']) && isset($_POST['reg_password']) && isset($_POST['reg_mail'])):
    echo $USession->Registration($_POST['reg_username'], $_POST['reg_password'], $_POST['reg_mail']);
    exit;
endif;

// Construct User if logged
require_once 'Controller/User.class.php';
if($Session->Exist(Config::$SessionName)):
    $User = new User($DB, $Session);
    // Re-declare variables for user rows
    $UData = $User->UData;
    $UPData = $User->UPData;

    if(Config::$_MANT && $UData['rank'] < 5 && $UData['username'] != "Tester"):
        $Session->Delete(Config::$SessionName);
        $Session->Destroy();
        header("Location: /man.html");
        exit;
    endif;

endif;

// Construct UserManager
require_once 'Controller/UserManager.class.php';
$UserMG = new UserManager($DB, $USession);

// Construct Gangs Manager
require_once 'Controller/GangsManager.class.php';
$GangMG = new GangsManager($DB);

if(isset($_GET['gang_id'])):
    require_once "Controller/Gang.class.php";
    $Gang = new Gang($DB, $_GET['gang_id']);
endif;

require_once 'Controller/BusinessManager.class.php';
// Construct Business Manager
$BusinessMG = new BusinessManager($DB);

if(isset($_GET['fb_login']) || isset($_GET['code'])):
    require_once 'Controller/FBManager.class.php';
    $FBManager = new FBManager($DB, $USession);
endif;









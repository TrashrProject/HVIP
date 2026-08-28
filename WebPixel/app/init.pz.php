<?php
/**
 * ParadiseRP / WavePlus CMS bootstrap.
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

// Runtime switches written from the secured staff CMS.
$runtimeSettings = API . 'runtime-settings.json';
if(is_file($runtimeSettings)) {
    $runtimeData = json_decode((string) file_get_contents($runtimeSettings), true);
    if(is_array($runtimeData) && array_key_exists('maintenance', $runtimeData)) Config::$_MANT = (bool)$runtimeData['maintenance'];
}

// Initialize DB Manager
$DB = new DBManager();
// Initialize Session Manager
$Session = new SessionMG();
require_once 'Modal/AppFunctions.php';
require_once 'Modal/UserSessionMG.class.php';



// Initialize Site Session
$USession = new UserSessionMG($DB, $Session);

// Small public preview used by the login form. It exposes only the avatar
// figure already visible in the hotel, never an account/session detail.
if(isset($_POST['preview_username'])):
    header('Content-Type: application/json; charset=utf-8');
    $previewName = trim((string) $_POST['preview_username']);
    if(!preg_match('/^[A-Za-z0-9]{3,18}$/', $previewName)):
        echo json_encode(array('found' => false));
        exit;
    endif;
    $previewName = mysqli_real_escape_string($DB->Con(), $previewName);
    $previewUser = $DB->Select('users', 'username, look', "username = '" . $previewName . "'");
    echo json_encode(array(
        'found' => $previewUser !== null,
        'username' => $previewUser ? $previewUser['username'] : '',
        'look' => $previewUser ? $previewUser['look'] : ''
    ));
    exit;
endif;

// If Login requested
if(isset($_POST['login_username']) && isset($_POST['login_password'])):
    echo $USession->Login($_POST['login_username'], $_POST['login_password']);
    exit;
endif;

// If Registration requested
if(isset($_POST['reg_username']) && isset($_POST['reg_password']) && isset($_POST['reg_mail'])):
    if(Config::$_MANT):
        echo json_encode(array('type' => 'error', 'text' => 'Les inscriptions sont indisponibles pendant la maintenance.'));
        exit;
    endif;
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

endif;

// During maintenance, the public CMS is unavailable.  Staff accounts keep
// access and the staff-login view remains reachable so they can authenticate.
if(Config::$_MANT):
    $staffAccess = isset($UData) && (int)$UData['rank'] >= 3;
    $staffLoginView = isset($_GET['staff-login']);
    if(!$staffAccess && !$staffLoginView):
        header("Location: " . Config::$URL . "/man.html");
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

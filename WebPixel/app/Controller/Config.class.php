<?php
class Config
{
    public static $V = "2.5.1";

    ## DB Configuration ##
    protected static $DBHOST = "127.0.0.1";
    protected static $DBName = "hv_rp";
    protected static $DBUser = "root";
    protected static $DBPass = '';

    ## Web Configuration ##
    public static $WName = "ParadiseRP";
    public static $URL = "https://paradiserp.fr";
    public static $WEB_DY = "https://paradiserp.fr/Dynamics";
    public static $SessionName = "PZ_lOGIN";
    public static $MaxUsers = 4;
    public static $_MANT = false;
    public static $SWF_MPUS = "https://paradiserp.fr/swf_pz/v5-0-2/MPU/pz_v2_hween/image_loader.php";

    ## SWF Configuration ##
    public static $SWF = "https://paradiserp.fr/swf_pz/v5-0-2/";

    ## Client & Socket Configuration ##
    // Internal server services stay on loopback; browser-facing endpoints use paradiserp.fr.
    public static $TCP = "127.0.0.1";
    public static $TCP_PORT = "2021";
    public static $WS_SERVER = "127.0.0.1";
    public static $WS_PORT = "2087";

    ## External services ##
    public static $SandBox = true;
    public static $S_PAYPAL_API = "";
    public static $S_PAYPAL_SECRET = "";
    public static $PAYPAL_API = "";
    public static $PAYPAL_SECRET = "";

    public static $DiscordInvite = "";

    public static $FB_PAGE_LINK = "";
    public static $FB_API_LINK = "";
    public static $FB_ASSOC_API_LINK = "";
    public static $FB_API_CLIENT = "";
    public static $FB_API_SECRET = "";
    public static $FB_API_REDIRECT = "https://paradiserp.fr/fb_login";
    public static $FB_ASSOC_API_REDIRECT = "https://paradiserp.fr/fb_assoc";
    public static $FB_API_REQUEST = "";
}

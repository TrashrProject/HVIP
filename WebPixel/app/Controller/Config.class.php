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
    public static $WName = "Velora RP";
    public static $URL = "http://localhost/WebPixel";
    public static $WEB_DY = "http://localhost/WebPixel/Dynamics";
    public static $SessionName = "PZ_lOGIN";
    public static $MaxUsers = 4;
    public static $_MANT = false;
    public static $SWF_MPUS = "http://localhost/swf_pz/v5-0-2/MPU/pz_v2_hween/image_loader.php";

    ## SWF Configuration ##
    public static $SWF = "http://localhost/swf_pz/v5-0-2/";

    ## Client & Socket Configuration ##
    public static $TCP = "127.0.0.1";
    public static $TCP_PORT = "2021";
    public static $WS_SERVER = "127.0.0.1";
    public static $WS_PORT = "2087";

    ## External services disabled for localhost ##
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
    public static $FB_API_REDIRECT = "http://localhost/WebPixel/fb_login";
    public static $FB_ASSOC_API_REDIRECT = "http://localhost/WebPixel/fb_assoc";
    public static $FB_API_REQUEST = "";
}

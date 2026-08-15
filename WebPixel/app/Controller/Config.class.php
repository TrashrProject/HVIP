<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class Config
{
    ## RDP Config ##
    public static $V                = "2.5.1"; // Build number

    ## DB Configuration ##
    protected static $DBHOST        = "localhost";
    protected static $DBName        = "hv_rp";
    protected static $DBUser        = "root";
    protected static $DBPass        = 'Hv_oLa.323.';

    ## Web Configuration ##
    public static $WName            = "HabboVIP";
    public static $URL              = "https://habbovip.us";
    public static $WEB_DY           = "https://dynamics.habbovip.us";
    public static $SessionName      = "PZ_lOGIN";
    public static $MaxUsers         = 4;
    public static $_MANT            = false; // Maintenance Variable
    public static $SWF_MPUS         = "../../../../../swf_pz/v5-0-2/MPU/pz_v2_hween/image_loader.php";

    ## SWF Configuration ##
    public static $SWF              = "https://swf.habbovip.us/v5-0-2/";

    ## Client & Socket Configuration ##
    public static $TCP              = "51.222.208.249";
    public static $TCP_PORT         = "2021";
    public static $WS_SERVER        = "ws.habbovip.us";
    public static $WS_PORT          = "2087";

    ## PAYPAL CONFIGS ##
    public static $SandBox              = false;
    public static $S_PAYPAL_API         = "ATm39AHoeLD4B2yf0XJ1_smZtOJcbccbquOT6RF__EWmz9nRFRIbPfnWV3IFg0Xj_CCa01GShFp8xfM4";
    public static $S_PAYPAL_SECRET   = "EAzVlipEYXBNxDdCvUNFkXqs2ORvulYwWz_Z_V0cOx3yX1inpLXI9ZJX3hVsxsLbsRVN7VMSAlLGpa5-";
    public static $PAYPAL_API           = "AVsrP8aqV0-HHVUvbGbzDg_J_Kt2wTBjytdXltOCBo1AnG_2TBuYhYceXs_DFiyNdc0Z4jy-4rs2z4_L";
    public static $PAYPAL_SECRET     = "EPfmozPaHg6_bMssu8StdVbZd3zMIaUEicqGTlTFJKGlObFHlOHmmajcAqfh9iD0CWzdTK_ObF5DX_s5";

    ## DISCORD CONFIG ##
    public static $DiscordInvite        = "https://discord.gg/2cBaSpuwSM";

    ## FB Config ##
    public static $FB_PAGE_LINK        = "https://www.facebook.com/habboviprp";
    public static $FB_API_LINK         = "https://www.facebook.com/v7.0/dialog/oauth?client_id=2991567804406685&redirect_uri=https://habbovip.us/fb_login&auth_type=rerequest&scope=email";
    public static $FB_ASSOC_API_LINK   = "https://www.facebook.com/v7.0/dialog/oauth?client_id=2991567804406685&redirect_uri=https://habbovip.us/fb_assoc&auth_type=rerequest&scope=email";
    public static $FB_API_CLIENT       = "2991567804406685";
    public static $FB_API_SECRET       = "95ef3ebbd29790623874292caa511977";
    public static $FB_API_REDIRECT     = "https://habbovip.us/fb_login";
    public static $FB_ASSOC_API_REDIRECT  = "https://habbovip.us/fb_assoc";
    public static $FB_API_REQUEST      = "";






}
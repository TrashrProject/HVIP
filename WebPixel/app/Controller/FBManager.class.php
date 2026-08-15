<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class FBManager
{
    protected $DB;
    protected $Session;

    // Construct Class
    public function __construct($DB, $Session)
    {
        $this->DB = $DB;
        $this->Session = $Session;
    }

    public function FBRedirect($IP){
        $R = $this->DB->Query("SELECT null FROM fb_data WHERE ip = '". $IP ."' LIMIT 1");
        if(mysqli_num_rows($R) >= 1):
            return true;
        else:
            return false;
        endif;
    }

    public function FBSaveAuth($E, $C, $IP){

        $R = $this->DB->Query("INSERT INTO `fb_data` (`ip`, `email`, `auth_code`, `time`, `blocked`) VALUES ('". $IP ."', '". $E ."', '". AppFunctions::EncryptPass($C) ."', ". time() .", '0');");
        return true;
    }

    public function ExchangeCode($C){
        $ch = curl_init('https://graph.facebook.com/v7.0/oauth/access_token?client_id='. Config::$FB_API_CLIENT .'&redirect_uri='. Config::$FB_API_REDIRECT .'&client_secret='. Config::$FB_API_SECRET .'&code=' . $C . '');
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
        $response = curl_exec($ch);

        return $response;
    }

    public function ExchangeCodeAssoc($C){
        $ch = curl_init('https://graph.facebook.com/v7.0/oauth/access_token?client_id='. Config::$FB_API_CLIENT .'&redirect_uri='. Config::$FB_ASSOC_API_REDIRECT .'&client_secret='. Config::$FB_API_SECRET .'&code=' . $C . '');
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
        $response = curl_exec($ch);

        return $response;
    }

    public function GetFBData($TKN){
        $ch = curl_init('https://graph.facebook.com/me?fields=name,email&access_token='.$TKN.'');
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
        $response = curl_exec($ch);

        return $response;
    }

    // Check if User already Exists with this FB account
    public function FBExists($E, $ID){

        $R = $this->DB->Query("SELECT null FROM users WHERE mail = '". $E ."' AND facebook_id = '". $ID ."' LIMIT 1");
        if(mysqli_num_rows($R) >= 1):
            return true;
        else:
            return false;
        endif;

    }

    //Register User with FB Account
    public function FBRegister($E, $N, $ID){
        $this->Session->FBRegister($E, $N, $ID);
    }

    // Assoc FBAccount to current user
    public function FBAssoc($U, $E, $ID){

        $R = $this->DB->Query("SELECT username, facebook_id, mail FROM users WHERE username = '". $U ."' AND facebook_id IS NOT NULL LIMIT 1");
        $R_ = $this->DB->Query("SELECT username, facebook_id, mail FROM users WHERE facebook_id = '". $ID ."' LIMIT 1");

        if(mysqli_num_rows($R) >= 1):
            $Response['msg'] = 'Tu cuenta ya está asociada a Facebook.';
            $Response['result'] = false;
            return json_encode($Response);
        elseif(mysqli_num_rows($R_) >= 1):
            $Response['msg'] = 'Tu cuenta de Facebook ya está asociada a una cuenta de PZ.';
            $Response['result'] = false;
            return json_encode($Response);
        else:
            $Response['msg'] = '¡Tu cuenta ha sido asociada correctamente! Serás redireccionad@ en breve.';
            $Response['result'] = true;
            $this->DB->Query("UPDATE `users` SET `mail` = '" . $E . "', `facebook_id` = '" . $ID . "' WHERE `username` = '" . $U . "'");
            return json_encode($Response);
        endif;

    }

}
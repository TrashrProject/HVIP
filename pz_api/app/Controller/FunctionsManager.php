<?php
/**
 * This API Back-end system was design and developed for RDP Services.
 * Developed by P3x & Jeihden.
 * All rights reserved PZ API Copyright (c) 2020.
 *
 */

class FunctionsManager
{

    // GetPhoneAPP Function
    public static function GetPhoneAPP($APP_ID, $DB){
        // Verify if APP_ID exists
        $R = $DB->Select("play_phones_apps", "*", "id = " . $APP_ID . "", "LIMIT 1");
        if($R == null):
            self::ThrowE(1200, "Aplicaci&oacute;n solicitada no existe.");
            return;
        endif;

        // Show HTML of APP requested
        echo $R['code'];
        return;
    }

    // GetWebPage Function
    public static function GetWebPage($WEB_URL, $DB){
        // Verify if APP_ID exists
        $R = $DB->Select("play_internet", "*", "url = '" . $WEB_URL . "'", "LIMIT 1");
        if($R == null):
            self::ThrowE(1200, "P&aacute;gina web solicitada no existe.");
            return;
        endif;

        // Show HTML of APP requested
        echo $R['code'];
        return;
    }

    public static function SendPacket($PacketName, $APP_ID, $M, $Params){
        switch ($PacketName) {
            case "PhoneError":
                $M->Ready("event_showphoneerror", $Params);
                break;
            case "GetUserContacts":
                $M->Ready("event_getusercontacts", $Params);
                break;
            case "SendMessage":
                $M->Ready("event_sendmessage", $Params);
                break;
            case "GetMessages":
                $M->Ready("event_getmessages", $Params);
                break;
            case "OpenChat":
                $M->Ready("event_openchat", $Params);
                break;
            case "GetRoomUsers":
                $M->Ready("event_getusersrooms", $Params);
                break;
            case "DiscconectUser":
                $M->Ready("event_disconnectuser", $Params);
                break;
            case "GivePL":
                $M->Ready("event_givepl", $Params);
                break;
            case "DeductPL":
                $M->Ready("event_deductpl", $Params);
                break;
            case "GiveVIP":
                $M->Ready("event_givevip", $Params);
                break;
            case "GiveFurni":
                $M->Ready("event_givefurni", $Params);
                break;
            default:
                self::ThrowE(1200, "La acci&oacute;n solicitada (return) no existe.");
                break;
        }
        return;
    }

    // Throws Error
    public static function ThrowE($N, $MSG){
        
        $E_['Error'] = true;
        $E_['Code'] = $N;
        $E_['Message'] = $MSG;
        echo json_encode($E_);
        return;
    }

    // Filters Input Variables
    public static function FilterVar($D)
    {
        $D = htmlspecialchars($D);
        $D = str_replace("<?php", "", $D);
        $D = str_replace("?>", "", $D);
        $D = str_replace("SELECT", "", $D);
        $D = str_replace("INSERT", "", $D);
        $D = str_replace("UPDATE", "", $D);
        $D = str_replace("DROP", "", $D);
        $D = str_replace("TRUNCATE", "", $D);
        $D = str_replace("CREATE", "", $D);
        $D = str_replace("<script", "", $D);
        $D = str_replace("data:image/jpeg;base64,", "", $D);
        $D = str_replace("data:image/png;base64,", "", $D);

        return $D;
    }
}
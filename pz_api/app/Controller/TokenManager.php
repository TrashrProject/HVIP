<?php
/**
 * This API Back-end system was design and developed for RDP Services.
 * Developed by P3x & Jeihden.
 * All rights reserved PZ API Copyright (c) 2020.
 *
 */

class TokenManager
{
    public $id;
    public $Token;
    public $action;
    public $UID;
    public $APP_ID;
    public $WEB_URL;
    public $E_DATA;
    public $Time;
    public $Used;

    public function Run($T, $DB, $M){
        $R = $DB->Select("api_token", "*", "token = '" . $T . "'", "LIMIT 1");
        if($R != null):
            $this->Used = $R['used'];
            if($this->Used == '0'):
                $this->id = $R['id'];
                $this->Token = $R['token'];
                $this->action = $R['action'];
                $this->UID = $R['user_id'];
                $this->APP_ID = $R['app_id'];
                $this->WEB_URL = $R['web_url'];
                $this->E_DATA = $R['extra_data'];
                $this->Time = $R['time'];
                $this->HandleTkn($DB, $M);

                if($this->action != "API_APP" && $this->action != "API_WEB"):
                    $this->UsedTkn($DB);
                endif;

            else:
                FunctionsManager::ThrowE(1010, "Token no autorizado");
            endif;

        else:
            FunctionsManager::ThrowE(1011, "Token no encontrado");
        endif;
    }

    protected function HandleTkn($DB, $M){
        switch ($this->action) {
            case "GetPhoneAPP":
                // Call Function GetPhoneAPP
                FunctionsManager::GetPhoneAPP($this->APP_ID, $DB);
                break;
            case "GetWebPage":
                // Call Function GetWebPage
                FunctionsManager::GetWebPage($this->WEB_URL, $DB);
                break;
            // MUS Actions
            case "API_APP":
                if(isset($_GET["return"])):
                    $Params = $this->SerializeParams($this->APP_ID);
                    FunctionsManager::SendPacket($_GET["return"], $this->APP_ID, $M, $Params);
                endif;
                break;
            case "GetRoomUsers":
                FunctionsManager::SendPacket("GetRoomUsers", 0, $M, null);
                break;
            case "DiscconectUser":
                FunctionsManager::SendPacket("DiscconectUser", 0, $M, array($this->UID));
                break;
            case "GivePL":
                FunctionsManager::SendPacket("GivePL", 0, $M, array($this->E_DATA . '|' . $this->UID));
                break;
            case "DeductPL":
                FunctionsManager::SendPacket("DeductPL", 0, $M, array($this->E_DATA . '|' . $this->UID));
                break;
            case "GiveVIP":
                FunctionsManager::SendPacket("GiveVIP", 0, $M, array($this->E_DATA . '|' . $this->UID));
                break;
            case "GiveFurni":
                FunctionsManager::SendPacket("GiveFurni", 0, $M, array($this->E_DATA . '|' . $this->UID));
                break;
            default:
                FunctionsManager::ThrowE(1012, "Acci&oacute;n no encontrada");
        }

    }

    protected function UsedTkn($DB) {
        $DB->Update("api_token", "used = '1'", "token = '". $this->Token ."'");
    }

    // Get all params after of tknk & return from url
    protected function GetAllParams() {
        $output = "";
        $firstRun = true; 
        foreach($_GET as $key => $val) { 
            if($key != "tkn" && $key != "return") { 
                if(!$firstRun) { 
                    $output .= "&"; 
                } else { 
                    $firstRun = false; 
                } 
                $output .= $key."=".$val;
            } 
        } 

        return $output;
    }

    protected function SetParamsArray($ID, $P) {
        $A = array($ID);
        $Param = explode("&", $P);
        foreach ($Param as $key => $value) {
            $mP = explode("=", $value);
            $V = $mP[1];
            array_push($A, $V);
        }
        return $A;
    }

    protected function SerializeParams($ID) {
        return $this->SetParamsArray($ID, $this->GetAllParams());
    }
}
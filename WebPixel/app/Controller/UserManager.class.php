<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class UserManager
{
    protected $DB;
    protected $Session;

    public function __construct($DB, $Session)
    {
        // Construct function
        $this->DB = $DB;
        $this->Session = $Session;

    }

    // GetTop3Users
    public function GetTopThree($TopType = "money")
    {
        //Find right type
        if($TopType == "money"):
            $R = $this->DB->Query("SELECT * FROM play_stats, users WHERE play_stats.id = users.id AND rank < 3 AND users.username != 'Tester' AND users.username NOT IN (SELECT value FROM bans WHERE expire >= '".time()."') ORDER BY play_stats.bank + users.credits DESC LIMIT 3");
            if(mysqli_num_rows($R) <= 2):
                return null;
            else:
                return $R;
            endif;
        endif;

    }

    // Generates a Token for client
    public function GenerateAUTH($I){
        $T = AppFunctions::Random(4) . '-' . AppFunctions::Random(4) . '-' . AppFunctions::Random(4) . '-' . AppFunctions::Random(12) . '-RDP';
        if($I !== 0):
            $this->DB->Update("users", "rdpticket = '". $T ."'", "id = ". $I);
            return $T;
        else:
            return "";
        endif;
    }

    // Gets all the money generated in the game
    // GetTop3Users
    public function GetStatData($Data = "money")
    {
        //Find right type
        if($Data == "money"):
            $r = mysqli_fetch_assoc($this->DB->Query("SELECT SUM(p.bank + u.credits) as total FROM play_stats p INNER JOIN users u ON u.id = p.id WHERE u.rank < 2"));

            return number_format($r['total']);
        elseif($Data == "users_registered"):
            $R = mysqli_fetch_assoc($this->DB->Query("SELECT COUNT(*) as Total FROM users"));
            return number_format($R['Total']);
        elseif($Data == "apts_total"):
            $R = mysqli_fetch_assoc($this->DB->Query("SELECT COUNT(*) as Total FROM play_apartments_owned"));
            return number_format($R['Total']);
        elseif($Data == "gangs_total"):
            $R = mysqli_fetch_assoc($this->DB->Query("SELECT COUNT(*) as Total FROM groups WHERE type = '3'"));
            return number_format($R['Total']);
        elseif($Data == "business_count"):
            $R = mysqli_fetch_assoc($this->DB->Query("SELECT COUNT(*) as Total FROM groups WHERE type = '1'"));
            return number_format($R['Total']);
        elseif($Data == "users_online"):
            $R = mysqli_fetch_assoc($this->DB->Query("SELECT COUNT(*) as Total FROM users WHERE online = '1'"));
            return number_format($R['Total']);
        elseif($Data == "users_bans"):

            $R = mysqli_fetch_assoc($this->DB->Query("SELECT COUNT(*) as Total FROM bans WHERE expire >= " . time()));
            return number_format($R['Total']);
        elseif($Data == "edit_version"):
            return "Build #" . Config::$V;
        endif;

    }

    // Gets Stats
    public function GetLeaderBoard($Data, $Column = null){
        if($Data == "richest_user"):
            $R = $this->DB->Query("SELECT users.username, users.look, users.online, users.credits, play_stats.bank, users.id FROM users, play_stats WHERE users.id = play_stats.id AND users.rank < 3 ORDER BY users.credits DESC, play_stats.bank DESC LIMIT 8");
            return $R;
        elseif($Data == "play_stats"):
            $R = $this->DB->Query("SELECT users.username, users.look, users.online, users.credits, play_stats.".  $Column .", users.id FROM users, play_stats WHERE users.id = play_stats.id AND users.rank < 3 ORDER BY play_stats.". $Column ." DESC LIMIT 8");
            return $R;
        elseif($Data == "user_stats"):
            $R = $this->DB->Query("SELECT users.username, users.look, users.online, users.credits, user_stats.".  $Column .", users.id FROM users, user_stats WHERE users.id = user_stats.id AND users.rank < 3 ORDER BY user_stats.". $Column ." DESC LIMIT 8");
            return $R;
        elseif($Data == "richest_pl_user"):
            $R = $this->DB->Query("SELECT users.username, users.look, users.online, users.vip_points, users.id FROM users WHERE users.rank < 3 ORDER BY users.vip_points DESC LIMIT 8");
            return $R;
        endif;
    }

    // Gets Stats
    public function GetLeaderBoardPL(){

        $R = $this->DB->Query("SELECT users.username, users.look, users.online, users.vip_points, users.id FROM users WHERE users.rank < 3 ORDER BY users.vip_points DESC LIMIT 3");
        return $R;

    }

    // Gets Stats
    public function GetStaffs(){
        $R = $this->DB->Query("SELECT username, look, online, rank, id FROM users WHERE rank > 2 AND rank < 7 AND username != 'Jeihden' ORDER BY rank DESC");
        return $R;
    }

    // Gets Onlines
    public function GetOnlines(){
        $R = $this->DB->Query("SELECT username, look, online, rank, id FROM users WHERE online = '1' ORDER BY id ASC");
        return $R;
    }

    // Get Free VIP for user
    public function GiveVIP($UID, $EXP, $URank, $VIP_2 = false){
        // Verify if user is already vip
        if($this->isVIP($UID)):
            return false;
        endif;

        // Assigns VIP type
        $VIP = 1;
        if($VIP_2 == true)
            $VIP = 2;


        // Green light go ahead and insert vip
        $this->DB->Query("INSERT INTO `user_subscriptions`(`user_id`, `rank_vip`, `cost`, `start_date`, `expire_date`) VALUES (". $UID .", '". $VIP ."', 0, ". time() .", ". $EXP .")");
        if($URank > 2):
            $this->DB->Query("UPDATE users SET rank_vip = '2' WHERE id = '".$UID."' LIMIT 1");
        else:
            $this->DB->Query("UPDATE users SET rank = '2', rank_vip = '2' WHERE id = '".$UID."' AND rank <= '2' LIMIT 1");
        endif;
        // VIP given
        return true;

    }

    // is User VIP
    public function isVIP($ID){
        $R = mysqli_num_rows($this->DB->Query("SELECT id, rank_vip FROM users WHERE id = ". $ID ." AND (rank_vip = 1 OR rank_vip = 2) LIMIT 1"));
        return ($R == 1)? true : false;
    }

    public function CheckVIPStatus($UID){
          if($this->isVIP($UID)):
              $R = $this->DB->Query("SELECT * FROM user_subscriptions WHERE user_id = " . $UID . " ORDER BY id DESC LIMIT 1");
              if(mysqli_num_rows($R) == 1):
                  $M = mysqli_fetch_assoc($R);
                  if(time() >=  $M['expire_date']):
                    if($M['rank'] > 2)
                        $this->DB->Query("UPDATE users SET rank_vip = '0' WHERE id = '".$UID."' LIMIT 1");
                    else
                        $this->DB->Query("UPDATE users SET rank = '1', rank_vip = '0' WHERE id = '".$UID."' AND rank <= '2' LIMIT 1");
                  endif;
              endif;
          endif;
    }

    // Gets the users data by id
    public function GetUserDataByID($ID){
        $R = $this->DB->Query("SELECT users.*, play_stats.*, user_stats.OnlineTime, user_stats.Respect FROM users, play_stats, user_stats  WHERE users.id = ". $ID ." AND play_stats.id = ". $ID ." AND user_stats.id = ". $ID ." LIMIT 1");
        $RCount = mysqli_num_rows($R);
        return ($RCount == 1)? mysqli_fetch_assoc($R) : null;
    }

    // IsUserAccount Link To FB
    public function IsLinkedFB($ID){
        $R = $this->DB->Query("SELECT null FROM users  WHERE id = ". $ID ." AND facebook_id IS NOT NULL LIMIT 1");
        $RCount = mysqli_num_rows($R);
        return $RCount == 1;
    }

    // Change Password Function
    public function ChangeOldPass($U, $Old, $N){

        // Does the Password Matches
        $Match = $this->Session->MatchPass($U, $Old);
        if($Match == false):
            $JS['msg'] = "Tu contranseña actual no es correcta.";
            $JS['result'] = false;
            return json_encode($JS);
        endif;

        $NPass = AppFunctions::EncryptPass($N);
        $this->Session->UpdateOldPassword($U, $NPass);
        $JS['msg'] = "Tu contraseña ha sido actualizada correctamente.";
        $JS['result'] = true;
        return json_encode($JS);
    }

    public function GetUserSecondJobLvl($UserId, $ID) {
        $LVL = 0;
        switch ($ID) {
            // Ladrón
            case 0:
                $User_ = $this->GetUserDataByID($UserId);
                $LVL = $User_["LadronLvl"];
                break;
            // Hospital
            case 1:
                break;
            // Basurero
            case 2:
                $User_ = $this->GetUserDataByID($UserId);
                $LVL = $User_["BasuLvl"];
                break;
            // Mecánico
            case 3:
                $User_ = $this->GetUserDataByID($UserId);
                $LVL = $User_["MecLvl"];
                break;
            // Armero
            case 4:
                $User_ = $this->GetUserDataByID($UserId);
                $LVL = $User_["ArmLvl"];
                break;
            // Taxista
            case 5:
                break;
            // Camionero
            case 6:
                $User_ = $this->GetUserDataByID($UserId);
                $LVL = $User_["CamLvl"];
                break;
            // Guardaespaldas
            case 7:
                break;
            // Minero
            case 8:
                $User_ = $this->GetUserDataByID($UserId);
                $LVL = $User_["MinerLvl"];
                break;
            default:
                break;
        }
        return $LVL;
    }

    public function GetUserSecondJobXP($UserId, $ID) {
        $XP = 0;
        switch ($ID) {
            // Ladrón
            case 0:
                $User_ = $this->GetUserDataByID($UserId);
                $XP = $User_["LadronXP"];
                break;
            // Hospital
            case 1:
                break;
            // Basurero
            case 2:
                $User_ = $this->GetUserDataByID($UserId);
                $XP = $User_["BasuXP"];
                break;
            // Mecánico
            case 3:
                $User_ = $this->GetUserDataByID($UserId);
                $XP = $User_["MecXP"];
                break;
            // Armero
            case 4:
                $User_ = $this->GetUserDataByID($UserId);
                $XP = $User_["ArmXP"];
                break;
            // Taxista
            case 5:
                break;
            // Camionero
            case 6:
                $User_ = $this->GetUserDataByID($UserId);
                $XP = $User_["CamXP"];
                break;
            // Guardaespaldas
            case 7:
                break;
            // Minero
            case 8:
                $User_ = $this->GetUserDataByID($UserId);
                $XP = $User_["MinerXP"];
                break;
            default:
                break;
        }
        return $XP;
    }

    public function BuyVIP($VipType, $UserId){
        $User_ = $this->GetUserDataByID($UserId);
        $VT = AppFunctions::GeneralClean($VipType);
        
        $Cost = 5;
        if($VT > 1)
            $Cost = 10;

        if($VT <= 0) {
            $Response['title'] = "¡Oh no!";
            $Response['type'] = "error";
            $Response['text'] = 'Esa membres&iacute;a no es v&aacute;lida.';
        }
        else if($User_['online'] > 0) {
            $Response['title'] = "¡Oh no!";
            $Response['type'] = "error";
            $Response['text'] = '¡Debes estar desconectado del Client para poder comprar en la Tienda!';
        }
        else if($User_['rank_vip'] > 0) {
            $Response['title'] = "¡Oh no!";
            $Response['type'] = "error";
            $Response['text'] = '¡Ya cuentas con una membres&iacute;a VIP activa! Espera a que termine para renovarla o mejorarla.';
        }
        else if($User_['vip_points'] < $Cost) {
            $Response['title'] = "¡Oh no!";
            $Response['type'] = "error";
            $Response['text'] = 'No cuentas con los Platinos suficientes para comprar VIP' . $VT;
        }
        else {
            // Compramos
            $Exp = time() + 2678400; // 31 días
            $SQL = $this->DB->Query("INSERT INTO user_subscriptions (user_id, rank_vip, cost, start_date, expire_date) VALUES ('".$User_['id']."', '".$VT."', '".$Cost."', '".time()."', '".$Exp."');");

            if($SQL) {
                if($User_['rank'] < 3) {
                    $SQL = $this->DB->Query("UPDATE users SET vip_points = vip_points-'".$Cost."', rank_vip = '".$VT."', rank = '2' WHERE id = '".$User_['id']."' LIMIT 1");
                }
                else
                {
                    $SQL = $this->DB->Query("UPDATE users SET vip_points = vip_points-'".$Cost."', rank_vip = '".$VT."' WHERE id = '".$User_['id']."' LIMIT 1");
                }

                if($SQL) {
                    $Response['title'] = "¡Genial!";
                    $Response['type'] = "success";
                    $Response['text'] = 'Has comprado VIP'.$VT.' satisfactoriamente. Fecha de expiraci&oacute;n el ' . $this->DateFormat_NoT($Exp);
                } else {
                    $Response['title'] = "¡Oh no!";
                    $Response['type'] = "error";
                    $Response['text'] = 'Ocurri&oacute; un problema al asignarte el rango VIP. Contacta con un Administrador.';
                }
            } else {
                $Response['title'] = "¡Oh no!";
                $Response['type'] = "error";
                $Response['text'] = 'Ocurri&oacute; un problema al registrar tu suscripci&oacute;n. Contacta con un Administrador.';
            }
        }
        return json_encode($Response);
    }

    public static function DateFormat_NoT($timestamp){
        $d = date('d \d\e F \d\e Y', $timestamp);

        $d = str_replace("January", "Enero", $d);
        $d = str_replace("February", "Febrero", $d);
        $d = str_replace("March", "Marzo", $d);
        $d = str_replace("April", "Abril", $d);
        $d = str_replace("May", "Mayo", $d);
        $d = str_replace("June", "Junio", $d);
        $d = str_replace("July", "Julio", $d);
        $d = str_replace("August", "Agosto", $d);
        $d = str_replace("September", "Septiembre", $d);
        $d = str_replace("October", "Octubre", $d);
        $d = str_replace("November", "Noviembre", $d);
        $d = str_replace("December", "Diciembre", $d);

        return $d;
    }

    // Generates a custom machineid for Nitro uses.
    public function GenerateMachineId($I){
        $this->DB->Update("users", "machine_id = '". uniqid() ."'", "id = ". $I);
    }

    public function GetWeaponList(){

        $R = $this->DB->Query("SELECT * FROM play_weapons");
        return $R;
    }
}
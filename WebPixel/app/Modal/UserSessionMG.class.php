<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class UserSessionMG
{
    protected $DB;
    protected $Session;

    // Construct Class and assign variables required
    public function __construct($DB, $Session){
        $this->DB = $DB;
        $this->Session = $Session;
    }

    // Login function for users
    public function Login($U, $P){
        $U = ucwords(strtolower(AppFunctions::GeneralClean($U)));
        $P = AppFunctions::GeneralClean($P);

        $Ban = $this->BanValidation("ip", AppFunctions::GetIP());
        if($Ban['banned']):
            $Response['msg'] = 'Votre compte est banni jusqu’au ('. date('d/m/Y', $Ban['exp']) .') pour le motif suivant : ('. $Ban['reason'] .').';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        $Ban = $this->BanValidation("user", $U);
        if($Ban['banned']):
            $Response['msg'] = 'Votre compte est banni jusqu’au ('. date('d/m/Y', $Ban['exp']) .') pour le motif suivant : ('. $Ban['reason'] .').';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;
        
        if($this->MatchPass($U, $P)):
            // The password is valid at this point. During maintenance we do
            // not create a session for regular players: only staff can work
            // on the hotel through the CMS.
            $Account = $this->DB->Select("users", "rank", "username = '" . $U . "'");
            if(Config::$_MANT && (!$Account || (int)$Account['rank'] < 3)):
                $Response['msg'] = 'L\'hôtel est actuellement en maintenance. Seuls les membres du staff peuvent se connecter.';
                $Response['result'] = false;
                $Response['maintenance'] = true;
                return json_encode($Response);
            endif;

            // Update last ip
            $this->DB->Update("users", "ip_last = '". AppFunctions::GetIP() ."'", "username = '". $U ."'");
            $this->Session->Save(Config::$SessionName, $U);
            $Response['msg'] = "Connexion réussie, vous allez être redirigé dans un instant.";
            $Response['result'] = true;
            return json_encode($Response);
        else:
            // Failed login
            $Response['msg'] = 'Une erreur est survenue : vérifie ton pseudo et ton mot de passe.';
            $Response['result'] = false;
            return json_encode($Response);
        endif;
    }

    // Registration function for users
    public function Registration($U, $P, $E){

        $U = ucwords(strtolower(AppFunctions::GeneralClean($U)));
        $P = AppFunctions::GeneralClean($P);
        $E = AppFunctions::GeneralClean($E);

        $Ban = $this->BanValidation("ip", AppFunctions::GetIP());

        if($Ban['banned']):
            $Response['msg'] = 'Votre compte est banni jusqu’au ('. date('d/m/Y', $Ban['exp']) .') pour le motif suivant : ('. $Ban['reason'] .').';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        // Checks name validation
        if(strlen($U) < 3):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo doit contenir au moins 3 caractères.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(strlen($U) > 18):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne doit pas dépasser 18 caractères.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(strlen($U) != strlen(str_replace(' ', '', $U))):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne peut pas contenir d’espace.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(!AppFunctions::OnlyLetters($U)):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne peut contenir que des lettres et des chiffres.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        // Email Validation
      /*  if(!filter_var($E, FILTER_VALIDATE_EMAIL)):
            // Message for Email failed validation
            $Response['msg'] = 'L’adresse e-mail renseignée n’est pas valide.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;*/

        // Password Validation
        if(strlen($P) < 6):
            // Message for Email failed validation
            $Response['msg'] = 'Le mot de passe doit contenir au moins 6 caractères.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;


        // Checks if Email or User has been taken
        $Exists = $this->ValidateRegisteredData($U, $E);
        if($Exists['result'] == false):
            if($Exists['type'] == "username"):
                // Message for username taken
                $Response['msg'] = 'Ce pseudo est déjà utilisé.';
                $Response['result'] = false;
                return json_encode($Response);
                exit;
             /*else:
                // Message for email taken
                $Response['msg'] = 'Cette adresse e-mail est déjà utilisée.';
                $Response['result'] = false;
                return json_encode($Response);
                exit;*/
            endif;
        endif;

        // Checks for the Maximum accounts per IP
        $UsersCount = $this->DB->Count("users", "null", "ip_reg = '" . AppFunctions::GetIP() . "'");
        if($UsersCount >= Config::$MaxUsers):
            // Message for email taken
            $Response['msg'] = 'Tu as atteint la limite de comptes autorisés. Connecte-toi à un compte existant.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        // Encrypt password
        $P = AppFunctions::EncryptPass($P);

        // Insert User in Database
        //Keys
        $PA["username"] = "username";
        $PA["password"] = "password";
        //$PA["mail"]    = "mail";
        $PA["ip_register"] = "ip_reg";
        $PA["ip_last"] = "ip_last";
        $PA["account_created"] = "account_created";

        // Values
        $VA["username"] = "'" . $U .  "'";
        $VA["password"] = "'" . $P . "'";
       // $VA["mail"]    = "'" . $E . "'";
        $VA["ip_register"] = "'" . AppFunctions::GetIP() . "'";
        $VA["ip_last"] = "'" . AppFunctions::GetIP() . "'";
        $VA["account_created"] = "" . time() . "";

        // Insert new user to our records
        $this->DB->Insert("users", $PA, $VA);

        // Select users Data
        $Row = $this->DB->Select("users", "id", "username = '". $U ."'");

        // Insert PlayStats for new user
        if($Row == null):
            $Response['msg'] = "La création du compte a échoué. Réessaie dans quelques instants.";
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        $PA_["id"] = "id";
        $VA_["id"] = "'". $Row['id'] ."'";
        $this->DB->Insert("play_stats", $PA_, $VA_);
        $this->DB->Insert("user_stats", $PA_, $VA_);

        $this->Session->Save(Config::$SessionName, $U);

        // Idea, se pueden mandar MUS de feed para avisar que alguein nuevo se ha registrado

        $Response['msg'] = "Bienvenue sur ParadiseRP ! Tu vas être redirigé(e) dans 3 secondes.";
        $Response['result'] = true;
        return json_encode($Response);
        exit;
    }

    public function ValidateUserName($U){
        // Checks name validation
        if(strlen($U) < 3):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo doit contenir au moins 3 caractères.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(strlen($U) > 18):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne doit pas dépasser 18 caractères.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(strlen($U) != strlen(str_replace(' ', '', $U))):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne peut pas contenir d’espace.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(!AppFunctions::OnlyLetters($U)):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne peut contenir que des lettres et des chiffres.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif($this->UsernameExists($U)):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo "' . $U .  '" est déjà utilisé.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        else:
            // Message for username success validation
            $Response['msg'] = 'Ce pseudo est disponible.';
            $Response['result'] = true;
            return json_encode($Response);
        endif;
    }

    public function ValidateRegisteredData($U, $E){

        if($this->DB->Count("users", "id", "username = '" . $U . "'") >= 1):
            $R["result"] = false;
            $R['type'] = "username";
            return $R;
        elseif($this->DB->Count("users", "id", "mail = '" . $E . "'") >= 1):
            $R["result"] = false;
            $R['type'] = "email";
            return $R;
        else:
            $R["result"] = true;
            return $R;
        endif;
    }

    // Username Taken Validation
    public function UsernameExists($U){
        if($this->DB->Count("users", "null", "username = '" . $U . "'") >= 1):
           return true;
        else:
            return false;
        endif;
    }

    // Mach old or new password encryption
    public function MatchPass($U, $P){

        $PassOLD = AppFunctions::TryOldmatch($U, $P);
        $Pass = AppFunctions::EncryptPass($P);

        if($this->DB->Count("users", "null", "username = '" . $U . "' AND password = '" . $PassOLD . "'")):
            $NPass = AppFunctions::EncryptPass($P);
            $this->UpdateOldPassword($U, $NPass);
            return true;
        elseif($this->DB->Count("users", "null", "username = '" . $U . "' AND password = '" . $Pass . "'")):
            return true;
        else:
            return false;
        endif;
    }

    public function UpdateOldPassword($U, $P){
        return $this->DB->Update("users", "password = '". $P ."'", "username = '". $U ."'");
    }

    // FB Login
    public function FBLogin($E, $ID){
        $Row = $this->DB->Select("users", "username", "mail = '". $E ."' AND facebook_id = '" . $ID . "'");

        $Ban = $this->BanValidation("user", $Row['username'] );
        if($Ban['banned']):
            return false;
            exit;
        endif;

        $Ban = $this->BanValidation("ip", AppFunctions::GetIP());
        if($Ban['banned']):
            return false;
            exit;
        endif;

        $this->DB->Update("users", "ip_last = '". AppFunctions::GetIP() ."'", "username = '". $Row['username'] ."'");
        $this->Session->Save(Config::$SessionName, $Row['username']);
        return true;
    }

    // FB Registration
    public function FBRegister($E, $N, $ID, $U){

        // Variables Cleaning
        $U = ucwords(strtolower(AppFunctions::GeneralClean($U)));
        $E = AppFunctions::GeneralClean($E);
        $ID = AppFunctions::GeneralClean($ID);
        $N = AppFunctions::GeneralClean($N);

        // Checks for the Maximum accounts per IP
        $UsersCount = $this->DB->Count("users", "null", "ip_reg = '" . AppFunctions::GetIP() . "'");
        if($UsersCount >= Config::$MaxUsers):
            $Response['msg'] = 'Tu as déjà le nombre maximum de comptes autorisés.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        $Ban = $this->BanValidation("ip", AppFunctions::GetIP());
        if($Ban['banned']):
            $Response['msg'] = 'Votre compte est banni jusqu’au ('. date('d/m/Y', $Ban['exp']) .') pour le motif suivant : ('. $Ban['reason'] .').';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        if(strlen($U) != strlen(str_replace(' ', '', $U))):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne peut pas contenir d’espace.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        elseif(!AppFunctions::OnlyLetters($U)):
            // Message for username failed validation
            $Response['msg'] = 'Le pseudo ne peut contenir que des lettres et des chiffres.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        //If Username already exists
        if($this->UsernameExists($U)):
            $Response['msg'] = 'Ce pseudo est déjà utilisé.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        // Email Validation
        if(!filter_var($E, FILTER_VALIDATE_EMAIL)):
            // Message for Email failed validation
            $Response['msg'] = 'L’adresse e-mail renseignée n’est pas valide.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        // Insert User in Database
        //Keys
        $PA["username"] = "username";
        $PA["password"] = "password";
        $PA["mail"]    = "mail";
        $PA["ip_register"] = "ip_reg";
        $PA["ip_last"] = "ip_last";
        $PA["account_created"] = "account_created";
        $PA["facebook_id"] = "facebook_id";
        $PA["facebook_change_name"] = "facebook_change_name";

        // Values
        $VA["username"] = "'" . $U .  "'";
        $VA["password"] = "'facebook'";
        $VA["mail"]    = "'" . $E . "'";
        $VA["ip_register"] = "'" . AppFunctions::GetIP() . "'";
        $VA["ip_last"] = "'" . AppFunctions::GetIP() . "'";
        $VA["account_created"] = "" . time() . "";
        $VA["facebook_id"]  = "'" . $ID . "'";
        $VA["facebook_change_name"] = "'1'";


        // Insert new user to our records
        $this->DB->Insert("users", $PA, $VA);

        // Select users Data
        $Row = $this->DB->Select("users", "id", "username = '". $U ."'");

        // Insert PlayStats for new user
        if($Row == null):
            $Response['msg'] = "La création du compte a échoué. Réessaie dans quelques instants.";
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

        $PA_["id"] = "id";
        $VA_["id"] = "'". $Row['id'] ."'";
        $this->DB->Insert("play_stats", $PA_, $VA_);


        $this->Session->Save(Config::$SessionName, $U);

        // Idea, se pueden mandar MUS de feed para avisar que alguein nuevo se ha registrado

        $Response['msg'] = "Bienvenue sur ParadiseRP ! Tu vas être redirigé(e) dans une seconde.";
        $Response['result'] = true;
        return json_encode($Response);
        exit;

    }

    public function BanValidation($T, $Data){

        $R = $this->DB->Query("SELECT * FROM bans WHERE bantype = '". $T ."' AND value = '". $Data ."'");
        if(mysqli_num_rows($R) >= 1):
            $Ban = mysqli_fetch_assoc($R);
            if($Ban['expire'] >= time()):
                $R_['banned'] = true;
                $R_['exp'] = $Ban['expire'];
                $R_['reason'] = $Ban['reason'];
                return $R_;
            else:
                $R_['banned'] = false;
                return $R_;
            endif;
        endif;
        $R_['banned'] = false;
        return $R_;

    }
}

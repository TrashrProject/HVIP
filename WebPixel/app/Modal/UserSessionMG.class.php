<?php
/**
 * ParadiseRP / WavePlus user session and authentication manager.
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
            session_regenerate_id(true);
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
        if(!filter_var($E, FILTER_VALIDATE_EMAIL)):
            // Message for Email failed validation
            $Response['msg'] = 'L’adresse e-mail renseignée n’est pas valide.';
            $Response['result'] = false;
            return json_encode($Response);
            exit;
        endif;

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
        $P = password_hash($P, PASSWORD_DEFAULT);

        // Insert User in Database
        //Keys
        $PA["username"] = "username";
        $PA["password"] = "password";
        $PA["mail"]    = "mail";
        $PA["ip_register"] = "ip_reg";
        $PA["ip_last"] = "ip_last";
        $PA["account_created"] = "account_created";
        $PA["look"] = "look";
        $PA["motto"] = "motto";
        $PA["home_room_data"] = "home_room_data";

        // Values
        $VA["username"] = "'" . $U .  "'";
        $VA["password"] = "'" . $P . "'";
        $VA["mail"]    = "'" . mysqli_real_escape_string($this->DB->Con(), $E) . "'";
        $VA["ip_register"] = "'" . AppFunctions::GetIP() . "'";
        $VA["ip_last"] = "'" . AppFunctions::GetIP() . "'";
        $VA["account_created"] = "" . time() . "";
        $VA["look"] = "'hr-100-61.hd-180-1.ch-210-66.lg-270-82.sh-290-80'";
        $VA["motto"] = "'Bienvenue sur ParadiseRP !'";
        $VA["home_room_data"] = "'{\"roomid\":3,\"x\":18,\"y\":22,\"z\":0,\"rotation\":4,\"weapon\":0}'";

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

        session_regenerate_id(true);
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
        $stmt = mysqli_prepare($this->DB->Con(), "SELECT password FROM users WHERE username = ? LIMIT 1");
        mysqli_stmt_bind_param($stmt, 's', $U);
        mysqli_stmt_execute($stmt);
        $row = mysqli_fetch_assoc(mysqli_stmt_get_result($stmt));
        mysqli_stmt_close($stmt);
        if(!$row) return false;
        $stored = (string)$row['password'];
        $isModern = password_get_info($stored)['algo'] !== null;
        $valid = $isModern ? password_verify($P, $stored) : (hash_equals($stored, AppFunctions::EncryptPass($P)) || hash_equals($stored, AppFunctions::TryOldmatch($U, $P)));
        if($valid && !$isModern) $this->UpdateOldPassword($U, password_hash($P, PASSWORD_DEFAULT));
        return $valid;
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
        $PA["look"] = "look";
        $PA["motto"] = "motto";
        $PA["home_room_data"] = "home_room_data";

        // Values
        $VA["username"] = "'" . $U .  "'";
        $VA["password"] = "'facebook'";
        $VA["mail"]    = "'" . $E . "'";
        $VA["ip_register"] = "'" . AppFunctions::GetIP() . "'";
        $VA["ip_last"] = "'" . AppFunctions::GetIP() . "'";
        $VA["account_created"] = "" . time() . "";
        $VA["facebook_id"]  = "'" . $ID . "'";
        $VA["facebook_change_name"] = "'1'";
        $VA["look"] = "'hr-100-61.hd-180-1.ch-210-66.lg-270-82.sh-290-80'";
        $VA["motto"] = "'Bienvenue sur ParadiseRP !'";
        $VA["home_room_data"] = "'{\"roomid\":3,\"x\":18,\"y\":22,\"z\":0,\"rotation\":4,\"weapon\":0}'";


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
        if($T === 'user') $T = 'account';
        $now = time();
        if($T === 'ip'):
            $stmt = mysqli_prepare($this->DB->Con(), "SELECT b.*, b.ban_expire AS expire, b.ban_reason AS reason FROM bans b WHERE b.type IN ('ip', 'super') AND b.ip = ? AND (b.ban_expire = 0 OR b.ban_expire >= ?) LIMIT 1");
        else:
            $stmt = mysqli_prepare($this->DB->Con(), "SELECT b.*, b.ban_expire AS expire, b.ban_reason AS reason FROM bans b INNER JOIN users u ON u.id = b.user_id WHERE b.type IN ('account', 'super') AND u.username = ? AND (b.ban_expire = 0 OR b.ban_expire >= ?) LIMIT 1");
        endif;
        if(!$stmt):
            return ['banned' => false];
        endif;
        mysqli_stmt_bind_param($stmt, 'si', $Data, $now);
        mysqli_stmt_execute($stmt);
        $R = mysqli_stmt_get_result($stmt);
        if(mysqli_num_rows($R) >= 1):
            $Ban = mysqli_fetch_assoc($R);
            if((int)$Ban['expire'] === 0 || $Ban['expire'] >= time()):
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

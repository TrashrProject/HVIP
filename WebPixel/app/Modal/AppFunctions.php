<?php
/**
 * ParadiseRP / WavePlus application helpers.
 */

class AppFunctions
{
    protected static $Key = "PZ_EN_CRYP_2020";
    protected static $Cipher = "AES-128-CTR";
    protected static $options = 0;
    protected static $encryption_iv = '1234567891011121';

    public static function GetIP()
    {
        if (isset($_SERVER["HTTP_CF_CONNECTING_IP"])) {
            $_SERVER['REMOTE_ADDR'] = $_SERVER["HTTP_CF_CONNECTING_IP"];
            $_SERVER['HTTP_CLIENT_IP'] = $_SERVER["HTTP_CF_CONNECTING_IP"];
        }
        $client = @$_SERVER['HTTP_CLIENT_IP'];
        $forward = @$_SERVER['HTTP_X_FORWARDED_FOR'];
        $remote = $_SERVER['REMOTE_ADDR'];

        if (filter_var($client, FILTER_VALIDATE_IP)) {
            return $client;
        } elseif (filter_var($forward, FILTER_VALIDATE_IP)) {
            return $forward;
        } else {
            return $remote;
        }
    }

    // Remove injections from inputs
    public static function InjectionCleaner($var)
    {
        $var = htmlspecialchars($var);
        $var = str_replace("<?php", "", $var);
        $var = str_replace("?>", "", $var);
        $var = str_replace("SELECT", "", $var);
        $var = str_replace("INSERT", "", $var);
        $var = str_replace("UPDATE", "", $var);
        $var = str_replace("DROP", "", $var);
        $var = str_replace("TRUNCATE", "", $var);
        $var = str_replace("CREATE", "", $var);
        $var = str_replace("<script", "", $var);
        $var = str_replace("data:image/jpeg;base64,", "", $var);
        $var = str_replace("data:image/png;base64,", "", $var);

        return $var;
    }

    //Remove unauthorized names from input
    public static function UNameCleaner($var)
    {
        $var = htmlspecialchars($var);
        $var = str_replace("MOD", "", $var);
        $var = str_replace("mod", "", $var);
        $var = str_replace("STAFF", "", $var);
        $var = str_replace("staff", "", $var);
        $var = str_replace("ADMIN", "", $var);
        $var = str_replace("admin", "", $var);
        return $var;
    }

    // Clean inputs in spanish from forms
    public static function AcentsCleaner($var)
    {
        $var = htmlspecialchars($var);
        $var = str_replace("'", "-", $var);
        $var = str_replace("á", "a", $var);
        $var = str_replace("í", "i", $var);
        $var = str_replace("é", "e", $var);
        $var = str_replace("ú", "u", $var);
        $var = str_replace("ó", "o", $var);
        return $var;
    }

    public static function GeneralClean($var){

        $var = htmlspecialchars($var);
        $var = str_replace("'", "-", $var);
        $var = str_replace("á", "a", $var);
        $var = str_replace("í", "i", $var);
        $var = str_replace("é", "e", $var);
        $var = str_replace("ú", "u", $var);
        $var = str_replace("ó", "o", $var);
        $var = str_replace("MOD", "", $var);
        $var = str_replace("mod", "", $var);
        $var = str_replace("STAFF", "", $var);
        $var = str_replace("staff", "", $var);
        $var = str_replace("ADMIN", "", $var);
        $var = str_replace("admin", "", $var);
        $var = str_replace("<?php", "", $var);
        $var = str_replace("?>", "", $var);
        $var = str_replace("SELECT", "", $var);
        $var = str_replace("INSERT", "", $var);
        $var = str_replace("UPDATE", "", $var);
        $var = str_replace("DROP", "", $var);
        $var = str_replace("TRUNCATE", "", $var);
        $var = str_replace("CREATE", "", $var);
        $var = str_replace("<script", "", $var);
        $var = str_replace("data:image/jpeg;base64,", "", $var);
        $var = str_replace("data:image/png;base64,", "", $var);
        return $var;

    }

    public static function OnlyLetters($in){
        if(preg_match("/^[A-Za-z0-9]*$/",$in))
            return true;
        else
            return false;
    }

    // Get Random
    public static function Random($l, $n = true, $L = true, $o = ''){

        $c = '';
        $c .= ($n) ? '0123456789' : '';
        $c .= ($L) ? 'QWERTYUIOPASDFGHJKLLZXCVBNMqwertyuiopasdfghjklzxcvbnm' : '';
        $c .= $o;

        $s = '';
        $C = 0;
        while ($C < $l){
            $s .= substr($c, rand(0, strlen($c) -1), 1);
            $C++;
        }

        return $s;

    }



    // Password Encryption
    public static function EncryptPass($P){
        //$iv_length = openssl_cipher_iv_length(self::$Cipher);
        $String = "-pz-2020-" . $P . "-pz-2020-";

        return openssl_encrypt($String, self::$Cipher,
            self::$Key, self::$options, self::$encryption_iv);
    }

    // Password Decrypt
    public static function DecryptPass($S){
        // Use openssl_decrypt() function to decrypt the data
        return openssl_decrypt ($S, self::$Cipher,
            self::$Key, self::$options, self::$encryption_iv);
    }

    // Old encrypt match
    public static function TryOldmatch($user, $pass)
    {
        $encript = $user.$pass.'pixelzone';
        $special = htmlspecialchars($encript);
        $base = base64_encode($special);
        $entities = htmlentities($base);
        $md = md5($entities);
        $sha = sha1($md);
        $asl = addslashes($sha);
        $convert = convert_uuencode($asl);
        return $convert;
    }

    #Función para convertir la fecha (de codigo) a tiempo actual hablado
    public static function GetTime($valor)
    {
        // FORMATOS:
        // segundos    desde 1970 (función time())        hace_tiempo('12313214');
        // defecto (variable $formato_defecto)        hace_tiempo('12:01:02 04-12-1999');
        // tu propio formato                        hace_tiempo('04-12-1999 12:01:02 [n.j.Y H:i:s]');

        $formato_defecto="H:i:s j-n-Y";

        // j,d = día
        // n,m = mes
        // Y = año
        // G,H = hora
        // i = minutos
        // s = segundos

        if(stristr($valor,'-') || stristr($valor,':') || stristr($valor,'.') || stristr($valor,','))
        {

            if(stristr($valor,'['))
            {
                $explotar_valor=explode('[',$valor);
                $valor=trim($explotar_valor[0]);
                $formato=str_replace(']','',$explotar_valor[1]);
            }
            else
            {
                $formato=$formato_defecto;
            }

            $valor = str_replace("-"," ",$valor);
            $valor = str_replace(":"," ",$valor);
            $valor = str_replace("."," ",$valor);
            $valor = str_replace(","," ",$valor);

            $numero = explode(" ",$valor);

            $formato = str_replace("-"," ",$formato);
            $formato = str_replace(":"," ",$formato);
            $formato = str_replace("."," ",$formato);
            $formato = str_replace(","," ",$formato);

            $formato = str_replace("d","j",$formato);
            $formato = str_replace("m","n",$formato);
            $formato = str_replace("G","H",$formato);

            $letra = explode(" ",$formato);

            $relacion[$letra[0]]=$numero[0];
            $relacion[$letra[1]]=$numero[1];
            $relacion[$letra[2]]=$numero[2];
            $relacion[$letra[3]]=$numero[3];
            $relacion[$letra[4]]=$numero[4];
            $relacion[$letra[5]]=$numero[5];

            $valor = mktime($relacion['H'],$relacion['i'],$relacion['s'],$relacion['n'],$relacion['j'],$relacion['Y']);

        }

        $ht = time()-$valor;
        if($ht>=2116800)
        {
            $dia = date('d',$valor);
            $mes = date('n',$valor);
            $año = date('Y',$valor);
            $hora = date('H',$valor);
            $minuto = date('i',$valor);
            $mesarray = array('','Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre');
            $fecha = "El $dia de $mesarray[$mes] del $año";
        }
        if($ht<30242054.045)
        {
            $hc=round($ht/2629743.83);if($hc>1){$s="es";}else{$s = "";}$fecha="Hace $hc mes".$s;
        }
        if($ht<2116800)
        {
            $hc=round($ht/604800);if($hc>1){$s="s";}else{$s = "";}$fecha="Hace $hc semana".$s;
        }
        if($ht<561600)
        {
            $hc=round($ht/86400);if($hc==1){$fecha="Ayer";}if($hc==2){$fecha="Antier";}if($hc>2)$fecha="Hace $hc d&iacute;as";
        }
        if($ht<84600)
        {
            $hc=round($ht/3600);if($hc>1){$s="s";}else{$s = "";}$fecha="Hace $hc hora".$s;if($ht>4200 && $ht<5400){$fecha="Hace m&aacute;s de una hora";}}
        if($ht<3570)
        {
            $hc=round($ht/60);if($hc>1){$s="s";}else{$s = "";}$fecha="Hace $hc minuto".$s;
        }
        if($ht<60)
        {
            $fecha="Hace $ht segundos";
        }
        if($ht<=3)
        {
            $fecha="Ahora mismo";
        }

        return $fecha;
    }
}
<?php
/**
 * This API Back-end system was design and developed for RDP Services.
 * Developed by P3x & Jeihden.
 * All rights reserved PZ API Copyright (c) 2020.
 *
 */

class MusManager
{
    // Config MUS
    protected static $H = "51.222.208.249";      // Host IP
    protected static $P = "3043";               // Host Port

    // DO NOT TOUCH DYNAMIC VARIABLES
    protected $Host;      // Host IP
    protected $Port;      // Host Port

    ## SENDING PACKAGES FUNCTIONS ##
    // Connects to Socket
    protected function Con(){

        $this->Host = self::$H;       // IP
        $this->Port = self::$P;       // Port

        $S = socket_create(AF_INET, SOCK_STREAM, SOL_TCP) or die("Could not create socket\n");
        $R = socket_connect($S, $this->Host, $this->Port) or die("Could not connect to server2\n");

        return $S;
    }

    // Set The Package ready
    public function Ready($C, $P = null){
        // Start Building the String Socket to Send

        // Command is added to the string
        $S = $C . ":";

        // If Params aren't null we add params to the string
        if($P != null):
            $Count = 0;
            // Foreach to get all the params required
            foreach ($P as $K => $V):
                if($Count >= 1):
                    $S .= "|". $V;
                else:
                    $S .= $V;
                endif;
                $Count++;
            endforeach;
        endif;
        
        // Once String is completed send the package
        $this->Send($S);
    }

    // Sends the Package with Socket
    public function Send($S, $return = false){
        $Socket = $this->Con();

        socket_write($Socket, $S, strlen($S)) or die("Could not send data to server\n");
        $R = null;
        while ($out = socket_read($Socket, 8192)) {
            $P = explode(":", $out);
            $PacketName = $P[0];
            $R = str_replace($PacketName . ":", "", $out);
            echo str_replace($PacketName . ":", "", $out);
        }

        socket_close($Socket);

        return ($return)? $R : true;
    }
}
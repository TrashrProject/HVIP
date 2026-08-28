<?php
/**
 * ParadiseRP / WavePlus user model.
 */

class User
{
    // Variables
    public $UData;
    public $UPData;
    public $Session;
    public $DB;


    // Construct User
    public function __construct($DB, $S)
    {
        // Require Params DB & Session to handle user
        $this->Session = $S;
        $this->DB = $DB;

        // Read User's Session
        $UName = $this->Session->Read(Config::$SessionName);

        if ($UName == null):
            return;
        endif;

        // Get User's data base from users
        $this->UData = $this->getUData($UName);

        if($this->UData == null):
            return;
        endif;

        // Get User's play_stats from db
        $this->UPData = $this->getUPData($this->UData['id']);

        if($this->UPData == null):
            return;
        endif;

    }

    // Gets data from users
    public function getUData($U){
        $Q = $this->DB->Select("users", "*", "username = '". $U ."'");
        if($Q == null):
            return null;
        else:
            return $Q;
        endif;

    }

    //Gets Data from play_stats
    public function getUPData($ID){
        $Q = $this->DB->Select("play_stats", "*", "id = ". $ID ."");
        if($Q == null):
            return null;
        else:
            return $Q;
        endif;
    }

}
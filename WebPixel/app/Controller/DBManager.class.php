<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class DBConManager extends Config
{

    protected $host;   // DB Host
    protected $user;   // DB User
    protected $pass;   // DB Pass
    protected $db;     // DB Name
    protected $pz_conn = null; // Connection Variable

    function connect() {
        // Assign Config to variables

        $this->host = self::$DBHOST;
        $this->user = self::$DBUser;
        $this->pass = self::$DBPass;
        $this->db = self::$DBName;

        $con = mysqli_connect($this->host.":3306", $this->user, $this->pass, $this->db);
        if (!$con):
            die('Could not connect to database!');
        else:
            $this->pz_conn = $con;
        endif;
        return $this->pz_conn;
    }

    function close() {
        mysqli_close($this->pz_conn);
        echo 'Connection closed!';
    }
}

class DBManager extends DBConManager {

    public function Con(){
        $Con = new DBConManager();
        return $Con->connect();
    }
    // Query Function
    public function Query($Q) {

        // Execute Query and return result
        return mysqli_query($this->Con(), $Q);

    }
    // Select Function
    public function Select($T, $C = '*', $W = null, $E = null, $NoFetch = false) {
        // Query String
        $Q = 'SELECT '. $C .' FROM  '. $T;

        // Where Params
        if($W != null):
            $Q .= ' WHERE '. $W;
        endif;

        // Extras Params
        if($E != null):
            $Q .= ' '. $E;
        endif;

        // Execute Query
        $R = mysqli_query($this->Con(), $Q);

        // Verify result
        if(mysqli_num_rows($R) > 0):
            // Return Result
            if($NoFetch):
                return $R;
            else:
                return mysqli_fetch_assoc($R);
            endif;
        else:
            // Return null
            return null;
        endif;
    }

    // Count Function
    public function Count($T, $C = '*', $W = null, $E = null) {
        // Query String
        $Q = 'SELECT '. $C .' FROM  '. $T;

        // Where Params
        if($W != null):
            $Q .= ' WHERE '. $W;
        endif;

        // Extras Params
        if($E != null):
            $Q .= ' '. $E;
        endif;
        // Execute Query
        $R = mysqli_query($this->Con(), $Q);

        // Return result
        return mysqli_num_rows($R);
    }

    // Count Function
    public function Update($T, $P, $W = null, $E = null) {
        // Query String
        $Q = 'UPDATE '. $T .' SET '. $P;

        // Where Params
        if($W != null):
            $Q .= ' WHERE '. $W;
        endif;

        // Extras Params
        if($E != null):
            $Q .= ' '. $E;
        endif;

        // Execute Query
        $R = mysqli_query($this->Con(), $Q);

        // Return result
        return true;
    }

    public function Insert($T, $P, $V){
        // Query String
        $Q = 'INSERT INTO '. $T .'  ';

        $Q .= '(';
        $PMaxCount = count($P);
        $PCount = 0;
        foreach($P as $Key => $Data):
            $Q .= $Data;
            $PCount++;
            if($PCount < $PMaxCount):
                $Q .= ",";
            endif;

        endforeach;
        $Q .= ")";

        $Q .= " VALUES ";

        $Q .= '(';
        $VMaxCount = count($V);
        $VCount = 0;
        foreach($V as $Key => $Data):
            $Q .= $Data;
            $VCount++;
            if($VCount < $VMaxCount):
                $Q .= ",";
            endif;
        endforeach;
        $Q .= ")";


        // Execute Query
        $R = mysqli_query($this->Con(), $Q);

        // Return result
        return true;
    }

}
<?php
/**
 * This API Back-end system was design and developed for RDP Services.
 * Developed by P3x & Jeihden.
 * All rights reserved PZ API Copyright (c) 2020.
 *
 */

class DBConManager
{
    protected $host = 'localhost:3306';    // DB Host
    protected $user = 'root';         // DB User
    protected $pass = 'Hv_oLa.323.';             // DB Pass
    protected $db = 'hv_rp';        // DB Name
    protected $pz_conn = null;

    function connect() {
        $con = mysqli_connect($this->host, $this->user, $this->pass, $this->db);
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

   // Select Function
   public function Select($T, $C = '*', $W = null, $E = null) {
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
           return mysqli_fetch_assoc($R);
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

}

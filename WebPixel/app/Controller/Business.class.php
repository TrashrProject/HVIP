<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */


class Business
{
    public $CorpData;
    public $CorpID;
    public $DB;
    public $status;

    public function __construct($DB, $CorpID)  {
        $this->DB = $DB;
        $this->CorpID = $CorpID;

        if(!$this->CorpExists()):
            $this->status = false;
            return;
        endif;

        $this->CorpData = $this->GetCorpData();

        $this->status = true;

    }

    // Checks if CorpID exists
    public function CorpExists(){
        $R = $this->DB->Query("SELECT * FROM groups WHERE id = " .$this->CorpID. " AND (type = '1' OR type = '2') LIMIT 1");
        if(mysqli_num_rows($R) != 1):
            return false;
        else:
            return true;
        endif;

    }

    // Get Corp Data
    public function GetCorpData(){
        $R = $this->DB->Query("SELECT * FROM groups WHERE id = " .$this->CorpID. " LIMIT 1");
         return mysqli_fetch_assoc($R);
    }

    // Get Corp Ranks
    public function GetCorpRanks(){
        $R = $this->DB->Query("SELECT * FROM play_jobs_ranks WHERE job = " .$this->CorpID. " ORDER BY rank DESC ");
        return $R;
    }

    // Get Corp Employees
    public function GetCorpEmployees(){
        $R = $this->DB->Query("SELECT * FROM groups_memberships WHERE group_id = " .$this->CorpID. " ");
        if(mysqli_num_rows($R) > 0):
            return mysqli_fetch_assoc($R);
        else:
            return null;
        endif;
    }

    // Get Corp Employees Count
    public function GetCorpEmployeesCount(){
        $R = $this->DB->Query("SELECT * FROM group_memberships WHERE group_id = " .$this->CorpID. " ");
        return mysqli_num_rows($R);
    }

    // Get Corp Employees Count
    public function GetCorpEmployeesByRank($Rank){
        $R = $this->DB->Query("SELECT * FROM group_memberships WHERE group_id = " .$this->CorpID. " AND rank = ". $Rank ." ");
        return $R;
    }




}
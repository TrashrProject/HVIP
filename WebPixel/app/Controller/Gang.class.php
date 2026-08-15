<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */


class Gang
{
    protected $DB;
    public $GangID;
    public $Data;
    public $TurfCount;
    public $MemberCount;
    public $RankList;
    public $Status;
    public $GangOwner;

    public function __construct($DB, $GID)
    {
        $this->DB = $DB;
        $this->GangID = AppFunctions::GeneralClean($GID);

        // Validate Entry
        if(!is_numeric($this->GangID)):
            $this->Status = false;
            return;
        endif;

        // Gang Exists
        if(!$this->GangExists()):
            $this->Status = false;
            return;
        endif;

        // Get Gang Data
        $this->Data = $this->GetGangData();
        // Get Turf Count
        $this->TurfCount = $this->GetGangTurfCount();
        // Member Count
        $this->MemberCount = $this->GetGangMembersCount();
        // Gang Owner
        $this->GangOwner = $this->GetGangOwner();

        $this->Status = true;
        return;

    }

    //Gang Exists
    public function GangExists(){
        $R = $this->DB->Query("SELECT null FROM groups WHERE id = ".$this->GangID." AND type = '3' LIMIT 1");
        if(mysqli_num_rows($R) == 1):
            return true;
        else:
            return false;
        endif;
    }

    //Get Gang Data
    public function GetGangData(){
        $R = $this->DB->Query("SELECT * FROM groups WHERE id = ".$this->GangID." AND type = '3' LIMIT 1");
        return mysqli_fetch_assoc($R);
    }

    // Get Gang Turf Count
    public function GetGangTurfCount(){
        $R = $this->DB->Query("SELECT groups.id, rooms.group_id FROM groups, rooms WHERE groups.id = ". $this->GangID ." AND groups.id = rooms.group_id");
        return mysqli_num_rows($R);
    }

    public function GetGangMembersCount(){
        $R = $this->DB->Query("SELECT null FROM group_memberships WHERE group_id = ".$this->GangID." ");
        return mysqli_num_rows($R);
    }

    //Get Rank List
    public function GetRankList(){

        $R = $this->DB->Query("SELECT * FROM play_jobs_ranks WHERE job = ".$this->GangID." ORDER BY rank DESC");
        if(mysqli_num_rows($R) == 0):
            return null;
        endif;
        return mysqli_fetch_assoc($R);
    }

    public function GetGangOwner(){
        $R = $this->DB->Query("SELECT username, look, id FROM users WHERE id = ".$this->Data['owner_id']. " LIMIT 1");
        return mysqli_fetch_assoc($R);
    }



}
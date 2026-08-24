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
        $this->CorpID = (int)$CorpID;

        if(!$this->CorpExists()):
            $this->status = false;
            return;
        endif;

        $this->CorpData = $this->GetCorpData();

        $this->status = true;

    }

    // Checks if CorpID exists
    public function CorpExists(){
        $R = $this->DB->Query("SELECT *, group_type AS type FROM groups WHERE id = " .$this->CorpID. " AND group_type IN (1,5) LIMIT 1");
        if(mysqli_num_rows($R) != 1):
            return false;
        else:
            return true;
        endif;

    }

    // Get Corp Data
    public function GetCorpData(){
        $R = $this->DB->Query("SELECT *, group_type AS type FROM groups WHERE id = " .$this->CorpID. " LIMIT 1");
         return mysqli_fetch_assoc($R);
    }

    // Get Corp Ranks
    public function GetCorpRanks(){
        $R = $this->DB->Query("(SELECT level AS rank, CONVERT(name USING utf8mb4) COLLATE utf8mb4_general_ci AS name, shift_pay AS pay, CONVERT(shift_motto USING utf8mb4) COLLATE utf8mb4_general_ci AS shift_motto FROM group_roles WHERE group_id = " .$this->CorpID. ") UNION (SELECT DISTINCT gm.level AS rank, IF(gm.level=0,'Direction / proprietaire','Grade non configure') COLLATE utf8mb4_general_ci AS name,0 AS pay,'' COLLATE utf8mb4_general_ci AS shift_motto FROM group_memberships gm WHERE gm.group_id=".$this->CorpID." AND NOT EXISTS(SELECT 1 FROM group_roles gr WHERE gr.group_id=gm.group_id AND gr.level=gm.level)) ORDER BY rank DESC");
        return $R;
    }

    // Get Corp Employees
    public function GetCorpEmployees(){
        $R = $this->DB->Query("SELECT * FROM group_memberships WHERE group_id = " .$this->CorpID. " ");
        if(mysqli_num_rows($R) > 0):
            return mysqli_fetch_assoc($R);
        else:
            return null;
        endif;
    }

    // Get Corp Employees Count
    public function GetCorpEmployeesCount(){
        $R = $this->DB->Query("SELECT gm.id FROM group_memberships gm INNER JOIN users u ON u.id=gm.user_id WHERE gm.group_id = " .$this->CorpID. " ");
        return mysqli_num_rows($R);
    }

    // Get Corp Employees Count
    public function GetCorpEmployeesByRank($Rank){
        $R = $this->DB->Query("SELECT gm.*, gm.level AS rank, u.username, u.look, u.online FROM group_memberships gm INNER JOIN users u ON u.id=gm.user_id WHERE gm.group_id = " .$this->CorpID. " AND gm.level = ". (int)$Rank ." ORDER BY u.username ");
        return $R;
    }

    public function GetCorpRoster()
    {
        return $this->DB->Query("SELECT gr.level AS rank,gr.name AS rank_name,gr.shift_pay AS pay,gm.user_id,u.username,u.look,u.online FROM group_roles gr LEFT JOIN group_memberships gm ON gm.group_id=gr.group_id AND gm.level=gr.level LEFT JOIN users u ON u.id=gm.user_id WHERE gr.group_id=".$this->CorpID." ORDER BY gr.level DESC,u.username ASC");
    }




}

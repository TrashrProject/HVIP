<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

class GangsManager
{
    protected $DB;

    public function __construct($DB)
    {
        // Construct function
        $this->DB = $DB;

    }

    public function GetUserGang($ID) {
        $R = $this->DB->Query("SELECT `group_id`, `type`, `user_id` FROM group_memberships WHERE `type` = '3' AND `user_id` = ". $ID ." LIMIT 1");
        if(mysqli_num_rows($R) == 1):
            $GangID = mysqli_fetch_assoc($R);
            $R_ = $this->DB->Query("SELECT `name`, `id`, `type`, `badge`, `gang_deaths`, `gang_kills` FROM groups WHERE `type` = '3' AND `id` = ". $GangID['group_id'] ." LIMIT 1");
            return mysqli_fetch_assoc($R_);
        else:
            return null;
        endif;
    }

    // Gets Stats
    public function GetLeaderBoard($Data, $Column){

        if($Data == "groups"):
            $R = $this->DB->Query("SELECT * FROM groups WHERE type = '3' ORDER BY ". $Column ." DESC LIMIT 8");
            return $R;
        endif;

        return null;
    }

    public function GetGangLeader($GangId) {
        $R = $this->DB->Query("SELECT users.username FROM groups, users WHERE groups.id = ".$GangId." AND users.id = groups.owner_id LIMIT 1");
        if(mysqli_num_rows($R) == 1):
            $GangID = mysqli_fetch_assoc($R);
            return $GangID['username'];
        else:
            return "Desconocid@";
        endif;
    }
}
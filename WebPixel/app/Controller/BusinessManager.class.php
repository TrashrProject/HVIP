<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */


class BusinessManager
{
    protected $DB;

    public function __construct($DB)
    {
        // Construct function
        $this->DB = $DB;

    }

    public function GetUserPrimaryBusiness($ID) {
        $R = $this->DB->Query("SELECT gm.*, gm.level AS rank, g.group_type AS type FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE g.group_type IN (1,5) AND gm.user_id=". (int)$ID ." ORDER BY gm.id ASC LIMIT 1");
        if(mysqli_num_rows($R) == 1):
            $BusinessID = mysqli_fetch_assoc($R);
            $R_ = $this->DB->Query("SELECT g.name,g.badge,g.id,g.id AS job,g.group_type AS type,gm.level AS rank,COALESCE(gr.name,'Grade non configure') AS RankName,COALESCE(gr.shift_pay,0) AS pay FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id LEFT JOIN group_roles gr ON gr.group_id=gm.group_id AND gr.level=gm.level WHERE gm.id=".(int)$BusinessID['id']." AND g.group_type IN (1,5) LIMIT 1");
            return mysqli_fetch_assoc($R_);
        else:
            return null;
        endif;
    }

    public function GetUserSecondaryBusiness($ID) {
        $R = $this->DB->Query("SELECT gm.*, gm.level AS rank, g.group_type AS type FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE g.group_type IN (1,5) AND gm.user_id=". (int)$ID ." ORDER BY gm.id ASC LIMIT 1,1");
        if(mysqli_num_rows($R) == 1):
            $BusinessID = mysqli_fetch_assoc($R);
            $R_ = $this->DB->Query("SELECT g.name,g.badge,g.id,g.id AS job,g.group_type AS type,gm.level AS rank,COALESCE(gr.name,'Grade non configure') AS RankName,COALESCE(gr.shift_pay,0) AS pay FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id LEFT JOIN group_roles gr ON gr.group_id=gm.group_id AND gr.level=gm.level WHERE gm.id=".(int)$BusinessID['id']." AND g.group_type IN (1,5) LIMIT 1");
            return mysqli_fetch_assoc($R_);
        else:
            return null;
        endif;
    }
}

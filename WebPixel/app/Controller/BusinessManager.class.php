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
        $R = $this->DB->Query("SELECT * FROM `group_memberships` WHERE `type` = '1' AND `user_id` = ". $ID ." LIMIT 1");
        if(mysqli_num_rows($R) == 1):
            $BusinessID = mysqli_fetch_assoc($R);
            $R_ = $this->DB->Query("SELECT groups.name, groups.badge, groups.id, play_jobs_ranks.job, groups.type, play_jobs_ranks.rank, play_jobs_ranks.name as RankName, play_jobs_ranks.pay  FROM groups, play_jobs_ranks WHERE groups.id = play_jobs_ranks.job AND groups.id= ". $BusinessID['group_id'] ." AND  groups.type = '1' AND play_jobs_ranks.rank = ". $BusinessID['rank'] ." LIMIT 1");
            return mysqli_fetch_assoc($R_);
        else:
            return null;
        endif;
    }

    public function GetUserSecondaryBusiness($ID) {
        $R = $this->DB->Query("SELECT * FROM group_memberships WHERE type = '2' AND user_id = ". $ID ." LIMIT 1");
        if(mysqli_num_rows($R) == 1):
            $BusinessID = mysqli_fetch_assoc($R);
            $R_ = $this->DB->Query("SELECT groups.name, groups.badge, groups.id, play_jobs_ranks.job, groups.type, play_jobs_ranks.rank, play_jobs_ranks.name as RankName, play_jobs_ranks.pay  FROM groups, play_jobs_ranks WHERE groups.type = '2' AND  groups.id= ". $BusinessID['group_id'] ." AND play_jobs_ranks.job = ". $BusinessID['group_id'] ." AND play_jobs_ranks.rank = ". $BusinessID['rank'] ." LIMIT 1");
            return mysqli_fetch_assoc($R_);
        else:
            return null;
        endif;
    }
}
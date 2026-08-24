<?php
class Gang
{
    protected $DB;
    public $GangID;
    public $Data;
    public $TurfCount = 0;
    public $MemberCount = 0;
    public $Status = false;
    public $GangOwner;

    public function __construct($DB, $GID)
    {
        $this->DB = $DB;
        if (!is_numeric($GID)) return;
        $this->GangID = (int) $GID;
        if (!$this->GangExists()) return;
        $this->Data = $this->GetGangData();
        $this->TurfCount = $this->GetGangTurfCount();
        $this->MemberCount = $this->GetGangMembersCount();
        $this->GangOwner = $this->GetGangOwner();
        $this->Status = true;
    }

    public function GangExists() { return mysqli_num_rows($this->DB->Query("SELECT id FROM groups WHERE id={$this->GangID} AND group_type IN (2,3) LIMIT 1")) === 1; }
    public function GetGangData() { return mysqli_fetch_assoc($this->DB->Query("SELECT *, group_type AS type FROM groups WHERE id={$this->GangID} AND group_type IN (2,3) LIMIT 1")); }
    public function GetGangTurfCount() { return mysqli_num_rows($this->DB->Query("SELECT id FROM rooms WHERE group_id={$this->GangID}")); }
    public function GetGangMembersCount() { return mysqli_num_rows($this->DB->Query("SELECT gm.user_id FROM group_memberships gm INNER JOIN users u ON u.id=gm.user_id WHERE gm.group_id={$this->GangID}")); }
    public function GetRankList() { return $this->DB->Query("SELECT level AS rank, name FROM group_roles WHERE group_id={$this->GangID} ORDER BY level DESC"); }
    public function GetMembersForRank($rank)
    {
        $rank = (int) $rank;
        return $this->DB->Query("SELECT u.id,u.username,u.look,u.online FROM group_memberships gm INNER JOIN users u ON u.id=gm.user_id WHERE gm.group_id={$this->GangID} AND gm.level={$rank} ORDER BY u.username ASC");
    }
    public function GetGangOwner()
    {
        $ownerId = (int) $this->Data['owner_id'];
        $R = $this->DB->Query("SELECT username,look,id FROM users WHERE id={$ownerId} LIMIT 1");
        return mysqli_num_rows($R) === 1 ? mysqli_fetch_assoc($R) : array('username'=>'Compte supprimé','look'=>'','id'=>0);
    }
}

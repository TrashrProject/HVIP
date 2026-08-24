<?php
class GangsManager
{
    protected $DB;
    public function __construct($DB) { $this->DB = $DB; }

    public function GetUserGang($ID)
    {
        $ID = (int) $ID;
        $R = $this->DB->Query("SELECT g.*, g.group_type AS type FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE g.group_type IN (2,3) AND gm.user_id={$ID} LIMIT 1");
        return mysqli_num_rows($R) === 1 ? mysqli_fetch_assoc($R) : null;
    }

    public function GetGangs($search = '')
    {
        $where = "g.group_type IN (2,3)";
        $search = trim((string) $search);
        if ($search !== '') {
            $escaped = mysqli_real_escape_string($this->DB->Con(), $search);
            $where .= " AND g.name LIKE '%{$escaped}%'";
        }
        return $this->DB->Query("SELECT g.*, (SELECT COUNT(*) FROM group_memberships gm INNER JOIN users u ON u.id=gm.user_id WHERE gm.group_id=g.id) AS member_count FROM groups g WHERE {$where} ORDER BY g.name ASC");
    }

    public function GetLeaderBoard($Data, $Column)
    {
        $allowed = array('bank','gang_kills','gang_deaths','gang_cop_kills','gang_turfs_taken','gang_turfs_defend','gang_farm_cocaine','gang_farm_weed','gang_farm_medicines','gang_fab_guns','gang_heists');
        if ($Data !== 'groups' || !in_array($Column, $allowed, true)) return null;
        return $this->DB->Query("SELECT *, group_type AS type FROM groups WHERE group_type IN (2,3) ORDER BY `{$Column}` DESC, name ASC LIMIT 8");
    }

    public function GetGangLeader($GangId)
    {
        $GangId = (int) $GangId;
        $R = $this->DB->Query("SELECT u.username FROM groups g INNER JOIN users u ON u.id=g.owner_id WHERE g.id={$GangId} LIMIT 1");
        return mysqli_num_rows($R) === 1 ? mysqli_fetch_assoc($R)['username'] : 'Compte supprimé';
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Incoming;
using System.Collections.Concurrent;

using Plus.Database.Interfaces;
using log4net;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Groups.Forums;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;
using System.Net;
using Plus.Core;

namespace Plus.HabboHotel.Groups
{
    public class GroupManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Groups.GroupManager");

        public Dictionary<int, GroupBackGroundColours> BackGroundColours;
        public List<GroupBaseColours> BaseColours;
        public List<GroupBases> Bases;

        public Dictionary<int, GroupSymbolColours> SymbolColours;
        public List<GroupSymbols> Symbols;

        private readonly Object _groupLoadingSync;
        private ConcurrentDictionary<int, Group> _groups;
        private readonly GroupForumManager _groupForumManager;

        //public static ConcurrentDictionary<int, Group> Jobs = new ConcurrentDictionary<int, Group>();
        //public static ConcurrentDictionary<int, Group> Gangs = new ConcurrentDictionary<int, Group>();
        public static ConcurrentDictionary<int, GroupRank> GenericGangRanks = new ConcurrentDictionary<int, GroupRank>();

        public GroupManager()
        {
            this._groupLoadingSync = new Object();
            this._groupForumManager = new GroupForumManager();
            this.Init();
            this.PreLoadGroups();
        }

        public bool TryGetGroup(int Id, out Group Group)
        {
            Group = null;

            if (this._groups.ContainsKey(Id))
                return this._groups.TryGetValue(Id, out Group);

            lock (this._groupLoadingSync)
            {
                if (this._groups.ContainsKey(Id))
                    return this._groups.TryGetValue(Id, out Group);

                DataRow Row = null;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `groups` WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", Id);
                    Row = dbClient.getRow();
                    ConcurrentDictionary<int, GroupRank> Ranks = GenerateJobRanks(Id);
                    List<int> Requests;
                    ConcurrentDictionary<int, GroupMember> Members = GenerateJobMembers(Id, out Requests);
                    ConcurrentDictionary<int, GroupLogs> Logs = GenerateLogs(Id);
                    if (Row != null)
                    {
                        Group = new Group(
                            Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["type"]), Convert.ToString(Row["name"]), Convert.ToString(Row["desc"]), Convert.ToString(Row["badge"]), Convert.ToInt32(Row["room_id"]), Convert.ToInt32(Row["owner_id"]),
                            Convert.ToInt32(Row["created"]), Convert.ToInt32(Row["state"]), Convert.ToString(Row["colour1"]), Convert.ToString(Row["colour2"]), Convert.ToInt32(Row["admindeco"]), Convert.ToInt32(Row["has_forum"]), Convert.ToInt32(Row["has_groupchat"]), Ranks, Members, Convert.ToInt32(Row["gang_kills"]), Convert.ToInt32(Row["gang_cop_kills"]), Convert.ToInt32(Row["gang_deaths"]), Convert.ToInt32(Row["gang_score"]), Convert.ToInt32(Row["gang_heists"]), Convert.ToInt32(Row["gang_turfs_taken"]), Convert.ToInt32(Row["gang_turfs_defend"]), Convert.ToInt32(Row["gang_farm_cocaine"]), Convert.ToInt32(Row["gang_farm_medicines"]), Convert.ToInt32(Row["gang_farm_weed"]), Convert.ToInt32(Row["gang_fab_guns"]), Convert.ToString(Row["activity"]), Convert.ToInt32(Row["stock"]), Convert.ToInt32(Row["shifts"]), Convert.ToInt32(Row["sells"]), Convert.ToInt32(Row["actions"]), Convert.ToInt32(Row["bank"]), Convert.ToInt32(Row["spend"]), Convert.ToInt32(Row["profits"]), PlusEnvironment.EnumToBool(Row["removable"].ToString()), Logs, PlusEnvironment.EnumToBool(Row["bankruptcy"].ToString()));
                        this._groups.TryAdd(Group.Id, Group);
                        return true;
                    }
                }
            }
            return false;
        }

        public void PreLoadGroups()
        {
            lock (this._groupLoadingSync)
            {
                DataTable Row = null;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `groups`");
                    Row = dbClient.getTable();

                    foreach (DataRow G in Row.Rows)
                    {
                        ConcurrentDictionary<int, GroupRank> Ranks = GenerateJobRanks(Convert.ToInt32(G["id"]));
                        List<int> Requests;
                        ConcurrentDictionary<int, GroupMember> Members = GenerateJobMembers(Convert.ToInt32(G["id"]), out Requests);
                        ConcurrentDictionary<int, GroupLogs> Logs = GenerateLogs(Convert.ToInt32(G["id"]));
                        Group Group = new Group(
                                Convert.ToInt32(G["id"]), Convert.ToInt32(G["type"]), Convert.ToString(G["name"]), Convert.ToString(G["desc"]), Convert.ToString(G["badge"]), Convert.ToInt32(G["room_id"]), Convert.ToInt32(G["owner_id"]),
                                Convert.ToInt32(G["created"]), Convert.ToInt32(G["state"]), Convert.ToString(G["colour1"]), Convert.ToString(G["colour2"]), Convert.ToInt32(G["admindeco"]), Convert.ToInt32(G["has_forum"]), Convert.ToInt32(G["has_groupchat"]), Ranks, Members, Convert.ToInt32(G["gang_kills"]), Convert.ToInt32(G["gang_cop_kills"]), Convert.ToInt32(G["gang_deaths"]), Convert.ToInt32(G["gang_score"]), Convert.ToInt32(G["gang_heists"]), Convert.ToInt32(G["gang_turfs_taken"]), Convert.ToInt32(G["gang_turfs_defend"]), Convert.ToInt32(G["gang_farm_cocaine"]), Convert.ToInt32(G["gang_farm_medicines"]), Convert.ToInt32(G["gang_farm_weed"]), Convert.ToInt32(G["gang_fab_guns"]), Convert.ToString(G["activity"]), Convert.ToInt32(G["stock"]), Convert.ToInt32(G["shifts"]), Convert.ToInt32(G["sells"]), Convert.ToInt32(G["actions"]), Convert.ToInt32(G["bank"]), Convert.ToInt32(G["spend"]), Convert.ToInt32(G["profits"]), PlusEnvironment.EnumToBool(G["removable"].ToString()), Logs, PlusEnvironment.EnumToBool(G["bankruptcy"].ToString()));
                        this._groups.TryAdd(Group.Id, Group);
                    }
                }
            }
        }

        public void Init()
        {
            Bases = new List<GroupBases>();
            Symbols = new List<GroupSymbols>();
            BaseColours = new List<GroupBaseColours>();
            SymbolColours = new Dictionary<int, GroupSymbolColours>();
            BackGroundColours = new Dictionary<int, GroupBackGroundColours>();
            _groups = new ConcurrentDictionary<int, Group>();
            this._groupForumManager.Init();
            ClearInfo();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM groups_items WHERE enabled='1'");
                DataTable dItems = dbClient.getTable();

                foreach (DataRow dRow in dItems.Rows)
                {
                    switch (dRow[0].ToString())
                    {
                        case "base":
                            Bases.Add(new GroupBases(Convert.ToInt32(dRow[1]), dRow[2].ToString(), dRow[3].ToString()));
                            break;

                        case "symbol":
                            Symbols.Add(new GroupSymbols(Convert.ToInt32(dRow[1]), dRow[2].ToString(), dRow[3].ToString()));
                            break;

                        case "color":
                            BaseColours.Add(new GroupBaseColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;

                        case "color2":
                            SymbolColours.Add(Convert.ToInt32(dRow[1]), new GroupSymbolColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;

                        case "color3":
                            BackGroundColours.Add(Convert.ToInt32(dRow[1]), new GroupBackGroundColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;
                    }
                }
            }

            log.Info("Group Manager -> LOADED");
        }

        public void ClearInfo()
        {
            Bases.Clear();
            Symbols.Clear();
            BaseColours.Clear();
            SymbolColours.Clear();
            BackGroundColours.Clear();
        }

        public bool TryCreateGroup(Habbo Player, int GType, string Name, string Description, int RoomId, string Badge, string Colour1, string Colour2, string GActivity, bool Removable, int State, out Group Group)
        {
            Group = new Group(0, GType, Name, Description, Badge, RoomId, Player.Id, (int)PlusEnvironment.GetUnixTimestamp(), 0, Colour1, Colour2, 0, 0, 0, GenericGangRanks, new ConcurrentDictionary<int, GroupMember>(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, GActivity, 0, 0, 0, 0, 0, 0, 0, Removable, new ConcurrentDictionary<int, GroupLogs>(), false);
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Badge))
                return false;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                // Gangs no require set badge by Id. Is obtained by the code of the badge generator.
                if (Group.GType != 3)
                    Group.Badge = Group.Id.ToString();

                //int StateDefault = 2;// 0 = open 1 = Invitacion 2 = Private
                dbClient.SetQuery("INSERT INTO `groups` (`type`, `name`, `desc`, `badge`, `owner_id`, `created`, `room_id`, `state`, `colour1`, `colour2`, `admindeco`, `activity`, `bank`, `removable`) VALUES (@type, @name, @desc, @badge, @owner, UNIX_TIMESTAMP(), @room, @state, @colour1, @colour2, '0', @gactivity, @bank, @removable)");
                dbClient.AddParameter("type", Group.GType);
                dbClient.AddParameter("name", Group.Name);
                dbClient.AddParameter("desc", Group.Description);
                dbClient.AddParameter("owner", Group.CreatorId);//Group.CreatorId
                dbClient.AddParameter("badge", Group.Badge);
                dbClient.AddParameter("room", Group.RoomId);
                dbClient.AddParameter("state", Convert.ToString(State));
                dbClient.AddParameter("colour1", Group.Colour1);
                dbClient.AddParameter("colour2", Group.Colour2);
                dbClient.AddParameter("gactivity", Group.GActivity);
                dbClient.AddParameter("bank", (RoleplayManager.GangsPrice / 4));
                dbClient.AddParameter("removable", PlusEnvironment.BoolToEnum(Group.Removable));
                Group.Id = Convert.ToInt32(dbClient.InsertQuery());
                Group.Bank = (RoleplayManager.GangsPrice / 4);

                Group.AddMember(Player.Id, 1, false, 0, "", "", Player.GetClient());
                Group.MakeAdmin(Player.Id);

                if (Player.GetClient() != null && Player.GetClient().GetPlay() != null)
                {
                    if (Group.GType < 3)
                        Player.GetClient().GetPlay().Corp = Group.Id;
                    else
                        Player.GetClient().GetPlay().Gang = Group.Id;
                }

                if (!this._groups.TryAdd(Group.Id, Group))
                    return false;
                else
                {
                    dbClient.SetQuery("UPDATE `rooms` SET `group_id` = @gid WHERE `id` = @rid LIMIT 1");
                    dbClient.AddParameter("gid", Group.Id);
                    dbClient.AddParameter("rid", Group.RoomId);
                    dbClient.RunQuery();

                    dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + RoomId + "'");
                }

                // Insert the first rank
                string[] commands = Group.GetCommandsbyActivity(Group.GActivity).ToString().Split(',');
                string[] workrooms = Group.RoomId.ToString().Split(',');
                Group.AddRank(Group.Id, 1, Group.Name, "", "", 0, commands, workrooms, 0, 0);

                if (State == 0)
                    Group.GroupType = GroupType.OPEN;
                else
                    Group.GroupType = GroupType.LOCKED;

				#region Download image OFF
                /*
				try
				{
                    // Download de badge
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(Badge, RoleplayManager.SWFPath + "\\habbo-imaging\\gang\\" + Group.Id + ".gif");
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString() + " => " + e.Message);
                }
                */
                #endregion
            }

            return true;
        }

        public bool TryCreateBusiness(Habbo Player, int GType, string Name, string Description, int RoomId, string Badge, string Colour1, string Colour2, string GActivity, bool Removable, out Group Group)
        {
            Group = new Group(0, GType, Name, Description, Badge, RoomId, Player.Id, (int)PlusEnvironment.GetUnixTimestamp(), 0, Colour1, Colour2, 0, 0, 0, GenericGangRanks, new ConcurrentDictionary<int, GroupMember>(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, GActivity, 0, 0, 0, 0, 0, 0, 0, Removable, new ConcurrentDictionary<int, GroupLogs>(), false);
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Badge))
                return false;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                int StateDefault = 2;// 0 = open 1 = Invitacion 2 = Private
                dbClient.SetQuery("INSERT INTO `groups` (`type`, `name`, `desc`, `badge`, `owner_id`, `created`, `room_id`, `state`, `colour1`, `colour2`, `admindeco`, `activity`) VALUES (@type, @name, @desc, @badge, @owner, UNIX_TIMESTAMP(), @room, @state, @colour1, @colour2, '0', @gactivity)");
                dbClient.AddParameter("type", Group.GType);
                dbClient.AddParameter("name", Group.Name);
                dbClient.AddParameter("desc", Group.Description);
                dbClient.AddParameter("owner", '0');//Group.CreatorId
                dbClient.AddParameter("badge", Group.Badge);
                dbClient.AddParameter("room", Group.RoomId);
                dbClient.AddParameter("state", StateDefault);
                dbClient.AddParameter("colour1", Group.Colour1);
                dbClient.AddParameter("colour2", Group.Colour2);
                dbClient.AddParameter("gactivity", Group.GActivity);
                Group.Id = Convert.ToInt32(dbClient.InsertQuery());

                Group.AddMember(Player.Id);
                Group.MakeAdmin(Player.Id);

                if (!this._groups.TryAdd(Group.Id, Group))
                    return false;
                else
                {
                    dbClient.SetQuery("UPDATE `rooms` SET `group_id` = @gid WHERE `id` = @rid LIMIT 1");
                    dbClient.AddParameter("gid", Group.Id);
                    dbClient.AddParameter("rid", Group.RoomId);
                    dbClient.RunQuery();

                    dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + RoomId + "'");

                }
            }
            return true;
        }


        public string CheckActiveSymbol(string Symbol)
        {
            if (Symbol == "s000" || Symbol == "s00000")
            {
                return "";
            }
            return Symbol;
        }

        public string GetGroupColour(int Index, bool Colour1)
        {
            if (Colour1)
            {
                if (SymbolColours.ContainsKey(Index))
                {
                    return SymbolColours[Index].Colour;
                }
            }
            else
            {
                if (BackGroundColours.ContainsKey(Index))
                {
                    return BackGroundColours[Index].Colour;
                }
            }

            return "4f8a00";
        }

        public void DeleteGroup(int Id)
        {
            Group Group = null;
            if (this._groups.ContainsKey(Id))
                this._groups.TryRemove(Id, out Group);

            if (Group != null)
            {
                Group.Dispose();
            }
        }

        public List<Group> GetGroupsForUser(int UserId)
        {
            List<Group> Groups = new List<Group>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE m.user_id = @user");
                dbClient.AddParameter("user", UserId);
                DataTable GetGroups = dbClient.getTable();

                if (GetGroups != null)
                {
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        Group Group = null;
                        if (this.TryGetGroup(Convert.ToInt32(Row["id"]), out Group))
                            Groups.Add(Group);
                    }
                }
            }
            return Groups;
        }

        public List<Group> GetGroupsForUserDict(int UserId) // Obtiene todos los grupos en general
        {
            List<Group> Groups = new List<Group>();

            lock (_groups)
            {
                if (_groups.Values.Where(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId).ToList().Count > 0)
                    Groups.Add(_groups.Values.FirstOrDefault(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId));
            }
            /*
            lock (Gangs)
            {
                if (Gangs.Values.Where(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId).ToList().Count > 0)
                    Groups.Add(Gangs.Values.FirstOrDefault(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId));
            }
            */
            return Groups;
        }

        public List<Group> GetJobsForUserDict(int UserId)// Obtiene Grupos con Type 1 o 2
        {
            List<Group> Groups = new List<Group>();

            lock (_groups)
            {
                if (_groups.Values.Where(x => ((x.Members.ContainsKey(UserId) || x.CreatorId == UserId) && (x.GType < 3))).ToList().Count > 0)
                    Groups.Add(_groups.Values.FirstOrDefault(x => ((x.Members.ContainsKey(UserId) || x.CreatorId == UserId) && (x.GType < 3))));
            }
            return Groups;
        }

        public List<Group> GetGroupForumsForUser(int UserId)
        {
            List<Group> Groups = new List<Group>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE g.has_forum = '1' AND m.user_id = @user");
                dbClient.AddParameter("user", UserId);
                DataTable GetGroups = dbClient.getTable();

                if (GetGroups != null)
                {
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        Group Group = null;
                        if (this.TryGetGroup(Convert.ToInt32(Row["id"]), out Group))
                            Groups.Add(Group);
                    }
                }
            }

            return Groups;
        }

        public GroupForumManager GetGroupForumManager()
        {
            return this._groupForumManager;
        }

        public List<Group> GetActiveGroupForums()
        {
            return this._groups.Values.Where(x => x.ForumEnabled && x.GetForum().MessageCount > 0).OrderByDescending(x => x.GetForum().MessageCount).ToList();
        }

        public List<Group> GetPopularGroupForums()
        {
            return this._groups.Values.Where(x => x.ForumEnabled).OrderByDescending(x => x.GetForum().LastReplierId).ToList();
        }
        public ICollection<Group> Groups
        {
            get { return this._groups.Values; }
        }
        public ConcurrentDictionary<int, GroupMember> GenerateJobMembers(int Id, out List<int> Requests)
        {
            ConcurrentDictionary<int, GroupMember> Members = new ConcurrentDictionary<int, GroupMember>();
            Requests = new List<int>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id,rank FROM `group_memberships` WHERE `group_id` = '" + Id + "'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        int Rank = Convert.ToInt32(Row["rank"]);
                        bool IsAdmin = Rank == 6;

                        GroupMember Member = new GroupMember(Id, UserId, Rank, IsAdmin);

                        if (!Members.ContainsKey(UserId))
                            Members.TryAdd(UserId, Member);
                    }
                }

                dbClient.SetQuery("SELECT `user_id` FROM `group_requests` WHERE `group_id` = '" + Id + "'");
                DataTable RequestTable = dbClient.getTable();

                if (RequestTable != null)
                {
                    foreach (DataRow Row in RequestTable.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["user_id"]);

                        if (!Requests.Contains(UserId))
                            Requests.Add(UserId);
                    }
                }
            }

            return Members;
        }
        public ConcurrentDictionary<int, GroupRank> GenerateJobRanks(int Id)
        {
            ConcurrentDictionary<int, GroupRank> Ranks = new ConcurrentDictionary<int, GroupRank>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_jobs_ranks` WHERE `job` = '" + Id + "'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {

                    foreach (DataRow Row in Table.Rows)
                    {
                        int JobRank = Convert.ToInt32(Row["rank"]);
                        string Name = Row["name"].ToString();
                        string MaleFigure = Row["male_figure"].ToString();
                        string FemaleFigure = Row["female_figure"].ToString();
                        int Pay = Convert.ToInt32(Row["pay"]);
                        string[] Commands = Row["commands"].ToString().Split(',');
                        string[] WorkRooms = Row["workrooms"].ToString().Split(',');
                        int Limit = Convert.ToInt32(Row["limit"]);
                        int Timer = Convert.ToInt32(Row["timer"]);

                        GroupRank Rank = new GroupRank(Id, JobRank, Name, MaleFigure, FemaleFigure, Pay, Commands, WorkRooms, Limit, Timer);

                        if (!Ranks.ContainsKey(JobRank))
                            Ranks.TryAdd(JobRank, Rank);
                    }
                }
            }
            return Ranks;
        }
        public List<Group> GetJobsForUser(int UserId)
        {
            List<Group> Groups = new List<Group>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE m.user_id = @user AND (m.type = '1' OR m.type = '2')");
                dbClient.AddParameter("user", UserId);
                DataTable GetGroups = dbClient.getTable();

                if (GetGroups != null)
                {
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        Group Group = null;
                        if (this.TryGetGroup(Convert.ToInt32(Row["id"]), out Group))
                            Groups.Add(Group);
                    }
                }
            }
            return Groups;
        }
        public ConcurrentDictionary<int, GroupLogs> GenerateLogs(int Id)
        {
            ConcurrentDictionary<int, GroupLogs> Logs = new ConcurrentDictionary<int, GroupLogs>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `groups_logs` WHERE `group_id` = '" + Id + "' ORDER BY timestamp DESC LIMIT 30");
                DataTable Table = dbClient.getTable();
                int c = 0;
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        GroupLogs mLogs = new GroupLogs(Id, Convert.ToInt32(Row["user_id"]), Convert.ToString(Row["action"]), Convert.ToInt32(Row["cant"]), PlusEnvironment.UnixTimeStampToDateTime(Convert.ToDouble(Row["timestamp"])));

                        if (!Logs.ContainsKey(c))
                            Logs.TryAdd(c, mLogs);
                        c++;
                    }
                }
            }

            return Logs;
        }

        public bool JobExists(int JobId, int RankId)
        {
            Group Job = GetJob(JobId);
            if (Job != null && Job.Ranks.ContainsKey(RankId))
            {
                return true;
            }

            return false;
        }
        public Group GetJob(int Id)
        {
            Group Group;
            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Id, out Group);
            
            /*if (_groups.ContainsKey(Id))
            {
                return _groups[Id];
            }
            */
            return Group;
        }
        public GroupRank GetJobRank(int JobId, int RankId)
        {
            Group Group = GetJob(JobId);

            if (Group != null && Group.Ranks.ToList().Where(x => x.Value.RankId == RankId).Count() > 0)
            {
                var getrank = Group.Ranks.Where(item => item.Value.RankId == RankId).First();
                return Group.Ranks[getrank.Key];
            }

            //if (Group != null && Group.Ranks.ContainsKey(RankId))
            //  return Group.Ranks[RankId];

            return null;
        }

        public bool HasJobCommand(GameClient Session, string command, bool Gang = false)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetPlay() == null)
                return false;

            List<Group> Groups = null;
            if (!Gang)
                Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);
            else
                Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);

            if (Groups.Count <= 0 || Groups == null)
                return false;

            bool HasRight = true;

            // Chequeamos el primer grupo de trabajo
            int JobId = Groups[0].Id;
            int JobRank = Groups[0].Members[Session.GetHabbo().Id].UserRank;

            Group Job = GetJob(JobId);
            GroupRank Rank = GetJobRank(JobId, JobRank);

            if (Job == null || Rank == null)
                HasRight = false;

            else if (!Rank.HasCommand(command))
                HasRight = false;

            // Si tiene un segundo chequeamos
            if (Groups.Count == 2)
            {
                // Si el primero no obtuvo permisos, revisamos el segundo.
                if (!HasRight)
                {
                    HasRight = true;

                    // Chequeamos el primer grupo de trabajo
                    JobId = Groups[1].Id;
                    JobRank = Groups[1].Members[Session.GetHabbo().Id].UserRank;

                    Group Job2 = GetJob(JobId);
                    GroupRank Rank2 = GetJobRank(JobId, JobRank);

                    if (Job2 == null || Rank2 == null)
                        HasRight = false;

                    else if (!Rank2.HasCommand(command))
                        HasRight = false;
                }
            }


            return HasRight;
        }

        public static GroupRank GetGroupRank(int JobId, int RankId)
        {
            Group Group = null;
            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(JobId, out Group);

            if (Group != null && Group.Ranks.ContainsKey(RankId))
                return Group.Ranks[RankId];

            return null;
        }

        public void DeleteGroupRank(int JobId, int RankId)
        {
            Group Group = null;
            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(JobId, out Group);

            if (Group != null && Group.Ranks.Where(x => x.Value.RankId == RankId).Count() > 0)
            {
                GroupRank Junk;
                //Group.Ranks.TryRemove(RankId, out Junk);
                var itemtoremove = Group.Ranks.Where(item => item.Value.RankId == RankId).First();
                Group.Ranks.TryRemove(itemtoremove.Key, out Junk);

                // Refresh the dictionary for the new keys
                Group.Ranks = GenerateJobRanks(JobId);
            }
        }

        public Group GetJobByID(int CorpId)
        {
            if (_groups.Values.Where(x => x.Id == CorpId).ToList().Count > 0)
                return _groups.Values.FirstOrDefault(x => x.Id == CorpId);

            return null;
        }

        public Group GetJobByRoomId(int RoomId)
        {
            if (_groups.Values.Where(x => x.RoomId == RoomId).ToList().Count > 0)
                return _groups.Values.FirstOrDefault(x => x.RoomId == RoomId);

            return null;
        }

        public List<Group> GetJobByName(string name)
        {
            List<Group> Groups = new List<Group>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id FROM groups WHERE name LIKE '" + name + "%'");
                DataTable GetGroups = dbClient.getTable();

                if (GetGroups != null)
                {
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        Group Group = null;
                        if (this.TryGetGroup(Convert.ToInt32(Row["id"]), out Group))
                            Groups.Add(Group);
                    }
                }
            }
            return Groups;
        }

        public List<Group> GetGangsForUser(int UserId)
        {
            List<Group> Groups = new List<Group>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE m.user_id = @user AND g.type = '3'");
                dbClient.AddParameter("user", UserId);
                DataTable GetGroups = dbClient.getTable();

                if (GetGroups != null)
                {
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        Group Group = null;
                        if (this.TryGetGroup(Convert.ToInt32(Row["id"]), out Group))
                            Groups.Add(Group);
                    }
                }
            }
            return Groups;
        }
    }
}
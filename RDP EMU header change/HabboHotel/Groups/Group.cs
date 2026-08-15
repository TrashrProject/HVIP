using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Collections.Concurrent;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using System.IO;
using Plus.HabboRoleplay.GangTurfs;

namespace Plus.HabboHotel.Groups
{
    public class Group
    {
        public int Id { get; set; }
        public int GType { get; set; }
        public string Name { get; set; }
        public int AdminOnlyDeco { get; set; }
        public string Badge { get; set; }
        public int CreateTime { get; set; }
        public int CreatorId { get; set; }
        public string Description { get; set; }
        public int RoomId { get; set; }
        public string Colour1 { get; set; }
        public string Colour2 { get; set; }
        public bool ForumEnabled { get; set; }
        public bool GroupChatEnabled { get; set; }
        public GroupType GroupType { get; set; }
        private GroupForum _forum;
        private List<int> _members;
        private List<int> _requests;
        private List<int> _administrators;
    public ConcurrentDictionary<int, GroupRank> Ranks;
        public ConcurrentDictionary<int, GroupMember> Members;
        public ConcurrentDictionary<int, GroupLogs> Logs;

        // New RP Vars
        public int GangKills { get; set; }
        public int GangCopKills { get; set; }
        public int GangDeaths { get; set; }
        public int GangScore { get; set; }
        public int GangHeists { get; set; }
        public int GangTurfsTaken { get; set; }
        public int GangTurfsDefended { get; set; }
        public int GangFarmCocaine{ get; set; }
        public int GangFarmMedicines { get; set; }
        public int GangFarmWeed { get; set; }
        public int GangFabGuns { get; set; }
        public string GActivity { get; set; }
        public int Stock { get; set; }
        public int Shifts { get; set; }
        public int Sells { get; set; }
        public int Actions { get; set; }
        public int Bank { get; set; }
        public int Spend { get; set; }
        public int Profits { get; set; }
        public bool Removable { get; set; }
        public bool BankRuptcy { get; set; }

        public Group(int Id, int GType, string Name, string Description, string Badge, int RoomId, int Owner, int Time, int Type, string Colour1, string Colour2, int AdminOnlyDeco, int forumEnabled, int chatEnabled, ConcurrentDictionary<int, GroupRank> Ranks, ConcurrentDictionary<int, GroupMember> Members, int GangKills, int GangCopKills, int GangDeaths, int GangScore, int GangHeists, int GangTurfsTaken, int GangTurfsDefended, int GangFarmCocaine, int GangFarmMedicines, int GangFarmWeed, int GangFabGuns, string GActivity, int Stock, int Shifts, int Sells, int Actions, int Bank, int Spend, int Profits, bool Removable, ConcurrentDictionary<int, GroupLogs> Logs, bool BankRuptcy)
        {
            this.Id = Id;
            this.GType = GType;
            this.Name = Name;
            this.Description = Description;
            this.RoomId = RoomId;
            this.Badge = Badge;
            this.CreateTime = Time;
            this.CreatorId = Owner;
            this.Colour1 = Colour1;
            this.Colour2 = Colour2;

            switch (Type)
            {
                case 0:
                    this.GroupType = GroupType.OPEN;
                    break;
                case 1:
                    this.GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    this.GroupType = GroupType.PRIVATE;
                    break;
            }

            this.AdminOnlyDeco = AdminOnlyDeco;
            this.ForumEnabled = forumEnabled == 1 ? true: false;
            this.GroupChatEnabled = chatEnabled == 1 ? true: false;
            this._members = new List<int>();
            this._requests = new List<int>();
            this._administrators = new List<int>();
            this.Ranks = Ranks;
            this.Members = Members;
            this.Logs = Logs;

            // New RP Vars
            this.GangKills = GangKills;
            this.GangCopKills = GangCopKills;
            this.GangDeaths = GangDeaths;
            this.GangScore = GangScore;
            this.GangHeists = GangHeists;
            this.GangTurfsTaken = GangTurfsTaken;
            this.GangTurfsDefended = GangTurfsDefended;
            this.GangFarmCocaine = GangFarmCocaine;
            this.GangFarmMedicines = GangFarmMedicines;
            this.GangFarmWeed = GangFarmWeed;
            this.GangFabGuns = GangFabGuns;
            this.GActivity = GActivity;
            this.Stock = Stock;
            this.Shifts = Shifts;
            this.Sells = Sells;
            this.Actions = Actions;
            this.Bank = Bank;
            this.Spend = Spend;
            this.Profits = Profits;
            this.Removable = Removable;
            this.BankRuptcy = BankRuptcy;

            if (this.ForumEnabled)
                InitForum(false);

            InitMembers();
            InitLogs();
        }

        /// <summary>
        /// Used to load the GroupForum, we use this on the initial initialization, and also if a user has just bought a forum for a group.
        /// </summary>
        /// <param name="purchased"></param>
        public void InitForum(bool purchased = false)
        {
            if (purchased)
                this.ForumEnabled = true;

            DataRow row = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `group_forum_settings` WHERE `group_id` = @GroupId LIMIT 1");
                dbClient.AddParameter("GroupId", this.Id);
                row = dbClient.getRow();
            }

            if (row == null)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("INSERT INTO `group_forum_settings` (`group_id`) VALUES ('" + this.Id + "')");
                    dbClient.SetQuery("SELECT * FROM `group_forum_settings` WHERE `group_id` = @GroupId LIMIT 1");
                    dbClient.AddParameter("GroupId", this.Id);
                    row = dbClient.getRow();
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `groups` SET `has_forum` = '1' WHERE `id` = '" + this.Id + "' LIMIT 1");
            }

            if (row != null)
            {
                this._forum = new GroupForum(this.Id, Convert.ToInt32(row["readability_setting"]), Convert.ToInt32(row["post_creation_setting"]), Convert.ToInt32(row["thread_creation_setting"]), Convert.ToInt32(row["moderation_setting"]), Convert.ToInt32(row["score"]));
            }
        }


        public void InitMembers()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetMembers = null;
                dbClient.SetQuery("SELECT `user_id`, `rank` FROM `group_memberships` WHERE `group_id` = @id");
                dbClient.AddParameter("id", this.Id);
                GetMembers = dbClient.getTable();

                if (GetMembers != null)
                {
                    foreach (DataRow Row in GetMembers.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        bool IsAdmin = GetGroupbyUserId(UserId);

                        GroupMember Member = new GroupMember(this.Id, UserId, Convert.ToInt32(Row["rank"]), IsAdmin);

                        this.Members.TryAdd(UserId, Member);

                        if (IsAdmin)
                        {
                            if (!this._administrators.Contains(UserId))
                                this._administrators.Add(UserId);
                        }
                        else
                        {
                            if (!this._members.Contains(UserId))
                                this._members.Add(UserId);
                        }
                    }
                }

                DataTable GetRequests = null;
                dbClient.SetQuery("SELECT `user_id` FROM `group_requests` WHERE `group_id` = @id");
                dbClient.AddParameter("id", this.Id);
                GetRequests = dbClient.getTable();

                if (GetRequests != null)
                {
                    foreach (DataRow Row in GetRequests.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        
                        if (this._members.Contains(UserId) || this._administrators.Contains(UserId) || this.Members.ContainsKey(UserId))
                        {
                            dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + this.Id + "' AND `user_id` = '" + UserId + "'");
                        }
                        else if (!this._requests.Contains(UserId))
                        {
                            this._requests.Add(UserId);
                        }
                    }
                }
            }
        }

        public void InitLogs()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetLogs = null;
                dbClient.SetQuery("SELECT * FROM `groups_logs` WHERE `group_id` = @id ORDER BY timestamp DESC LIMIT 30");
                dbClient.AddParameter("id", this.Id);
                GetLogs = dbClient.getTable();
                int c = 0;
                if (GetLogs != null)
                {
                    foreach (DataRow Row in GetLogs.Rows)
                    {
                        GroupLogs Logs = new GroupLogs(this.Id, Convert.ToInt32(Row["user_id"]), Convert.ToString(Row["action"]), Convert.ToInt32(Row["cant"]), PlusEnvironment.UnixTimeStampToDateTime(Convert.ToDouble(Row["timestamp"])));

                        this.Logs.TryAdd(c, Logs);
                        c++;
                    }
                }
            }
        }

        public void ClaimTurf(int RoomId, int TurfFlagId)
        {
            List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfbyRoomList(RoomId);
            if (TF != null && TF.Count > 0)
            {
                TF[0].GangIdOwner = this.Id;
            }

            // Actualizamos el group de la sala y colores de la bandera en DB
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `group_id` = '" + this.Id + "' WHERE `id` = '" + RoomId + "' LIMIT 1");
                dbClient.RunQuery("UPDATE `items_groups` SET `group_id` = '" + this.Id + "' WHERE `id` = '" + TurfFlagId + "' LIMIT 1");
            }
        }

        public List<int> GetMembers
        {
            get { return this._members.ToList(); }
        }

        public List<int> GetRequests
        {
            get { return this._requests.ToList(); }
        }

        public List<int> GetAdministrators
        {
            get { return this._administrators.ToList(); }
        }
        
        public List<int> GetAdministrator
        {
            get { return this._administrators; }
        }

        public List<int> GetAllMembers
        {
            get
            {
                List<int> Members = new List<int>(this._administrators.ToList());
                Members.AddRange(this._members.ToList());
                return Members;
            }
        }

        public ConcurrentDictionary<int, GroupMember> GetAllMembersDict
        {
            get
            {                
                return this.Members;
            }
        }

        public ConcurrentDictionary<int, GroupLogs> GetLogs
        {
            get
            {
                return this.Logs;
            }
        }

        public List<GroupLogs> getAllLogs()
        {
            List<GroupLogs> VH = new List<GroupLogs>();

            foreach (var item in this.Logs)
            {
                VH.Add(item.Value);
            }
            return VH;
        }

        public int MemberCount
        {
            get { return this._members.Count + this._administrators.Count; }
        }

        public int RequestCount
        {
            get { return this._requests.Count; }
        }

        public bool IsMember(int Id)
        {
            return this._members.Contains(Id) || this._administrators.Contains(Id);
        }

        public bool IsAdmin(int Id)
        {
            return this._administrators.Contains(Id);
        }

        public bool IsMemberDict(int UserId)
        {
            return this.Members.ContainsKey(UserId);
        }

        public bool IsAdminDict(int UserId)
        {
            if (!this.Members.ContainsKey(UserId))
                return false;

            return this.Members[UserId].IsAdmin;
        }

        public bool HasRequest(int Id)
        {
            return this._requests.Contains(Id);
        }

        public void MakeAdmin(int Id)
        {
            //if (!this.Ranks.ContainsKey(6))
              //  return;
            
            if(this._administrators.ToList().Count > 0)
            {
                TakeAdmin(this._administrators.ToList()[0]);
            }

            if (this._members.Contains(Id))
                this._members.Remove(Id);

            if (!this.Removable)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE group_memberships SET `rank` = '"+ RoleplayManager.AdminRankGroupsNoRemov +"' WHERE `user_id` = @uid AND `group_id` = @gid LIMIT 1");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();
                }
            }
            
            if (this.GType < 3)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE play_stats SET `corp` = @gid WHERE `id` = @uid LIMIT 1");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();
                }
            }
            else
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE play_stats SET `gang` = @gid WHERE `id` = @uid LIMIT 1");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();
                }
            }

            if (!this._administrators.Contains(Id))
                this._administrators.Add(Id);

            //this.Members[Id].UserRank = 6;
            this.Members[Id].IsAdmin = true;
        }

        public void TakeAdmin(int UserId)
        {
            if (!this._administrators.Contains(UserId))
                return;

            /*
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE group_memberships SET `rank` = '1' WHERE user_id = @uid AND group_id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", UserId);
                dbClient.RunQuery();
            }
            */
            if (this.GType < 3)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE play_stats SET `corp` = '0' WHERE id = @uid AND corp = @gid");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", UserId);
                    dbClient.RunQuery();
                }
            }
            else
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE play_stats SET `gang` = '0' WHERE id = @uid AND gang = @gid");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", UserId);
                    dbClient.RunQuery();
                }
            }

            this._administrators.Remove(UserId);
            this._members.Add(UserId);
            //this.Members[UserId].UserRank = 1;
            this.Members[UserId].IsAdmin = false;
        }

        public void MakeOwner(int Id)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `owner_id` = @uid WHERE `id` = @gid LIMIT 1");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }
        }

        public void AddMember(int Id, int RankId = 1, bool UpdateDatabase = false, int WS_Hours = 0, string WS_Desc = "", string WS_Region = "", GameClient Session = null, bool Invited = false, string Who = "")
        {
            if ((this.IsMember(Id) && this.IsMemberDict(Id)) || this.GroupType == GroupType.LOCKED && this._requests.Contains(Id))
                return;

            GroupMember Member = new GroupMember(this.Id, Id, RankId, GetGroupbyUserId(Id));
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (this.IsAdmin(Id) && this.IsAdminDict(Id))
                {
                    if(this.GType < 3)
                        dbClient.SetQuery("UPDATE `play_stats` SET corp = @gid WHERE id = @uid");
                    else
                        dbClient.SetQuery("UPDATE `play_stats` SET gang = @gid WHERE id = @uid");

                    this._administrators.Remove(Id);
                    this._members.Add(Id);                  

                    this.Members.TryAdd(Id, Member);
                }
                else if (this.GroupType == GroupType.LOCKED/* && this.GetAdministrator.Count != 0*/)
                {
                    if (Session == null)
                    {
                        dbClient.SetQuery("INSERT INTO `group_requests` (user_id, group_id, ws_hours, ws_desc, ws_region, invited, invited_by, timestamp) VALUES (@uid, @gid, @wsh, @wsd, @wsr, @inv, @invb, @tim)");
                        this._requests.Add(Id);
                    }
                    if(Session != null && Session.GetPlay() != null && Session.GetPlay().BuyingCorp)
                    {
                        dbClient.SetQuery("INSERT INTO `group_memberships` (user_id, group_id, type) VALUES (@uid, @gid, @type)");
                        this._members.Add(Id);
                        this.Members.TryAdd(Id, Member);
                        if (this.IsMember(Id))
                        {
                            if (!this.IsAdmin(Id))
                            {
                                this.MakeAdmin(Id);
                            }
                        }
                    }
                }
                else
                {
                    dbClient.SetQuery("INSERT INTO `group_memberships` (user_id, group_id, type) VALUES (@uid, @gid, @type)");
                    this._members.Add(Id);
                    this.Members.TryAdd(Id, Member);
                }
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.AddParameter("wsh", WS_Hours);
                dbClient.AddParameter("wsd", WS_Desc);
                dbClient.AddParameter("wsr", WS_Region);
                dbClient.AddParameter("type", this.GType);
                dbClient.AddParameter("inv", PlusEnvironment.BoolToEnum(Invited));
                dbClient.AddParameter("invb", Who);
                dbClient.AddParameter("tim", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public void DeleteMember(int Id)
        {
            if (IsMemberDict(Id))
            {
                GroupMember Junk;
                this.Members.TryRemove(Id, out Junk);
            }

            if (IsMember(Id))
            {
                if (this._members.Contains(Id))
                    this._members.Remove(Id);
            }
            else if (IsAdmin(Id))
            {
                if (this._administrators.Contains(Id))
                    this._administrators.Remove(Id);
            }
            else
                return;
            if(this.GroupChatEnabled)
            {
                GameClient Client;
                Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);
                if (Client != null)
                {
                    Client.SendMessage(new FriendListUpdateComposer(-this.Id));
                   
                }
                    

            }
            
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM group_memberships WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }

            // NEW
            if (this.GType < 3)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE play_stats SET `corp` = '0' WHERE id = @uid AND corp = @gid");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();
                }
            }
            else
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE play_stats SET `gang` = '0' WHERE id = @uid AND gang = @gid");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();
                }
            }
        }

        public void AddRank(int GroupId, int RankId, string Name, string M_Figure, string F_Figure, int Pay, string[] Commands, string[] WorkRooms, int Limit = 0, int Timer = 5)
        {
            if (PlusEnvironment.GetGame().GetGroupManager().JobExists(GroupId, RankId))
                return;

            GroupRank Rank = new GroupRank(GroupId, RankId, Name, M_Figure, F_Figure, Pay, Commands, WorkRooms, Limit, Timer);

            string sCommands = "";
            string sWorkRooms = "";

            for (int i = 0; i < Commands.Length; i++)
            {
                if(Commands[i].Length > 0)
                    sCommands += Commands[i].ToString() + ",";
            }                

            for (int i = 0; i < WorkRooms.Length; i++)
            {
                if(WorkRooms[i].Length > 0)
                    sWorkRooms += WorkRooms[i].ToString() + ",";
            }                

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `play_jobs_ranks` (`job`, `rank`, `name`, `male_figure`, `female_figure`, `pay`, `commands`, `workrooms`, `limit`, `timer`) VALUES (@job, @rank, @name, @male_figure, @female_figure, @pay, @commands, @workrooms, @limit, @timer)");
                this.Ranks.TryAdd(RankId, Rank);

                dbClient.AddParameter("job", GroupId);
                dbClient.AddParameter("rank", RankId);
                dbClient.AddParameter("name", Name);
                dbClient.AddParameter("male_figure", M_Figure);
                dbClient.AddParameter("female_figure", F_Figure);
                dbClient.AddParameter("pay", Pay);
                dbClient.AddParameter("commands", sCommands);
                dbClient.AddParameter("workrooms", sWorkRooms);
                dbClient.AddParameter("limit", Limit);
                dbClient.AddParameter("timer", Timer);
                dbClient.RunQuery();
            }
        }

        public void AddLog(int Id, string Action, int Cant)
        {
            if (!this.IsMember(Id) && !this.IsMemberDict(Id) && !this.IsAdmin(Id) && !this.IsAdminDict(Id))
                return;

            GroupLogs Log = new GroupLogs(this.Id, Id, Action, Cant, DateTime.Now);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `groups_logs` (user_id, group_id, action, cant, timestamp) VALUES (@uid, @gid, @act, @cant, @tim)");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.AddParameter("act", Action);
                dbClient.AddParameter("cant", Cant);
                dbClient.AddParameter("tim", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();

                this.Logs.TryAdd(this.Logs.Count, Log);
            }
        }

        public void CreateForum(Group group)
        {
            if (group.ForumEnabled)
                return;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `has_forum` = '1' WHERE id = @gid");
                dbClient.AddParameter("gid", group.Id);
                dbClient.RunQuery();
            }
            group.ForumEnabled = true;

        }

        public void UpdateJobBadge(string URL)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `badge` = @url WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("url", URL);
                dbClient.RunQuery();
            }
            this.Badge = URL;
        }

        public void UpdateJobSettings(int RankId, string NewName, int NewPay, int NewTimer)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (!string.IsNullOrEmpty(NewName))
                {
                    dbClient.SetQuery("UPDATE play_jobs_ranks SET `name` = @name, `pay` = @pay, `timer` = @timer WHERE job = @gid AND rank = @rank");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("rank", RankId);
                    dbClient.AddParameter("name", NewName);
                    dbClient.AddParameter("pay", NewPay);
                    dbClient.AddParameter("timer", NewTimer);
                }
				else
				{
                    dbClient.SetQuery("UPDATE play_jobs_ranks SET `pay` = @pay, `timer` = @timer WHERE job = @gid AND rank = @rank");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("rank", RankId);
                    dbClient.AddParameter("pay", NewPay);
                    dbClient.AddParameter("timer", NewTimer);
                }

                dbClient.RunQuery();
            }

            if (!string.IsNullOrEmpty(NewName))
                this.Ranks.ToList().Where(x => x.Value.RankId == RankId).ToList().ForEach(x => x.Value.Name = NewName);

            this.Ranks.ToList().Where(x => x.Value.RankId == RankId).ToList().ForEach(x => x.Value.Pay = NewPay);
            this.Ranks.ToList().Where(x => x.Value.RankId == RankId).ToList().ForEach(x => x.Value.Timer = NewTimer);
        }

        public void UpdateJobLooks(int RankId, string NewFigure, string Gender)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                string table = "male_figure";

                if (Gender == "F")
                    table = "female_figure";

                dbClient.SetQuery("UPDATE play_jobs_ranks SET `"+ table +"` = @figure WHERE job = @gid AND rank = @rank");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("rank", RankId);
                dbClient.AddParameter("figure", NewFigure);
                dbClient.RunQuery();
            }

            if(Gender == "M")
                this.Ranks.ToList().Where(x => x.Value.RankId == RankId).ToList().ForEach(x => x.Value.MaleFigure = NewFigure);
            else
                this.Ranks.ToList().Where(x => x.Value.RankId == RankId).ToList().ForEach(x => x.Value.FemaleFigure = NewFigure);
        }

        public void UpdateJobCommads(int RankId, string NewCommand)
        {
            GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(this.Id, RankId);

            if (Rank == null)
                return;

            string RnwCommands = "";

            for (int i = 0; i < Rank.Commands.Length; i++)
            {
                //Console.WriteLine(i + ": " + Rank.Commands[i]);
                if(Rank.Commands[i].ToString().Length > 0)
                    RnwCommands += Rank.Commands[i].ToString() + ",";
            }

            if (this.Ranks.ToList().Where(x => x.Value.RankId == RankId && x.Value.HasCommand(NewCommand)).Count() > 0)
            {
                // Tiene el permiso. Quitamos del Arrelgo.
                RnwCommands = RnwCommands.Replace(NewCommand + ",", "");
            }
            else
            {
                // No tiene el permiso. Agregarlo al arreglo.
                RnwCommands += NewCommand + ",";
            }

            string[] ArrayCommands = RnwCommands.Split(',');
            this.Ranks.ToList().Where(x => x.Value.RankId == RankId).ToList().ForEach(x => x.Value.Commands = ArrayCommands);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE play_jobs_ranks SET `commands` = @cmds WHERE job = @gid AND rank = @rank");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("rank", RankId);
                dbClient.AddParameter("cmds", RnwCommands);
                dbClient.RunQuery();
            }
        }

        public void ResetCorp()
        {
            SetBussines(RoleplayManager.GangsPrice, 100);

            this.Sells = 0;
            this.Spend = 0;
            this.Profits = 0;

            UpdateSells(0);
            UpdateSpend(0);
            UpdateShifts(0);
            ResetRanks();

            PlusEnvironment.GetGame().GetBusinessBalanceManager().DeleteBalance(this.Id, true);
        }

        public void SetBussines(int bank, int stock)
        {
            this.Bank = bank;
            this.Stock = stock;
            
            this.BankRuptcy = (this.Bank <= ((RoleplayManager.GangsPrice / 4) * -1));

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `bank` = @bank, `stock` = @stock, `bankruptcy` = @br WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("bank", bank);
                dbClient.AddParameter("stock", stock);
                dbClient.AddParameter("br", PlusEnvironment.BoolToEnum(this.BankRuptcy));
                dbClient.RunQuery();
            }
        }

        public void UpdateSpend(int cost)
        {
            this.Spend += cost;
            this.Profits = (this.Sells - this.Spend);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `spend` = @spend, `profits` = @profits WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("spend", this.Spend);
                dbClient.AddParameter("profits", this.Profits);
                dbClient.RunQuery();
            }
        }

        public void UpdateSells(int cost)
        {
            this.Sells += cost;
            this.Profits = (this.Sells - this.Spend);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `sells` = @sells, `profits` = @profits WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("sells", this.Sells);
                dbClient.AddParameter("profits", this.Profits);
                dbClient.RunQuery();
            }
        }

        public void UpdateShifts(int shifts)
        {
            this.Shifts = shifts;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `shifts` = @shifts WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("shifts", this.Shifts);
                dbClient.RunQuery();
            }
        }

        public void UpdateStat(string stat, string value)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `"+stat+"` = '"+value+"' WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.RunQuery();
            }
        }

        public void UpdateStat(string stat, int value)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `" + stat + "` = " + value + " WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.RunQuery();
            }
        }

        public void UpdateJobMember(int UserId)
        {
            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client != null && Client.GetPlay() != null)
            {

                if (!this.Members.ContainsKey(UserId))
                    return;

                this.Members[UserId].UserRank = Client.GetPlay().JobRank;
                this.Members[UserId].IsAdmin = GetGroupbyUserId(UserId);
            }
        }
        public void UpdateInfoJobMember(int UserId)
        {
            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client != null && Client.GetPlay() != null)
            {

                if (!this.Members.ContainsKey(UserId))
                    return;

                this.Members[UserId].UserRank = Client.GetPlay().JobRank;
                this.Members[UserId].IsAdmin = GetGroupbyUserId(UserId);
            }
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE group_memberships SET `type` = @gtype, `rank` = @rank WHERE user_id = @uid AND group_id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", UserId);
                dbClient.AddParameter("gtype", this.GType);
                dbClient.AddParameter("rank", Client.GetPlay().JobRank);
                dbClient.RunQuery();
            }
        }

        public void UpdateGroupName(string NewName)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `name` = @name WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("name", NewName);
                dbClient.RunQuery();
            }
            this.Name = NewName;
        }

        public void ResetRanks()
		{
            UpdateJobSettings(1, "", 25, 10);
            UpdateJobSettings(2, "", 30, 10);
            UpdateJobSettings(3, "", 35, 10);
            UpdateJobSettings(4, "", 40, 10);
            UpdateJobSettings(5, "", 45, 10);
            UpdateJobSettings(6, "", 50, 10);
        }

        public void UpdateGroupAccessType(int AccessType)
        {
            
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `state` = @atype WHERE id = @gid");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("atype", AccessType.ToString());
                dbClient.RunQuery();
            }

            if(AccessType == 1)
                this.GroupType = GroupType.LOCKED;
            else
                this.GroupType = GroupType.OPEN;
        }

        public string GetCommandsbyActivity(string Giro)
        {
            string cmds = "";

            #region Get CMDS
            switch (Giro)
            {
                #region Indefinido
                case "Indefinido":
                    {
                    }
                    break;
                #endregion

                #region Restaurant
                case "Restaurant":
                    {
                        cmds += "serve,food,drink";
                    }
                    break;
                #endregion

                #region Concesionario
                case "Concesionario":
                    {
                        cmds += "concesionario";
                    }
                    break;
                #endregion

                #region Tecnologia
                case "Tecnologia":
                    {
                        cmds += "tecnologia";
                    }
                    break;
                #endregion

                #region Ropa
                case "Ropa":
                    {
                        cmds += "ropa";
                    }
                    break;
                #endregion

                #region Spa
                case "Spa":
                    {
                        cmds += "spa";
                    }
                    break;
                #endregion

                #region 24/7
                case "24/7":
                    {
                        cmds += "24/7";
                    }
                    break;
                #endregion

                #region Default
                default:
                    break;
                #endregion
            }
            #endregion

            return cmds;
        }

        public GroupForum GetForum()
        {
            return this._forum;
        }

        public void CreateGroupChat(Group group)
        {
            if (group.GroupChatEnabled)
                return;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE groups SET `has_groupchat` = '1' WHERE id = @gid");
                dbClient.AddParameter("gid", group.Id);
                dbClient.RunQuery();
            }
            group.GroupChatEnabled = true;
            List<GameClient> GroupMembers = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && this.IsMember(Client.GetHabbo().Id) select Client).ToList();
            foreach (GameClient Client in GroupMembers)
            {
                if (Client == null)
                    continue;
                Client.SendMessage(new FriendListUpdateComposer(-this.Id, this.Id));
            }

        }

        public void HandleRequest(int Id, bool Accepted)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Accepted)
                {
                    dbClient.SetQuery("INSERT INTO group_memberships (user_id, group_id) VALUES (@uid, @gid)");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();

                    this._members.Add(Id);
                    GroupMember Member = new GroupMember(this.Id, Id, 1, false);

                    this.Members.TryAdd(Id, Member);
                    
                    if(PlusEnvironment.GetHabboById(Id) != null && PlusEnvironment.GetHabboById(Id).GetClient() != null)
                        PlusEnvironment.GetHabboById(Id).GetClient().SendMessage(new FriendListUpdateComposer(-this.Id, this.Id));
                }

                dbClient.SetQuery("DELETE FROM group_requests WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }

            if (this._requests.Contains(Id))
                this._requests.Remove(Id);
        }

        public void ClearRequests()
        {
            this._requests.Clear();
        }

        public bool GetGroupbyUserId(int userid)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetData = null;
                dbClient.SetQuery("SELECT `corp`,`gang` FROM `play_stats` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", userid);
                GetData = dbClient.getTable();

                if (GetData != null)
                {
                    foreach (DataRow Row in GetData.Rows)
                    {
                        if (Convert.ToInt32(Row["corp"]) == this.Id)
                            return true;
                        else if (Convert.ToInt32(Row["gang"]) == this.Id)
                            return true;
                    }
                }
            }

            return false;
        }

        public string GetBadge()
		{
            DataRow Data = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `badge` FROM `groups` WHERE `id` = '" + this.Id + "' LIMIT 1");
                Data = dbClient.getRow();

                if (Data != null)
                    return Data["badge"].ToString();
            }
            return string.Empty;
        }

        public void Dispose()
        {
            List<GameClient> GroupMembers = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && this.IsMember(Client.GetHabbo().Id) select Client).ToList();
            foreach (GameClient Client in GroupMembers)
            {
                if (Client == null)
                    continue;
                Client.SendMessage(new FriendListUpdateComposer(-this.Id));
            }

            if (this._forum != null)
            {
                this._forum.Dispose();

                //And finally..
                this._forum = null;
            }
            this._requests.Clear();
            this._members.Clear();
            this._administrators.Clear();
            this.Ranks.Clear();
            this.Members.Clear();

            // Delete badge from swf
            string filePath = RoleplayManager.SWFPath + "\\habbo-imaging\\gang\\" + this.Id + ".gif";
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
}

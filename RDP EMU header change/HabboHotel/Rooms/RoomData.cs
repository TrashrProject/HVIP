using System;
using System.Collections.Generic;
using System.Data;

using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using System.Globalization;
using System.Linq;

namespace Plus.HabboHotel.Rooms
{
    public class RoomData
    {
        public bool TurfCapturing = false;
        public int TurfUserAtackerId = 0;

        public int Id;
        public int AllowPets;
        public int AllowPetsEating;
        public int RoomBlockingEnabled;
        public int Category;
        public string Description;
        public string Floor;
        public int FloorThickness;
        public Group Group;
        public int Hidewall;
        public string Landscape;
        public string ModelName;
        public string Name;
        public string OwnerName;
        public int OwnerId;
        public string Password;
        public int Score;
        public RoomAccess Access;
        public List<string> Tags;
        public string Type;
        public int UsersMax;
        public int UsersNow;
        public int WallThickness;
        public string Wallpaper;
        public int WhoCanBan;
        public int WhoCanKick;
        public int WhoCanMute;
        private RoomModel mModel;
        public int chatMode;
        public int chatSpeed;
        public int chatSize;
        public int extraFlood;
        public int chatDistance;

        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordDay;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordWeek;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordMonth;
        public List<int> WiredScoreFirstBordInformation = new List<int>();


        public int TradeSettings;//Default = 2;

        public RoomPromotion _promotion;

        public bool PushEnabled;
        public bool PullEnabled;
        public bool SPushEnabled;
        public bool SPullEnabled;
        public bool EnablesEnabled;
        public bool RespectNotificationsEnabled;
        public bool PetMorphsAllowed;
        public bool HideWired;

        // RP Vars
        public string City;
        public int TaxiNode;
        public string[] JobsInside;
        public int DoorOrientation;
        public int DoorX;
        public int DoorY;
        public double DoorZ;
        public bool BankEnabled;
        public bool ShootEnabled;
        public bool HitEnabled;
        public bool SafeZoneEnabled;
        public bool SexCommandsEnabled;
        public bool TurfEnabled;
        public bool RobEnabled;
        public bool GymEnabled;
        public bool DriveEnabled;
        public string EnterRoomMessage;
        public bool BuyCarEnabled;
        public bool SupermarketEnabled;
        public bool MunicipalidadEnabled;
        public bool NoHungerEnabled;
        public bool IsHospital;
        public bool IsPrison;
        public bool IsPrison2;
        public bool IsPolStation;
        public bool IsCourt;
        public bool IsCamionero;
        public bool IsMecanico;
        public bool IsBasurero;
        public bool IsMinero;
        public bool IsArmero;
        public bool MallEnabled;
        public bool FerreteriaEnabled;
        public bool GrangeEnabled;
        public bool GasEnabled;
        public bool RobStoreEnabled;
        public bool WardrobeEnabled;
        public bool PhoneStoreEnabled;
        public bool IsSanc;

        public void Fill(DataRow Row)
        {
            Id = Convert.ToInt32(Row["id"]);
            Name = Convert.ToString(Row["caption"]);
            Description = Convert.ToString(Row["description"]);
            Type = Convert.ToString(Row["roomtype"]);
            OwnerId = Convert.ToInt32(Row["owner"]);

            OwnerName = "";
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @owner LIMIT 1");
                dbClient.AddParameter("owner", OwnerId);
                string result = dbClient.getString();
                if (!String.IsNullOrEmpty(result))
                    OwnerName = result;
            }

            this.Access = RoomAccessUtility.ToRoomAccess(Row["state"].ToString().ToLower());

            Category = Convert.ToInt32(Row["category"]);
            if (!string.IsNullOrEmpty(Row["users_now"].ToString()))
                UsersNow = Convert.ToInt32(Row["users_now"]);
            else
                UsersNow = 0;
            UsersMax = Convert.ToInt32(Row["users_max"]);
            ModelName = Convert.ToString(Row["model_name"]);
            Score = Convert.ToInt32(Row["score"]);
            Tags = new List<string>();
            AllowPets = Convert.ToInt32(Row["allow_pets"].ToString());
            AllowPetsEating = Convert.ToInt32(Row["allow_pets_eat"].ToString());
            RoomBlockingEnabled = Convert.ToInt32(Row["room_blocking_disabled"].ToString());
            Hidewall = Convert.ToInt32(Row["allow_hidewall"].ToString());
            Password = Convert.ToString(Row["password"]);
            Wallpaper = Convert.ToString(Row["wallpaper"]);
            Floor = Convert.ToString(Row["floor"]);
            Landscape = Convert.ToString(Row["landscape"]);
            FloorThickness = Convert.ToInt32(Row["floorthick"]);
            WallThickness = Convert.ToInt32(Row["wallthick"]);
            WhoCanMute = Convert.ToInt32(Row["mute_settings"]);
            WhoCanKick = Convert.ToInt32(Row["kick_settings"]);
            WhoCanBan = Convert.ToInt32(Row["ban_settings"]);
            chatMode = Convert.ToInt32(Row["chat_mode"]);
            chatSpeed = Convert.ToInt32(Row["chat_speed"]);
            chatSize = Convert.ToInt32(Row["chat_size"]);
            TradeSettings = Convert.ToInt32(Row["trade_settings"]);

            Group G = null;
            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Convert.ToInt32(Row["group_id"]), out G))
                Group = G;
            else
                Group = null;

            foreach (string Tag in Row["tags"].ToString().Split(','))
            {
                Tags.Add(Tag);
            }

            mModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(ModelName);

            this.PushEnabled = PlusEnvironment.EnumToBool(Row["push_enabled"].ToString());
            this.PullEnabled = PlusEnvironment.EnumToBool(Row["pull_enabled"].ToString());
            this.SPushEnabled = PlusEnvironment.EnumToBool(Row["spush_enabled"].ToString());
            this.SPullEnabled = PlusEnvironment.EnumToBool(Row["spull_enabled"].ToString());
            this.EnablesEnabled = PlusEnvironment.EnumToBool(Row["enables_enabled"].ToString());
            this.RespectNotificationsEnabled = PlusEnvironment.EnumToBool(Row["respect_notifications_enabled"].ToString());
            this.PetMorphsAllowed = PlusEnvironment.EnumToBool(Row["pet_morphs_allowed"].ToString());
            this.HideWired = PlusEnvironment.EnumToBool(Row["hide_wired"].ToString());

            WiredScoreBordDay = new Dictionary<int, KeyValuePair<int, string>>();
            WiredScoreBordWeek = new Dictionary<int, KeyValuePair<int, string>>();
            WiredScoreBordMonth = new Dictionary<int, KeyValuePair<int, string>>();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                List<bool> SuperCheck = new List<bool>()
                {
                    false,
                    false,
                    false
                };

                DateTime now = DateTime.Now;
                int getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
                int getmonthtoday = Convert.ToInt32(now.ToString("MM"));
                int getweektoday = CultureInfo.GetCultureInfo("Nl-nl").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                this.WiredScoreFirstBordInformation = new List<int>()
                {
                    getdaytoday,
                    getmonthtoday,
                    getweektoday
                };

                dbClient.SetQuery("SELECT * FROM wired_scorebord WHERE roomid = @id ORDER BY `punten` DESC ");
                dbClient.AddParameter("id", this.Id);
                foreach (DataRow row in dbClient.getTable().Rows)
                {
                    int userid = Convert.ToInt32(row["userid"]);
                    string username = Convert.ToString(row["username"]);
                    int Punten = Convert.ToInt32(row["punten"]);
                    string soort = Convert.ToString(row["soort"]);
                    int timestamp = Convert.ToInt32(row["timestamp"]);
                    if ((!(soort == "day") || this.WiredScoreBordDay.ContainsKey(userid) ? false : !SuperCheck[0]))
                    {
                        if (timestamp != getdaytoday)
                        {
                            SuperCheck[0] = false;
                        }
                        if (!SuperCheck[0])
                        {
                            this.WiredScoreBordDay.Add(userid, new KeyValuePair<int, string>(Punten, username));
                        }
                    }
                    if ((!(soort == "month") || this.WiredScoreBordMonth.ContainsKey(userid) ? false : !SuperCheck[1]))
                    {
                        if (timestamp != getmonthtoday)
                        {
                            SuperCheck[1] = false;
                        }
                        this.WiredScoreBordMonth.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                    if ((!(soort == "week") || this.WiredScoreBordWeek.ContainsKey(userid) ? false : !SuperCheck[2]))
                    {
                        if (timestamp != getweektoday)
                        {
                            SuperCheck[2] = false;
                        }
                        this.WiredScoreBordWeek.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                }
                if (SuperCheck[0])
                {
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", this.Id, "' AND `soort`='day'"));
                    this.WiredScoreBordDay.Clear();
                }
                if (SuperCheck[1])
                {
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", this.Id, "' AND `soort`='month'"));
                    this.WiredScoreBordMonth.Clear();
                }
                if (SuperCheck[2])
                {
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", this.Id, "' AND `soort`='week'"));
                    this.WiredScoreBordDay.Clear();
                }
            }
        }
        public void FillRP(DataRow Row)
        {
            this.City = Row["city"].ToString();
            this.TaxiNode = Convert.ToInt32(Row["taxi_node"]);
            this.JobsInside = Row["jobs_inside"].ToString().Split(',');
            this.DoorOrientation = Convert.ToInt32(Row["door_dir"]);
            this.DoorX = Convert.ToInt32(Row["door_x"]);
            this.DoorY = Convert.ToInt32(Row["door_y"]);
            this.DoorZ = Convert.ToDouble(Row["door_z"]);
            this.BankEnabled = PlusEnvironment.EnumToBool(Row["bank_enabled"].ToString());
            this.ShootEnabled = PlusEnvironment.EnumToBool(Row["shoot_enabled"].ToString());
            this.HitEnabled = PlusEnvironment.EnumToBool(Row["hit_enabled"].ToString());
            this.SafeZoneEnabled = PlusEnvironment.EnumToBool(Row["safezone_enabled"].ToString());
            this.SexCommandsEnabled = PlusEnvironment.EnumToBool(Row["sexcommands_enabled"].ToString());
            this.TurfEnabled = PlusEnvironment.EnumToBool(Row["turf_enabled"].ToString());
            this.RobEnabled = PlusEnvironment.EnumToBool(Row["rob_enabled"].ToString());
            this.GymEnabled = PlusEnvironment.EnumToBool(Row["gym_enabled"].ToString());
            this.DriveEnabled = PlusEnvironment.EnumToBool(Row["drive_enabled"].ToString());
            this.EnterRoomMessage = Convert.ToString(Row["enter_message"]);
            this.BuyCarEnabled = PlusEnvironment.EnumToBool(Row["buycar_enabled"].ToString());
            this.SupermarketEnabled = PlusEnvironment.EnumToBool(Row["supermarket_enabled"].ToString());
            this.MunicipalidadEnabled = PlusEnvironment.EnumToBool(Row["municip_enabled"].ToString());
            this.NoHungerEnabled = PlusEnvironment.EnumToBool(Row["nohunger_enabled"].ToString());
            this.IsHospital = PlusEnvironment.EnumToBool(Row["is_hospital"].ToString());
            this.IsPrison = PlusEnvironment.EnumToBool(Row["is_prison"].ToString());
            this.IsPrison2 = PlusEnvironment.EnumToBool(Row["is_prisonback"].ToString());
            this.IsPolStation = PlusEnvironment.EnumToBool(Row["is_polstation"].ToString());
            this.IsCourt = PlusEnvironment.EnumToBool(Row["is_court"].ToString());
            this.IsCamionero = PlusEnvironment.EnumToBool(Row["is_camionero"].ToString());
            this.IsMecanico = PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString());
            this.IsBasurero = PlusEnvironment.EnumToBool(Row["is_basurero"].ToString());
            this.IsMinero = PlusEnvironment.EnumToBool(Row["is_minero"].ToString());
            this.IsArmero = PlusEnvironment.EnumToBool(Row["is_armero"].ToString());
            this.MallEnabled = PlusEnvironment.EnumToBool(Row["mall_enabled"].ToString());
            this.FerreteriaEnabled = PlusEnvironment.EnumToBool(Row["ferreteria_enabled"].ToString());
            this.GrangeEnabled = PlusEnvironment.EnumToBool(Row["grange_enabled"].ToString());
            this.GasEnabled = PlusEnvironment.EnumToBool(Row["gas_enabled"].ToString());
            this.RobStoreEnabled = PlusEnvironment.EnumToBool(Row["robstore_enabled"].ToString());
            this.WardrobeEnabled = PlusEnvironment.EnumToBool(Row["wardrobe_enabled"].ToString());
            this.PhoneStoreEnabled = PlusEnvironment.EnumToBool(Row["phonestore_enabled"].ToString());
            this.IsSanc = PlusEnvironment.EnumToBool(Row["is_sanc"].ToString());
        }
        public RoomPromotion Promotion
        {
            get { return this._promotion; }
            set { this._promotion = value; }
        }

        public bool HasActivePromotion
        {
            get { return this.Promotion != null; }
        }

        public void EndPromotion()
        {
            if (!this.HasActivePromotion)
                return;

            this.Promotion = null;
        }

        public RoomModel Model
        {
            get
            {
                if (mModel == null)
                    mModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(ModelName);
                return mModel;
            }
        }
    }
}
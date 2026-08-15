using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;
using Plus.Database.Interfaces;
using System.Data;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.RolePlay.PlayRoom
{
    public class PlayRoomManager
    {
        public readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.RolePlay.PlayRoom");
        public readonly Dictionary<string, PlayRoom> HospitalRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> JailRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> JailRooms2 = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> CourtRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> SancRooms = new Dictionary<string, PlayRoom>();

        // Jobs Zones
        public readonly Dictionary<string, PlayRoom> PolStationRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> CamionerosRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> MecanicosRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> BasurerosRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> MinerosRooms = new Dictionary<string, PlayRoom>();
        public readonly Dictionary<string, PlayRoom> ArmerosRooms = new Dictionary<string, PlayRoom>();

        public PlayRoomManager()
        {

        }
        public void Init()
        {
            this.HospitalRooms.Clear();
            this.JailRooms.Clear();
            this.JailRooms2.Clear();
            this.CourtRooms.Clear();
            this.SancRooms.Clear();
            this.PolStationRooms.Clear();
            this.CamionerosRooms.Clear();
            this.MecanicosRooms.Clear();
            this.BasurerosRooms.Clear();
            this.MinerosRooms.Clear();
            this.ArmerosRooms.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_hospital` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.HospitalRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_prison` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.JailRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_prisonback` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.JailRooms2.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_court` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.CourtRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_sanc` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.SancRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }

            #region Jobs Zones
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_camionero` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.CamionerosRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_mecanico` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.MecanicosRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_basurero` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.BasurerosRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_minero` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.MinerosRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_armero` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.ArmerosRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_rooms` WHERE `is_polstation` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.PolStationRooms.Add(Convert.ToString(Row["city"]), new PlayRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PlusEnvironment.EnumToBool(Row["is_hospital"].ToString()), PlusEnvironment.EnumToBool(Row["is_prison"].ToString()), PlusEnvironment.EnumToBool(Row["is_court"].ToString()), PlusEnvironment.EnumToBool(Row["is_camionero"].ToString()), PlusEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PlusEnvironment.EnumToBool(Row["is_basurero"].ToString()), PlusEnvironment.EnumToBool(Row["is_minero"].ToString()), PlusEnvironment.EnumToBool(Row["is_armero"].ToString()), PlusEnvironment.EnumToBool(Row["is_polstation"].ToString()), PlusEnvironment.EnumToBool(Row["is_sanc"].ToString())));
                    }
                }
            }
            #endregion


            log.Info("Loaded " + this.HospitalRooms.Count + " Hospital(s).");
            log.Info("Loaded " + this.JailRooms.Count + " Prison(s).");
            log.Info("Loaded " + this.JailRooms2.Count + " Prisonback(s).");
            log.Info("Loaded " + this.CourtRooms.Count + " Court(s).");
            log.Info("Loaded " + this.SancRooms.Count + " Sanc Room(s).");
            log.Info("Loaded " + this.PolStationRooms.Count + " Police Station(s).");
            log.Info("Loaded " + this.CamionerosRooms.Count + " Camioneros Zone(s).");
            log.Info("Loaded " + this.MecanicosRooms.Count + " Mecanicos Zone(s).");
            log.Info("Loaded " + this.BasurerosRooms.Count + " Basureros Zone(s).");
            log.Info("Loaded " + this.MinerosRooms.Count + " Mineros Zone(s).");
            log.Info("Loaded " + this.ArmerosRooms.Count + " Armeros Zone(s).");


        }

        public int TryToGetHospital(string city, out PlayRoom Room)
        {
            
            if(this.HospitalRooms.TryGetValue(city, out Room))
            {
                return this.HospitalRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetJail(string city, out PlayRoom Room)
        {

            if (this.JailRooms.TryGetValue(city, out Room))
            {
                return this.JailRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetJailBack(string city, out PlayRoom Room)
            {

                if (this.JailRooms2.TryGetValue(city, out Room))
                {
                    return this.JailRooms2[city].Id;
                }
                else
                {
                    return 0;
                }
            }
        public int TryToGetCourt(string city, out PlayRoom Room)
        {

            if (this.CourtRooms.TryGetValue(city, out Room))
            {
                return this.CourtRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetSanc(string city, out PlayRoom Room)
        {

            if (this.SancRooms.TryGetValue(city, out Room))
            {
                return this.SancRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetCamioneros(string city, out PlayRoom Room)
        {

            if (this.CamionerosRooms.TryGetValue(city, out Room))
            {
                return this.CamionerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetPolStation(string city, out PlayRoom Room)
        {

            if (this.PolStationRooms.TryGetValue(city, out Room))
            {
                return this.PolStationRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetMecanicos(string city, out PlayRoom Room)
        {

            if (this.MecanicosRooms.TryGetValue(city, out Room))
            {
                return this.MecanicosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetBasureros(string city, out PlayRoom Room)
        {

            if (this.BasurerosRooms.TryGetValue(city, out Room))
            {
                return this.BasurerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetMineros(string city, out PlayRoom Room)
        {

            if (this.MinerosRooms.TryGetValue(city, out Room))
            {
                return this.MinerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetArmeros(string city, out PlayRoom Room)
        {

            if (this.ArmerosRooms.TryGetValue(city, out Room))
            {
                return this.ArmerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
    }
}
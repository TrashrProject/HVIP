using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Timers;
using System;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Weapons;
using System.Data;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Items.Data.RentableSpace;
using Plus.HabboRoleplay.VehiclesJobs;
using System.Linq;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.RolePlay.PlayRoom;
using System.Text;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.Cache;
using Plus.HabboRoleplay.Products;
using Plus.HabboRoleplay.ProductOwned;
using log4net;
using Plus.HabboRoleplay.PhoneAppOwned;
using Plus.HabboRoleplay.PhonesApps;
using Plus.HabboRoleplay.PhoneOwned;
using Plus.HabboRoleplay.Phones;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.TaxiRoomNodes;
using Plus.Utilities;

namespace Plus.HabboRoleplay.Misc
{
    public class RoleplayManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.RoleplayManager");

        public static ConcurrentDictionary<int, Wanted> WantedList = new ConcurrentDictionary<int, Wanted>();
        private static readonly object itemobj = new object();

        #region PlayData
        // API
        public static string APIUrl = "https://" + Convert.ToString(RoleplayData.GetData("api", "apiurl"));
        public static string APIPath = @"C:" + Convert.ToString(RoleplayData.GetData("ws", "apipath"));

        // WS
        public static string CDNSWF = "https://" + Convert.ToString(RoleplayData.GetData("ws", "cdnswf"));
        public static string SWFPath = @"C:" + Convert.ToString(RoleplayData.GetData("ws", "swfpath"));
        public static string CMSPath = @"C:" + Convert.ToString(RoleplayData.GetData("ws", "cmspath"));
        // Para imagenes que manda en HTML con WS
        public static string CdnURL = Convert.ToString(RoleplayData.GetData("ws", "cdnurl"));
        public static string PhoneAppsPath = @"C:" + Convert.ToString(RoleplayData.GetData("ws", "phoneappspath"));
        public static string PhoneAppsUrl = Convert.ToString(RoleplayData.GetData("ws", "phoneappsurl"));

        // Timer
        public static int DeathTime = Convert.ToInt32(RoleplayData.GetData("timer", "deathtime"));// min
        public static int DyingTime = Convert.ToInt32(RoleplayData.GetData("timer", "dyingtime"));// min
        public static int HungerTime = Convert.ToInt32(RoleplayData.GetData("timer", "hungertime"));// segs
        public static int HygieneTime = Convert.ToInt32(RoleplayData.GetData("timer", "hygienetime"));// segs
        public static int TimeForRobHouses = Convert.ToInt32(RoleplayData.GetData("timer", "timeforrobhouses"));// segs
        public static int MaxForceHouseTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxforcehousetime"));// segs
        public static int ATMRobTime = Convert.ToInt32(RoleplayData.GetData("timer", "atmrobtime"));// segs
        public static int MinBanChNTime = Convert.ToInt32(RoleplayData.GetData("timer", "minbanchntime"));// min
        public static int MaxBanChNTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxbanchntime"));// min
        public static int CuffedTime = Convert.ToInt32(RoleplayData.GetData("timer", "cuffedtime"));// min
        public static int CamCargTime = Convert.ToInt32(RoleplayData.GetData("timer", "camcargtime"));// segs
        public static int CamDepositTime = Convert.ToInt32(RoleplayData.GetData("timer", "camdeposittime"));// segs
        public static int MinMinerTime = Convert.ToInt32(RoleplayData.GetData("timer", "minminertime"));// segs
        public static int MaxMinerTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxminertime"));// segs
        public static int MinMecTime = Convert.ToInt32(RoleplayData.GetData("timer", "minmectime"));// segs
        public static int MaxMecTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxmectime"));// segs
        public static int MinBasuTime = Convert.ToInt32(RoleplayData.GetData("timer", "minbasutime"));// segs
        public static int MaxBasuTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxbasutime"));// segs
        public static int SembrarTime = Convert.ToInt32(RoleplayData.GetData("timer", "sembrartime"));// segs
        public static int RegarTime = Convert.ToInt32(RoleplayData.GetData("timer", "regartime"));// segs
        public static int HealHospitalTime = Convert.ToInt32(RoleplayData.GetData("timer", "healhospitaltime"));// min
        public static int WantedTime = Convert.ToInt32(RoleplayData.GetData("timer", "wantedtime"));// min
        public static int DefaultJailTime = Convert.ToInt32(RoleplayData.GetData("timer", "defaultjailtime"));// min
        public static int StarsJailTime = Convert.ToInt32(RoleplayData.GetData("timer", "starsjailtime"));// min
        public static int Star0Time = Convert.ToInt32(RoleplayData.GetData("timer", "star0time"));// min
        public static int StunTime = Convert.ToInt32(RoleplayData.GetData("timer", "stuntime"));// segs
        public static int TaxiTime = Convert.ToInt32(RoleplayData.GetData("timer", "taxitime"));// segs
        public static int VehicleJobTime = Convert.ToInt32(RoleplayData.GetData("timer", "vehiclejobtime"));// segs
        public static int VehicleJobPoliTime = Convert.ToInt32(RoleplayData.GetData("timer", "vehiclejobpolitime"));// segs
        public static int WorkoutTime = Convert.ToInt32(RoleplayData.GetData("timer", "workouttime"));// segs
        public static int TurfCapTime = Convert.ToInt32(RoleplayData.GetData("timer", "turfcaptime"));// segs
        public static int PurgeTime = Convert.ToInt32(RoleplayData.GetData("timer", "purgetime"));// segs
        public static int CallTaxiTime = Convert.ToInt32(RoleplayData.GetData("timer", "calltaxitime"));// segs
        public static int PSVTime = Convert.ToInt32(RoleplayData.GetData("timer", "psvtime"));// segs
        // Level
        public static bool LevelDifference = Convert.ToBoolean(RoleplayData.GetData("server", "leveldifference"));
        // Combat
        public static int DefaultHitCooldown = Convert.ToInt32(RoleplayData.GetData("combat", "defaulthitcooldown"));
        // Police
        public static int StunGunRange = Convert.ToInt32(RoleplayData.GetData("police", "stungunrange"));
        // Hospital
        public static int PayRightJob = Convert.ToInt32(RoleplayData.GetData("hospital", "payrightjob"));
        public static int PayWrongJob = Convert.ToInt32(RoleplayData.GetData("hospital", "paywrongjob"));
        public static int AmbulSave = Convert.ToInt32(RoleplayData.GetData("hospital", "ambulsave"));
        // Strength
        public static int StrengthCap = Convert.ToInt32(RoleplayData.GetData("strength", "cap"));
        // Server
        public static bool ConfiscateWeapons = Convert.ToBoolean(RoleplayData.GetData("server", "confiscateweapons"));
        // Límite de materiales a comprar (Solo hay 1 punto de venta con un pack de 50);
        // De agregarse más, incrementar el contador.
        public static int ArmMatLimit = Convert.ToInt32(RoleplayData.GetData("server", "armmatlimit"));
        public static int ArmMatPrice = Convert.ToInt32(RoleplayData.GetData("server", "armmatprice"));
        public static int MaxATMRobMoney = Convert.ToInt32(RoleplayData.GetData("server", "maxatmrobmoney"));
        // Rango máximo perteneciente a Admin para Trabajos GType 1 NO removibles.
        public static int AdminRankGroupsNoRemov = Convert.ToInt32(RoleplayData.GetData("server", "adminrankgroupsnoremov"));
        public static string DefaultWebPage = Convert.ToString(RoleplayData.GetData("server", "defaultwebpage"));
        public static int LastTutorialStep = Convert.ToInt32(RoleplayData.GetData("server", "lasttutorialstep"));// is the complete tutorial. If add more frank dialogs, increase this.
        public static int PurgeBonif = Convert.ToInt32(RoleplayData.GetData("server", "purgebonif"));
        public static int ClothPrice = Convert.ToInt32(RoleplayData.GetData("server", "clothprice"));
        public static int RestTips = Convert.ToInt32(RoleplayData.GetData("server", "resttips"));
        public static int TaxiCost = Convert.ToInt32(RoleplayData.GetData("server", "taxicost"));
        public static int TaxiCostJobs = Convert.ToInt32(RoleplayData.GetData("server", "taxicostjobs"));
        public static int SeedsPrice = Convert.ToInt32(RoleplayData.GetData("server", "seedsprice"));
        public static int RepairKitPrice = Convert.ToInt32(RoleplayData.GetData("server", "repairkitprice"));
        public static int GrangePay = Convert.ToInt32(RoleplayData.GetData("server", "grangepay"));
        public static int BailCost = Convert.ToInt32(RoleplayData.GetData("server", "bailcost"));
        public static int FuelPrice = Convert.ToInt32(RoleplayData.GetData("server", "fuelprice"));
        public static int MinerPay = Convert.ToInt32(RoleplayData.GetData("server", "minerpay"));
        public static int ChangeNameCost = Convert.ToInt32(RoleplayData.GetData("server", "changenamecost"));
        // Gangs
        public static int GangsPrice = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsprice"));
        public static int GangsTurfBonif = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsturfbonif"));
        public static int GangsClaimTurfBonif = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsclaimturfbonif"));
        public static int GangsMaxMembers = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsmaxmembers"));
        #endregion

        // IDS de Productos de inventario de play_products
        public static int WeedID = 0;
        public static int CocaineID = 0;
        public static int MedicinesID = 0;
        public static int BidonID = 0;
        public static int MecPartsID = 0;
        public static int ArmMatID = 0;
        public static int ArmPiecesID = 0;

        /// <summary>
        /// Global RP System Timer Manager
        /// </summary>
        public static SystemTimerManager TimerManager = new SystemTimerManager();
        public static string TaxiBotCSV = @"extra\taxibot.csv"; // Cerebro del taxi bot
        public static string TaxiLook = "hd-3102-1.hr-889-38.ha-3409-66.ch-3077-66.cc-3186-66.lg-285-110.sh-290-110";
        public static string AVATARIMG = "https://nitro-imager.kubbo.city/?figure=";
        public static int ItemId = 10000000;
        public static int ChatsID = 0;
        public static int VehiclesOwnedID = 10000000;// Para Autos CORP
        public static bool PurgeEvent = false;
        public static int TaxiBotsId = 10000000;
        public static bool PreLoadedRooms = false;

        #region PlayData
        public static void ReloadData()
        {
            // API
            APIUrl = "https://" + Convert.ToString(RoleplayData.GetData("api", "apiurl"));

            // WS
            CDNSWF = "https://" + Convert.ToString(RoleplayData.GetData("ws", "cdnswf"));
            SWFPath = @"C:" + Convert.ToString(RoleplayData.GetData("ws", "swfpath"));
            // Para imagenes que manda en HTML con WS
            CdnURL = Convert.ToString(RoleplayData.GetData("ws", "cdnurl"));
            PhoneAppsPath = @"C:" + Convert.ToString(RoleplayData.GetData("ws", "phoneappspath"));
            PhoneAppsUrl = Convert.ToString(RoleplayData.GetData("ws", "phoneappsurl"));

            // Timer
            DeathTime = Convert.ToInt32(RoleplayData.GetData("timer", "deathtime"));// min
            DyingTime = Convert.ToInt32(RoleplayData.GetData("timer", "dyingtime"));// min
            HungerTime = Convert.ToInt32(RoleplayData.GetData("timer", "hungertime"));// segs
            HygieneTime = Convert.ToInt32(RoleplayData.GetData("timer", "hygienetime"));// segs
            TimeForRobHouses = Convert.ToInt32(RoleplayData.GetData("timer", "timeforrobhouses"));// segs
            MaxForceHouseTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxforcehousetime"));// segs
            ATMRobTime = Convert.ToInt32(RoleplayData.GetData("timer", "atmrobtime"));// segs
            MinBanChNTime = Convert.ToInt32(RoleplayData.GetData("timer", "minbanchntime"));// min
            MaxBanChNTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxbanchntime"));// min
            CuffedTime = Convert.ToInt32(RoleplayData.GetData("timer", "cuffedtime"));// min
            CamCargTime = Convert.ToInt32(RoleplayData.GetData("timer", "camcargtime"));// segs
            CamDepositTime = Convert.ToInt32(RoleplayData.GetData("timer", "camdeposittime"));// segs
            MinMinerTime = Convert.ToInt32(RoleplayData.GetData("timer", "minminertime"));// segs
            MaxMinerTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxminertime"));// segs
            MinMecTime = Convert.ToInt32(RoleplayData.GetData("timer", "minmectime"));// segs
            MaxMecTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxmectime"));// segs
            MinBasuTime = Convert.ToInt32(RoleplayData.GetData("timer", "minbasutime"));// segs
            MaxBasuTime = Convert.ToInt32(RoleplayData.GetData("timer", "maxbasutime"));// segs
            SembrarTime = Convert.ToInt32(RoleplayData.GetData("timer", "sembrartime"));// segs
            RegarTime = Convert.ToInt32(RoleplayData.GetData("timer", "regartime"));// segs
            HealHospitalTime = Convert.ToInt32(RoleplayData.GetData("timer", "healhospitaltime"));// min
            WantedTime = Convert.ToInt32(RoleplayData.GetData("timer", "wantedtime"));// min
            DefaultJailTime = Convert.ToInt32(RoleplayData.GetData("timer", "defaultjailtime"));// min
            StarsJailTime = Convert.ToInt32(RoleplayData.GetData("timer", "starsjailtime"));// min
            Star0Time = Convert.ToInt32(RoleplayData.GetData("timer", "star0time"));// min
            StunTime = Convert.ToInt32(RoleplayData.GetData("timer", "stuntime"));// segs
            TaxiTime = Convert.ToInt32(RoleplayData.GetData("timer", "taxitime"));// segs
            VehicleJobTime = Convert.ToInt32(RoleplayData.GetData("timer", "vehiclejobtime"));// segs
            VehicleJobPoliTime = Convert.ToInt32(RoleplayData.GetData("timer", "vehiclejobpolitime"));// segs
            WorkoutTime = Convert.ToInt32(RoleplayData.GetData("timer", "workouttime"));// segs
            TurfCapTime = Convert.ToInt32(RoleplayData.GetData("timer", "turfcaptime"));// segs
            PurgeTime = Convert.ToInt32(RoleplayData.GetData("timer", "purgetime"));// segs
            CallTaxiTime = Convert.ToInt32(RoleplayData.GetData("timer", "calltaxitime"));// segs
            PSVTime = Convert.ToInt32(RoleplayData.GetData("timer", "psvtime"));// segs
            // Level
           LevelDifference = Convert.ToBoolean(RoleplayData.GetData("server", "leveldifference"));
            // Combat
            DefaultHitCooldown = Convert.ToInt32(RoleplayData.GetData("combat", "defaulthitcooldown"));
            // Police
            StunGunRange = Convert.ToInt32(RoleplayData.GetData("police", "stungunrange"));
            // Hospital
            PayRightJob = Convert.ToInt32(RoleplayData.GetData("hospital", "payrightjob"));
            PayWrongJob = Convert.ToInt32(RoleplayData.GetData("hospital", "paywrongjob"));
            AmbulSave = Convert.ToInt32(RoleplayData.GetData("hospital", "ambulsave"));
            // Strength
            StrengthCap = Convert.ToInt32(RoleplayData.GetData("strength", "cap"));
            // Server
            ConfiscateWeapons = Convert.ToBoolean(RoleplayData.GetData("server", "confiscateweapons"));
            // Límite de materiales a comprar (Solo hay 1 punto de venta con un pack de 50);
            // De agregarse más, incrementar el contador.
            ArmMatLimit = Convert.ToInt32(RoleplayData.GetData("server", "armmatlimit"));
            ArmMatPrice = Convert.ToInt32(RoleplayData.GetData("server", "armmatprice"));
            MaxATMRobMoney = Convert.ToInt32(RoleplayData.GetData("server", "maxatmrobmoney"));
            // Rango máximo perteneciente a Admin para Trabajos GType 1 NO removibles.
            AdminRankGroupsNoRemov = Convert.ToInt32(RoleplayData.GetData("server", "adminrankgroupsnoremov"));
            DefaultWebPage = Convert.ToString(RoleplayData.GetData("server", "defaultwebpage"));
            LastTutorialStep = Convert.ToInt32(RoleplayData.GetData("server", "lasttutorialstep"));// is the complete tutorial. If add more frank dialogs, increase this.
            PurgeBonif = Convert.ToInt32(RoleplayData.GetData("server", "purgebonif"));
            ClothPrice = Convert.ToInt32(RoleplayData.GetData("server", "clothprice"));
            RestTips = Convert.ToInt32(RoleplayData.GetData("server", "resttips"));
            TaxiCost = Convert.ToInt32(RoleplayData.GetData("server", "taxicost"));
            TaxiCostJobs = Convert.ToInt32(RoleplayData.GetData("server", "taxicostjobs"));
            SeedsPrice = Convert.ToInt32(RoleplayData.GetData("server", "seedsprice"));
            RepairKitPrice = Convert.ToInt32(RoleplayData.GetData("server", "repairkitprice"));
            GrangePay = Convert.ToInt32(RoleplayData.GetData("server", "grangepay"));
            BailCost = Convert.ToInt32(RoleplayData.GetData("server", "bailcost"));
            FuelPrice = Convert.ToInt32(RoleplayData.GetData("server", "fuelprice"));
            MinerPay = Convert.ToInt32(RoleplayData.GetData("server", "minerpay"));
            ChangeNameCost = Convert.ToInt32(RoleplayData.GetData("server", "changenamecost"));
            // Gangs
            GangsPrice = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsprice"));
            GangsTurfBonif = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsturfbonif"));
            GangsClaimTurfBonif = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsclaimturfbonif"));
            GangsMaxMembers = Convert.ToInt32(RoleplayData.GetData("gangs", "gangsmaxmembers"));
        }
        #endregion
        public static void SendUserOriginal(GameClient Client, int RID, string Message = "")
        {
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RID);
            if (Client != null && roomData != null)
            {
                Client.GetPlay().AntiArrowCheck = true;

                if (Client.GetHabbo().InRoom)
                {
                    Room OldRoom = null;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Client.GetHabbo().CurrentRoomId, out OldRoom))
                        return;

                    if (OldRoom.GetRoomUserManager() != null)
                        OldRoom.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);

                }

                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, false, true));
                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, true, false));

                if (Message != "")
                    Client.SendNotification(Message);
            }
            else
            {
                Client.SendNotification("[Error][100] -> Lamentablemente ha habido un error al mandarte a la zona solicitada, porfavor comunicate con el administrador/dueño del servidor explicando con detalles de lo ocurrido. ¡Gracias!");
                return;
            }
        }

        // This is the same function that 'SendUserOriginal' but this is called and the other is a backup.
        public static void SendUserOld(GameClient Client, int RID, string Message = "")
        {
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RID);
            if (Client != null && roomData != null)
            {
                Client.GetPlay().AntiArrowCheck = true;

                if (Client.GetHabbo().InRoom)
                {
                    Room OldRoom = null;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Client.GetHabbo().CurrentRoomId, out OldRoom))
                        return;

                    if (OldRoom.GetRoomUserManager() != null)
                        OldRoom.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);

                }

                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, false, true));
                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, true, false));

                if (!string.IsNullOrEmpty(Message))
                    Client.SendNotification(Message);                
            }
            else
            {
                Client.SendNotification("[Error][100] -> Lamentablemente ha habido un error al mandarte a la zona solicitada, porfavor comunicate con el administrador/dueño del servidor explicando con detalles de lo ocurrido. ¡Gracias!");
                return;
            }
        }

        public static void SendUser(GameClient Client, int RID, string Message = "")
        {
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RID);
            if (Client != null && roomData != null)
            {
                Client.GetPlay().AntiArrowCheck = true;

                if (Client.GetHabbo().InRoom)
                {
                    Room OldRoom = null;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Client.GetHabbo().CurrentRoomId, out OldRoom))
                        return;

                    if (OldRoom.GetRoomUserManager() != null)
                        OldRoom.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);

                    // New JDN
                    OldRoom.GetRoomItemHandler().ClearItems(Client);
                    OldRoom.GetRoomUserManager().ClearUsers(Client);
                }

                //Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, false, true));
                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, true, false));

                // New JDN
                #region Habbo=>PrepareRoom
                Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(RID);
                if (Room == null)
                {
                    Client.SendMessage(new CloseConnectionComposer());
                    return;
                }

                if (Room.isCrashed)
                {
                    Client.SendNotification("Esta sala está corrompida :(");
                    Client.SendMessage(new CloseConnectionComposer());
                    return;
                }

                if (!Room.GetRoomUserManager().AddAvatarToRoom(Client))
                {
                    Room.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);
                    return;
                }

                Client.GetHabbo().CurrentRoomId = Room.RoomId;
                #endregion

                #region Taxi Bot
                if(Client.GetHabbo().TaxiChofer > 0 && Client.GetRoomUser() != null)
                {
                    Client.GetPlay().TaxiLastIndex++;
                    RoleplayManager.TaxiBotsId++;
                    List<RandomSpeech> BotSpeechList = new List<RandomSpeech>();

                    RoomUser BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(
                        RoleplayManager.TaxiBotsId,
                        Client.GetHabbo().CurrentRoomId,
                        "taxibot",
                        "stand",
                        "Taxi#" + Client.GetHabbo().Username,
                        "",
                        RoleplayManager.TaxiLook,
                        Client.GetRoomUser().X,
                        Client.GetRoomUser().Y,
                        Client.GetRoomUser().Z,
                        Client.GetRoomUser().RotBody,
                        0,
                        0,
                        0,
                        0,
                        ref BotSpeechList,
                        "",
                        0,
                        0,
                        false,
                        30,
                        false,
                        2,
                        Room.RoomData.TaxiNode), null);
                    BotUser.ApplyEffect(EffectsList.TaxiChofer);
                    BotUser.FastWalking = true;
                    Room.GetGameMap().UpdateUserMovement(new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y), new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y), BotUser);

                    Client.GetHabbo().TaxiChofer = BotUser.BotData.BotId;
                }
                #endregion

                Client.GetHabbo().HomeRoom = Room.Id;
                Client.GetPlay().InState = false;

                #region Habbo=>EntrerRoom
                if (Room.Wallpaper != "0.0")
                    Client.SendMessage(new RoomPropertyComposer("wallpaper", Room.Wallpaper));
                if (Room.Floor != "0.0")
                    Client.SendMessage(new RoomPropertyComposer("floor", Room.Floor));

                Client.SendMessage(new RoomPropertyComposer("landscape", Room.Landscape));

                if (Room.OwnerId != Client.GetHabbo().Id)
                    Client.GetHabbo().GetStats().RoomVisits += 1;

                #endregion

                Room.GetGameMap().GenerateMaps();
                Room.SendObjects(Client);

                #region GetRoomEntryDataEvent
                #region CONDITIONS RP BY JEIHDEN 

                #region DeathCheck
                if (Client.GetPlay().IsDead)
                {
                    string MyCity = Room.City;

                    PlayRoom Data;
                    int ToRoomId = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);

                    if (Room.Id != ToRoomId)
                    {
                        AddedToRoom(Client, Room, "death", ToRoomId);
                        return;
                    }
                }
                #endregion

                #region DyingCheck

                if (Client.GetPlay().IsDying)
                {
                    if (Room.Id != Client.GetHabbo().HomeRoom)
                    {
                        AddedToRoom(Client, Room, "dying", Client.GetHabbo().HomeRoom);
                        return;
                    }
                }

                #endregion

                #region JailCheck

                #endregion

                #region SancCheck
                if (Client.GetPlay().IsSanc)
                {
                    string MyCity = Room.City;

                    PlayRoom Data;
                    int ToRoomId = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetSanc(MyCity, out Data);

                    if (Room.Id != ToRoomId)
                    {
                        AddedToRoom(Client, Room, "sanc", ToRoomId);
                        return;
                    }
                }
                #endregion

                #region Tutorial Step Check
                if (Client.GetPlay().TutorialStep == 13 && Room.WardrobeEnabled && Room.Type.Equals("public"))
                {
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|13");
                }
                else if (Client.GetPlay().TutorialStep == 18 && Room.PhoneStoreEnabled && Room.Type.Equals("public"))
                {
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|18");
                }
                else if (Client.GetPlay().TutorialStep == 23 && Room.BuyCarEnabled && Room.Type.Equals("public"))
                {
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|24");
                }
                else if (Client.GetPlay().TutorialStep == 27 && Room.MallEnabled && Room.Type.Equals("public"))
                {
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|28");
                }
                #endregion

                #endregion

                Client.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));

                RoomUser ThisUser = null;

                if (Client.GetHabbo() != null)
                    ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Username);

                if (ThisUser != null && Client.GetHabbo().PetId == 0)
                    Room.SendMessage(new UserChangeComposer(ThisUser, false));

                Client.SendMessage(new RoomEventComposer(Room.RoomData, Room.RoomData.Promotion));

                if (Room.GetWired() != null)
                    Room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, Client.GetHabbo());

                foreach (RoomUser Bot in Room.GetRoomUserManager().GetBots().ToList())
                {
                    if (Bot.IsBot || Bot.IsPet)
                        Bot.BotAI.OnUserEnterRoom(ThisUser);
                }

                if (PlusEnvironment.GetUnixTimestamp() < Client.GetHabbo().FloodTime && Client.GetHabbo().FloodTime != 0)
                    Client.SendMessage(new FloodControlComposer((int)Client.GetHabbo().FloodTime - (int)PlusEnvironment.GetUnixTimestamp()));
                #endregion

                #region GetGuestRoomEvent
                #region Clean Websockets (Al cambiar de sala)

                #region Groups
                // WS Groups
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "close");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "turf_cap_off");
                #endregion

                #endregion
                #endregion

                if (Message != "")
                    Client.SendNotification(Message);

                Room.GetGameMap().GenerateMaps();
            }
            else
            {
                Client.SendNotification("[Error][100] -> Lamentablemente ha habido un error al mandarte a la zona solicitada, porfavor comunicate con el administrador/dueño del servidor explicando con detalles de lo ocurrido. ¡Gracias!");
                return;
            }
        }

        public static void SendUserTimer(GameClient Client, int RID, string Message = "", string Timer = "")
        {
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RID);
            if (Client != null && roomData != null)
            {
                Client.GetPlay().AntiArrowCheck = true;

                if (Client.GetHabbo().InRoom)
                {
                    Room OldRoom = null;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Client.GetHabbo().CurrentRoomId, out OldRoom))
                        return;

                    if (OldRoom.GetRoomUserManager() != null)
                        OldRoom.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);
                }

                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, false, true));
                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, true, false));

                if (Message != "")
                    Client.SendNotification(Message);

                if (Timer != "" && Timer != null)
                {
                    Client.GetPlay().TimerManager.CreateTimer(Timer, 1000, true);
                }
            }
            else
            {
                Client.SendNotification("[Error][101] -> Lamentablemente ha habido un error al mandarte a la zona solicitada, porfavor comunicate con el administrador/dueño del servidor explicando con detalles de lo ocurrido. ¡Gracias!");
                return;
            }
        }

        public static void SendPassenger(GameClient Client, GameClient Chofer, Room Room, int RID, string Message = "")
        {
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RID);
            if (Client != null && roomData != null)
            {
                Client.GetPlay().AntiArrowCheck = true;

                if (Client.GetHabbo().InRoom)
                {
                    Room OldRoom = null;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Client.GetHabbo().CurrentRoomId, out OldRoom))
                        return;

                    if (OldRoom.GetRoomUserManager() != null)
                        OldRoom.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);
                }

                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, false, true));
                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, true, false));

                if (Message != "")
                    Client.SendNotification(Message);

                Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(Client.GetRoomUser(), new Point(Chofer.GetRoomUser().X, Chofer.GetRoomUser().Y), 0, Room.GetGameMap().SqAbsoluteHeight(Chofer.GetRoomUser().X, Chofer.GetRoomUser().Y)));
            }
            else
            {
                Client.SendNotification("[Error][100] -> Lamentablemente ha habido un error al mandarte a la zona solicitada, porfavor comunicate con el administrador/dueño del servidor explicando con detalles de lo ocurrido. ¡Gracias!");
                return;
            }
        }

        public static void AddedToRoom(GameClient Session, Room Room, string Type, int RoomId)
        {
            switch (Type)
            {
                case "death":
                    #region Death Check
                    RoleplayManager.SendUserTimer(Session, RoomId, "¡No puedes abandonar el Hospital sin haber sido dad@ de alta!", "death");
                    #endregion
                    break;
                case "dying":
                    #region Dying Check
                    RoleplayManager.SendUserTimer(Session, RoomId, "¡No puedes ir a ningún lado en tu estado actual!", "dying");
                    #endregion
                    break;
                case "sanc":
                    #region Sanc Check
                    RoleplayManager.SendUserTimer(Session, RoomId, "¡No puedes abandonar la sala de sanciones sin completar tu castigo!", "sanc");
                    #endregion
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Generates a shout message based on paramter session
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="Speech"></param>
        /// <param name="Bubble"></param>
        public static void Shout(GameClient Session, string Speech, int Bubble = 0)
        {
            Room Room = null;
            RoomUser User = null;

            if (Speech.StartsWith("*"))
                Speech = "" + Char.ToLowerInvariant(Speech[1]) + Speech.Substring(2);

            if (Session == null || Session.GetHabbo() == null || Session.GetPlay() == null || Session.GetRoomUser() == null)
                return;

            Room = Session.GetHabbo().CurrentRoom;
            User = Session.GetRoomUser();

            if (User != null)
            {
                if (User.GetClient() != null && User.GetClient().GetHabbo() != null)
                {
                    if (Room != null)
                    {
                        //if (!Room.TutorialEnabled)
                        //{
                        User.SendNameColourPacket();
                        User.SendMeCommandPacket();

                        foreach (RoomUser roomUser in Room.GetRoomUserManager().GetRoomUsers())
                        {
                            if (roomUser == null || roomUser.IsBot)
                                continue;

                            if (roomUser.GetClient() == null || roomUser.GetClient().GetConnection() == null)
                                continue;

                            //if (User.GetClient().GetPlay().Invisible)
                            //  if (User.GetClient().GetHabbo().Username != roomUser.GetClient().GetHabbo().Username && !roomUser.GetClient().GetPlay().Invisible)
                            //    continue;

                            roomUser.GetClient().SendMessage(new ShoutComposer(User.VirtualId, Speech, 0, Bubble));
                        }
                        /*}
                        else
                        {
                            User.SendNameColourPacket();
                            User.SendMeCommandPacket();

                            Session.SendMessage(new ShoutComposer(User.VirtualId, Speech, 0, Bubble));
                        }
                        */
                    }
                }
                User.SendNamePacket();
            }
        }

        /// <summary>
        /// Generates room based on roomid
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>
        public static Room GenerateRoom(int RoomId)
        {
            if (PlusEnvironment.GetGame() == null || PlusEnvironment.GetGame().GetRoomManager() == null)
                return null;

            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(RoomId);
            return Room;
        }


        /// <summary>
        /// Gets all vital parts of a users figure
        /// </summary>
        public static string SplitFigure(string Look, string Outfit)
        {
            ConcurrentDictionary<string, string> NewFigure = new ConcurrentDictionary<string, string>();
            string[] Splitted = Look.Split('.');
            string[] Splitted2 = Outfit.Split('.');

            for (int i = 0; i < Splitted.Length; i++)
            {
                string[] SplittedPart = Splitted[i].Split('-');

                if (Splitted == null)
                    continue;

                string BodyPart = SplittedPart[0];
                string Type = SplittedPart[1];
                string Colour;

                if (SplittedPart.Length >= 3)
                    Colour = SplittedPart[2];
                else
                    Colour = "110";

                string SpecificPart = "-" + Type + "-" + Colour;

                if (!NewFigure.ContainsKey(BodyPart))
                    NewFigure.TryAdd(BodyPart, SpecificPart);
                else
                    NewFigure.TryUpdate(BodyPart, SpecificPart, NewFigure[BodyPart]);
            }

            for (int i = 0; i < Splitted2.Length; i++)
            {
                string[] SplittedPart2 = Splitted2[i].Split('-');

                if (Splitted2 == null)
                    continue;

                string BodyPart2 = SplittedPart2[0];
                string Type2 = SplittedPart2[1];
                string Colour2;

                if (SplittedPart2.Length >= 3)
                    Colour2 = SplittedPart2[2];
                else
                    Colour2 = "110";

                string SpecificPart2 = "-" + Type2 + "-" + Colour2;

                if (!NewFigure.ContainsKey(BodyPart2))
                    NewFigure.TryAdd(BodyPart2, SpecificPart2);
                else
                    NewFigure.TryUpdate(BodyPart2, SpecificPart2, NewFigure[BodyPart2]);
            }

            string ReturnFigure = "";

            int count = 0;
            foreach (var Row in NewFigure)
            {
                count++;

                if (NewFigure.Count == count)
                    ReturnFigure += Row.Key + Row.Value;
                else
                    ReturnFigure += Row.Key + Row.Value + ".";
            }

            return ReturnFigure;
        }
        /// <summary>
        /// Generates room based on roomid
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>
        public static Room GenerateRoom(int RoomId, bool BotCheck = false)
        {
            if (PlusEnvironment.GetGame() == null || PlusEnvironment.GetGame().GetRoomManager() == null)
                return null;

            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(RoomId, BotCheck);
            return Room;
        }
        public static void GetLookAndMotto(GameClient Client, string Type = "")
        {
            string WorkLook = "";
            string Look = Client.GetHabbo().Look;
            string Motto = Client.GetHabbo().Motto;
            if (Client.GetHabbo().Gender == null || Client.GetHabbo().Gender == "")
                Client.GetHabbo().Gender = "m";

            string Gender = Client.GetHabbo().Gender;
            

            if (Type.ToLower() == "poof")
            {
                Look = Client.GetPlay().OriginalOutfit;
                Motto = "Ciudadan@";
            }

            int JobId = Client.GetPlay().JobId;
            int JobRank = Client.GetPlay().JobRank;

            Group Group = PlusEnvironment.GetGame().GetGroupManager().GetJob(JobId);
            GroupRank GroupRank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(JobId, JobRank);

            if (Client.GetPlay().IsDead)
            {
                if (Gender.ToLower() == "m")
                    Look = SplitFigure(Look, "lg-280-83.ch-215-83");

                if (Gender.ToLower() == "f")
                    Look = SplitFigure(Look, "lg-710-83.ch-635-83");

                Motto = "[HERID@] Ciudadan@";
            }

            if (Client.GetPlay().IsJailed)
            {
                Random Random = new Random();
                int PrisonNumber = Random.Next(11111, 100000);

                if (Gender.ToLower() == "m")
                    Look = SplitFigure(Look, "lg-280-1323.sh-3016-92.ch-220-1323");

                if (Gender.ToLower() == "f")
                    Look = SplitFigure(Look, "lg-710-1323.sh-3016-92.ch-3067-1323");

                Motto = "[ENCARCELAD@] Preso [#" + PrisonNumber + "]";
            }

            if (Client.GetPlay().IsWorking)
            {
                if (Gender.ToLower() == "m" && GroupRank.MaleFigure != "*")
                    WorkLook = GroupRank.MaleFigure;

                if (Gender.ToLower() == "f" && GroupRank.FemaleFigure != "*")
                    WorkLook = GroupRank.FemaleFigure;
                

                Look = SplitFigure(Look, WorkLook);
                Motto = "[TRABAJANDO] " + Group.Name + " " + GroupRank.Name;
            }

            if (Client.GetPlay().SexTimer > 0)
            {
                if (Gender.ToLower() == "m")
                    Look = SplitFigure(Look, "lg-78322-79.ch-3203-153638.-180-7");

                if (Gender.ToLower() == "f")
                    Look = SplitFigure(Look, "ch-3135-1320.lg-78322-66.-600-1");
            }

            Client.SendMessage(new AvatarAspectUpdateMessageComposer(Look, Gender));

            var RoomUser = Client.GetRoomUser();
            if (RoomUser != null)
            {
                Client.GetHabbo().Look = Look;
                if (RoomUser.IsAsleep)
                    Client.GetHabbo().Motto = "[AFK] " + Motto;
                else
                    Client.GetHabbo().Motto = Motto;

                Client.SendMessage(new UserChangeComposer(RoomUser, true));

                if (Client.GetHabbo().CurrentRoom != null)
                    Client.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
            }
        }

        /// <summary>
        /// Sends the user to desired chair in the room
        /// </summary>
        /// <param name="Client"></param>
        public static void SpawnChairsOld(GameClient Client, string ChairName, RoomUser Bot = null)
        {
            try
            {
                RoomUser RoomUser;

                if (Client != null)
                {
                    if (Client.GetHabbo().CurrentRoomId != 10)//La corte
                    {
                        Client.GetHabbo().Look = Client.GetPlay().OriginalOutfit;
                        Client.GetHabbo().Motto = "Ciudadan@";
                        Client.GetHabbo().Poof(false);
                    }
                }

                if (Bot != null)
                    RoomUser = Bot;
                else
                    RoomUser = Client.GetRoomUser();

                List<Item> Chairs = new List<Item>();

                if (RoomUser == null)
                    return;

                if (RoomUser.isSitting || RoomUser.Statusses.ContainsKey("sit"))
                {
                    if (RoomUser.Statusses.ContainsKey("sit"))
                        RoomUser.RemoveStatus("sit");
                    RoomUser.isSitting = false;
                    RoomUser.UpdateNeeded = true;
                }

                if (RoomUser.isLying || RoomUser.Statusses.ContainsKey("lay"))
                {
                    if (RoomUser.Statusses.ContainsKey("lay"))
                        RoomUser.RemoveStatus("lay");
                    RoomUser.isLying = false;
                    RoomUser.UpdateNeeded = true;
                }

                RoomUser.CanWalk = false;
                RoomUser.ClearMovement(true);

                lock (RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    foreach (Item item in RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                    {
                        if (item.GetBaseItem().ItemName == ChairName)
                        {
                            if (!Chairs.Contains(item))
                            {
                                Chairs.Add(item);
                            }
                        }
                    }

                    var Chairs2 = new List<Item>();
                    foreach (var chair in Chairs)
                    {
                        if (!chair.GetRoom().GetGameMap().SquareHasUsers(chair.GetX, chair.GetY))
                        {
                            if (!Chairs2.Contains(chair))
                                Chairs2.Add(chair);
                        }
                    }

                    Item LandItem = null;
                    Random Random = new Random();
                    if (Chairs2.Count >= 1)
                    {
                        if (Chairs2.Count == 1)
                            LandItem = Chairs2[0];
                        else
                            LandItem = Chairs2[Random.Next(0, Chairs2.Count)];
                    }
                    else if (Chairs2.Count >= 1)
                    {
                        if (Chairs.Count == 1)
                            LandItem = Chairs[0];
                        else
                            LandItem = Chairs[Random.Next(0, Chairs.Count)];
                    }

                    if (LandItem != null)
                    {
                        if (RoomUser.Statusses.ContainsKey("sit"))
                            RoomUser.RemoveStatus("sit");
                        if (RoomUser.Statusses.ContainsKey("lay"))
                            RoomUser.RemoveStatus("lay");
                        RoomUser.Statusses.Add("sit", Utilities.TextHandling.GetString(LandItem.GetBaseItem().Height));

                        Point OldCoord = new Point(RoomUser.Coordinate.X, RoomUser.Coordinate.Y);
                        Point NewCoord = new Point(LandItem.GetX, LandItem.GetY);

                        var Room = GenerateRoom(RoomUser.RoomId);

                        if (Room != null)
                            Room.GetGameMap().UpdateUserMovement(OldCoord, NewCoord, RoomUser);

                        RoomUser.X = LandItem.GetX;
                        RoomUser.Y = LandItem.GetY;
                        RoomUser.Z = LandItem.GetZ;
                        RoomUser.RotHead = LandItem.Rotation;
                        RoomUser.RotBody = LandItem.Rotation;
                    }
                    RoomUser.CanWalk = true;
                    RoomUser.UpdateNeeded = true;
                }
                Console.WriteLine("Hecho");
            }
            catch { Console.WriteLine("No Hecho"); }
        }

        public static void SpawnChairs(GameClient Client, string ChairName, RoomUser Bot = null)
        {
            RoomUser RoomUser;

            if (Bot != null)
                RoomUser = Bot;
            else
                RoomUser = Client.GetRoomUser();

            List<Item> Chairs = new List<Item>();

            if (RoomUser == null)
                return;

            if (RoomUser.isSitting || RoomUser.Statusses.ContainsKey("sit"))
            {
                if (RoomUser.Statusses.ContainsKey("sit"))
                    RoomUser.RemoveStatus("sit");
                RoomUser.isSitting = false;
                RoomUser.UpdateNeeded = true;
            }

            if (RoomUser.isLying || RoomUser.Statusses.ContainsKey("lay"))
            {
                if (RoomUser.Statusses.ContainsKey("lay"))
                    RoomUser.RemoveStatus("lay");
                RoomUser.isLying = false;
                RoomUser.UpdateNeeded = true;
            }

            if (RoomUser != null)
                RoomUser.ClearMovement(true);

            lock (RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
            {
                foreach (Item item in RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().ItemName == ChairName)
                    {
                        if (!Chairs.Contains(item))
                            Chairs.Add(item);
                    }
                }

                var Chairs2 = new List<Item>();
                foreach (var bed in Chairs)
                {
                    if (!bed.GetRoom().GetGameMap().SquareHasUsers(bed.GetX, bed.GetY))
                    {
                        if (!Chairs2.Contains(bed))
                            Chairs2.Add(bed);
                    }
                }
                Item LandItem = null;
                Random Random = new Random();
                if (Chairs2.Count >= 1)
                {
                    if (Chairs2.Count == 1)
                        LandItem = Chairs2[0];
                    else
                        LandItem = Chairs2[Random.Next(0, Chairs2.Count)];
                }
                else if (Chairs.Count >= 1)
                {
                    if (Chairs.Count == 1)
                        LandItem = Chairs[0];
                    else
                        LandItem = Chairs[Random.Next(0, Chairs.Count)];
                }

                if (LandItem != null)
                {
                    if (RoomUser.Statusses.ContainsKey("sit"))
                        RoomUser.RemoveStatus("sit");
                    if (RoomUser.Statusses.ContainsKey("lay"))
                        RoomUser.RemoveStatus("lay");
                    RoomUser.Statusses.Add("sit", Utilities.TextHandling.GetString(LandItem.GetBaseItem().Height) + " null");

                    Point OldCoord = new Point(RoomUser.X, RoomUser.Y);
                    Point NewCoord = new Point(LandItem.GetX, LandItem.GetY);


                    RoomUser.X = LandItem.GetX;
                    RoomUser.Y = LandItem.GetY;
                    RoomUser.Z = LandItem.GetZ;
                    RoomUser.RotHead = LandItem.Rotation;
                    RoomUser.RotBody = LandItem.Rotation;

                    RoomUser.UpdateNeeded = true;
                    RoomUser.GetRoom().GetGameMap().UpdateUserMovement(OldCoord, NewCoord, RoomUser);
                }
            }
        }

        /// <summary>
        /// Sends the user to desired bed in the room
        /// </summary>
        /// <param name="Client"></param>
        public static void SpawnBeds(GameClient Client, string BedName, RoomUser Bot = null)
        {
            RoomUser RoomUser;

            if (Bot != null)
                RoomUser = Bot;
            else
                RoomUser = Client.GetRoomUser();

            List<Item> Beds = new List<Item>();

            if (RoomUser == null)
                return;

            if (RoomUser.isSitting || RoomUser.Statusses.ContainsKey("sit"))
            {
                if (RoomUser.Statusses.ContainsKey("sit"))
                    RoomUser.RemoveStatus("sit");
                RoomUser.isSitting = false;
                RoomUser.UpdateNeeded = true;
            }

            if (RoomUser.isLying || RoomUser.Statusses.ContainsKey("lay"))
            {
                if (RoomUser.Statusses.ContainsKey("lay"))
                    RoomUser.RemoveStatus("lay");
                RoomUser.isLying = false;
                RoomUser.UpdateNeeded = true;
            }

            if (RoomUser != null)
                RoomUser.ClearMovement(true);

            lock (RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
            {
                foreach (Item item in RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().ItemName == BedName)
                    {
                        if (!Beds.Contains(item))
                            Beds.Add(item);
                    }
                }

                var Beds2 = new List<Item>();
                foreach (var bed in Beds)
                {
                    if (!bed.GetRoom().GetGameMap().SquareHasUsers(bed.GetX, bed.GetY))
                    {
                        if (!Beds2.Contains(bed))
                            Beds2.Add(bed);
                    }
                }
                Item LandItem = null;
                Random Random = new Random();
                if (Beds2.Count >= 1)
                {
                    if (Beds2.Count == 1)
                        LandItem = Beds2[0];
                    else
                        LandItem = Beds2[Random.Next(0, Beds2.Count)];
                }
                else if (Beds.Count >= 1)
                {
                    if (Beds.Count == 1)
                        LandItem = Beds[0];
                    else
                        LandItem = Beds[Random.Next(0, Beds.Count)];
                }

                if (LandItem != null)
                {
                    if (RoomUser.Statusses.ContainsKey("sit"))
                        RoomUser.RemoveStatus("sit");
                    if (RoomUser.Statusses.ContainsKey("lay"))
                        RoomUser.RemoveStatus("lay");
                    RoomUser.Statusses.Add("lay", Utilities.TextHandling.GetString(LandItem.GetBaseItem().Height) + " null");

                    Point OldCoord = new Point(RoomUser.X, RoomUser.Y);
                    Point NewCoord = new Point(LandItem.GetX, LandItem.GetY);


                    RoomUser.X = LandItem.GetX;
                    RoomUser.Y = LandItem.GetY;
                    RoomUser.Z = LandItem.GetZ;
                    RoomUser.RotHead = LandItem.Rotation;
                    RoomUser.RotBody = LandItem.Rotation;

                    RoomUser.UpdateNeeded = true;
                    RoomUser.GetRoom().GetGameMap().UpdateUserMovement(OldCoord, NewCoord, RoomUser);
                }
            }
        }

        /// <summary>
        /// Gets the distance between 2 points
        /// </summary>
        public static double GetDistanceBetweenPoints2D(Point From, Point To)
        {
            Vector2D Pos1 = new Vector2D(From.X, From.Y);
            Vector2D Pos2 = new Vector2D(To.X, To.Y);

            double XDistance = Math.Abs(Pos1.X - Pos2.X);
            double YDistance = Math.Abs(Pos1.Y - Pos2.Y);

            if (XDistance == 0 && YDistance == 0)
                return 0;

            if (XDistance == 0)
                return YDistance;

            if (YDistance == 0)
                return XDistance;

            double DiagonalDistance = Math.Sqrt(XDistance * XDistance + YDistance * YDistance);

            return DiagonalDistance;
        }

        /// <summary>
        /// Adds a weapon to the users owned weapons
        /// </summary>
        /// <returns></returns>
        public static void AddWeapon(GameClient Client, Weapon Weapon)
        {
            if (!Client.GetPlay().OwnedWeapons.ContainsKey(Weapon.Name.ToLower()))
            {
                using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.SetQuery("INSERT INTO `play_weapons_owned` (`user_id`,`base_weapon`,`name`,`min_damage`,`max_damage`,`range`,`clip`) VALUES (@userid,@baseweapon,@name,@mindamage,@maxdamage,@range,@clip)");
                    DB.AddParameter("userid", Client.GetHabbo().Id);
                    DB.AddParameter("baseweapon", Weapon.Name.ToLower());
                    DB.AddParameter("name", Weapon.PublicName);
                    DB.AddParameter("mindamage", Weapon.MinDamage);
                    DB.AddParameter("maxdamage", Weapon.MaxDamage);
                    DB.AddParameter("range", Weapon.Range);
                    DB.AddParameter("clip", Weapon.ClipSize);
                    DB.RunQuery();

                    Client.GetPlay().OwnedWeapons.TryAdd(Weapon.Name.ToLower(), Weapon);
                }
            }
            else
            {
                Client.SendWhisper("¡Ya tienes un/a " + Weapon.PublicName + "!", 1);
            }
        }

        public static void AddProduct(GameClient Client, Product Product, string Extradata = "")
        {
            if (!Client.GetPlay().OwnedProducts.ContainsKey(Product.ID))
            {
                using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.SetQuery("INSERT INTO `play_products_owned` (`product_id`, `user_id`, `extradata`) VALUES (@productid, @owner, @extra)");
                    DB.AddParameter("productid", Product.ID);
                    DB.AddParameter("owner", Client.GetHabbo().Id);
                    DB.AddParameter("extra", Extradata);
                    DB.RunQuery();

                    ProductsOwned PO = new ProductsOwned((Client.GetPlay().OwnedProducts.Count + 1), Product.ID, Client.GetHabbo().Id, Extradata);

                    Client.GetPlay().OwnedProducts.TryAdd(Product.ID, PO);
                }
            }
            else
            {
                // Ya tiene el producto, revisamos el max_cant y sumamos en extradata
                int getCant;
                if (!int.TryParse(Client.GetPlay().OwnedProducts[Product.ID].Extradata, out getCant))
                    getCant = Client.GetPlay().OwnedProducts.Where(x => x.Value.ProductId == Product.ID).Count();

                if (getCant >= Product.MaxCant && Product.MaxCant != -1)
                    Client.SendWhisper("¡Ya cuentas con " + Product.MaxCant + " " + Product.DisplayName + " en tu inventario! No es posible tener más a la vez.", 1);
                else
                {
                    int newCant = Convert.ToInt32(Client.GetPlay().OwnedProducts[Product.ID].Extradata) + 1;
                    Client.GetPlay().OwnedProducts[Product.ID].Extradata = newCant.ToString();
                    UpdateMyProductExtrada(Client, Product.ID, newCant.ToString());
                }
            }
        }

        public static void AddPhoneAppOwned(GameClient Client, int AppId, string Extradata = "")
        {
            if (Client.GetPlay().Phone > 0)
            {
                if (!Client.GetPlay().OwnedPhonesApps.ContainsKey(AppId))
                {
                    int id = 0;
                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        List<Phone> mPH = PhoneManager.getPhoneById(Client.GetPlay().PhoneModelId);
                        if (mPH != null)
                        {
                            int MaxApps = mPH[0].ScreenSlots - mPH[0].DockSlots;
                            int LastSlot = 0;
                            int LastScreen = 0;

                            #region Get the LastScreen & LastSlot
                            foreach (KeyValuePair<int, PhonesAppsOwned> App in Client.GetPlay().OwnedPhonesApps.ToList().OrderByDescending(S => S.Value.ScreenId))
                            {
                                LastScreen = App.Value.ScreenId;
                                break;
                            }

                            foreach (KeyValuePair<int, PhonesAppsOwned> App in Client.GetPlay().OwnedPhonesApps.ToList().Where(x => x.Value.ScreenId == LastScreen).OrderByDescending(S => S.Value.SlotId))
                            {
                                LastSlot = App.Value.SlotId + 1;
                                break;
                            }

                            if(LastSlot > MaxApps)
                            {
                                LastSlot = 0;
                                LastScreen++;
                            }
                            #endregion


                            DB.SetQuery("INSERT INTO `play_phones_apps_owned` (`phone_id`, `app_id`, `screen_id`, `slot_id`, `extradata`) VALUES (@phoneid, @appid, @screenid @slotid, @extradata)");
                            DB.AddParameter("phoneid", Client.GetPlay().Phone);
                            DB.AddParameter("appid", AppId);
                            DB.AddParameter("screenid", LastScreen);
                            DB.AddParameter("slotid", LastSlot);
                            DB.AddParameter("extradata", Extradata);
                            DB.RunQuery();

                            PhonesAppsOwned PO = new PhonesAppsOwned(id, Client.GetPlay().Phone, AppId, LastScreen, LastSlot, Extradata);
                            Client.GetPlay().OwnedPhonesApps.TryAdd(AppId, PO);
                        }
                    }
                }
            }
        }

        public static void SetDefaultApps(GameClient Client)
        {
            if (Client.GetPlay().Phone > 0)
            {
                int id = 0;
                using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    List<Phone> mPH = PhoneManager.getPhoneById(Client.GetPlay().PhoneModelId);
                    if (mPH != null)
                    {
                        // MYSQL PROCEDIMIENTO
                        DB.RunQuery("CALL `SetDefaultApps`(" + Client.GetPlay().Phone + ");");

                        int LastScreen = 1;
                        int LastSlot = 1;
                        for (int AppId = 1; AppId <= 18; AppId++)
                        {
                            id++;

                            PhonesAppsOwned PO = new PhonesAppsOwned(id, Client.GetPlay().Phone, AppId, LastScreen, LastSlot, "");
                            Client.GetPlay().OwnedPhonesApps.TryAdd(AppId, PO);

                            LastSlot++;

                            if (AppId == 14)
                            {
                                LastScreen = 0;
                                LastSlot = 1;
                            }
                        }
                    }
                }
            }
        }

        #region AddVehicle OFF
        /// <summary>
        /// Adds a Vehicles to the users owned vehicles
        /// </summary>
        /// <returns></returns>
        /*
        public static void AddVehicle(GameClient Client, Vehicle Vehicle)
        {
            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("INSERT INTO `play_vehicles_owned` (`furni_id`,`item_id`,`owner`,`last_user`,`model`,`fuel`,`km`,`state`,`traba`,`alarm`,`location`,`baul`,`baul_state`) VALUES (@furnidi,@itemid,@owner,@lastuser,@model,@fuel,@km,@state,@traba,@alarm,@location,@baul,@baul_state)");
                DB.AddParameter("furnidi", 0);
                DB.AddParameter("itemid", Vehicle.ItemID);
                DB.AddParameter("owner", Client.GetHabbo().Id);
                DB.AddParameter("lastuser", Client.GetHabbo().Id);
                DB.AddParameter("model", Vehicle.Model);
                DB.AddParameter("fuel", Vehicle.MaxFuel);
                DB.AddParameter("km", 0);
                DB.AddParameter("state", "0");
                DB.AddParameter("traba", "0");
                DB.AddParameter("alarm", "0");
                DB.AddParameter("location", 0);
                DB.AddParameter("location", 0);
                DB.AddParameter("baul", "");
                DB.AddParameter("baul_state", "0");
                //x y z pasan por defecto
                DB.RunQuery();

                Client.GetPlay().OwnedVehicles.TryAdd(Vehicle.Model.ToLower(), Vehicle);
            }
        }
        */
        #endregion

        #region AddVehicleCorp OFF
        /*
        public static void AddVehicleCorp(GameClient Client, Vehicle Vehicle, int newfurniid)
        {
            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("INSERT INTO `play_vehicles_owned` (`furni_id`,`item_id`,`owner`,`last_user`,`model`,`fuel`,`km`,`state`,`traba`,`alarm`,`location`,`baul`,`baul_state`) VALUES (@furnidi,@itemid,@owner,@lastuser,@model,@fuel,@km,@state,@traba,@alarm,@location,@baul,@baul_state)");
                DB.AddParameter("furnidi", newfurniid);
                DB.AddParameter("itemid", Vehicle.ItemID);
                DB.AddParameter("owner", 0);
                DB.AddParameter("lastuser", Client.GetHabbo().Id);
                DB.AddParameter("model", Vehicle.Model);
                DB.AddParameter("fuel", Vehicle.MaxFuel);
                DB.AddParameter("km", 0);
                DB.AddParameter("state", "0");
                DB.AddParameter("traba", "0");
                DB.AddParameter("alarm", "0");
                DB.AddParameter("location", 0);
                DB.AddParameter("baul", "");
                DB.AddParameter("baul_state", "0");
                //x y z pasan por defecto
                DB.RunQuery();

                Client.GetPlay().OwnedVehicles.TryAdd(Vehicle.Model.ToLower(), Vehicle);
            }
        }
        */
        #endregion

        #region UpdateVehicle OFF
        /*
        public static void UpdateVehicle(GameClient Client, int id, int furniid, int fuel, int km, int state, int traba, int location, int x, int y, double z)
        {
            int OldBaul = 0;
            DataRow Search;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("SELECT furni_id FROM play_vehicles_owned WHERE id = '" + id + "'");
                Search = dbClient.getRow();
                if (Search != null)
                {
                    OldBaul = Convert.ToInt32(Search["furni_id"]);
                }
            }
            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("UPDATE `play_vehicles_owned` SET `furni_id` = @furniid, `owner` = @owner, `last_user` = @lastuser, `fuel` = @fuel, `km` = @km, `state` = @state, `traba` = @traba, `alarm` = @alarm, `location` = @location, `x` = @x, `y` = @y, `z` = @z, `CamCargID` = @cargaid, `baul` = @baul, `baul_state` = @baul_state WHERE `play_vehicles_owned`.`id` = @id");
                DB.AddParameter("id", id);
                DB.AddParameter("furniid", furniid);
                DB.AddParameter("owner", Client.GetPlay().CarLastOWn);
                DB.AddParameter("lastuser", Client.GetHabbo().Id);
                DB.AddParameter("fuel", fuel);
                DB.AddParameter("km", km);
                DB.AddParameter("state", "" + state);
                DB.AddParameter("traba", "" + traba);
                DB.AddParameter("alarm", PlusEnvironment.BoolToEnum(Client.GetPlay().CarAlarm));
                DB.AddParameter("x", x);
                DB.AddParameter("y", y);
                DB.AddParameter("z", z);
                DB.AddParameter("location", location);
                DB.AddParameter("cargaid", Client.GetPlay().CamCargID);
                DB.AddParameter("baul", Client.GetPlay().CarBaul);
                DB.AddParameter("baul_state", "" + Client.GetPlay().CarBaulState);
                DB.RunQuery();
            }
            if (OldBaul > 0)
            {
                using (var DB2 = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB2.SetQuery("UPDATE `play_weapons_owned` SET `baul_car_id` = @baul WHERE `play_weapons_owned`.`baul_car_id` = @oldbaul");
                    DB2.AddParameter("baul", furniid);
                    DB2.AddParameter("oldbaul", OldBaul);
                    DB2.RunQuery();
                }
            }
        }
        */
        #endregion

        #region BuyVehicle OFF
        /*
        public static void BuyVehicle(GameClient Client, int furniid, int fuel, int km, int state, int traba, int location, int x, int y, double z)
        {
            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("INSERT INTO `play_vehicles_owned` (`id`, `furni_id`, `item_id`, `owner`, `last_user`, `model`, `fuel`, `km`, `state`, `traba`, `alarm`, `location`, `x`, `y`, `z`, `baul`, `baul_state`, `CamCargID`) VALUES (NULL, @furniid, @itemid, @owner, @lastuser, @model, @fuel, @km, @state, @traba, @alarm, @location, @x, @y, @z, @baul, @baul_state, @cargaid);");
                DB.AddParameter("furniid", furniid);
                DB.AddParameter("itemid", Client.GetPlay().CarFurniId);
                DB.AddParameter("owner", Client.GetPlay().CarLastOWn);
                DB.AddParameter("lastuser", Client.GetHabbo().Id);
                DB.AddParameter("model", Client.GetPlay().CarModel);
                DB.AddParameter("fuel", fuel);
                DB.AddParameter("km", km);
                DB.AddParameter("state", "" + state);
                DB.AddParameter("traba", "" + traba);
                DB.AddParameter("alarm", "0");
                DB.AddParameter("x", x);
                DB.AddParameter("y", y);
                DB.AddParameter("z", z);
                DB.AddParameter("location", location);
                DB.AddParameter("cargaid", 0);
                DB.AddParameter("baul", "");
                DB.AddParameter("baul_state", "0");
                DB.RunQuery();
            }
            if (Client.GetPlay().DrivingInCar)
            {
                int NewCarId = 0;
                DataRow Search;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("SELECT id FROM play_vehicles_owned WHERE owner = '" + Client.GetHabbo().Id + "' ORDER BY id DESC LIMIT 1");
                    Search = dbClient.getRow();
                    if (Search != null)
                    {
                        NewCarId = Convert.ToInt32(Search["id"]);
                    }
                    Client.GetPlay().CarId = NewCarId;
                }
            }
        }
        */
        #endregion
        public static void PickItem(GameClient Client, int furni_id, int newroom = 0)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + furni_id + "' LIMIT 1");
            }

            Room Room = null;

            if (Client != null && Client.GetRoomUser() != null && newroom <= 0)
                Room = GenerateRoom(Client.GetRoomUser().RoomId);
            else
                Room = GenerateRoom(newroom);

            if (Room != null)
                Room.GetRoomItemHandler().RemoveFurniture(null, furni_id);            
        }
        /* OLD
        public static void PickItem(GameClient Client, int furni_id)
        {
            Room Room = null;

            if (Client != null && Client.GetRoomUser() != null)
                Room = GenerateRoom(Client.GetRoomUser().RoomId);

            if (Client != null)
            {
                Client.SendMessage(new FurniListUpdateComposer());

                if (Room != null)
                    Room.GetRoomItemHandler().RemoveFurniture(Client, furni_id);

                Client.GetHabbo().GetInventoryComponent().RemoveItem(furni_id);
            }

            DataRow Clean;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("SELECT id FROM items WHERE id = '" + furni_id + "'");
                Clean = dbClient.getRow();
                if (Clean != null)
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + furni_id + "' LIMIT 1");
                }
            }
        }
        */

        public static void ClearCorpCar(GameClient Client, int furni_id)
        {
            DataRow Clean;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("SELECT furni_id FROM play_vehicles_owned WHERE furni_id = '" + furni_id + "'");
                Clean = dbClient.getRow();
                if (Clean != null)
                {
                    dbClient.RunQuery("DELETE FROM `play_vehicles_owned` WHERE `furni_id` = '" + furni_id + "'");
                }
            }
        }
        #region LeaveMyTruck OFF
        /*
        public static void LeaveMyTruck(GameClient Client)
        {
            DataRow Clean;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("SELECT furni_id FROM play_vehicles_owned WHERE owner = '"+ Client.GetHabbo().Id +"' AND CamCargID > '0' ");
                Clean = dbClient.getRow();
                if (Clean != null)
                {
                    dbClient.RunQuery("DELETE FROM `play_vehicles_owned` WHERE owner = '" + Client.GetHabbo().Id + "' AND CamCargID > '0' ");
                }
            }
        }
        */
        #endregion

        /// <summary>
        /// Lets you place a furni in the desired location
        /// </summary>
        ///       
        public static Item PlaceItemToRoom(GameClient Session, int BaseId, int GroupId, int X, int Y, double Z, int Rot, bool FromInventory, int roomid, bool ToDB = true, string ExtraData = "", bool IsFood = false, string deliverytype = "")
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                Room Room = GenerateRoom(roomid, false);
                int IdItem = 0;

                if (BaseId == 9436)// Zanahorias
                {
                    ItemId++;
                    IdItem = ItemId;
                }
                else
                {
                    if (ToDB)
                    {
                        dbClient.SetQuery("INSERT INTO items (user_id,base_item,room_id) VALUES (0, " + BaseId + ", " + roomid + ")");
                        dbClient.RunQuery();
                        dbClient.SetQuery("SELECT id FROM items WHERE user_id = '0' AND room_id = '" + roomid + "' AND base_item = '" + BaseId + "' ORDER BY id DESC LIMIT 1");
                        IdItem = dbClient.getInteger();
                    }
                    else
                    {
                        ItemId++;
                        IdItem = ItemId;
                    }
                }

                Item NewItem = new Item(IdItem, roomid, BaseId, ExtraData, X, Y, Z, Rot, 0, GroupId, 0, 0, string.Empty, Room);

                if (NewItem != null)
                    NewItem.DeliveryType = deliverytype;

                if (IsFood == true && NewItem != null)
                {
                    if (Session != null)
                        NewItem.InteractingUser = Session.GetHabbo().Id;

                }

                if (NewItem != null)
                    Room.GetRoomItemHandler().SetFloorItem(Session, NewItem, X, Y, Rot, true, false, true);
                return NewItem;
            }
        }

        public static Item PlaceItemToRoomRP(GameClient Session, int BaseId, int GroupId, int X, int Y, double Z, int Rot, bool FromInventory, int roomid, bool ToDB = true, string ExtraData = "", bool IsFood = false, string deliverytype = "", RentableSpaceData House = null)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                Room Room = GenerateRoom(roomid, false);
                int ItDemId = 0;

                if (House != null)
                    ItemId = PlusEnvironment.GetGame().GetHouseManager().SignMultiplier + House.RoomId;
               /* else if (FarmingSpace != null)
                    ItemId = FarmingManager.SignMultiplier + FarmingSpace.Id;*/
                else if (ToDB)
                {
                    dbClient.SetQuery("INSERT INTO items (user_id,base_item,room_id) VALUES (1, " + BaseId + ", " + roomid + ")");
                    dbClient.RunQuery();
                    dbClient.SetQuery("SELECT id FROM items WHERE user_id = '1' AND room_id = '" + roomid + "' AND base_item = '" + BaseId + "' ORDER BY id DESC LIMIT 1");
                    ItDemId = dbClient.getInteger();
                    ItemId = ItDemId;
                }
                else
                {
                    //while (Room.GetRoomItemHandler().GetFloor.Where(x => x.Id == ItemId).ToList().Count > 0)
                    ItemId++;
                }

                Item NewItem = new Item(ItemId, Room.RoomId, BaseId, ExtraData, X, Y, Z, Rot, 0, GroupId, 0, 0, "", null, House);
                NewItem.DeliveryType = deliverytype;

                /*
                if (NewItem != null && NewItem.FarmingData != null && NewItem.GetBaseItem().InteractionType == InteractionType.FARMING && Session != null && Session.GetHabbo() != null)
                {
                    NewItem.FarmingData.OwnerId = Session.GetHabbo().Id;

                    new Thread(() =>
                    {
                        if (NewItem != null && NewItem.FarmingData != null)
                            NewItem.FarmingData.BeingFarmed = true;

                        Thread.Sleep(3000);

                        if (Session != null && NewItem != null && NewItem.FarmingData != null)
                        {
                            Session.SendWhisper("The " + NewItem.GetBaseItem().PublicName + " you just planted is ready to be watered!", 1);
                            NewItem.FarmingData.BeingFarmed = false;
                        }
                    }).Start();
                }
                */
                if (IsFood == true)
                {
                    if (Session != null)
                    {
                        NewItem.InteractingUser = Session.GetHabbo().Id;
                        Session = null;
                    }
                }

                if (NewItem != null)
                    Room.GetRoomItemHandler().SetFloorItemRP(Session, NewItem, X, Y, Rot, true, false, true, false, false, null, Z, true);

                return NewItem;
            }
        }

        public static Item PutItemToRoom(GameClient Session, int ItemId, int roomid, int BaseItem, int X, int Y, int Rotation, bool ToDB = false)
        {
            Room Room = GenerateRoom(roomid, false);

            Item RoomItem = new Item(ItemId, Room.RoomId, BaseItem, "", X, Y, 0, Rotation, /*Session.GetHabbo().Id*/0, 0, 0, 0, string.Empty, Room);
            if (ToDB)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT id FROM items WHERE id = '"+ ItemId + "'");
                    if (dbClient.getInteger() <= 0)
                    {
                        dbClient.SetQuery("INSERT INTO items (id,user_id,room_id,base_item) VALUES (" + ItemId + ", 0, " + roomid + ", " + BaseItem + ")");
                        dbClient.RunQuery();
                    }
                }
            }
            if (Room.GetRoomItemHandler().SetFloorItem(Session, RoomItem, X, Y, Rotation, true, false, true))
                return RoomItem;
            else
            {
                return null;
            }
        }

        public static void UpdateBankBalance(GameClient Session)
        {
            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("UPDATE `play_stats` SET `bank` = @money WHERE `play_stats`.`id` = @id");
                DB.AddParameter("id", Session.GetHabbo().Id);
                DB.AddParameter("money", Session.GetPlay().Bank);
                DB.RunQuery();
                // Agregar UpdateBalance WS
            }
        }
        public static void UpdateCreditsBalance(GameClient Session)
        {
            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("UPDATE `users` SET `credits` = @money WHERE `users`.`id` = @id");
                DB.AddParameter("id", Session.GetHabbo().Id);
                DB.AddParameter("money", Session.GetHabbo().Credits);
                DB.RunQuery();
                // Agregar UpdateBalance WS
                Session.GetPlay().UpdateInteractingUserDialogues();
                Session.GetPlay().RefreshStatDialogue();
            }
        }
        public static void SaveQuickStat(GameClient Session, string Stat, string Newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_stats SET " + Stat + " = '" + Newval + "' WHERE id = " + Session.GetHabbo().Id + "");
            }
        }
        public static void SaveQuickStat(GameClient Session, string Stat, int Newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_stats SET " + Stat + " = '" + Newval + "' WHERE id = " + Session.GetHabbo().Id + "");
            }
        }
        public static void SaveQuickStat(int UserId, string Stat, string Newval, bool Query = false)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (!Query)
                    dbClient.RunQuery("UPDATE play_stats SET " + Stat + " = '" + Newval + "' WHERE id = " + UserId + "");
                else
                    dbClient.RunQuery("UPDATE play_stats SET " + Stat + " = " + Newval + " WHERE id = " + UserId + "");
            }
        }

        public static void SaveQuickStat(int UserId, string Stat, int Newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_stats SET " + Stat + " = '" + Newval + "' WHERE id = " + UserId + "");
            }
        }

        public static void SaveQuickUserInfo(GameClient Session, string Stat, string Newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE users SET " + Stat + " = '" + Newval + "' WHERE id = " + Session.GetHabbo().Id + "");
            }
        }

        public static void SaveQuickUserInfo(GameClient Session, string Stat, int Newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE users SET " + Stat + " = " + Newval + " WHERE id = " + Session.GetHabbo().Id + "");
            }
        }

        public static void SaveQuickUserInfo(int UserId, string Stat, string Newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE users SET " + Stat + " = '" + Newval + "' WHERE id = " + UserId + "");
            }
        }

        public static void SetSancStatus(int UserId, int Time, int SancID)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_stats SET is_dying = '0', dying_time_left = '0', is_dead = '0', dead_time_left = '0', is_jailed = '0', jailed_time_left = '0', is_cuffed = '0', cuffed_time_left = '0', is_sanc = '1', sanc_time_left = '"+Time+"', sancs = sancs + 1 WHERE id = " + UserId + "; UPDATE users SET home_room = '"+SancID+ "' WHERE id = " + UserId + ";");
            }
        }
        public static void UnSanc(int UserId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_stats SET is_sanc = '0', sanc_time_left = '0', last_coordinates = '23,22,0,3' WHERE id = " + UserId + "; UPDATE users SET home_room = '1', time_muted = '0' WHERE id = " + UserId + ";");
            }
        }
        public static void UpdateMyWeaponStats(GameClient Session, string stat, string newsat, string weaponname)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_weapons_owned` SET `" + stat + "` = '" + newsat + "' WHERE `play_weapons_owned`.`base_weapon` = '" + weaponname + "' AND `play_weapons_owned`.`user_id` = " + Session.GetHabbo().Id + " AND `play_weapons_owned`.`baul_car_id` = '0';");
            }
        }
        public static void UpdateToWeaponStats(int OwnID, string stat, string newsat, string weaponname)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_weapons_owned` SET `" + stat + "` = '" + newsat + "' WHERE `play_weapons_owned`.`base_weapon` = '" + weaponname + "' AND `play_weapons_owned`.`user_id` = " + OwnID + " AND `play_weapons_owned`.`baul_car_id` = '0';");
            }
        }
        public static void UpdateToWeaponBaul(GameClient Session, int Owner, int BaulID, string weaponname)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_weapons_owned` SET `user_id` = '" + Session.GetHabbo().Id + "', `baul_car_id` = '0' WHERE `play_weapons_owned`.`base_weapon` = '" + weaponname + "' AND `play_weapons_owned`.`baul_car_id` = " + BaulID + " LIMIT 1;");
                //dbClient.RunQuery("UPDATE `play_weapons_owned` SET `user_id` = '" + Session.GetHabbo().Id + "', `baul_car_id` = '0' WHERE `play_weapons_owned`.`base_weapon` = '" + weaponname + "' AND `play_weapons_owned`.`baul_car_id` = " + BaulID + " AND `play_weapons_owned`.`user_id` = " + Owner + " LIMIT 1;");
            }
        }
        public static void UpdateToBaulWeaponBaul(GameClient Session, int BaulID, string weaponname)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_weapons_owned` SET `baul_car_id` = '"+ BaulID +"' WHERE `play_weapons_owned`.`base_weapon` = '" + weaponname + "' AND `play_weapons_owned`.`baul_car_id` = '0' AND `play_weapons_owned`.`user_id` = " + Session.GetHabbo().Id + " LIMIT 1;");
            }
        }
        public static void UpdateMyWeaponStats(GameClient Session, string stat, int newsat, string weaponname)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_weapons_owned` SET `"+stat+"` = '"+newsat+ "' WHERE `play_weapons_owned`.`base_weapon` = '"+weaponname+"' AND `play_weapons_owned`.`user_id` = " + Session.GetHabbo().Id+ " AND `play_weapons_owned`.`baul_car_id` = '0';");
            }
        }
        public static void DropMyWeapon(GameClient Session, string weaponname)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `play_weapons_owned` WHERE `play_weapons_owned`.`base_weapon` = '" + weaponname + "' AND `play_weapons_owned`.`user_id` = " + Session.GetHabbo().Id + " AND `play_weapons_owned`.`baul_car_id` = '0';");
            }
        }
        public static void DropAllMyWeapon(GameClient Session)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `play_weapons_owned` WHERE `play_weapons_owned`.`user_id` = " + Session.GetHabbo().Id + " AND `play_weapons_owned`.`baul_car_id` = '0';");
            }
        }

        public static void UpdateMyProductExtrada(GameClient Session, int ProductId, string extradata)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_products_owned` SET `extradata` = '" + extradata + "' WHERE `play_products_owned`.`product_id` = '" + ProductId + "' AND `play_products_owned`.`user_id` = " + Session.GetHabbo().Id + ";");
            }
        }

        public static void UpdateVehicleState(int furni_id, int newstate)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_vehicles_owned SET state = '" + newstate + "' WHERE furni_id = '" + furni_id + "'");
            }
        }

        public static void UpdateVehicleBaul(int furni_id, string value, int newvalue)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_vehicles_owned SET "+value+" = '" + newvalue + "' WHERE furni_id = '" + furni_id + "'");
            }
        }

        public static void UpdateVehicleBaul(int furni_id, string value, string newvalue)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_vehicles_owned SET " + value + " = '" + newvalue + "' WHERE furni_id = '" + furni_id + "'");
            }
        }
        // Update a String
        public static void UpdateVehicleStat(int carid, string val, string newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_vehicles_owned SET "+val+" = '" + newval + "' WHERE id = '" + carid + "'");
            }
        }
        // Update a INT
        public static void UpdateVehicleStat(int carid, string val, int newval)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE play_vehicles_owned SET " + val + " = " + newval + " WHERE id = '" + carid + "'");
            }
        }
        public static bool IsMyVehicle(GameClient Session, int carid)
        {
            VehiclesOwned VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwned(carid);

            if (VO == null)
                return false;

            return (VO.OwnerId == Session.GetHabbo().Id) ? true : false;
            /*
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id` FROM `play_vehicles_owned` WHERE `id` = '" + carid + "' AND `owner` = '"+ Session.GetHabbo().Id +"' LIMIT 1");
                DataRow Check = dbClient.getRow();
                if (Check == null)
                    return false;
                else
                    return true;
            }
            */
        }
        public static List<Room> GetRoomByDesc(string name)
        {
            List<Room> rooms = new List<Room>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id` FROM `rooms` WHERE `description` LIKE '" + name + "%'");
                DataTable GetRooms = dbClient.getTable();
                if (GetRooms != null)
                {
                    foreach (DataRow Row in GetRooms.Rows)
                    {
                        Room Room = GenerateRoom(Convert.ToInt32(Row["id"]));
                        if (Room != null)
                            rooms.Add(Room);
                    }
                }
            }
            return rooms;
        }

        public static List<string> GetReporterList()
        {
            List<string> list = new List<string>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT users.username, users.online, users.rank FROM users, play_stats WHERE users.id = play_stats.id AND play_stats.is_news_reporter = '1'");
                DataTable GetReporters = dbClient.getTable();
                if (GetReporters != null)
                {
                    foreach (DataRow Row in GetReporters.Rows)
                    {
                        string status = PlusEnvironment.EnumToBool(Row["online"].ToString()) ? "Online" : "Offline";
                        list.Add(Convert.ToString(Row["username"]) + " - Rango: " + Convert.ToString(Row["rank"]) + " (" + status + ")");
                    }
                }
            }
            return list;
        }

        public static List<Room> GetRoomByName(string name)
        {
            List<Room> rooms = new List<Room>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id FROM rooms WHERE caption LIKE '" + name + "%'");
                DataTable GetRooms = dbClient.getTable();

                if (GetRooms != null)
                {
                    foreach (DataRow Row in GetRooms.Rows)
                    {
                        Room Room = GenerateRoom(Convert.ToInt32(Row["id"]));
                        if (Room != null)
                            rooms.Add(Room);
                    }
                }
            }
            return rooms;
        }

        // Recibimos información ACTUAL del Trabajo del ususario
        // Éste método se encarga de subir XP y/o Niveles respectivos
        public static void JobSkills(GameClient Session, int JobId, int JobLvl, int JobXp)
        {
            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(JobId, 1))
                return;

            Group Job = null;

            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(JobId, out Job);

            if (Job == null)
                return;

            if (Job.GType != 2)
                return;

            string Type = "";

            #region Get Job Info
            if (Job.Name.Contains("Camionero"))
            {
                Type = "Camionero";
            }
            else if (Job.Name.Contains("Fábrica de Armas"))
            {
                Type = "Armero";
            }
            else if (Job.Name.Contains("Mecánico"))
            {
                Type = "Mecánico";
            }
            else if (Job.Name.Contains("Basurero"))
            {
                Type = "Basurero";
            }
            else if (Job.Name.Contains("Ladrón"))
            {
                Type = "Ladrón";
            }
            else if (Job.Name.Contains("Minero"))
            {
                Type = "Minero";
            }
            #endregion

            switch (Type)
            {
                #region Camionero
                case "Camionero":
                    if (JobXp < 50)
                    {
                        if (Session.GetHabbo().VIPRank > 0)
                        {
                            Session.GetPlay().CamXP += 1;
                            Session.SendWhisper("Has ganado +1 de Habilidad de Camionero", 1);
                        }
                        else
                        {
                            Random rnd = new Random();
                            int Prob = rnd.Next(0, 101);
                            if (Prob <= 50)
                            {
                                Session.GetPlay().CamXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Camionero", 1);
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().CamXP = 0;
                        Session.GetPlay().CamLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().CamLvl + " de Camionero. ¡Ahora se te pagará más! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Armero
                case "Armero":
                    if (JobXp < 50)
                    {
                        // Caso especial temporal
                        if (Session.GetHabbo().Id == 0)// Nunca entrará
                        {
                            Session.GetPlay().ArmXP += 5;
                            Session.SendWhisper("Has ganado +5 de Habilidad de Armero bebesini. Lov U |", 1);
                        }
                        else
                        {
                            if (Session.GetHabbo().VIPRank > 0)
                            {
                                Session.GetPlay().ArmXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Armero", 1);
                            }
                            else
                            {
                                Random rnd = new Random();
                                int Prob = rnd.Next(0, 101);

                                if (Prob <= 50)
                                {
                                    Session.GetPlay().ArmXP += 1;
                                    Session.SendWhisper("Has ganado +1 de Habilidad de Armero", 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().ArmXP = 0;
                        Session.GetPlay().ArmLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().ArmLvl + " de Armero. ¡Ahora podrás Fabricar más tipos de Armas! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Mecánico
                case "Mecanico":
                    if (JobXp < 50)
                    {
                        if (Session.GetHabbo().VIPRank > 0)
                        {
                            Session.GetPlay().MecXP += 1;
                            Session.SendWhisper("Has ganado +1 de Habilidad de Mecánico", 1);
                        }
                        else
                        {
                            Random rnd = new Random();
                            int Prob = rnd.Next(0, 101);
                            if (Prob <= 50)
                            {
                                Session.GetPlay().MecXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Mecánico", 1);
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().MecXP = 0;
                        Session.GetPlay().MecLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().MecLvl + " de Mecánico. ¡Ahora repararás más rápido! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Basurero
                case "Basurero":
                    if (JobXp < 50)
                    {
                        if (Session.GetHabbo().VIPRank > 0)
                        {
                            Session.GetPlay().BasuXP += 1;
                            Session.SendWhisper("Has ganado +1 de Habilidad de Basurero", 1);
                        }
                        else
                        {
                            Random rnd = new Random();
                            int Prob = rnd.Next(0, 101);
                            if (Prob <= 50)
                            {
                                Session.GetPlay().BasuXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Basurero", 1);
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().BasuXP = 0;
                        Session.GetPlay().BasuLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().BasuLvl + " de Basurero. ¡Ahora se te pagará más y recolectarás más rápido! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Ladrón
                case "Ladron":
                    if (JobXp < 50)
                    {
                        if (Session.GetHabbo().VIPRank > 0)
                        {
                            Session.GetPlay().LadronXP += 1;
                            Session.SendWhisper("Has ganado +1 de Habilidad de Ladrón", 1);
                        }
                        else
                        {
                            Random rnd = new Random();
                            int Prob = rnd.Next(0, 101);
                            if (Prob <= 50)
                            {
                                Session.GetPlay().LadronXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Ladrón", 1);
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().LadronXP = 0;
                        Session.GetPlay().LadronLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().LadronLvl + " de Ladrón. ¡Ahora forzarás cerraduras más rápido! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Minero
                case "Minero":
                    if (JobXp < 50)
                    {
                        if (Session.GetHabbo().VIPRank > 0)
                        {
                            Session.GetPlay().MinerXP += 1;
                            Session.SendWhisper("Has ganado +1 de Habilidad de Minero", 1);
                        }
                        else
                        {
                            Random rnd = new Random();
                            int Prob = rnd.Next(0, 101);
                            if (Prob <= 50)
                            {
                                Session.GetPlay().MinerXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Minero", 1);
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().MinerXP = 0;
                        Session.GetPlay().MinerLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().MinerLvl + " de Minero. ¡Ahora podrás minar rocas más Cercanas! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Default
                default:
                    break;
                #endregion
            }
        }

        public static void NoJobSkills(GameClient Session, string Job, int JobLvl, int JobXp)
        {
            switch (Job.ToLower())
            {
                #region Ladrón
                case "ladron":
                    if (JobXp < 50)
                    {
                        if (Session.GetHabbo().VIPRank > 0)
                        {
                            Session.GetPlay().LadronXP += 1;
                            Session.SendWhisper("Has ganado +1 de Habilidad de Ladrón", 1);
                        }
                        else
                        {
                            Random rnd = new Random();
                            int Prob = rnd.Next(0, 101);
                            if (Prob <= 50)
                            {
                                Session.GetPlay().LadronXP += 1;
                                Session.SendWhisper("Has ganado +1 de Habilidad de Ladrón", 1);
                            }
                        }
                    }
                    else
                    {
                        Session.GetPlay().LadronXP = 0;
                        Session.GetPlay().LadronLvl += 1;
                        Session.SendWhisper("¡Enhorabuena! Has subido a nivel " + Session.GetPlay().LadronLvl + " de Ladrón. ¡Ahora tienes menos probabilidades de Fallar al Forzar Cerraduras! ((Usa ':habilidades' para más información))", 1);
                    }
                    break;
                #endregion

                #region Default
                default:
                    break;
                    #endregion
            }
        }

        // Retorna en número entero en segundos
        public static int GetTimerByMyJob(GameClient Session, string Job)
        {
            int Timer = 0;

            #region GetByCases
            switch (Job.ToLower())
            {
                #region Basurero
                case "basurero":
                    {
                        Timer = MaxBasuTime - (Session.GetPlay().BasuLvl - 1);

                        if (Timer < MinBasuTime)
                            Timer = MinBasuTime;
                    }
                    break;
                #endregion

                #region Mecánico
                case "mecanico":
                    {
                        Timer = MaxMecTime - ((Session.GetPlay().MecLvl - 1) * 5);

                        if (Timer < MinMecTime)
                            Timer = MinMecTime;
                    }
                    break;
                #endregion

                // Ladrón (Por Herramientas)

                #region Minero
                case "minero":
                    {
                        Timer = MaxMinerTime - ((Session.GetPlay().MinerLvl - 1) * 10);

                        if (Timer < MinMinerTime)
                            Timer = MinMinerTime;
                    }
                    break;
                #endregion

                #region default
                default:
                    break;
                #endregion
            }
            #endregion

            if (Timer < 0)
                Timer = 0;
            return Timer;
        }
        
        public static void GetRandomObject(GameClient Session)
        {

            Random rnd = new Random();
            int Num = rnd.Next(1, 12);

            #region Object List
            if (Num <= 1)
            {
                Session.GetPlay().Object = "Reloj de Estuche";
                Session.GetPlay().ObjectPrice = 150;
            }
            else if (Num == 2)
            {
                Session.GetPlay().Object = "Maletín";
                Session.GetPlay().ObjectPrice = 125;
            }
            else if (Num == 3)
            {
                Session.GetPlay().Object = "Guitarra";
                Session.GetPlay().ObjectPrice = 50;
            }
            else if (Num == 4)
            {
                Session.GetPlay().Object = "Televisor Moderno";
                Session.GetPlay().ObjectPrice = 25;
            }
            else if (Num == 5)
            {
                Session.GetPlay().Object = "Bajo";
                Session.GetPlay().ObjectPrice = 38;
            }
            else if (Num == 6)
            {
                Session.GetPlay().Object = "Televisor Antiguo";
                Session.GetPlay().ObjectPrice = 20;
            }
            else if (Num == 7)
            {
                Session.GetPlay().Object = "Nintendo 64";
                Session.GetPlay().ObjectPrice = 30;
            }
            else if (Num == 8)
            {
                Session.GetPlay().Object = "Extintor";
                Session.GetPlay().ObjectPrice = 18;
            }
            else if (Num == 9)
            {
                Session.GetPlay().Object = "Radio";
                Session.GetPlay().ObjectPrice = 15;
            }
            else if (Num == 10)
            {
                Session.GetPlay().Object = "Tabla de Surf";
                Session.GetPlay().ObjectPrice = 13;
            }
            else if (Num >= 11)
            {
                Session.GetPlay().Object = "VideoCasetera";
                Session.GetPlay().ObjectPrice = 6;
            }
            #endregion
        }

        public static bool IsRobObject(string Obj)
        {
            bool rob = false;
            switch (Obj.ToLower())
            {
                case "reloj de estuche":
                case "maletín":
                case "guitarra":
                case "televisor moderno":
                case "bajo":
                case "televisor antiguo":
                case "nintendo 64":
                case "extintor":
                case "radio":
                case "tabla de surf":
                case "videocasetera":
                    rob = true;
                    break;

                default:
                    break;
            }

            return rob;
        }

        public static void SetPriceObject(GameClient Session, string Obj)
        {
            switch (Obj)
            {
                case "reloj de estuche":
                    Session.GetPlay().ObjectPrice = 150;
                    break;
                case "maletín":
                    Session.GetPlay().ObjectPrice = 125;
                    break;
                case "guitarra":
                    Session.GetPlay().ObjectPrice = 50;
                    break;
                case "televisor moderno":
                    Session.GetPlay().ObjectPrice = 25;
                    break;
                case "bajo":
                    Session.GetPlay().ObjectPrice = 38;
                    break;
                case "televisor antiguo":
                    Session.GetPlay().ObjectPrice = 20;
                    break;
                case "nintendo 64":
                    Session.GetPlay().ObjectPrice = 30;
                    break;
                case "extintor":
                    Session.GetPlay().ObjectPrice = 18;
                    break;
                case "radio":
                    Session.GetPlay().ObjectPrice = 15;
                    break;
                case "tabla de surf":
                    Session.GetPlay().ObjectPrice = 13;
                    break;
                case "videocasetera":
                    Session.GetPlay().ObjectPrice = 6;
                    break;
                default:
                    Session.GetPlay().ObjectPrice = 0;
                    break;
            }
        }

        public static String GetLada()
        {
            return "555";
        }

        public static String GeneratePhoneNumber(int userid)
        {
            bool repetido = true;
            Random rnd = new Random();
            String Num = "";

            while (repetido)
            {
                Num = PlusEnvironment.GetGame().GetClientManager().NumberFormatRP(GetLada() + rnd.Next(1000000, 9999999));
                
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT phone_number FROM play_phones_owned WHERE phone_number = '" + Num + "'");
                    DataRow GetNums = dbClient.getRow();

                    if (GetNums == null)
                        repetido = false;
                }
                
            }

            return Num;
        }

        public static int GetInfoPD(int CurLevel, string type)
        {
            #region Set Values
            int PD = 0;
            int NeedXP = 0;
            int Cost = 0;

            #region Niveles
            if (CurLevel == 1) { Cost = 98; PD = 150; NeedXP = 7; }
            if (CurLevel == 2) { Cost = 324; PD = 250; NeedXP = 12; }
            if (CurLevel == 3) { Cost = 760; PD = 350; NeedXP = 19; }
            if (CurLevel == 4) { Cost = 1643; PD = 450; NeedXP = 31; }
            if (CurLevel == 5) { Cost = 3432; PD = 550; NeedXP = 52; }
            if (CurLevel == 6) { Cost = 6794; PD = 650; NeedXP = 86; }
            if (CurLevel == 7) { Cost = 13156; PD = 750; NeedXP = 143; }
            if (CurLevel == 8) { Cost = 25095; PD = 850; NeedXP = 239; }
            if (CurLevel == 9) { Cost = 46846; PD = 950; NeedXP = 397; }
            if (CurLevel == 10) { Cost = 86722; PD = 1050; NeedXP = 662; }
            if (CurLevel == 11) { Cost = 158832; PD = 1150; NeedXP = 1103; }
            if (CurLevel == 12) { Cost = 288566; PD = 1250; NeedXP = 1838; }
            if (CurLevel == 13) { Cost = 520710; PD = 1350; NeedXP = 3063; }
            if (CurLevel == 14) { Cost = 934215; PD = 1450; NeedXP = 5105; }
            if (CurLevel >= 15) { Cost = 1488566; PD = 1550; NeedXP = 7840; }
            #endregion
            #endregion

            if (type.Contains("PD"))
                return PD;
            else if (type.Contains("NeedXP"))
                return NeedXP;
            else if (type.Contains("Cost"))
                return Cost;
            else
                return 0;
        }

        public static void SetJobCar(GameClient Session, int VehicleJobID)
        {
            VehicleJobs VehicleJob = VehicleJobsManager.getVehicleJob(VehicleJobID);
            if(VehicleJob != null)
            {
                Room Room = GenerateRoom(VehicleJob.RoomID);
                if (Room != null)
                {                    
                    foreach (Item item in Room.GetRoomItemHandler().GetFurniObjects(VehicleJob.X, VehicleJob.Y).ToList())
                    {
                        //if (item.GetBaseItem().ItemName.StartsWith("carro_rp_"))
                        if (item.GetBaseItem().Id >= 8000002)//Cualquier vehículo (play_vehicles)
                        {
                            Room.GetRoomItemHandler().RemoveRoomItem(item);
                            PickItem(Session, item.Id);
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery("DELETE FROM items WHERE id = " + item.Id);
                            }
                            //Session.SendWhisper("Se ha quitado un vehiculo detectado en esa posición", 1);
                        }
                    }

                    lock (itemobj)
                    {
                        Session.GetPlay().isParking = true;

                        //Console.WriteLine("SessionID: " + Session.GetHabbo().Id + "\nVehicleJob.BaseItem: " + VehicleJob.BaseItem + "\nVehicleJobX/Y/Z" + VehicleJob.X + " " + VehicleJob.Y + " " + VehicleJob.Z + "VehicleJob.Rot: " + VehicleJob.Rot + "Room.Id" + Room.Id);
                        
                        var Obj = PlaceItemToRoom(Session, VehicleJob.BaseItem, 0, VehicleJob.X, VehicleJob.Y, VehicleJob.Z, VehicleJob.Rot, false, Room.Id, true, "", false);
                        
                        //Console.WriteLine(Obj);

                        Session.GetPlay().isParking = false;
                    }
                    //Session.SendWhisper("Auto colocado", 1);
                }
            }
        }
        public static void CheckCorpCarp(GameClient Session)
        {
            // Si conduce un auto de trabajo, pero ya había conducido uno
            // Verificamos si es el mismo.
            // Si es otro, recogemos el anterior (de existir) y spawn.
            if (Session.GetPlay().CarJobId > 0)
            {
                /*VERFICAR SI LO TRAE CONDUCIENDO ALGUIEN PARA EVITAR DUPLICAR*/
                #region Remove Vehicle From Target
                List<GameClient> TargetDriver = (from TG in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where TG != null && TG.GetHabbo() != null && TG.GetPlay() != null && TG.GetPlay().DrivingCarItem == Session.GetPlay().CarJobLastItemId && TG.GetHabbo().Id != Session.GetHabbo().Id select TG).ToList();
                foreach (GameClient Target in TargetDriver)
                {
                    #region Extra Conditions & Checks

                    #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                    //Vars
                    string Pasajeros = Target.GetPlay().Pasajeros;
                    string[] stringSeparators = new string[] { ";" };
                    string[] result;
                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string psjs in result)
                    {
                        GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                        if (PJ != null)
                        {
                            if (PJ.GetPlay().ChoferName == Target.GetHabbo().Username)
                            {
                                RoleplayManager.Shout(PJ, "*Baja del vehículo de " + Target.GetHabbo().Username + "*", 5);
                            }
                            // PASAJERO
                            PJ.GetPlay().Pasajero = false;
                            PJ.GetPlay().ChoferName = "";
                            PJ.GetPlay().ChoferID = 0;
                            PJ.GetRoomUser().CanWalk = true;
                            PJ.GetRoomUser().FastWalking = false;
                            PJ.GetRoomUser().TeleportEnabled = false;
                            PJ.GetRoomUser().AllowOverride = false;

                            // Descontamos Pasajero
                            Target.GetPlay().PasajerosCount--;
                            StringBuilder builder = new StringBuilder(Target.GetPlay().Pasajeros);
                            builder.Replace(PJ.GetHabbo().Username + ";", "");
                            Target.GetPlay().Pasajeros = builder.ToString();

                            // CHOFER 
                            Target.GetPlay().Chofer = (Target.GetPlay().PasajerosCount <= 0) ? false : true;
                            Target.GetRoomUser().AllowOverride = (Target.GetPlay().PasajerosCount <= 0) ? false : true;

                            // SI EL PASAJERO ES UN HERIDO
                            if (PJ.GetPlay().IsDying)
                            {
                                #region Send To Hospital
                                RoleplayManager.Shout(PJ, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
                                Room Room2 = RoleplayManager.GenerateRoom(PJ.GetRoomUser().RoomId);
                                string MyCity2 = Room2.City;

                                PlayRoom Data2;
                                int ToHosp2 = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity2, out Data2);

                                if (ToHosp2 > 0)
                                {
                                    Room Room3 = RoleplayManager.GenerateRoom(ToHosp2);
                                    if (Room3 != null)
                                    {
                                        PJ.GetPlay().IsDead = true;
                                        PJ.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                                        PJ.GetHabbo().HomeRoom = ToHosp2;

                                        /*
                                        if (PJ.GetHabbo().CurrentRoomId != ToHosp)
                                            RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                                        else
                                            Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                                        */
                                        RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                                    }
                                    else
                                    {
                                        PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                        PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                        PJ.GetPlay().RefreshStatDialogue();
                                        PJ.GetRoomUser().Frozen = false;
                                        PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                    }
                                }
                                else
                                {
                                    PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                    PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                    PJ.GetPlay().RefreshStatDialogue();
                                    PJ.GetRoomUser().Frozen = false;
                                    PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                }
                                PJ.GetPlay().IsDying = false;
                                PJ.GetPlay().DyingTimeLeft = 0;
                                #endregion
                            }

                            // SI EL PASAJERO ES COMPAÑERO DE BASURERO
                            if (PJ.GetPlay().IsBasuPasaj)
                                PJ.GetPlay().IsBasuPasaj = false;

                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                        }
                    }
                    #endregion

                    #region Online ParkVars
                    //Retornamos a valores predeterminados
                    Target.GetPlay().DrivingCar = false;
                    Target.GetPlay().DrivingInCar = false;
                    Target.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                    //Combustible System
                    Target.GetPlay().CarType = 0;// Define el gasto de combustible
                    Target.GetPlay().CarFuel = 0;
                    Target.GetPlay().CarMaxFuel = 0;
                    Target.GetPlay().CarTimer = 0;
                    Target.GetPlay().CarLife = 0;

                    Target.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                    Target.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                    if (Target.GetRoomUser() != null)
                    {
                        Target.GetRoomUser().ApplyEffect(0);
                        Target.GetRoomUser().FastWalking = false;
                    }
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Target, "event_vehicle", "close");// WS FUEL

                    Target.GetPlay().CarJobId = 0;
                    Target.GetPlay().CarJobLastItemId = 0;
                    #endregion

                    Shout(Target, " el vehículo que " + Target.GetHabbo().Username + " conducía fue decomisado por una grúa.", 4);
                    Target.SendWhisper("Al parecer el propietario original del vehículo ha reclamado su uso.", 1);
                    #endregion
                }
                #endregion

                //Recoge el item
                PickItem(Session, Session.GetPlay().CarJobLastItemId);
                // Quitar del diccionario
                PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwnedByFurniId(Session.GetPlay().CarJobLastItemId);
                //ClearCorpCar(Session, Session.GetPlay().CarJobLastItemId); <= NO se guardan en db, no necesario.
                //Respawneamos nuevo auto
                SetJobCar(Session, Session.GetPlay().CarJobId);
            }

            // Retornamos
            Session.GetPlay().CarJobId = 0;
            Session.GetPlay().CarJobLastItemId = 0;

            if (Session.GetPlay().TimerManager != null && Session.GetPlay().TimerManager.ActiveTimers != null)
            {
                if (Session.GetPlay().TimerManager.ActiveTimers.ContainsKey("vehiclejob"))
                {
                    Session.SendWhisper("¡Tu Vehículo de trabajo ha sido regresado a su sitio por haberlo abandonado mucho tiempo!", 1);
                    Session.GetPlay().TimerManager.ActiveTimers["vehiclejob"].EndTimer();
                }
            }
        }

        public static bool isValidPark(Room Room, Point Coords)
        {
            if (Room == null)
                return false;

            Item BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "arow" && x.Coordinate == Coords);
            Item BTile2 = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "room_wl15_teleblock" && x.Coordinate == Coords);
            Item BTile3 = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Coords);
            Item BTile4 = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carr2" && x.Coordinate == Coords);
            Item BTile5 = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Coords);
            Item BTile6 = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint02" && x.Coordinate == Coords);
            Item BTile7 = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "room_wl15_telearrow" && x.Coordinate == Coords);

            if (BTile != null || BTile2 != null || BTile3 != null || BTile4 != null || BTile5 != null || BTile6 != null || BTile7 != null)
                return false;

            return true;
        }

        public static int getCamCargDest(Room Room, int CargaId)
        {
            List<Room> Rooms = null;
            string MyCity = Room.City;
            int Destino = -1;
            switch (CargaId)
            {
                case 1:
                    Rooms = GetRoomByDesc("24/7");
                    break;
                case 2:
                    Rooms = GetRoomByDesc("Tienda de Ropa");
                    break;
                case 3:
                    Rooms = GetRoomByDesc("Drogas");
                    break;
                case 4:
                    Rooms = GetRoomByDesc("Ammunation");
                    break;
                default:
                    break;
            }
            if (Rooms.Count > 0)
            {
                foreach (Room CurRoom in Rooms)
                {
                    Room RoomInfo = GenerateRoom(CurRoom.RoomId);
                    if (RoomInfo != null)
                    {
                        string JobCity = RoomInfo.City;
                        if (MyCity == JobCity)
                            Destino = CurRoom.RoomId;
                    }
                }

            }
            return Destino;
        }
        public static string getCamCargName(int CargaId)
        {
            string CargName = "Ninguno";
            switch (CargaId)
            {
                case 1:
                    CargName = "Productos de 24/7";
                    break;
                case 2:
                    CargName = "Ropa";
                    break;
                case 3:
                    CargName = "Drogas";
                    break;
                case 4:
                    CargName = "Armas";
                    break;
                default:
                    break;
            }
            return CargName;
        }

        public static void CheckBasurero(GameClient Client)
        {
            if (Client.GetPlay().BasuTeamId > 0)
            {
                GameClient TeamPasaj = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetPlay().BasuTeamId);
                if (TeamPasaj != null)
                {
                    TeamPasaj.SendWhisper("¡Tu compañero se ha ido! Han fracasado el recorrido.", 1);
                    TeamPasaj.GetPlay().BasuTeamId = 0;
                    TeamPasaj.GetPlay().BasuTeamName = "";
                    TeamPasaj.GetPlay().BasuTrashCount = 0;
                    TeamPasaj.GetPlay().IsBasuPasaj = false;
                    if (!TeamPasaj.GetPlay().DrivingCar)
                        TeamPasaj.GetPlay().IsBasuChofer = false;
                    else
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TeamPasaj,
                            "compose_basurero|" +
                            "showinfo|" +
                            TeamPasaj.GetHabbo().Username + "|" + // Chofer
                            "Ninguno|" + // Recolector
                            TeamPasaj.GetPlay().BasuTrashCount + "/15|" +
                            TeamPasaj.GetPlay().IsBasuChofer);
                    }
                }
            }
        }

        public static void CheckAntiLaw(GameClient Client)
        {
            if (Client.GetPlay().Cuffed || Client.GetHabbo().EscortID > 0)
                ForceAntirolJail(Client);

            if (Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("stun"))
                ForceAntirolSanc(Client, 15);

            // Si está siendo escoltado
            if (Client.GetHabbo().EscortID > 0)
            {
                GameClient police = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetHabbo().EscortID);
                if (police != null)
                {
                    police.GetHabbo().Escorting = 0;
                }
            }

            // Si está escoltando
            if (Client.GetHabbo().Escorting > 0)
            {
                GameClient convic = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetHabbo().Escorting);
                if (convic != null)
                {
                    convic.GetRoomUser().ClearMovement(true);
                    convic.GetRoomUser().CanWalk = true;
                    convic.GetHabbo().EscortID = 0;
                }
            }
        }

        public static void ForceAntirolJail(GameClient Client)
		{
            Wanted Wanted = RoleplayManager.WantedList.ContainsKey(Client.GetHabbo().Id) ? RoleplayManager.WantedList[Client.GetHabbo().Id] : null;
            int WantedTime = Wanted == null ? 10 : (Wanted.WantedLevel + 1) * 5;

            Client.GetPlay().IsJailed = true;
            Client.GetPlay().JailedTimeLeft = WantedTime;
            Client.GetPlay().TimerManager.CreateTimer("jail", 1000, false);
            Client.GetPlay().Arrested++;
            SaveQuickStat(Client, "is_jailed", "1");
            SaveQuickStat(Client, "jailed_time_left", "" + WantedTime);
            SaveQuickStat(Client, "is_wanted", "0");
            SaveQuickStat(Client, "wanted_level", "0");
            SaveQuickStat(Client, "wanted_time_left", "0");
            SaveQuickStat(Client, "is_cuffed", "0");
            SaveQuickStat(Client, "cuffed_time_left", "0");
            SaveQuickStat(Client, "arrested", Client.GetPlay().Arrested);
        }

        public static void ForceAntirolJail(int Id)
        {
            Wanted Wanted = RoleplayManager.WantedList.ContainsKey(Id) ? RoleplayManager.WantedList[Id] : null;
            int WantedTime = Wanted == null ? 10 : (Wanted.WantedLevel + 1) * 5;

            SaveQuickStat(Id, "is_jailed", "1");
            SaveQuickStat(Id, "jailed_time_left", "" + WantedTime);
            SaveQuickStat(Id, "is_wanted", "0");
            SaveQuickStat(Id, "wanted_level", "0");
            SaveQuickStat(Id, "wanted_time_left", "0");
            SaveQuickStat(Id, "is_cuffed", "0");
            SaveQuickStat(Id, "cuffed_time_left", "0");
            SaveQuickStat(Id, "arrested", "arrested + 1", true);
        }

        public static void ForceAntirolSanc(GameClient Client, int timeSanc)
        {
            #region Check Workers
            if (Client.GetPlay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(Client);
                Client.GetPlay().IsWorking = false;
                Client.GetHabbo().Poof();
                //RoleplayManager.CheckCorpCarp(client);
                RoleplayManager.Shout(Client, "*Ha dejado de trabajar*", 5);

                #region Check Police Car
                if (Client.GetPlay().DrivingCar && Client.GetPlay().CarEnableId == EffectsList.CarPolice)
                {
                    #region Park
                    int ItemPlaceId = 0;
                    int roomid = Client.GetRoomUser().RoomId;
                    Room Room = RoleplayManager.GenerateRoom(roomid, false);
                    VehiclesOwned VOD = null;
                    if (Client.GetPlay().DrivingInCar)
                    {
                        RoleplayManager.Shout(Client, "* Una Grúa se ha llevado el vehículo que " + Client.GetHabbo().Username + " conducía.", 4);
                        // Actualizamos datos del auto en el diccionario y DB
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(Client, 0, false, out VOD);
                        ItemPlaceId = VOD.Id;
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                        RoleplayManager.CheckCorpCarp(Client);
                    }

                    #region Extra Conditions & Checks
                    #region CorpCar Respawn
                    Client.GetPlay().CarJobLastItemId = ItemPlaceId;
                    #endregion

                    #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                    //Vars
                    string Pasajeros = Client.GetPlay().Pasajeros;
                    string[] stringSeparators = new string[] { ";" };
                    string[] result;
                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string psjs in result)
                    {
                        GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                        if (PJ != null)
                        {
                            if (PJ.GetPlay().ChoferName == Client.GetHabbo().Username)
                            {
                                RoleplayManager.Shout(PJ, "*Baja del vehículo de " + Client.GetHabbo().Username + "*", 5);
                            }
                            // PASAJERO
                            PJ.GetPlay().Pasajero = false;
                            PJ.GetPlay().ChoferName = "";
                            PJ.GetPlay().ChoferID = 0;
                            PJ.GetRoomUser().CanWalk = true;
                            PJ.GetRoomUser().FastWalking = false;
                            PJ.GetRoomUser().TeleportEnabled = false;
                            PJ.GetRoomUser().AllowOverride = false;

                            // Descontamos Pasajero
                            Client.GetPlay().PasajerosCount--;
                            StringBuilder builder = new StringBuilder(Client.GetPlay().Pasajeros);
                            builder.Replace(PJ.GetHabbo().Username + ";", "");
                            Client.GetPlay().Pasajeros = builder.ToString();

                            // CHOFER 
                            Client.GetPlay().Chofer = (Client.GetPlay().PasajerosCount <= 0) ? false : true;
                            Client.GetRoomUser().AllowOverride = (Client.GetPlay().PasajerosCount <= 0) ? false : true;

                            // SI EL PASAJERO ES UN HERIDO
                            if (PJ.GetPlay().IsDying)
                            {
                                #region Send To Hospital
                                RoleplayManager.Shout(PJ, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
                                Room Room2 = RoleplayManager.GenerateRoom(PJ.GetRoomUser().RoomId);
                                string MyCity2 = Room2.City;

                                PlayRoom Data2;
                                int ToHosp2 = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity2, out Data2);

                                if (ToHosp2 > 0)
                                {
                                    Room Room3 = RoleplayManager.GenerateRoom(ToHosp2);
                                    if (Room3 != null)
                                    {
                                        PJ.GetPlay().IsDead = true;
                                        PJ.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                                        PJ.GetHabbo().HomeRoom = ToHosp2;

                                        /*
                                        if (PJ.GetHabbo().CurrentRoomId != ToHosp)
                                            RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                                        else
                                            Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                                        */
                                        RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                                    }
                                    else
                                    {
                                        PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                        PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                        PJ.GetPlay().RefreshStatDialogue();
                                        PJ.GetRoomUser().Frozen = false;
                                        PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                    }
                                }
                                else
                                {
                                    PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                    PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                    PJ.GetPlay().RefreshStatDialogue();
                                    PJ.GetRoomUser().Frozen = false;
                                    PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                }
                                PJ.GetPlay().IsDying = false;
                                PJ.GetPlay().DyingTimeLeft = 0;
                                #endregion
                            }

                            // SI EL PASAJERO ES COMPAÑERO DE BASURERO
                            if (PJ.GetPlay().IsBasuPasaj)
                                PJ.GetPlay().IsBasuPasaj = false;

                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                        }
                    }
                    #endregion

                    #region Check Jobs

                    #region Taxista
                    if (Client.GetPlay().Ficha != 0)
                    {
                        Client.GetPlay().Ficha = 0;
                        Client.GetPlay().FichaTimer = 0;
                        RoleplayManager.Shout(Client, "*Apaga el Taxímetro y deja de trabajar*", 5);
                    }
                    #endregion

                    #endregion
                    #endregion

                    #region Online ParkVars
                    //Retornamos a valores predeterminados
                    Client.GetPlay().DrivingCar = false;
                    Client.GetPlay().DrivingInCar = false;
                    Client.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                    //Combustible System
                    Client.GetPlay().CarType = 0;// Define el gasto de combustible
                    Client.GetPlay().CarFuel = 0;
                    Client.GetPlay().CarMaxFuel = 0;
                    Client.GetPlay().CarTimer = 0;
                    Client.GetPlay().CarLife = 0;

                    Client.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                    Client.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                    Client.GetRoomUser().ApplyEffect(0);
                    Client.GetRoomUser().FastWalking = false;
                    #endregion

                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_vehicle", "close");
                    #endregion

                    RoleplayManager.CheckCorpCarp(Client);
                }
                #endregion

                #region Check Escorting
                if (Client.GetHabbo().Escorting > 0)
                {
                    GameClient tClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PlusEnvironment.GetUsernameById(Client.GetHabbo().Escorting));
                    if (tClient != null)
                    {
                        #region Stand
                        if (tClient.GetRoomUser().isSitting)
                        {
                            tClient.GetRoomUser().Statusses.Remove("sit");
                            tClient.GetRoomUser().Z += 0.35;
                            tClient.GetRoomUser().isSitting = false;
                            tClient.GetRoomUser().UpdateNeeded = true;
                        }
                        else if (tClient.GetRoomUser().isLying)
                        {
                            tClient.GetRoomUser().Statusses.Remove("lay");
                            tClient.GetRoomUser().Z += 0.35;
                            tClient.GetRoomUser().isLying = false;
                            tClient.GetRoomUser().UpdateNeeded = true;
                        }
                        #endregion

                        var This = Client.GetHabbo();
                        var User = tClient.GetRoomUser();

                        RoleplayManager.Shout(Client, "*Deja de escoltar a " + tClient.GetHabbo().Username + "*", 37);
                        User.ClearMovement(true);
                        tClient.GetRoomUser().CanWalk = true;
                        User.GetClient().GetHabbo().EscortID = 0;
                        This.Escorting = 0;
                    }
                }
                #endregion
            }
            #endregion

            #region Check Dying or Dead
            if (Client.GetPlay().IsDying || Client.GetPlay().IsDead)
            {
                Client.GetPlay().IsDead = false;
                Client.GetPlay().IsDying = false;

                if (Client.GetRoomUser() != null)
                {
                    Client.GetRoomUser().ApplyEffect(0);
                    Client.GetRoomUser().CanWalk = true;
                    Client.GetRoomUser().Frozen = false;
                }

                Client.GetPlay().DeadTimeLeft = 0;
                Client.GetPlay().DyingTimeLeft = 0;
                Client.GetPlay().CurHealth = Client.GetPlay().MaxHealth;

                // Refrescamos WS
                Client.GetPlay().UpdateInteractingUserDialogues();
                Client.GetPlay().RefreshStatDialogue();
            }
            #endregion

            #region Check Cuffed
            if (Client.GetPlay().Cuffed)
                Client.GetPlay().Cuffed = false;
            #endregion

            #region Check Jailed
            Client.GetPlay().IsJailed = false;
            Client.GetPlay().JailedTimeLeft = 0;
            Client.GetHabbo().Poof(true);
            #endregion

            #region Desequipar
            if (Client.GetPlay().EquippedWeapon != null)
            {
                string UnEquipMessage = Client.GetPlay().EquippedWeapon.UnEquipText;
                UnEquipMessage = UnEquipMessage.Replace("[NAME]", Client.GetPlay().EquippedWeapon.PublicName);

                RoleplayManager.Shout(Client, UnEquipMessage, 5);

                if (Client.GetRoomUser().CurrentEffect == Client.GetPlay().EquippedWeapon.EffectID)
                    Client.GetRoomUser().ApplyEffect(0);

                if (Client.GetRoomUser().CarryItemID == Client.GetPlay().EquippedWeapon.HandItem)
                    Client.GetRoomUser().CarryItem(0);

                Client.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                Client.GetPlay().EquippedWeapon = null;

                Client.GetPlay().WLife = 0;
                Client.GetPlay().Bullets = 0;
            }
            #endregion

            Client.GetPlay().Sancs++;
            Client.GetPlay().IsSanc = true;
            Client.GetPlay().SancTimeLeft = timeSanc;
            Client.GetPlay().TimerManager.CreateTimer("sanc", 1000, false);

            if (Client.GetRoomUser() != null)
            {
                string MyCity = Client.GetHabbo().CurrentRoom.City;
                int SancID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetSanc(MyCity, out PlayRoom Data);//sanc de la cd.

                if (Client.GetRoomUser().RoomId != SancID)
                {
                    RoleplayManager.SendUserOld(Client, SancID);
                    Client.GetHabbo().HomeRoom = SancID;
                    SaveQuickUserInfo(Client, "home_room", SancID);
                }
            }

            Client.SendMessage(new RoomNotificationComposer("room_sanc", "message", "Has sido sancionad@ por Antirol durante " + timeSanc + " minuto(s)"));

            SaveQuickStat(Client, "is_jailed", "0");
            SaveQuickStat(Client, "jailed_time_left", "0");
            SaveQuickStat(Client, "is_cuffed", "0");
            SaveQuickStat(Client, "cuffed_time_left", "0");
            SaveQuickStat(Client, "is_sanc", "1");
            SaveQuickStat(Client, "sanc_time_left", timeSanc);
            SaveQuickStat(Client, "sancs", Client.GetPlay().Sancs);
        }

        public static void ForceAntirolSanc(int Id, int timeSanc)
        {
            SaveQuickStat(Id, "is_jailed", "0");
            SaveQuickStat(Id, "jailed_time_left", "0");
            SaveQuickStat(Id, "is_cuffed", "0");
            SaveQuickStat(Id, "cuffed_time_left", "0");
            SaveQuickStat(Id, "is_sanc", "1");
            SaveQuickStat(Id, "sanc_time_left", timeSanc);
            SaveQuickStat(Id, "sancs", "sancs + 1", true);
        }

        public static void CheckEscort(GameClient Client)
        {
            // Si está siendo escoltado
            if (Client.GetHabbo().EscortID > 0)
            {
                GameClient police = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetHabbo().EscortID);
                if (police != null)
                {
                    police.GetHabbo().Escorting = 0;
                }
            }

            // Si está escoltando
            if (Client.GetHabbo().Escorting > 0)
            {
                GameClient convic = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetHabbo().Escorting);
                if (convic != null)
                {
                    convic.GetRoomUser().ClearMovement(true);
                    convic.GetRoomUser().CanWalk = true;
                    convic.GetHabbo().EscortID = 0;
                }
            }
        }

        public static void PoliceCMDSCheck(GameClient Client)
        {
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law") && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_commands", "hide_police_cmds");
            else
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_commands", "show_police_cmds");
        }

        public static void CheckOnCar(GameClient Client)
        {
            #region Is Driving?
            if (Client.GetPlay().DrivingCar)
            {
                #region Vehicle Check
                Vehicle vehicle = null;
                int corp = 0;
                bool ToDB = true;
                foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                {
                    if (Client.GetPlay().CarEnableId == Convert.ToInt32(Vehicle.EffectID))
                    {
                        vehicle = Vehicle;
                        if (vehicle.CarCorp > 0)
                        {
                            corp = vehicle.CarCorp;
                            ToDB = false;
                        }
                    }
                }
                #endregion

                #region Park
                int ItemPlaceId = 0;
                int roomid = Client.GetRoomUser().RoomId;
                Room RoomDriving = RoleplayManager.GenerateRoom(roomid, false);

                object[] Coords = { Client.GetRoomUser().X, Client.GetRoomUser().Y, Client.GetRoomUser().Z, Client.GetRoomUser().RotBody };
                if (Client.GetPlay().DrivingInCar || !RoleplayManager.isValidPark(RoomDriving, Client.GetRoomUser().Coordinate))
                {
                    RoleplayManager.Shout(Client, "* Una Grúa se ha llevado el vehículo que " + Client.GetHabbo().Username + " conducía por encontrarse mal estacionado y sin combustible.", 4);
                    // Actualizamos datos del auto en el diccionario y DB
                    PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(Client, 0, ToDB, out VehiclesOwned VOD);
                    ItemPlaceId = VOD.Id;
                    if (corp > 0)
                    {
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                        RoleplayManager.CheckCorpCarp(Client);
                    }
                }
                else if (Client.GetPlay().DrivingCar)
                {
                    if (corp > 0)
                    {
                        RoleplayManager.Shout(Client, "* Una Grúa se ha llevado el vehículo que " + Client.GetHabbo().Username + " conducía por encontrarse mal estacionado y sin combustible.", 4);
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                        RoleplayManager.CheckCorpCarp(Client);
                    }
                    else
                    {
                        // Colocamos Furni en Sala
                        Client.GetPlay().isParking = true;
                        Item ItemPlace = RoleplayManager.PutItemToRoom(Client, Client.GetPlay().DrivingCarItem, Client.GetRoomUser().RoomId, vehicle.ItemID, Convert.ToInt32(Coords[0]), Convert.ToInt32(Coords[1]), Convert.ToInt32(Coords[3]), ToDB);
                        //Item ItemPlace = RoleplayManager.PlaceItemToRoom(Client, vehicle.ItemID, 0, Client.GetRoomUser().X, Client.GetRoomUser().Y, Client.GetRoomUser().Z, Client.GetRoomUser().RotBody, false, RoomDriving.Id, ToDB, "");
                        Client.GetPlay().isParking = false;
                        ItemPlaceId = ItemPlace.Id;
                        // Actualizamos datos del auto en el diccionario y DB
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(Client, ItemPlaceId, ToDB, out VehiclesOwned VOD);
                    }
                }

                #region Extra Conditions & Checks
                #region CorpCar Respawn
                if (corp > 0)
                {
                    Client.GetPlay().CarJobLastItemId = ItemPlaceId;
                }
                #endregion

                #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                //Vars
                string Pasajeros = Client.GetPlay().Pasajeros;
                string[] stringSeparators = new string[] { ";" };
                string[] result;
                result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string psjs in result)
                {
                    GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                    if (PJ != null)
                    {
                        if (PJ.GetPlay().ChoferName == Client.GetHabbo().Username)
                        {
                            RoleplayManager.Shout(PJ, "*Baja del vehículo de " + Client.GetHabbo().Username + "*", 5);
                        }
                        // PASAJERO
                        PJ.GetPlay().Pasajero = false;
                        PJ.GetPlay().ChoferName = "";
                        PJ.GetPlay().ChoferID = 0;
                        PJ.GetRoomUser().CanWalk = true;
                        PJ.GetRoomUser().FastWalking = false;
                        PJ.GetRoomUser().TeleportEnabled = false;
                        PJ.GetRoomUser().AllowOverride = false;

                        // Descontamos Pasajero
                        Client.GetPlay().PasajerosCount--;
                        StringBuilder builder = new StringBuilder(Client.GetPlay().Pasajeros);
                        builder.Replace(PJ.GetHabbo().Username + ";", "");
                        Client.GetPlay().Pasajeros = builder.ToString();

                        // CHOFER 
                        Client.GetPlay().Chofer = (Client.GetPlay().PasajerosCount <= 0) ? false : true;
                        Client.GetRoomUser().AllowOverride = (Client.GetPlay().PasajerosCount <= 0) ? false : true;

                        // SI EL PASAJERO ES UN HERIDO
                        if (PJ.GetPlay().IsDying)
                        {
                            #region Send To Hospital
                            RoleplayManager.Shout(PJ, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
                            Room Room2 = RoleplayManager.GenerateRoom(PJ.GetRoomUser().RoomId);
                            string MyCity2 = Room2.City;

                            PlayRoom Data2;
                            int ToHosp2 = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity2, out Data2);

                            if (ToHosp2 > 0)
                            {
                                Room Room3 = RoleplayManager.GenerateRoom(ToHosp2);
                                if (Room3 != null)
                                {
                                    PJ.GetPlay().IsDead = true;
                                    PJ.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                                    PJ.GetHabbo().HomeRoom = ToHosp2;

                                    /*
                                    if (PJ.GetHabbo().CurrentRoomId != ToHosp)
                                        RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                                    else
                                        Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                                    */
                                    RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                                }
                                else
                                {
                                    PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                    PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                    PJ.GetPlay().RefreshStatDialogue();
                                    PJ.GetRoomUser().Frozen = false;
                                    PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                }
                            }
                            else
                            {
                                PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                PJ.GetPlay().RefreshStatDialogue();
                                PJ.GetRoomUser().Frozen = false;
                                PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                            }
                            PJ.GetPlay().IsDying = false;
                            PJ.GetPlay().DyingTimeLeft = 0;
                            #endregion
                        }

                        // SI EL PASAJERO ES COMPAÑERO DE BASURERO
                        if (PJ.GetPlay().IsBasuPasaj)
                            PJ.GetPlay().IsBasuPasaj = false;

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                    }
                }
                #endregion

                #region Check Jobs

                #region Taxista
                if (Client.GetPlay().Ficha != 0)
                {
                    Client.GetPlay().Ficha = 0;
                    Client.GetPlay().FichaTimer = 0;
                    RoleplayManager.Shout(Client, "*Apaga el Taxímetro y deja de trabajar*", 5);
                }
                #endregion

                #endregion
                #endregion

                #region Online ParkVars
                //Retornamos a valores predeterminados
                Client.GetPlay().DrivingCar = false;
                Client.GetPlay().DrivingInCar = false;
                Client.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                //Combustible System
                Client.GetPlay().CarType = 0;// Define el gasto de combustible
                Client.GetPlay().CarFuel = 0;
                Client.GetPlay().CarMaxFuel = 0;
                Client.GetPlay().CarTimer = 0;
                Client.GetPlay().CarLife = 0;

                Client.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                Client.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                Client.GetRoomUser().ApplyEffect(0);
                Client.GetRoomUser().FastWalking = false;
                #endregion

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_vehicle", "close");
                #endregion
            }
            #endregion

            #region Is Pasajero?
            if (Client.GetPlay().Pasajero)
            {
                // PASAJERO
                Client.GetPlay().Pasajero = false;
                Client.GetPlay().ChoferName = "";
                Client.GetPlay().ChoferID = 0;
                Client.GetRoomUser().CanWalk = true;
                Client.GetRoomUser().FastWalking = false;
                Client.GetRoomUser().TeleportEnabled = false;
                Client.GetRoomUser().AllowOverride = false;

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Client.GetPlay().ChoferName);

                if (TargetClient != null)
                {
                    // Descontamos Pasajero
                    TargetClient.GetPlay().PasajerosCount--;
                    if (TargetClient.GetPlay().PasajerosCount <= 0)
                        TargetClient.GetPlay().Pasajeros = "";
                    else
                        TargetClient.GetPlay().Pasajeros.Replace(Client.GetHabbo().Username + ";", "");

                    // CHOFER 
                    TargetClient.GetPlay().Chofer = (TargetClient.GetPlay().PasajerosCount <= 0) ? false : true;
                    TargetClient.GetRoomUser().AllowOverride = (TargetClient.GetPlay().PasajerosCount <= 0) ? false : true;
                }
                RoleplayManager.Shout(Client, "*Baja del vehículo*", 5);
            }
            #endregion
        }

        public static void AssingRights(GameClient Session, int RoomId, bool ChangeOwner = false)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            int UserId = Session.GetHabbo().Id;

            Room Room = GenerateRoom(RoomId);
            if (Room == null)
                return;

            // Limpiamos Permisos de Caché
            if (Room.UsersWithRights.Count > 0)
                Room.UsersWithRights.Clear();

            // Limpiamos Expulsiones de Caché
            foreach (int Id in Room.BannedUsers().ToList())
            {
                Room.Unban(Id);
            }

            Room.UsersWithRights.Add(UserId);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + Room.RoomId + "'");
                dbClient.RunQuery("DELETE FROM `room_bans` WHERE `room_id` = '" + Room.RoomId + "'");
                dbClient.RunQuery("UPDATE items SET user_id = '" + Session.GetHabbo().Id + "' WHERE room_id = '" + Room.RoomId + "'");

                if (ChangeOwner)
                {
                    Room.OwnerId = Session.GetHabbo().Id;
                    Room.OwnerName = Session.GetHabbo().Username;
                    dbClient.RunQuery("UPDATE rooms SET owner = '" + Session.GetHabbo().Id + "' WHERE id = '" + Room.RoomId + "'");
                }

                dbClient.RunQuery("INSERT INTO `room_rights` (`room_id`,`user_id`) VALUES ('" + Room.RoomId + "','" + UserId + "')");
            }

            RoomUser RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            if (RoomUser != null && !RoomUser.IsBot)
            {
                RoomUser.SetStatus("flatctrl 1", "");
                RoomUser.UpdateNeeded = true;
                if (RoomUser.GetClient() != null)
                    RoomUser.GetClient().SendMessage(new YouAreControllerComposer(1));

                Session.SendMessage(new FlatControllerAddedComposer(Room.RoomId, RoomUser.GetClient().GetHabbo().Id, RoomUser.GetClient().GetHabbo().Username));
            }
            else
            {
                UserCache User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId);
                if (User != null)
                    Session.SendMessage(new FlatControllerAddedComposer(Room.RoomId, User.Id, User.Username));
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room, true);
        }

        public static bool DeleteRoomRP(GameClient Session, int RoomId)
        {
            if(Session == null || Session.GetHabbo() == null || Session.GetHabbo().UsersRooms == null)
                return false;

            if (RoomId == 0)
                return false;

            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room))
                return false;

            RoomData data = Room.RoomData;
            if (data == null)
                return false;

            if (Room.OwnerId != Session.GetHabbo().Id && !Session.GetHabbo().GetPermissions().HasRight("room_delete_any"))
                return false;

            foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUser == null || RoomUser.GetClient() == null)
                    continue;

                SendUser(RoomUser.GetClient(), 61, "");
            }
            
            List<Item> ItemsToRemove = new List<Item>();
            foreach (Item Item in Room.GetRoomItemHandler().GetWallAndFloor.ToList())
            {
                if (Item == null)
                    continue;

                if (Item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                {
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("DELETE FROM `room_items_moodlight` WHERE `item_id` = '" + Item.Id + "' LIMIT 1");
                    }
                }

                ItemsToRemove.Add(Item);
            }

            foreach (Item Item in ItemsToRemove)
            {
                GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.UserID);
                if (targetClient != null && targetClient.GetHabbo() != null)//Again, do we have an active client?
                {
                    Room.GetRoomItemHandler().RemoveFurniture(targetClient, Item.Id);
                    //targetClient.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, Item.LimitedNo, Item.LimitedTot);
                    //targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else//No, query time.
                {
                    Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                    }
                }
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room, true);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `user_roomvisits` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunQuery("DELETE FROM `rooms` WHERE `id` = '" + RoomId + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `user_favorites` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunQuery("DELETE FROM `items` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunQuery("DELETE FROM `play_rooms` WHERE `id` = '" + RoomId + "' LIMIT 1");
                dbClient.RunQuery("UPDATE `users` SET `home_room` = '1' WHERE `home_room` = '" + RoomId + "'");
                dbClient.RunQuery("DELETE FROM `play_apartments_owned` WHERE `room_id` = '" + RoomId + "' LIMIT 1");
            }

            RoomData removedRoom = (from p in Session.GetHabbo().UsersRooms where p.Id == RoomId select p).SingleOrDefault();
            if (removedRoom != null)
                Session.GetHabbo().UsersRooms.Remove(removedRoom);

            return true;
        }

        public static bool CheckHaveProduct(GameClient Session, string productname)
        {
            Product PO = ProductsManager.getProduct(productname);
            if (PO != null)
            {
                return Session.GetPlay().OwnedProducts.ContainsKey(PO.ID);
            }

            return false;
        }

        public static void AssingInventoryProducts()
        {
            Product PO = ProductsManager.getProduct("weed");
            if (PO != null)
                WeedID = PO.ID;

            PO = ProductsManager.getProduct("cocaine");
            if (PO != null)
                CocaineID = PO.ID;

            PO = ProductsManager.getProduct("medicines");
            if (PO != null)
                MedicinesID = PO.ID;

            PO = ProductsManager.getProduct("bidon");
            if (PO != null)
                BidonID = PO.ID;

            PO = ProductsManager.getProduct("MecParts");
            if (PO != null)
                MecPartsID = PO.ID;

            PO = ProductsManager.getProduct("ArmMat");
            if (PO != null)
                ArmMatID = PO.ID;

            PO = ProductsManager.getProduct("ArmPieces");
            if (PO != null)
                ArmPiecesID = PO.ID;

            log.Info("Assignaments of Inventory Products successfully");
        }

        public static int GetStockCost(Group B)
        {
            int Cost = 0;

            if(B != null)
            {
                switch (B.GActivity)
                {
                    case "Restaurant":
                        {
                            Cost = 2;
                        }
                        break;
                    case "Consecionario":
                        {
                            Cost = 0; // No yet
                        }
                        break;
                    case "Tecnologia":
                        {
                            Cost = 0; // No yet
                        }
                        break;
                    case "Ropa":
                        {
                            Cost = 0; // No yet
                        }
                        break;
                    case "Spa":
                        {
                            Cost = 0; // No yet
                        }
                        break;
                    case "24/7":
                        {
                            Cost = 0; // No yet
                        }
                        break;

                    default:
                        break;
                }
            }

            return Cost;
        }

        public static void ClaimTurf(GameClient Session, Room Turf, Group Gang)
        {
            if (Turf.Group != null)
            {
                // Alertamos que han perdido banda
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;

                    List<Group> thegroup = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(client.GetHabbo().Id);

                    if (thegroup == null || thegroup.Count <= 0)
                        continue;

                    if (thegroup[0] != Turf.Group)
                        continue;

                    if (client.GetPlay().DisableRadio == true)
                        continue;

                    client.SendWhisper("[RADIO] ¡Hemos perdido el " + Turf.Name + "!", 30);
                }

                if (!Turf.Group.BankRuptcy)
                {
                    Turf.Group.Bank -= GangsClaimTurfBonif;
                    Turf.Group.SetBussines(Turf.Group.Bank, Turf.Group.Stock);
                    // Alertar miembros de banda que han perdido su territorio

                    Gang.Bank += GangsClaimTurfBonif;
                    Gang.SetBussines(Gang.Bank, Gang.Stock);
                    Session.SendWhisper("¡Tu banda ha saqueado $ " + String.Format("{0:N0}", GangsClaimTurfBonif) + " de las riquezas de la banda " + Turf.Group.Name + "!", 1);
                    Gang.AddLog(Session.GetHabbo().Id, Session.GetHabbo().Username + " ha capturado el " + Turf.Name + " ganando $ " + String.Format("{0:N0}", GangsClaimTurfBonif) + " para la banda.", GangsClaimTurfBonif);
                }
                else
                    Gang.AddLog(Session.GetHabbo().Id, Session.GetHabbo().Username + " ha capturado el " + Turf.Name, 0);
            }
            else
                Gang.AddLog(Session.GetHabbo().Id, Session.GetHabbo().Username + " ha capturado el " + Turf.Name, 0);

            if (Gang == null)
                return;

            // Stats Ganga
            Gang.GangTurfsTaken++;
            Gang.UpdateStat("gang_turfs_taken", Gang.GangTurfsTaken);

            // Actualizamos caché del grupo de la sala
            Turf.Group = Gang;
            Turf.RoomData.Group = Gang;
            Session.SendMessage(new NewGroupInfoComposer(Turf.RoomId, Gang.Id));

            Gang.ClaimTurf(Turf.RoomId, Session.GetPlay().TurfFlagId);

            // Actualizamos colores de la bandera en caché
            Turf.GetRoomItemHandler().LoadFurniture(true);
            Turf.GetGameMap().GenerateMaps();
            Item GroupFlag = Turf.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "army_c15_groupflag");
            if (GroupFlag != null)
            {
                PickItem(Session, GroupFlag.Id);
                Session.GetPlay().isParking = true;
                Turf.GetRoomItemHandler().SetFloorItem(Session, GroupFlag, GroupFlag.GetX, GroupFlag.GetY, GroupFlag.Rotation, true, false, true);
                Session.GetPlay().isParking = false;
            }

            // Mandamos WS del Grupo de la Sala a los users de la sala.
            foreach (RoomUser roomUser in Turf.GetRoomUserManager().GetRoomUsers())
            {
                if (roomUser == null)
                    continue;

                if (roomUser.GetClient() == null || roomUser.GetClient().GetConnection() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(roomUser.GetClient(), "event_group", "open");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(roomUser.GetClient(), "event_gang", "turf_cap_off");
            }
        }

        public static void CallTaxi(GameClient Session, Room Room)
        {
            if (Room.RoomData.TaxiNode == Session.GetPlay().TaxiNodeGo)
                return;

            #region Basic Conditions
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk || Session.GetRoomUser().Frozen)
            {
                Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás conduciendo!", 1);
                return;
            }
            if (Session.GetPlay().DrivingInCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras tienes un vehículo en marcha afuera!", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás de pasajero!", 1);
                return;
            }
            #endregion

            #region Prepare
            string IdTemplate = Room.RoomData.TaxiNode + "," + Session.GetPlay().TaxiNodeGo;
            List<int> ruta = TaxiRoomNodeTemplateManager.getTaxiRoomNodeTemplate(IdTemplate).Path;

            if (ruta == null || ruta.Count <= 0)
                return;

            RoleplayManager.TaxiBotsId++;
            List<RandomSpeech> BotSpeechList = new List<RandomSpeech>();

            RoomUser BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(
                RoleplayManager.TaxiBotsId,
                Session.GetHabbo().CurrentRoomId,
                "taxibot",
                "stand",
                "Taxi#" + Session.GetHabbo().Username,
                "",
                RoleplayManager.TaxiLook,
                Session.GetRoomUser().X,
                Session.GetRoomUser().Y,
                Session.GetRoomUser().Z,
                Session.GetRoomUser().RotBody,
                0,
                0,
                0,
                0,
                ref BotSpeechList,
                "",
                0,
                0,
                false,
                30,
                false,
                2,
                Room.RoomData.TaxiNode), null);

            //string r = "";
            //foreach (int posicion in ruta)
                //r += "[" + posicion + "]" + "->";

            //BotUser.Chat("La ruta a seguir para llegar al destino es: " + r, false, 0);

            Session.GetHabbo().TaxiPath = ruta;
            Session.GetPlay().TaxiLastIndex = 1;
            Room.GetGameMap().UpdateUserMovement(new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y), new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y), BotUser);
            #endregion

            #region Stand
            if (Session.GetRoomUser().isSitting)
            {
                Session.GetRoomUser().Statusses.Remove("sit");
                Session.GetRoomUser().Z += 0.35;
                Session.GetRoomUser().isSitting = false;
                Session.GetRoomUser().UpdateNeeded = true;
            }
            else if (Session.GetRoomUser().isLying)
            {
                Session.GetRoomUser().Statusses.Remove("lay");
                Session.GetRoomUser().Z += 0.35;
                Session.GetRoomUser().isLying = false;
                Session.GetRoomUser().UpdateNeeded = true;
            }
            #endregion

            #region Execute
            var This = Session.GetHabbo();
            var User = BotUser;

            if (User.Statusses.Count > 0)
                User.Statusses.Clear();

            User.ApplyEffect(EffectsList.TaxiChofer);
            User.FastWalking = true;

            This.TaxiChofer = User.BotData.BotId;

            This.GetClient().GetRoomUser().ApplyEffect(EffectsList.TaxiPasajero);
            This.GetClient().GetRoomUser().CanWalk = false;
            This.GetClient().GetRoomUser().FastWalking = true;

            int Price = (ruta.Count - 1) * RoleplayManager.TaxiCostJobs;
            Session.GetHabbo().Credits -= Price;
            Session.GetHabbo().UpdateCreditsBalance();
            #endregion
        }

        public static void DropTaxi(GameClient Session, string Username)
        {
            RoomUser TaxiBot = null;

            if (Session != null && Session.GetRoomUser() != null)
            {
                TaxiBot = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetBotByName("Taxi#" + Username);

                Session.GetRoomUser().ClearMovement(true);
                Session.GetRoomUser().ApplyEffect(0);
                Session.GetHabbo().TaxiChofer = 0;
                Session.GetRoomUser().FastWalking = false;
                Session.GetRoomUser().CanWalk = true;

                Session.GetHabbo().TaxiPath = null;
                Session.GetPlay().TaxiNodeGo = -1;
            }

            if (TaxiBot == null)
                return;

            TaxiBot.CanWalk = false;

            // Remove bot from room
            TaxiBot.GetRoom().GetGameMap().RemoveUserFromMap(TaxiBot, new Point(TaxiBot.X, TaxiBot.Y));
            TaxiBot.GetRoom().GetRoomUserManager().RemoveBot(TaxiBot.VirtualId, false);
        }

        public static void GuessCosechador(GameClient Session)
        {
            var Random = new CryptoRandom();
            int Chance = Random.Next(1, 101);

            #region Knife
            if (Chance == 50)// 1% de Prob.
            {
                // Insertamos nuevo producto en play_products_owned
                Product PO = ProductsManager.getProduct("knife");
                if (PO != null)
                {
                    RoleplayManager.AddProduct(Session, PO);
                    Session.GetPlay().OwnedProducts = Session.GetPlay().LoadAndReturnProducts();
                    RoleplayManager.Shout(Session, "*Encuentra un Cuchillo mientras cosechaba un cultivo de Zanahorias*", 5);
                }
            }
            #endregion

            #region No Reward
            else
            {
                //Session.SendWhisper("Esta vez no has encontrado nada en el contenedor.", 1);
            }
            #endregion
        }

        public static void TogglePassiveMode(GameClient Client)
        {
            #region Conditions

            #region Basic Conditions
            if (Client.GetPlay().Cuffed)
            {
                Client.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Client.GetRoomUser().CanWalk)
            {
                Client.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                return;
            }
            if (Client.GetPlay().IsDead)
            {
                Client.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Client.GetPlay().IsJailed)
            {
                Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Client.GetPlay().IsDying)
            {
                Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            #endregion

            #region Conditions Checks
            if (!Client.GetRoomUser().GetRoom().RoomData.SafeZoneEnabled)
            {
                Client.SendWhisper("Debes estar en una zona segura para hacer eso.", 1);
                return;
            }
            if (RoleplayManager.PurgeEvent)
            {
                Client.SendWhisper("¡No puedes hacer eso durante la purga!", 1);
                return;
            }
            if (!Client.GetPlay().PassiveMode)
            {
                if (Client.GetPlay().CurHealth < Client.GetPlay().MaxHealth)
                {
                    Client.SendWhisper("Debes tener el 100% de vida para hacer eso.", 1);
                    return;
                }
            }
            if (Client.GetPlay().IsWanted)
            {
                Client.SendWhisper("No puedes hacer eso mientras estás en la lista de buscados.", 1);
                return;
            }
            if (Client.GetPlay().IsWorking && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
            {
                Client.SendWhisper("No puedes hacer eso mientras estás trabajando de policía", 1);
                return;
            }
            if (Client.GetPlay().CamCargId == 3 || Client.GetPlay().CamCargId == 4)
            {
                Client.SendWhisper("No puedes hacer eso mientras transportas cargamentos ilegales.", 1);
                return;
            }
            if (Client.GetPlay().EquippedWeapon != null)
            {
                Client.SendWhisper("No puedes hacer eso mientras lleves un arma equipada.", 1);
                return;
            }
            #endregion

            #endregion

            Client.GetPlay().PassiveMode = !Client.GetPlay().PassiveMode;

            if (Client.GetPlay().PassiveMode)
            {
                Client.SendMessage(new RoomNotificationComposer("psv-icon", 3, "Modo Pasivo: Activado", ""));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_psv_mode|active");
                RoleplayManager.Shout(Client, "((Ha entrado en modo pasivo))", 7);
                Client.GetRoomUser().ApplyEffect(EffectsList.Passive);
            }
            else
            {
                Client.SendMessage(new RoomNotificationComposer("psv-icon", 3, "Modo Pasivo: Desactivado", ""));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_psv_mode|desactive");
                RoleplayManager.Shout(Client, "((Ha salido del modo pasivo))", 7);
                Client.GetRoomUser().ApplyEffect(EffectsList.None);
            }
        }

        public static void StaffLogOut(GameClient Client)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO staff_paysheet (user_id,login,logout,real_total_time) VALUES ("+ Client.GetHabbo().Id + ", " + Client.GetPlay().StaffLoginTime + ", " + PlusEnvironment.GetUnixTimestamp() + ", " + Client.GetPlay().StaffCounterTime + ")");
                dbClient.RunQuery();
            }
        }
    }
}
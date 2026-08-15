using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items.Wired;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus;
using Plus.Database.Interfaces;
using Plus.Database;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using System.Data;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.Rewards.Rooms.AI.Types;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom; // No recuerdo cual sería.  mIRA
            if (Room == null)
                return;

            Session.GetHabbo().HomeRoom = Room.Id;

            Session.GetPlay().InState = false;

            #region CONDITIONS RP BY JEIHDEN 
            
            #region DeathCheck
            if (Session.GetPlay().IsDead)
            {
                string MyCity = Room.City;

                PlayRoom Data;
                int ToRoomId = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);

                if (Room.Id != ToRoomId)
                {
                    AddedToRoom(Session, Room, "death", ToRoomId);
                    return;
                }
            }
            #endregion

            #region DyingCheck
            
            if (Session.GetPlay().IsDying)
            {
                if (Room.Id != Session.GetHabbo().HomeRoom)
                {
                    AddedToRoom(Session, Room, "dying", Session.GetHabbo().HomeRoom);
                    return;
                }
            }

            #endregion

            #region JailCheck

            #endregion

            #region SancCheck
            if (Session.GetPlay().IsSanc)
            {
                string MyCity = Room.City;

                PlayRoom Data;
                int ToRoomId = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetSanc(MyCity, out Data);

                if (Room.Id != ToRoomId)
                {
                    AddedToRoom(Session, Room, "sanc", ToRoomId);
                    return;
                }
            }
            #endregion

            #region Tutorial Step Check
            if (Session.GetPlay().TutorialStep == 13 && Room.WardrobeEnabled && Room.Type.Equals("public"))
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_tutorial|13");
            }
            else if (Session.GetPlay().TutorialStep == 18 && Room.PhoneStoreEnabled && Room.Type.Equals("public"))
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_tutorial|18");
            }
            else if (Session.GetPlay().TutorialStep == 23 && Room.BuyCarEnabled && Room.Type.Equals("public"))
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_tutorial|24");
            }
            else if (Session.GetPlay().TutorialStep == 27 && Room.MallEnabled && Room.Type.Equals("public"))
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_tutorial|28");
            }
            #endregion
            #endregion

            if (Session.GetHabbo().InRoom)
            {
                Room OldRoom;

                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                    return;

                if (OldRoom.GetRoomUserManager() != null)
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
            }

            if (!Room.GetRoomUserManager().AddAvatarToRoom(Session))
            {
                Room.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
                return;//TODO: Remove?
            }

            Room.SendObjects(Session);
            if (Room.HideWired && Room.CheckRights(Session, true, false))
                Session.SendMessage(new RoomNotificationComposer("furni_placement_error", "message", "El wired está oculto en esta zona."));
            //Status updating for messenger, do later as buggy.
            try
            {
                if (Session.GetHabbo().GetMessenger() != null)
                    Session.GetHabbo().GetMessenger().OnStatusChanged(true);
            }
            catch { }

            if (Session.GetHabbo().GetStats().QuestID > 0)
                PlusEnvironment.GetGame().GetQuestManager().QuestReminder(Session, Session.GetHabbo().GetStats().QuestID);

            Session.SendMessage(new RoomEntryInfoComposer(Room.RoomId, Room.CheckRights(Session, true)));
            Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
           
            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);

            if (ThisUser != null && Session.GetHabbo().PetId == 0)
                Room.SendMessage(new UserChangeComposer(ThisUser, false));

            Session.SendMessage(new RoomEventComposer(Room.RoomData, Room.RoomData.Promotion));

            if (Room.GetWired() != null)
                Room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, Session.GetHabbo());

            foreach (RoomUser Bot in Room.GetRoomUserManager().GetBots().ToList())
            {
                if (Bot.IsBot || Bot.IsPet)
                    Bot.BotAI.OnUserEnterRoom(ThisUser);
            }

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                Session.SendMessage(new FloodControlComposer((int)Session.GetHabbo().FloodTime - (int)PlusEnvironment.GetUnixTimestamp()));

            /*
            if (Session.GetHabbo().isNoob)
            {
                Session.SendMessage(new NuxAlertMessageComposer("nux/lobbyoffer/show"));
                Session.SendMessage(new NuxAlertMessageComposer("helpBubble/add/CHAT_INPUT/Puedes chatear con tus amigos escribiendo aquí."));
            }
            */
            /* BOT FRANK OFF
            if (Room.OwnerId == Session.GetHabbo().Id)
            {
                string dFrank = null;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT bot_frank FROM users WHERE id = '" + Session.GetHabbo().Id + "' LIMIT 1");
                    dFrank = dbClient.getString();
                }
                int dFrankInt = Int32.Parse(dFrank);
                DateTime dateGregorian = new DateTime();
                dateGregorian = DateTime.Today;
                int day = (dateGregorian.Day);
                if (dFrankInt != day)
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE users SET bot_frank = '" +  day + "' WHERE id = " + Session.GetHabbo().Id + ";");
                    }
                    List<RandomSpeech> BotSpeechList = new List<RandomSpeech>();
                    RoomUser BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(0, Session.GetHabbo().CurrentRoomId, "welcome", "freeroam", "Frank", "Manager del hotel", "hr-3194-38-36.hd-180-1.ch-220-1408.lg-285-73.sh-906-90.ha-3129-73.fa-1206-73.cc-3039-73", 0, 0, 0, 4, 0, 0, 0, 0, ref BotSpeechList, "", 0, 0, false, 0, false, 33), null);
                    
                   
                }
                else
                {
                    //User has already gotten today's prize :(
                }
            }
            */
        }
        
        
        public void AddedToRoom(GameClient Session, Room Room, string Type, int RoomId)
        {
            switch (Type){
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
        
    }
}
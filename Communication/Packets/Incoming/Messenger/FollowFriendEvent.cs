using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class FollowFriendEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            #region Original Algorithm
            /*
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            int BuddyId = Packet.PopInt();
            if (BuddyId == 0 || BuddyId == Session.GetHabbo().Id)
                return;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(BuddyId);
            if (Client == null || Client.GetHabbo() == null)
                return;

            if (!Client.GetHabbo().InRoom)
            {
                Session.SendMessage(new FollowFriendFailedComposer(2));
                Session.GetHabbo().GetMessenger().UpdateFriend(Client.GetHabbo().Id, Client, true);
                return;
            }
            else if (Session.GetHabbo().CurrentRoom != null && Client.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.RoomId == Client.GetHabbo().CurrentRoom.RoomId)
                    return;
            }

            Session.SendMessage(new RoomForwardComposer(Client.GetHabbo().CurrentRoomId));
            */
            #endregion

            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;
            /*
            if (!RoleplayManager.FollowFriends)
            {
                Session.SendWhisper("La administración ha desactivado el seguimiento de amigos. ¡Camina!", 1);
                return;
            }
            */
            int BuddyId = Packet.PopInt();
            /*bool IsVip = Session.GetHabbo().VIPRank < 1 ? false : true;
            int Cost = IsVip ? 0 : 3;
            int Time = IsVip ? (5 + DayNightManager.GetTaxiTime()) : (10 + DayNightManager.GetTaxiTime());
            string TaxiText = IsVip ? " VIP" : "";
            bool OnDuty = false;
            */

            if (BuddyId == 0 || BuddyId == Session.GetHabbo().Id)
                return;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(BuddyId);
            if (Client == null || Client.GetHabbo() == null)
                return;

            if (!Client.GetHabbo().InRoom)
            {
                Session.SendMessage(new FollowFriendFailedComposer(2));
                Session.GetHabbo().GetMessenger().UpdateFriend(Client.GetHabbo().Id, Client, true);
                return;
            }
            else if (Session.GetHabbo().CurrentRoom != null && Client.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.RoomId == Client.GetHabbo().CurrentRoom.RoomId)
                    return;
            }
            /*
            if (!Client.GetHabbo().AllowConsoleMessages)
            {
                if (Session.GetHabbo().InRoom)
                    Session.SendWhisper("Sorry, but this citizen has turned off their phone, so you cannot follow them.", 1);
                else
                    Session.SendNotification("Sorry, but this citizen has turned off their phone, so you cannot follow them.");
                return;
            }*/



            /*if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Session.GetRoleplay().StaffOnDuty)
                OnDuty = true;
            if (Session.GetHabbo().VIPRank > 1)
                OnDuty = true;

            
            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (!Session.GetHabbo().CurrentRoom.TaxiFromEnabled && !OnDuty)
                {
                    Session.SendWhisper("[HOLO TAXI] Sorry, we cannot taxi you out of this room!", 1);
                    return;
                }
            }*/



            /*bool PoliceCost = false;
            if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
                PoliceCost = true;
            
            if (Session.GetHabbo().Credits < Cost && Cost > 0 && !OnDuty && !PoliceCost)
            {
                Session.SendWhisper("[HOLO TAXI] You do not have enough money to get a ride!", 1);
                return;
            }

            if (Session.GetRoleplay().InsideTaxi)
            {
                Session.SendWhisper("[HOLO TAXI] I'm already picking you up! Type ':stoptaxi' if you change your mind!", 1);
                return;
            }*/

            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Client.GetHabbo().CurrentRoomId);


            /*
            if (!roomData.TaxiToEnabled && !OnDuty)
            {
                Session.SendWhisper("[HOLO TAXI] Sorry, we cannot taxi you to this room!", 1);
                return;
            }

            if (roomData.TutorialEnabled && !OnDuty)
            {
                Session.SendWhisper("You cannot taxi to a tutorial room, sorry!", 1);
                return;
            }
            
            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.TutorialEnabled && !OnDuty)
                {
                    Session.SendWhisper("You cannot taxi out of a tutorial room! Finish the tutorial instead.", 1);
                    return;
                }
            }*/



            /*Session.GetRoleplay().InsideTaxi = true;
            bool PoliceTaxi = false;
            
            if (!OnDuty)
            {
                if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
                {
                    Cost = 0;
                    Time = 5;

                    if (Session.GetRoomUser() != null)
                        Session.GetRoomUser().ApplyEffect(597);

                    Session.Shout("*Pulls out their Police Radio and calls for a quick pickup to " + roomData.Name + " [ID: " + roomData.Id + "]*", 37);
                    PoliceTaxi = true;
                }
                else
                {
                    if (Session.GetRoomUser() != null)
                        Session.GetRoomUser().ApplyEffect(596);

                    Session.Shout("*Calls a" + TaxiText + " Taxi for " + roomData.Name + " [ID: " + roomData.Id + "]*", 4);
                }

                new Thread(() =>
                {
                    for (int i = 0; i < (Time + 1) * 10; i++)
                    {
                        if (Session.GetRoleplay() == null)
                            break;

                        if (Session.GetRoleplay().InsideTaxi)
                            Thread.Sleep(100);
                        else
                            break;
                    }
                    if (Session.GetRoleplay() != null)
                    {
                        if (Session.GetRoleplay().InsideTaxi)
                        {
                            if (Cost > 0)
                            {
                                Session.GetHabbo().Credits -= Cost;
                                Session.GetHabbo().UpdateCreditsBalance();
                            }

                            if (PoliceTaxi)
                            {
                                if (Session.GetRoomUser() != null)
                                    Session.GetRoomUser().ApplyEffect(EffectsList.CarPolice);
                                Session.Shout("*Hops inside their partner's police car as they see it pull up*", 37);
                            }
                            else
                                Session.Shout("*Hops inside their" + TaxiText + " Taxi as they see it pull up*", 4);
                            RoleplayManager.SendUser(Session, roomData.Id);
                        }
                    }
                }).Start();
            }
            else
            {
                Session.Shout("*Uses their god-like powers and follows " + Client.GetHabbo().Username + "*", 23);
                RoleplayManager.SendUser(Session, Client.GetHabbo().CurrentRoomId);
                PlusEnvironment.GetGame().GetChatManager().GetCommands().LogCommand(Session.GetHabbo().Id, "follow " + Client.GetHabbo().Username, Session.GetHabbo().MachineId, "staff");
            }
            */
            if (Session.GetHabbo().GetPermissions().HasRight("can_follow_friends"))
            {

                RoleplayManager.SendUser(Session, Client.GetHabbo().CurrentRoomId);
            }
            else
            {
                Session.SendNotification("En un mundo rol no puedes seguir a tus amigos con magia. ¡Pregúntale dónde está y camina! Como en la vida real, es muy fácil. Quizás un vehículo podría ayudarte ;)");
                return;
            }
        }
    }
}

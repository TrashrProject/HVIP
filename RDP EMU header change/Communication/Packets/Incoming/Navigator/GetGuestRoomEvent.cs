using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            #region Original Algorithm (ON)
            
            int roomID = Packet.PopInt();


            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomID);
            if (roomData == null)
                return;

            /*
            if (Session.GetPlay().Chofer || Session.GetPlay().EscortingWalk)
            {
                Session.SendNotification("Lo sentimos, pero no se permite hacer uso del navegador mientras llevas pasajeros o escoltados.");
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("can_use_guest_navigator"))
            {
                Session.SendNotification("Lo sentimos, pero no se permite hacer uso del navegador.");
                return;
            }
            */

            Boolean isLoading = Packet.PopInt() == 1;
            Boolean checkEntry = Packet.PopInt() == 1;

            Session.SendMessage(new GetGuestRoomResultComposer(Session, roomData, isLoading, checkEntry));

            #region Clean Websockets (Al cambiar de sala)

            #region Groups
            // WS Groups
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "close");
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "open");
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_gang", "turf_cap_off");
            #endregion

            #region Products
            if (Session.GetPlay().ViewProducts)
            {
                // WS Products
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "close");
                Session.GetPlay().ViewProducts = false;
            }
            #endregion

            #region View Taxi List
            if (Session.GetPlay().ViewTaxiList)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_taxi", "close");
                Session.GetPlay().ViewTaxiList = false;
            }

            if (Session.GetPlay().CallingTaxi)
            {
                Session.GetPlay().TaxiNodeGo = -1;
                Session.GetPlay().BreakGeneralTimer = true;

                if (Session.GetRoomUser().CurrentEffect == EffectsList.Taxi)
                    Session.GetRoomUser().ApplyEffect(0);

                RoleplayManager.Shout(Session, "*Cancela su taxi*", 5);
            }
            #endregion

            #region Change Name
            if (Session.GetPlay().ViewChangeName)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_changename", "close");
                Session.GetPlay().ViewChangeName = false;
            }
            #endregion

            #endregion

            #endregion

            #region New Algorithm (OFF)
            /*
            int roomID = Packet.PopInt();
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomID);

            if (roomData == null)
                return;

            if (roomID != Session.GetHabbo().CurrentRoomId)
            {
                if (Session.GetPlay().Chofer || Session.GetPlay().EscortingWalk)
                {
                    Session.SendNotification("Lo sentimos, pero no se permite hacer uso del navegador mientras llevas pasajeros o escoltados.");
                    return;
                }
                if (Session.GetHabbo().GetPermissions().HasRight("can_use_guest_navigator"))
                {
                    RoleplayManager.SendUser(Session, roomData.Id);
                    // Cerramos Group WS
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "close");
                    
                }
                else
                {
                    Session.SendNotification("No tienes permitido usar esta herramienta. ¡Transportate como lo harías en la vida real!");
                    return;
                }
            }
            else
            {
                Session.SendMessage(new GetGuestRoomResultComposer(Session, roomData, true, false));
                // Cerramos Group WS
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "close");
                
            }
            */
            #endregion

        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    class KickUserEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.CheckRights(Session) && Room.WhoCanKick != 2 && Room.Group == null)
                return;

            if (Room.Group != null && !Room.CheckRights(Session, false, true))
                return;

            var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByInsideRoom(Room.Id);
            var ApartInside = PlusEnvironment.GetGame().GetApartmentOwnedManager().GetApartmentByInsideRoom(Room.Id);
            if (House == null)
            {
                if (ApartInside == null)
                {
                    Session.SendWhisper("¡No estás dentro de ninguna Casa/Apartamento para hacer eso!", 1);
                    return;
                }
            }

            if (!Room.CheckRights(Session, false, true) && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("¡Solo el propietario de la casa/apartamento puede expulsar personas!", 1);
                return;
            }

            int UserId = Packet.PopInt();
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            if (User == null || User.IsBot)
                return;

            //Cannot kick owner or moderators.
            if (Room.CheckRights(User.GetClient(), false) || User.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("No puedes echar a los propietarios de la casa o Administradores", 1);
                return;
            }

            // Habbo Orginal
            // Room.GetRoomUserManager().RemoveUserFromRoom(User.GetClient(), true, true);
            // PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModKickSeen", 1);

            #region Casa
            if (House != null)
            {
                // New RP
                RoleplayManager.Shout(Session, "*Ha echado a " + User.GetUsername() + " de la Casa*", 5);
                // Enviar a la Sala Exterior y Posición de la Puerta
                User.GetClient().GetPlay().ExitingHouse = true;
                User.GetClient().GetPlay().HouseX = House.DoorX;
                User.GetClient().GetPlay().HouseY = House.DoorY;
                User.GetClient().GetPlay().HouseZ = House.DoorZ;
                RoleplayManager.SendUserOld(User.GetClient(), House.RoomId, Session.GetHabbo().Username + " te ha echado de la casa.");
            }
            #endregion

            #region Apartament
            else if (ApartInside != null)
            {
                RoleplayManager.Shout(Session, "*Ha echado a " + User.GetUsername() + " del apartamento*", 5);
                RoleplayManager.SendUserOld(User.GetClient(), ApartInside.LobbyId, Session.GetHabbo().Username + " te ha echado del apartamento.");
            }
            #endregion
        }
    }
}

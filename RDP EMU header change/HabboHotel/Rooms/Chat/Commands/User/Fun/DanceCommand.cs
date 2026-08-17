using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class DanceCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_dance"; }
        }

        public string Parameters
        {
            get { return "%DanceId%"; }
        }

        public string Description
        {
            get { return "Haz que tu personaje baile."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (Session.GetPlay().DrivingCar || Session.GetPlay().Pasajero || Session.GetPlay().EquippedWeapon != null
                || Session.GetPlay().IsFarming || Session.GetPlay().WateringCan || Session.GetPlay().IsDying || Session.GetPlay().IsDead || Session.GetHabbo().TaxiChofer > 0)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el ID del baile. (1-4)", 1);
                return;
            }

            int DanceId;
            if (int.TryParse(Params[1], out DanceId))
            {
                if (DanceId > 4 || DanceId < 0)
                {
                    Session.SendWhisper("El ID debe ser entre 1 y 4.", 1);
                    return;
                }

                Session.GetHabbo().CurrentRoom.SendMessage(new DanceComposer(ThisUser, DanceId));
            }
            else
                Session.SendWhisper("Ingresa un ID válido.", 1);
        }
    }
}

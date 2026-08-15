using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GOTOCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_goto"; }
        }

        public string Parameters
        {
            get { return "%room_id%"; }
        }

        public string Description
        {
            get { return "Ir a una zona específica."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar el ID de la zona destino.", 1);
                return;
            }

            int RoomID;

            if (!int.TryParse(Params[1], out RoomID))
                Session.SendWhisper("Ingresa una ID válida.", 1);
            else
            {
                Room _room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(RoomID);
                if (_room == null)
                    Session.SendWhisper("Esa zona no existe.", 1);
                else
                {
                    Session.GetHabbo().PrepareRoom(_room.Id, "");
                }
            }
        }
    }
}
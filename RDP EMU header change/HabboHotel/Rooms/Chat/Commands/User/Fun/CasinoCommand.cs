using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class CasinoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_casino"; }
        }

        public string Parameters
        {
            get { return "%action%"; }
        }

        public string Description
        {
            get { return "Mantiene la cuenta de tu juego de da2."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar la acción a realizar. [start/pl/reset]", 1);
                return;
            }
            string query = Params[1];

            RoomUser roomUser = Room?.GetRoomUserManager()?.GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUser == null)
            {
                return;
            }

            List<Items.Item> userBooth = Room.GetRoomItemHandler().GetFloor.Where(x => x != null && Gamemap.TilesTouching(
                x.Coordinate, roomUser.Coordinate) && x.Data.InteractionType == Items.InteractionType.DICE).ToList();

            if (userBooth.Count != 5)
            {
                Session.SendWhisper("Debes tener 5 dados cerca para iniciar un juego de da2.", 1);
                return;
            }

            if((query.ToLower() == "pl"))
            {
                if (Session.GetHabbo().casinoCount < 19)
                    Session.SendWhisper("Solo puedes dar PL si has tirado 19 o más.", 1);
                else
                {
                    Room.SendMessage(new RoomNotificationComposer("UK202", 3, Session.GetHabbo().Username + " lleva " + Session.GetHabbo().casinoCount + " y ha dado PL.", ""));
                    Session.GetHabbo().casinoEnabled = false;
                    Session.GetHabbo().casinoCount = 0;
                }

            }
            else if(query.ToLower() == "start")
            {
                Session.SendWhisper("Has iniciado el modo casino. El contador de dados está activado.", 1);
                Session.GetHabbo().casinoEnabled = true;
               
            }
            else if(query.ToLower() == "reset")
            {
                Session.SendWhisper("Se ha reseteado el contador de dados.", 1);
                Session.GetHabbo().casinoCount = 0;
                Session.GetHabbo().casinoEnabled = false;
            }
            else
            {
                Session.SendWhisper("Debes especificar una acción válida. [start/pl/reset]", 1);
            }
            
            
        }
    }
}
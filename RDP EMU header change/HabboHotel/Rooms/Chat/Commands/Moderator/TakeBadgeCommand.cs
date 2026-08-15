using System;
using Plus.HabboHotel.GameClients;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;


namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class TakeBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_take_badge"; }
        }


        public string Parameters
        {
            get { return "%username% %badge%"; }
        }


        public string Description
        {
            get { return "Quita una placa de un usuario."; }
        }


        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 3)
            {
                GameClient TargetClient = null; //Li3s
                TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                if (TargetClient != null)
                    if (!TargetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                    {
                        {
                            Session.SendNotification("Este usuario no tiene la placa " + Params[2] + ".");
                        }
                    }
                    else
                    {
                        RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        TargetClient.GetHabbo().GetBadgeComponent().RemoveBadge(Params[2], TargetClient);
                        TargetClient.SendNotification("Tu placa " + Params[2] + " ha sido retirada por " + ThisUser.GetUsername() +".");
                        Session.SendNotification("La placa se le ha removido al usuario exitosamente.");

                    }
            }
            else
            {
                Session.SendNotification("Usuario no encontrado.");
                return;
            }
        }
    }
}
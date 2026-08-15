using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GiveBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_badge"; }
        }

        public string Parameters
        {
            get { return "%username% %badge%"; }
        }

        public string Description
        {
            get { return "Da una placa a un usuario."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Ingresa el nombre de usuario y el código de placa.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient != null)
            {
                if (!TargetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                {
                    TargetClient.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, TargetClient);
                    if (TargetClient.GetHabbo().Id != Session.GetHabbo().Id)
                        TargetClient.SendMessage(new RoomNotificationComposer("/badge/" + Params[2], 3, "¡Acabas de recibir una placa!", "/inventory/open/badge"));
                    else
                        Session.SendWhisper("Te acabas de dar la placa " + Params[2] + ".", 1);
                }
                else
                    Session.SendWhisper("¡Oops, ese usario ya tiene la placa (" + Params[2] + ") !", 1);
                return;
            }
            else
            {
                Session.SendWhisper("¡Oops, ese usuario no fue encontrado!", 1);
                return;
            }
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Handshake;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class FlagUserCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_flaguser"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Forza a una persona a cambiar su nombre."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir el nombre de una persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("No puedes hacer eso con esta persona.", 1);
                return;
            }
            else
            {
                TargetClient.GetHabbo().LastNameChange = 0;
                TargetClient.GetHabbo().ChangingName = true;
                TargetClient.SendNotification("Un Moderador ha solicitado tu cambio de nombre debido a que es inapropiado para el servidor.\r\rToma en cuenta que no se te permitirán más cambios de nombres. Si usas otro nombre inapropiado, tu cuenta será baneada y deberás crear una nueva con un nombre válido.\r\rCierra esta ventana, haz clic en tu personaje y usa la herramienta 'Cambiar nombre'.");
                TargetClient.SendMessage(new UserObjectComposer(TargetClient.GetHabbo()));
            }

        }
    }
}

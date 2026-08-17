using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Handshake;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class FlagMeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_flagme"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa la opción de cambiar tu nombre."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!this.CanChangeName(Session.GetHabbo()))
            {
                Session.SendWhisper("Lo sentimos, no tienes permiso para usar la opción de cambio de nombre.", 1);
                return;
            }

            Session.GetHabbo().ChangingName = true;
            Session.SendNotification("Por favor, establece un nombre apropiado. Si no cumple las reglas podrás ser banead@ sin previo aviso.\r\rToma en cuenta que ningún miembro del equipo Administrativo podrá darte oportunidad de cambiar nuevamente el nombre.\r\rCierra esta ventana, haz clic en tu personaje y en la opción 'Cambiar nombre'.");
            Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
        }

        private bool CanChangeName(Habbo Habbo)
        {
            if (Habbo.Rank == 1 && Habbo.VIPRank == 0 && Habbo.LastNameChange == 0)
                return true;
            else if (Habbo.Rank == 1 && Habbo.VIPRank == 1 && (Habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 604800) > Habbo.LastNameChange))
                return true;
            else if (Habbo.Rank == 1 && Habbo.VIPRank == 2 && (Habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 86400) > Habbo.LastNameChange))
                return true;
            //else if (Habbo.Rank == 1 && Habbo.VIPRank == 3)
              //  return true;
            else if (Habbo.GetPermissions().HasRight("mod_tool"))
                return true;

            return false;
        }
    }
}

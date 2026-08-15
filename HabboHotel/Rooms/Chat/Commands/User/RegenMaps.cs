using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RegenMaps : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_regen_maps"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Si tu zona está bugueada, usa este comando."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("¡Solo personas con permisos pueden hacer eso!", 1);
                return;
            }

            Room.GetGameMap().GenerateMaps();
            Session.SendWhisper("¡Zona desbugueada!", 1);
        }
    }
}

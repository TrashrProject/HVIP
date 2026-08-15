using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    class MassEnableCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_massenable"; }
        }

        public string Parameters
        {
            get { return "%EffectId%"; }
        }

        public string Description
        {
            get { return "Pon un efecto a todos los usuarios de tu zona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar un ID.", 1);
                return;
            }

            int EnableId = 0;
            if (int.TryParse(Params[1], out EnableId))
            {
                if (EnableId == 102 || EnableId == 178)
                {
                    Session.SendWhisper("No puedes colocar este efecto a los usuarios.", 1);
                    return;
                }

                if (!Session.GetHabbo().GetPermissions().HasCommand("command_override_massenable") && Room.OwnerId != Session.GetHabbo().Id)
                {
                    Session.SendWhisper("Solo puedes hacer eso en zonas de tu propiedad o con permisos de uso para este comando.", 1);
                    return;
                }

                List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();
                if (Users.Count > 0)
                {
                    foreach (RoomUser U in Users.ToList())
                    {
                        if (U == null || U.RidingHorse)
                            continue;

                        U.ApplyEffect(EnableId);
                    }
                }
            }
            else
            {
                Session.SendWhisper("Debes ingresar un ID.", 1);
                return;
            }
        }
    }
}

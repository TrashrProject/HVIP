using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Timers.Types;
using Plus.HabboRoleplay.Timers;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class UnSancCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_sanc"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Quita la sanción a jugadores mandando liberando del calabozo"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("sanc"))
                return;

            if (Params.Length != 2)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona a liberar.", 1);
                return;
            }

            Habbo Target = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("No se puedo encontrár a ningún usuario con ese nombre.", 1);
                return;
            }
            #endregion

            if(Target.GetClient() != null && !Target.GetClient().GetPlay().IsDisconnecting)
            {
                // Online
                GameClient client = Target.GetClient();

                if(!client.GetPlay().IsSanc)
                {
                    Session.SendWhisper("¡Esa persona no se encuentra sancionada!", 1);
                    return;
                }

                client.GetPlay().IsSanc = false;
                client.GetPlay().SancTimeLeft = 0;

                RoleplayManager.SaveQuickUserInfo(client, "time_muted", 0);
                client.GetHabbo().TimeMuted = 0;

                if (client.GetRoomUser() != null)
                {
                    if(client.GetRoomUser().RoomId != 1)
                        RoleplayManager.SendUserOld(client, 1);
                }
            }

            RoleplayManager.UnSanc(Target.Id);
            Session.GetPlay().CooldownManager.CreateCooldown("sanc", 1000, 5);
            RoleplayManager.Shout(Session, "*Libera a " + Target.Username + " de su sanción*", 23);
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class GiveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_give"; }
        }

        public string Parameters
        {
            get { return "%user% %amount%"; }
        }

        public string Description
        {
            get { return "Dar la cantidad de dinero deseado a una persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            int Amount;

            if (Params.Length != 3)
            {
                Session.SendWhisper("Comando inválido, usa :dar [usario] [cantidad]", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en enconrar al usuario, probablemente esté desconectado.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error en enconrar al usuario, probablemente esté desconectado o no está en esta Zona.", 1);
                return;
            }

            if (Session.GetPlay().Level < 2)
            {
                Session.SendWhisper("Debes ser nivel 2 para poder dar dinero.", 1);
                return;
            }

            if (TargetClient == Session)
            {
                Session.SendWhisper("¡No puedes darte dinero a ti mism@!", 1);
                return;
            }

            if (TargetClient.GetConnection().getIp() == Session.GetConnection().getIp())
            {
                Session.SendWhisper("¡No puedes pasar dinero entre tus cuentas!", 1);
                return;
            }

            if (int.TryParse((Params[2]), out Amount))
            {
                if (Amount <= 0)
                {
                    Session.SendWhisper("Cantidad inválida.", 1);
                    return;
                }

                if (Session.GetHabbo().Credits < Amount)
                {
                    Session.SendWhisper("No tienes $" + String.Format("{0:N0}", Amount) + " para dar.", 1);
                    return;
                }

                if (Session.GetPlay().TryGetCooldown("givecommand"))
                    return;

                Session.GetHabbo().Credits -= Amount;
                TargetClient.GetHabbo().Credits += Amount;
                Session.GetPlay().MoneyEarned += Amount;

                Session.GetHabbo().UpdateCreditsBalance();
                TargetClient.GetHabbo().UpdateCreditsBalance();

                RoleplayManager.Shout(Session, "*Da $" + String.Format("{0:N0}", Amount) + " a " + TargetClient.GetHabbo().Username + "*", 5);
                TargetClient.SendWhisper("Has recibido $" + String.Format("{0:N0}", Amount) + " de " + Session.GetHabbo().Username, 1);
                Session.GetPlay().CooldownManager.CreateCooldown("givecommand", 1000, 3);
            }
            else
                Session.SendWhisper("Ingrese una cantidad válida.", 1);
        }
    }
}
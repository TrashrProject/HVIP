using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class OnlinesCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_onlines"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ver los usuarios conectados."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("ons"))
                return;
            #endregion

            int onli = 0;
            string Online = "";
            string Head = "";
            string str = "";

            lock (PlusEnvironment.GetGame().GetClientManager()._clients.Values)
            {
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager()._clients.Values)
                {
                    if (client == null || client.GetHabbo() == null) { continue; }
                    Online += "" + client.GetHabbo().Username + "\n";
                    onli++;
                }
            }

            Head += "============================================\n";
            Head += "          Usuarios conectados ("+onli+")        \n";
            Head += "============================================\n\n";

            str += Head + Online;
            Session.SendNotifWithScroll(str);
            Session.GetPlay().CooldownManager.CreateCooldown("ons", 1000, 5);
        }
    }
}

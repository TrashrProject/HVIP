using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.Database.Adapter;
using Plus.HabboHotel.Users.UserDataManagement;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class GambleCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_gamble"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Apuesta en las maquinas tragaperras"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room CurrentRoom, string[] Params)
        {
            if (Session == null)
                return;

            int Bet = 0;

            try
            {
                Bet = int.Parse(Params[1]);
            }
            catch
            {
                Session.SendWhisper("La cantidad introducida no es valida. Apuesta entre 1 a 500,000 Creditos. Suerte.", 3);
                return;
            }
            if (Bet < 1 || Bet > 500000)
            {
                Session.SendWhisper("Puedes apostar de 1 Credito a 500000 Creditos.", 3);
                return;
            }

            if (Session.GetHabbo().Credits < Bet)
            {
                Session.SendWhisper("No tienes Creditos suficiente spara apostar.", 3);
                return;
            }

            Session.GetHabbo().SlotsInteractation = Bet;
            Session.SendWhisper("Has puesto la maquina tragaperra en " + Bet + " Creditos, has Doble clic en la maquina para apostar.");
        }
    }
}
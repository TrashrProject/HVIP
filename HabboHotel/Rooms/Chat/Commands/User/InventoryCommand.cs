using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Notifications;


using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Polls;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Polls.Questions;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class InventoryCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_inventory"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Abre tu inventario."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            string Stats = "";
            Stats += "\n============================================\n                  INVENTARIO \n============================================\n";

            Stats += "\n============= CONSUMIBLES ====================\n";
            Stats += "Crack: " + Session.GetPlay().Cocaine + " gramo(s)\n";
            Stats += "Medicamentos: " + Session.GetPlay().Medicines + " medicamento(s)\n";
            Stats += "Marihuana: " + Session.GetPlay().Weed + " gramo(s)\n";

            Stats += "\n============= ARMAS ====================\n";
            Stats += "Armas: ";
            Session.GetPlay().OwnedWeapons = null;
            Session.GetPlay().OwnedWeapons = Session.GetPlay().LoadAndReturnWeapons();
            if (Session.GetPlay().OwnedWeapons.Count > 0)
            {
                foreach (KeyValuePair<string, Weapon> Weapon in Session.GetPlay().OwnedWeapons.ToList())
                {
                    Stats += Weapon.Value.Name + " (Estado: "+Weapon.Value.WLife+"/100), ";
                }
            }
            else
                Stats += "Ninguna";
            Stats += "\n";
            Stats += "Piezas de Armas: " + Session.GetPlay().ArmPieces + "\n";
            Stats += "Materiales para Piezas: " + Session.GetPlay().ArmMat + "\n";

            Stats += "\n============= OBJETOS ====================\n";
            if (RoleplayManager.CheckHaveProduct(Session, "knife"))
                Stats += "Cuchillo\n";
            if (RoleplayManager.CheckHaveProduct(Session, "destornillador"))
                Stats += "Destornillador\n";
            if (RoleplayManager.CheckHaveProduct(Session, "palanca"))
                Stats += "Palanca\n";
            if (Session.GetPlay().MecParts > 0)
                Stats += Session.GetPlay().MecParts + " Repuesto(s)\n";
            if (Session.GetPlay().Bidon > 0)
                Stats += Session.GetPlay().Bidon + " Bidon(es)\n";

            Stats += "\n============= FARMING ====================\n";
            if (Session.GetPlay().FarmSeeds == true)
                Stats += "Semillas de Zanahorias\n";
            if (Session.GetPlay().WateringCan == true)
                Stats += "Regadera con agua\n";
            Stats += "============================================\n";
            Session.SendMessage(new MOTDNotificationComposer(Stats));
        }
    }
}

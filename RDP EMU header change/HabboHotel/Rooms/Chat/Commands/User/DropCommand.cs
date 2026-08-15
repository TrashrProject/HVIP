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
    class DropCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_drop"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Tira algo de tu inventario."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetPlay().TryGetCooldown("drop"))
                return;

            if (Params.Length != 2)
            {
                Session.SendWhisper("Comando inválido, debes especificar el objeto a tirar. ((:tirar [item]))", 1);
                return;
            }
            string Type = Params[1].ToLower();

            switch (Type)
            {
                #region Regadera
                case "regadera":
                    {
                        if (!Session.GetPlay().WateringCan)
                        {
                            Session.SendWhisper("No tienes ninguna regadera para tirar.", 1);
                            return;
                        }
                        if (Session.GetPlay().IsFarming)
                        {
                            Session.SendWhisper("¡No puedes hacer eso mientras cultivas!", 1);
                            return;
                        }

                        Session.GetPlay().WateringCan = false;
                        RoleplayManager.Shout(Session, "*Tira una regadera con agua deshaciendose de ella*", 5);
                        Session.GetPlay().CooldownManager.CreateCooldown("drop", 1000, 3);
                    }
                    break;
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("'"+ Type +"' no es un objeto válido para tirar.", 1);
                    }
                    break;
                #endregion
            }

        }
    }
}

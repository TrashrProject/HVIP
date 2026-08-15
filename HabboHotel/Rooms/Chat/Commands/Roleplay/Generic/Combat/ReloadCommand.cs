using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
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
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class ReloadGunCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_reload_gun"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Recarga de balas tu arma."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            var Weapon = Session.GetPlay().EquippedWeapon;

            if (Weapon == null)
            {
                Session.SendWhisper("¡No tienes ningún arma Equipada para recargar!", 1);
                return;
            }
            if (Session.GetPlay().GunShots <= 0 && Session.GetPlay().Bullets == Weapon.ClipSize)
            {
                Session.SendWhisper("¡Tu arma está completamente cargada!", 1);
                return;
            }
            
            Weapon.Reload(Session);
            Session.GetPlay().CooldownManager.CreateCooldown("reload", 1000, Weapon.ReloadTime);
        }
    }
}
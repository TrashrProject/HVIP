using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class UnEquipCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_equip_undo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desequipar el arma en uso."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (Session.GetRoomUser().Frozen)
                return;

            if (Session.GetPlay().EquippedWeapon == null)
            {
                Session.SendWhisper("No tienes esa arma equipada.", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("unequip", true))
                return;

            #endregion

            #region Execute

            RoleplayManager.UpdateMyWeaponStats(Session, "bullets", Session.GetPlay().Bullets, Session.GetPlay().EquippedWeapon.Name);
            RoleplayManager.UpdateMyWeaponStats(Session, "life", Session.GetPlay().WLife, Session.GetPlay().EquippedWeapon.Name);

            #region Con Probabilidad (OFF)
            /*
            CryptoRandom Random = new CryptoRandom();
            int Chance = Random.Next(1, 101);

            if (Chance <= 8)
            {
                Session.Shout("*Tries to slide their " + Session.GetPlay().EquippedWeapon.PublicName + " back into its holster, but fails*", 4);
                return;
            }
            else
            {
                string UnEquipMessage = Session.GetPlay().EquippedWeapon.UnEquipText;
                UnEquipMessage = UnEquipMessage.Replace("[NAME]", Session.GetPlay().EquippedWeapon.PublicName);

                Session.Shout(UnEquipMessage, 4);

                if (Session.GetRoomUser().CurrentEffect == Session.GetPlay().EquippedWeapon.EffectID)
                    Session.GetRoomUser().ApplyEffect(0);

                if (Session.GetRoomUser().CarryItemID == Session.GetPlay().EquippedWeapon.HandItem)
                    Session.GetRoomUser().CarryItem(0);

                Session.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                Session.GetPlay().EquippedWeapon = null;
                return;
            }*/
            #endregion
            string UnEquipMessage = Session.GetPlay().EquippedWeapon.UnEquipText;
            UnEquipMessage = UnEquipMessage.Replace("[NAME]", Session.GetPlay().EquippedWeapon.PublicName);

            RoleplayManager.Shout(Session, UnEquipMessage, 5);

            if (Session.GetRoomUser().CurrentEffect == Session.GetPlay().EquippedWeapon.EffectID)
                Session.GetRoomUser().ApplyEffect(0);

            if (Session.GetRoomUser().CarryItemID == Session.GetPlay().EquippedWeapon.HandItem)
                Session.GetRoomUser().CarryItem(0);

            Session.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
            Session.GetPlay().EquippedWeapon = null;
            
            Session.GetPlay().Bullets = 0;

            #region Sound System
            foreach (RoomUser RoomUsers in Session.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUsers == null || RoomUsers.GetClient() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|unequip");
            }
            #endregion
            return;
            #endregion
        }
    }
}
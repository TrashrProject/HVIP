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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class EquipCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_equip"; }
        }

        public string Parameters
        {
            get { return "%weapon%"; }
        }

        public string Description
        {
            get { return "Equipar un arma."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre del arma a equipar.", 1);
                return;
            }
            if(Session.GetPlay().Level < 2)
            {
                Session.SendWhisper("Necesitas nivel 2 para poder equipar armas.", 1);
                return;
            }
            string GunName = Params[1].ToLower();
            Weapon BaseWeapon = WeaponManager.getWeapon(GunName);

            if (BaseWeapon == null)
            {
                Session.SendWhisper("¡Esa arma no existe!", 1);
                return;
            }

            if (Session.GetRoomUser().Frozen)
                return;

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes equipar armas en modo pasivo.", 1);
                return;
            }

            if (Session.GetPlay().EquippedWeapon != null)
            {
                if (Session.GetPlay().EquippedWeapon.Name == BaseWeapon.Name)
                {
                    Session.SendWhisper("Ya tienes equipada esa arma.", 1);
                    return;
                }
            }

            if (!Session.GetPlay().OwnedWeapons.ContainsKey(GunName))
            {
                Session.SendWhisper("No posees esa arma.", 1);
                return;
            }
            /*
            if (!Session.GetPlay().OwnedWeapons[GunName].CanUse)
            {
                Session.SendWhisper("¡No puedes usar esa arma hasta pagar un monto de $" + String.Format("{0:N0}", Session.GetPlay().OwnedWeapons[GunName].CostFine) + "!", 1);
                return;
            }
            */
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                return;
            }

            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes equipar tu " + GunName + ", tus manos están esposadas.", 1);
                return;
            }
            if (Session.GetPlay().LoadingTimeLeft > 0 || Session.GetPlay().IsFarming)
            {
                Session.SendWhisper("¡No puedes hacer eso hasta que termines de realizar tu acción actual!", 1);
                return;
            }
            if (Session.GetPlay().WateringCan)
            {
                Session.SendWhisper("¡Debes :tirar la regadera para poder portar armas!", 1);
                return;
            }
            /*
            if (BaseWeapon.LevelRequirement > Session.GetPlay().Level)
            {
                Session.SendWhisper("Necesitas ser Nivel " + BaseWeapon.LevelRequirement + " para usar esta arma.", 1);
                return;
            }
            */
            if (Session.GetPlay().TryGetCooldown("equip", true))
                return;
            /*
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("Debes dejar de conducir para equipar un arma.", 1);
                return;
            }
            */
            #endregion

            #region Execute
            if(Session.GetPlay().EquippedWeapon != null)
            {
                RoleplayManager.UpdateMyWeaponStats(Session, "bullets", Session.GetPlay().Bullets, Session.GetPlay().EquippedWeapon.Name);
                RoleplayManager.UpdateMyWeaponStats(Session, "life", Session.GetPlay().WLife, Session.GetPlay().EquippedWeapon.Name);
            }

            var Weapon = Session.GetPlay().OwnedWeapons[GunName];

            string EquipMessage = Weapon.EquipText;
            EquipMessage = EquipMessage.Replace("[NAME]", Weapon.PublicName);

            RoleplayManager.Shout(Session, EquipMessage, 5);
            //Session.SendWhisper("Your " + Weapon.PublicName + " has " + Weapon.Clip + "/" + Weapon.ClipSize + " bullets in the magazine.", 1);

            Session.GetPlay().EquippedWeapon = Weapon;
            Session.GetPlay().Bullets = Weapon.TotalBullets;
            Session.GetPlay().WLife = Weapon.WLife;

            Session.GetPlay().CooldownManager.CreateCooldown("equip", 1000, 3);

            if (Session.GetRoomUser().CurrentEffect != Weapon.EffectID)
                Session.GetRoomUser().ApplyEffect(Weapon.EffectID);

            if (Session.GetRoomUser().CarryItemID != Weapon.HandItem)
                Session.GetRoomUser().CarryItem(Weapon.HandItem);

            #region Sound System
            foreach (RoomUser RoomUsers in Session.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUsers == null || RoomUsers.GetClient() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUsers.GetClient(), "event_feedcomposer", "sound|reload|" + Session.GetPlay().EquippedWeapon.Name);
            }
            #endregion
            return;
            #endregion
        }
    }
}
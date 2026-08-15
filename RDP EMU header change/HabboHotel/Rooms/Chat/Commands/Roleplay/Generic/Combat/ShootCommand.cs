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
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class ShootCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_shoot"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Dispara el arma hacia alguna persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Comando inválido, usa :disparar [nombre-objetivo].", 1);
                return;
            }

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes hacer uso de armas en modo pasivo.", 1);
                return;
            }

            if (Session.GetPlay().EquippedWeapon == null)
            {
                Session.SendWhisper("¡No tienes ningún arma Equipada! Usa :equipar [nombre-arma]", 1);
                return;
            }

            GameClients.GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en esta Zona.", 1);
                return;
            }
            if(TargetClient.GetHabbo().EscortID > 0)
            {
                Session.SendWhisper("¡No puedes dispararle a una persona que va siendo escoltada!", 1);
                return;
            }
            if (TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("¡No puedes dispararle a una persona que va en Taxi!", 1);
                return;
            }
            if (TargetClient.GetPlay().Cuffed)
            {
                Session.SendWhisper("¡No puedes dispararle a una persona esposada!", 1);
                return;
            }
            if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes dispararle a una persona muerta!", 1);
                return;
            }
            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes dispararle a una persona encarcelada!", 1);
                return;
            }
            if (TargetClient.GetConnection().getIp() == Session.GetConnection().getIp())
            {
                Session.SendWhisper("¡No puedes dispararle a tus propias cuentas!", 1);
                return;
            }
            if (Session.GetPlay().WLife <= 0)
            {
                Session.SendWhisper("¡Tu arma está dañada! Busca a un Armero para que la repare.", 1);
                return;
            }

            if ((Session.GetPlay().DrivingCar))
            {
                if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "law") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras vas dentro de un vehículo!", 1);
                    return;
                }
            }


            #region Fuego amigo entre bandas
            List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);
            List<Group> EnemyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(TargetClient.GetHabbo().Id);

            if (MyGang != null && MyGang.Count > 0)
            {
                if (EnemyGang != null && EnemyGang.Count > 0 && EnemyGang[0] == MyGang[0])
                {
                    Session.SendWhisper("¡No puedes dispararle a tus compañeros de banda!", 1);
                    return;
                }
            }
            #endregion

            // New Target System
            Session.GetPlay().Target = TargetClient.GetHabbo().Username;

            Session.GetPlay().LastCommand = ":shoot " + Params[1];
            CombatManager.GetCombatType("gun").Execute(Session, TargetClient);
        }
    }
}
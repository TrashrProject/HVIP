using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class CuffCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_cuff"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Esposa a una persona para arrestarla."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
                {
                    Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                    return;
                }
                if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                {
                    Session.SendWhisper("¡No puedes hacer eso entre compañeros de trabajo!", 1);
                    return;
                }
            }
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "cuff") && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3 && TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                return;
            }
            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacer eso mientras vas en taxi))", 1);
                return;
            }
            if (TargetClient.GetPlay().Cuffed)
            {
                Session.SendWhisper("Esa persona ya está esposada.", 1);
                return;
            }
            if (TargetClient.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona muerta!", 1);
                return;
            }

            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona encarcelada!", 1);
                return;
            }
            if (TargetClient.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona mientras va conduciendo!", 1);
                return;
            }
            if (TargetClient.GetPlay().Pasajero || TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona mientras va de pasajer@!", 1);
                return;
            }
            if (!TargetClient.GetRoomUser().Frozen)
            {
                Session.SendWhisper("¡Debes paralizar primero a esa persona!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes esposar a un usuario ausente!", 1);
                return;
            }
            if (TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona que está en modo pasivo!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                if (TargetClient.GetPlay().EquippedWeapon != null)
                {
                    RoleplayManager.Shout(Session, "*Confisca la " + TargetClient.GetPlay().EquippedWeapon.PublicName + " de " + TargetClient.GetHabbo().Username + " y l@ guarda*", 37);

                    if (RoleplayManager.ConfiscateWeapons)
                    {
                        using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            DB.SetQuery("UPDATE `play_weapons_owned` SET `can_use` = '0' WHERE `user_id` = @userid AND `base_weapon` = @baseweapon LIMIT 1");
                            DB.AddParameter("userid", TargetClient.GetHabbo().Id);
                            DB.AddParameter("baseweapon", TargetClient.GetPlay().EquippedWeapon.Name.ToLower());
                            DB.RunQuery();
                        }

                        TargetClient.GetPlay().EquippedWeapon = null;
                        TargetClient.GetPlay().OwnedWeapons = null;
                        TargetClient.GetPlay().OwnedWeapons = TargetClient.GetPlay().LoadAndReturnWeapons();
                    }
                    else
                        TargetClient.GetPlay().EquippedWeapon = null;
                }
                RoleplayManager.Shout(Session, "*Saca las esposas de su cinturón y las coloca en las muñecas de " + TargetClient.GetHabbo().Username + "*", 37);
                TargetClient.GetPlay().Cuffed = true;
                TargetClient.GetPlay().CuffedTimeLeft = RoleplayManager.CuffedTime;
                TargetClient.GetPlay().TimerManager.CreateTimer("cuff", 1000, false);
                if (TargetClient.GetRoomUser() != null)
                    TargetClient.GetRoomUser().ApplyEffect(590);
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca para hacer eso.", 1);
                return;
            }
            #endregion
        }
    }
}
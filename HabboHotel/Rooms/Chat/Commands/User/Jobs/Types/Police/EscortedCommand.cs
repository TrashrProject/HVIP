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
using Plus.Utilities;
using Plus.HabboHotel.Pathfinding;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class EscortedCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_escorted"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Somete a una persona esposada para llevarla a la cárcel."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona. ((:escoltar [nombre]))", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar || Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás dentro de un vehículo!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacer eso mientras vas en taxi))", 1);
                return;
            }
            if (TargetClient.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer eso a una persona muerta!", 1);
                return;
            }
            if (TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso a una persona encarcelada!", 1);
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
            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                {
                    Session.SendWhisper("¡No puedes hacer eso entre compañeros de trabajo!", 1);
                    return;
                }
            }
            if (TargetClient == Session)
            {
                Session.SendWhisper("No puedes escoltarte a ti mism@.", 1);
                return;
            }
            if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3)
            {
                Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                return;
            }
            if (!TargetClient.GetPlay().Cuffed)
            {
                Session.SendWhisper("¡Primero debes esposar al Convicto!", 1);
                return;
            }
            
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "stun") && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
                return;
            }
            
            if (!Session.GetPlay().IsWorking && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            
            RoomUser RoomUser = Session.GetRoomUser();
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetClient.GetRoomUser().X, TargetClient.GetRoomUser().Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance > 1)
            {
                Session.SendWhisper("¡Debes acercarte más a la persona!", 1);
                return;
            }
            if (Session.GetPlay().CallingTaxi)
			{
                Session.SendWhisper("No puedes escoltar a alguien mientras esperas un taxi. ¡Cancelalo primero!", 1);
                return;
            }
            if (TargetClient.GetHabbo().EscortID > 0)
            {
                Session.SendWhisper("Esa persona ya está siendo escoltada.", 1);
                return;
            }
            if (Session.GetHabbo().Escorting > 0)
            {
                Session.SendWhisper("No puedes escoltar a más de una persona a la vez. ¡Pide refuerzos!", 1);
                return;
            }
            if (TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("No puedes escoltar a una persona que va en Taxi.", 1);
                return;
            }
            if (TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona que está en modo pasivo!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("escoltado"))
                return;
            #endregion

            #region Execute

            #region Desparalizar
            TargetClient.GetRoomUser().Frozen = false;
            TargetClient.GetRoomUser().CanWalk = true;
            TargetClient.GetRoomUser().ClearMovement(true);
            #endregion

            #region Stand
            if (TargetClient.GetRoomUser().isSitting)
            {
                TargetClient.GetRoomUser().Statusses.Remove("sit");
                TargetClient.GetRoomUser().Z += 0.35;
                TargetClient.GetRoomUser().isSitting = false;
                TargetClient.GetRoomUser().UpdateNeeded = true;
            }
            else if (TargetClient.GetRoomUser().isLying)
            {
                TargetClient.GetRoomUser().Statusses.Remove("lay");
                TargetClient.GetRoomUser().Z += 0.35;
                TargetClient.GetRoomUser().isLying = false;
                TargetClient.GetRoomUser().UpdateNeeded = true;
            }
            #endregion

            #region Execute
            var This = Session.GetHabbo();
            var User = TargetClient.GetRoomUser();

            This.GetClient().GetRoomUser().SetRot(Rotation.Calculate(This.GetClient().GetRoomUser().X, This.GetClient().GetRoomUser().Y, User.X, User.Y), false);
            int dis = Math.Abs(User.X - This.GetClient().GetRoomUser().X) + Math.Abs(User.Y - This.GetClient().GetRoomUser().Y);
            {

                if (This.GetClient().GetRoomUser() != User)
                    User.SetRot(Rotation.Calculate(This.GetClient().GetRoomUser().X, This.GetClient().GetRoomUser().Y, User.X, User.Y), false);

                if (User.Statusses.Count > 0)
                    User.Statusses.Clear();

                User.ClearMovement(true);
                This.GetClient().GetRoomUser().ClearMovement(true);
                User.GetClient().GetHabbo().EscortID = This.Id;
                This.Escorting = User.HabboId;

                This.GetClient().GetRoomUser().UpdateNeeded = true;
                User.UpdateNeeded = true;

                User.CanWalk = false;
                RoleplayManager.Shout(Session, "*Comienza a  escoltar a " + TargetClient.GetHabbo().Username + "*", 37);
                Session.GetPlay().CooldownManager.CreateCooldown("escoltado", 1000, 5);
            }
            #endregion
            #endregion
        }
    }
}
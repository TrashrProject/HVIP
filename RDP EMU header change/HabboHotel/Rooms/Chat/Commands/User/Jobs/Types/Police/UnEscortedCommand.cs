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
    class UnEscortedCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_unescorted"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dejar de Escoltar al Convicto sometido por ti."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás conduces!", 1);
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

            if (Session.GetHabbo().Escorting <= 0)
            {
                Session.SendWhisper("No te encuentras escoltando a nadie.", 1);
                return;
            }
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PlusEnvironment.GetUsernameById(Session.GetHabbo().Escorting));
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "stun") && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
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
            if (!Session.GetPlay().IsWorking && !Session.GetPlay().PoliceTrial && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3)
            {
                Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                return;
            }
            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacer eso mientras vas en taxi))", 1);
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
            RoomUser RoomUser = Session.GetRoomUser();
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetClient.GetRoomUser().X, TargetClient.GetRoomUser().Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance > 1)
            {
                Session.SendWhisper("¡Debes acercarte más a la persona!", 1);
                return;
            }

            if (Session.GetPlay().TryGetCooldown("escoltado"))
                return;
            #endregion

            #region Execute          

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

            var This = Session.GetHabbo();
            var User = TargetClient.GetRoomUser();

            RoleplayManager.Shout(Session, "*Deja de escoltar a " + TargetClient.GetHabbo().Username + "*", 37);
            User.ClearMovement(true);
            TargetClient.GetRoomUser().CanWalk = true;
            User.GetClient().GetHabbo().EscortID = 0;
            This.Escorting = 0;

            Session.GetPlay().CooldownManager.CreateCooldown("escoltado", 1000, 5);
            #endregion
        }
    }
}
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Drawing;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Pathfinding;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class HugCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_hug"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Abrazar a una persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir el nombre de la persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }

            if (Session.GetPlay().TryGetCooldown("hug"))
                return;

            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacer eso mientras vas en taxi))", 1);
                return;
            }
            if (TargetClient.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("((No puedes hacerle eso a una persona que va de pasajer@))", 1);
                return;
            }
            if (TargetClient.LoggingOut)
            {
                Session.SendWhisper("((Esta persona se encuentra desconectándose.))", 1);
                return;
            }
            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("No puedes abrazar a una persona ausente.", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                int Rot = Rotation.Calculate(TargetUser.X, TargetUser.Y, Session.GetRoomUser().X, Session.GetRoomUser().Y);
                TargetUser.SetRot(Rot, false);
                TargetUser.UpdateNeeded = true;

                Rot = Rotation.Calculate(Session.GetRoomUser().X, Session.GetRoomUser().Y, TargetUser.X, TargetUser.Y);
                Session.GetRoomUser().SetRot(Rot, false);
                Session.GetRoomUser().UpdateNeeded = true;

                RoleplayManager.Shout(Session, "*Extiende sus brazos alrededor de " + TargetClient.GetHabbo().Username + ", y le da un fuerte abrazo*", 12);
                RoleplayManager.Shout(TargetClient, "*se sorprende y sonríe*", 12);
                Session.GetPlay().CooldownManager.CreateCooldown("hug", 1000, 5);
                RoomUser.ApplyEffect(EffectsList.Love);
                TargetUser.ApplyEffect(EffectsList.Love);
                Session.GetPlay().HugTimer = 5;
                TargetClient.GetPlay().HugTimer = 5;
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a abrazar.", 1);
                return;
            }
            #endregion
        }
    }
}
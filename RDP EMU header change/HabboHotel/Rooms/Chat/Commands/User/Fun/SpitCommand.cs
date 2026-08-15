using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class SpitCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_spit"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Escupe a una persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar un nombre de usuario.", 1);
                return;
            }

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes realizar acciones agresivas en modo pasivo.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a esa persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a esa persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }

            if (Session.GetPlay().TryGetCooldown("spit"))
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
                Session.SendWhisper("No puedes darle una bofetada a una persona ausente.", 1);
                return;
            }

            if (TargetClient.GetPlay().IsGodMode)
            {
                Session.SendWhisper("¡No puedes hacer eso a una persona con inmunidad!", 1);
                return;
            }

            if (Session.GetPlay().IsGodMode)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras tengas inmunidad!", 1);
                return;
            }

            if (TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona en modo pasivo!", 1);
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

                RoleplayManager.Shout(Session, "*Escupe un gargajo de saliva en el rostro de " + TargetClient.GetHabbo().Username + "*", 27);
                RoleplayManager.Shout(TargetClient, "*Guacarea del asco limpiando su rostro*", 27);
                Session.GetPlay().CooldownManager.CreateCooldown("spit", 1000, 3);
                RoomUser.ApplyEffect(EffectsList.Twinkle);
                TargetUser.ApplyEffect(EffectsList.Twinkle);
                Session.GetPlay().RapeTimer = 5;
                TargetClient.GetPlay().RapeTimer = 5;
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a escupir.", 1);
                return;
            }
            #endregion
        }
    }
}
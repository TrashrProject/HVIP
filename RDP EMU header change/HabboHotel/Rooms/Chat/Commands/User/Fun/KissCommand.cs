using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System.Threading.Tasks;
using System.Drawing;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class KissCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_kiss"; }
        }

        public string Parameters
        {
            get { return "%target%"; }
        }

        public string Description
        {
            get { return "Besa a otra persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            /* OLD
            if (Params.Length == 1)
            {
                Session.SendWhisper("Escribe el nombre de la otra persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("¡Oops! No se pudo encontrar a esa persona.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetUser == null)
            {
                Session.SendWhisper("¡Oops! Probablemente esa persona no se encuentre en esta zona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("El amor propio es importante :)", 1);
                return;
            }


            if (TargetUser.isLying || TargetUser.isSitting)
            {
                Session.SendWhisper("No es posible completar esa acción en esa posición.", 1);
                return;
            }

            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (!((Math.Abs(TargetUser.X - ThisUser.X) > 1) || (Math.Abs(TargetUser.Y - ThisUser.Y) > 1)))
            {
                Task.Run(async delegate
                {
                    TargetUser.ApplyEffect(168);
                    ThisUser.ApplyEffect(168);
                    Room.SendMessage(new ChatComposer(ThisUser.VirtualId, " *Se acerca lentamente y besa a " + Params[1] + "*", 0, 16));
                    Room.SendMessage(new ChatComposer(TargetUser.VirtualId, "*Se sonroja*", 0, 16));
                    await Task.Delay(1000);
                    TargetUser.ApplyEffect(0);
                    ThisUser.ApplyEffect(0);
                    / *TargetUser.RotBody--;//
                    TargetUser.ApplyEffect(168);
                    TargetUser.Z -= 0.35;
                    TargetUser.isLying = true;
                    TargetUser.UpdateNeeded = true;* /
                });
            }
            else
            {
                Session.SendWhisper("¡Oops! " + Params[1] + " está muy lejos de ti.", 1);
            }
            */
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
                Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada. o no está en esta zona.", 1);
                return;
            }

            if (Session.GetPlay().TryGetCooldown("kiss"))
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
                Session.SendWhisper("No puedes besar a alguien ausente.", 1);
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

                RoleplayManager.Shout(Session, "*Se inclina hacia " + TargetClient.GetHabbo().Username + " y le da un beso en los labios*", 16);
                RoleplayManager.Shout(TargetClient, "*Se sonroja*", 16);
                Session.GetPlay().CooldownManager.CreateCooldown("kiss", 1000, 5);
                RoomUser.ApplyEffect(EffectsList.Love);
                TargetUser.ApplyEffect(EffectsList.Love);
                Session.GetPlay().KissTimer = 5;
                TargetClient.GetPlay().KissTimer = 5;
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona para besarla.", 1);
                return;
            }
            #endregion
        }
    }
}
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
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ReanimCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_reanim"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Reanima a una persona inconsciente en la calle de la ciudad."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "reviewhosp"))
            {
                Session.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                return;
            }
            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                return;
            }
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
            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }

            if (!TargetClient.GetPlay().IsDying)
            {
                Session.SendWhisper("¡Esa persona no se encuentra inconsciente!", 1);
                return;
            }
            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes reanimar a un usuario ausente!", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás mert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                return;
            }
            if (TargetClient.GetPlay().PediMedico == false)
            {
                Session.SendWhisper("¡Esta persona no ha pedido una Ambulancia!", 1);
                return;
            }
            if (TargetClient.GetPlay().TargetReanim == true)
            {
                Session.SendWhisper("¡Esta persona ha sido reanimada!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("reanim", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 2)
            {
                RoleplayManager.Shout(Session, "*Atiende a " + TargetClient.GetHabbo().Username + " logrando reanimarlo*", 5);
                Session.SendWhisper("¡Bien hecho! ((Ahora  usa :subirpaciente [nombre], trasladalo al Hospital y usa :salvar su vida para recibir tu pago.", 1);
                TargetClient.GetPlay().TargetReanim = true;// Paciente reanimado SÍ
                if (TargetClient.GetPlay().PediMedico)
                    TargetClient.GetPlay().PediMedico = false;

                TargetClient.GetPlay().CooldownManager.CreateCooldown("reanim", 1000, 5);
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a reanimar.", 1);
                return;
            }
            #endregion
        }
    }
}
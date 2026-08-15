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
    class AttendPatientCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_attend"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Atiende con tu remedio a tu paciente previamente revisado."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "reviewhosp"))
            {
                Session.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                return;
            }
            string MyCity = Room.City;
            int HospID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out PlayRoom Data);//hospital de la cd.
            if (Session.GetHabbo().CurrentRoomId != HospID)
            {
                Session.SendWhisper("No puedes hacer eso fuera del Hospital.", 1);
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

            if (!TargetClient.GetPlay().IsDead)
            {
                Session.SendWhisper("¡Esa persona no se encuentra herida!", 1);
                return;
            }
            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes atender a un usuario ausente!", 1);
                return;
            }
            if (Session.GetPlay().RevisPaci != TargetUser.GetClient().GetHabbo().Id)
            {
                Session.SendWhisper("¡Primero debes :revisarpaciente [nombre] para saber qué operación hacer!", 1);
                return;
            }
            if (Session.GetPlay().BotiquinDoc <= 0)
            {
                Session.SendWhisper("¡Primero debes :verbotiquin y :usarbotiquin [ID] para comenzar el tratamiento!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("reviewpatient", true))
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
                if (Session.GetPlay().BotiquinDoc == TargetClient.GetPlay().HeridaPaci)
                {
                    Session.SendWhisper("¡Bien hecho! Ganas $ "+ RoleplayManager.PayRightJob + " por salvar a este paciente", 1);
                    Session.GetHabbo().Credits += RoleplayManager.PayRightJob;//8
                    RoleplayManager.UpdateCreditsBalance(Session);
                    Session.GetPlay().MoneyEarned += RoleplayManager.PayRightJob;
                }
                else
                {                    
                    Session.SendWhisper("Ganas solo $ "+ RoleplayManager.PayWrongJob + " por equivocarte salvando a este paciente", 1);
                    Session.GetHabbo().Credits += RoleplayManager.PayWrongJob;//6
                    RoleplayManager.UpdateCreditsBalance(Session);
                    Session.GetPlay().MoneyEarned += RoleplayManager.PayWrongJob;
                }
                TargetClient.GetPlay().HeridaName = "";
                TargetClient.GetPlay().HeridaPaci = 0;
                TargetClient.GetPlay().Revisado = false;

                Session.GetPlay().BotiquinDoc = 0;
                Session.GetPlay().BotiquinName = "";
                Session.GetPlay().RevisPaci = 0;
                RoleplayManager.Shout(Session, "*Comienza a curar las heridas de " + TargetClient.GetHabbo().Username + "*", 5);
                TargetClient.GetRoomUser().ApplyEffect(0);
                TargetClient.GetPlay().BeingHealed = true;
                TargetClient.GetPlay().TimerManager.CreateTimer("heal", 1000, false);
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a atender.", 1);
                return;
            }
            #endregion
        }
    }
}
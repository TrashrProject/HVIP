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
    class ReviewPatientCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_review"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Revisar a un herido en camilla para saber cómo atenderlo."; }
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
                Session.SendWhisper("((Ha ocurrido un error al buscar a la persona, probablemente esté desconectada))", 1);
                return;
            }
            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("((Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona))", 1);
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
                Session.SendWhisper("¡No puedes revisar a un usuario ausente!", 1);
                return;
            }
            if (TargetClient.GetPlay().Revisado)
            {
                Session.SendWhisper("¡Est@ paciente ya esta siendo atendid@!", 1);
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
                Random Random = new Random();

                int Chance = Random.Next(1, 101);

                if (Chance <= 8 && Session.GetPlay().JobRank <= 1)
                {
                    RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " pero no ha podido decifrar qué problema tiene*", 5);
                    return;
                }
                else
                {
                    bool NeedHelp = TargetClient.GetPlay().DeadTimeLeft > 0;

                    if (!NeedHelp || TargetClient.GetPlay().Revisado == true)
                    {
                        RoleplayManager.Shout(Session, "*Revisa a " + TargetClient.GetHabbo().Username + " pero nota que no necesita más ayuda*", 5);
                        return;
                    }
                    else// HERE
                    {
                        TargetClient.GetPlay().Revisado = true;
                        Session.GetPlay().RevisPaci = TargetClient.GetHabbo().Id;
                        RoleplayManager.Shout(Session, "*Revisa al paciente " + TargetClient.GetHabbo().Username + " para atenderlo*", 5);
                        Session.SendWhisper("Esta persona presenta " + TargetClient.GetPlay().HeridaName + ", ve a buscar al botiquin lo que necesites para el tratamiento.", 1);
                        Session.GetPlay().CooldownManager.CreateCooldown("reviewpatient", 1000, 10);
                        return;
                    }
                }
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a revisar.", 1);
                return;
            }
            #endregion
        }
    }
}
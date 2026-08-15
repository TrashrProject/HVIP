using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class UniformCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_uniform"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Te colocas el uniforme de tu Trabajo (tipo secundario) para comenzar una Jornada Laboral."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Principal Conditions
            if (Session.GetPlay().TryGetCooldown("startwork", true))
            {
                Session.SendWhisper("Por favor espera un poco para trabajar nuevamente.", 1);
                return;
            }
            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                return;
            }
            #endregion

            #region Group Conditions
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);           

            if (Groups.Count <= 0)
            {
                Session.SendWhisper("Necesitas primero tener un trabajo para colcarte el uniforme.", 1);
                return;
            }

            int GroupNumber = -1;
            
            if(Groups[0].GType != 2)
            {
                if(Groups.Count > 1)
                {
                    if (Groups[1].GType != 2)
                    {
                        Session.SendWhisper("((No perteneces a ningún trabajo de tipo Secundario para usar ese comando. Usa ':ayuda trabajos' para más info.))", 1);
                        return;
                    }
                    GroupNumber = 1; // Segundo indicie de variable
                }
                else
                {
                    Session.SendWhisper("((No perteneces a ningún trabajo de tipo Secundario para usar ese comando. Usa ':ayuda trabajos' para más info.))", 1);
                    return;
                }
            }
            else
            {
                GroupNumber = 0; // Primer indice de Variable Group
            }
            
            Session.GetPlay().JobId = Groups[GroupNumber].Id;
            Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
            #endregion

            #region Extra Conditions            
            // Existe el trabajo?
            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Session.GetPlay().JobId, Session.GetPlay().JobRank))
            {
                Session.GetPlay().TimeWorked = 0;
                Session.GetPlay().JobId = 0; // Desempleado
                Session.GetPlay().JobRank = 0;

                //Room.Group.DeleteMember(Session.GetHabbo().Id);// OJO ACÁ

                Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                return;
            }
            // Puede trabajar aquí?
            Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);
            GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Job.Id, Session.GetPlay().JobRank);
            if (Rank.MaleFigure == "" || Rank.MaleFigure == "blank" || Rank.MaleFigure == "*")
            {
                Session.SendWhisper("Al parecer ninguno de tus trabajos actuales requiere Uniforme. Si estás en una Empresa, recuerda que su comando es ':trabajar'.", 1);
                return;
            }

            #region Conditions Status
            if (Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Ya tienes puesto el uniforme.", 1);
                return;
            }
            if (Job.Name.Contains("Policía") && Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes comenzar a trabajar de policía en modo pasivo.", 1);
                return;
            }
            // Agregamos excepción con Médicos, para que puedan ponerse uniforme en dos zonas.
            if ((Room.Group == null || Room.Group.GType != 2) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "botiquin"))
            {
                Session.SendWhisper("No te encuentras en la zona de tu trabajo donde puedas colocarte el uniforme de "+Job.Name, 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                return;
            }
            if (Session.GetPlay().IsWorkingOut)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás ejercitandote!", 1);
                return;
            }

            if (Session.GetPlay().TimerManager.ActiveTimers.ContainsKey("sendhome"))
            {
                Session.SendWhisper("¡Te han mandado a casa! No puedes trabajar hasta que termine tu castigo.", 1);
                return;
            }
            #endregion
            
            #endregion

            #region Special Exceptions
            string MyCity = Room.City;
            PlayRoom Data;
            int Comisaria = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetPolStation(MyCity, out Data);
            int Hospital = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);
            int Basurero = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetBasureros(MyCity, out Data);
            int Mecanico = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetMecanicos(MyCity, out Data);
            int Minero = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetMineros(MyCity, out Data);
            // Police            
            if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "police") && Room.Id != Comisaria)// Comisaría de la cd.
            {
                Session.SendWhisper("Debes ir a la comisaría para colocarte el uniforme.", 1);
                return;
            }
            // Hospital
            else if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "botiquin") && ((Room.Id != Hospital) && (Room.Group == null || Room.Group.GType != 2)))// Hospital de la cd.
            {
                Session.SendWhisper("Debes ir al hospital para colocarte el uniforme.", 1);
                return;
            }
            // Basurero
            else if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "basurero") && Room.Id != Basurero)// Basurero de la cd.
            {
                Session.SendWhisper("Debes ir al Basurero de la Ciudad para colocarte el uniforme.", 1);
                return;
            }
            // Mecánico
            else if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "mecanico") && Room.Id != Mecanico)// Mecanico de la cd.
            {
                Session.SendWhisper("Debes ir al Taller Mecánico de la Ciudad para colocarte el uniforme.", 1);
                return;
            }
            // Mineros
            else if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "minero") && Room.Id != Minero)// Minero de la cd.
            {
                Session.SendWhisper("Debes ir a la mina de la Ciudad para colocarte el uniforme.", 1);
                return;
            }
            #endregion

            #region Execute
            Session.GetPlay().IsWorking = true;
            RoleplayManager.GetLookAndMotto(Session);
            WorkManager.AddWorkerToList(Session);

            // Iniciamos Timer a Policias
            if(Room.Id == Comisaria)
                Session.GetPlay().TimerManager.CreateTimer("work", 1000, true);

            Session.GetPlay().CooldownManager.CreateCooldown("startwork", 1000, 10);
            RoleplayManager.Shout(Session, "Se coloca su uniforme y comienza a trabajar", 5);

            #region On Service messages
            if(PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "heal"))
            {
                Session.SendWhisper("Te has puesto en servicio de Médico. Ahora recibirás alertas de ayuda para atender pacientes.", 1);
            }
            else if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "mecanico"))
            {
                Session.SendWhisper("Te has puesto en servicio de Mecánico. Ahora recibirás alertas de ayuda para reparar vehículos.", 1);
            }
            #endregion
            return;
            #endregion
        }
    }
}
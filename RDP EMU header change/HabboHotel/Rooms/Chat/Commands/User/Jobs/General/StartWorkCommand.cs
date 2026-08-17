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
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class StartWorkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_work_start"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Comienzas a trabajar."; }
        }

        private static string GetFigurePart(string look, string type)
        {
            if (String.IsNullOrWhiteSpace(look))
                return "";

            foreach (string part in look.Split('.'))
            {
                if (part.StartsWith(type + "-", StringComparison.OrdinalIgnoreCase))
                    return part;
            }

            return "";
        }

        private static string ReplaceFigurePart(string look, string type, string replacement)
        {
            if (String.IsNullOrWhiteSpace(replacement))
                return look ?? "";

            List<string> parts = String.IsNullOrWhiteSpace(look)
                ? new List<string>()
                : look.Split('.').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

            bool replaced = false;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].StartsWith(type + "-", StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = replacement;
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
                parts.Add(replacement);

            return String.Join(".", parts);
        }

        private static void RestoreCurrentHair(GameClients.GameClient Session, Rooms.Room Room, string hair)
        {
            if (Session == null || Session.GetHabbo() == null || String.IsNullOrWhiteSpace(hair))
                return;

            Session.GetHabbo().Look = ReplaceFigurePart(Session.GetHabbo().Look, "hr", hair);

            RoomUser user = Room != null ? Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id) : null;
            if (user != null)
            {
                Session.SendMessage(new UserChangeComposer(user, true));
                Room.SendMessage(new UserChangeComposer(user, false));
            }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Principal Conditions
            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                return;
            }
            if (Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Ya te encuentras trabajando.", 1);
                return;
            }
            if (Room.Group == null || Room.Group.GType != 1)
            {
                Session.SendWhisper("No te encuentras en ninguna zona de trabajo de empresa.", 1);
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

            if (Session.GetPlay().IsRobATM)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás Robando un Cajero!", 1);
                return;
            }

            if (Session.GetPlay().TimerManager.ActiveTimers.ContainsKey("sendhome"))
            {
                Session.SendWhisper("¡Te han mandado a casa! No puedes trabajar hasta que termine tu castigo.", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("startwork", true))
            {
                Session.SendWhisper("Por favor espera un poco para trabajar nuevamente.", 1);
                return;
            }
            #endregion

            #region Group Conditions
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

            if (Groups.Count <= 0)
            {
                Session.SendWhisper("No tienes ningún trabajo.", 1);
                return;
            }

            int GroupNumber = -1;

            if(Groups[0].GType != 1)
            {
                if(Groups.Count > 1)
                {
                    if (Groups[1].GType != 1)
                    {
                        Session.SendWhisper("((No perteneces a ninguna empresa para usar ese comando))", 1);
                        return;
                    }
                    GroupNumber = 1;
                }
                else
                {
                    Session.SendWhisper("((No perteneces a ninguna empresa para usar ese comando))", 1);
                    return;
                }
            }
            else
            {
                GroupNumber = 0;
            }

            Session.GetPlay().JobId = Groups[GroupNumber].Id;
            Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
            #endregion

            #region Extra Conditions
            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Session.GetPlay().JobId, Session.GetPlay().JobRank))
            {
                Session.GetPlay().TimeWorked = 0;
                Session.GetPlay().JobId = 0;
                Session.GetPlay().JobRank = 0;

                Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                return;
            }

            Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);
            GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Job.Id, Session.GetPlay().JobRank);
            if (!Rank.CanWorkHere(Room.Id))
            {
                Session.SendWhisper("Debes ir a la Zona del Trabajo "+Job.Name+" para comenzar a trabajar.", 1);
                return;
            }
            if(Rank.MaleFigure.Length <= 0 || Rank.FemaleFigure.Length <= 0)
            {
                Session.SendWhisper("Al parecer el uniforme para tu puesto no ha sido asignado. Contacta con el Gerente o Fundador.", 1);
                return;
            }
            if (Job.Name.Contains("Policía") && Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes comenzar a trabajar de policía en modo pasivo.", 1);
                return;
            }
            #endregion

            #region Execute
            string currentHair = GetFigurePart(Session.GetHabbo().Look, "hr");
            Session.GetPlay().IsWorking = true;
            RoleplayManager.GetLookAndMotto(Session);
            RestoreCurrentHair(Session, Room, currentHair);
            WorkManager.AddWorkerToList(Session);
            Session.GetPlay().TimerManager.CreateTimer("work", 1000, true);
            Session.GetPlay().CooldownManager.CreateCooldown("startwork", 1000, 10);
            RoleplayManager.Shout(Session, "*Ha comenzado a trabajar*", 5);
            return;
            #endregion
        }
    }
}
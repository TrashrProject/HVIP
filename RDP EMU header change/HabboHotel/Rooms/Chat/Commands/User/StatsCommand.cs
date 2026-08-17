using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class StatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_stats"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ver tus estadísticas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder JailMessage = new StringBuilder();
            if (Session.GetPlay().IsJailed) JailMessage.Append("Estás encarcelad@ durante " + Session.GetPlay().JailedTimeLeft + " minuto(s)");
            else JailMessage.Append("No estás encarcelad@");

            StringBuilder DeadMessage = new StringBuilder();
            if (Session.GetPlay().IsDead) DeadMessage.Append("Estás murt@ durante " + Session.GetPlay().DeadTimeLeft + " minuto(s)");
            else DeadMessage.Append("No estás muert@");

            StringBuilder WantedMessage = new StringBuilder();
            if (Session.GetPlay().IsWanted) WantedMessage.Append("Estás siendo buscad@ durante " + Session.GetPlay().WantedTimeLeft + " minuto(s)");
            else WantedMessage.Append("No estás siendo buscad@");

            StringBuilder SendhomeMessage = new StringBuilder();
            if (Session.GetPlay().SendHomeTimeLeft > 0) SendhomeMessage.Append("Has sido enviad@ a casa durante " + Session.GetPlay().SendHomeTimeLeft + " minuto(s)");
            else SendhomeMessage.Append("No has sido enviad@ a casa");

            StringBuilder PhoneType = new StringBuilder();
            if (Session.GetPlay().Phone == 0)
                PhoneType.Append("No tienes teléfono");
            else
                PhoneType.Append("Tienes Teléfono con número: " + Session.GetPlay().PhoneNumber);

            #region Jobs
            StringBuilder JobMessages = new StringBuilder();
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

            if (Groups.Count <= 0)
                JobMessages.Append("No tienes ningún trabajo.");
            else
            {
                GroupRank Rank = null;

                Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Groups[0].Id, Groups[0].Members[Session.GetHabbo().Id].UserRank);
                JobMessages.Append("\n - " + Groups[0].Name + " (ID " + Groups[0].Id + ") con puesto como " + Rank.Name);
                
                if (Groups.Count > 1)
                {
                    Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Groups[1].Id, Groups[1].Members[Session.GetHabbo().Id].UserRank);
                    JobMessages.Append("\n - " + Groups[1].Name + " (ID "+Groups[1].Id+") con puesto como " + Rank.Name + "\n");
                }
            }
            #endregion

            StringBuilder MessageToSend = new StringBuilder().Append(
            "-------- Tus estadísticas --------\n\n" +

            "--- Información básica ---\n" +
            "Nivel: " + Session.GetPlay().Level +"\n" +
            "Reputación: " + String.Format("{0:N0}", Session.GetPlay().CurXP) + "/" + String.Format("{0:N0}", RoleplayManager.GetInfoPD(Session.GetPlay().Level, "NeedXP")) + "\n\n" +
                                   
            "--- Trabajo(s) ---\n" +
            "Trabajo(s): " + JobMessages + "\n" +
            "Trabajo Primario:\n" +
            " - Enviad@ a casa: " + SendhomeMessage + "\n" +
            " - Minutos trabajados: " + String.Format("{0:N0}", Session.GetPlay().TimeWorked) + "\n\n" +

            "--- Necesidades Fisiológicas ---\n" +
            "Vida: " + String.Format("{0:N0}", Session.GetPlay().CurHealth) + "/" + Session.GetPlay().MaxHealth + "\n" +
            "Hambre: " + Session.GetPlay().Hunger + "/100\n" +
            "Higiene: " + Session.GetPlay().Hygiene + "/100\n\n" +

            "--- Estadísticas nivelables ---\n" +
            "Fuerza: " + Session.GetPlay().Strength + "/" + RoleplayManager.StrengthCap + " => Rutina: " + String.Format("{0:N0}", Session.GetPlay().StrengthEXP) + " / 50" + "\n\n" +
            
            "--- Estados ---\n" +
            "Encarcelad@: " + JailMessage + "\n" +
            "Muert@: " + DeadMessage + "\n" +
            "Buscad@: " + WantedMessage + "\n\n" +
            
            "--- Otras estadísticas ---\n" +
            //"Golpes: " + String.Format("{0:N0}", Session.GetPlay().Punches) + "\n" +
            //"Asesinatos: " + String.Format("{0:N0}", Session.GetPlay().Kills) + "\n" +
            //"Asesinatos a puño: " + String.Format("{0:N0}", Session.GetPlay().HitKills) + "\n" +
            //"Asesinatos con arma: " + String.Format("{0:N0}", Session.GetPlay().GunKills) + "\n" +
            //"Muertes: " + String.Format("{0:N0}", Session.GetPlay().Deaths) + "\n" +
            //"Muertes de policías: " + String.Format("{0:N0}", Session.GetPlay().CopDeaths) + "\n" +
            "Arrestos: " + String.Format("{0:N0}", Session.GetPlay().Arrests) + "\n" +
            "Arrestad@: " + String.Format("{0:N0}", Session.GetPlay().Arrested) + "\n\n" +
            //"Evaciones: " + String.Format("{0:N0}", Session.GetPlay().Evasions) + "\n\n" +

            "--- Economía ---\n" +
            "Dinero: $" + String.Format("{0:N0}", Session.GetHabbo().Credits) + "\n" +
            "Banco: $" + String.Format("{0:N0}", Session.GetPlay().Bank) + "\n" +
            "Platinos (PL): " + String.Format("{0:N0}", Session.GetHabbo().Diamonds) + " PL\n\n" +
                       
            "--- Armas ---\n" +
            "Usa el comando :inventario.\n\n");

            Session.SendMessage(new MOTDNotificationComposer(MessageToSend.ToString()));
        }
    }
}

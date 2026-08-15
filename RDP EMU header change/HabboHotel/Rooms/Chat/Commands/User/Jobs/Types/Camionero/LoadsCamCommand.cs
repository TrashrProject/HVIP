using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class LoadsCamCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_loads_camion"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo Camionero, permite ver la Lista de Cargas para transportar."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (Session.GetPlay().TryGetCooldown("cargcam"))
                return;

            #endregion

            #region Group Conditions
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

            if (Groups.Count <= 0)
            {
                Session.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                return;
            }

            int GroupNumber = -1;

            if (Groups[0].GType != 2)
            {
                if (Groups.Count > 1)
                {
                    if (Groups[1].GType != 2)
                    {
                        Session.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                        return;
                    }
                    GroupNumber = 1; // Segundo indicie de variable
                }
                else
                {
                    Session.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
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

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "camionero"))
            {
                Session.SendWhisper("Debes tener el trabajo de Camionero para usar ese comando.", 1);
                return;
            }

            #endregion

            #region Execute

            #region Pagas
            int Amn = 0, Med = 0, Crack = 0, Piezas = 0;

            if (Session.GetPlay().CamLvl == 1)
            {
                Amn = 13;
                Med = 2;
                Crack = 1;
                Piezas = 2;
            }
            else if (Session.GetPlay().CamLvl == 2)
            {
                Amn = 16;
                Med = 4;
                Crack = 2;
                Piezas = 5;
            }
            else if (Session.GetPlay().CamLvl == 3)
            {
                Amn = 20;
                Med = 6;
                Crack = 3;
                Piezas = 7;
            }
            else if (Session.GetPlay().CamLvl == 4)
            {
                Amn = 22;
                Med = 8;
                Crack = 4;
                Piezas = 7;
            }
            else if (Session.GetPlay().CamLvl == 5)
            {
                Amn = 25;
                Med = 10;
                Crack = 5;
                Piezas = 7;
            }
            else if (Session.GetPlay().CamLvl >= 6) // Max Lvl
            {
                Amn = 30;
                Med = 12;
                Crack = 6;
                Piezas = 7;
            }
            #endregion

            string Cargas = "";
            Cargas += "==========================\n Cargas de Camionero Nivel " + Session.GetPlay().CamLvl + "\n==========================\n";//3 Tabs
            Cargas += "[1] [L] Productos de 24/7 (Ganancias $" + Amn + ")\n";
            Cargas += "[2] [L] Ropa (Ganancias $" + Amn + ")\n";
            Cargas += "[3] [I] Drogas (Ganancias $" + Amn + " + " + Med + " Medicamentos + " + Crack + " g. de Crack)\n";
            Cargas += "[4] [I] Armas (Ganancias $" + Amn + " + " + Piezas + " piezas de armas)\n\n\n";
            Cargas += "[I] = Carga Ilegal\n";
            Cargas += "[L] = Carga Legal\n";

            Session.SendMessage(new MOTDNotificationComposer(Cargas));

            Session.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 3);
            #endregion
        }
    }
}

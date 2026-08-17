using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Data;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class LeaveRockCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_miner_leave"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo Minero, deja la roca piacada para recibir tu pago."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            
            #region Basic Conditions
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk)
            {
                Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }
            #endregion

            if (Session.GetPlay().TryGetCooldown("pick"))
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

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "minero"))
            {
                Session.SendWhisper("Debes tener el trabajo de Minero para usar ese comando.", 1);
                return;
            }
            string MyCity = Room.City;
            int MinerID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetMineros(MyCity, out PlayRoom Data);//mineros de la cd.
            if (Session.GetHabbo().CurrentRoomId != MinerID)
            {
                Session.SendWhisper("Debes ir al exterior de la mina para hacer eso.", 1);
                return;
            }
            if (!Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Debes llevar puesto el uniforme de Minero para hacer eso.", 1);
                return;
            }

            #endregion

            #region Check Rock InFront (OFF)
            /*
            bool found = false;
            Item Item = null;
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;
            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetX == User.SquareInFront.X && item.GetY == User.SquareInFront.Y)
                {
                    if (item.Data.ItemName == "comodin_carro")// Name de la procesadora
                    {
                        Item = item;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                Session.SendWhisper("¡Debes estar frente a la procesadora para hacer eso!", 1);
                return;
            }
            */
            #endregion

            #region Comodin Conditions
            if (!Session.GetPlay().MinerRock)
            {
                Session.SendWhisper("No llevas ninguna roca para tirar en la procesadora.", 1);
                return;
            }
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendWhisper("Debes acercarte a la procesadora para hacer eso.", 1);
                return;
            }
            #endregion

            #region Execute
            #region Pagas
            int Pay = RoleplayManager.MinerPay * Session.GetPlay().MinerRockLvl + (Session.GetPlay().MinerLvl * Session.GetPlay().MinerRockLvl);
            #endregion

            RoleplayManager.Shout(Session, "*Tira una roca en la procesadora*", 5);
            Session.SendWhisper("¡Buen Trabajo! Tus ganancias son: $" + Pay, 1);            
            Session.GetPlay().MinerRock = false;
            Session.GetPlay().MinerRockLvl = 0;
            Session.GetHabbo().Credits += Pay;
            Session.GetPlay().MoneyEarned += Pay;
            Session.GetHabbo().UpdateCreditsBalance();

            RoleplayManager.JobSkills(Session, Session.GetPlay().JobId, Session.GetPlay().MinerLvl, Session.GetPlay().MinerXP);
            Session.GetPlay().CooldownManager.CreateCooldown("pick", 1000, 3);
            #endregion
        }
    }
}

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
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class PickCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_miner_pick"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo Minero, comienza a picar la roca que tienes enfrente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                return;
            }

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
            // Puede trabajar aquí?
            Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);
            GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Job.Id, Session.GetPlay().JobRank);
            if (!Rank.CanWorkHere(Room.Id))
            {
                //String.Join(",", Rank.WorkRooms)
                Session.SendWhisper("Debes ir al interior de la mina.", 1);
                return;
            }
            if (Session.GetHabbo().CurrentRoomId != 59)
            {
                Session.SendWhisper("Debes ir al interior de la mina para hacer eso.", 1);
                return;
            }
            if (!Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Debes llevar puesto el uniforme de Minero para hacer eso.", 1);
                return;
            }
            
            #endregion

            #region Check Rock InFront
            bool found = false;
            int RockLvl = 0;
            Item Item = null;
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;
            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetX == User.SquareInFront.X && item.GetY == User.SquareInFront.Y && item.GetZ == User.Z)
                {
                    if (item.Data.ItemName == "comodin_carro")
                    {
                        Item = item;
                        RockLvl = 1;
                        found = true;
                    }
                    else if(item.Data.ItemName == "comodin_carr2")
                    {
                        Item = item;
                        RockLvl = 3;
                        found = true;
                    }
                    else if (item.Data.ItemName == "prison_stone")
                    {
                        Item = item;
                        RockLvl = 2;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                Session.SendWhisper("¡Debes estar frente de una roca para hacer eso!", 1);
                return;
            }

            #endregion
            
            #region Execute
            if(Session.GetPlay().MinerRock)
            {
                Session.SendWhisper("Ya has picado una roca. Ve a dejarla a la procesadora.", 1);
                return;
            }
            if (Session.GetPlay().MinerLvl < RockLvl)
            {
                Session.SendWhisper("Necesitas ser Minero de Nivel "+RockLvl+" para picar esa Roca.", 1);
                return;
            }

            int Time = RoleplayManager.GetTimerByMyJob(Session, "minero");

            RoleplayManager.Shout(Session, "*Saca un pico y comienza a picar la Roca*", 5);
            Session.GetPlay().LoadingTimeLeft = Time;//Depende del nivel del minero
            Session.GetPlay().IsMinerLoading = true;
            Session.GetPlay().MinerRockLvl = RockLvl;
            Session.SendWhisper("Debes esperar "+ Session.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
            Session.GetPlay().CooldownManager.CreateCooldown("pick", 1000, 3);
            Session.GetPlay().TimerManager.CreateTimer("general", 1000, true);
            Session.GetRoomUser().ApplyEffect(EffectsList.Pickaxe);
            #endregion
        }
    }
}

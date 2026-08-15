using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class StopWorkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_work_stop"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dejas de trabajar si te encuentras trabajando."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (!Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("¡No te encuentras trabajando!", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }
            if (Session.GetPlay().DrivingInCar)
            {
                Session.SendWhisper("¡Primero detén el auto que tienes afuera!", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas de pasajero!", 1);
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

            if (Session.GetPlay().TryGetCooldown("stopwork", true))
                return;

            #endregion
            
            WorkManager.RemoveWorkerFromList(Session);
            Session.GetPlay().IsWorking = false;
            Session.GetPlay().TimeWorked = 0;
            Session.GetPlay().JobId = 0; // Desempleado
            Session.GetPlay().JobRank = 0;
            Session.GetHabbo().Poof();
            RoleplayManager.CheckCorpCarp(Session);
            RoleplayManager.Shout(Session, "*Ha dejado de trabajar*", 5);
            Session.GetPlay().CooldownManager.CreateCooldown("stopwork", 1000, 10);
        }
    }
}
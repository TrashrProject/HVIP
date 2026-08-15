using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.VehiclesJobs;
using Plus.HabboRoleplay.VehicleOwned;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class SembrarCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_sembrar"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siembra semillas para cultivar."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetPlay().TryGetCooldown("sembrar", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
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
            if (Session.GetPlay().EquippedWeapon != null)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras equipas un arma!", 1);
                return;
            }
            #endregion

            #region Conditons
            if (!Room.GrangeEnabled)
            {
                Session.SendWhisper("Debes estar en la granja para cultivar zanahorias.", 1);
                return;
            }
            if (!Session.GetPlay().FarmSeeds)
            {
                Session.SendWhisper("¡No tienes semillas de Zanahorias para sembrar!", 1);
                return;
            }
            if(Session.GetPlay().IsFarming)
            {
                Session.SendWhisper("¡Ya te encuentras cultivando! Por favor, espera.", 1);
                return;
            }
            #endregion

            #region Comodin Conditions
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "nest_dirt" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendWhisper("Debes estar sobre un surco para sembrar.", 1);
                return;
            }
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "easter_c17_carrot" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile != null)
            {
                Session.SendWhisper("Ya hay unas semillas sembradas en este surco.", 1);
                return;
            }
            if (Session.GetPlay().WateringCan)
            {
                Session.SendWhisper("No puedes sembrar con la regadera en la mano. ((Usa :tirar regadera))", 1);
                return;
            }
            #endregion

            #region Execute
            Session.GetPlay().IsFarming = true;
            Session.GetPlay().LoadingTimeLeft = RoleplayManager.SembrarTime;
            Session.GetPlay().TimerManager.CreateTimer("general", 1000, true);

            RoleplayManager.Shout(Session, "*Comienza a arrojar unas semillas a un surco*", 5);
            Session.GetPlay().CooldownManager.CreateCooldown("sembrar", 1000, 3);
            return;
            #endregion
        }
    }
}
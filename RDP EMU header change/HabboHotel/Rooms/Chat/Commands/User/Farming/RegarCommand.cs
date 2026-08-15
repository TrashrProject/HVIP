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
    class RegarCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_regar"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Riega unas semillas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetPlay().TryGetCooldown("regar", true))
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

            bool IsWeed = false;
            #region Comodin Conditions
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "easter_c17_carrot" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "marihuana" && x.Coordinate == Session.GetRoomUser().Coordinate);
                if (BTile != null)
                    IsWeed = true;
            }
            if (BTile == null)
            {
                Session.SendWhisper("Debes estar sobre unas semillas o planta para regar.", 1);
                return;
            }
            #endregion

            #region Execute
            #region Cosechador
            if (!IsWeed)
            {
                #region Conditons
                if(BTile.ExtraData == "1")
                {
                    Session.SendWhisper("¡Estas semillas ya fueron regadas! ((Usa ahora :cosechar))", 1);
                    return;
                }
                if (Session.GetPlay().IsFarming)
                {
                    Session.SendWhisper("¡Ya te encuentras cultivando! Por favor, espera.", 1);
                    return;
                }
                if (!Session.GetPlay().WateringCan)
                {
                    Session.SendWhisper("¡Necesitas una regadera para regar tu cultivo! Ve por ella a la bomba cerca del Establo.", 1);
                    return;
                }
                #endregion

                Session.GetPlay().IsFarming = true;
                Session.GetPlay().LoadingTimeLeft = RoleplayManager.RegarTime;
                Session.GetPlay().TimerManager.CreateTimer("general", 1000, true);

                RoleplayManager.Shout(Session, "*Comienza a regar unas semillas*", 5);
                Session.GetPlay().CooldownManager.CreateCooldown("regar", 1000, 3);
                return;
            }
            #endregion

            #region Weed
            else
            {

            }
            #endregion
            #endregion
        }
    }
}
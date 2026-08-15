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
using Plus.Utilities;
using Plus.HabboRoleplay.ProductOwned;
using Plus.HabboRoleplay.Products;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class CosecharCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_cosechar"; }
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
            if (Session.GetPlay().TryGetCooldown("cosechar", true))
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
                Session.SendWhisper("Debes estar sobre unas semillas o planta para cosechar.", 1);
                return;
            }
            #endregion

            #region Execute
            #region Cosechador
            if (!IsWeed)
            {
                #region Conditons
                if(BTile.ExtraData != "1")
                {
                    Session.SendWhisper("¡Estas semillas no han sido regadas! ((Usa ahora :regar))", 1);
                    return;
                }
                if (Session.GetPlay().IsFarming)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás sembrando o regando!", 1);
                    return;
                }
                #endregion

                int Win = RoleplayManager.GrangePay;
                Session.GetHabbo().Credits += Win;
                Session.GetPlay().MoneyEarned += Win;
                Session.GetHabbo().UpdateCreditsBalance();

                RoleplayManager.Shout(Session, "*Toma una guadaña y cosecha su cultivo*", 5);
                Session.SendWhisper("¡Buen trabajo! Has ganado $" + Win, 1);
                BTile.ExtraData = "0";
                Room.GetRoomItemHandler().RemoveFurniture(Session, BTile.Id);
                RoleplayManager.GuessCosechador(Session);
                Session.GetPlay().CooldownManager.CreateCooldown("cosechar", 1000, 3);
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
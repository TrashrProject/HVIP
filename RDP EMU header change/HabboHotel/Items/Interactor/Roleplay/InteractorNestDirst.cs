using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorNestDirst : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            Room Room = User.GetRoom();

            if (Room == null)
                return;

            if (!Room.GrangeEnabled)
                return;
            
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "" || Item.ExtraData == "0")
                    if (User.CanWalk)
                        User.MoveTo(Item.Coordinate);
            }
            else
            {
                if (Session.GetPlay().WateringCan)
                {
                    #region Regar
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
                        if (BTile.ExtraData == "1")
                        {
                            Session.SendWhisper("¡Estas semillas ya fueron regadas! ((Si quieres cosechar primero usa :tirar regadera))", 1);
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
                    #endregion
                }
                else
                {
                    if (Session.GetPlay().FarmSeeds)
                    {
                        #region Sembrar
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
                        if (Session.GetPlay().IsFarming)
                        {
                            Session.SendWhisper("¡Ya te encuentras cultivando! Por favor, espera.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        /* Unnecesary
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "nest_dirt" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes estar sobre un surco para sembrar.", 1);
                            return;
                        }
                        */
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
                        #endregion
                    }
                    else
                    {
                        #region Cosechar
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
                            if (BTile.ExtraData != "1")
                            {
                                Session.SendWhisper("¡Estas semillas no han sido regadas! ((Ve por una regadera y dale doble clic al surco para regar))", 1);
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
                        #endregion
                    }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}
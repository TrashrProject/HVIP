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
    public class InteractorWaterPump : IFurniInteractor
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

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "" || Item.ExtraData == "0")
                    if (User.CanWalk)
                        User.MoveTo(Item.SquareInFront);
            }
            else
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
                if (Session.GetPlay().EquippedWeapon != null)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras equipas un arma!", 1);
                    return;
                }
                #endregion

                #endregion

                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    User.ClearMovement(true);
                    User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    RoleplayManager.Shout(Session, "*Comienza a llenar una regadera con agua*", 5);

                    new Thread(() =>
                    {
                        User.CanWalk = false;

                        if (User.CurrentEffect != 4 && Session.GetPlay().EquippedWeapon == null)
                            User.ApplyEffect(EffectsList.Twinkle);

                        Thread.Sleep(5000);

                        if (User.CurrentEffect != 0 && Session.GetPlay().EquippedWeapon == null)
                            User.ApplyEffect(0);

                        if (User != null)
                            User.CanWalk = true;

                        Session.GetPlay().WateringCan = true;// Se coloca regadera gracias a ConditionCheckTimer.cs
                        Item.ExtraData = "0";
                        Item.UpdateState(false, true);
                    }).Start();                   
                }
                else
                    Session.SendWhisper("¡Al parecer esta bomba de agua está siendo usada!", 1);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}
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
    public class InteractorGroupFlag : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (!Item.GetBaseItem().ItemName.Contains("army_c15_groupflag"))
                return;

            if (!Item.GetRoom().Type.Contains("public"))
                return;

            if (!Item.GetRoom().TurfEnabled)
                return;

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
                if (Session.GetPlay().TryGetCooldown("capturing", true))
                    return;

                #region Conditions
                #region Group Conditions
                List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);

                if (Groups.Count <= 0)
                {
                    Session.SendWhisper("No tienes perteneces a ninguna banda para capturar territorios.", 1);
                    return;
                }
                if(Groups[0].BankRuptcy)
                {
                    Session.SendWhisper("¡Tu banda está en banca rota! No pueden seguir operando en ella.", 1);
                    return;
                }
                #endregion

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
                if (Session.GetPlay().IsWorking)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras tabajas!", 1);
                    return;
                }
                #endregion

                if (Session.GetPlay().TurfCapturing)
                {
                    Session.SendWhisper("Ya te encuentras capturando el territorio. ¡Procura no moverte!", 1);
                    return;
                }
                if(User.GetRoom().TurfCapturing)
                {
                    Session.SendWhisper("¡El territorio ya está siendo capturado!", 1);
                    return;
                }
                #endregion

                if (User.GetRoom().Group != null)
                {
                    if(User.GetRoom().Group == Groups[0])
                    {
                        Session.SendWhisper("¡Este barrio ya pertence a tu banda!", 1);
                        return;
                    }
                    else
                    {
                        // Alertamos a los integrantes de la banda atacada
                        foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (client == null || client.GetHabbo() == null)
                                continue;

                            List<Groups.Group> thegroup = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(client.GetHabbo().Id);

                            if (thegroup == null || thegroup.Count <= 0)
                                continue;

                            if (thegroup[0] != User.GetRoom().Group)
                                continue;

                            if (client.GetPlay().DisableRadio == true)
                                continue;

                            client.SendWhisper("[RADIO] ¡Están atacando nuestro "+User.GetRoom().Name+"! ¡Vamos a defenderlo!", 30);
                        }
                    }
                }
                
                // Esta variable se valida en RoomUserManager para enviar a todos el WS del timer.
                User.GetRoom().TurfCapturing = true;
                User.GetRoom().TurfUserAtackerId = Session.GetHabbo().Id;

                // Timer
                Session.GetPlay().TurfCapturing = true;
                Session.GetPlay().LoadingTimeLeft = RoleplayManager.TurfCapTime;
                Session.GetPlay().TurfFlagId = Item.Id;

                RoleplayManager.Shout(Session, "*Comienza a capturar el " + User.GetRoom().Name + "*", 5);
                Session.SendWhisper("Debes esperar " + (RoleplayManager.TurfCapTime/60) + " minuto(s) sin moverte para capturar el territorio.", 1);
                Session.GetPlay().TimerManager.CreateTimer("general", 1000, true);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }

    }
}
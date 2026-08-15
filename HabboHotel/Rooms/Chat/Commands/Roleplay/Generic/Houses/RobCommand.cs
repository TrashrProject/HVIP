using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using System.Data;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class RobCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_rob"; }
        }
        // :robar [cajero]
        // :robar [objeto]
        
        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Intenta robar un Cajero automático o un Objeto dentro de una Casa de Robo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            string toDo = "";

            #region Conditions
            if (Params.Length == 2)
            {
                toDo = Params[1].ToLower();
            }
            else
            {
                Session.SendWhisper("Comando inválido, usa ':robar [cajero/objeto]'.", 1);
                return;
            }

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes realizar acciones ilegales en modo pasivo.", 1);
                return;
            }

            #region Basic Conditions
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                return;
            }
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
            #endregion
            
            if (Session.GetPlay().TryGetCooldown("rob", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            switch (toDo)
            {
                #region Cajero
                case "cajero":
                    {
                        #region Conditions
                        if (Session.GetPlay().IsNoob)
                        {
                            Session.SendWhisper("No puedes robar Cajeros siendo usuario Nuevo.", 1);
                            return;
                        }
                        if (Session.GetPlay().GodMode)
                        {
                            Session.SendWhisper("No puedes robar Cajeros siendo usuario con inmunidad.", 1);
                            return;
                        }
                        if(Session.GetPlay().IsWorking)
                        {
                            Session.SendWhisper("No puedes robar Cajeros mientras trabajas.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin(Cajero) Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "atm_moneymachine" && x.Coordinate == Session.GetRoomUser().SquareInFront);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes estar frente a un Cajero Automático.", 1);
                            return;
                        }
                        if (BTile.ExtraData == "1")
                        {
                            Session.SendWhisper("Este Cajero ya ha sido robado. Regresa más tarde.", 1);
                            return;
                        }
                        #endregion

                        if (!RoleplayManager.CheckHaveProduct(Session, "palanca"))
                        {
                            Session.SendWhisper("Necesitas una Palanca para robar Cajeros.", 1);
                            return;
                        }

                        Session.GetPlay().IsRobATM = true;
                        Session.GetPlay().ATMRobTimeLeft = RoleplayManager.ATMRobTime - (Session.GetPlay().Strength * 10);
                        Session.GetPlay().ATMRobbinItem = BTile;
                        RoleplayManager.Shout(Session, "*Toma una Palanca y comienza a Robar el Cajero Automático*", 5);
                        Session.SendWhisper("Debes esperar "+ Session.GetPlay().ATMRobTimeLeft + " segundo(s)...", 1);
                        Session.GetPlay().TimerManager.CreateTimer("atmrob", 1000, true);
                        Session.GetRoomUser().ApplyEffect(EffectsList.Twinkle);
                        PlusEnvironment.GetGame().GetClientManager().RadioAlert("Atención... Buah... lo de siempre, una rata más robando un cajero automático en " + Room.Name, null, true);
                        Session.GetPlay().CooldownManager.CreateCooldown("rob", 1000, 5);
                        break;
                    }
                #endregion

                #region Objeto
                case "objeto":
                    {
                        if (Session.GetPlay().Level < 3 && Session.GetHabbo().VIPRank != 2)
                        {
                            Session.SendWhisper("Necesitas ser Nivel 3 o VIP2 para robar objetos.", 1);
                            return;
                        }

                        var HouseInside = PlusEnvironment.GetGame().GetHouseManager().GetHouseByInsideRoom(Room.Id);

                        if (HouseInside == null)//No Estamos dentro de una casa
                        {
                            Session.SendWhisper("No te encuentras dentro de niguna Casa.", 1);
                            return;
                        }

                        if(HouseInside.Type != 2)
                        {
                            Session.SendWhisper("Esta no es una Casa de Robo para robar objetos.", 1);
                            return;
                        }
                        else // Si es casa de Robo
                        {
                            long Seconds = DateTimeOffset.Now.ToUnixTimeSeconds();
                            long Total = Seconds - HouseInside.Last_Forcing;
                            if (Total > RoleplayManager.TimeForRobHouses)// X segundos para cerrarse
                            {
                                Session.SendWhisper("La casa ha sido Alarmada por los Vecinos. ¡Corre antes de que lleguen las Autoridades!", 1);
                                return;
                            }
                        }

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Session.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Session.SendWhisper("Debes estar cerca de la estantería de la Casa para robar un Objeto.", 1);
                            return;
                        }
                        #endregion


                        if(Session.GetPlay().Object.Length > 0)
                        {
                            Session.SendWhisper("Ya tienes un Objeto en tu Inventario. ¡Ve a guardarlo o Venderlo!", 1);
                            return;
                        }

                        RoleplayManager.GetRandomObject(Session);

                        RoleplayManager.Shout(Session, "*Roba un/a " + Session.GetPlay().Object + " de la Estantería*", 5);
                        Session.SendWhisper("Ahora puedes dirigirte a la Tienda de Venta de Objetos robados y usa ':vender objeto'", 1);
                        Session.GetPlay().CooldownManager.CreateCooldown("rob", 1000, 2);
                        break;
                    }
                #endregion

                default:
                    Session.SendWhisper("Comando inválido. Usa ':robar [cajero/objeto]'.", 1);
                    break;
            }
            #endregion
        }
    }
}
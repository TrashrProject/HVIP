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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class ForceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_force_house"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Forza la Cerradura de una casa de Robo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
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

            if(Session.GetPlay().EquippedWeapon != null)
            {
                Session.SendWhisper("No puedes hacer eso mientras portas armas.", 1);
                return;
            }
            if (Session.GetPlay().Level < 5 && Session.GetHabbo().VIPRank != 2)
            {
                Session.SendWhisper("Necesitas ser Nivel 5 o ser VIP2 para forzar cerraduras.", 1);
                return;
            }
            if (!RoleplayManager.CheckHaveProduct(Session, "destornillador"))
            {
                Session.SendWhisper("Necesitas un Destornillador para poder Forzar Cerraduras.", 1);
                return;
            }
            
            if (Session.GetPlay().TryGetCooldown("force", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByPosition(Room.Id, Session.GetRoomUser().X, Session.GetRoomUser().Y, Session.GetRoomUser().Z);
            if (House == null)
            {
                Session.SendWhisper("Debes estar frente a la puerta de una casa para hacer eso.", 1);
                return;
            }
            if(House.Type != 2)
            {
                Session.SendWhisper("Esta no es una Casa de Robo.", 1);
                return;
            }

            long Seconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            long Total = Seconds - House.Last_Forcing;
            if (Total <= RoleplayManager.TimeForRobHouses)// X segundos para cerrarse
            {
                Session.SendWhisper("La casa se encuentra abierta. ¡Entra antes de que se active la alarma!", 1);
                return;
            }
            else if(Total <= 10800)// 10800 segundos = 3 HRS.
            {
                Session.SendWhisper("La Casa está Alertada por los Vecinos. Intentalo más Tarde.", 1);
                return;
            }

            Session.GetPlay().HouseToForce = House;
            Session.GetPlay().IsForcingHouse = true;
            Session.GetPlay().LoadingTimeLeft = RoleplayManager.MaxForceHouseTime; // Depende nivel de Ladrón
            RoleplayManager.Shout(Session, "*Comienza a Forzar la Cerradura de la Casa*", 5);

            Session.SendWhisper("Debes esperar " + Session.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
            Session.GetPlay().TimerManager.CreateTimer("general", 1000, true);
            Session.GetPlay().CooldownManager.CreateCooldown("force", 1000, 5);
            
            #endregion
        }
    }
}
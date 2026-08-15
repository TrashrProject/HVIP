using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.Vehicles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class SavePatientCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_save"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Salva a la persona que llevas en tu ambulancia."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "reviewhosp"))
            {
                Session.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                return;
            }
            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                return;
            }
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                return;
            }
            if (!Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (!Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("Debes estar conduciendo tu ambulancia.", 1);
                return;
            }
            #region Get Information form VehiclesManager
            Vehicle vehicle = null;
            int corp = 0;
            foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
            {
                if (Session.GetPlay().CarEffectId == Vehicle.EffectID)
                {
                    vehicle = Vehicle;
                    corp = Convert.ToInt32(Vehicle.CarCorp);
                }
            }
            if (vehicle == null)
            {
                Session.SendWhisper("¡Ha ocurrido un error al buscar los datos del vehículo que conduces!", 1);
                return;
            }
            #endregion

            if (corp <= 0 || !PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Hospital"))
            {
                Session.SendWhisper("Debes estar conduciendo una ambulancia.", 1);
                return;
            }
            if (!TargetClient.GetPlay().IsDying)
            {
                Session.SendWhisper("¡Esa persona no se encuentra inconsciente!", 1);
                return;
            }
            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes hacerle eso a un usuario ausente!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás mert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                return;
            }
            if (TargetClient.GetPlay().TargetReanim == false)
            {
                Session.SendWhisper("¡Primero debes reanimar a la persona!");
                return;
            }
            if(TargetClient.GetHabbo().Id != Session.GetPlay().HospReanim)
            {
                Session.SendWhisper("¡Esa persona no está dentro de tu ambulancia!");
                return;
            }
            if (Session.GetPlay().TryGetCooldown("savepatient", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Comodin Conditions
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendWhisper("Debes acercarte a la entrada del Hospital para salvar al Herido.", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                int Cost = RoleplayManager.AmbulSave;
                RoleplayManager.Shout(Session, "*Salva la vida de " + TargetClient.GetHabbo().Username + "*", 5);
                Session.SendWhisper("Ganaste $"+Cost+" por salvar la vida de " + TargetClient.GetHabbo().Username, 1);
                Session.GetHabbo().Credits += Cost;
                RoleplayManager.UpdateCreditsBalance(Session);
                Session.GetPlay().MoneyEarned += Cost;
                Session.GetPlay().HospReanim = 0;

                if(TargetClient.GetPlay().PediMedico)
                    TargetClient.GetPlay().PediMedico = false;
                TargetClient.GetPlay().TargetReanim = false;
                TargetClient.GetPlay().CurHealth = TargetClient.GetPlay().MaxHealth;
                TargetClient.GetPlay().RefreshStatDialogue();
                TargetClient.GetRoomUser().Frozen = false;
                TargetClient.GetPlay().IsDying = false;
                TargetClient.GetPlay().DyingTimeLeft = 0;
                TargetClient.GetPlay().InState = false;

                // Bajarlo de la Ambulancia
                #region Bajar al Herido
                // PASAJERO
                TargetClient.GetPlay().Pasajero = false;
                TargetClient.GetPlay().ChoferName = "";
                TargetClient.GetPlay().ChoferID = 0;
                TargetClient.GetRoomUser().CanWalk = true;
                TargetClient.GetRoomUser().FastWalking = false;
                TargetClient.GetRoomUser().TeleportEnabled = false;
                TargetClient.GetRoomUser().AllowOverride = false;

                // Descontamos Pasajero
                Session.GetPlay().PasajerosCount--;
                if (Session.GetPlay().PasajerosCount <= 0)
                    Session.GetPlay().Pasajeros = "";
                else
                {
                    StringBuilder builder = new StringBuilder(Session.GetPlay().Pasajeros);
                    builder.Replace(TargetClient.GetHabbo().Username + ";", "");
                    Session.GetPlay().Pasajeros = builder.ToString();
                }

                // CHOFER 
                Session.GetPlay().Chofer = (Session.GetPlay().PasajerosCount <= 0) ? false : true;
                Session.GetRoomUser().AllowOverride = (Session.GetPlay().PasajerosCount <= 0) ? false : true;
                #endregion

                TargetClient.SendWhisper("¡Has sido atendid@ por el servicio de Ambulancia y has sido revivid@! Pagas: $"+Cost, 1);
                if ((TargetClient.GetHabbo().Credits - Cost) >= 0)
                {
                    TargetClient.GetHabbo().Credits -= Cost;
                    RoleplayManager.UpdateCreditsBalance(TargetClient);
                }
                else
                {
                    TargetClient.GetPlay().Bank -= Cost;
                    RoleplayManager.UpdateBankBalance(TargetClient);
                    TargetClient.SendWhisper("Se te ha cobrado directamente a tu cuenta bancaria debido a que no tienes dinero suciente en tu Cartera.", 1);

                }
                Session.GetPlay().CooldownManager.CreateCooldown("savepatient", 1000, 10);
                return;
            }
            else
            {
                Session.SendWhisper("El pasajero no se encuentra cerca de ti.", 1);
                return;
            }
            #endregion
        }
    }
}
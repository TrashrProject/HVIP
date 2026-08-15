using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Timers.Types;
using Plus.HabboRoleplay.Timers;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class PurgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_purge"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Inicia/Detiene el evento de Purga"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetPlay().TryGetCooldown("purge"))
                return;

            if (RoleplayManager.PurgeEvent)
            {
                // Detener purga
                #region Execute
                RoleplayManager.PurgeEvent = false;
                PlusEnvironment.GetGame().GetClientManager().StaffAlertMsg(Session.GetHabbo().Username + " ha detenido el evento de 'La Purga'");

                #region WS
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                        continue;
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_purge", "close");
                }
                #endregion
                #endregion
            }
            else
            {
                // Iniciar purga
                #region Execute

                #region Check Workers & Special Checkers
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                        continue;

                    #region Force Desactivate PSV Mode
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_psv", "forceoff");
                    #endregion

                    if (client.GetPlay().IsWorking)
                    {
                        WorkManager.RemoveWorkerFromList(client);
                        client.GetPlay().IsWorking = false;
                        client.GetHabbo().Poof();
                        //RoleplayManager.CheckCorpCarp(client);
                        RoleplayManager.Shout(client, "*Ha dejado de trabajar*", 5);

                        #region Check Police Car
                        if (client.GetPlay().DrivingCar && client.GetPlay().CarEnableId == EffectsList.CarPolice)
                        {
                            #region Park
                            int ItemPlaceId = 0;
                            int roomid = client.GetRoomUser().RoomId;
                            Room = RoleplayManager.GenerateRoom(roomid, false);
                            VehiclesOwned VOD = null;
                            if (client.GetPlay().DrivingInCar)
                            {
                                RoleplayManager.Shout(client, "* Una Grúa se ha llevado el vehículo que " + client.GetHabbo().Username + " conducía.", 4);
                                // Actualizamos datos del auto en el diccionario y DB
                                PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(client, 0, false, out VOD);
                                ItemPlaceId = VOD.Id;
                                PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(client.GetPlay().DrivingCarId);
                                RoleplayManager.CheckCorpCarp(client);
                            }

                            #region Extra Conditions & Checks
                            #region CorpCar Respawn
                            client.GetPlay().CarJobLastItemId = ItemPlaceId;
                            #endregion

                            #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                            //Vars
                            string Pasajeros = client.GetPlay().Pasajeros;
                            string[] stringSeparators = new string[] { ";" };
                            string[] result;
                            result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string psjs in result)
                            {
                                GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                if (PJ != null)
                                {
                                    if (PJ.GetPlay().ChoferName == client.GetHabbo().Username)
                                    {
                                        RoleplayManager.Shout(PJ, "*Baja del vehículo de " + client.GetHabbo().Username + "*", 5);
                                    }
                                    // PASAJERO
                                    PJ.GetPlay().Pasajero = false;
                                    PJ.GetPlay().ChoferName = "";
                                    PJ.GetPlay().ChoferID = 0;
                                    PJ.GetRoomUser().CanWalk = true;
                                    PJ.GetRoomUser().FastWalking = false;
                                    PJ.GetRoomUser().TeleportEnabled = false;
                                    PJ.GetRoomUser().AllowOverride = false;

                                    // Descontamos Pasajero
                                    client.GetPlay().PasajerosCount--;
                                    StringBuilder builder = new StringBuilder(client.GetPlay().Pasajeros);
                                    builder.Replace(PJ.GetHabbo().Username + ";", "");
                                    client.GetPlay().Pasajeros = builder.ToString();

                                    // CHOFER 
                                    client.GetPlay().Chofer = (client.GetPlay().PasajerosCount <= 0) ? false : true;
                                    client.GetRoomUser().AllowOverride = (client.GetPlay().PasajerosCount <= 0) ? false : true;

                                    // SI EL PASAJERO ES UN HERIDO
                                    if (PJ.GetPlay().IsDying)
                                    {
                                        #region Send To Hospital
                                        RoleplayManager.Shout(PJ, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
                                        Room Room2 = RoleplayManager.GenerateRoom(PJ.GetRoomUser().RoomId);
                                        string MyCity2 = Room2.City;

                                        PlayRoom Data2;
                                        int ToHosp2 = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity2, out Data2);

                                        if (ToHosp2 > 0)
                                        {
                                            Room Room3 = RoleplayManager.GenerateRoom(ToHosp2);
                                            if (Room3 != null)
                                            {
                                                PJ.GetPlay().IsDead = true;
                                                PJ.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                                                PJ.GetHabbo().HomeRoom = ToHosp2;

                                                /*
                                                if (PJ.GetHabbo().CurrentRoomId != ToHosp)
                                                    RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                                                else
                                                    Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                                                */
                                                RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                                            }
                                            else
                                            {
                                                PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                                PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                                PJ.GetPlay().RefreshStatDialogue();
                                                PJ.GetRoomUser().Frozen = false;
                                                PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                            }
                                        }
                                        else
                                        {
                                            PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                            PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                            PJ.GetPlay().RefreshStatDialogue();
                                            PJ.GetRoomUser().Frozen = false;
                                            PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                        }
                                        PJ.GetPlay().IsDying = false;
                                        PJ.GetPlay().DyingTimeLeft = 0;
                                        #endregion
                                    }

                                    // SI EL PASAJERO ES COMPAÑERO DE BASURERO
                                    if (PJ.GetPlay().IsBasuPasaj)
                                        PJ.GetPlay().IsBasuPasaj = false;

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                                }
                            }
                            #endregion

                            #region Check Jobs

                            #region Taxista
                            if (client.GetPlay().Ficha != 0)
                            {
                                client.GetPlay().Ficha = 0;
                                client.GetPlay().FichaTimer = 0;
                                RoleplayManager.Shout(client, "*Apaga el Taxímetro y deja de trabajar*", 5);
                            }
                            #endregion

                            #endregion
                            #endregion

                            #region Online ParkVars
                            //Retornamos a valores predeterminados
                            client.GetPlay().DrivingCar = false;
                            client.GetPlay().DrivingInCar = false;
                            client.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                            //Combustible System
                            client.GetPlay().CarType = 0;// Define el gasto de combustible
                            client.GetPlay().CarFuel = 0;
                            client.GetPlay().CarMaxFuel = 0;
                            client.GetPlay().CarTimer = 0;
                            client.GetPlay().CarLife = 0;

                            client.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                            client.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                            client.GetRoomUser().ApplyEffect(0);
                            client.GetRoomUser().FastWalking = false;
                            #endregion

                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_vehicle", "close");
                            #endregion

                            RoleplayManager.CheckCorpCarp(client);
                        }
                        #endregion

                        #region Check Escorting
                        if (client.GetHabbo().Escorting > 0)
                        {
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PlusEnvironment.GetUsernameById(client.GetHabbo().Escorting));
                            if (TargetClient != null)
                            {
                                #region Stand
                                if (TargetClient.GetRoomUser().isSitting)
                                {
                                    TargetClient.GetRoomUser().Statusses.Remove("sit");
                                    TargetClient.GetRoomUser().Z += 0.35;
                                    TargetClient.GetRoomUser().isSitting = false;
                                    TargetClient.GetRoomUser().UpdateNeeded = true;
                                }
                                else if (TargetClient.GetRoomUser().isLying)
                                {
                                    TargetClient.GetRoomUser().Statusses.Remove("lay");
                                    TargetClient.GetRoomUser().Z += 0.35;
                                    TargetClient.GetRoomUser().isLying = false;
                                    TargetClient.GetRoomUser().UpdateNeeded = true;
                                }
                                #endregion

                                var This = client.GetHabbo();
                                var User = TargetClient.GetRoomUser();

                                RoleplayManager.Shout(client, "*Deja de escoltar a " + TargetClient.GetHabbo().Username + "*", 37);
                                User.ClearMovement(true);
                                TargetClient.GetRoomUser().CanWalk = true;
                                User.GetClient().GetHabbo().EscortID = 0;
                                This.Escorting = 0;
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                RoleplayManager.PurgeEvent = true;
                RoleplayManager.TimerManager.CreateTimer("purge", 1000, true);
                PlusEnvironment.GetGame().GetClientManager().StaffAlertMsg(Session.GetHabbo().Username + " ha iniciado el evento de 'La Purga'");

                #region WS
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                        continue;
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_purge", "open");
                }
                #endregion

                #endregion
            }

            Session.GetPlay().CooldownManager.CreateCooldown("purge", 1000, 80);
        }
    }
}

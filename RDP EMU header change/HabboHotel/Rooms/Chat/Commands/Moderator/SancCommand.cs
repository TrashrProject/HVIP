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
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class SancCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_sanc"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Sanciona a jugadores mandando al calabozo"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("sanc"))
                return;

            string MyCity = Room.City;
            int SancID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetSanc(MyCity, out PlayRoom Data);//sanc de la cd.

            if (Params.Length != 3)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona y el tiempo en minutos.", 1);
                return;
            }

            int Time = 0;
            if (!int.TryParse(Params[2], out Time))
            {
                Session.SendWhisper("El tiempo debe ser un número entero.", 1);
                return;
            }

            if(Time <= 0 || Time > 60)
            {
                Session.SendWhisper("El tiempo debe ser un número entre 1 y 60 minutos.", 1);
                return;
            }

            Habbo Target = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("No se puedo encontrár a ningún usuario con ese nombre.", 1);
                return;
            }

            if(Target.Rank > Session.GetHabbo().Rank)
            {
                Session.SendWhisper("¡No puedes hacerle eso a un rango superior!", 1);
                return;
            }
            #endregion
            if(Target.GetClient() != null && !Target.GetClient().GetPlay().IsDisconnecting)
            {
                // Online
                GameClient client = Target.GetClient();

                #region Check Workers
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
                        GameClient tclient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PlusEnvironment.GetUsernameById(client.GetHabbo().Escorting));
                        if (tclient != null)
                        {
                            #region Stand
                            if (tclient.GetRoomUser().isSitting)
                            {
                                tclient.GetRoomUser().Statusses.Remove("sit");
                                tclient.GetRoomUser().Z += 0.35;
                                tclient.GetRoomUser().isSitting = false;
                                tclient.GetRoomUser().UpdateNeeded = true;
                            }
                            else if (tclient.GetRoomUser().isLying)
                            {
                                tclient.GetRoomUser().Statusses.Remove("lay");
                                tclient.GetRoomUser().Z += 0.35;
                                tclient.GetRoomUser().isLying = false;
                                tclient.GetRoomUser().UpdateNeeded = true;
                            }
                            #endregion

                            var This = client.GetHabbo();
                            var User = tclient.GetRoomUser();

                            RoleplayManager.Shout(client, "*Deja de escoltar a " + tclient.GetHabbo().Username + "*", 37);
                            User.ClearMovement(true);
                            tclient.GetRoomUser().CanWalk = true;
                            User.GetClient().GetHabbo().EscortID = 0;
                            This.Escorting = 0;
                        }
                    }
                    #endregion
                }
                #endregion

                #region Check Dying or Dead
                if (client.GetPlay().IsDying || client.GetPlay().IsDead)
                {
                    client.GetPlay().IsDead = false;
                    client.GetPlay().IsDying = false;

                    if (client.GetRoomUser() != null)
                    {
                        client.GetRoomUser().ApplyEffect(0);
                        client.GetRoomUser().CanWalk = true;
                        client.GetRoomUser().Frozen = false;
                    }

                    client.GetPlay().DeadTimeLeft = 0;
                    client.GetPlay().DyingTimeLeft = 0;
                    client.GetPlay().CurHealth = client.GetPlay().MaxHealth;
                    
                    // Refrescamos WS
                    client.GetPlay().UpdateInteractingUserDialogues();
                    client.GetPlay().RefreshStatDialogue();
                }
                #endregion
                
                #region Check Cuffed
                if(client.GetPlay().Cuffed)
                    client.GetPlay().Cuffed = false;
                #endregion

                #region Check Jailed
                client.GetPlay().IsJailed = false;
                client.GetPlay().JailedTimeLeft = 0;
                client.GetHabbo().Poof(true);
                #endregion

                #region Desequipar
                if (client.GetPlay().EquippedWeapon != null)
                {
                    string UnEquipMessage = client.GetPlay().EquippedWeapon.UnEquipText;
                    UnEquipMessage = UnEquipMessage.Replace("[NAME]", client.GetPlay().EquippedWeapon.PublicName);

                    RoleplayManager.Shout(client, UnEquipMessage, 5);

                    if (client.GetRoomUser().CurrentEffect == client.GetPlay().EquippedWeapon.EffectID)
                        client.GetRoomUser().ApplyEffect(0);

                    if (client.GetRoomUser().CarryItemID == client.GetPlay().EquippedWeapon.HandItem)
                        client.GetRoomUser().CarryItem(0);

                    client.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                    client.GetPlay().EquippedWeapon = null;

                    client.GetPlay().WLife = 0;
                    client.GetPlay().Bullets = 0;
                }
                #endregion

                client.GetPlay().Sancs++;
                client.GetPlay().IsSanc = true;
                client.GetPlay().SancTimeLeft = Time;
                client.GetPlay().TimerManager.CreateTimer("sanc", 1000, false);

                if(client.GetRoomUser() != null)
                {
                    if(client.GetRoomUser().RoomId != SancID)
                        RoleplayManager.SendUserOld(client, SancID);
                }

                client.SendMessage(new RoomNotificationComposer("room_sanc", "message", "Has sido sancionad@ por Antirol durante " + Time + " minuto(s)"));
            }

            RoleplayManager.SetSancStatus(Target.Id, Time, SancID);
            Session.GetPlay().CooldownManager.CreateCooldown("sanc", 1000, 5);
            RoleplayManager.Shout(Session, "*Sanciona a " + Target.Username + " durante " + Time + " minuto(s)*", 23);
        }
    }
}

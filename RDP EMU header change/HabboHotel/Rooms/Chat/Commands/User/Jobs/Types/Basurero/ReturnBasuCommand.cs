using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.Vehicles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class ReturnBasuCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_return_basurero"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo el conductor del camión de basura, descarga la basura para recibir tu paga."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("cargcam"))
                return;
            #endregion

            #region Group Conditions
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

            if (Groups.Count <= 0)
            {
                Session.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                return;
            }

            int GroupNumber = -1;

            if (Groups[0].GType != 2)
            {
                if (Groups.Count > 1)
                {
                    if (Groups[1].GType != 2)
                    {
                        Session.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                        return;
                    }
                    GroupNumber = 1; // Segundo indicie de variable
                }
                else
                {
                    Session.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
                    return;
                }
            }
            else
            {
                GroupNumber = 0; // Primer indice de Variable Group
            }

            Session.GetPlay().JobId = Groups[GroupNumber].Id;
            Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
            #endregion

            #region Extra Conditions            
            // Existe el trabajo?
            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Session.GetPlay().JobId, Session.GetPlay().JobRank))
            {
                Session.GetPlay().TimeWorked = 0;
                Session.GetPlay().JobId = 0; // Desempleado
                Session.GetPlay().JobRank = 0;

                //Room.Group.DeleteMember(Session.GetHabbo().Id);// OJO ACÁ

                Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                return;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "basurero"))
            {
                Session.SendWhisper("Debes tener el trabajo de Basurero para usar ese comando.", 1);
                return;
            }

            #endregion

            #region Basurero Conditions
            string MyCity = Room.City;
            PlayRoom Data;
            int Basurero = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetBasureros(MyCity, out Data);
            if (Room.Id != Basurero)
            {
                Session.SendWhisper("¡Debes ir al Basurero de la Ciudad para descargar el camión!", 1);
                return;
            }
            if (Session.GetPlay().BasuTeamId <= 0)
            {
                Session.SendWhisper("¡Primero debes conseguir un compañero de trabajo para recolectar 15 contenedores de basura!", 1);
                return;
            }
            if (Session.GetPlay().DrivingInCar)
            {
                Session.SendWhisper("¡Debes estar conduciendo el camión de basura!", 1);
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

            if (corp != Session.GetPlay().JobId)
            {
                Session.SendWhisper("¡Debes estar conduciendo el camión de basura!", 1);
                return;
            }
            if (!Session.GetPlay().IsBasuChofer)
            {
                Session.SendWhisper("¡Solo el chofer del Camión puede hacer eso!", 1);
                return;
            }
            if (Session.GetPlay().BasuTrashCount < 15)
            {
                Session.SendWhisper("¡Deben recolectar 15 contedendores de basura para poder descargar el camión!", 1);
                return;
            }
            GameClient TeamPasaj = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetPlay().BasuTeamId);
            if (TeamPasaj == null)
            {
                Session.SendWhisper("Al parecer tu compañero de Basurero se ha ido y han Fracasado el Recorrido.", 1);

                // Quitar Camión
                PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Session.GetPlay().DrivingCarId);
                #region Online ParkVars
                //Retornamos a valores predeterminados
                Session.GetPlay().DrivingCar = false;
                Session.GetPlay().DrivingInCar = false;
                Session.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                //Combustible System
                Session.GetPlay().CarType = 0;// Define el gasto de combustible
                Session.GetPlay().CarFuel = 0;
                Session.GetPlay().CarMaxFuel = 0;
                Session.GetPlay().CarTimer = 0;
                Session.GetPlay().CarLife = 0;

                Session.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                Session.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                Session.GetRoomUser().ApplyEffect(0);
                Session.GetRoomUser().FastWalking = false;
                #endregion

                #region Retornamos Basurero Vars
                Session.GetPlay().BasuTeamId = 0;
                Session.GetPlay().BasuTeamName = "";
                Session.GetPlay().BasuTrashCount = 0;
                Session.GetPlay().IsBasuPasaj = false;
                Session.GetPlay().IsBasuChofer = false;
                #endregion
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "close");// WS FUEL
                return;
            }
            if (!Session.GetPlay().Pasajeros.Contains(TeamPasaj.GetHabbo().Username))
            {
                Session.SendWhisper("Tu compañero de Basurero debe estar de Pasajero de tu Camión.", 1);
                return;
            }

            #region Comodin Conditions
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendWhisper("Debes estar en cerca del bulto de basura central para terminar el recorrido.", 1);
                return;
            }
            #endregion

            #endregion

            #region Execute

            #region Pagas
            int PayC = (Session.GetPlay().BasuLvl * 25);
            int PayP = (TeamPasaj.GetPlay().BasuLvl * 25);
            #endregion

            // Quitar Camión
            PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Session.GetPlay().DrivingCarId);
            RoleplayManager.CheckCorpCarp(Session);

            #region Online ParkVars
            //Retornamos a valores predeterminados
            Session.GetPlay().DrivingCar = false;
            Session.GetPlay().DrivingInCar = false;
            Session.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

            //Combustible System
            Session.GetPlay().CarType = 0;// Define el gasto de combustible
            Session.GetPlay().CarFuel = 0;
            Session.GetPlay().CarMaxFuel = 0;
            Session.GetPlay().CarTimer = 0;
            Session.GetPlay().CarLife = 0;

            Session.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
            Session.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
            Session.GetRoomUser().ApplyEffect(0);
            Session.GetRoomUser().FastWalking = false;
            #endregion

            #region Retornamos Basurero Vars
            Session.GetPlay().BasuTeamId = 0;
            Session.GetPlay().BasuTeamName = "";
            Session.GetPlay().BasuTrashCount = 0;
            Session.GetPlay().IsBasuPasaj = false;
            Session.GetPlay().IsBasuChofer = false;

            TeamPasaj.GetPlay().BasuTeamId = 0;
            TeamPasaj.GetPlay().BasuTeamName = "";
            TeamPasaj.GetPlay().BasuTrashCount = 0;
            TeamPasaj.GetPlay().IsBasuPasaj = false;
            TeamPasaj.GetPlay().IsBasuChofer = false;
            #endregion

            #region Online ParkVars & Pasajero TeamPasaj
            //Retornamos a valores predeterminados
            TeamPasaj.GetPlay().DrivingCar = false;
            TeamPasaj.GetPlay().DrivingInCar = false;
            TeamPasaj.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

            //Combustible System
            TeamPasaj.GetPlay().CarType = 0;// Define el gasto de combustible
            TeamPasaj.GetPlay().CarFuel = 0;
            TeamPasaj.GetPlay().CarMaxFuel = 0;
            TeamPasaj.GetPlay().CarTimer = 0;
            TeamPasaj.GetPlay().CarLife = 0;

            TeamPasaj.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
            TeamPasaj.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
            if(TeamPasaj.GetRoomUser() != null)
                TeamPasaj.GetRoomUser().ApplyEffect(0);
            TeamPasaj.GetRoomUser().FastWalking = false;

            // PASAJERO
            TeamPasaj.GetPlay().Pasajero = false;
            TeamPasaj.GetPlay().ChoferName = "";
            TeamPasaj.GetPlay().ChoferID = 0;
            if (TeamPasaj.GetRoomUser() != null)
            {
                TeamPasaj.GetRoomUser().CanWalk = true;
                TeamPasaj.GetRoomUser().FastWalking = false;
                TeamPasaj.GetRoomUser().TeleportEnabled = false;
                TeamPasaj.GetRoomUser().AllowOverride = false;
            }

            // CHOFER 
            // Descontamos Pasajero
            Session.GetPlay().PasajerosCount = 0;
            Session.GetPlay().Pasajeros = "";            
            Session.GetPlay().Chofer = false;
            Session.GetRoomUser().AllowOverride = false;
            #endregion

            #region Retornamos Basurero Vars TeamPasaj
            TeamPasaj.GetPlay().BasuTeamId = 0;
            TeamPasaj.GetPlay().BasuTeamName = "";
            TeamPasaj.GetPlay().BasuTrashCount = 0;
            TeamPasaj.GetPlay().IsBasuPasaj = false;
            TeamPasaj.GetPlay().IsBasuChofer = false;

            TeamPasaj.GetPlay().BasuTeamId = 0;
            TeamPasaj.GetPlay().BasuTeamName = "";
            TeamPasaj.GetPlay().BasuTrashCount = 0;
            TeamPasaj.GetPlay().IsBasuPasaj = false;
            TeamPasaj.GetPlay().IsBasuChofer = false;
            #endregion

            #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
            //Vars
            string Pasajeros = Session.GetPlay().Pasajeros;
            string[] stringSeparators = new string[] { ";" };
            string[] result;
            result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string psjs in result)
            {
                GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                if (PJ != null && PJ.GetPlay() != null && PJ.GetRoomUser() != null)
                {
                    if (PJ.GetPlay().ChoferName == Session.GetHabbo().Username)
                    {
                        RoleplayManager.Shout(PJ, "*Baja del vehículo de " + Session.GetHabbo().Username + "*", 5);
                    }
                    // PASAJERO
                    PJ.GetPlay().Pasajero = false;
                    PJ.GetPlay().ChoferName = "";
                    PJ.GetPlay().ChoferID = 0;
                    PJ.GetRoomUser().CanWalk = true;
                    PJ.GetRoomUser().FastWalking = false;
                    PJ.GetRoomUser().TeleportEnabled = false;
                    PJ.GetRoomUser().AllowOverride = false;

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
                                RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                            }
                            else
                            {
                                PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                PJ.GetPlay().RefreshStatDialogue();
                                PJ.GetRoomUser().Frozen = false;
                                PJ.GetRoomUser().CanWalk = true;
                                PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                            }
                        }
                        else
                        {
                            PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                            PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                            PJ.GetPlay().RefreshStatDialogue();
                            PJ.GetRoomUser().Frozen = false;
                            PJ.GetRoomUser().CanWalk = true;
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

            // CHOFER 
            Session.GetPlay().PasajerosCount = 0;
            Session.GetPlay().Pasajeros = "";
            Session.GetPlay().Chofer = false;
            Session.GetRoomUser().AllowOverride = false;
            #endregion

            RoleplayManager.Shout(Session, "*Descarga el camión de basura completando su trabajo junto con "+TeamPasaj.GetHabbo().Username+"*", 5);
            Session.SendWhisper("¡Buen trabajo! Tus ganancias son: " + PayC, 1);
            TeamPasaj.SendWhisper("¡Buen trabajo! Tus ganancias son: " + PayP, 1);

            Session.GetHabbo().Credits += PayC;
            Session.GetPlay().MoneyEarned += PayC;
            Session.GetHabbo().UpdateCreditsBalance();
            TeamPasaj.GetHabbo().Credits += PayP;
            TeamPasaj.GetPlay().MoneyEarned += PayP;
            TeamPasaj.GetHabbo().UpdateCreditsBalance();

            RoleplayManager.JobSkills(Session, Session.GetPlay().JobId, Session.GetPlay().BasuLvl, Session.GetPlay().BasuXP);
            RoleplayManager.JobSkills(TeamPasaj, TeamPasaj.GetPlay().JobId, TeamPasaj.GetPlay().BasuLvl, TeamPasaj.GetPlay().BasuXP);
            
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "close");// WS FUEL
            Session.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
            return;
            #endregion
        }
    }
}

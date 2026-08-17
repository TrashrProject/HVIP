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
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboRoleplay.GangTurfs;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class ReturnCamCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_return_camion"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo Camionero, entrega tu Camión una vez entregada la mercancía para recibir tu paga."; }
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

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "camionero"))
            {
                Session.SendWhisper("Debes tener el trabajo de Camionero para usar ese comando.", 1);
                return;
            }
            if (!Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("Debes conducir un Camión para hacer eso.", 1);
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

            if (!Session.GetPlay().DrivingCar || !PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Camioneros"))
            {
                Session.SendWhisper("Debes conducir un Camión para hacer eso.", 1);
                return;
            }
            #endregion

            #region Camionero Conditions
            List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(Session.GetPlay().DrivingCarId);
            if (VO == null || VO.Count <= 0)
            {
                Session.SendWhisper("((No se pudo obtener información del vehículo que conduces))", 1);
                return;
            }
            if (VO[0].CamOwnId > 0 && VO[0].CamOwnId != Session.GetHabbo().Id)
            {
                Session.SendWhisper("Este camión pertenece a por otra persona. ((Si perdiste el tuyo usa :abandonarcarga))", 1);
                return;
            }
            if (VO[0].CamState == 0)
            {
                Session.SendWhisper("El camión no ha sido cargado aún. ¡Ve a cargarlo de mercancía! ((Usa :cargarcamion [ID]))", 1);
                return;
            }
            if (VO[0].CamState != 2)
            {
                Session.SendWhisper("El camión no ha sido descargado aún. ¡Ve a entregar la mercancía!", 1);
                return;
            }
            if (VO[0].CamDest != Room.Id)
            {
                Room _room = RoleplayManager.GenerateRoom(VO[0].CamDest);
                Session.SendWhisper("¡Debes ir a " + _room.Name + " para entregar el camión!", 1);
                return;
            }

            #region Comodin Conditions
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carr2" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendWhisper("Debes estar en la zona de entrega para terminar el recorrido.", 1);
                return;
            }
            #endregion

            #endregion

            #region Execute

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

            List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);

            #region Pagas
            int Amn = 0, Med = 0, Crack = 0, Piezas = 0, bonif = 0;

            #region Cants By Level
            if (Session.GetPlay().CamLvl == 1)
            {
                Amn = 13;
                Med = 2;
                Crack = 1;
                Piezas = 2;
            }
            else if (Session.GetPlay().CamLvl == 2)
            {
                Amn = 16;
                Med = 4;
                Crack = 2;
                Piezas = 5;
            }
            else if (Session.GetPlay().CamLvl == 3)
            {
                Amn = 20;
                Med = 6;
                Crack = 3;
                Piezas = 7;
            }
            else if (Session.GetPlay().CamLvl == 4)
            {
                Amn = 22;
                Med = 8;
                Crack = 4;
                Piezas = 7;
            }
            else if (Session.GetPlay().CamLvl == 5)
            {
                Amn = 25;
                Med = 10;
                Crack = 5;
                Piezas = 7;
            }
            else if (Session.GetPlay().CamLvl >= 6) // Max Lvl
            {
                Amn = 30;
                Med = 12;
                Crack = 6;
                Piezas = 7;
            }
            #endregion

            string win = "¡Excelente entra! Tus ganancias son: $" + Amn;
            #region By Carg
            if (VO[0].CamCargId == 3)// Drogas
            {
                Session.GetPlay().Cocaine += Crack;
                Session.GetPlay().Medicines += Med;
                Session.GetPlay().CocaineTaken += Crack;
                Session.GetPlay().MedicinesTaken += Med;
                //RoleplayManager.SaveQuickStat(Session, "cocaine", "" + Session.GetPlay().Cocaine);
                //RoleplayManager.SaveQuickStat(Session, "medicines", "" + Session.GetPlay().Medicines);
                win += " y " + Med + " Medicamentos + " + Crack + "g de Crack.";

                #region Gang Bonif
                if (MyGang != null && MyGang.Count > 0 && !MyGang[0].BankRuptcy)
                {
                    int NewTurfsCount = 0;
                    List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(MyGang[0].Id);
                    if (TF != null && TF.Count > 0)
                        NewTurfsCount = TF.Count;

                    bonif = RoleplayManager.GangsTurfBonif * NewTurfsCount;
                    if (bonif > 0)
                    {
                        MyGang[0].GangFarmCocaine += Crack;
                        MyGang[0].GangFarmMedicines += Med;
                        MyGang[0].Bank += bonif;
                        MyGang[0].UpdateStat("gang_farm_cocaine", MyGang[0].GangFarmCocaine);
                        MyGang[0].UpdateStat("gang_farm_medicines", MyGang[0].GangFarmMedicines);
                        MyGang[0].SetBussines(MyGang[0].Bank, MyGang[0].Stock);

                        Session.GetHabbo().Credits += bonif;
                        Session.GetPlay().MoneyEarned += bonif;
                        Session.GetHabbo().UpdateCreditsBalance();
                    }
                }
                #endregion
            }
            else if (VO[0].CamCargId == 4)// Armas
            {
                Session.GetPlay().ArmPieces += Piezas;
                //RoleplayManager.SaveQuickStat(Session, "Pieces", "" + Session.GetPlay().Pieces);
                win += " y " + Piezas + " Piezas de armas.";

                #region Gang Bonif
                if (MyGang == null && MyGang.Count <= 0 && !MyGang[0].BankRuptcy)
                {
                    int NewTurfsCount = 0;
                    List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(MyGang[0].Id);
                    if (TF != null && TF.Count > 0)
                        NewTurfsCount = TF.Count;

                    bonif = RoleplayManager.GangsTurfBonif * NewTurfsCount;
                    if (bonif > 0)
                    {
                        MyGang[0].Bank += bonif;
                        MyGang[0].SetBussines(MyGang[0].Bank, MyGang[0].Stock);

                        Session.GetHabbo().Credits += bonif;
                        Session.GetPlay().MoneyEarned += bonif;
                        Session.GetHabbo().UpdateCreditsBalance();
                    }
                }
                #endregion
            }

            Session.GetHabbo().Credits += Amn;
            Session.GetPlay().MoneyEarned += Amn;
            Session.GetHabbo().UpdateCreditsBalance();

            #endregion
            #endregion

            // Reseteamos Camion por seguridad
            VO[0].CamCargId = 0;
            VO[0].CamDest = 0;
            VO[0].CamOwnId = 0;
            VO[0].CamState = 0;

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

            Session.GetPlay().CamCargId = 0;// Retornamos a 0 para que pueda cargar otro

            RoleplayManager.Shout(Session, "*Entrega su camión completando su recorrido*", 5);
            Session.SendWhisper(win, 1);
            RoleplayManager.JobSkills(Session, Session.GetPlay().JobId, Session.GetPlay().CamLvl, Session.GetPlay().CamXP);

            if (MyGang != null && MyGang.Count > 0)
            {
                if (MyGang[0].BankRuptcy) {
                    Session.SendWhisper("Tu banda está en bancarota y no podrás gozar de los beneficios de ella.", 1);
                }
                else if (bonif > 0)
                {
                    MyGang[0].AddLog(Session.GetHabbo().Id, Session.GetHabbo().Username + " ha obtenido $ " + String.Format("{0:N0}", bonif) + " para la banda en cargas ilegales.", bonif);
                    Session.SendWhisper("¡Tu banda y tú han ganado una bonifcación extra de $ " + String.Format("{0:N0}", bonif) + " por tu entrega ilegal!", 1);
                }                    
            }

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "close");// WS FUEL
            Session.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
            #endregion
        }
    }
}

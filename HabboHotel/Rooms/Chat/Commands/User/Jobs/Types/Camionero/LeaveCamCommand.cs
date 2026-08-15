using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class LeaveCamCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_leave_camion"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo Camionero, permite abandonar tu carga para poder usar un nuevo camión."; }
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

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "camionero") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "basurero"))
            {
                Session.SendWhisper("Debes tener el trabajo de Camionero o Basurero para usar ese comando.", 1);
                return;
            }
            /*
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                return;
            }
            */
            #endregion

            #region Camionero Conditions
            if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "camionero"))
            {
                List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByCamOwnId(Session.GetHabbo().Id);
                if (VO == null || VO.Count <= 0)
                {
                    Session.SendWhisper("No tienes ninguna carga a tu nombre para abandonar.", 1);
                    return;
                }
                else
                {
                    // Quitar Camión de Diccionario
                    PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(VO[0].Id);
                    Session.GetPlay().CamCargId = 0;
                }
            }
            else
            {
                if (!Session.GetPlay().IsBasuChofer)
                {
                    Session.SendWhisper("No eres el chofer de ninguna carga de basura a abandonar.", 1);
                    return;
                }
            }
            #endregion

            #region Execute
            RoleplayManager.Shout(Session, "*Abandona la Carga de su Camión*", 5);
           
            if (!Session.GetPlay().IsBasuChofer)
                Session.SendWhisper("Tu Camión ha sido descargadado. No has terminado el recorrido, no se te pagará nada.", 1);
            else
            {
                Session.GetPlay().IsBasuChofer = false;

                // Solo al abandonar carga
                Session.GetPlay().BasuTeamId = 0;
                Session.GetPlay().BasuTeamName = string.Empty;
                Session.GetPlay().BasuTrashCount = 0;

                Session.SendWhisper("Su Camión ha sido abandonado. No han terminado el recorrido, no se les pagará nada.", 1);
            }

            RoleplayManager.CheckCorpCarp(Session);

            #region Driving
            if (Session.GetPlay().DrivingCar)
            {
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
                        {
                            PJ.GetPlay().IsBasuPasaj = false;

                            // Solo al abandonar carga
                            PJ.GetPlay().BasuTeamId = 0;
                            PJ.GetPlay().BasuTeamName = string.Empty;
                            PJ.GetPlay().BasuTrashCount = 0;
                            PJ.SendWhisper("Su Camión ha sido abandonado. No han terminado el recorrido, no se les pagará nada.", 1);
                        }

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                    }
                }

                // CHOFER 
                Session.GetPlay().PasajerosCount = 0;
                Session.GetPlay().Pasajeros = "";
                Session.GetPlay().Chofer = false;
                Session.GetRoomUser().AllowOverride = false;
                #endregion

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

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "close");// WS FUEL
            }
            #endregion

            Session.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
            #endregion
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class DepositCamCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_depositar_camion"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Siendo Camionero, entrega la carga de tu Camión a tu respectivo destino."; }
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
                Session.SendWhisper("Este camión no ha sido cargado bajo tu nombre. No puedes hacer recorridos ajenos.", 1);
                return;
            }
            if (VO[0].CamState != 1)
            {
                Session.SendWhisper("El camión no ha sido cargado aún.", 1);
                return;
            }
            if (VO[0].CamState == 2)
            {
                Session.SendWhisper("El camión ya ha sido descargado. ¡Ve a entregarlo a Camioneros! ((Usa :entregarcamion))", 1);
                return;
            }
            if (VO[0].CamDest != Room.Id)
            {
                Room _room = RoleplayManager.GenerateRoom(VO[0].CamDest);
                Session.SendWhisper("¡Debes ir a "+ _room.Name +" para entregar la mercancía!", 1);
                return;
            }

            #region Comodin Conditions
            Item BTile = null;
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendWhisper("Debes estar en la zona de descarga de tu destino para entregar la mercancía.", 1);
                return;
            }
            #endregion
                    
            if (Session.GetPlay().IsCamUnLoading)
            {
                Session.SendWhisper("Ya te encuentras descargando el camión. Por favor espera...", 1);
                return;
            }
            #endregion

            #region Execute
            Session.GetPlay().IsCamUnLoading = true;
            Session.GetPlay().LoadingTimeLeft = RoleplayManager.CamDepositTime;

            RoleplayManager.Shout(Session, "*Comienza a descargar su camión*", 5);
            Session.SendWhisper("Debes esperar " + Session.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
            Session.GetPlay().TimerManager.CreateTimer("general", 1000, true);
            Session.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
            #endregion
        }
    }
}

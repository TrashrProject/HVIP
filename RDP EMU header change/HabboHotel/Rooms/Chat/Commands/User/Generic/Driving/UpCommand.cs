using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using System.Drawing;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class UpCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_driving_up"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Permite subir de pasajero a un auto que alguien conduzca."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre del chofer. :subir [usuario]", 1);
                return;
            }
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error en buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }
            if (Session.GetHabbo().EscortID > 0)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás siendo escoltad@", 1);
                return;
            }
            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás vas en Taxi", 1);
                return;
            }
            if (Session.GetHabbo().Escorting > 0)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás escoltas a alguien", 1);
                return;
            }
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk)
            {
                Session.SendWhisper("Al parecer no puedes ni moverte para poder subir al vehículo.", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡Ya vas de pasajero de alguien!", 1);
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
            if (TargetClient == Session)
            {
                Session.SendWhisper("No puedes ir de pasajer@ de ti mism@.", 1);
                return;
            }
            if (Session.GetPlay().DrivingInCar || Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }      
            if (Session.GetRoomUser().IsWalking)
			{
                Session.SendWhisper("¡No puedes hacer eso mientras caminas!", 1);
                return;
            }
            if (!TargetClient.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡Esa persona no se encuentra conduciendo!", 1);
                return;
            }
            if (TargetClient.GetRoomUser().IsWalking)
            {
                Session.SendWhisper("¡No subirte a un vehículo en movimiento!", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetClient.GetRoomUser().X, TargetClient.GetRoomUser().Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance > 1)
            {
                Session.SendWhisper("¡Debes acercarte más al chofer!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("pasajero"))
                return;
            #endregion

            #region Execute            
            VehiclesOwned VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwned(TargetClient.GetPlay().DrivingCarId);
            if(VO == null)
            {
                Session.SendWhisper("No se pudo obtener información del vehículo de " + TargetClient.GetHabbo().Username, 1);
                return;
            }

            #region Get Veihcle Info (play_vehicles)  
            Vehicle vehicle = null;
            bool found = false;
            foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
            {
                if (!found)
                {
                    if (Vehicle.DisplayName == VO.Model)
                    {
                        found = true;
                        vehicle = Vehicle;
                    }
                }
            }
            //Al examinar todos los autos ninguno conincide con el item donde está parado el user...
            if (!found || vehicle == null)
            {
                Session.SendWhisper("No se pudo obtener información del vehículo que " + TargetClient.GetHabbo().Username + " conduce.", 1);
                return;
            }
            #endregion

            if (TargetClient.GetPlay().PasajerosCount >= vehicle.MaxDoors)
            {
                Session.SendWhisper("No hay espacio suficiente para subir de pasajero en ese vehículo.", 1);
                return;
            }

            if(VO.State == 1 || VO.State == 3)
            {
                Session.SendWhisper("Las puertas del Vehículo están cerradas.", 1);
                return;
            }

            // PASAJERO
            Session.GetRoomUser().ClearMovement(true);
            Session.GetPlay().Pasajero = true;
            Session.GetPlay().ChoferName = TargetClient.GetHabbo().Username;
            Session.GetPlay().ChoferClient = TargetClient;
            Session.GetPlay().ChoferID = TargetClient.GetHabbo().Id;
            Session.GetRoomUser().CanWalk = false;
            Session.GetRoomUser().FastWalking = true;
            Session.GetRoomUser().TeleportEnabled = true;
            Session.GetRoomUser().AllowOverride = true;

            // CHOFER 
            TargetClient.GetPlay().Chofer = true;
            TargetClient.GetPlay().Pasajeros += Session.GetHabbo().Username + ";";
            TargetClient.GetPlay().PasajerosCount++;
            TargetClient.GetRoomUser().FastWalking = true;
            TargetClient.GetRoomUser().AllowOverride = true;
            
            //Animación de subir al Auto
            int NewX = TargetClient.GetRoomUser().X;
            int NewY = TargetClient.GetRoomUser().Y;
            Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(Session.GetRoomUser(), new Point(NewX, NewY), 0, Room.GetGameMap().SqAbsoluteHeight(NewX, NewY)));
            Session.GetRoomUser().MoveTo(NewX, NewY);


            RoleplayManager.Shout(Session, "*Sube al vehículo de " + TargetClient.GetHabbo().Username + "*", 5);
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "open");// WS FUEL
            Session.GetPlay().CooldownManager.CreateCooldown("pasajero", 1000, 5);

            #region Basurero
            if (TargetClient.GetPlay().IsBasuChofer)
            {
                if (Session.GetPlay().IsBasuChofer && Session.GetPlay().BasuTeamId > 0)
                {
                    Session.SendWhisper("¡No puedes ser recolector de otro chofer! Primero abandona la carga actual.", 1);
                    return;
                }

                if (TargetClient.GetPlay().BasuTeamId <= 0)
                {
                    // Asignamos de compañero al que subió siempre y cuando sea basurero también.
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "basurero"))
                    {
                        if (Session.GetHabbo().VIPRank < 2)
                        {
                            if (!Session.GetPlay().IsWorking)
                            {
                                Session.SendWhisper("¡Debes tener el uniforme de Basurero para asignarte de compañero a ese chofer!", 1);
                                return;
                            }
                        }

                        Session.GetPlay().IsBasuPasaj = true;
                        // Sincronizamos Team
                        TargetClient.GetPlay().BasuTeamId = Session.GetHabbo().Id;
                        TargetClient.GetPlay().BasuTeamName = Session.GetHabbo().Username;
                        Session.GetPlay().BasuTeamId = TargetClient.GetHabbo().Id;
                        Session.GetPlay().BasuTeamName = TargetClient.GetHabbo().Username;
                        Session.SendWhisper("Se te ha asignado a " + TargetClient.GetHabbo().Username + " como tu Compañero de Basurero. ¡Pueden comenzar!", 1);
                        TargetClient.SendWhisper("Se te ha asignado a " + Session.GetHabbo().Username + " como tu Compañero de Basurero. ¡Pueden comenzar!", 1);

                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient,
                            "compose_basurero|" +
                            "showinfo|" +
                            TargetClient.GetHabbo().Username + "|" + // Chofer
                            Session.GetHabbo().Username + "|" + // Recolector
                            TargetClient.GetPlay().BasuTrashCount + "/15|" +
                            TargetClient.GetPlay().IsBasuChofer);
                    }
                }
                else
                {
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "basurero"))
                        Session.SendWhisper("Esta persona ya tiene un compañero de trabajo asignado.", 1);
                }

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session,
                            "compose_basurero|" +
                            "showinfo|" +
                            TargetClient.GetHabbo().Username + "|" + // Chofer
                            Session.GetHabbo().Username + "|" + // Recolector
                            Session.GetPlay().BasuTrashCount + "/15|" +
                            Session.GetPlay().IsBasuChofer);
            }
            #endregion
            #endregion
        }
    }
}

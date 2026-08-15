using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.Vehicles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class UpPatientCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hosp_up"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Sube a una persona inconsciente a tu ambulancia una vez reanimada."; }
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
            if (Session.GetPlay().PasajerosCount >= vehicle.MaxDoors)
            {
                Session.SendWhisper("No hay espacio suficiente para un pasajero en tu Ambulancia.", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("uppatient", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 2)
            {
                RoleplayManager.Shout(Session, "*Sube a " + TargetClient.GetHabbo().Username + " a su Ambulancia*", 5);
                Session.SendWhisper("¡Bien hecho! ((Ahora traslada al paciente al Hospital y usa :salvar para recibir tu pago.))", 1);
                Session.GetPlay().HospReanim = TargetClient.GetHabbo().Id;                

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

                #region Subir
                // HERIDO
                TargetClient.GetPlay().Pasajero = true;
                TargetClient.GetPlay().ChoferName = Session.GetHabbo().Username;
                TargetClient.GetPlay().ChoferID = Session.GetHabbo().Id;
                TargetClient.GetRoomUser().CanWalk = false;
                TargetClient.GetRoomUser().FastWalking = true;
                TargetClient.GetRoomUser().TeleportEnabled = true;
                TargetClient.GetRoomUser().AllowOverride = true;

                // MEDICO (YO) 
                Session.GetPlay().Chofer = true;
                Session.GetPlay().Pasajeros += TargetClient.GetHabbo().Username + ";";
                Session.GetPlay().PasajerosCount++;
                Session.GetRoomUser().FastWalking = true;
                Session.GetRoomUser().AllowOverride = true;

                //Animación de subir al Auto
                int NewX = Session.GetRoomUser().X;
                int NewY = Session.GetRoomUser().Y;
                Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(TargetClient.GetRoomUser(), new Point(NewX, NewY), 0, Room.GetGameMap().SqAbsoluteHeight(NewX, NewY)));
                TargetClient.GetRoomUser().MoveTo(NewX, NewY);
                #endregion

                TargetClient.GetPlay().TimerManager.CreateTimer("uppatient", 1000, false);
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona a reanimar.", 1);
                return;
            }
            #endregion
        }
    }
}
using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ServiceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_service_jobs"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Pide el servicio de grua, médico, mecánico o armero."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el tipo de servicio. (medico, mecanico, grua).", 1);
                return;
            }

            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡Todos los servicios no están disponibles durante la purga!", 1);
                return;
            }
            #endregion

            #region Execute
            string Serv = Convert.ToString(Params[1]);
            int roomid = 0;
            Room ThisRoom = null;
            switch (Serv)
            {
                #region Médico
                case "medico":
                    #region Conditions
                    if (!Session.GetPlay().IsDying)
                    {
                        Session.SendWhisper("¡Debes estar inconsciente para llamar a un paramédico!", 1);
                        return;
                    }
                    if (Session.GetPlay().TryGetCooldown("servmedico", true))
                        return;
                    #endregion

                    #region Execute
                    roomid = Session.GetRoomUser().RoomId;
                    ThisRoom = RoleplayManager.GenerateRoom(roomid, false);
                    if (ThisRoom == null)
                    {
                        Session.SendWhisper("Ha ocurrido un error inesperado, contacta con un Administrador. [1]", 1);
                        return;
                    }
                    RoleplayManager.Shout(Session, "*Llamó a un paramédico y espera*", 5);
                    Session.SendWhisper("Has mandado una solicitud de Ambulancia. Un Médico atenderá tu llamado. Por favor espera.", 1);
                    Session.GetRoomUser().ApplyEffect(599);
                    Session.GetPlay().PediMedico = true;
                    PlusEnvironment.GetGame().GetClientManager().sendWorkAlert(Session.GetHabbo().Username + " ha solicitado servicio de médico en "+ThisRoom.Name, "heal", true);
                    Session.GetPlay().CooldownManager.CreateCooldown("servmedico", 1000, 30);
                    #endregion
                    break;
                #endregion

                #region Armero
                case "armero":
                    #region Basic Conditions
                    if (Session.GetPlay().EquippedWeapon == null)
                    {
                        Session.SendWhisper("Debes equipar un arma para solicitar los servicios de un Armero.", 1);
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
                    if (Session.GetPlay().DrivingCar)
                    {
                        Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                        return;
                    }
                    if (Session.GetPlay().TryGetCooldown("servarm", true))
                    {
                        Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                        return;
                    }
                    #endregion

                    #region Execute
                    roomid = Session.GetRoomUser().RoomId;
                    ThisRoom = RoleplayManager.GenerateRoom(roomid, false);
                    if (ThisRoom == null)
                    {
                        Session.SendWhisper("Ha ocurrido un error inesperado, contacta con un Administrador. [1]", 1);
                        return;
                    }
                    RoleplayManager.Shout(Session, "*Ha publicado una solicitud de Armero*", 5);
                    Session.SendWhisper("Has mandado una solicitud de Armero. Un Armero atenderá tu llamado. Por favor espera.", 1);
                    Session.GetPlay().PediArm = true;
                    PlusEnvironment.GetGame().GetClientManager().sendWorkAlert(Session.GetHabbo().Username + " ha solicitado servicio de Armero en " + ThisRoom.Name, "armero");
                    Session.GetPlay().CooldownManager.CreateCooldown("servarm", 1000, 30);
                    #endregion
                    break;
                #endregion

                #region Grúa
                case "grua":
                    #region Basic Conditions
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
                    if (Session.GetPlay().DrivingCar)
                    {
                        Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                        return;
                    }
                    if (Session.GetPlay().TryGetCooldown("grua", true))
                    {
                        Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                        return;
                    }
                    #endregion

                    if (!Room.MunicipalidadEnabled)
                    {
                        Session.SendWhisper("Debes estar en la municpalidad para pedir ese servicio.", 1);
                        return;
                    }

                    #region Action Point Conditions
                    Item BTile = null;
                    BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Session.GetRoomUser().Coordinate);
                    if (BTile == null)
                    {
                        Session.SendWhisper("Debes acercarte al mostrador para solicitar el servicio de grúa.", 1);
                        return;
                    }
                    #endregion

                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "close");
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "open_grua");
                    break;
                #endregion

                #region Taxi OFF
                    /*
                case "taxi":
                    #region Basic Conditions
                    
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
                    if (Session.GetPlay().DrivingCar)
                    {
                        Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                        return;
                    }
                    if (Session.GetPlay().TryGetCooldown("servtaxi", true))
                    {
                        Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                        return;
                    }
                    #endregion

                    #region Execute
                    roomid = Session.GetRoomUser().RoomId;
                    ThisRoom = RoleplayManager.GenerateRoom(roomid, false);
                    if (ThisRoom == null)
                    {
                        Session.SendWhisper("Ha ocurrido un error inesperado, contacta con un Administrador. [1]", 1);
                        return;
                    }
                    RoleplayManager.Shout(Session, "*Ha solicitado un Taxi*", 5);
                    Session.SendWhisper("Has mandado una solicitud de Taxi. Un Taxista atenderá tu llamado. Por favor espera.", 1);
                    PlusEnvironment.GetGame().GetClientManager().sendWorkAlert(Session.GetHabbo().Username + " ha solicitado un taxi en " + ThisRoom.Name, "taxi");
                    Session.GetPlay().CooldownManager.CreateCooldown("sertaxi", 1000, 30);
                    #endregion
                    break;
                    */
                #endregion

                #region Mecánico
                case "mecanico":
                    #region Basic Conditions

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
                    if (Session.GetPlay().DrivingCar)
                    {
                        Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                        return;
                    }
                    if (Session.GetPlay().TryGetCooldown("servmeca", true))
                    {
                        Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                        return;
                    }
                    #endregion

                    #region Execute
                    roomid = Session.GetRoomUser().RoomId;
                    ThisRoom = RoleplayManager.GenerateRoom(roomid, false);
                    if (ThisRoom == null)
                    {
                        Session.SendWhisper("Ha ocurrido un error inesperado, contacta con un Administrador. [2]", 1);
                        return;
                    }
                    RoleplayManager.Shout(Session, "*Ha solicitado un Mecánico*", 5);
                    Session.SendWhisper("Has mandado una solicitud de Mécánico. Un Mecánico atenderá tu llamado. Por favor espera.", 1);
                    Session.GetPlay().PediMec = true;
                    PlusEnvironment.GetGame().GetClientManager().sendWorkAlert(Session.GetHabbo().Username + " ha solicitado un mecánico en " + ThisRoom.Name, "mecanico", true);
                    Session.GetPlay().CooldownManager.CreateCooldown("sermeca", 1000, 30);
                    #endregion
                    break;
                #endregion

                #region Default
                default:
                    Session.SendWhisper("Ese servicio no existe. ((Usa :servicio [medico/mecanico/grua/armero]))", 1);
                    break;
                #endregion
            }

            #endregion
        }
    }
}
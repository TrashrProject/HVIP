using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Database.Interfaces;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using System.Data;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.VehiclesJobs;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class MyCarsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mycars"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Te muestra una lista con todos tu(s) vehículo(s)."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getMyVehiclesOwned(Session.GetHabbo().Id);

            #region Conditions
            if (Session.GetPlay().TryGetCooldown("mycars"))
                return;
            #endregion

            #region Execute
            string str = "";
            str += "\n============================================\n                  Listado de tus Vehículos \n============================================\n";
            if (VO == null || VO.Count <= 0)
            {
                str += "No tienes ningún vehículo a tu nombre. ¡Compra uno en el Concesionario de la ciudad!";
            }
            else
            {
                foreach (VehiclesOwned _vo in VO)
                {
                    #region Get Estado
                    int state = _vo.State;
                    string mode = "";
                    if (state == 0)
                        mode = "Óptimo & Abierto";
                    else if (state == 1)
                        mode = "Óptimo & Cerrado";
                    else if (state == 2)
                        mode = "Averiado & Abierto";
                    else if (state == 3)
                        mode = "Averiado & Cerrado";
                    else if (state == 4)//?
                        mode = "En Grúa";
                    #endregion

                    Vehicle vehicle = VehicleManager.getVehicle(_vo.Model);

                    str += "\nModelo: " + _vo.Model + "\n";
                    str += "Última persona en manejarlo: " + PlusEnvironment.GetGame().GetClientManager().GetNameById(_vo.LastUserId) + "\n";
                    str += "Vida: " + _vo.CarLife + "/100\n";
                    if(vehicle != null)
                        str += "Combustible: " + _vo.Fuel + "/" + vehicle.MaxFuel + "\n";
                    //str += "KM: " + _vo.Km + "\n";
                    str += "Estado: " + mode + "\n";
                    str += "Traba: " + (_vo.Traba ? "Sí" : "No") + "\n";
                    str += "Alarma: " + (_vo.Alarm ? "Sí" : "No") + "\n";
                    if (vehicle != null)
                        str += "Max. Pasajeros: " + vehicle.MaxDoors + "\n";

                }

                str += "\n\nRecuerda que puedes ir a la Municipalidad de la Ciudad a pedir servicio de Grúa para tu vehículo extraviado.";
            }
            Session.SendNotifWithScroll(str);
            Session.GetPlay().CooldownManager.CreateCooldown("mycars", 1000, 3);
            #endregion
        }

    }
}

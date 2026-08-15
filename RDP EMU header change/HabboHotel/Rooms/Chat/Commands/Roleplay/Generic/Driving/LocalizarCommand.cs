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
    class LocalizarCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_localizar"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Te muestra donde se encuentran tu(s) vehículo(s)."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getMyVehiclesOwned(Session.GetHabbo().Id);

            #region Conditions
            if (VO == null || VO.Count <= 0)
            {
                Session.SendWhisper("No tienes ningún vehículo a tu nombre a localizar. ¡Compra uno en el Concesionario de la ciudad!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("localizar"))
                return;
            #endregion

            #region Execute
            string str = "";
            str += "\n============================================\n                  Localizador de tus Vehículos \n============================================\n";

            foreach (VehiclesOwned _vo in VO)
            {
                Room loc = RoleplayManager.GenerateRoom(_vo.Location);
                string localizado = (loc != null) ? loc.Name : "Corralón";

                str += "\nModelo: " + _vo.Model + "\n";
                str += "Última Localización: " + localizado + "\n";
            }

            str += "\n\nRecuerda que puedes ir a la Municipalidad de la Ciudad a pedir servicio de Grúa para tu vehículo extraviado.";
            Session.SendNotifWithScroll(str);
            Session.GetPlay().CooldownManager.CreateCooldown("localizar", 1000, 3);
            #endregion
        }

    }
}

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Incoming.Groups;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Cache;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using System.Text.RegularExpressions;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Vehicles;
using System.Data;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// VehiclesWebEvent class.
    /// </summary>
    class VehiclesWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;
            /*
            if (!Client.GetPlay().UsingAtm)
            {
                Client.SendNotification("Buen intento, tratando de injectar el systema, ve a un ATM!");
                return;
            }
            */

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);
        

            switch (Action)
            {

                #region Open
                case "open":
                    {
                        int CarFuel = Client.GetPlay().CarFuel;
                        int CarMaxFuel = Client.GetPlay().CarMaxFuel;

                        if (Client.GetPlay().Pasajero)
                        {
                            if (Client.GetPlay().ChoferClient != null)
                            {
                                CarFuel = Client.GetPlay().ChoferClient.GetPlay().CarFuel;
                                CarMaxFuel = Client.GetPlay().ChoferClient.GetPlay().CarMaxFuel;
                            }
                        }
                        else
                        {
                            #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                            //Vars
                            string Pasajeros = Client.GetPlay().Pasajeros;
                            string[] stringSeparators = new string[] { ";" };
                            string[] result;
                            result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string psjs in result)
                            {
                                GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                if (PJ != null)
                                {
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "open");// WS FUEL
                                }
                            }
                            #endregion
                        }

                        string SendData = "";
                        SendData += CarFuel + ";";
                        SendData += CarMaxFuel + ";";
                        Socket.Send("compose_fuel|open|" + SendData);
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Socket.Send("compose_fuel|close|");
                        break;
                    }
                #endregion

                #region Baul
                case "baul":
                    {
                        Client.GetPlay().ViewBaul = true;
                        string SendData = "";
                        SendData += Client.GetPlay().CarWSBaul + ";";
                        Socket.Send("compose_vehicle|baul|" + SendData);
                        break;
                    }
                #endregion

                #region Close Baul
                case "closebaul":
                    {
                        Client.GetPlay().ViewBaul = false;
                        Client.GetPlay().CarWSBaul = "";
                        Socket.Send("compose_vehicle|closebaul|");
                        break;
                    }
                #endregion

                #region Open Shop
                case "openshop":
                    {
                        #region Conditions & Vars
                        Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                        if (Room == null)
                            return;

                        if (Client.GetPlay().TryGetCooldown("openshopcar"))
                            return;

                        #region Conditions
                        if (!Room.BuyCarEnabled)
                        {
                            Client.SendWhisper("Debes ir a un concesionario para comprar vehículos.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes acercarte a un despacho para comprar un vehículo.", 1);
                            return;
                        }
                        #endregion
                        #endregion

                        Client.GetPlay().ViewCarList = true;

                        #region HTML
                        string html = "";
                        List<Vehicle> PO = VehicleManager.getAllVehicles();
                        if (PO != null && PO.Count > 0)
                        {
                            foreach (var car in PO)
                            {
                                if (car.CarCorp >= 1)
                                    continue;

                                string Price = (car.Price > 100) ? "$ " + String.Format("{0:N0}", car.Price) : car.Price + " PL";
                                
                                html += "<div class=\"ft1\">";
                                html += "<div style=\"background-image:url('"+ RoleplayManager.CdnURL + "/ws_resources/images/Vehicles/"+ car.EffectID +".png');height: 150px;width: 250px;\"></div>";
                                html += "<div class=\"datos2\">Nombre: <span>"+ car.DisplayName +"</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Capacidad de combustible: <span>"+ car.MaxFuel +" Litros</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Espacio en el baúl: <span>"+ car.MaxTrunks +" lugares</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Pasajeros Límite: <span>"+ car.MaxDoors +" espacio(s)</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Precio: <span style=\"color:#339900\"><font color=\"orange\">"+ Price +"</font> - <div id=\""+ car.EffectID +","+ car.DisplayName +"\" class=\"shopcar\"></div></span>";
                                html += "</div>";
                                html += "</div>";
                            }
                        }
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_vehicle|openshop|" + SendData);
                        Client.GetPlay().CooldownManager.CreateCooldown("openshopcar", 1000, 1);
                        break;
                    }
                #endregion

                #region Close Shop
                case "closeshop":
                    {
                        Client.GetPlay().ViewCarList = false;
                        Socket.Send("compose_vehicle|closeshop|");
                        break;
                    }
                #endregion

                #region Buy Car
                case "shop":
                    {
                        Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                        if (Room == null)
                            return;

                        #region Conditions & Vars
                        if (Client.GetPlay().TryGetCooldown("buy"))
                            return;

                        #region Conditions
                        if (!Room.BuyCarEnabled)
                        {
                            Client.SendWhisper("Debes ir a un concesionario para comprar vehículos.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes acercarte a un despacho para comprar un vehículo.", 1);
                            return;
                        }
                        #endregion

                        if (Client.GetPlay().DrivingInCar)
                        {
                            Socket.Send("compose_vehicle|shopmsg|Primero debes detener el vehículo que tienes afuera.");
                            return;
                        }
                        if (Client.GetPlay().InTutorial && Client.GetPlay().TutorialStep < 23)
                        {
                            Client.SendWhisper("¡Hey, no tan rápido! Ve siguiendo el Tutorial paso a paso para guiarte de la mejor manera.", 1);
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        int GetEffect;
                        string GetCarModel = ReceivedData[2];
                        if (!int.TryParse(ReceivedData[1], out GetEffect))
                        {
                            Socket.Send("compose_vehicle|shopmsg|Ha ocurrido un problema al obtener la Información del Vehículo.");
                            return;
                        }
                        GetCarModel = Regex.Replace(GetCarModel, "<(.|\\n)*?>", string.Empty);

                        Vehicle vehicle = VehicleManager.getVehicle(GetCarModel);
                        if(vehicle == null)
                        {
                            Socket.Send("compose_vehicle|shopmsg|Ha ocurrido un problema al obtener la Información del Vehículo. [2]");
                            return;
                        }
                        if (vehicle.Price > 100)
                        {
                            if (Client.GetHabbo().Credits < vehicle.Price)
                            {
                                Socket.Send("compose_vehicle|shopmsg|No tienes dinero suficiente para comprar ese vehículo.");
                                return;
                            }
                        }
                        else
                        {
                            if (Client.GetHabbo().Diamonds < vehicle.Price)
                            {
                                Socket.Send("compose_vehicle|shopmsg|No tienes los Platinos suficientes para comprar ese vehículo.");
                                return;
                            }
                        }
                        #endregion

                        #region Check Car Limit
                        int MyCars = 0;
                        List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getMyVehiclesOwned(Client.GetHabbo().Id);
                        if(VO != null)
                            MyCars = VO.Count;

                        if(MyCars >= 2 && Client.GetHabbo().VIPRank == 0)
                        {
                            Socket.Send("compose_vehicle|shopmsg|¡Ya tienes "+MyCars+" Vehículos! Usuarios VIP pueden tener más de dos.");

                            #region Tutorial Step Check
                            if (Client.GetPlay().TutorialStep == 23 && Room.BuyCarEnabled && Room.Type.Equals("public"))
                            {
                                Client.GetPlay().TutorialStep = 27;
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|26");
                            }
                            #endregion
                            return;
                        }
                        else if (MyCars >= 3 && Client.GetHabbo().VIPRank != 2)
                        {
                            Socket.Send("compose_vehicle|shopmsg|¡Ya tienes "+MyCars+" Vehículos! Usuarios VIP2 pueden tener hasta 4 autos.");

                            #region Tutorial Step Check
                            if (Client.GetPlay().TutorialStep == 23 && Room.BuyCarEnabled && Room.Type.Equals("public"))
                            {
                                Client.GetPlay().TutorialStep = 27;
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|26");
                            }
                            #endregion
                            return;
                        }
                        else if (MyCars >= 4)
                        {
                            Socket.Send("compose_vehicle|shopmsg|¡Ya tienes " + MyCars + " Vehículos! Solo es posible tener hasta 4 autos.");

                            #region Tutorial Step Check
                            if (Client.GetPlay().TutorialStep == 23 && Room.BuyCarEnabled && Room.Type.Equals("public"))
                            {
                                Client.GetPlay().TutorialStep = 27;
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|26");
                            }
                            #endregion
                            return;
                        }
                        #endregion

                        #region Execute
                        if (!PlusEnvironment.GetGame().GetVehiclesOwnedManager().TryCreateVehicleOwned(Client, 0, vehicle.ItemID, Client.GetHabbo().Id, Client.GetHabbo().Id, vehicle.Model, vehicle.MaxFuel, 0, 0, false, false, Client.GetRoomUser().RoomId, 0, 0, 0, string.Empty.ToString().Split(';'), false, out VehiclesOwned nVO))
                        {
                            Socket.Send("compose_vehicle|shopmsg|No se pudo autorizar el registro de papeles para tu nuevo vehículo. Inténtalo de nuevo.");
                            return;
                        }

                        #region Online DriveVars
                        //Lo conduce
                        Client.GetPlay().DrivingCar = false;
                        Client.GetPlay().DrivingInCar = true;
                        Client.GetPlay().DrivingCarId = nVO.Id;// - Ya lo establece al Crear el Vehicle en Diccionario y DB.
                        Client.GetPlay().DrivingCarItem = nVO.FurniId;

                        //Combustible System
                        Client.GetPlay().CarType = vehicle.CarType;// Define el gasto de combustible
                        Client.GetPlay().CarFuel = vehicle.MaxFuel;
                        Client.GetPlay().CarMaxFuel = vehicle.MaxFuel;
                        Client.GetPlay().CarTimer = 0;
                        Client.GetPlay().CarLife = 100;

                        Client.GetPlay().CarEnableId = vehicle.EffectID;//Coloca el enable para conducir
                        Client.GetPlay().CarEffectId = vehicle.EffectID;//Guarda el enable del último auto en conducción.
                        //Session.GetRoomUser().ApplyEffect(vehicle.EffectID); - No Pone efecto con DrivingIncar = True
                        //Session.GetRoomUser().FastWalking = true; - No FastWalking sin efecto
                        #endregion

                        if (vehicle.Price > 100)
                        {
                            Client.GetHabbo().Credits -= vehicle.Price;
                            Client.GetHabbo().UpdateCreditsBalance();
                            RoleplayManager.Shout(Client, "*Compra un " + vehicle.Model + " y paga $ " + String.Format("{0:N0}", vehicle.Price) + "*", 5);
                        }
                        else
                        {
                            Client.GetHabbo().Diamonds -= vehicle.Price;
                            Client.GetHabbo().UpdateDiamondsBalance();
                            RoleplayManager.Shout(Client, "*Compra un " + vehicle.Model + " y paga " + String.Format("{0:N0}", vehicle.Price) + " PL*", 5);
                        }

                        Client.SendWhisper("¡Tu vehículo se encuentra estacionado afuera! Sal del Establecimiento para conducirlo.", 1);
                        Socket.Send("compose_vehicle|shopmsg_green|¡Felicitaciones! Tu nuevo vehículo se encuentra afuera.");
                        Client.GetPlay().CooldownManager.CreateCooldown("buy", 1000, 15);

                        #region Tutorial Step Check
                        if (Client.GetPlay().TutorialStep == 23 && Room.BuyCarEnabled && Room.Type.Equals("public"))
                        {
                            Client.GetPlay().TutorialStep = 27;
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_tutorial|26");
                        }
                        #endregion
                        break;
                        #endregion
                    }
                #endregion

            }
        }
    }
}

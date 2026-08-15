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
using Plus.HabboRoleplay.PhoneChat;
using System.Data;
using Plus.HabboHotel.Users.Messenger;
using Plus.Utilities;
using Plus.HabboHotel.Quests;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboRoleplay.Phones;
using Plus.HabboRoleplay.PhoneOwned;
using Plus.HabboRoleplay.PhoneAppOwned;
using Plus.HabboRoleplay.PhonesApps;
using System.Web;
using Plus.Communication.Packets.Incoming.Inventory.Purse;
using Plus.HabboRoleplay.API;
using Plus.HabboHotel.RolePlay.PlayInternet;
using Plus.HabboRoleplay.TaxiRoomNodes;
using Group = Plus.HabboHotel.Groups.Group;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// TaxiWebEvent class.
    /// </summary>
    class TaxiWebEvent : IWebEvent
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
            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);


            switch (Action)
            {
                #region Open
                case "open":
                    {
                        if (Client.GetRoomUser().RoomId != Client.GetPlay().LastRoomNodeLoaded)
                            Client.GetPlay().LoadRoomNodes = false;

                        string SendData = "";
                        #region Get Room Nodes
                        if (!Client.GetPlay().LoadRoomNodes)
                        {
                            string HTML = "";

                            #region 
                            List<TaxiRoomNode> TN = TaxiRoomNodeManager.getAllTaxiRoomNodes();

                            HTML += "<div class=\"box\">";
                            HTML += "<div class=\"mt-1 uppercase tracking-wider font-bold text-center\">Trabajos & Empresas</div>";
                            HTML += "<div class=\"mt-2 -m-1 flex justify-center flex-wrap\">";

                            // Trabajos & Empresas
                            if (TN.Count > 0 && TN != null)
                            {
                                foreach (var N in TN)
                                {
                                    RoomData Room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(N.RoomId);
                                    if (Room == null || Room.JobsInside == null || (Room.JobsInside.Count() - 1) <= 0)
                                        continue;

                                    foreach (var J in Room.JobsInside)
                                    {
                                        if (!int.TryParse(J, out int nJ))
                                            continue;

                                        Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(nJ);
                                        if (Job == null)
                                            continue;

                                        HTML += "<div class=\"p-1 w-28\">";
                                        HTML += "<div class=\"bg-h-grey-2 rounded-lg p-1 flex flex-col items-center\">";
                                        HTML += "<div class=\"flex justify-between items-center relative\" data-balloon=\"" + Job.Name + "\" data-balloon-pos=\"right\"><img src=\"" + RoleplayManager.CDNSWF + "/habbo-imaging/badge/" + Job.Badge + "\" class=\"\"></div>";
                                        HTML += "<div class=\"relative z-10\">";
                                        HTML += "<div class=\"flex items-center bg-h-grey-3 hover:bg-h-grey-4 rounded-full group cursor-pointer-r\" data-id=\""+Room.Id +"\">";
                                        HTML += "<div class=\"bg-h-grey-4 group-hover:bg-h-grey-5 rounded-full px-2 py-1 flex items-center   taxi_price\">";

                                        string IdTemplate = Client.GetRoomUser().GetRoom().RoomData.TaxiNode + "," + N.NodeId;
                                        List<int> ruta = TaxiRoomNodeTemplateManager.getTaxiRoomNodeTemplate(IdTemplate).Path;
                                        int P = ruta.Count - 1;
                                        P = (P < 0) ? 0 : P * RoleplayManager.TaxiCostJobs;
                                        HTML += "<div>$ " + P + "</div>";
                                        HTML += "</div>";

                                        byte[] bytes = Encoding.Default.GetBytes(Room.Name.Split(']')[1]);
                                        HTML += "<div class=\"pl-2 pr-2 py-1 text-sm\">" + Encoding.UTF8.GetString(bytes) + "</div>";
                                        HTML += "</div>";
                                        HTML += "</div>";
                                        HTML += "</div>";
                                        HTML += "</div>";
                                    }
                                }
                            }

                            HTML += "</div>";
                            HTML += "</div>";
                            HTML += "<div class=\"mt-2 box\">";
                            HTML += "<div class=\"mt-1 uppercase tracking-wider font-bold text-center\">Otros</div>";
                            HTML += "<div class=\"mt-1 flex flex-wrap\">";

                            // Calles normales
                            if (TN.Count > 0 && TN != null)
                            {
                                foreach (var N in TN)
                                {
                                    RoomData Room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(N.RoomId);

                                    if (Room == null || (Room.JobsInside.Count() - 1) > 0)
                                        continue;

                                    HTML += "<div class=\"p-1 w-1/3\">";
                                    HTML += "<div class=\"flex items-center bg-h-grey-3 hover:bg-h-grey-4 rounded-full group cursor-pointer-r\"  data-id=\"" + Room.Id + "\">";
                                    HTML += "<div class=\"bg-h-grey-4 group-hover:bg-h-grey-5 rounded-full px-2 py-1 flex items-center taxi_price\">";

                                    string IdTemplate = Client.GetRoomUser().GetRoom().RoomData.TaxiNode + "," + N.NodeId;
                                    List<int> ruta = TaxiRoomNodeTemplateManager.getTaxiRoomNodeTemplate(IdTemplate).Path;
                                    int P = ruta.Count - 1;
                                    P = (P < 0) ? 0 : P * RoleplayManager.TaxiCostJobs;
                                    HTML += "<div>$ "+P+"</div>";
                                    HTML += "</div>";
                                    byte[] bytes = Encoding.Default.GetBytes(Room.Name.Split(']')[1]);
                                    HTML += "<div class=\"pl-2 pr-2 py-1 text-sm\">"+ Encoding.UTF8.GetString(bytes) + "</div>";
                                    HTML += "</div>";
                                    HTML += "</div>";
                                }
                            }

                            HTML += "</div>";
                            HTML += "</div>";
                            #endregion

                            SendData = "load|" + HTML;
                            Client.GetPlay().LastRoomNodeLoaded = Client.GetRoomUser().RoomId;
                            Client.GetPlay().LoadRoomNodes = true;
                        }
                        else
                            SendData = "open";
                        #endregion

                        Client.GetPlay().ViewTaxiList = true;
                        Socket.Send("compose_taxi|" + SendData);
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetPlay().ViewTaxiList = false;
                        Socket.Send("compose_taxi|close|");
                    }
                    break;
                #endregion

                #region Call Taxi
                case "calltaxi":
                    {
                        if (!Client.GetPlay().CallingTaxi)
                        {
                            if (Client.GetRoomUser() == null || !Client.GetHabbo().InRoom)
                                return;

                            #region Basic Conditions
                            if (Client.GetPlay().Cuffed)
                            {
                                Client.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                                return;
                            }
                            if (Client.GetHabbo().Escorting > 0)
							{
                                Client.SendWhisper("No puedes pedir un taxi mientras escoltas a alguien.", 1);
                                return;
                            }
                            if (Client.GetHabbo().EscortID > 0)
                            {
                                Client.SendWhisper("No puedes pedir un taxi mientras estás siendo escoltad@.", 1);
                                return;
                            }
                            if (!Client.GetRoomUser().CanWalk || Client.GetRoomUser().Frozen)
                            {
                                Client.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                                return;
                            }
                            if (Client.GetPlay().IsDead)
                            {
                                Client.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                                return;
                            }
                            if (Client.GetPlay().IsJailed)
                            {
                                Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                                return;
                            }
                            if (Client.GetPlay().IsDying)
                            {
                                Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                                return;
                            }
                            if (Client.GetPlay().DrivingCar)
                            {
                                Client.SendWhisper("¡No puedes hacer eso mientras estás conduciendo!", 1);
                                return;
                            }
                            if (Client.GetPlay().DrivingInCar)
                            {
                                Client.SendWhisper("¡No puedes hacer eso mientras tienes un vehículo en marcha afuera!", 1);
                                return;
                            }
                            if (Client.GetPlay().Pasajero)
                            {
                                Client.SendWhisper("¡No puedes hacer eso mientras estás de pasajero!", 1);
                                return;
                            }
                            #endregion

                            if (Client.GetPlay().TryGetCooldown("calltaxi"))
                                return;

                            string[] ReceivedData = Data.Split(',');

                            if (!int.TryParse(ReceivedData[1], out int RoomId))
                                return;

                            if (RoomId == Client.GetRoomUser().RoomId)
                                return;

                            RoomData Room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);

                            if (Room == null || Room.TaxiNode <= -1)
                                return;

                            Room thisRoom = Client.GetHabbo().CurrentRoom;

                            string IdTemplate = thisRoom.RoomData.TaxiNode + "," + Room.TaxiNode;
                            List<int> ruta = TaxiRoomNodeTemplateManager.getTaxiRoomNodeTemplate(IdTemplate).Path;

                            if (ruta == null || ruta.Count <= 0)
                            {
                                Client.SendWhisper("No se encontró ningún taxi disponible para llevarte a tu destino.", 1);
                                return;
                            }

                            int Price = (ruta.Count - 1) * RoleplayManager.TaxiCostJobs;
                            if (Client.GetHabbo().Credits < Price)
							{
                                Client.SendWhisper($"No cuentas con ${String.Format("0:N0", Price)} para pagar el taxi.", 1);
                                return;
                            }

                            // Timer
                            Client.GetPlay().TaxiNodeGo = Room.TaxiNode;
                            Client.GetPlay().CallingTaxi = true;
                            Client.GetPlay().LoadingTimeLeft = RoleplayManager.CallTaxiTime;

                            Client.GetRoomUser().ApplyEffect(EffectsList.Taxi);
                            RoleplayManager.Shout(Client, "*Pide un taxi para dirigirse a "+ Room.Name +"*", 5);
                            Client.SendWhisper("Debes esperar " + Client.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
                            Client.GetPlay().TimerManager.CreateTimer("general", 1000, true);
                            Client.GetPlay().CooldownManager.CreateCooldown("calltaxi", 1000, 5);
                        }

                        Client.GetPlay().ViewTaxiList = false;
                        Socket.Send("compose_taxi|close|");
                    }
                    break;
                #endregion
            }
        }
    }
}

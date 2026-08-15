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
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.GangTurfs;
using System.Drawing;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// OfferWebEvent class.
    /// </summary>
    class OfferWebEvent : IWebEvent
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
            string Type = (Data.Contains(',') ? Data.Split(',')[0] : Data);


            switch (Type)
            {
                #region BodyGard
                case "bodyguard":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Action = ReceivedData[1];

                        switch (Action)
                        {
                            #region Cubrir
                            case "cubrir":
                                {
                                    #region Initials
                                    string TargetName = ReceivedData[3];
                                    int Price = 0;

                                    if (!int.TryParse(ReceivedData[2], out Price))
                                    {
                                        Client.SendWhisper("Ingresa un precio numérico entero.", 1);
                                        return;
                                    }

                                    if (Price < 400 || Price > 1000)
                                    {
                                        Client.SendWhisper("El precio debe ser entre $400 y $1,000.", 1);
                                        return;
                                    }

                                    if (Client.GetPlay().TryGetCooldown("cubrir"))
                                        return;

                                    GameClient Target = null;
                                    RoomUser TargetUser = null;
                                    Room Room = Client.GetRoomUser().GetRoom();
                                    #endregion

                                    #region Group Conditions
                                    List<Groups.Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Client.GetHabbo().Id);

                                    if (Groups.Count <= 0)
                                    {
                                        Socket.Send("compose_bodyguard|hide_sell_button|");
                                        Client.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                                        return;
                                    }

                                    int GroupNumber = -1;

                                    if (Groups[0].GType != 2)
                                    {
                                        if (Groups.Count > 1)
                                        {
                                            if (Groups[1].GType != 2)
                                            {
                                                Socket.Send("compose_bodyguard|hide_sell_button|");
                                                Client.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                                                return;
                                            }
                                            GroupNumber = 1; // Segundo indicie de variable
                                        }
                                        else
                                        {
                                            Socket.Send("compose_bodyguard|hide_sell_button|");
                                            Client.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        GroupNumber = 0; // Primer indice de Variable Group
                                    }

                                    Client.GetPlay().JobId = Groups[GroupNumber].Id;
                                    Client.GetPlay().JobRank = Groups[GroupNumber].Members[Client.GetHabbo().Id].UserRank;
                                    #endregion

                                    #region Extra Conditions            
                                    // Existe el trabajo?
                                    if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Client.GetPlay().JobId, Client.GetPlay().JobRank))
                                    {
                                        Client.GetPlay().TimeWorked = 0;
                                        Client.GetPlay().JobId = 0; // Desempleado
                                        Client.GetPlay().JobRank = 0;

                                        //Room.Group.DeleteMember(Client.GetHabbo().Id);// OJO ACÁ

                                        Socket.Send("compose_bodyguard|hide_sell_button|");
                                        Client.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                                        return;
                                    }

                                    if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "cubrir"))
                                    {
                                        Socket.Send("compose_bodyguard|hide_sell_button|");
                                        Client.SendWhisper("Solo guardaespaldas pueden ofrecer protección.", 1);
                                        return;
                                    }
                                    #endregion

                                    #region Conditions
                                    Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetName);
                                    if (Target == null)
                                    {
                                        Client.SendWhisper("No se ha podido encontrar al usuario.", 1);
                                        return;
                                    }

                                    TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
                                    if (TargetUser == null)
                                    {
                                        Client.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en la Zona.", 1);
                                        return;
                                    }
                                    if (Target.GetPlay().PassiveMode)
                                    {
                                        Client.SendWhisper("No puedes proteger a una persona que está en modo pasivo.", 1);
                                        return;
                                    }

                                    #region Basic Conditions
                                    if (Client.GetPlay().PassiveMode)
                                    {
                                        Client.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                                        return;
                                    }
                                    if (Client.GetPlay().Cuffed)
                                    {
                                        Client.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                                        return;
                                    }
                                    if (!Client.GetRoomUser().CanWalk)
                                    {
                                        Client.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                                        return;
                                    }
                                    if (Client.GetPlay().Pasajero)
                                    {
                                        Client.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
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
                                        Client.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                                        return;
                                    }
                                    #endregion

                                    #endregion

                                    #region Execute
                                    #region Offer
                                    RoleplayManager.Shout(Client, "*Ofrece una protección a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Price) + "*", 5);
                                    Target.GetPlay().OfferManager.CreateOffer("proteccion", Client.GetHabbo().Id, Price);
                                    Target.SendWhisper("Te han ofrecido una protección por $" + String.Format("{0:N0}", Price) + ". Escribe ':aceptar proteccion' para aceptarla ó en su defecto ':rechazar proteccion'.", 1);
                                    Client.GetPlay().CooldownManager.CreateCooldown("cubrir", 1000, 15);
                                    return;
                                    #endregion
                                    #endregion
                                }
                                break;
                            #endregion

                            default:
                                break;
                        }
                    }
                    break;
                #endregion
            }
        }
    }
}
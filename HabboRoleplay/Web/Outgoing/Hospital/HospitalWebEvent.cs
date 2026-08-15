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
    /// HospitalWebEvent class.
    /// </summary>
    class HospitalWebEvent : IWebEvent
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
                #region Open Botiquin
                case "open_botiq":
                    {
                        #region Variables
                        //Item Item = null;
                        RoomUser User = Client.GetRoomUser();
                        Room Room = User.GetRoom();
                        #endregion

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "botiquin"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para ver el botiquín!", 1);
                            return;
                        }
                        string MyCity = Room.City;
                        int HospID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out PlayRoom mData);//hospital de la cd.
                        if (Client.GetHabbo().CurrentRoomId != HospID)
                        {
                            Client.SendWhisper("No puedes hacer eso fuera del Hospital.", 1);
                            return;
                        }
                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes comer mientras estás muert@!", 1);
                            return;
                        }

                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes comer mientras estás encarcelad@!", 1);
                            return;
                        }

                        if (Client.GetPlay().Cuffed)
                        {
                            Client.SendWhisper("¡No puedes comer mientras estás esposad@!", 1);
                            return;
                        }

                        /* Se omite validación del furni botiquín considerando que el comodín que activa este evento estará colocado convenientemente frente a uno.
                        foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                        {
                            if (item.GetX == User.SquareInFront.X && item.GetY == User.SquareInFront.Y)
                            {
                                if (item.BaseItem == 10280 || item.BaseItem == 10281)// ID del Botiquín
                                {
                                    Item = item;
                                }
                            }
                        }

                        if (Item == null)
                        {
                            Client.SendWhisper("No hay ningún botiquín en frente de ti.", 1);
                            return;
                        }
                        */

                        if (Client.GetPlay().TryGetCooldown("botiq", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        Client.GetPlay().ViewHospBotiq = true;
                        Socket.Send("compose_hospital|open_botiq|");
                    }
                    break;
                #endregion

                #region Close Botiquin
                case "close_botiq":
                    {
                        Client.GetPlay().ViewHospBotiq = false;
                        Socket.Send("compose_hospital|close_botiq|");
                    }
                    break;
                #endregion

                #region Use Botiquin
                case "use_botiq":
                    {
                        int heri;
                        string[] ReceivedData = Data.Split(',');
                        if (!int.TryParse(ReceivedData[1], out heri))
                            return;

                        #region Variables
                        //Item Item = null;
                        RoomUser User = Client.GetRoomUser();
                        Room Room = User.GetRoom();
                        #endregion

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "botiquin"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                            return;
                        }
                        string MyCity = Room.City;
                        int HospID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out PlayRoom mData);//hospital de la cd.
                        if (Client.GetHabbo().CurrentRoomId != HospID)
                        {
                            Client.SendWhisper("No puedes hacer eso fuera del Hospital.", 1);
                            return;
                        }
                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes comer mientras estás muert@!", 1);
                            return;
                        }

                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes comer mientras estás encarcelad@!", 1);
                            return;
                        }

                        if (Client.GetPlay().Cuffed)
                        {
                            Client.SendWhisper("¡No puedes comer mientras estás esposad@!", 1);
                            return;
                        }

                        if (!Client.GetPlay().ViewHospBotiq)
                        {
                            Client.SendWhisper("No hay ningún botiquín en frente de ti.", 1);
                            return;
                        }

                        if (Client.GetPlay().TryGetCooldown("botiq", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        #region Heridas
                        if (heri <= 0 || heri > 10)
                        {
                            Client.SendWhisper("Debes seleccionar un remedio del listado.", 1);
                            return;
                        }
                        if (heri == 1)
                        {
                            Client.GetPlay().BotiquinName = "Pinzas, vendas, jeringa con anestesia";
                            Client.GetPlay().BotiquinDoc = 1;
                        }
                        else if (heri == 2)
                        {
                            Client.GetPlay().BotiquinName = "Pinzas, vendas, jeringa con morfina";
                            Client.GetPlay().BotiquinDoc = 2;
                        }
                        else if (heri == 3)
                        {
                            Client.GetPlay().BotiquinName = "Antiinflamatorios y yeso";
                            Client.GetPlay().BotiquinDoc = 3;
                        }
                        else if (heri == 4)
                        {
                            Client.GetPlay().BotiquinName = "Antiinflamatorios";
                            Client.GetPlay().BotiquinDoc = 4;
                        }
                        else if (heri == 5)
                        {
                            Client.GetPlay().BotiquinName = "Hilo, aguja, vendas, y suero fisiológico";
                            Client.GetPlay().BotiquinDoc = 5;
                        }
                        else if (heri == 6)
                        {
                            Client.GetPlay().BotiquinName = "Antiinflamatorios y hielo";
                            Client.GetPlay().BotiquinDoc = 6;
                        }
                        else if (heri == 7)
                        {
                            Client.GetPlay().BotiquinName = "Antiinflamatorios hielo y yeso es";
                            Client.GetPlay().BotiquinDoc = 7;
                        }
                        else if (heri == 8)
                        {
                            Client.GetPlay().BotiquinName = "Bisturí, escalpelo, hilo, aguja y jeringa con morfina";
                            Client.GetPlay().BotiquinDoc = 8;
                        }
                        else if (heri == 9)
                        {
                            Client.GetPlay().BotiquinName = "Hielo, vendas y jeringa con morfina";
                            Client.GetPlay().BotiquinDoc = 9;
                        }
                        else if (heri == 10)
                        {
                            Client.GetPlay().BotiquinName = "Yeso, vendas, morfina y antiinflamatorios";
                            Client.GetPlay().BotiquinDoc = 10;
                        }
                        else
                        {
                            Client.GetPlay().BotiquinName = "Pinzas, vendas, jeringa con anestesia";
                            Client.GetPlay().BotiquinDoc = 1;
                        }
                        #endregion

                        RoleplayManager.Shout(Client, "*Toma un remedio del botiquín y corre a atender su paciente*", 5);
                        Client.SendWhisper("Tomas " + Client.GetPlay().BotiquinName + " del botiquín.", 1);

                        Client.GetPlay().ViewHospBotiq = false;
                        Socket.Send("compose_hospital|use_botiq|");

                        Client.GetPlay().CooldownManager.CreateCooldown("botiq", 1000, 3);
                        #endregion
                    }
                    break;
                #endregion

                #region Revisar
                case "revisar":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Target = ReceivedData[1];

                        Room Room = Client.GetRoomUser().GetRoom();

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "reviewhosp"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                            return;
                        }
                        string MyCity = Room.City;
                        int HospID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out PlayRoom mData);//hospital de la cd.
                        if (Client.GetHabbo().CurrentRoomId != HospID)
                        {
                            Client.SendWhisper("No puedes hacer eso fuera del Hospital.", 1);
                            return;
                        }
                        if (string.IsNullOrEmpty(Target))
                        {
                            Client.SendWhisper("No se encontró a la persona para realizar esa acción.", 1);
                            return;
                        }
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Target);
                        if (TargetClient == null)
                        {
                            Client.SendWhisper("((Ha ocurrido un error al buscar a la persona, probablemente esté desconectada))", 1);
                            return;
                        }
                        RoomUser RoomUser = Client.GetRoomUser();
                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
                        if (TargetUser == null)
                        {
                            Client.SendWhisper("((Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona))", 1);
                            return;
                        }
                        if (TargetClient.GetHabbo().Id == Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                            return;
                        }
                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }
                        if (!TargetClient.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡Esa persona no se encuentra herida!", 1);
                            return;
                        }
                        if (TargetUser.IsAsleep)
                        {
                            Client.SendWhisper("¡No puedes revisar a un usuario ausente!", 1);
                            return;
                        }
                        if (Client.GetPlay().TryGetCooldown("reviewpatient", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
                        Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
                        double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                        if (Distance <= 2)
                        {
                            Random Random = new Random();

                            int Chance = Random.Next(1, 101);

                            if (Chance <= 8 && Client.GetPlay().JobRank <= 1)
                            {
                                RoleplayManager.Shout(Client, "*Revisa a " + TargetClient.GetHabbo().Username + " pero no ha podido decifrar qué problema tiene*", 5);
                                return;
                            }
                            else
                            {
                                bool NeedHelp = TargetClient.GetPlay().DeadTimeLeft > 0;

                                if (!NeedHelp || TargetClient.GetPlay().Revisado == true)
                                {
                                    RoleplayManager.Shout(Client, "*Revisa a " + TargetClient.GetHabbo().Username + " pero nota que no necesita más ayuda*", 5);
                                    return;
                                }
                                else// HERE
                                {
                                    TargetClient.GetPlay().Revisado = true;
                                    Client.GetPlay().RevisPaci = TargetClient.GetHabbo().Id;
                                    RoleplayManager.Shout(Client, "*Revisa al paciente " + TargetClient.GetHabbo().Username + " para atenderlo*", 5);
                                    Client.SendWhisper("Esta persona presenta " + TargetClient.GetPlay().HeridaName + ", ve a buscar al botiquin lo que necesites para el tratamiento.", 1);
                                    Client.GetPlay().CooldownManager.CreateCooldown("reviewpatient", 1000, 10);

                                    // Stats WebSocket
                                    Client.GetPlay().OpenUsersDialogue(TargetClient);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Client.SendWhisper("Debes estar más cerca de la persona a revisar.", 1);
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Atender
                case "atender":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Target = ReceivedData[1];

                        Room Room = Client.GetRoomUser().GetRoom();

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "reviewhosp"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                            return;
                        }
                        string MyCity = Room.City;
                        int HospID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out PlayRoom mData);//hospital de la cd.
                        if (Client.GetHabbo().CurrentRoomId != HospID)
                        {
                            Client.SendWhisper("No puedes hacer eso fuera del Hospital.", 1);
                            return;
                        }
                        if (string.IsNullOrEmpty(Target))
                        {
                            Client.SendWhisper("No se encontró a la persona para realizar esa acción.", 1);
                            return;
                        }
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Target);
                        if (TargetClient == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                            return;
                        }

                        RoomUser RoomUser = Client.GetRoomUser();
                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
                        if (TargetUser == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                            return;
                        }
                        if (TargetClient.GetHabbo().Id == Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                            return;
                        }

                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }

                        if (!TargetClient.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡Esa persona no se encuentra herida!", 1);
                            return;
                        }
                        if (TargetUser.IsAsleep)
                        {
                            Client.SendWhisper("¡No puedes atender a un usuario ausente!", 1);
                            return;
                        }
                        if (Client.GetPlay().RevisPaci != TargetUser.GetClient().GetHabbo().Id)
                        {
                            Client.SendWhisper("¡Primero debes :revisarpaciente [nombre] para saber qué operación hacer!", 1);
                            return;
                        }
                        if (Client.GetPlay().BotiquinDoc <= 0)
                        {
                            Client.SendWhisper("¡Primero debes sacar un remedio del botiquín para comenzar el tratamiento!", 1);
                            return;
                        }
                        if (Client.GetPlay().TryGetCooldown("reviewpatient", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
                        Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
                        double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                        if (Distance <= 2)
                        {
                            if (Client.GetPlay().BotiquinDoc == TargetClient.GetPlay().HeridaPaci)
                            {
                                Client.SendWhisper("¡Bien hecho! Ganas $ " + RoleplayManager.PayRightJob + " por salvar a este paciente", 1);
                                Client.GetHabbo().Credits += RoleplayManager.PayRightJob;//8
                                RoleplayManager.UpdateCreditsBalance(Client);
                                Client.GetPlay().MoneyEarned += RoleplayManager.PayRightJob;
                            }
                            else
                            {
                                Client.SendWhisper("Ganas solo $ " + RoleplayManager.PayWrongJob + " por equivocarte salvando a este paciente", 1);
                                Client.GetHabbo().Credits += RoleplayManager.PayWrongJob;//6
                                RoleplayManager.UpdateCreditsBalance(Client);
                                Client.GetPlay().MoneyEarned += RoleplayManager.PayWrongJob;
                            }
                            TargetClient.GetPlay().HeridaName = "";
                            TargetClient.GetPlay().HeridaPaci = 0;
                            TargetClient.GetPlay().Revisado = false;


                            Client.GetPlay().BotiquinDoc = 0;
                            Client.GetPlay().BotiquinName = "";
                            Client.GetPlay().RevisPaci = 0;
                            RoleplayManager.Shout(Client, "*Comienza a curar las heridas de " + TargetClient.GetHabbo().Username + "*", 5);
                            TargetClient.GetRoomUser().ApplyEffect(0);
                            TargetClient.GetPlay().BeingHealed = true;
                            TargetClient.GetPlay().TimerManager.CreateTimer("heal", 1000, false);

                            // Stats WebSocket
                            Client.GetPlay().OpenUsersDialogue(TargetClient);
                            return;
                        }
                        else
                        {
                            Client.SendWhisper("Debes estar más cerca de la persona a atender.", 1);
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Reanimar
                case "reanimar":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Target = ReceivedData[1];

                        Room Room = Client.GetRoomUser().GetRoom();

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "reviewhosp"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                            return;
                        }
                        if (RoleplayManager.PurgeEvent)
                        {
                            Client.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                            return;
                        }
                        if (string.IsNullOrEmpty(Target))
                        {
                            Client.SendWhisper("No se encontró a la persona para realizar esa acción.", 1);
                            return;
                        }
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Target);
                        if (TargetClient == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                            return;
                        }

                        RoomUser RoomUser = Client.GetRoomUser();
                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
                        if (TargetUser == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                            return;
                        }
                        if (TargetClient.GetHabbo().Id == Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                            return;
                        }

                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }

                        if (!TargetClient.GetPlay().IsDying)
                        {
                            Client.SendWhisper("¡Esa persona no se encuentra inconsciente!", 1);
                            return;
                        }
                        if (TargetUser.IsAsleep)
                        {
                            Client.SendWhisper("¡No puedes reanimar a un usuario ausente!", 1);
                            return;
                        }
                        if (Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás mert@!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                            return;
                        }
                        if (TargetClient.GetPlay().PediMedico == false)
                        {
                            Client.SendWhisper("¡Esta persona no ha pedido una Ambulancia!", 1);
                            return;
                        }
                        if (TargetClient.GetPlay().TargetReanim == true)
                        {
                            Client.SendWhisper("¡Esta persona ya ha sido reanimada!", 1);
                            return;
                        }
                        if (Client.GetPlay().TryGetCooldown("reanim", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
                        Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
                        double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                        if (Distance <= 2)
                        {
                            RoleplayManager.Shout(Client, "*Atiende a " + TargetClient.GetHabbo().Username + " logrando reanimarlo*", 5);
                            Client.SendWhisper("¡Bien hecho! Ahora sube al paciente a tu ambulancia y trasladalo al Hospital para salvar su vida y recibir tu pago.", 1);
                            TargetClient.GetPlay().TargetReanim = true;// Paciente reanimado SÍ
                            if (TargetClient.GetPlay().PediMedico)
                                TargetClient.GetPlay().PediMedico = false;

                            TargetClient.GetPlay().CooldownManager.CreateCooldown("reanim", 1000, 5);

                            // Stats WebSocket
                            Client.GetPlay().OpenUsersDialogue(TargetClient);
                            return;
                        }
                        else
                        {
                            Client.SendWhisper("Debes estar más cerca de la persona a reanimar.", 1);
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Subir
                case "subir":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Target = ReceivedData[1];

                        Room Room = Client.GetRoomUser().GetRoom();

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "reviewhosp"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                            return;
                        }
                        if (RoleplayManager.PurgeEvent)
                        {
                            Client.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                            return;
                        }
                        if (string.IsNullOrEmpty(Target))
                        {
                            Client.SendWhisper("No se encontró a la persona para realizar esa acción.", 1);
                            return;
                        }
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Target);
                        if (TargetClient == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                            return;
                        }

                        RoomUser RoomUser = Client.GetRoomUser();
                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
                        if (TargetUser == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                            return;
                        }
                        if (TargetClient.GetHabbo().Id == Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                            return;
                        }
                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }
                        if (!Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("Debes estar conduciendo tu ambulancia.", 1);
                            return;
                        }

                        #region Get Information form VehiclesManager
                        Vehicle vehicle = null;
                        int corp = 0;
                        foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                        {
                            if (Client.GetPlay().CarEffectId == Vehicle.EffectID)
                            {
                                vehicle = Vehicle;
                                corp = Convert.ToInt32(Vehicle.CarCorp);
                            }
                        }
                        if (vehicle == null)
                        {
                            Client.SendWhisper("¡Ha ocurrido un error al buscar los datos del vehículo que conduces!", 1);
                            return;
                        }
                        #endregion

                        if (corp <= 0 || !PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Hospital"))
                        {
                            Client.SendWhisper("Debes estar conduciendo una ambulancia.", 1);
                            return;
                        }
                        if (!TargetClient.GetPlay().IsDying)
                        {
                            Client.SendWhisper("¡Esa persona no se encuentra inconsciente!", 1);
                            return;
                        }
                        if (TargetUser.IsAsleep)
                        {
                            Client.SendWhisper("¡No puedes hacerle eso a un usuario ausente!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás mert@!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                            return;
                        }
                        if (TargetClient.GetPlay().TargetReanim == false)
                        {
                            Client.SendWhisper("¡Primero debes reanimar a la persona!");
                            return;
                        }
                        if (Client.GetPlay().PasajerosCount >= vehicle.MaxDoors)
                        {
                            Client.SendWhisper("No hay espacio suficiente para un pasajero en tu Ambulancia.", 1);
                            return;
                        }
                        if (Client.GetPlay().TryGetCooldown("uppatient", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
                        Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
                        double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                        if (Distance <= 2)
                        {
                            RoleplayManager.Shout(Client, "*Sube a " + TargetClient.GetHabbo().Username + " a su Ambulancia*", 5);
                            Client.SendWhisper("¡Bien hecho! Ahora traslada al paciente al Hospital y sálvalo para recibir tu pago.", 1);
                            Client.GetPlay().HospReanim = TargetClient.GetHabbo().Id;

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
                            TargetClient.GetPlay().ChoferName = Client.GetHabbo().Username;
                            TargetClient.GetPlay().ChoferID = Client.GetHabbo().Id;
                            TargetClient.GetRoomUser().CanWalk = false;
                            TargetClient.GetRoomUser().FastWalking = true;
                            TargetClient.GetRoomUser().TeleportEnabled = true;
                            TargetClient.GetRoomUser().AllowOverride = true;

                            // MEDICO (YO) 
                            Client.GetPlay().Chofer = true;
                            Client.GetPlay().Pasajeros += TargetClient.GetHabbo().Username + ";";
                            Client.GetPlay().PasajerosCount++;
                            Client.GetRoomUser().FastWalking = true;
                            Client.GetRoomUser().AllowOverride = true;

                            //Animación de subir al Auto
                            int NewX = Client.GetRoomUser().X;
                            int NewY = Client.GetRoomUser().Y;
                            Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(TargetClient.GetRoomUser(), new Point(NewX, NewY), 0, Room.GetGameMap().SqAbsoluteHeight(NewX, NewY)));
                            TargetClient.GetRoomUser().MoveTo(NewX, NewY);
                            #endregion

                            TargetClient.GetPlay().TimerManager.CreateTimer("uppatient", 1000, false);

                            // Stats WebSocket
                            Client.GetPlay().OpenUsersDialogue(TargetClient);
                            return;
                        }
                        else
                        {
                            Client.SendWhisper("Debes estar más cerca de la persona a reanimar.", 1);
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Salvar
                case "salvar":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Target = ReceivedData[1];

                        Room Room = Client.GetRoomUser().GetRoom();

                        #region Conditions
                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "reviewhosp"))
                        {
                            Client.SendWhisper("¡Debes trabajar de Médico para hacer eso!", 1);
                            return;
                        }
                        if (RoleplayManager.PurgeEvent)
                        {
                            Client.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                            return;
                        }
                        if (string.IsNullOrEmpty(Target))
                        {
                            Client.SendWhisper("No se encontró a la persona para realizar esa acción.", 1);
                            return;
                        }
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Target);
                        if (TargetClient == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                            return;
                        }

                        RoomUser RoomUser = Client.GetRoomUser();
                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
                        if (TargetUser == null)
                        {
                            Client.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                            return;
                        }
                        if (TargetClient.GetHabbo().Id == Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                            return;
                        }
                        if (!Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                            return;
                        }
                        if (!Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("Debes estar conduciendo tu ambulancia.", 1);
                            return;
                        }
                        #region Get Information form VehiclesManager
                        Vehicle vehicle = null;
                        int corp = 0;
                        foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                        {
                            if (Client.GetPlay().CarEffectId == Vehicle.EffectID)
                            {
                                vehicle = Vehicle;
                                corp = Convert.ToInt32(Vehicle.CarCorp);
                            }
                        }
                        if (vehicle == null)
                        {
                            Client.SendWhisper("¡Ha ocurrido un error al buscar los datos del vehículo que conduces!", 1);
                            return;
                        }
                        #endregion

                        if (corp <= 0 || !PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Hospital"))
                        {
                            Client.SendWhisper("Debes estar conduciendo una ambulancia.", 1);
                            return;
                        }
                        if (!TargetClient.GetPlay().IsDying)
                        {
                            Client.SendWhisper("¡Esa persona no se encuentra inconsciente!", 1);
                            return;
                        }
                        if (TargetUser.IsAsleep)
                        {
                            Client.SendWhisper("¡No puedes hacerle eso a un usuario ausente!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás mert@!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                            return;
                        }
                        if (TargetClient.GetPlay().TargetReanim == false)
                        {
                            Client.SendWhisper("¡Primero debes reanimar a la persona!");
                            return;
                        }
                        if (TargetClient.GetHabbo().Id != Client.GetPlay().HospReanim)
                        {
                            Client.SendWhisper("¡Esa persona no está dentro de tu ambulancia!");
                            return;
                        }
                        if (Client.GetPlay().TryGetCooldown("savepatient", true))
                        {
                            Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                            return;
                        }
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes acercarte a la entrada del Hospital para salvar al Herido.", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
                        Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
                        double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                        if (Distance <= 1)
                        {
                            int Cost = RoleplayManager.AmbulSave;
                            RoleplayManager.Shout(Client, "*Salva la vida de " + TargetClient.GetHabbo().Username + "*", 5);
                            Client.SendWhisper("Ganaste $" + Cost + " por salvar la vida de " + TargetClient.GetHabbo().Username, 1);
                            Client.GetHabbo().Credits += Cost;
                            RoleplayManager.UpdateCreditsBalance(Client);
                            Client.GetPlay().MoneyEarned += Cost;
                            Client.GetPlay().HospReanim = 0;

                            if (TargetClient.GetPlay().PediMedico)
                                TargetClient.GetPlay().PediMedico = false;
                            TargetClient.GetPlay().TargetReanim = false;
                            TargetClient.GetPlay().CurHealth = TargetClient.GetPlay().MaxHealth;
                            TargetClient.GetPlay().RefreshStatDialogue();
                            TargetClient.GetRoomUser().Frozen = false;
                            TargetClient.GetPlay().IsDying = false;
                            TargetClient.GetPlay().DyingTimeLeft = 0;
                            TargetClient.GetPlay().InState = false;

                            // Bajarlo de la Ambulancia
                            #region Bajar al Herido
                            // PASAJERO
                            TargetClient.GetPlay().Pasajero = false;
                            TargetClient.GetPlay().ChoferName = "";
                            TargetClient.GetPlay().ChoferID = 0;
                            TargetClient.GetRoomUser().CanWalk = true;
                            TargetClient.GetRoomUser().FastWalking = false;
                            TargetClient.GetRoomUser().TeleportEnabled = false;
                            TargetClient.GetRoomUser().AllowOverride = false;

                            // Descontamos Pasajero
                            Client.GetPlay().PasajerosCount--;
                            if (Client.GetPlay().PasajerosCount <= 0)
                                Client.GetPlay().Pasajeros = "";
                            else
                            {
                                StringBuilder builder = new StringBuilder(Client.GetPlay().Pasajeros);
                                builder.Replace(TargetClient.GetHabbo().Username + ";", "");
                                Client.GetPlay().Pasajeros = builder.ToString();
                            }

                            // CHOFER 
                            Client.GetPlay().Chofer = (Client.GetPlay().PasajerosCount <= 0) ? false : true;
                            Client.GetRoomUser().AllowOverride = (Client.GetPlay().PasajerosCount <= 0) ? false : true;
                            #endregion

                            TargetClient.SendWhisper("¡Has sido atendid@ por el servicio de Ambulancia y has sido revivid@! Pagas: $" + Cost, 1);
                            if ((TargetClient.GetHabbo().Credits - Cost) >= 0)
                            {
                                TargetClient.GetHabbo().Credits -= Cost;
                                RoleplayManager.UpdateCreditsBalance(TargetClient);
                            }
                            else
                            {
                                TargetClient.GetPlay().Bank -= Cost;
                                RoleplayManager.UpdateBankBalance(TargetClient);
                                TargetClient.SendWhisper("Se te ha cobrado directamente a tu cuenta bancaria debido a que no tienes dinero suciente en tu Cartera.", 1);

                            }
                            Client.GetPlay().CooldownManager.CreateCooldown("savepatient", 1000, 10);

                            // Stats WebSocket
                            Client.GetPlay().OpenUsersDialogue(TargetClient);
                            return;
                        }
                        else
                        {
                            Client.SendWhisper("El pasajero no se encuentra cerca de ti.", 1);
                        }
                        #endregion
                    }
                    break;
                #endregion
            }
        }
    }
}
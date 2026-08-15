using Plus.Communication.Packets.Outgoing.Campaigns;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Rooms.Chat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using System.Drawing;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.Core;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.Utilities;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rewards.Rooms.Chat.Commands.Administrator
{
    class TestEventCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_test"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Comando para pruebas."; }
        }

        public void Execute(GameClients.GameClient Session, Plus.HabboHotel.Rooms.Room Room, string[] Params)
        {
            Session.SendWhisper("Ejecutando comando Test...");



            #region Taxi Bot
            if (true) {
                #region Prepare
                RoleplayManager.TaxiBotsId++;
                List<RandomSpeech> BotSpeechList = new List<RandomSpeech>();

                RoomUser BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(
                    RoleplayManager.TaxiBotsId,
                    Session.GetHabbo().CurrentRoomId,
                    "taxibot",
                    "stand",
                    "Taxi#" + Session.GetHabbo().Username,
                    "",
                    "hr-3020-34.hd-3091-2.ch-225-92.lg-3058-100.sh-3089-1338.ca-3084-78-108.wa-2005",
                    Session.GetRoomUser().X,
                    Session.GetRoomUser().Y,
                    Session.GetRoomUser().Z,
                    Session.GetRoomUser().RotBody,
                    0,
                    0,
                    0,
                    0,
                    ref BotSpeechList,
                    "",
                    0,
                    0,
                    false,
                    30,
                    false,
                    2,
                    Room.RoomData.TaxiNode),
                    null);

                List<int> ruta = PlusEnvironment.GetGame().GetDijkstra().RunDijkstra(Room.RoomData.TaxiNode, 18);
                string r = "";
                foreach (int posicion in ruta)
                    r += "["+posicion+"]" + "->";

                Session.GetHabbo().TaxiPath = ruta;
                BotUser.Chat("La ruta a seguir para llegar al centro es: " + r, false, 0);

                Room.GetGameMap().UpdateUserMovement(new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y), new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y), BotUser);
                #endregion

                #region Stand
                if (Session.GetRoomUser().isSitting)
                {
                    Session.GetRoomUser().Statusses.Remove("sit");
                    Session.GetRoomUser().Z += 0.35;
                    Session.GetRoomUser().isSitting = false;
                    Session.GetRoomUser().UpdateNeeded = true;
                }
                else if (Session.GetRoomUser().isLying)
                {
                    Session.GetRoomUser().Statusses.Remove("lay");
                    Session.GetRoomUser().Z += 0.35;
                    Session.GetRoomUser().isLying = false;
                    Session.GetRoomUser().UpdateNeeded = true;
                }
                #endregion

                #region Execute
                var This = Session.GetHabbo();
                var User = BotUser;

                This.GetClient().GetRoomUser().SetRot(Rotation.Calculate(This.GetClient().GetRoomUser().X, This.GetClient().GetRoomUser().Y, User.X, User.Y), false);
                int dis = Math.Abs(User.X - This.GetClient().GetRoomUser().X) + Math.Abs(User.Y - This.GetClient().GetRoomUser().Y);
                {

                    if (This.GetClient().GetRoomUser() != User)
                        User.SetRot(Rotation.Calculate(This.GetClient().GetRoomUser().X, This.GetClient().GetRoomUser().Y, User.X, User.Y), false);

                    if (User.Statusses.Count > 0)
                        User.Statusses.Clear();

                    //User.ClearMovement(true);
                    //This.GetClient().GetRoomUser().ClearMovement(true);
                    //User.GetClient().GetHabbo().TaxiPassenger = This.Id;
                    This.TaxiChofer = User.BotData.BotId;

                    //This.GetClient().GetRoomUser().UpdateNeeded = true;
                    //User.UpdateNeeded = true;
                    User.ApplyEffect(EffectsList.TaxiChofer);
                    User.FastWalking = true;
                    This.GetClient().GetRoomUser().ApplyEffect(EffectsList.TaxiPasajero);
                    This.GetClient().GetRoomUser().CanWalk = false;
                    This.GetClient().GetRoomUser().FastWalking = true;
                    RoleplayManager.Shout(Session, "*Pide un Taxi para dirigirse a [CALLE] El Centro*", 5);
                    Session.GetPlay().CooldownManager.CreateCooldown("calltaxi", 1000, 5);
                }
                #endregion
            }
            #endregion


            //PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_feedcomposer", "alert|" + Session.GetHabbo().Username + "|Tester|" + "acribilló a");

            //PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_feedcomposer", "sound|punch");

            //RoleplayManager.GetRandomObject(Session);
            /*List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getAllVehiclesOwned();

            #region Execute
            string str = "";
            str += "\n============================================\n                  Listado de TODOS LOS Vehículos \n============================================\n";
            if (VO == null || VO.Count <= 0)
            {
                str += "No hay nada en el diccionario VehiclesOwned";
            }
            else
            {
                foreach (VehiclesOwned _vo in VO)
                {
                    if (_vo.Id < 10000000)
                        continue;

                    str += "ID: " + _vo.Id;
                    str += "\nFurniId: " + _vo.FurniId;
                    str += "\nItemId: " + _vo.ItemId;
                    str += "\nOwnerId: " + _vo.OwnerId;
                    str += "\nLastUserId: " + _vo.LastUserId;
                    str += "\nModel: " + _vo.Model;
                    str += "\nFuel: " + _vo.Fuel;
                    str += "\nKm: " + _vo.Km;
                    str += "\nState: " + _vo.State;  //(ENUM) 0 = Abierto| 1 = Con Traba | 2 = No traba y Averid. | 3 = Con traba y Averid.
                    str += "\nTraba: " + _vo.Traba;
                    str += "\nAlarm: " + _vo.Alarm;
                    str += "\nLocation: " + _vo.Location;
                    str += "\nX: " + _vo.X;
                    str += "\nY: " + _vo.Y;
                    str += "\nZ: " + _vo.Z;
                    str += "\nBaul: " + _vo.Baul;
                    str += "\nBaul: " + _vo.Baul;
                    str += "\nCarLife: " + _vo.CarLife;
                    str += "\nCamCargId: " + _vo.CamCargId;
                    str += "\nCamState: " + _vo.CamState;// 0: Sin carga | 1: Cargado | 2: Entregado
                    str += "\nCamDest: " + _vo.CamDest;
                    str += "\nCamOwnId: " + _vo.CamOwnId;
                    str += "\n------------------------------------\n\n";
                }
            }
            //Session.SendNotifWithScroll(str);
            Logging.LogPacketException("LISTA DE VEHICULOS ["+DateTime.Now+"]", str);
            #endregion

            Session.GetPlay().Weed += 10;
            Session.GetPlay().Cocaine += 20;
            Session.GetPlay().Medicines += 30;
            Session.GetPlay().Bidon += 40;
            Session.GetPlay().MecParts += 50;
            Session.GetPlay().ArmMat += 60;
            Session.GetPlay().ArmPieces += 70;
            */
            Session.SendWhisper("Comando Test ejecutado.");
        }
    }
}

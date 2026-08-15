using System;
using System.Linq;
using System.Drawing;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.HabboRoleplay.TaxiNodes;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.AI.Types
{
    public class TaxiBot : BotAI
    {
        private bool Finish;
        Point ValidSquare;

        public TaxiBot(int VirtualId)
        {
            Finish = false;
            ValidSquare = new Point(-1, -1);
        }


        public override void OnSelfEnterRoom()
        {
        }

        public override void OnSelfLeaveRoom(bool Kicked)
        {
        }

        public override void OnUserEnterRoom(RoomUser User)
        {
        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
        }

        public override void OnTimerTick()
        {
            if (GetRoom() == null || GetRoomUser() == null)
                return;

            string PassengerName = GetRoomUser().BotData.Name.Split('#')[1];
            GameClient PassengerClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PassengerName);

            if (PassengerClient == null)
                return;

            RoomUser Passenger = PassengerClient.GetRoomUser();

            if (Passenger == null || Passenger.GetClient() == null)
                return;

            List<int> ruta = Passenger.GetClient().GetHabbo().TaxiPath;

            if (ruta == null)
                return;

            if (GetRoomUser().BotData.LastNode != GetRoom().RoomData.TaxiNode)
            {
                GetRoomUser().BotData.LastNode = GetRoom().RoomData.TaxiNode;
            }

            if ((Passenger.GetClient().GetPlay().TaxiLastIndex) >= ruta.Count)
            {
                Finish = true;
            }
            
            if (Finish)
            {
                // Caminamos en frente a la parada de Taxi
                Item TaxiStop = GetRoom().GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "bump_signs");

                if (TaxiStop != null)
                {
                    if(ValidSquare.X < 0 || ValidSquare.Y < 0)
                    {
                        ValidSquare = TaxiStop.SquareInFront;
                    }

                    if (!Gamemap.TilesTouching(ValidSquare.X, ValidSquare.Y, GetRoomUser().Coordinate.X, GetRoomUser().Coordinate.Y))
                    {
                        GetRoomUser().MoveTo(ValidSquare);
                        Passenger.MoveTo(ValidSquare);

                        if (!Passenger.IsWalking)
                        {
                            ValidSquare = TaxiStop.SquareBehind;
                        }
                        if (!Passenger.IsWalking)
                        {
                            ValidSquare = TaxiStop.SquareLeft;
                        }
                        if (!Passenger.IsWalking)
                        {
                            ValidSquare = TaxiStop.SquareRight;
                        }
                    }
                    else
                    {
                        // Bot llega al destino
                        GetRoomUser().CanWalk = false;

                        if (!Passenger.IsWalking)
                        {
                            Passenger.ApplyEffect(0);
                            Passenger.GetClient().GetHabbo().TaxiChofer = 0;
                            Passenger.FastWalking = false;
                            Passenger.CanWalk = true;

                            Passenger.GetClient().GetHabbo().TaxiPath = null;
                            Passenger.GetClient().GetPlay().TaxiNodeGo = -1;

                            // Remove bot from room
                            GetRoom().GetGameMap().RemoveUserFromMap(GetRoomUser(), new Point(GetRoomUser().X, GetRoomUser().Y));
                            GetRoom().GetRoomUserManager().RemoveBot(GetRoomUser().VirtualId, false);
                        }
                    }
                }
            }
            else
            {
                // Caminamos hacia la flecha port
                int NextNode = ruta[Passenger.GetClient().GetPlay().TaxiLastIndex];
                List<TaxiNode> TN = TaxiNodeManager.getTaxiNodeByFromTo(GetRoom().RoomData.TaxiNode, NextNode);
                if (TN.Count > 0 && TN != null)
                {
                    GetRoomUser().MoveTo(TN[0].X, TN[0].Y);
                    Passenger.MoveTo(TN[0].X, TN[0].Y);
                }
            }
        }

        #region Commands
        public override void OnUserSay(RoomUser User, string Message)
        {
            
        }
        #endregion
    }
}
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using System.Drawing;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class MoveAvatarEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || !User.CanWalk)
                return;

            int MoveX = Packet.PopInt();
            int MoveY = Packet.PopInt();

            if (MoveX == User.X && MoveY == User.Y)
                return;

            #region WS Overlays RP
            #region ATM
            if (Session.GetPlay().UsingAtm)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_atm", "close");
            }

            if (Session.GetPlay().IsRobATM)
                Session.GetPlay().IsRobATM = false;

            #endregion
            #region Houses
            if (Session.GetPlay().ViewHouse)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_house", "close");
                Session.SendWhisper("Has dejado de mirar la Información de la Casa.", 1);
            }
            #region Camionero
            if (Session.GetPlay().ViewCamCargas)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_camionero", "close");
                Session.SendWhisper("Has dejado de mirar los cargamentos.", 1);
            }
            #endregion

            #region Armero
            if (Session.GetPlay().ViewArmeroPieces)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_armero", "close_pieces");
                Session.SendWhisper("Has dejado de mirar el creador de piezas.", 1);
            }
            if (Session.GetPlay().ViewArmeroWeapons)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_armero", "close_weapons");
                Session.SendWhisper("Has dejado de mirar el creador de armas.", 1);
            }
            #endregion
            #region Hospital
            if (Session.GetPlay().ViewHospBotiq)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_hospital", "close_botiq");
                Session.SendWhisper("Has dejado de mirar el botiquín", 1);
            }
            #endregion
            #endregion
            #region Baul
            if (Session.GetPlay().ViewBaul)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "closebaul");
                Session.SendWhisper("Has dejado de ver el interior del Maletero del Vehículo.", 1);
            }
            #endregion
            #region Phone Shop
            if (Session.GetPlay().ViewShopPhones)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "close_shop");
                Session.GetPlay().ViewShopPhones = false;
            }
            #endregion
            #region Products
            if (Session.GetPlay().ViewProducts)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_products", "close");
                Session.GetPlay().ViewProducts = false;
            }
            #endregion
            #region Apartments
            if (Session.GetPlay().ViewApartments)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_apart", "close");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_apart", "apart_close");
                Session.GetPlay().ViewApartments = false;
            }
            #endregion
            #region Taxi List
            if (Session.GetPlay().ViewTaxiList)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_taxi", "close");
                Session.GetPlay().ViewTaxiList = false;
            }
            #endregion
            #region Change Name
            if (Session.GetPlay().ViewChangeName)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_changename", "close");
                Session.GetPlay().ViewChangeName = false;
            }
            #endregion
            #region GeneralTimer
            if (Session.GetPlay().IsFuelCharging || Session.GetPlay().IsCamLoading || Session.GetPlay().IsMinerLoading || Session.GetPlay().IsMecLoading || Session.GetPlay().IsFarming || Session.GetPlay().IsForcingHouse || Session.GetPlay().TurfCapturing || Session.GetPlay().CallingTaxi || Session.GetPlay().TogglingPSV)
                Session.GetPlay().BreakGeneralTimer = true;

            if (Session.GetPlay().CallingTaxi)
            {
                Session.GetPlay().TaxiNodeGo = -1;

                if (Session.GetRoomUser().CurrentEffect == EffectsList.Taxi)
                    Session.GetRoomUser().ApplyEffect(0);

                RoleplayManager.Shout(Session, "*Cancela su taxi*", 5);
            }

            if (Session.GetPlay().TogglingPSV)
            {
                Session.SendWhisper("Cambio del Modo pasivo cancelado.", 1);
            }
            #endregion
            #endregion

            if (User.RidingHorse)
            {
                RoomUser Horse = Room.GetRoomUserManager().GetRoomUserByVirtualId(User.HorseID);
                if (Horse != null)
                    Horse.MoveTo(MoveX, MoveY);
            }

            if (User.GetClient().GetPlay().Chofer)
            {
                User.MoveDriving(MoveX, MoveY, User);
            }

            if (User.GetClient().GetPlay().EscortingWalk)
            {
                User.MoveConvictEscort(MoveX, MoveY, User);
                // Convicto.MoveTo(MoveX, MoveY);
                //Convicto.MoveTo(SquareInFront(MoveX, MoveY, User.RotBody));
            }

            if (!User.GetClient().GetPlay().Chofer && !User.GetClient().GetPlay().EscortingWalk)
                User.MoveTo(MoveX, MoveY);
        }

        public Point SquareInFront(int X, int Y, int RotBody)
        {
            var Sq = new Point(X, Y);

            if (RotBody == 0)
            {
                Sq.Y--;
            }
            else if (RotBody == 2)
            {
                Sq.X++;
            }
            else if (RotBody == 4)
            {
                Sq.Y++;
            }
            else if (RotBody == 6)
            {
                Sq.X--;
            }

            return Sq;
        }
    }
}
using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using System;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorHouseSing : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session.GetRoomUser() == null)
                return;

            var User = Session.GetRoomUser();

            if (Session.GetPlay().WebSocketConnection != null)
            {
                if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Item.Id);
                    if (House != null)
                    {
                        
                        string Owner = (PlusEnvironment.GetHabboById(House.OwnerId) != null) ? PlusEnvironment.GetHabboById(House.OwnerId).Username : null; // Revisa el Diccionario

                        if (Owner == null)
                            Owner = PlusEnvironment.GetUsernameById(House.OwnerId);// <= Hace SELECT porque puede no estar Online el User a buscar

                        Session.GetPlay().HouseSignId = House.ItemId;
                        Session.GetPlay().HouseOwner = Owner;
                        Session.GetPlay().ViewHouse = true;
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_house", "open");
                    }
                    else
                    {
                        Session.SendNotification("((Ha ocurrido un problema al buscar la Información de la Casa o Terreno. Contacte con un Administrador))");
                    }
                }
                else
                    User.MoveTo(Item.SquareBehind);
            }
            else
            {
                Session.SendWhisper("((El Websocket del servidor está Offline. Por favor contacte con un Administrador))", 1);
                #region Poll Code (OFF)
                /*
                Session.SendWhisper("Since our general websocket server is offline you can use our backup ATM machine!", 1);
                if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    string PollName = "HoloRP ATM";
                    string PollInvitation = "HoloRP ATM";
                    string PollThanks = "Thanks for using HoloRP's ATM!";

                    Polls.Poll ATMPoll = new Polls.Poll(500000, 0, PollName, PollInvitation, PollThanks, "", 1, null);
                    Session.SendMessage(new SuggestPollMessageComposer(ATMPoll));
                }
                else
                    User.MoveTo(Item.SquareInFront);
                */
                #endregion
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}
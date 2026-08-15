using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using System;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorATM : IFurniInteractor
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
                    Session.GetPlay().UsingAtm = true;
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_atm", "open");
                }
                else
                    User.MoveTo(Item.SquareInFront);
            }
            else
            {
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
                Console.WriteLine("[InteractorATM.cs:54]El WS está OFF y el Cajero no puede funcionar.");
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}
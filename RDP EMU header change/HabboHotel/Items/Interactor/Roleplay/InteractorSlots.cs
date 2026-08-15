using System;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Items.Interactor.Roleplay
{
    public class InteractorSlots : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";
            Item.InteractingUser = 0;
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";
            Item.InteractingUser = 0;
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                if (Session.GetHabbo().SlotsInteractation == 0)
                {
                    Session.SendWhisper("Aun no has puesto tu apuesta usa el comando :apostar mas la cantidad.", 3);
                    return;
                }
                if (Session.GetHabbo().SlotLastSpun != 0)
                {
                    if (Session.GetHabbo().SlotLastSpun > PlusEnvironment.Now())
                    {
                        Session.SendWhisper("Espera vas de prisa?!", 3);
                        return;
                    }
                }
                if (Session.GetHabbo().Credits >= Session.GetHabbo().SlotsInteractation)
                {
                    Item.ExtraData = "1";
                    Item.UpdateState();
                    Item.RequestUpdate(4, true);
                    Session.GetHabbo().SlotLastSpun = PlusEnvironment.Now() + 1000;
                    //13 options Min: 0 Max: 13 (0-12)
                    String[] slotOptions = { "As", "2", "3", "Jota", "Rey", "Reina" };

                    Random randCard = new Random();
                    int Card1 = randCard.Next(0, 6);
                    int Card2 = randCard.Next(0, 6);
                    int Card3 = randCard.Next(0, 6);

                    String SlotCard1 = slotOptions[Card1];
                    String SlotCard2 = slotOptions[Card2];
                    String SlotCard3 = slotOptions[Card3];

                    int CreditsWon = 0;

                    if (Card1 == Card2 && Card2 == Card3 && Card3 == Card1)
                    {
                        CreditsWon = randCard.Next(Session.GetHabbo().SlotsInteractation, (Session.GetHabbo().SlotsInteractation * 3));
                    }
                    else if (Card1 == Card2 || Card2 == Card3 || Card1 == Card3)
                    {
                        CreditsWon = randCard.Next(Session.GetHabbo().SlotsInteractation, (Session.GetHabbo().SlotsInteractation * 2));
                    }
                    else
                    {
                        CreditsWon = 0;
                    }
                    Session.SendWhisper("Maquina Tragaperras: " + SlotCard1 + ", " + SlotCard2 + ", " + SlotCard3 + ", Ganaste: " + CreditsWon + " Creditos!");
                    RoleplayManager.Shout(Session, "*Ha ganado " + CreditsWon + " Creditos en la Maquina Tragaperras ", 5);
                    Session.GetHabbo().Credits = Session.GetHabbo().Credits - Session.GetHabbo().SlotsInteractation;
                    Session.GetHabbo().Credits = Session.GetHabbo().Credits + CreditsWon;
                    Session.GetHabbo().UpdateCreditsBalance();
                    Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                }
                else
                {
                    Session.SendWhisper("Tu cantidad de Creditos es muy alta.", 3);
                    return;
                }
            }
            else
            {
                User.MoveTo(Item.SquareInFront);
            }

        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}
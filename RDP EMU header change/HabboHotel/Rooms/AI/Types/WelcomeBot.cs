using System;
using System.Drawing;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Rooms.AI.Responses;
using Plus.Utilities;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rewards.Rooms.AI.Types
{
    class WelcomeBot : BotAI
    {
        private int VirtualId;
        private int ActionTimer = 0;
        private int SpeechTimer = 0;
        int credits;
        int duckets;
        int diamonds;
        int furniID;
        int gotws;
        int hasSomething;

        public WelcomeBot(int VirtualId)
        {
            this.VirtualId = VirtualId;
             credits = Int32.Parse(PlusEnvironment.GetDBConfig().DBData["frank.give.credits"]);
             duckets = Int32.Parse(PlusEnvironment.GetDBConfig().DBData["frank.give.duckets"]);
             diamonds = Int32.Parse(PlusEnvironment.GetDBConfig().DBData["frank.give.diamonds"]);
             furniID = Int32.Parse(PlusEnvironment.GetDBConfig().DBData["frank.give.furni"]);
             gotws = Int32.Parse(PlusEnvironment.GetDBConfig().DBData["frank.give.gotws"]);
            ActionTimer = 0;
             hasSomething = 0;
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
            //if ()
        }

        public override void OnUserSay(RoomUser User, string Message)
        {

        }

        public override void OnUserShout(RoomUser User, string Message)
        {

        }

        public override void OnTimerTick()
        {
            if (GetBotData() == null)
                return;

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(GetRoom().OwnerName);
            if (Target == null || Target.GetHabbo() == null || Target.GetHabbo().CurrentRoom != GetRoom())
            {
                GetRoom().GetGameMap().RemoveUserFromMap(GetRoomUser(), new Point(GetRoomUser().X, GetRoomUser().Y));
                GetRoom().GetRoomUserManager().RemoveBot(GetRoomUser().VirtualId, false);
                return;
            }


            if (ActionTimer <= 0)
            {
                switch (Target.GetHabbo().GetStats().WelcomeLevel)
                    {
                    case 0:
                    default:
                        Point nextCoord;
                        RoomUser Target2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().ForcedUserTargetMovement);
                        if (GetBotData().ForcedMovement)
                        {
                            if (GetRoomUser().Coordinate == GetBotData().TargetCoordinate)
                            {
                                GetBotData().ForcedMovement = false;
                                GetBotData().TargetCoordinate = new Point();

                                GetRoomUser().MoveTo(GetBotData().TargetCoordinate.X, GetBotData().TargetCoordinate.Y);
                            }
                        }
                        else if (GetBotData().ForcedUserTargetMovement > 0)
                        {
                           
                            if (Target2 == null)
                            {
                                GetBotData().ForcedUserTargetMovement = 0;
                                GetRoomUser().ClearMovement(true);
                            }
                            else
                            {
                                var Sq = new Point(Target2.X, Target2.Y);

                                if (Target2.RotBody == 0)
                                {
                                    Sq.Y--;
                                }
                                else if (Target2.RotBody == 2)
                                {
                                    Sq.X++;
                                }
                                else if (Target2.RotBody == 4)
                                {
                                    Sq.Y++;
                                }
                                else if (Target2.RotBody == 6)
                                {
                                    Sq.X--;
                                }


                                GetRoomUser().MoveTo(Sq);
                            }
                        }
                        else if (GetBotData().TargetUser == 0)
                        {
                            nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }
                        Target.GetHabbo().GetStats().WelcomeLevel++;
                        break;
                    case 1:
                            GetRoomUser().Chat("¡Bienvenid@ de nuevo " + GetRoom().OwnerName + "!", false, 33);
                            Target.GetHabbo().GetStats().WelcomeLevel++;
                            break;
                    case 2:
                            if (credits != 0 && diamonds != 0 && duckets != 0 && gotws != 0)
                            {
                                GetRoomUser().Chat("¡Te regalare " + credits + " créditos, " + diamonds + " diamantes, " + duckets + " duckets y " + gotws + " estrellas!", false, 33);
                                Target.GetHabbo().Credits += credits;
                                Target.GetPlay().MoneyEarned += credits;
                                Target.GetHabbo().Diamonds += diamonds;
                                Target.GetPlay().PLEarned += diamonds;
                                Target.GetHabbo().Duckets += duckets;
                                Target.GetHabbo().GOTWPoints += gotws;
                                Target.SendMessage(new CreditBalanceComposer(Target.GetHabbo().Credits));
                                Target.SendMessage(new ActivityPointsComposer(Target.GetHabbo().Duckets, Target.GetHabbo().Diamonds, Target.GetHabbo().GOTWPoints));
                                hasSomething = 1;
                            }
                            else if (credits != 0 && diamonds != 0 && duckets != 0)
                            {
                                GetRoomUser().Chat("¡Te regalare " + credits + " créditos, " + diamonds + " diamantes y " + duckets + " duckets!", false, 33);
                                Target.GetHabbo().Credits += credits;
                                Target.GetPlay().MoneyEarned += credits;
                                Target.GetHabbo().Diamonds += diamonds;
                                Target.GetPlay().PLEarned += diamonds;
                                Target.GetHabbo().Duckets += duckets;
                                Target.SendMessage(new CreditBalanceComposer(Target.GetHabbo().Credits));
                                Target.SendMessage(new ActivityPointsComposer(Target.GetHabbo().Duckets, Target.GetHabbo().Diamonds, Target.GetHabbo().GOTWPoints));
                                hasSomething = 1;
                            }
                            else if (credits != 0 && diamonds != 0)
                            {
                                GetRoomUser().Chat("¡Te regalare " + credits + " créditos y " + diamonds + " diamantes!", false, 33);
                                Target.GetHabbo().Credits += credits;
                                Target.GetPlay().MoneyEarned += credits;
                                Target.GetHabbo().Diamonds += diamonds;
                                Target.GetPlay().PLEarned += diamonds;
                                Target.SendMessage(new CreditBalanceComposer(Target.GetHabbo().Credits));
                                Target.SendMessage(new ActivityPointsComposer(Target.GetHabbo().Duckets, Target.GetHabbo().Diamonds, Target.GetHabbo().GOTWPoints));
                                hasSomething = 1;
                            }
                            else if (credits != 0)
                            {
                                GetRoomUser().Chat("¡Te regalare " + credits + " créditos!", false, 33);
                                Target.GetHabbo().Credits += credits;
                                Target.GetPlay().MoneyEarned += credits;
                                Target.SendMessage(new CreditBalanceComposer(Target.GetHabbo().Credits));
                                hasSomething = 1;
                            }
                            Target.GetHabbo().GetStats().WelcomeLevel++;
                            break;
                        case 3:
                            if (furniID != 0)
                            {
                                hasSomething = 1;
                                GetRoomUser().Chat("¡Ten un furni de regalo!", false, 33);
                                //heere add furni

                                ItemData furni = null;
                                if (PlusEnvironment.GetGame().GetItemManager().GetItem(furniID, out furni))
                                {
                                    Item purchasefurni = ItemFactory.CreateSingleItemNullable(furni, Target.GetHabbo(), "", "");
                                    if (purchasefurni != null)
                                    {
                                        Target.GetHabbo().GetInventoryComponent().TryAddItem(purchasefurni);
                                        Target.SendMessage(new FurniListNotificationComposer(purchasefurni.Id, 1));
                                        Target.SendMessage(new FurniListUpdateComposer());
                                    }
                                }
                            }
                            if (hasSomething == 0)
                            {
                                GetRoomUser().Chat("¡Hoy no tengo algo que darte!", false, 33);

                            }
                            Target.GetHabbo().GetStats().WelcomeLevel++;
                            break;
                        case 4:
                            GetRoomUser().Chat("¡Te espero mañana con mas regalos!", false, 33);
                            Target.GetHabbo().GetStats().WelcomeLevel++;
                            break;
                        case 5:
                            GetRoom().GetGameMap().RemoveUserFromMap(GetRoomUser(), new Point(GetRoomUser().X, GetRoomUser().Y));
                            GetRoom().GetRoomUserManager().RemoveBot(GetRoomUser().VirtualId, false);
                            Target.GetHabbo().GetStats().WelcomeLevel++;
                            break;
                    }
                ActionTimer = new Random(DateTime.Now.Millisecond + this.VirtualId ^ 2).Next(5, 15);
            }
                else
                    ActionTimer--;
            }
        }
    }

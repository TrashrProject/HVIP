using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.Food;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class EatCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_eat"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Come la comida del plato que tengas en frente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            //bool Stolen = false;
            int FoodId = 0;
            Item Item = null;
            RoomUser User = Session.GetRoomUser();
            #endregion

            #region Conditions
            if (User == null)
                return;

            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetX == User.SquareInFront.X && item.GetY == User.SquareInFront.Y)
                {
                    if (FoodManager.GetFood(item.BaseItem) != null)
                    {
                        Item = item;
                        FoodId = item.BaseItem;
                    }
                }
            }

            Food Food = FoodManager.GetFood(FoodId);
            if (Food == null || Item == null)
            {
                Session.SendWhisper("No hay comida en frente de ti.", 1);
                return;
            }

            if (Food.Type != "food")
            {
                if (Food.Type == "drink")
                {
                    Session.SendWhisper("Escribe el comando :tomar para beber esto.", 1);
                    return;
                }
                else
                {
                    Session.SendWhisper("¡No puedes comer eso!", 1);
                    return;
                }
            }

            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes comer mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes comer mientras estás encarcelad@!", 1);
                return;
            }

            if (Session.GetPlay().Hunger <= 0)
            {
                Session.SendWhisper("Tu estómago se encuentra lleno.", 1);
                return;
            }

            if (Food.Cost > 0)
            {
                if (Session.GetHabbo().Credits < Food.Cost)
                {
                    Session.SendWhisper("¡No tienes suficiente dinero para permitirte comer esta comida!", 1);
                    return;
                    /*
                    if (Session.GetPlay().RobItem != Item)
                    {
                        Session.GetPlay().RobItem = Item;
                        Session.SendWhisper("¡No tienes suficiente dinero para permitirte comer esta comida! Quizás si vuelves a intentar...", 1);
                        return;
                    }
                    else
                    {
                        Session.GetPlay().RobItem = null;
                        Session.SendWhisper("¡Comiste la comida sin pagarla! ¡Cuidado si alguien te vio!", 1);
                        Stolen = true;
                    }
                    */
                }
            }
            #endregion

            #region Execute
            string EatText = Food.EatText;

            if (Food.Health == 0)
                EatText = EatText.Replace("[SALUD]", "");
            else
                EatText = EatText.Replace("[SALUD]", "[+" + Food.Health + " HP]");
            /*
            if (Food.Energy == 0)
                EatText = EatText.Replace("[ENERGÍA]", "");
            else
                EatText = EatText.Replace("[ENERGÍA]", "[+" + Food.Energy + " E
            */

            if (Food.Cost == 0)
                EatText = EatText.Replace("[PRECIO]", "");
            else
                EatText = EatText.Replace("[PRECIO]", "[-$" + Food.Cost + "]");

            if (Food.Hunger == 0)
                EatText = EatText.Replace("[HAMBRE]", "");
            else
                EatText = EatText.Replace("[HAMBRE]", "[-" + Food.Hunger + " Hambre]");

            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Eating", 1);
            //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.EAT_FOOD);

            if (Food.Hunger > 0)
            {
                int HungerChange = Session.GetPlay().Hunger - Food.Hunger;

                if (HungerChange <= 0)
                    Session.GetPlay().Hunger = 0;
                else
                    Session.GetPlay().Hunger = HungerChange;

                // Enviar WS
                //Session.GetPlay().RefreshStatDialogue();
                //Session.GetPlay().UpdateInteractingUserDialogues();
            }
            /*
            if (Food.Energy > 0)
            {
                int EnergyChange = Session.GetPlay().CurEnergy + Food.Energy;

                if (EnergyChange >= Session.GetPlay().MaxEnergy)
                    Session.GetPlay().CurEnergy = Session.GetPlay().MaxEnergy;
                else
                    Session.GetPlay().CurEnergy = EnergyChange;
            }
            */
            if (Food.Health > 0)
            {
                int HealthChange = Session.GetPlay().CurHealth + Food.Health;

                if (HealthChange >= Session.GetPlay().MaxHealth)
                    Session.GetPlay().CurHealth = Session.GetPlay().MaxHealth;
                else
                    Session.GetPlay().CurHealth = HealthChange;
            }

            if (Food.Cost > 0/* && !Stolen*/)
            {
                Session.GetHabbo().Credits -= Food.Cost;
                Session.GetHabbo().UpdateCreditsBalance();
            }

            string FoodName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);
            /*
            if (Stolen)
            {
                RoleplayManager.Shout(Session, "*Rápidamente se come el plato con " + FoodName + " sin pagar por él.*", 5);

                if (!Session.GetPlay().WantedFor.Contains("robar comida"))
                    Session.GetPlay().WantedFor = Session.GetPlay().WantedFor + "robar comida, ";
            }
            else
                RoleplayManager.Shout(Session, EatText, 4);
            */

            RoleplayManager.Shout(Session, EatText, 5);
            Session.SendWhisper("Has pagado $"+Food.Cost+" por la comida.", 1);

            if (Item.InteractingUser > 0/* && !Stolen*/)
            {
                var Server = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.InteractingUser);

                if (Server != null)
                {
                    if (Session != Server)
                    {
                        int Tips = (Server.GetHabbo().VIPRank == 1) ? (RoleplayManager.RestTips + Convert.ToInt32(RoleplayManager.RestTips * 0.10)) : (Server.GetHabbo().VIPRank == 2) ? (RoleplayManager.RestTips + Convert.ToInt32(RoleplayManager.RestTips * 0.20)) : RoleplayManager.RestTips;

                        Server.GetHabbo().Credits += Tips;
                        Server.GetPlay().MoneyEarned += Tips;
                        Server.GetHabbo().UpdateCreditsBalance();
                        Server.SendWhisper("Has ganado $"+ Tips + " extra en propina por servir el plato que " + Session.GetHabbo().Username + " comió.", 1);
                        //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Server, "ACH_ServingFood", 1);
                    }
                }

                Item.InteractingUser = 0;
            }
            if(Room.Group != null && Room.Group.GType == 1 && Room.Group.GetAdministrator.Count > 0)
                RoleplayManager.PickItem(Session, Item.Id);
            else
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);
            #endregion
        }
    }
}
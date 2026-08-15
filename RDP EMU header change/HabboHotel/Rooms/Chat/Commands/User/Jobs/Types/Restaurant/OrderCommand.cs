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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Restaurant
{
    class OrderCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_restaurant_order"; }
        }

        public string Parameters
        {
            get { return "%name%"; }
        }

        public string Description
        {
            get { return "Ordena comida o bebida en un plato limpio frente a ti."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            RoomUser User = Session.GetRoomUser();
            #endregion

            #region Conditions
            if(Room.Group == null || Room.Group.GActivity != "Restaurant")
            {
                Session.SendWhisper("Debes estar en un resutaurant para ordenar comida.", 1);
                return;
            }

            if (User == null)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Comando inválido, escribe ':ordenar [" + FoodManager.GetOrdenableItemsName() + "]'", 1);
                return;
            }

            string FoodName = Params[1].ToString();
            Food Food = FoodManager.GetFoodAndDrink(FoodName);

            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes ordenar comida mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes ordenar comida mientras estás encarcelad@!", 1);
                return;
            }
                        
            if (Food == null)
            {
                Session.SendWhisper("Comida o Bebida inválida. Alimentos: " + FoodManager.GetOrdenableItemsName(), 1);
                return;
            }

            if (!FoodManager.CanServe(User))
            {
                Session.SendWhisper("¡Encuentra una mesa libre donde ordenar!", 1);
                return;
            }

            if (!Food.Servable)
            {
                if (Food.Type == "drink")
                    Session.SendWhisper("Solo puedes ordenar: " + FoodManager.GetOrdenableItemsName(), 1);
                else
                    Session.SendWhisper("Solo puedes ordenar: " + FoodManager.GetOrdenableItemsName(), 1);
                return;
            }

            #endregion

            #region Execute
            if (Room.Group.GetAdministrator.Count <= 0)
            {
                double MaxHeight = 0.0;
                Item ItemInFront;
                if (Room.GetGameMap().GetHighestItemForSquare(User.SquareInFront, out ItemInFront))
                {
                    if (ItemInFront != null)
                        MaxHeight = ItemInFront.TotalHeight + 0.1;
                }

                RoleplayManager.Shout(Session, Food.ServeText.Replace("Sirve", "Ordena"), 5);
                Session.GetPlay().isParking = true;
                RoleplayManager.PlaceItemToRoom(Session, Food.ItemId, 0, User.SquareInFront.X, User.SquareInFront.Y, MaxHeight, User.RotBody, false, Room.Id, true, "", true);

                //RoleplayManager.PlaceItemToRoom(Session, Food.ItemId, 0, User.SquareInFront.X, User.SquareInFront.Y, MaxHeight, User.RotBody, false, Room.Id, false, Food.ExtraData, true);
                Session.GetPlay().isParking = false;
            }
            else
            {
                RoleplayManager.Shout(Session, "*Ordena un/@s"+ Food.Name +" y espera a ser atendid@*", 5);
                Session.SendWhisper("Esta empresa no pertenece al gobierno, por lo tanto debes esperar a que algún empleador te atienda.", 1);
            }

            #endregion
        }
    }
}
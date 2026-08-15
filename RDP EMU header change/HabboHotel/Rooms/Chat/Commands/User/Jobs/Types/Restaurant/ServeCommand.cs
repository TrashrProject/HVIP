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
    class ServeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_restaurant_serve"; }
        }

        public string Parameters
        {
            get { return "%name%"; }
        }

        public string Description
        {
            get { return "Sirve comida o bebida en un plato limpio frente a ti."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            RoomUser User = Session.GetRoomUser();
            #endregion

            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                #region Group Conditions
                List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

                if (Groups.Count <= 0)
                {
                    Session.SendWhisper("No tienes ningún trabajo.", 1);
                    return;
                }

                int GroupNumber = -1;

                if (Groups[0].GType != 1)
                {
                    if (Groups.Count > 1)
                    {
                        if (Groups[1].GType != 1)
                        {
                            Session.SendWhisper("((No perteneces a ninguna empresa para usar ese comando))", 1);
                            return;
                        }
                        GroupNumber = 1; // Segundo indicie de variable
                    }
                    else
                    {
                        Session.SendWhisper("((No perteneces a ninguna empresa para usar ese comando))", 1);
                        return;
                    }
                }
                else
                {
                    GroupNumber = 0; // Primer indice de Variable Group
                }

                Session.GetPlay().JobId = Groups[GroupNumber].Id;
                Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
                #endregion

                #region Extra Conditions            
                // Existe el trabajo?
                if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Session.GetPlay().JobId, Session.GetPlay().JobRank))
                {
                    Session.GetPlay().TimeWorked = 0;
                    Session.GetPlay().JobId = 0; // Desempleado
                    Session.GetPlay().JobRank = 0;

                    //Room.Group.DeleteMember(Session.GetHabbo().Id);// OJO ACÁ

                    Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                    return;
                }

                // Puede trabajar aquí?
                Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);
                GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Job.Id, Session.GetPlay().JobRank);
                if (!Rank.CanWorkHere(Room.Id))
                {
                    //String.Join(",", Rank.WorkRooms)
                    Session.SendWhisper("Esta no es la zona de tu trabajo para comenzar a trabajar.", 1);
                    return;
                }
                #endregion
            }

            #region Conditions
            if (User == null)
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "serve") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("No tienes permisos para hacer esa acción.", 1);
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Comando inválido, escribe ':servir [" + FoodManager.GetServableItemsName(Session) + "]'", 1);
                return;
            }

            string FoodName = Params[1].ToString();
            Food Food = FoodManager.GetFoodAndDrink(FoodName);

            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (!Session.GetPlay().IsWorking)
                {
                    Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                    return;
                }
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes servir comida mientras estás muert@!", 1);
                return;
            }

            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes servir comida mientras estás encarcelad@!", 1);
                return;
            }

            
            if (Food == null)
            {
                Session.SendWhisper("Comida o Bebida inválida. Alimentos: " + FoodManager.GetServableItemsName(Session), 1);
                return;
            }

            if (!FoodManager.CanServe(User))
            {
                Session.SendWhisper("¡Encuentra una mesa libre donde servir!", 1);
                return;
            }

            if (Food.Type == "food" && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "food") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Solo puedes servir: " + FoodManager.GetServableItemsName(Session), 1);
                return;
            }

            if (Food.Type == "drink" && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "drink") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Solo puedes servir: " + FoodManager.GetServableItemsName(Session), 1);
                return;
            }

            if (!Food.Servable)
            {
                if (Food.Type == "drink")
                    Session.SendWhisper("Solo puedes servir: " + FoodManager.GetServableItemsName(Session), 1);
                else
                    Session.SendWhisper("Solo puedes servir: " + FoodManager.GetServableItemsName(Session), 1);
                return;
            }

            #region Check Restaurant Corps
            if (Room.Group != null)
            {
                if (Room.Group.GActivity == "Restaurant" && Room.Group.GetAdministrator.Count > 0)
                {
                    if (Room.Group.Stock <= 0)
                    {
                        Session.SendWhisper("¡Oh oh! Al parecer la empresa en la que intentas hace eso no tiene el Stock suficiente para ofrecerte ese servicio.", 1);
                        return;
                    }

                    Room.Group.UpdateSells(Food.Cost);
                    int B = (Room.Group.Bank + Food.Cost), S = (Room.Group.Stock - 1);
                    Room.Group.SetBussines(B, S);

                    if (Session.GetPlay().ViewCorpId == Room.Group.Id)
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_business", "open_room");
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "open");
                    }
                }
            }
            #endregion
            #endregion

            #region Execute
            double MaxHeight = 0.0;
            Item ItemInFront;
            if (Room.GetGameMap().GetHighestItemForSquare(User.SquareInFront, out ItemInFront))
            {
                if (ItemInFront != null)
                    MaxHeight = ItemInFront.TotalHeight + 0.1;
            }

            RoleplayManager.Shout(Session, Food.ServeText, 5);
            Session.GetPlay().isParking = true;
            RoleplayManager.PlaceItemToRoom(Session, Food.ItemId, 0, User.SquareInFront.X, User.SquareInFront.Y, MaxHeight, User.RotBody, false, Room.Id, true, "", true);

            //RoleplayManager.PlaceItemToRoom(Session, Food.ItemId, 0, User.SquareInFront.X, User.SquareInFront.Y, MaxHeight, User.RotBody, false, Room.Id, false, Food.ExtraData, true);
            Session.GetPlay().isParking = false;

            #endregion
        }
    }
}
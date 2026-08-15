using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorTrashCan : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "" || Item.ExtraData == "0")
                    if (User.CanWalk)
                        User.MoveTo(Item.SquareInFront);
            }
            else
            {
                #region Conditions                

                #region Group Conditions
                List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

                if (Groups.Count <= 0)
                {
                    Session.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                    return;
                }

                int GroupNumber = -1;

                if (Groups[0].GType != 2)
                {
                    if (Groups.Count > 1)
                    {
                        if (Groups[1].GType != 2)
                        {
                            Session.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                            return;
                        }
                        GroupNumber = 1; // Segundo indicie de variable
                    }
                    else
                    {
                        Session.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
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

                if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "basurero"))
                {
                    Session.SendWhisper("Debes tener el trabajo de Basuero recoger contenedores de basura.", 1);
                    return;
                }

                #endregion

                #region Basic Conditions
                if (Session.GetPlay().Cuffed)
                {
                    Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                    return;
                }
                if (!Session.GetRoomUser().CanWalk)
                {
                    Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                    return;
                }
                if (Session.GetPlay().Pasajero)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
                    return;
                }
                if (Session.GetPlay().IsDead)
                {
                    Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsJailed)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().IsDying)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetPlay().DrivingCar)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                    return;
                }
                if(Session.GetPlay().EquippedWeapon != null)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras equipas un arma!", 1);
                    return;
                }
                if (!Session.GetPlay().IsWorking)
                {
                    Session.SendWhisper("¡Debes trabajar de recolector de basura para hacer eso!", 1);
                    return;
                }
                if(Session.GetPlay().BasuTrashCount >= 15)
                {
                    Session.SendWhisper("¡Ya tienen 15 contenedores recogidos! Vuelvan al Basurero a ':descargarcamion' para recibir su paga.", 1);
                    return;
                }
                #endregion

                #region Basurero Conditions
                if(Session.GetPlay().BasuTeamId <= 0)
                {
                    Session.SendWhisper("Para recolectar basura necesitas un Compañero que conduzca el Camión.", 1);
                    return;
                }
                GameClient TeamChofer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetPlay().BasuTeamId);
                if(TeamChofer == null)
                {
                    Session.SendWhisper("Al parecer tu compañero de Basurero se ha ido y han Fracasado el Recorrido.", 1);
                    #region Retornamos Vars
                    Session.GetPlay().BasuTeamId = 0;
                    Session.GetPlay().BasuTeamName = "";
                    Session.GetPlay().BasuTrashCount = 0;
                    Session.GetPlay().IsBasuPasaj = false;
                    Session.GetPlay().IsBasuChofer = false;
                    #endregion
                    return;
                }
                if (!TeamChofer.GetPlay().IsBasuChofer)
                {
                    Session.SendWhisper("Tu compañero de Basurero debe estar conduciendo el Camión de Basura. Recuerda que puedes usar ':nobasurero' para cancelar la misión.", 1);
                    return;
                }
                #endregion
                #endregion

                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    int Minutes = 5;

                    User.ClearMovement(true);
                    User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(135 * Minutes, true);
                    RoleplayManager.Shout(Session, "*Comienza a recoger la basura del contenedor*", 5);

                    new Thread(() =>
                    {
                        User.CanWalk = false;

                        if (User.CurrentEffect != 4 && Session.GetPlay().EquippedWeapon == null)
                            User.ApplyEffect(EffectsList.Twinkle);

                        Thread.Sleep(RoleplayManager.GetTimerByMyJob(Session, "basurero") * 1000);

                        if (User.CurrentEffect != 0 && Session.GetPlay().EquippedWeapon == null)
                            User.ApplyEffect(0);

                        if (Session != null && Session.GetPlay() != null && Session.GetHabbo() != null)
                            ChooseReward(Session);
                        if (User != null)
                            User.CanWalk = true;

                        Session.GetPlay().BasuTrashCount++;
                        TeamChofer.GetPlay().BasuTrashCount++;

                        Session.SendWhisper("Contenedores: "+ Session.GetPlay().BasuTrashCount + " / 15", 1);
                        TeamChofer.SendWhisper("Contenedores: " + TeamChofer.GetPlay().BasuTrashCount + " / 15", 1);

                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session,
                            "compose_basurero|" +
                            "basucount|" +
                            Session.GetPlay().BasuTrashCount + "/15");

                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TeamChofer,
                            "compose_basurero|" +
                            "basucount|" +
                            TeamChofer.GetPlay().BasuTrashCount + "/15");

                        if (Session.GetPlay().BasuTrashCount >= 15)
                        {
                            Session.SendWhisper("¡Han llegado a recolectar 15 Contenedores! Ahora vuelvan al Basurero para ':descargarcamion' y recibir su paga.", 1);
                            TeamChofer.SendWhisper("¡Han llegado a recolectar 15 Contenedores! Ahora vuelvan al Basurero para ':descargarcamion' y recibir su paga.", 1);
                        }

                    }).Start();
                }
                else
                    Session.SendWhisper("¡Al parecer este contenedor de basura ya ha sido recogido!", 1);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }

        public void ChooseReward(GameClient Session)
        {
            var Random = new CryptoRandom();
            //int TotalCraftingItems = CraftingManager.CraftableItems.Count;
            int Chance = Random.Next(1, 101);
            int SecondChance = Random.Next(1, 101);

            //if (SecondChance < 4 && Chance > TotalCraftingItems)
              //  Chance = Random.Next(1, TotalCraftingItems + 1);

            #region Crafting Materials (OFF)
            /*
            if (Chance <= TotalCraftingItems)
            {
                var CraftingItemName = CraftingManager.CraftableItems[Chance - 1];

                ItemData Data = null;
                foreach (var itemdata in PlusEnvironment.GetGame().GetItemManager()._items.Values)
                {
                    if (itemdata.ItemName != CraftingItemName)
                        continue;

                    Data = itemdata;
                    break;
                }

                var Item = ItemFactory.CreateSingleItemNullable(Data, Session.GetHabbo(), "", "");
                Session.GetHabbo().GetInventoryComponent().TryAddItem(Item);

                ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
                ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

                Session.GetPlay().CraftingCheck = true;
                Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetPlay().CraftingCheck));
                RoleplayManager.Shout(Session, "*After rummaging the trash can, they pull out what appears to be one " + Item.GetBaseItem().PublicName +"*", 4);
            }
            */
            #endregion

            #region Drugs
            if (Chance <= 40)
            {
                int Amount;

                // Cocaine
                if (Chance > 30)
                {
                    Amount = Random.Next(1, 3);
                    Session.GetPlay().Cocaine += Amount;
                    RoleplayManager.Shout(Session, "*Encuentra " + Amount + "g de Crack dentro de la basura*", 5);
                }

                // Medicamentos
                else if (Chance <= 30 && Chance > 16)
                {
                    Amount = Random.Next(1, 3);
                    Session.GetPlay().Medicines += Amount;
                    RoleplayManager.Shout(Session, "*Encuentra " + Amount + " medicamento(s) dentro de la basura*", 5);
                }
                /*// Weed
                else
                {
                    Amount = Random.Next(1, 4);
                    Session.GetPlay().Weed += Amount;
                    RoleplayManager.Shout(Session, "*After rummaging the trash can, they pull out a small bag containing " + Amount + "g of weed*", 4);
                }
                */
            }
            #endregion

            #region Money
            else if (Chance > 40 && Chance <= 65)
            {
                int Amount = Random.Next(3, 9);

                Session.GetHabbo().Credits += Amount;
                Session.GetHabbo().UpdateCreditsBalance();
                Session.GetPlay().MoneyEarned += Amount;
                RoleplayManager.Shout(Session, "*Ha encontrado una billetera con $" + Amount + " dentro de la basura*", 5);
            }
            #endregion

            #region Special Bot
            /*else if (Chance > 75 && Chance <= 78)
            {

            }*/
            #endregion

            #region No Reward
            else
            {
                Session.SendWhisper("Esta vez no has encontrado nada en el contenedor.", 1);
            }
            #endregion
        }
    }
}
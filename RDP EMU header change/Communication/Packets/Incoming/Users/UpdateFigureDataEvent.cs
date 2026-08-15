/*
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Global;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Quests;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Communication.Packets.Incoming.Users
{
    class UpdateFigureDataEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            string Gender = Packet.PopString().ToUpper();
            string Look = PlusEnvironment.GetGame().GetAntiMutant().RunLook(Packet.PopString());

            if (Look == Session.GetHabbo().Look)
                return;

            if ((DateTime.Now - Session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().ClothingUpdateWarnings += 1;
                if (Session.GetHabbo().ClothingUpdateWarnings >= 25)
                    Session.GetHabbo().SessionClothingBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionClothingBlocked)
                return;

            Session.GetHabbo().LastClothingUpdateTime = DateTime.Now;

            string[] AllowedGenders = { "M", "F" };
            if (!AllowedGenders.Contains(Gender))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Sorry, you chose an invalid gender."));
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);

            Session.GetHabbo().Look = PlusEnvironment.FilterFigure(Look);
            Session.GetHabbo().Gender = Gender.ToLower();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("look", Look);
                dbClient.AddParameter("gender", Gender);
                dbClient.RunQuery();
            }

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_AvatarLooks", 1);
            Session.SendMessage(new AvatarAspectUpdateMessageComposer(Look, Gender)); //esto
            if (Session.GetHabbo().Look.Contains("ha-1006"))
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WEAR_HAT);

            if (Session.GetHabbo().InRoom)
            {
                RoomUser RoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (RoomUser != null)
                {
                    Session.SendMessage(new UserChangeComposer(RoomUser, true));
                    Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
                }
            }
        }
    }
}

*/

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Global;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Quests;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Incoming.Users
{
    class UpdateFigureDataEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetRoomUser() == null)
                return;

            //Generamos la Sala
            Room Room = RoleplayManager.GenerateRoom(Session.GetRoomUser().RoomId);

            string Gender = Packet.PopString().ToUpper();
            string Look = PlusEnvironment.GetGame().GetAntiMutant().RunLook(Packet.PopString());

            if (!Session.GetHabbo().InRoom)
                return;

            if (!Room.WardrobeEnabled)
            {
                Session.SendNotification("Debes dirigirte a una tienda de ropa o a un probador para cambiar tu atuendo.");
                return;
            }

            // NEW
            Item BTile = null;
            //BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => (x.GetBaseItem().ItemName.ToLower() == "boutique_changing1" || x.GetBaseItem().ItemName.ToLower() == "boutique_changing2" || x.GetBaseItem().ItemName.ToLower() == "boutique_changing3") && x.Coordinate == Session.GetRoomUser().Coordinate);
            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => (x.GetBaseItem().InteractionType == InteractionType.WARDROBE) && x.Coordinate == Session.GetRoomUser().Coordinate);
            if (BTile == null)
            {
                Session.SendNotification("Debes estar dentro de un probador para cambiar tu atuendo.");
                return;
            }

            if (Session.GetPlay().IsWorking)
            {
                Session.SendNotification("¡No puedes cambiar tu atuendo mientras trabajas!");
                return;
            }

            if (Look == Session.GetHabbo().Look)
            {
                Session.SendWhisper("¡Tu ropa sigue siendo la misma!", 1);
                return;
            }

            if(Session.GetPlay().InTutorial && Session.GetPlay().TutorialStep < 13)
            {
                Session.SendWhisper("¡Hey, no tan rápido! Ve siguiendo el Tutorial paso a paso para guiarte de la mejor manera.", 1);
                return;
            }

            if ((DateTime.Now - Session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().ClothingUpdateWarnings += 1;
                if (Session.GetHabbo().ClothingUpdateWarnings >= 25)
                    Session.GetHabbo().SessionClothingBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionClothingBlocked)
                return;

            if (Session.GetPlay().PurchasingClothing)
            {
                Session.GetPlay().PurchasingClothing = false;
                return;
            }

            Session.GetHabbo().LastClothingUpdateTime = DateTime.Now;

            string[] AllowedGenders = { "M", "F" };
            if (!AllowedGenders.Contains(Gender))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Lo sentimos, ha seleccionado un género inválido."));
                return;
            }

            //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);

            Session.GetHabbo().Look = PlusEnvironment.FilterFigure(Look);
            Session.GetHabbo().Gender = Gender.ToLower();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("look", Look);
                dbClient.AddParameter("gender", Gender);
                dbClient.RunQuery();
            }

            Session.SendMessage(new AvatarAspectUpdateMessageComposer(Look, Gender));

            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_AvatarLooks", 1);
            //if (Session.GetHabbo().Look.Contains("ha-1006"))
            //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WEAR_HAT);

            if (Session.GetHabbo().InRoom)
            {
                RoomUser RoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (RoomUser != null)
                {
                    Session.SendMessage(new UserChangeComposer(RoomUser, true));
                    Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
                }
            }

            if (Session.GetHabbo().GetMessenger() != null)
                Session.GetHabbo().GetMessenger().OnStatusChanged(true);

            Session.GetPlay().OriginalOutfit = Session.GetHabbo().Look;

            var HouseInside = PlusEnvironment.GetGame().GetHouseManager().GetHouseByInsideRoom(Room.Id);
            var ApartInside = PlusEnvironment.GetGame().GetApartmentOwnedManager().GetApartmentByInsideRoom(Room.Id);

            // Está dentro de una casa o apartamento. No cobramos el cambio de atuendo.
            if (HouseInside != null || ApartInside != null)
            {
                RoleplayManager.Shout(Session, "*Ha cambiado su atuendo*", 5);
            }
            else
            {
                if (Session.GetHabbo().Credits < RoleplayManager.ClothPrice)
                {
                    Session.SendWhisper("Necesitas $"+ RoleplayManager.ClothPrice + " para comprar ropa nueva.", 1);
                }
                else
                {
                    RoleplayManager.Shout(Session, "*Ha comprado un atuendo nuevo*", 5);
                    Session.SendWhisper("Has comprado ropa nueva por $" + RoleplayManager.ClothPrice, 1);

                    Session.GetHabbo().Credits -= RoleplayManager.ClothPrice;
                    Session.GetHabbo().UpdateCreditsBalance();

                    #region Tutorial Step Check
                    if (Session.GetPlay().TutorialStep == 13 && Room.WardrobeEnabled && Room.Type.Equals("public"))
                    {
                        Session.GetPlay().TutorialStep = 17;
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_tutorial|16");
                    }
                    #endregion
                }
            }
        }
    }
}

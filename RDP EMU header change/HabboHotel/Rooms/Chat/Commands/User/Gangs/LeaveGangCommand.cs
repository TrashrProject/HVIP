using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboRoleplay.VehiclesJobs;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class LeaveGangCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_leave"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Renunciar a una banda."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Principal Conditions            
            if (Session.GetPlay().TurfCapturing)
            {
                Session.SendWhisper("Primero debes dejar de capturar un territorio.", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("leavegang", true))
                return;
            #endregion

            #region Group Conditions            
            List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);

            if (MyGang.Count <= 0 || MyGang == null)
            {
                Session.SendWhisper("No perteneces a ninguna banda para abandonar.", 1);
                return;
            }
            #endregion

            #region Execute
            if(MyGang[0].IsAdmin(Session.GetHabbo().Id))
            {
                Session.SendWhisper("¡Hey! No puedes abandonar tu banda sin antes dejar el mandato a otro miembro.");
                return;
            }

            #region Sacar
            int UserId = Session.GetHabbo().Id;

            if (MyGang[0].IsMember(UserId))
                MyGang[0].DeleteMember(UserId);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                dbClient.AddParameter("GroupId", MyGang[0].Id);
                dbClient.AddParameter("UserId", UserId);
                dbClient.RunQuery();
            }
            // OFF Packet
            //Session.SendMessage(new GroupInfoComposer(MyGang[0], Session));
            if (Session.GetHabbo().GetStats().FavouriteGroupId == MyGang[0].Id)
            {
                Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                }

                if (MyGang[0].AdminOnlyDeco == 0)
                {
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(MyGang[0].RoomId, out Room))
                        return;

                    RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (User != null)
                    {
                        User.RemoveStatus("flatctrl 1");
                        User.UpdateNeeded = true;

                        if (User.GetClient() != null)
                            User.GetClient().SendMessage(new YouAreControllerComposer(0));
                    }
                }

                if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoom != null)
                {
                    RoomUser User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (User != null)
                        Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, MyGang[0], User.VirtualId));
                    Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                }
                else
                    Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
            #endregion

            Session.GetPlay().CooldownManager.CreateCooldown("leavegang", 1000, 5);

            RoleplayManager.Shout(Session, "*Ha abandonado la banda "+ MyGang[0].Name+"*", 5);
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "open");
            return;
            #endregion
        }
    }
}
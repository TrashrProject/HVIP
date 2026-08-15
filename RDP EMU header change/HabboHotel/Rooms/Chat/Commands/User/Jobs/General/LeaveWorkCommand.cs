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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class LeaveWorkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_work_leave"; }
        }

        public string Parameters
        {
            get { return "%id%"; }
        }

        public string Description
        {
            get { return "Renunciar a un Trabajo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Principal Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el ID del Trabajo al que renunciarás. Usa ':ayuda trabajos' para ver sus ID's.", 1);
                return;
            }
            int Gid = 0;
            if (!int.TryParse((Params[1]), out Gid))
            {
                Session.SendWhisper("El ID del trabajo no es válida.", 1);
                return;
            }
            if (Session.GetPlay().IsWorking)
            {
                Session.SendWhisper("Primero debes quitarte el uniforme para dejar de Trabajar.", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }
            if (Session.GetPlay().DrivingInCar)
            {
                Session.SendWhisper("¡Primero detén el auto que tienes afuera!", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas de pasajero!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("leavework", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Group Conditions            
            List<Group> Jobs = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

            if (Jobs.Count <= 0)
            {
                Session.SendWhisper("No tienes ningún trabajo al cual renunciar", 1);
                return;
            }
            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Gid, 1))
            {
                Session.SendWhisper("¡Trabajo inválido!", 1);
                return;
            }
            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Gid, out Group))
            {
                Session.SendWhisper("Ha ocurrido un Error al Obtener información del Trabajo. Contacte con un Administrador. [1]", 1);
                return;
            }
            if (Group.GType > 2)
            {
                Session.SendWhisper("¡Trabajo inválido!", 1);
                return;
            }
            if (!Group.IsMember(Session.GetHabbo().Id) && !Group.IsAdmin(Session.GetHabbo().Id)/* || (Group.HasRequest(Session.GetHabbo().Id) && Group.GroupType == GroupType.PRIVATE)*/)
            {
                Session.SendWhisper("¡No perteneces a ese Trabajo para poder renunciar!", 1);
                return;
            }

            #endregion

            #region Execute
            int Indx = 0;
            if(Jobs.Count == 2)
            {
                if (Jobs[1].Id == Group.Id)
                    Indx = 1;
            }

            if(Jobs[Indx].IsAdmin(Session.GetHabbo().Id) && !Jobs[Indx].Name.Contains("Policía"))
            {
                Session.SendWhisper("¡Hey! No puedes renunciar de tu propia Empresa. ¡Abre el Panel de tu Empresa y véndela!", 1);
                return;
            }

            #region Sacar
            int UserId = Session.GetHabbo().Id;
            string ExtraInf = "";
            if (Jobs[Indx].IsAdmin(UserId))
            {
                ExtraInf = "¡Has renunciado al Cargo de Fundador en " + Jobs[Indx].Name + "!";
            }      

            if (Jobs[Indx].IsAdmin(UserId))
            {
                if (Jobs[Indx].IsAdmin(UserId))
                    Jobs[Indx].TakeAdmin(UserId);

                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[Indx].RoomId, out Room))
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

            if (Jobs[Indx].IsMember(UserId))
                Jobs[Indx].DeleteMember(UserId);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                dbClient.AddParameter("GroupId", Jobs[Indx].Id);
                dbClient.AddParameter("UserId", UserId);
                dbClient.RunQuery();
            }
            // OFF Packet
            //Session.SendMessage(new GroupInfoComposer(Jobs[Indx], Session));
            if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[Indx].Id)
            {
                Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                }

                if (Jobs[Indx].AdminOnlyDeco == 0)
                {
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[Indx].RoomId, out Room))
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
                        Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[Indx], User.VirtualId));
                    Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                }
                else
                    Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
            #endregion
            Session.GetPlay().CooldownManager.CreateCooldown("leavework", 1000, 5);
            RoleplayManager.Shout(Session, "*Ha renunciado al trabajo de "+Group.Name+"*", 5);
            if (ExtraInf.Length > 0)
                Session.SendWhisper(ExtraInf, 1);

            RoleplayManager.CheckCorpCarp(Session);
            Session.GetPlay().CamCargId = 0;

            RoleplayManager.PoliceCMDSCheck(Session);
            // WS Groups
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_group", "open");
            return;
            #endregion
        }
    }
}
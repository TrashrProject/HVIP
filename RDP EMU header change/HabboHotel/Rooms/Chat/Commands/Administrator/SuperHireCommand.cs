using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class SuperHireCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_superhire"; }
        }

        public string Parameters
        {
            get { return "%user% %jobid% %jobrank%"; }
        }

        public string Description
        {
            get { return "Contrata a un usuario en un Empleo y Rango específico."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length < 4)
            {
                Session.SendWhisper("Comando inválido, escribe ':superhire [usuario] [Trabajo] [Puesto]'", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            int jobId;
            if (!int.TryParse(Params[2], out jobId))
            {
                Session.SendWhisper("¡Debes ingresar una ID del Trabajo válida!", 1);
                return;
            }

            int jobRank;
            if (!int.TryParse(Params[3], out jobRank))
            {
                Session.SendWhisper("¡Debes ingresar un número de Puesto válido!", 1);
                return;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(jobId, jobRank))
            {
                Session.SendWhisper("¡Trabajo inválido!", 1);
                return;
            }

            #endregion

            #region Execute
            string ExtraInf = "";
            Group Group = null;
            bool OnlyUpdate = false;
            #region Conditions
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(jobId, out Group))
            {
                Session.SendWhisper("Ha ocurrido un Error al Obtener información del Trabajo. Contacte con un Administrador. [2]", 1);
                return;
            }
            
            if (Group.IsMember(TargetClient.GetHabbo().Id) || Group.IsAdmin(TargetClient.GetHabbo().Id)/* || (Group.HasRequest(Session.GetHabbo().Id) && Group.GroupType == GroupType.PRIVATE)*/)
            {
                if (Group.Members[TargetClient.GetHabbo().Id].UserRank == jobRank)
                {
                    Session.SendWhisper("¡Esa persona ya tiene ese Trabajo y mismo Puesto!", 1);
                    return;
                }
                OnlyUpdate = true;
            }

            // Obtenemos los Trabajos del Target
            List<Group> Jobs = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(TargetClient.GetHabbo().Id);
            if(Jobs == null)
            {
                Session.SendWhisper("Ha ocurrido un Error al Obtener información de los Trabajos del Usuario. Contacte con un Administrador. [1]", 1);
                return;
            }
            #endregion

            int TotalJobs = Jobs.Count;

            #region Si se encuentra Trabajando
            if (TargetClient.GetPlay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(TargetClient);
                TargetClient.GetPlay().IsWorking = false;
                TargetClient.GetHabbo().Poof();
                RoleplayManager.CheckCorpCarp(TargetClient);

            }
            if (TargetClient.GetPlay().Ficha > 0)
            {
                TargetClient.GetPlay().Ficha = 0;
                RoleplayManager.CheckCorpCarp(TargetClient);
            }
            #endregion

            if (TotalJobs == 0)
            {
                #region DirectJoin

                #region SendPackets JoinGroupEvent
                Group.AddMember(TargetClient.GetHabbo().Id);

                if (Group.GroupType == GroupType.LOCKED)
                {
                    List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                    foreach (GameClient Client in GroupAdmins)
                    {
                        Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, TargetClient.GetHabbo(), 3));
                    }

                    TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));
                }
                else
                {
                    TargetClient.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(TargetClient.GetHabbo().Id)));
                    TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));

                    if (TargetClient.GetHabbo().CurrentRoom != null)
                        TargetClient.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));
                    else
                        TargetClient.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));

                    if (Group.GroupChatEnabled)
                        TargetClient.SendMessage(new FriendListUpdateComposer(-Group.Id, Group.Id));
                }
                TargetClient.GetPlay().JobRequest = false;
                #endregion

                //Si es Empresa (PRIVATE)
                // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                #region SendPackets AcceptGroupMembershipEvent
                if (Group.GroupType == GroupType.LOCKED)
                {
                    int UserId = TargetClient.GetHabbo().Id;
                    if (!Group.HasRequest(UserId))
                        return;

                    Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                    if (Habbo == null)
                    {
                        Session.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                        return;
                    }

                    Group.HandleRequest(UserId, true);

                    Session.SendMessage(new GroupMemberUpdatedComposer(jobId, Habbo, 4));

                }
                #endregion


                // Actualizamos Información del Rank del User
                TargetClient.GetPlay().JobId = jobId;
                TargetClient.GetPlay().JobRank = jobRank;
                Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                // Retornamos Vars
                TargetClient.GetPlay().JobId = 0;
                TargetClient.GetPlay().JobRank = 0;
                #endregion
            }
            else if(TotalJobs == 1)
            {
                #region Check Jobs by VIP Type
                if (OnlyUpdate)
                {
                    #region Only Update
                    // Actualizamos Información del Rank del User
                    TargetClient.GetPlay().JobId = jobId;
                    TargetClient.GetPlay().JobRank = jobRank;
                    Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                    // Retornamos Vars
                    TargetClient.GetPlay().JobId = 0;
                    TargetClient.GetPlay().JobRank = 0;
                    #endregion
                }
                else
                {
                    if (TargetClient.GetHabbo().VIPRank > 1)
                    {
                        if (Jobs[0].GType != Group.GType)
                        {
                            #region DirectJoin

                            #region SendPackets JoinGroupEvent
                            Group.AddMember(TargetClient.GetHabbo().Id);

                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                                foreach (GameClient Client in GroupAdmins)
                                {
                                    Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, TargetClient.GetHabbo(), 3));
                                }

                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));
                            }
                            else
                            {
                                TargetClient.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(TargetClient.GetHabbo().Id)));
                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));

                                if (TargetClient.GetHabbo().CurrentRoom != null)
                                    TargetClient.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));
                                else
                                    TargetClient.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));

                                if (Group.GroupChatEnabled)
                                    TargetClient.SendMessage(new FriendListUpdateComposer(-Group.Id, Group.Id));
                            }
                            TargetClient.GetPlay().JobRequest = false;
                            #endregion

                            //Si es Empresa (PRIVATE)
                            // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                            #region SendPackets AcceptGroupMembershipEvent
                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                int UserId = TargetClient.GetHabbo().Id;
                                if (!Group.HasRequest(UserId))
                                    return;

                                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                                if (Habbo == null)
                                {
                                    Session.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                                    return;
                                }

                                Group.HandleRequest(UserId, true);

                                Session.SendMessage(new GroupMemberUpdatedComposer(jobId, Habbo, 4));

                            }
                            #endregion


                            // Actualizamos Información del Rank del User
                            TargetClient.GetPlay().JobId = jobId;
                            TargetClient.GetPlay().JobRank = jobRank;
                            Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                            // Retornamos Vars
                            TargetClient.GetPlay().JobId = 0;
                            TargetClient.GetPlay().JobRank = 0;
                            #endregion
                        }
                        else
                        {
                            //Sacar del Jobs[0]
                            #region Sacar
                            RoleplayManager.CheckCorpCarp(TargetClient);
                            int UserId = TargetClient.GetHabbo().Id;
                            if (Jobs[0].IsAdmin(UserId))
                            {
                                Session.SendWhisper("Esta persona era Fundadora de la Empresa " + Jobs[0].Name + ", por lo cual se le ha retirado el puesto para colocarle el Trabajo Asignado.", 1);
                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                                //return;
                            }
                            {
                                Session.SendWhisper("Le has retirado el puesto a "+TargetClient.GetHabbo().Username+" en "+Jobs[0].Name+" para colocarle el Trabajo Asignado.", 1);
                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                            }
                            if (UserId == Session.GetHabbo().Id)
                            {
                                if (Jobs[0].IsMember(UserId))
                                    Jobs[0].DeleteMember(UserId);

                                if (Jobs[0].IsAdmin(UserId))
                                {
                                    if (Jobs[0].IsAdmin(UserId))
                                        Jobs[0].TakeAdmin(UserId);
                                    
                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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

                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                                    dbClient.AddParameter("GroupId", Jobs[0].Id);
                                    dbClient.AddParameter("UserId", UserId);
                                    dbClient.RunQuery();
                                }

                                Session.SendMessage(new GroupInfoComposer(Jobs[0], Session));
                                if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                                {
                                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                    }

                                    if (Jobs[0].AdminOnlyDeco == 0)
                                    {
                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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
                                            Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[0], User.VirtualId));
                                        Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                    }
                                    else
                                        Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                }
                                //return;
                            }
                            else
                            {
                                //if (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id))
                                //{
                                    if (!Jobs[0].IsMember(UserId))
                                        return;

                                    /*
                                    if (Jobs[0].IsAdmin(UserId) && Jobs[0].CreatorId != Session.GetHabbo().Id)
                                    {
                                        Session.SendNotification("Sorry, only group creators can remove other administrators from the Jobs[0].");
                                        return;
                                    }
                                    */

                                    if (Jobs[0].IsAdmin(UserId))
                                        Jobs[0].TakeAdmin(UserId);

                                    if (Jobs[0].IsMember(UserId))
                                        Jobs[0].DeleteMember(UserId);

                                    List<UserCache> Members = new List<UserCache>();
                                    List<int> MemberIds = Jobs[0].GetAllMembers;
                                    foreach (int Id in MemberIds.ToList())
                                    {
                                        UserCache GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                                        if (GroupMember == null)
                                            continue;

                                        if (!Members.Contains(GroupMember))
                                            Members.Add(GroupMember);
                                    }

                                    /*
                                    int FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                                    int MembersCount = Members.Count;

                                    Session.SendMessage(new GroupMembersComposer(Jobs[0], Members.Take(FinishIndex).ToList(), MembersCount, 1, (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id)), 0, ""));*/
                                //}
                            }
                            #endregion

                            #region DirectJoin

                            #region SendPackets JoinGroupEvent
                            Group.AddMember(TargetClient.GetHabbo().Id);

                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                                foreach (GameClient Client in GroupAdmins)
                                {
                                    Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, TargetClient.GetHabbo(), 3));
                                }

                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));
                            }
                            else
                            {
                                TargetClient.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(TargetClient.GetHabbo().Id)));
                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));

                                if (TargetClient.GetHabbo().CurrentRoom != null)
                                    TargetClient.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));
                                else
                                    TargetClient.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));

                                if (Group.GroupChatEnabled)
                                    TargetClient.SendMessage(new FriendListUpdateComposer(-Group.Id, Group.Id));
                            }
                            TargetClient.GetPlay().JobRequest = false;
                            #endregion

                            //Si es Empresa (PRIVATE)
                            // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                            #region SendPackets AcceptGroupMembershipEvent
                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                if (!Group.HasRequest(UserId))
                                    return;

                                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                                if (Habbo == null)
                                {
                                    Session.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                                    return;
                                }

                                Group.HandleRequest(UserId, true);

                                Session.SendMessage(new GroupMemberUpdatedComposer(jobId, Habbo, 4));

                            }
                            #endregion


                            // Actualizamos Información del Rank del User
                            TargetClient.GetPlay().JobId = jobId;
                            TargetClient.GetPlay().JobRank = jobRank;
                            Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                            // Retornamos Vars
                            TargetClient.GetPlay().JobId = 0;
                            TargetClient.GetPlay().JobRank = 0;
                            #endregion
                        }
                    }
                    else
                    {
                        //Sacar del Jobs[0]
                        #region Sacar
                        RoleplayManager.CheckCorpCarp(TargetClient);
                        int UserId = TargetClient.GetHabbo().Id;
                        if (Jobs[0].IsAdmin(UserId))
                        {
                            Session.SendWhisper("Esta persona era Fundadora de la Empresa " + Jobs[0].Name + ", por lo cual se le ha retirado el puesto para colocarle el Trabajo Asignado.", 1);
                            ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                            //return;
                        }
                        {
                            Session.SendWhisper("Le has retirado el puesto a " + TargetClient.GetHabbo().Username + " en " + Jobs[0].Name + " para colocarle el Trabajo Asignado.", 1);
                            ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                        }
                        if (UserId == Session.GetHabbo().Id)
                        {
                            if (Jobs[0].IsMember(UserId))
                                Jobs[0].DeleteMember(UserId);

                            if (Jobs[0].IsAdmin(UserId))
                            {
                                if (Jobs[0].IsAdmin(UserId))
                                    Jobs[0].TakeAdmin(UserId);

                                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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

                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                                dbClient.AddParameter("GroupId", Jobs[0].Id);
                                dbClient.AddParameter("UserId", UserId);
                                dbClient.RunQuery();
                            }

                            Session.SendMessage(new GroupInfoComposer(Jobs[0], Session));
                            if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                            {
                                Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                }

                                if (Jobs[0].AdminOnlyDeco == 0)
                                {
                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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
                                        Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[0], User.VirtualId));
                                    Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                }
                                else
                                    Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                            }
                            //return;
                        }
                        else
                        {
                            //if (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id))
                            //{
                                if (!Jobs[0].IsMember(UserId))
                                    return;

                                /*
                                if (Jobs[0].IsAdmin(UserId) && Jobs[0].CreatorId != Session.GetHabbo().Id)
                                {
                                    Session.SendNotification("Sorry, only group creators can remove other administrators from the Jobs[0].");
                                    return;
                                }
                                */

                                if (Jobs[0].IsAdmin(UserId))
                                    Jobs[0].TakeAdmin(UserId);

                                if (Jobs[0].IsMember(UserId))
                                    Jobs[0].DeleteMember(UserId);

                                List<UserCache> Members = new List<UserCache>();
                                List<int> MemberIds = Jobs[0].GetAllMembers;
                                foreach (int Id in MemberIds.ToList())
                                {
                                    UserCache GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                                    if (GroupMember == null)
                                        continue;

                                    if (!Members.Contains(GroupMember))
                                        Members.Add(GroupMember);
                                }

                                /*
                                int FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                                int MembersCount = Members.Count;

                                Session.SendMessage(new GroupMembersComposer(Jobs[0], Members.Take(FinishIndex).ToList(), MembersCount, 1, (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id)), 0, ""));*/
                            //}
                        }
                        #endregion

                        #region DirectJoin

                        #region SendPackets JoinGroupEvent
                        Group.AddMember(TargetClient.GetHabbo().Id);

                        if (Group.GroupType == GroupType.LOCKED)
                        {
                            List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                            foreach (GameClient Client in GroupAdmins)
                            {
                                Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, TargetClient.GetHabbo(), 3));
                            }

                            TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));
                        }
                        else
                        {
                            TargetClient.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(TargetClient.GetHabbo().Id)));
                            TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));

                            if (TargetClient.GetHabbo().CurrentRoom != null)
                                TargetClient.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));
                            else
                                TargetClient.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));

                            if (Group.GroupChatEnabled)
                                TargetClient.SendMessage(new FriendListUpdateComposer(-Group.Id, Group.Id));
                        }
                        TargetClient.GetPlay().JobRequest = false;
                        #endregion

                        //Si es Empresa (PRIVATE)
                        // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                        #region SendPackets AcceptGroupMembershipEvent
                        if (Group.GroupType == GroupType.LOCKED)
                        {
                            if (!Group.HasRequest(UserId))
                                return;

                            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                            if (Habbo == null)
                            {
                                Session.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                                return;
                            }

                            Group.HandleRequest(UserId, true);

                            Session.SendMessage(new GroupMemberUpdatedComposer(jobId, Habbo, 4));

                        }
                        #endregion


                        // Actualizamos Información del Rank del User
                        TargetClient.GetPlay().JobId = jobId;
                        TargetClient.GetPlay().JobRank = jobRank;
                        Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                        // Retornamos Vars
                        TargetClient.GetPlay().JobId = 0;
                        TargetClient.GetPlay().JobRank = 0;
                        #endregion
                    }
                }
                #endregion
            }
            else if(TotalJobs == 2)
            {                
                #region Check Jobs by VIP Type [2 Jobs]
                if (OnlyUpdate)
                {
                    #region Only Update
                    // Actualizamos Información del Rank del User
                    TargetClient.GetPlay().JobId = jobId;
                    TargetClient.GetPlay().JobRank = jobRank;
                    Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                    // Retornamos Vars
                    TargetClient.GetPlay().JobId = 0;
                    TargetClient.GetPlay().JobRank = 0;
                    #endregion
                }
                else
                {
                    if (TargetClient.GetHabbo().VIPRank > 1)
                    {
                        if (Jobs[0].GType == Group.GType)
                        {
                            //Sacar del Jobs[0]
                            #region Sacar
                            RoleplayManager.CheckCorpCarp(TargetClient);
                            int UserId = TargetClient.GetHabbo().Id;
                            if (Jobs[0].IsAdmin(UserId))
                            {
                                Session.SendWhisper("Esta persona era Fundadora de la Empresa " + Jobs[0].Name + ", por lo cual se le ha retirado el puesto para colocarle el Trabajo Asignado.", 1);
                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                                //return;
                            }
                            {
                                Session.SendWhisper("Le has retirado el puesto a " + TargetClient.GetHabbo().Username + " en " + Jobs[0].Name + " para colocarle el Trabajo Asignado.", 1);
                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                            }
                            if (UserId == Session.GetHabbo().Id)
                            {
                                if (Jobs[0].IsMember(UserId))
                                    Jobs[0].DeleteMember(UserId);

                                if (Jobs[0].IsAdmin(UserId))
                                {
                                    if (Jobs[0].IsAdmin(UserId))
                                        Jobs[0].TakeAdmin(UserId);

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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

                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                                    dbClient.AddParameter("GroupId", Jobs[0].Id);
                                    dbClient.AddParameter("UserId", UserId);
                                    dbClient.RunQuery();
                                }

                                Session.SendMessage(new GroupInfoComposer(Jobs[0], Session));
                                if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                                {
                                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                    }

                                    if (Jobs[0].AdminOnlyDeco == 0)
                                    {
                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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
                                            Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[0], User.VirtualId));
                                        Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                    }
                                    else
                                        Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                }
                                //return;
                            }
                            else
                            {
                                //if (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id))
                                //{
                                    if (!Jobs[0].IsMember(UserId))
                                        return;

                                    /*
                                    if (Jobs[0].IsAdmin(UserId) && Jobs[0].CreatorId != Session.GetHabbo().Id)
                                    {
                                        Session.SendNotification("Sorry, only group creators can remove other administrators from the Jobs[0].");
                                        return;
                                    }
                                    */

                                    if (Jobs[0].IsAdmin(UserId))
                                        Jobs[0].TakeAdmin(UserId);

                                    if (Jobs[0].IsMember(UserId))
                                        Jobs[0].DeleteMember(UserId);

                                    List<UserCache> Members = new List<UserCache>();
                                    List<int> MemberIds = Jobs[0].GetAllMembers;
                                    foreach (int Id in MemberIds.ToList())
                                    {
                                        UserCache GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                                        if (GroupMember == null)
                                            continue;

                                        if (!Members.Contains(GroupMember))
                                            Members.Add(GroupMember);
                                    }

                                    /*
                                    int FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                                    int MembersCount = Members.Count;

                                    Session.SendMessage(new GroupMembersComposer(Jobs[0], Members.Take(FinishIndex).ToList(), MembersCount, 1, (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id)), 0, ""));*/
                                //}
                            }
                            #endregion

                            #region DirectJoin

                            #region SendPackets JoinGroupEvent
                            Group.AddMember(TargetClient.GetHabbo().Id);

                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                                foreach (GameClient Client in GroupAdmins)
                                {
                                    Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, TargetClient.GetHabbo(), 3));
                                }

                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));
                            }
                            else
                            {
                                TargetClient.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(TargetClient.GetHabbo().Id)));
                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));

                                if (TargetClient.GetHabbo().CurrentRoom != null)
                                    TargetClient.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));
                                else
                                    TargetClient.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));

                                if (Group.GroupChatEnabled)
                                    TargetClient.SendMessage(new FriendListUpdateComposer(-Group.Id, Group.Id));
                            }
                            TargetClient.GetPlay().JobRequest = false;
                            #endregion

                            //Si es Empresa (PRIVATE)
                            // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                            #region SendPackets AcceptGroupMembershipEvent
                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                UserId = TargetClient.GetHabbo().Id;
                                if (!Group.HasRequest(UserId))
                                    return;

                                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                                if (Habbo == null)
                                {
                                    Session.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                                    return;
                                }

                                Group.HandleRequest(UserId, true);

                                Session.SendMessage(new GroupMemberUpdatedComposer(jobId, Habbo, 4));

                            }
                            #endregion


                            // Actualizamos Información del Rank del User
                            TargetClient.GetPlay().JobId = jobId;
                            TargetClient.GetPlay().JobRank = jobRank;
                            Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                            // Retornamos Vars
                            TargetClient.GetPlay().JobId = 0;
                            TargetClient.GetPlay().JobRank = 0;
                            #endregion
                        }
                        else
                        {
                            //Sacar del Jobs[1]
                            #region Sacar
                            RoleplayManager.CheckCorpCarp(TargetClient);
                            int UserId = TargetClient.GetHabbo().Id;
                            if (Jobs[1].IsAdmin(UserId))
                            {
                                Session.SendWhisper("Esta persona era Fundadora de la Empresa " + Jobs[1].Name + ", por lo cual se le ha retirado el puesto para colocarle el Trabajo Asignado.", 1);
                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[1].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                                //return;
                            }
                            {
                                Session.SendWhisper("Le has retirado el puesto a " + TargetClient.GetHabbo().Username + " en " + Jobs[1].Name + " para colocarle el Trabajo Asignado.", 1);
                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[1].Name + " para colocarte el Trabajo Asignado por " + Session.GetHabbo().Username;
                            }
                            if (UserId == Session.GetHabbo().Id)
                            {
                                if (Jobs[1].IsMember(UserId))
                                    Jobs[1].DeleteMember(UserId);

                                if (Jobs[1].IsAdmin(UserId))
                                {
                                    if (Jobs[1].IsAdmin(UserId))
                                        Jobs[1].TakeAdmin(UserId);

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
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

                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                                    dbClient.AddParameter("GroupId", Jobs[1].Id);
                                    dbClient.AddParameter("UserId", UserId);
                                    dbClient.RunQuery();
                                }

                                Session.SendMessage(new GroupInfoComposer(Jobs[0], Session));
                                if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[1].Id)
                                {
                                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                    }

                                    if (Jobs[1].AdminOnlyDeco == 0)
                                    {
                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
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
                                            Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[0], User.VirtualId));
                                        Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                    }
                                    else
                                        Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                }
                                //return;
                            }
                            else
                            {
                                //if (Jobs[1].CreatorId == Session.GetHabbo().Id || Jobs[1].IsAdmin(Session.GetHabbo().Id))
                                //{
                                    if (!Jobs[1].IsMember(UserId))
                                        return;

                                    /*
                                    if (Jobs[1].IsAdmin(UserId) && Jobs[1].CreatorId != Session.GetHabbo().Id)
                                    {
                                        Session.SendNotification("Sorry, only group creators can remove other administrators from the Jobs[1].");
                                        return;
                                    }
                                    */

                                    if (Jobs[1].IsAdmin(UserId))
                                        Jobs[1].TakeAdmin(UserId);

                                    if (Jobs[1].IsMember(UserId))
                                        Jobs[1].DeleteMember(UserId);

                                    List<UserCache> Members = new List<UserCache>();
                                    List<int> MemberIds = Jobs[1].GetAllMembers;
                                    foreach (int Id in MemberIds.ToList())
                                    {
                                        UserCache GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                                        if (GroupMember == null)
                                            continue;

                                        if (!Members.Contains(GroupMember))
                                            Members.Add(GroupMember);
                                    }

                                    /*
                                    int FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                                    int MembersCount = Members.Count;

                                    Session.SendMessage(new GroupMembersComposer(Jobs[0], Members.Take(FinishIndex).ToList(), MembersCount, 1, (Jobs[1].CreatorId == Session.GetHabbo().Id || Jobs[1].IsAdmin(Session.GetHabbo().Id)), 0, ""));*/
                                //}
                            }
                            #endregion

                            #region DirectJoin

                            #region SendPackets JoinGroupEvent
                            Group.AddMember(TargetClient.GetHabbo().Id);

                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                                foreach (GameClient Client in GroupAdmins)
                                {
                                    Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, TargetClient.GetHabbo(), 3));
                                }

                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));
                            }
                            else
                            {
                                TargetClient.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(TargetClient.GetHabbo().Id)));
                                TargetClient.SendMessage(new GroupInfoComposer(Group, TargetClient));

                                if (TargetClient.GetHabbo().CurrentRoom != null)
                                    TargetClient.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));
                                else
                                    TargetClient.SendMessage(new RefreshFavouriteGroupComposer(TargetClient.GetHabbo().Id));

                                if (Group.GroupChatEnabled)
                                    TargetClient.SendMessage(new FriendListUpdateComposer(-Group.Id, Group.Id));
                            }
                            TargetClient.GetPlay().JobRequest = false;
                            #endregion

                            //Si es Empresa (PRIVATE)
                            // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                            #region SendPackets AcceptGroupMembershipEvent
                            if (Group.GroupType == GroupType.LOCKED)
                            {
                                if (!Group.HasRequest(UserId))
                                    return;

                                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                                if (Habbo == null)
                                {
                                    Session.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                                    return;
                                }

                                Group.HandleRequest(UserId, true);

                                Session.SendMessage(new GroupMemberUpdatedComposer(jobId, Habbo, 4));

                            }
                            #endregion


                            // Actualizamos Información del Rank del User
                            TargetClient.GetPlay().JobId = jobId;
                            TargetClient.GetPlay().JobRank = jobRank;
                            Group.UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                            // Retornamos Vars
                            TargetClient.GetPlay().JobId = 0;
                            TargetClient.GetPlay().JobRank = 0;
                            #endregion
                        }
                    }
                    else
                    {
                        // Quitarle un trabajo de GType 2 puesto que ya no es VIP.
                        if (Jobs[0].GType == 2)
                        {
                            //Sacar del Jobs[0]
                            #region Sacar
                            RoleplayManager.CheckCorpCarp(TargetClient);
                            int UserId = TargetClient.GetHabbo().Id;
                            if (Jobs[0].IsAdmin(UserId))
                            {
                                Session.SendWhisper("Esta persona era Fundadora de la Empresa " + Jobs[0].Name + ", por lo cual se le ha retirado el puesto porque ya no es VIP para tener 2 Trabajos.", 1);
                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name + " por " + Session.GetHabbo().Username + " debido a que ya no eres VIP para tener 2 Trabajos.";
                                //return;
                            }
                            {
                                Session.SendWhisper("Le has retirado el puesto a " + TargetClient.GetHabbo().Username + " en " + Jobs[0].Name + " porque ya no es VIP para tener 2 Trabajos.", 1);
                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name + " por " + Session.GetHabbo().Username + " porque ya no eres VIP para tener 2 Trabajos";
                            }
                            if (UserId == Session.GetHabbo().Id)
                            {
                                if (Jobs[0].IsMember(UserId))
                                    Jobs[0].DeleteMember(UserId);

                                if (Jobs[0].IsAdmin(UserId))
                                {
                                    if (Jobs[0].IsAdmin(UserId))
                                        Jobs[0].TakeAdmin(UserId);

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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

                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                                    dbClient.AddParameter("GroupId", Jobs[0].Id);
                                    dbClient.AddParameter("UserId", UserId);
                                    dbClient.RunQuery();
                                }

                                Session.SendMessage(new GroupInfoComposer(Jobs[0], Session));
                                if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                                {
                                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                    }

                                    if (Jobs[0].AdminOnlyDeco == 0)
                                    {
                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
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
                                            Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[0], User.VirtualId));
                                        Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                    }
                                    else
                                        Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                }
                                //return;
                            }
                            else
                            {
                                //if (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id))
                                //{
                                    if (!Jobs[0].IsMember(UserId))
                                        return;

                                    /*
                                    if (Jobs[0].IsAdmin(UserId) && Jobs[0].CreatorId != Session.GetHabbo().Id)
                                    {
                                        Session.SendNotification("Sorry, only group creators can remove other administrators from the Jobs[0].");
                                        return;
                                    }
                                    */

                                    if (Jobs[0].IsAdmin(UserId))
                                        Jobs[0].TakeAdmin(UserId);

                                    if (Jobs[0].IsMember(UserId))
                                        Jobs[0].DeleteMember(UserId);

                                    List<UserCache> Members = new List<UserCache>();
                                    List<int> MemberIds = Jobs[0].GetAllMembers;
                                    foreach (int Id in MemberIds.ToList())
                                    {
                                        UserCache GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                                        if (GroupMember == null)
                                            continue;

                                        if (!Members.Contains(GroupMember))
                                            Members.Add(GroupMember);
                                    }

                                    /*
                                    int FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                                    int MembersCount = Members.Count;

                                    Session.SendMessage(new GroupMembersComposer(Jobs[0], Members.Take(FinishIndex).ToList(), MembersCount, 1, (Jobs[0].CreatorId == Session.GetHabbo().Id || Jobs[0].IsAdmin(Session.GetHabbo().Id)), 0, ""));*/
                                //}
                            }
                            #endregion
                        }
                        else
                        {
                            //Sacar del Jobs[1]
                            #region Sacar
                            RoleplayManager.CheckCorpCarp(TargetClient);
                            int UserId = TargetClient.GetHabbo().Id;
                            if (Jobs[1].IsAdmin(UserId))
                            {
                                Session.SendWhisper("Esta persona era Fundadora de la Empresa " + Jobs[1].Name + ", por lo cual se le ha retirado el puesto porque ya no es VIP para tener 2 Trabajos.", 1);
                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[1].Name + " por " + Session.GetHabbo().Username + " debido a que ya no eres VIP para tener 2 Trabajos.";
                                //return;
                            }
                            {
                                Session.SendWhisper("Le has retirado el puesto a " + TargetClient.GetHabbo().Username + " en " + Jobs[1].Name + " porque ya no es VIP para tener 2 Trabajos.", 1);
                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[1].Name + " por " + Session.GetHabbo().Username + " porque ya no eres VIP para tener 2 Trabajos";
                            }
                            if (UserId == Session.GetHabbo().Id)
                            {
                                if (Jobs[1].IsMember(UserId))
                                    Jobs[1].DeleteMember(UserId);

                                if (Jobs[1].IsAdmin(UserId))
                                {
                                    if (Jobs[1].IsAdmin(UserId))
                                        Jobs[1].TakeAdmin(UserId);

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
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

                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                                    dbClient.AddParameter("GroupId", Jobs[1].Id);
                                    dbClient.AddParameter("UserId", UserId);
                                    dbClient.RunQuery();
                                }

                                Session.SendMessage(new GroupInfoComposer(Jobs[0], Session));
                                if (Session.GetHabbo().GetStats().FavouriteGroupId == Jobs[1].Id)
                                {
                                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                    }

                                    if (Jobs[1].AdminOnlyDeco == 0)
                                    {
                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
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
                                            Session.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Jobs[0], User.VirtualId));
                                        Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                    }
                                    else
                                        Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                                }
                                //return;
                            }
                            else
                            {
                                //if (Jobs[1].CreatorId == Session.GetHabbo().Id || Jobs[1].IsAdmin(Session.GetHabbo().Id))
                                //{
                                    if (!Jobs[1].IsMember(UserId))
                                        return;

                                    /*
                                    if (Jobs[1].IsAdmin(UserId) && Jobs[1].CreatorId != Session.GetHabbo().Id)
                                    {
                                        Session.SendNotification("Sorry, only group creators can remove other administrators from the Jobs[1].");
                                        return;
                                    }
                                    */

                                    if (Jobs[1].IsAdmin(UserId))
                                        Jobs[1].TakeAdmin(UserId);

                                    if (Jobs[1].IsMember(UserId))
                                        Jobs[1].DeleteMember(UserId);

                                    List<UserCache> Members = new List<UserCache>();
                                    List<int> MemberIds = Jobs[1].GetAllMembers;
                                    foreach (int Id in MemberIds.ToList())
                                    {
                                        UserCache GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                                        if (GroupMember == null)
                                            continue;

                                        if (!Members.Contains(GroupMember))
                                            Members.Add(GroupMember);
                                    }

                                    /*
                                    int FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                                    int MembersCount = Members.Count;

                                    Session.SendMessage(new GroupMembersComposer(Jobs[0], Members.Take(FinishIndex).ToList(), MembersCount, 1, (Jobs[1].CreatorId == Session.GetHabbo().Id || Jobs[1].IsAdmin(Session.GetHabbo().Id)), 0, ""));*/
                                //}
                            }
                            #endregion
                        }
                        return;
                    }
                }
                #endregion
            }
            else
            {
                Session.SendWhisper("[!] Esta personsa tiene " + TotalJobs + " Trabajos. ¡¿WTH?!", 1);
                return;
            }
            //else  Tiene más de 2 Trabajos (Imposible)

            GroupRank JobRank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(jobId, jobRank);
            RoleplayManager.Shout(Session, "*Le otorga a " + TargetClient.GetHabbo().Username + " el Empleo en '" + Group.Name + "' como '" + JobRank.Name + "'*", 5);
            TargetClient.SendWhisper("Has sido contradado por " + Session.GetHabbo().Username + " en '" + Group.Name + "' como '" + JobRank.Name + "'", 1);
            if(ExtraInf.Length > 0)
                TargetClient.SendWhisper(ExtraInf, 1);
            // WS Groups
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_group", "open");
            return;
            #endregion
        }
    }
}
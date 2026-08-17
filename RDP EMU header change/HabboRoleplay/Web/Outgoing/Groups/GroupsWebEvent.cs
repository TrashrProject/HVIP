using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Incoming.Groups;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Cache;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using System.Text.RegularExpressions;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// GroupsWebEvent class.
    /// </summary>
    class GroupsWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            if (Client == null || Client.GetRoomUser() == null || Client.GetRoomUser().RoomId <= 0)
                return;

            /*
            if (!Client.GetPlay().UsingAtm)
            {
                Client.SendNotification("Buen intento, tratando de injectar el systema, ve a un ATM!");
                return;
            }
            */

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            //Generamos la Sala
            Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);            

            switch (Action)
            {

                #region Open
                case "open":
                    {
                        if (Room == null)
                            return;

                        if (Room.Group == null)
                            return;

                        string Founder = "Ninguno";

                        if (Room.Group.GetAdministrator.Count > 0)
                        {
                            Founder = (PlusEnvironment.GetHabboById(Room.Group.GetAdministrators[0]) != null) ? PlusEnvironment.GetHabboById(Room.Group.GetAdministrators[0]).Username : null; // Revisa el Diccionario

                            if (Founder == null)
                                Founder = PlusEnvironment.GetUsernameById(Room.Group.GetAdministrators[0]);// <= Hace SELECT porque puede no estar Online el User a buscar
                        }

                        string SendData = "";
                        SendData += Room.Group.Badge + ";";
                        SendData += Room.Group.Name + ";";
                        SendData += Room.Group.GType + ";";
                        SendData += Room.Group.GroupType + ";";
                        SendData += (Client.GetHabbo().Username == Founder ? "True;" : "False;");
                        SendData += Room.Group.IsMember(Client.GetHabbo().Id) + ";";
                        SendData += Room.Group.HasRequest(Client.GetHabbo().Id) + ";";
                        Socket.Send("compose_group|open|" + SendData);
                        Client.GetPlay().GroupRoom = true;
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetPlay().GroupRoom = false;
                        Socket.Send("compose_group|close|");
                        break;
                    }
                #endregion

                #region Send
                case "send":
                    {
                        if (Room == null)
                            return;

                        if (Room.Group == null)
                            return;

                        if (Client.GetPlay().TryGetCooldown("groupinfo"))
                            return;

                        if (Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                            return;
                        }

                        Client.GetPlay().CooldownManager.CreateCooldown("groupinfo", 1000, 5);                        

                        string Founder = "Ninguno";

                        if (Room.Group.GetAdministrator.Count > 0)
                        {
                            Founder = (PlusEnvironment.GetHabboById(Room.Group.GetAdministrators[0]) != null) ? PlusEnvironment.GetHabboById(Room.Group.GetAdministrators[0]).Username : null; // Revisa el Diccionario

                            if (Founder == null)
                                Founder = PlusEnvironment.GetUsernameById(Room.Group.GetAdministrators[0]);// <= Hace SELECT porque puede no estar Online el User a buscar
                        }

                        bool Mine = (Client.GetHabbo().Username == Founder ? true : false);
                        bool Member = Room.Group.IsMember(Client.GetHabbo().Id);

                        // Si no es dueño del Grupo
                        if (!Mine)
                        {
                            #region If is Job
                            if (Room.Group.GType < 3)
                            {
                                // Verificamos Trabajos Actuales                                
                                //List<Groups.Group> Jobs = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Client.GetHabbo().Id); <= Hace SELECT Directo a DB
                                List<Groups.Group> Jobs = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUserDict(Client.GetHabbo().Id);

                                if (Jobs == null)
                                {
                                    Client.SendWhisper("((Ha ocurrido un Error al Obtener información de tus Trabajos. Contacte con un Administrador. [3]))", 1);
                                    return;
                                }
                                if (Room.Group.Name.Contains("Policía"))
                                {
                                    List<Groups.Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                                    if (Groups != null && Groups.Count > 0)
                                    {
                                        Client.SendWhisper("¡Eres miembro de una banda! No puedes ser contratad@ como policía.", 1);
                                        return;
                                    }
                                }

                                int TotalJobs = Jobs.Count;

                                // Si no es Miembro
                                if (!Member)
                                {
                                    #region Action Point Conditions
                                    Item BTile = null;
                                    BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "actionpoint01" && x.Coordinate == Client.GetRoomUser().Coordinate);
                                    if (BTile == null)
                                    {
                                        Client.SendWhisper("Debes acercarte al punto de petición del Trabajo.", 1);
                                        return;
                                    }
                                    #endregion

                                    if (Room.Group.GroupType == GroupType.OPEN)
                                    {
                                        #region Special Levels Requirement
                                        if (Room.Group.Name.Contains("Armas"))
                                        {
                                            if (Client.GetPlay().Level < 3)
                                            {
                                                Client.SendWhisper("((Necesitas al menos nivel 3 para ser Fabricante de Armas))", 1);
                                                return;
                                            }
                                        }
                                        if (Room.Group.Name.Contains("Hospital"))
                                        {
                                            if (Client.GetPlay().Level < 2)
                                            {
                                                Client.SendWhisper("((Necesitas al menos nivel 2 para ser Médico))", 1);
                                                return;
                                            }
                                        }
                                        #endregion

                                        #region IsWorking
                                        if (Client.GetPlay().IsWorking)
                                        {
                                            WorkManager.RemoveWorkerFromList(Client);
                                            Client.GetPlay().IsWorking = false;
                                            Client.GetHabbo().Poof();

                                        }
                                        if (Client.GetPlay().Ficha > 0)
                                        {
                                            Client.GetPlay().Ficha = 0;
                                        }
                                        #endregion

                                        if (TotalJobs == 0)
                                        {
                                            #region Extra Cost
                                            GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Room.Group.Id, 1);

                                            if (Rank == null)
                                                return;

                                            if (Room.Group.GType == 2 && Rank.Pay > 0)
                                            {
                                                if (Client.GetHabbo().Credits < Rank.Pay)
                                                {
                                                    Client.SendWhisper("Necesitas $ " + Rank.Pay + " de cooperación para poder unirte a este trabajo.");
                                                    return;
                                                }
                                            }
                                            #endregion

                                            #region DirectJoin

                                            #region SendPackets JoinGroupEvent
                                            Room.Group.AddMember(Client.GetHabbo().Id);

                                            Client.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Client.GetHabbo().Id)));
                                            Client.SendMessage(new GroupInfoComposer(Room.Group, Client));

                                            if (Client.GetHabbo().CurrentRoom != null)
                                                Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                            else
                                                Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));

                                            if (Room.Group.GroupChatEnabled)
                                                Client.SendMessage(new FriendListUpdateComposer(-Room.Group.Id, Room.Group.Id));
                                            Client.GetPlay().JobRequest = false;
                                            #endregion

                                            #region RP Job Vars
                                            // Actualizamos Información del Rank del User
                                            Client.GetPlay().JobId = Room.Group.Id;
                                            Client.GetPlay().JobRank = 1;
                                            Room.Group.UpdateInfoJobMember(Client.GetHabbo().Id);

                                            // Retornamos Vars
                                            Client.GetPlay().JobId = 0;
                                            Client.GetPlay().JobRank = 0;
                                            #endregion

                                            #endregion

                                            RoleplayManager.Shout(Client, "*Ha conseguido el Trabajo de " + Room.Group.Name + "*", 5);
                                            Client.SendWhisper("¡Muy bien! Ahora tienes comandos nuevos para tu nuevo trabajo. Usa :ayuda para consultarlos.", 1);

                                            if (Room.Group.GType == 2 && Rank.Pay > 0)
                                            {
                                                Client.GetHabbo().Credits -= Rank.Pay;
                                                Client.GetHabbo().UpdateCreditsBalance();
                                                Client.SendWhisper("Has pagado $ " + Rank.Pay + " de cooperación para el trabajo.", 1);
                                            }
                                        }
                                        else if (TotalJobs == 1)
                                        {
                                            #region Check Jobs by VIP Type
                                            if (Client.GetHabbo().VIPRank > 1)
                                            {
                                                if (Jobs[0].GType != Room.Group.GType)
                                                {
                                                    #region Extra Cost
                                                    GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Room.Group.Id, 1);

                                                    if (Rank == null)
                                                        return;

                                                    if (Room.Group.GType == 2 && Rank.Pay > 0)
                                                    {
                                                        if (Client.GetHabbo().Credits < Rank.Pay)
                                                        {
                                                            Client.SendWhisper("Necesitas $ " + Rank.Pay + " de cooperación para poder unirte a este trabajo.");
                                                            return;
                                                        }
                                                    }
                                                    #endregion

                                                    #region DirectJoin

                                                    #region SendPackets JoinGroupEvent
                                                    Room.Group.AddMember(Client.GetHabbo().Id);

                                                    Client.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Client.GetHabbo().Id)));
                                                    Client.SendMessage(new GroupInfoComposer(Room.Group, Client));

                                                    if (Client.GetHabbo().CurrentRoom != null)
                                                        Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                    else
                                                        Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));

                                                    if (Room.Group.GroupChatEnabled)
                                                        Client.SendMessage(new FriendListUpdateComposer(-Room.Group.Id, Room.Group.Id));
                                                    Client.GetPlay().JobRequest = false;
                                                    #endregion

                                                    #region RP Job Vars
                                                    // Actualizamos Información del Rank del User
                                                    Client.GetPlay().JobId = Room.Group.Id;
                                                    Client.GetPlay().JobRank = 1;
                                                    Room.Group.UpdateInfoJobMember(Client.GetHabbo().Id);

                                                    // Retornamos Vars
                                                    Client.GetPlay().JobId = 0;
                                                    Client.GetPlay().JobRank = 0;
                                                    #endregion

                                                    #endregion

                                                    RoleplayManager.Shout(Client, "*Ha conseguido el Trabajo de " + Room.Group.Name + "*", 5);
                                                    Client.SendWhisper("¡Muy bien! Ahora tienes comandos nuevos para tu nuevo trabajo. Usa :ayuda para consultarlos.", 1);

                                                    if (Room.Group.GType == 2 && Rank.Pay > 0)
                                                    {
                                                        Client.GetHabbo().Credits -= Rank.Pay;
                                                        Client.GetHabbo().UpdateCreditsBalance();
                                                        Client.SendWhisper("Has pagado $ " + Rank.Pay + " de cooperación para el trabajo.", 1);
                                                    }
                                                }
                                                else
                                                {
                                                    Client.SendWhisper("Primero debes renunciar a tu Trabajo actual usando ':renunciar " + Jobs[0].Id + "'.", 1);
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Client.SendWhisper("Ya tienes un trabajo. Debes renunciar a tu Trabajo actual usando ':renunciar " + Jobs[0].Id + "'. ¡Con VIP puedes tener 2 trabajos a la vez!", 1);
                                                return;
                                            }
                                            #endregion
                                        }
                                        else if (TotalJobs == 2)
                                        {
                                            #region Check Jobs by VIP Type [2 Jobs]
                                            if (Client.GetHabbo().VIPRank > 1)
                                            {
                                                int idjob = (Jobs[0].GType == Room.Group.GType) ? Jobs[0].Id : Jobs[1].Id;
                                                Client.SendWhisper("Primero debes renunciar a tu Trabajo actual usando ':renunciar " + idjob + "'.", 1);
                                                return;
                                            }
                                            else
                                            {
                                                string ExtraInf = "";
                                                // Quitarle un trabajo de GType = 2 puesto que ya no es VIP.
                                                if (Jobs[0].GType == 2)
                                                {
                                                    //Sacar del Jobs[0]
                                                    #region Sacar
                                                    int UserId = Client.GetHabbo().Id;
                                                    if (Jobs[0].IsAdmin(UserId))
                                                    {
                                                        ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name + " debido a que ya no eres VIP para tener 2 Trabajos.";
                                                    }
                                                    {
                                                        ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name + " porque ya no eres VIP para tener 2 Trabajos";
                                                    }

                                                    if (Jobs[0].IsMember(UserId))
                                                        Jobs[0].DeleteMember(UserId);

                                                    if (Jobs[0].IsAdmin(UserId))
                                                    {
                                                        if (Jobs[0].IsAdmin(UserId))
                                                            Jobs[0].TakeAdmin(UserId);

                                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
                                                            return;

                                                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
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

                                                    Client.SendMessage(new GroupInfoComposer(Jobs[0], Client));
                                                    if (Client.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                                                    {
                                                        Client.GetHabbo().GetStats().FavouriteGroupId = 0;
                                                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                                        {
                                                            dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                                        }

                                                        if (Jobs[0].AdminOnlyDeco == 0)
                                                        {
                                                            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
                                                                return;

                                                            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                            if (User != null)
                                                            {
                                                                User.RemoveStatus("flatctrl 1");
                                                                User.UpdateNeeded = true;

                                                                if (User.GetClient() != null)
                                                                    User.GetClient().SendMessage(new YouAreControllerComposer(0));
                                                            }
                                                        }

                                                        if (Client.GetHabbo().InRoom && Client.GetHabbo().CurrentRoom != null)
                                                        {
                                                            RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                            if (User != null)
                                                                Client.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, Jobs[0], User.VirtualId));
                                                            Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                        }
                                                        else
                                                            Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    //Sacar del Jobs[1]
                                                    #region Sacar
                                                    int UserId = Client.GetHabbo().Id;
                                                    if (Jobs[1].IsAdmin(UserId))
                                                    {
                                                        ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[1].Name + " debido a que ya no eres VIP para tener 2 Trabajos.";
                                                    }
                                                    else
                                                    {
                                                        ExtraInf = "Se te ha retirado el trabajo de " + Jobs[1].Name + " porque ya no eres VIP para tener 2 Trabajos";
                                                    }

                                                    if (Jobs[1].IsMember(UserId))
                                                        Jobs[1].DeleteMember(UserId);

                                                    if (Jobs[1].IsAdmin(UserId))
                                                    {
                                                        if (Jobs[1].IsAdmin(UserId))
                                                            Jobs[1].TakeAdmin(UserId);

                                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
                                                            return;

                                                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
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

                                                    Client.SendMessage(new GroupInfoComposer(Jobs[1], Client));
                                                    if (Client.GetHabbo().GetStats().FavouriteGroupId == Jobs[1].Id)
                                                    {
                                                        Client.GetHabbo().GetStats().FavouriteGroupId = 0;
                                                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                                        {
                                                            dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                                        }

                                                        if (Jobs[1].AdminOnlyDeco == 0)
                                                        {
                                                            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
                                                                return;

                                                            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                            if (User != null)
                                                            {
                                                                User.RemoveStatus("flatctrl 1");
                                                                User.UpdateNeeded = true;

                                                                if (User.GetClient() != null)
                                                                    User.GetClient().SendMessage(new YouAreControllerComposer(0));
                                                            }
                                                        }

                                                        if (Client.GetHabbo().InRoom && Client.GetHabbo().CurrentRoom != null)
                                                        {
                                                            RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                            if (User != null)
                                                                Client.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, Jobs[1], User.VirtualId));
                                                            Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                        }
                                                        else
                                                            Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                    }
                                                    #endregion
                                                }
                                                Client.SendWhisper(ExtraInf, 1);
                                                return;
                                            }
                                            #endregion
                                        }

                                        RoleplayManager.CheckCorpCarp(Client);
                                        Client.GetPlay().CamCargId = 0;
                                    }
                                    else if (Room.Group.GroupType == GroupType.LOCKED)
                                    {
                                        if (Room.Group.HasRequest(Client.GetHabbo().Id))
                                        {
                                            Client.SendWhisper("¡Ya has mandado una Solicitud! Por favor espera a que sea respondida.", 1);
                                            return;
                                        }

                                        GroupRank Rank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Room.Group.Id, 1);

                                        string SendDatas = "";
                                        SendDatas += Room.Group.Name + ";";
                                        SendDatas += Room.Group.Badge + ";";
                                        SendDatas += Founder + ";";
                                        SendDatas += Rank.Name + ";";
                                        SendDatas += Rank.Pay.ToString("C") + ";";
                                        SendDatas += Rank.Timer + " minutos;";
                                        SendDatas += Room.Group.GType + ";";
                                        Socket.Send("compose_group|solicitud|" + SendDatas);
                                        Client.GetPlay().GroupRoom = true;
                                    }
                                }
                                // Dejar Grupo
                                else
                                {
                                    #region IsWorking
                                    if (Client.GetPlay().IsWorking)
                                    {
                                        WorkManager.RemoveWorkerFromList(Client);
                                        Client.GetPlay().IsWorking = false;
                                        Client.GetHabbo().Poof();

                                    }
                                    if (Client.GetPlay().Ficha > 0)
                                    {
                                        Client.GetPlay().Ficha = 0;
                                    }
                                    #endregion

                                    string ExtraInf = "";

                                    if (TotalJobs == 1)
                                    {
                                        //Sacar del Jobs[0]
                                        #region Sacar
                                        int UserId = Client.GetHabbo().Id;
                                        if (Jobs[0].IsAdmin(UserId))
                                        {
                                            ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name;
                                        }
                                        {
                                            ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name;
                                        }

                                        if (Jobs[0].IsMember(UserId))
                                            Jobs[0].DeleteMember(UserId);

                                        if (Jobs[0].IsAdmin(UserId))
                                        {
                                            if (Jobs[0].IsAdmin(UserId))
                                                Jobs[0].TakeAdmin(UserId);

                                            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
                                                return;

                                            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
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

                                        Client.SendMessage(new GroupInfoComposer(Jobs[0], Client));
                                        if (Client.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                                        {
                                            Client.GetHabbo().GetStats().FavouriteGroupId = 0;
                                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                            {
                                                dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                            }

                                            if (Jobs[0].AdminOnlyDeco == 0)
                                            {
                                                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
                                                    return;

                                                RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                if (User != null)
                                                {
                                                    User.RemoveStatus("flatctrl 1");
                                                    User.UpdateNeeded = true;

                                                    if (User.GetClient() != null)
                                                        User.GetClient().SendMessage(new YouAreControllerComposer(0));
                                                }
                                            }

                                            if (Client.GetHabbo().InRoom && Client.GetHabbo().CurrentRoom != null)
                                            {
                                                RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                if (User != null)
                                                    Client.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, Jobs[0], User.VirtualId));
                                                Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                            }
                                            else
                                                Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                        }
                                        #endregion

                                        RoleplayManager.Shout(Client, "*Ha renunciado a su trabajo de " + Room.Group.Name + "*", 5);

                                        if (ExtraInf != "")
                                            Client.SendWhisper(ExtraInf, 1);

                                    }
                                    else if (TotalJobs == 2)
                                    {
                                        if (Jobs[0] == Room.Group)
                                        {
                                            //Sacar del Jobs[0]
                                            #region Sacar
                                            int UserId = Client.GetHabbo().Id;
                                            if (Jobs[0].IsAdmin(UserId))
                                            {
                                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name;
                                            }
                                            {
                                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name;
                                            }

                                            if (Jobs[0].IsMember(UserId))
                                                Jobs[0].DeleteMember(UserId);

                                            if (Jobs[0].IsAdmin(UserId))
                                            {
                                                if (Jobs[0].IsAdmin(UserId))
                                                    Jobs[0].TakeAdmin(UserId);

                                                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
                                                    return;

                                                RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
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

                                            Client.SendMessage(new GroupInfoComposer(Jobs[0], Client));
                                            if (Client.GetHabbo().GetStats().FavouriteGroupId == Jobs[0].Id)
                                            {
                                                Client.GetHabbo().GetStats().FavouriteGroupId = 0;
                                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                                {
                                                    dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                                }

                                                if (Jobs[0].AdminOnlyDeco == 0)
                                                {
                                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[0].RoomId, out Room))
                                                        return;

                                                    RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                    if (User != null)
                                                    {
                                                        User.RemoveStatus("flatctrl 1");
                                                        User.UpdateNeeded = true;

                                                        if (User.GetClient() != null)
                                                            User.GetClient().SendMessage(new YouAreControllerComposer(0));
                                                    }
                                                }

                                                if (Client.GetHabbo().InRoom && Client.GetHabbo().CurrentRoom != null)
                                                {
                                                    RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                    if (User != null)
                                                        Client.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, Jobs[0], User.VirtualId));
                                                    Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                }
                                                else
                                                    Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                            }
                                            #endregion

                                            RoleplayManager.Shout(Client, "*Ha renunciado a su trabajo de " + Room.Group.Name + "*", 5);

                                            if (ExtraInf != "")
                                                Client.SendWhisper(ExtraInf, 1);
                                        }
                                        // Jobs[1]
                                        else
                                        {
                                            //Sacar del Jobs[1]
                                            #region Sacar
                                            int UserId = Client.GetHabbo().Id;
                                            if (Jobs[1].IsAdmin(UserId))
                                            {
                                                ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[1].Name;
                                            }
                                            {
                                                ExtraInf = "Se te ha retirado el trabajo de " + Jobs[1].Name;
                                            }

                                            if (Jobs[1].IsMember(UserId))
                                                Jobs[1].DeleteMember(UserId);

                                            if (Jobs[1].IsAdmin(UserId))
                                            {
                                                if (Jobs[1].IsAdmin(UserId))
                                                    Jobs[1].TakeAdmin(UserId);

                                                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
                                                    return;

                                                RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
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

                                            Client.SendMessage(new GroupInfoComposer(Jobs[1], Client));
                                            if (Client.GetHabbo().GetStats().FavouriteGroupId == Jobs[1].Id)
                                            {
                                                Client.GetHabbo().GetStats().FavouriteGroupId = 0;
                                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                                {
                                                    dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                                }

                                                if (Jobs[1].AdminOnlyDeco == 0)
                                                {
                                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Jobs[1].RoomId, out Room))
                                                        return;

                                                    RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                    if (User != null)
                                                    {
                                                        User.RemoveStatus("flatctrl 1");
                                                        User.UpdateNeeded = true;

                                                        if (User.GetClient() != null)
                                                            User.GetClient().SendMessage(new YouAreControllerComposer(0));
                                                    }
                                                }

                                                if (Client.GetHabbo().InRoom && Client.GetHabbo().CurrentRoom != null)
                                                {
                                                    RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                                    if (User != null)
                                                        Client.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, Jobs[1], User.VirtualId));
                                                    Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                }
                                                else
                                                    Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                            }
                                            #endregion

                                            RoleplayManager.Shout(Client, "*Ha renunciado a su trabajo de " + Room.Group.Name + "*", 5);

                                            if (ExtraInf != "")
                                                Client.SendWhisper(ExtraInf, 1);
                                        }
                                    }

                                    RoleplayManager.CheckCorpCarp(Client);
                                    Client.GetPlay().CamCargId = 0;
                                }
                            }
                            #endregion

                            #region If is Gang
                            else
                            {
                                List<Groups.Group> Gangs = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                                if (Gangs == null)
                                {
                                    Client.SendWhisper("((Ha ocurrido un Error al Obtener información de tus bandas. Contacte con un Administrador. [3]))", 1);
                                    return;
                                }

                                // Si no es Miembro
                                if (!Member)
                                {
                                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
                                    {
                                        Client.SendWhisper("¡No puedes pertenecer a una banda y ser policía a la vez!", 1);

                                    }
                                    else
                                    {
                                        if (Room.Group.GroupType == GroupType.OPEN)
                                        {
                                            if (Gangs.Count <= 0)
                                            {
                                                #region DirectJoin

                                                #region SendPackets JoinGroupEvent
                                                Room.Group.AddMember(Client.GetHabbo().Id);

                                                Client.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Client.GetHabbo().Id)));
                                                Client.SendMessage(new GroupInfoComposer(Room.Group, Client));

                                                if (Client.GetHabbo().CurrentRoom != null)
                                                    Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                                else
                                                    Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));

                                                if (Room.Group.GroupChatEnabled)
                                                    Client.SendMessage(new FriendListUpdateComposer(-Room.Group.Id, Room.Group.Id));
                                                Client.GetPlay().JobRequest = false;
                                                #endregion

                                                #region RP Job Vars
                                                // Actualizamos Información del Rank del User
                                                Client.GetPlay().JobId = Room.Group.Id;
                                                Client.GetPlay().JobRank = 1;
                                                Room.Group.UpdateInfoJobMember(Client.GetHabbo().Id);

                                                // Retornamos Vars
                                                Client.GetPlay().JobId = 0;
                                                Client.GetPlay().JobRank = 0;
                                                #endregion

                                                #endregion

                                                RoleplayManager.Shout(Client, "*Ha ingresado a la banda " + Room.Group.Name + "*", 5);
                                                Client.SendWhisper("¡Muy bien! Ahora perteneces a una nueva banda. ((Da clic en su emblema para ver más info.))", 1);
                                            }

                                        }
                                        else if (Room.Group.GroupType == GroupType.LOCKED)
                                        {
                                            if (Room.Group.HasRequest(Client.GetHabbo().Id))
                                            {
                                                Client.SendWhisper("¡Ya has mandado una Solicitud! Por favor espera a que sea respondida.", 1);
                                                return;
                                            }

                                            Client.GetPlay().BuyingCorp = false;
                                            Room.Group.AddMember(Client.GetHabbo().Id, 1, true);// Metemos directo a db por seguridad y evitar bugs

                                            List<GameClient> GroupAdmins = (from Clients in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Clients != null && Clients.GetHabbo() != null && Room.Group.IsAdmin(Clients.GetHabbo().Id) select Clients).ToList();
                                            foreach (GameClient Clients in GroupAdmins)
                                            {
                                                Client.SendMessage(new GroupMembershipRequestedComposer(Room.Group.Id, Client.GetHabbo(), 3));
                                            }
                                            Client.SendMessage(new GroupInfoComposer(Room.Group, Client));
                                            Client.GetPlay().JobRequest = false;

                                            RoleplayManager.Shout(Client, "*Ha solicitado ingresar a la banda " + Room.Group.Name + "*", 5);
                                            Client.SendMessage(new RoomNotificationComposer("gang_request_warning", "message", "¡Bien Hecho!\nAhora debes esperar a que aprueben tu solictud.\n\nToma en cuenta que si te encuentras en otra banda; el Líder de la nueva banda no podrá aceptarte hasta que abandones dicha banda anterior."));
                                            // Ws Groups
                                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                                        }
                                    }
                                }
                                // Dejar Grupo
                                else
                                {
                                    string ExtraInf = "";
                                    #region Sacar
                                    int UserId = Client.GetHabbo().Id;
                                    if (Gangs[0].IsAdmin(UserId))
                                    {
                                        Client.SendWhisper("No puedes abandonar tu propia banda sin dejar a alguien al mando. O bien, puedes eliminarla desde tu panel de gestión.", 1);
                                        return;
                                    }
                                    {
                                        ExtraInf = "Has abandonado tu banda";
                                    }

                                    if (Gangs[0].IsMember(UserId))
                                        Gangs[0].DeleteMember(UserId);

                                    if (Gangs[0].IsAdmin(UserId))
                                    {
                                        if (Gangs[0].IsAdmin(UserId))
                                            Gangs[0].TakeAdmin(UserId);

                                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Gangs[0].RoomId, out Room))
                                            return;

                                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
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
                                        dbClient.AddParameter("GroupId", Gangs[0].Id);
                                        dbClient.AddParameter("UserId", UserId);
                                        dbClient.RunQuery();
                                    }

                                    Client.SendMessage(new GroupInfoComposer(Gangs[0], Client));
                                    if (Client.GetHabbo().GetStats().FavouriteGroupId == Gangs[0].Id)
                                    {
                                        Client.GetHabbo().GetStats().FavouriteGroupId = 0;
                                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                        {
                                            dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                                        }

                                        if (Gangs[0].AdminOnlyDeco == 0)
                                        {
                                            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Gangs[0].RoomId, out Room))
                                                return;

                                            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                            if (User != null)
                                            {
                                                User.RemoveStatus("flatctrl 1");
                                                User.UpdateNeeded = true;

                                                if (User.GetClient() != null)
                                                    User.GetClient().SendMessage(new YouAreControllerComposer(0));
                                            }
                                        }

                                        if (Client.GetHabbo().InRoom && Client.GetHabbo().CurrentRoom != null)
                                        {
                                            RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                                            if (User != null)
                                                Client.GetHabbo().CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, Gangs[0], User.VirtualId));
                                            Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                        }
                                        else
                                            Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                                    }
                                    #endregion

                                    RoleplayManager.Shout(Client, "*Ha abandonado la banda " + Room.Group.Name + "*", 5);
                                    Client.SendWhisper(ExtraInf, 1);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Gestionar
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "open_room");
                        }

                        string SendData = "";
                        SendData += Room.Group.Badge + ";";
                        SendData += Room.Group.Name + ";";
                        SendData += Room.Group.GType + ";";
                        SendData += Room.Group.GroupType + ";";
                        SendData += (Client.GetHabbo().Username == Founder ? "True;" : "False;");
                        SendData += Room.Group.IsMember(Client.GetHabbo().Id) + ";";
                        SendData += Room.Group.HasRequest(Client.GetHabbo().Id) + ";";
                        Socket.Send("compose_group|open|" + SendData);
                        Client.GetPlay().GroupRoom = true;
                    }
                    break;
                #endregion

                #region Request
                case "request":
                    {
                        if (Room == null)
                            return;

                        if (Room.Group == null)
                            return;

                        if (Client.GetPlay().TryGetCooldown("grouprequest"))
                            return;

                        Client.GetPlay().CooldownManager.CreateCooldown("grouprequest", 1000, 5);

                        // No se supone que esto entre, pero por seguridad...
                        if (Room.Group.HasRequest(Client.GetHabbo().Id))
                        {
                            Socket.Send("compose_group|error|¡Ya has mandado una solicitud!");
                            return;
                        }
                        if (Room.Group.Name.Contains("Policía"))
                        {
                            List<Groups.Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                            if (Groups != null && Groups.Count > 0)
                            {
                                Socket.Send("compose_group|error|¡Eres miembro de una banda! No puedes ser contratad@ como policía.");
                                return;
                            }
                        }

                        string[] ReceivedData = Data.Split(',');
                        int Hours;

                        if (!int.TryParse(ReceivedData[1], out Hours))
                        {
                            Socket.Send("compose_group|error|Debe ingresar una hora válida.");
                            return;
                        }
                        string Desc = ReceivedData[2];
                        string Region = ReceivedData[3];
                        if (Desc.Length > 250)
                        {
                            Socket.Send("compose_group|error|La explicación no debe exceder los 250 Caracteres.");
                            return;
                        }
                        if (Region.Length > 10)
                        {
                            Socket.Send("compose_group|error|País Erróneo.");
                            return;
                        }

                        // Filter
                        Desc = Regex.Replace(Desc, "<(.|\\n)*?>", string.Empty);
                        Region = Regex.Replace(Region, "<(.|\\n)*?>", string.Empty);

                        Client.GetPlay().BuyingCorp = false;
                        Room.Group.AddMember(Client.GetHabbo().Id, 1, true, Hours, Desc, Region);// Metemos directo a db por seguridad y evitar bugs

                        List<GameClient> GroupAdmins = (from Clients in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Clients != null && Clients.GetHabbo() != null && Room.Group.IsAdmin(Clients.GetHabbo().Id) select Clients).ToList();
                        foreach (GameClient Clients in GroupAdmins)
                        {
                            Client.SendMessage(new GroupMembershipRequestedComposer(Room.Group.Id, Client.GetHabbo(), 3));
                        }
                        Client.SendMessage(new GroupInfoComposer(Room.Group, Client));
                        Client.GetPlay().JobRequest = false;
                        RoleplayManager.Shout(Client, "*Ha enviado una solicitud de Empleo a " + Room.Group.Name + "*", 5);
                        Client.SendMessage(new RoomNotificationComposer("job_request_warning", "message", "¡Bien Hecho!\nAhora debes esperar a que aprueben tu solictud.\n\nToma en cuenta que si te encuentras en otro trabajo que también requirió solicitud de empleo; el Fundador del nuevo trabajo no podrá aceptarte hasta que renuncies a dicho trabajo anterior."));
                        // Ws Groups
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                        Socket.Send("compose_group|close_rq|");
                        
                        break;
                    }
                #endregion
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;

using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Global;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms.Games;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Inventory;
using Plus.Communication.Packets.Incoming;

using Plus.Utilities;

using System.Data;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Handshake;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Rooms.Games.Teams;

using Plus.Database.Interfaces;
using Plus.HabboHotel.Rewards.Rooms.AI.Types;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUserManager
    {
        private Room _room;
        private ConcurrentDictionary<int, RoomUser> _users;
        private ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;

        private int primaryPrivateUserID;
        private int secondaryPrivateUserID;

        public int userCount;
        private int petCount;
        
        public RoomUserManager(Room room)
        {
            this._room = room;
            this._users = new ConcurrentDictionary<int, RoomUser>();
            this._pets = new ConcurrentDictionary<int, RoomUser>();
            this._bots = new ConcurrentDictionary<int, RoomUser>();

            this.primaryPrivateUserID = 0;
            this.secondaryPrivateUserID = 0;

            this.petCount = 0;
            this.userCount = 0;
        }

        public void Dispose()
        {
            this._users.Clear();
            this._pets.Clear();
            this._bots.Clear();

            this._users = null;
            this._pets = null;
            this._bots = null;
        }

        public RoomUser DeployTaxiBot(RoomBot Bot)
        {
            var BotUser = new RoomUser(0, _room.RoomId, primaryPrivateUserID++, _room);
            Bot.VirtualId = primaryPrivateUserID;

            int PersonalID = secondaryPrivateUserID++;
            BotUser.InternalRoomID = PersonalID;
            _users.TryAdd(PersonalID, BotUser);

            BotUser.SetPos(Bot.X, Bot.Y, Bot.Z);
            BotUser.SetRot(Bot.Rot, false);

            BotUser.BotData = Bot;
            BotUser.BotAI = Bot.GenerateBotAI(BotUser.VirtualId);
    
            BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);

            //UpdateUserStatus(BotUser, false);
            BotUser.UpdateNeeded = true;

            _room.SendMessage(new UsersComposer(BotUser));

            if (_bots.ContainsKey(BotUser.BotData.BotId))
                _bots[BotUser.BotData.BotId] = BotUser;
            else
                _bots.TryAdd(BotUser.BotData.Id, BotUser);

            return BotUser;
        }

        public RoomUser DeployBot(RoomBot Bot, Pet PetData)
        {
            var BotUser = new RoomUser(0, _room.RoomId, primaryPrivateUserID++, _room);
            Bot.VirtualId = primaryPrivateUserID;

            int PersonalID = secondaryPrivateUserID++;
            BotUser.InternalRoomID = PersonalID;
            _users.TryAdd(PersonalID, BotUser);

            DynamicRoomModel Model = _room.GetGameMap().Model;

            if ((Bot.X > 0 && Bot.Y > 0) && Bot.X < Model.MapSizeX && Bot.Y < Model.MapSizeY)
            {
                BotUser.SetPos(Bot.X, Bot.Y, Bot.Z);
                BotUser.SetRot(Bot.Rot, false);
            }
            else
            {
                Bot.X = Model.DoorX;
                Bot.Y = Model.DoorY;

                BotUser.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                BotUser.SetRot(Model.DoorOrientation, false);
            }

            BotUser.BotData = Bot;
            BotUser.BotAI = Bot.GenerateBotAI(BotUser.VirtualId);

            if (BotUser.IsPet)
            {
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);
                BotUser.PetData = PetData;
                BotUser.PetData.VirtualId = BotUser.VirtualId;
            }
            else
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);

            //UpdateUserStatus(BotUser, false);
            BotUser.UpdateNeeded = true;

            _room.SendMessage(new UsersComposer(BotUser));

            if (BotUser.IsPet)
            {
                if (_pets.ContainsKey(BotUser.PetData.PetId)) //Pet allready placed
                    _pets[BotUser.PetData.PetId] = BotUser;
                else
                    _pets.TryAdd(BotUser.PetData.PetId, BotUser);

                petCount++;
            }
            else if (BotUser.IsBot)
            {
                if (_bots.ContainsKey(BotUser.BotData.BotId))
                    _bots[BotUser.BotData.BotId] = BotUser;
                else
                    _bots.TryAdd(BotUser.BotData.Id, BotUser);
                _room.SendMessage(new DanceComposer(BotUser, BotUser.BotData.DanceId));
            }
            return BotUser;
        }

        public void RemoveBot(int VirtualId, bool Kicked)
        {
            RoomUser User = GetRoomUserByVirtualId(VirtualId);
            if (User == null || !User.IsBot)
                return;

            if (User.IsPet)
            {
                RoomUser PetRemoval = null;

                _pets.TryRemove(User.PetData.PetId, out PetRemoval);
                petCount--;
            }
            else
            {
                RoomUser BotRemoval = null;
                _bots.TryRemove(User.BotData.Id, out BotRemoval);
            }



            User.BotAI.OnSelfLeaveRoom(Kicked);

            _room.SendMessage(new UserRemoveComposer(User.VirtualId));

            RoomUser toRemove;

            if (_users != null)
                _users.TryRemove(User.InternalRoomID, out toRemove);

            onRemove(User);
        }

        public RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        public bool AddAvatarToRoom(GameClient Session)
        {
            if (_room == null)
                return false;

            if (Session == null || Session.GetHabbo() == null)
                return false;

            if (Session.GetHabbo().CurrentRoom == null)
                return false;

            #region Forcing WebSockets connection
            // Refrescamos WS
            Session.GetPlay().UpdateInteractingUserDialogues();
            Session.GetPlay().RefreshStatDialogue();

            Session.GetPlay().InitWSDialogues();
            Session.GetPlay().InitStatDialogue();

            // Refrescamos WS del Celular
            if (Session.GetPlay().Phone > 0)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "load_apps");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "show_button");
            }
            #endregion

            #region Old Stuff
            RoomUser User = new RoomUser(Session.GetHabbo().Id, _room.RoomId, primaryPrivateUserID++, _room);

            if (User == null || User.GetClient() == null)
                return false;

            User.UserId = Session.GetHabbo().Id;

            Session.GetHabbo().TentId = 0;

            int PersonalID = secondaryPrivateUserID++;
            User.InternalRoomID = PersonalID;


            Session.GetHabbo().CurrentRoomId = _room.RoomId;
            if (!this._users.TryAdd(PersonalID, User))
                return false;
            #endregion

            DynamicRoomModel Model = _room.GetGameMap().Model;
            if (Model == null)
                return false;

            if (!_room.PetMorphsAllowed && Session.GetHabbo().PetId != 0)
                Session.GetHabbo().PetId = 0;

            #region Roleplay Entering Pasajero OFF
            /*
            if (Session.GetPlay().Pasajero)
            {
                RoomUser user = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(User.GetClient().GetPlay().ChoferID);
                if (user != null)
                {
                    if (_room.GetGameMap().isInMap(user.X, user.Y))
                    {
                        User.GetClient().GetHabbo().warp(user.SetX, user.SetY);
                    }
                }
            }
            */
            #endregion

            if (!Session.GetHabbo().IsTeleporting && !Session.GetHabbo().IsHopping)
            {
                if (!Model.DoorIsValid())
                {
                    if (_room.DoorX < 0)
                    {
                        Point Square = _room.GetGameMap().getRandomWalkableSquare();
                        Model.DoorX = Square.X;
                        Model.DoorY = Square.Y;
                        Model.DoorZ = _room.GetGameMap().GetHeightForSquareFromData(Square);
                    }
                    else
                    {
                        Model.DoorX = _room.RoomData.DoorX;
                        Model.DoorY = _room.RoomData.DoorY;
                        Model.DoorZ = _room.RoomData.DoorZ;
                    }
                }

                if(_room.RoomData.DoorX > -1)
                {
                    Model.DoorX = _room.RoomData.DoorX;
                    Model.DoorY = _room.RoomData.DoorY;
                    Model.DoorZ = _room.RoomData.DoorZ;
                    Model.DoorOrientation = _room.RoomData.DoorOrientation;
                }

                #region Roleplay last spawn coordination
                
                if (!Session.GetPlay().AntiArrowCheck && Session.GetHabbo().IsLogin)
                {
                    object[] Coords = Session.GetPlay().LastCoordinates.Split(',');

                    int LastX = Convert.ToInt32(Coords[0]);
                    int LastY = Convert.ToInt32(Coords[1]);
                    double LastZ = Convert.ToDouble(Coords[2]);
                    int LastRot = Convert.ToInt32(Coords[3]);

                    if (_room.GetGameMap().isInMap(LastX, LastY))
                    {
                        if (LastX == 0 && LastY == 0)
                        {
                            User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                            User.SetRot(Model.DoorOrientation, false);
                        }
                        else
                        {
                            User.SetPos(LastX, LastY, LastZ);
                            User.SetRot(LastRot, false);
                            UpdateUserStatus(User, false);
                        }
                    }
                    else
                    {
                        User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }
                    Session.GetHabbo().IsLogin = false;
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                    User.SetRot(Model.DoorOrientation, false);
                }

                #endregion

                #region Roleplay Exiting Houses
                if (Session.GetPlay().ExitingHouse)
                {
                    int LastX = Session.GetPlay().HouseX;
                    int LastY = Session.GetPlay().HouseY;
                    double LastZ = Session.GetPlay().HouseZ;

                    if (LastX == 0 && LastY == 0)
                    {
                        User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }
                    else
                    {
                        User.SetPos(LastX, LastY, LastZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }

                    UpdateUserStatus(User, false);

                    Session.GetPlay().ExitingHouse = false;
                    Session.GetPlay().HouseX = 0;
                    Session.GetPlay().HouseY = 0;
                    Session.GetPlay().HouseZ = 0;                   

                    /* No hace falta validar si está en mapa, pues DEV se encarga de establecer las coords
                     * de puertas en play_houses.
                    if (_room.GetGameMap().isInMap(LastX, LastY))
                    {
                        if (LastX == 0 && LastY == 0)
                        {
                            User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                            User.SetRot(Model.DoorOrientation, false);
                        }
                        else
                        {
                            User.SetPos(LastX, LastY, LastZ);
                            //User.SetRot(LastRot, false);
                            UpdateUserStatus(User, false);
                        }
                    }
                    else
                    {
                        User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }
                    */
                }
                #endregion

                // User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                // User.SetRot(Model.DoorOrientation, false);

                #region Loader 100%
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_loader|4");// 4 = Cantidad a sumar al loader.
                #endregion

                #region Check Tutorial
                if(User.GetClient().GetPlay().TutorialStep < RoleplayManager.LastTutorialStep && !User.GetClient().GetPlay().InTutorial)
                {
                    User.GetClient().GetPlay().InTutorial = true;
                    if (User.GetClient().GetPlay().TutorialStep == 13 && _room.WardrobeEnabled && _room.Type.Equals("public"))
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|13");
                    }
                    else if (User.GetClient().GetPlay().TutorialStep == 18 && _room.PhoneStoreEnabled && _room.Type.Equals("public"))
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|18");
                    }
                    else if (User.GetClient().GetPlay().TutorialStep == 23 && _room.BuyCarEnabled && _room.Type.Equals("public"))
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|24");
                    }
                    else if (User.GetClient().GetPlay().TutorialStep == 27 && _room.MallEnabled && _room.Type.Equals("public"))
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|28");
                    }
                    else
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_my_tutorial|" + User.GetClient().GetPlay().TutorialStep);
                }
                #endregion
            }
            else if (!User.IsBot && (User.GetClient().GetHabbo().IsTeleporting || User.GetClient().GetHabbo().IsHopping))
            {
                Item Item = null;
                if (Session.GetHabbo().IsTeleporting)
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().TeleporterId);
                else if (Session.GetHabbo().IsHopping)
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().HopperId);

                if (Item != null)
                {
                    if (Session.GetHabbo().IsTeleporting)
                    {
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                        User.SetRot(Item.Rotation, false);
                        Item.InteractingUser2 = Session.GetHabbo().Id;
                        Item.ExtraData = "0";
                        Item.UpdateState(false, true);

                        // Empujamos al Usuario
                        #region Move User From 
                        //User.MoveTo(Item.SquareBehind.X, Item.SquareBehind.Y);
                        //_room.SendMessage(_room.GetRoomItemHandler().UpdateUserOnRoller(User, new Point(Item.SquareBehind.X, Item.SquareBehind.Y), 0, _room.GetGameMap().SqAbsoluteHeight(Item.SquareBehind.X, Item.SquareBehind.Y)));
                        
                        #endregion
                    }
                    else if (Session.GetHabbo().IsHopping)
                    {
                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                        User.SetRot(Item.Rotation, false);
                        User.AllowOverride = false;
                        Item.InteractingUser2 = Session.GetHabbo().Id;
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                    }
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ - 1);
                    User.SetRot(Model.DoorOrientation, false);
                }
            }

            Session.GetPlay().CooldownManager.CreateCooldown("gun", 1000, 5);
            Session.GetPlay().CooldownManager.CreateCooldown("fist", 1000, 5);

            _room.SendMessage(new UsersComposer(User));

            //Below = done
            if (_room.CheckRights(Session, true))
            {
                User.SetStatus("flatctrl", "useradmin");
                Session.SendMessage(new YouAreOwnerComposer());
                Session.SendMessage(new YouAreControllerComposer(4));
            }
            else if (_room.CheckRights(Session, false) && _room.Group == null)
            {
                User.SetStatus("flatctrl", "1");
                Session.SendMessage(new YouAreControllerComposer(1));
            }
            else if (_room.Group != null && _room.CheckRights(Session, false, true))
            {
                User.SetStatus("flatctrl", "3");
                Session.SendMessage(new YouAreControllerComposer(3));
            }
            else
                Session.SendMessage(new YouAreNotControllerComposer());
            User.UpdateNeeded = true;

            //(OFF) HERE FOR ENABLES FOR DIFFERENT RANKS
            /*
            if ((Session.GetHabbo().Rank == 2) && !Session.GetHabbo().DisableForcedEffects)
                Session.GetHabbo().Effects().ApplyEffect(187);
            else if ((Session.GetHabbo().Rank == 3) && !Session.GetHabbo().DisableForcedEffects)
                Session.GetHabbo().Effects().ApplyEffect(178);
            else if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().DisableForcedEffects)
                Session.GetHabbo().Effects().ApplyEffect(102);
            */
            // END ENABLES FOR RANKS

            if ((this._room.ForSale && (this._room.SalePrice > 0)) && (this._room.GetRoomUserManager().GetRoomUserByHabbo(this._room.OwnerName) != null))
            {
                Session.SendWhisper("Esta Sala esta en venta, en  " + this._room.SalePrice + " duckets. Escribe :comprarsala si deseas comprarla!", 0);
            }
            else if (this._room.ForSale && (this._room.GetRoomUserManager().GetRoomUserByHabbo(this._room.OwnerName) == null))
            {
                foreach (RoomUser user2 in this._room.GetRoomUserManager().GetRoomUsers())
                {
                    if (((user2.GetClient() != null) && (user2.GetClient().GetHabbo() != null)) && (user2.GetClient().GetHabbo().Id != Session.GetHabbo().Id))
                    {
                        user2.GetClient().SendWhisper("Esta Sala ya no se encuentra a la venta.", 0);
                    }
                }
                this._room.ForSale = false;
                this._room.SalePrice = 0;
            }

            /*
            if (!(!Session.GetHabbo().GetPermissions().HasRight("gold_vip") || Session.GetHabbo().DisableForcedEffects || Session.GetHabbo().GetPermissions().HasRight("mod_tool")))
            {
                Session.GetHabbo().Effects().ApplyEffect(0xb2);
            }
            */
            return true;
        }

        public void RemoveUserFromRoom(GameClient Session, Boolean NotifyClient, Boolean NotifyKick = false)
        {
            try
            {
                if (_room == null)
                    return;

                if (Session == null || Session.GetHabbo() == null)
                    return;

                if (NotifyKick)
                    Session.SendMessage(new GenericErrorComposer(4008));

                if (NotifyClient)
                    Session.SendMessage(new CloseConnectionComposer());

                if (Session.GetHabbo().TentId > 0)
                    Session.GetHabbo().TentId = 0;

                RoomUser User = GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User != null)
                {
                    if (User.RidingHorse)
                    {
                        User.RidingHorse = false;
                        RoomUser UserRiding = GetRoomUserByVirtualId(User.HorseID);
                        if (UserRiding != null)
                        {
                            UserRiding.RidingHorse = false;
                            UserRiding.HorseID = 0;
                        }
                    }

                    if (User.Team != TEAM.NONE)
                    {
                        TeamManager Team = this._room.GetTeamManagerForFreeze();
                        if (Team != null)
                        {
                            Team.OnUserLeave(User);

                            User.Team = TEAM.NONE;

                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                        }
                    }


                    RemoveRoomUser(User);

                    if (User.CurrentItemEffect != ItemEffectType.NONE)
                    {
                        if (Session.GetHabbo().Effects() != null)
                            Session.GetHabbo().Effects().CurrentEffect = -1;
                    }

                    if (_room != null)
                    {
                        if (_room.HasActiveTrade(Session.GetHabbo().Id))
                            _room.TryStopTrade(Session.GetHabbo().Id);
                    }

                    //Session.GetHabbo().CurrentRoomId = 0;

                    if (Session.GetHabbo().GetMessenger() != null)
                        Session.GetHabbo().GetMessenger().OnStatusChanged(true);

                    /* Evitamos estas consultas para ahorrar memoria
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE user_roomvisits SET exit_timestamp = '" + PlusEnvironment.GetUnixTimestamp() + "' WHERE room_id = '" + _room.RoomId + "' AND user_id = '" + Session.GetHabbo().Id + "' ORDER BY exit_timestamp DESC LIMIT 1");
                        dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + _room.UsersNow + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
                    }
                    */

                    if (User != null)
                        User.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        private void onRemove(RoomUser user)
        {
            try
            {

                GameClient session = user.GetClient();
                if (session == null)
                    return;

                List<RoomUser> Bots = new List<RoomUser>();

                try
                {
                    foreach (RoomUser roomUser in GetUserList().ToList())
                    {
                        if (roomUser == null)
                            continue;

                        if (roomUser.IsBot && !roomUser.IsPet)
                        {
                            if (!Bots.Contains(roomUser))
                                Bots.Add(roomUser);
                        }
                    }
                }
                catch { }

                List<RoomUser> PetsToRemove = new List<RoomUser>();
                foreach (RoomUser Bot in Bots.ToList())
                {
                    if (Bot == null || Bot.BotAI == null)
                        continue;

                    Bot.BotAI.OnUserLeaveRoom(session);

                    if (Bot.IsPet && Bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true))
                    {
                        if (!PetsToRemove.Contains(Bot))
                            PetsToRemove.Add(Bot);
                    }
                }

                foreach (RoomUser toRemove in PetsToRemove.ToList())
                {
                    if (toRemove == null)
                        continue;

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetInventoryComponent() == null)
                        continue;

                    user.GetClient().GetHabbo().GetInventoryComponent().TryAddPet(toRemove.PetData);
                    RemoveBot(toRemove.VirtualId, false);
                }

                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }
        }

        public void ClearUsers(GameClient Session)
        {
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                Session.SendMessage(new UserRemoveComposer(user.VirtualId));
            }
        }

        private void RemoveRoomUser(RoomUser user)
        {
            if (user.SetStep)
                _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
            else
                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;

            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            _room.SendMessage(new UserRemoveComposer(user.VirtualId));

            RoomUser toRemove = null;
            if (this._users.TryRemove(user.InternalRoomID, out toRemove))
            {
                //uhmm, could put the below stuff in but idk.
            }

            user.InternalRoomID = -1;
            onRemove(user);
        }

        public bool TryGetPet(int PetId, out RoomUser Pet)
        {
            return this._pets.TryGetValue(PetId, out Pet);
        }

        public bool TryGetBot(int BotId, out RoomUser Bot)
        {
            return this._bots.TryGetValue(BotId, out Bot);
        }

        public RoomUser GetBotByName(string Name)
        {
            bool FoundBot = this._bots.Where(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == Name.ToLower()).ToList().Count() > 0;
            if (FoundBot)
            {
                int Id = this._bots.FirstOrDefault(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == Name.ToLower()).Value.BotData.Id;

                return this._bots[Id];
            }

            return null;
        }

        public List<RoomUser> GetBots()
        {
            List<RoomUser> Bots = new List<RoomUser>();

            foreach (RoomUser User in this._bots.Values.ToList())
            {
                if (User == null || !User.IsBot)
                    continue;

                Bots.Add(User);
            }

            return Bots;
        }

        public void UpdateUserCount(int count)
        {
            userCount = count;
            _room.RoomData.UsersNow = count;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + count + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
            }
        }

        public RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            RoomUser User = null;
            if (!_users.TryGetValue(VirtualId, out User))
                return null;
            return User;
        }

        public RoomUser GetRoomUserByHabbo(int Id)
        {
            if (this == null)
                return null;

            RoomUser User = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == Id).FirstOrDefault();

            if (User != null)
                return User;

            return null;
        }

        public List<RoomUser> GetRoomUsers()
        {
            List<RoomUser> List = new List<RoomUser>();

            List = this.GetUserList().Where(x => (!x.IsBot)).ToList();

            return List;
        }

        public List<RoomUser> GetRoomUserByRank(int minRank)
        {
            var returnList = new List<RoomUser>();
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().Rank >= minRank)
                    returnList.Add(user);
            }

            return returnList;
        }

        public RoomUser GetRoomUserByHabbo(string pName)
        {
            RoomUser User = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (User != null)
                return User;

            return null;
        }

        public void UpdatePets()
        {
            foreach (Pet Pet in GetPets().ToList())
            {
                if (Pet == null)
                    continue;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (Pet.DBState == DatabaseUpdateState.NeedsInsert)
                    {
                        dbClient.SetQuery("INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('" + Pet.PetId + "','" + Pet.OwnerId + "','" + Pet.RoomId + "',@name,'0','0','0')");
                        dbClient.AddParameter("name", Pet.Name);
                        dbClient.RunQuery();

                        dbClient.SetQuery("INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('" + Pet.Type + "',@race,@color,'0','100','" + Pet.CreationStamp + "','0','0')");
                        dbClient.AddParameter(Pet.PetId + "race", Pet.Race);
                        dbClient.AddParameter(Pet.PetId + "color", Pet.Color);
                        dbClient.RunQuery();
                    }
                    else if (Pet.DBState == DatabaseUpdateState.NeedsUpdate)
                    {
                        //Surely this can be *99 better?
                        RoomUser User = GetRoomUserByVirtualId(Pet.VirtualId);

                        dbClient.RunQuery("UPDATE `bots` SET room_id = " + Pet.RoomId + ", x = " + (User != null ? User.X : 0) + ", Y = " + (User != null ? User.Y : 0) + ", Z = " + (User != null ? User.Z : 0) + " WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                        dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + Pet.experience + "', `energy` = '" + Pet.Energy + "', `nutrition` = '" + Pet.Nutrition + "', `respect` = '" + Pet.Respect + "' WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                    }

                    Pet.DBState = DatabaseUpdateState.Updated;
                }
            }
        }

        public List<Pet> GetPets()
        {
            List<Pet> Pets = new List<Pet>();
            foreach (RoomUser User in this._pets.Values.ToList())
            {
                if (User == null || !User.IsPet)
                    continue;

                Pets.Add(User.PetData);
            }

            return Pets;
        }

        public void SerializeStatusUpdates()
        {
            List<RoomUser> Users = new List<RoomUser>();
            ICollection<RoomUser> RoomUsers = GetUserList();

            if (RoomUsers == null)
                return;

            foreach (RoomUser User in RoomUsers.ToList())
            {
                if (User == null || !User.UpdateNeeded || Users.Contains(User))
                    continue;

                User.UpdateNeeded = false;
                Users.Add(User);
            }

            if (Users.Count > 0)
                _room.SendMessage(new UserUpdateComposer(Users));
        }

        public void UpdateUserStatusses()
        {
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                UpdateUserStatus(user, false);
            }
        }

        private bool isValid(RoomUser user)
        {
            if (user == null)
                return false;
            if (user.IsBot)
                return true;
            if (user.GetClient() == null)
                return false;
            if (user.GetClient().GetHabbo() == null)
                return false;
            if (user.GetClient().GetHabbo().CurrentRoomId != _room.RoomId)
                return false;
            return true;
        }

        public void OnCycle()
        {
            int userCounter = 0;            

            try
            {
                if (_room != null && _room.DiscoMode && _room.TonerData != null && _room.TonerData.Enabled == 1)
                {
                    Item Item = _room.GetRoomItemHandler().GetItem(_room.TonerData.ItemId);

                    if (Item != null)
                    {
                        _room.TonerData.Hue = PlusEnvironment.GetRandomNumber(0, 255);
                        _room.TonerData.Saturation = PlusEnvironment.GetRandomNumber(0, 255);
                        _room.TonerData.Lightness = PlusEnvironment.GetRandomNumber(0, 255);

                        _room.SendMessage(new ObjectUpdateComposer(Item, _room.OwnerId));
                        Item.UpdateState();
                    }
                }

                List<RoomUser> ToRemove = new List<RoomUser>();

                foreach (RoomUser User in GetUserList().ToList())
                {
                    if (User == null)
                        continue;

                    if (!isValid(User))
                    {
                        if (User.GetClient() != null)
                            RemoveUserFromRoom(User.GetClient(), false, false);
                        else
                            RemoveRoomUser(User);
                    }

                    #region GodModEnteringRoom *Zedd* V2
                    if (User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetHabbo().LoginGodProtect && User.GetClient().GetPlay().FirstTickBool == true && !User.GetClient().GetPlay().DrivingCar && !User.GetClient().GetPlay().Pasajero && !User.GetClient().GetPlay().IsNoob && !User.GetClient().GetPlay().IsGodMode)
                    {
                        if ((_room.SafeZoneEnabled && User.GetClient().GetPlay().GodModeTicks > 0)|| User.GetClient().GetPlay().PassiveMode)
                        {
                            User.GetClient().GetHabbo().LoginGodProtect = false;
                            User.GetClient().GetPlay().GodMode = false;
                            User.GetClient().GetPlay().GodModeTicks = 0;
                            User.GetClient().GetPlay().FirstTickBool = false;
                            if (User.GetClient().GetRoomUser() != null && User.CurrentEffect == EffectsList.Fireflies)
                                User.GetClient().GetRoomUser().ApplyEffect(EffectsList.None);
                        }

                        if (!_room.SafeZoneEnabled || User.GetClient().GetPlay().GodModeTicks > 0)
                        {
                            if (User.GetClient().GetPlay().GodModeTicks <= 10)
                            {
                                //User.GetClient().GetPlay().IsNoob = true; Variable de GodMode
                                User.GetClient().GetPlay().GodMode = true;
                                User.GetClient().GetPlay().GodModeTicks++;
                                if (User.GetClient().GetPlay().GodModeTicks % 2 == 0)
                                    User.GetClient().GetHabbo().GetClient().SendWhisper("((Te quedan " + (10 - User.GetClient().GetPlay().GodModeTicks) + " segundos de inmunidad en esta Zona))", 0);
                            }
                            else
                            {
                                //User.GetClient().GetPlay().IsNoob = false;
                                User.GetClient().GetHabbo().LoginGodProtect = false;
                                User.GetClient().GetPlay().GodMode = false;
                                User.GetClient().GetPlay().GodModeTicks = 0;
                                User.GetClient().GetHabbo().GetClient().SendWhisper("((Tu inmunidad en esta Zona ha acabado))", 0);
                                User.GetClient().GetPlay().FirstTickBool = false;
                                if (User.GetClient().GetRoomUser() != null)
                                    User.GetClient().GetRoomUser().ApplyEffect(EffectsList.None);
                            }
                        }
                    }
                    #endregion GodModEnteringRoom *Zedd* V2

                    #region Check Turf Capturing
                    if(_room.TurfCapturing)
                    {
                        if(User.GetClient() != null)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "event_gang", "turf_cap_w," + _room.Id + "," + _room.TurfUserAtackerId + "," + _room.Name);
                        }
                    }
                    #endregion

                    if (User.NeedsAutokick && !ToRemove.Contains(User))
                    {
                        ToRemove.Add(User);
                        continue;
                    }

                    bool updated = false;
                    User.IdleTime++;
                    User.HandleSpamTicks();
                    if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600)
                    {
                        User.IsAsleep = true;
                        _room.SendMessage(new SleepComposer(User, true));
                    }

                    if (User.CarryItemID > 0)
                    {
                        User.CarryTimer--;
                        if (User.CarryTimer <= 0)
                            User.CarryItem(0);
                    }

                    if (_room.GotFreeze())
                        _room.GetFreeze().CycleUser(User);

                    bool InvalidStep = false;

                    if (User.isRolling)
                    {
                        if (User.rollerDelay <= 0)
                        {
                            UpdateUserStatus(User, false);
                            User.isRolling = false;
                        }
                        else
                            User.rollerDelay--;
                    }

                    if (User.SetStep)
                    {
                        // Importante FastWalinkg 1
                        if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(User.SetX, User.SetY), (User.GoalX == User.SetX && User.GoalY == User.SetY), User.AllowOverride))
                        {
                            if (!User.RidingHorse)
                                _room.GetGameMap().UpdateUserMovement(new Point(User.Coordinate.X, User.Coordinate.Y), new Point(User.SetX, User.SetY), User);

                            List<Item> items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (Item Item in items.ToList())
                            {
                                Item.UserWalksOffFurni(User);
                            }

                            if (!User.IsBot)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }
                            else if (User.IsBot && !User.RidingHorse)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }

                            if (!User.IsBot && User.RidingHorse)
                            {
                                RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.X = User.SetX;
                                    Horse.Y = User.SetY;
                                }
                            }
                            
                            #region Door Kick from Rooms (OFF)
                            /*
                            if (User.X == _room.GetGameMap().Model.DoorX && User.Y == _room.GetGameMap().Model.DoorY && !ToRemove.Contains(User) && !User.IsBot)
                            {
                                ToRemove.Add(User);
                                continue;
                            }
                            */
                            #endregion

                            List<Item> Items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (Item Item in Items.ToList())
                            {
                                Item.UserWalksOnFurni(User);
                            }

                            // New rp by Jeihden
                            /*
                            if (User != null)
                            {
                                if (User.GetClient() != null)
                                {
                                    if (User.GetClient().GetPlay() != null)
                                    {
                                        User.GetClient().GetHabbo().HomeRoom = User.RoomId;
                                        User.GetClient().GetPlay().LastCoordinates = User.X + "," + User.Y + "," + User.Z + "," + User.RotBody;
                                    }
                                }
                            }
                            */

                            UpdateUserStatus(User, true);
                        }
                        else
                            InvalidStep = true;
                        User.SetStep = false;
                    }

                    if (User.PathRecalcNeeded)
                    {
                        if (User.Path.Count > 1)
                            User.Path.Clear();

                        if(User.GetClient() != null && User.GetClient().GetPlay().WalkNoDiagonal)
                            User.Path = PathFinder.FindPath(User, false, this._room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));
                        else
                            User.Path = PathFinder.FindPath(User, this._room.GetGameMap().DiagonalEnabled, this._room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));
                        //Console.WriteLine("Necesitamos recalcular la ruta del usuario : [" + User.GetClient().GetHabbo().Username + "] && a las coordenadas [" + + "] &&Con una cantidad de pasos de [" + User.Path.Count + "]");

                        if (User.Path.Count > 1)
                        {
                            User.PathStep = 1;
                            User.IsWalking = true;
                            User.PathRecalcNeeded = false;
                        }
                        else
                        {
                            User.PathRecalcNeeded = false;
                            if (User.Path.Count > 1)
                                User.Path.Clear();
                        }
                    }

                    if (User.IsWalking && !User.Freezed)
                    {
                        if (InvalidStep || (User.PathStep >= User.Path.Count) || (User.GoalX == User.X && User.GoalY == User.Y)) //No path found, or reached goal (:
                        {
                            User.IsWalking = false;
                            User.RemoveStatus("mv");

                            if (User.Statusses.ContainsKey("sign"))
                                User.RemoveStatus("sign");

                            if (User.IsBot && User.BotData.TargetUser > 0)
                            {
                                if (User.CarryItemID > 0)
                                {
                                    RoomUser Target = _room.GetRoomUserManager().GetRoomUserByHabbo(User.BotData.TargetUser);

                                    if (Target != null && Gamemap.TilesTouching(User.X, User.Y, Target.X, Target.Y))
                                    {
                                        User.SetRot(Rotation.Calculate(User.X, User.Y, Target.X, Target.Y), false);
                                        Target.SetRot(Rotation.Calculate(Target.X, Target.Y, User.X, User.Y), false);
                                        Target.CarryItem(User.CarryItemID);
                                    }
                                }

                                User.CarryItem(0);
                                User.BotData.TargetUser = 0;
                            }

                            if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                            {
                                RoomUser mascotaVinculada = GetRoomUserByVirtualId(User.HorseID);
                                if (mascotaVinculada != null)
                                {
                                    mascotaVinculada.IsWalking = false;
                                    mascotaVinculada.RemoveStatus("mv");
                                    mascotaVinculada.UpdateNeeded = true;
                                }
                            }
                        }
                        else
                        {
                            Vector2D NextStep = User.Path[(User.Path.Count - User.PathStep) - 1];
                            User.PathStep++;

                            if (User.IsBot)
                            {
                                if (User.FastWalking)
                                {
                                    string PassengerName = User.BotData.Name.Split('#')[1];
                                    RoomUser Passenger = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PassengerName).GetRoomUser();
                                    if(Passenger != null)
                                    {
                                        if (Passenger.FastWalking && Passenger.PathStep < Passenger.Path.Count && Passenger.Path.Count > 3)
                                        {
                                            int s2 = (Passenger.Path.Count - Passenger.PathStep) - 1;
                                            NextStep = Passenger.Path[s2];
                                            User.PathStep++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (User.FastWalking && User.PathStep < User.Path.Count && User.Path.Count > 3)
                                {
                                    int s2 = (User.Path.Count - User.PathStep) - 1;
                                    NextStep = User.Path[s2];
                                    User.PathStep++;

                                }

                                if (User.SuperFastWalking && User.PathStep < User.Path.Count && User.Path.Count > 3)
                                {
                                    int s2 = (User.Path.Count - User.PathStep) - 1;
                                    NextStep = User.Path[s2];
                                    User.PathStep++;
                                    User.PathStep++;
                                }
                            }


                            int nextX = NextStep.X;
                            int nextY = NextStep.Y;

                            int nextFrontX = SquareInFront(nextX, nextY, User.RotBody, "x");
                            int nextFrontY = SquareInFront(nextX, nextY, User.RotBody, "y"); 
                            User.RemoveStatus("mv");

                            if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(nextX, nextY), (User.GoalX == nextX && User.GoalY == nextY), User.AllowOverride))
                            {
                                double nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                                if (!User.IsBot)
                                {
                                    if (User.isSitting)
                                    {
                                        User.Statusses.Remove("sit");
                                        User.Z += 0.35;
                                        User.isSitting = false;
                                        User.UpdateNeeded = true;
                                    }
                                    else if (User.isLying)
                                    {
                                        User.Statusses.Remove("sit");
                                        User.Z += 0.35;
                                        User.isLying = false;
                                        User.UpdateNeeded = true;
                                    }
                                }
                                if (!User.IsBot)
                                {
                                    User.Statusses.Remove("lay");
                                    User.Statusses.Remove("sit");
                                }

                                if (!User.IsBot && !User.IsPet && User.GetClient() != null)
                                {
                                    if (User.GetClient().GetHabbo().IsTeleporting)
                                    {
                                        User.GetClient().GetHabbo().IsTeleporting = false;
                                        User.GetClient().GetHabbo().TeleporterId = 0;
                                    }
                                    else if (User.GetClient().GetHabbo().IsHopping)
                                    {
                                        User.GetClient().GetHabbo().IsHopping = false;
                                        User.GetClient().GetHabbo().HopperId = 0;
                                    }
                                }

                                if (!User.IsBot && User.RidingHorse && User.IsPet == false)
                                {
                                    RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                        Horse.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                    User.AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));

                                    User.UpdateNeeded = true;
                                    Horse.UpdateNeeded = true;
                                }
                                else  if (!User.IsBot && User.GetClient().GetPlay().Chofer)
                                {
                                    //Vars
                                    string Pasajeros = User.GetClient().GetPlay().Pasajeros;
                                    string[] stringSeparators = new string[] { ";" };
                                    string[] result;
                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (string psjs in result)
                                    {
                                        GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                        if (PJ != null && PJ.GetRoomUser() != null)
                                        {
                                            PJ.GetRoomUser().AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                            PJ.GetRoomUser().UpdateNeeded = true;
                                        }
                                    }

                                    User.AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                    User.UpdateNeeded = true;
                                    
                                }
                                else
                                    User.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                int newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);
                                int oldRot = User.RotBody;
                                User.RotBody = newRot;
                                User.RotHead = newRot;

                                User.SetStep = true;
                                User.SetX = nextX;
                                User.SetY = nextY;
                                User.SetZ = nextZ;
                                UpdateUserEffect(User, User.SetX, User.SetY);

                                updated = true;

                                #region New Escort
                                if ((User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetHabbo().Escorting > 0))
                                {
                                    RoomUser user = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(User.GetClient().GetHabbo().Escorting);
                                    if (user != null)
                                    {
                                        RoomUser roomUser = User;
                                        User.AddStatus("mv", roomUser.SetX + "," + roomUser.SetY + "," + TextHandling.GetString(nextZ));
                                        User.UpdateNeeded = true;

                                        user.warp(roomUser.SetX, roomUser.SetY, roomUser.SetZ);
                                        if (newRot == 0)
                                        {
                                            user.MoveTo(roomUser.SetX, checked(roomUser.SetY - 1));
                                            user.AddStatus("mv", roomUser.SetX + "," + checked(roomUser.SetY - 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 1)
                                        {
                                            user.MoveTo(checked(roomUser.SetX + 1), checked(roomUser.SetY - 1));
                                            user.AddStatus("mv", checked(roomUser.SetX + 1) + "," + checked(roomUser.SetY - 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 2)
                                        {
                                            user.MoveTo(checked(roomUser.SetX + 1), roomUser.SetY);
                                            user.AddStatus("mv", checked(roomUser.SetX + 1) + "," + roomUser.SetY + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 3)
                                        {
                                            user.MoveTo(checked(roomUser.SetX + 1), checked(roomUser.SetY + 1));
                                            user.AddStatus("mv", checked(roomUser.SetX + 1) + "," + checked(roomUser.SetY + 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 4)
                                        {
                                            user.MoveTo(roomUser.SetX, checked(roomUser.SetY + 1));
                                            user.AddStatus("mv", roomUser.SetX + "," + checked(roomUser.SetY + 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 5)
                                        {
                                            user.MoveTo(checked(roomUser.SetX - 1), checked(roomUser.SetY + 1));
                                            user.AddStatus("mv", checked(roomUser.SetX - 1) + "," + checked(roomUser.SetY + 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 6)
                                        {
                                            user.MoveTo(checked(roomUser.SetX - 1), roomUser.SetY);
                                            user.AddStatus("mv", checked(roomUser.SetX - 1) + "," + roomUser.SetY + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 7)
                                        {
                                            user.MoveTo(checked(roomUser.SetX - 1), checked(roomUser.SetY - 1));
                                            user.AddStatus("mv", checked(roomUser.SetX - 1) + "," + checked(roomUser.SetY - 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        if (User.RotBody != user.RotBody)
                                        {
                                            user.SetRot(User.RotBody, false);
                                        }
                                    }
                                }
                                #endregion

                                #region New Taxi Passenger (No entra, ya que forzamos que pisen los mismos cuadros con la validación del fastwalking)
                                if ((User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetHabbo().TaxiChofer > 0))
                                {
                                    RoomUser user = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetBotByName("Taxi#"+ User.GetClient().GetHabbo().Username);
                                    if (user != null && user.CanWalk && !user.FastWalking && !user.SuperFastWalking)
                                    {
                                        RoomUser roomUser = User;
                                        User.AddStatus("mv", roomUser.SetX + "," + roomUser.SetY + "," + TextHandling.GetString(nextZ));
                                        User.UpdateNeeded = true;
                                        user.warp(roomUser.SetX, roomUser.SetY, roomUser.SetZ);
                                        if (newRot == 0)
                                        {
                                            user.MoveTo(roomUser.SetX, checked(roomUser.SetY - 1));
                                            user.AddStatus("mv", roomUser.SetX + "," + checked(roomUser.SetY - 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 1)
                                        {
                                            user.MoveTo(checked(roomUser.SetX + 1), checked(roomUser.SetY - 1));
                                            user.AddStatus("mv", checked(roomUser.SetX + 1) + "," + checked(roomUser.SetY - 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 2)
                                        {
                                            user.MoveTo(checked(roomUser.SetX + 1), roomUser.SetY);
                                            user.AddStatus("mv", checked(roomUser.SetX + 1) + "," + roomUser.SetY + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 3)
                                        {
                                            user.MoveTo(checked(roomUser.SetX + 1), checked(roomUser.SetY + 1));
                                            user.AddStatus("mv", checked(roomUser.SetX + 1) + "," + checked(roomUser.SetY + 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 4)
                                        {
                                            user.MoveTo(roomUser.SetX, checked(roomUser.SetY + 1));
                                            user.AddStatus("mv", roomUser.SetX + "," + checked(roomUser.SetY + 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 5)
                                        {
                                            user.MoveTo(checked(roomUser.SetX - 1), checked(roomUser.SetY + 1));
                                            user.AddStatus("mv", checked(roomUser.SetX - 1) + "," + checked(roomUser.SetY + 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 6)
                                        {
                                            user.MoveTo(checked(roomUser.SetX - 1), roomUser.SetY);
                                            user.AddStatus("mv", checked(roomUser.SetX - 1) + "," + roomUser.SetY + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        else if (newRot == 7)
                                        {
                                            user.MoveTo(checked(roomUser.SetX - 1), checked(roomUser.SetY - 1));
                                            user.AddStatus("mv", checked(roomUser.SetX - 1) + "," + checked(roomUser.SetY - 1) + "," + TextHandling.GetString(nextZ));
                                            user.UpdateNeeded = true;
                                        }
                                        if (User.RotBody != user.RotBody)
                                        {
                                            user.SetRot(User.RotBody, false);
                                        }
                                    }
                                }
                                #endregion

                                if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                                {
                                    RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.RotBody = newRot;
                                        Horse.RotHead = newRot;

                                        Horse.SetStep = true;
                                        Horse.SetX = nextX;
                                        Horse.SetY = nextY;
                                        Horse.SetZ = nextZ;
                                    }
                                }

                                _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState; // REstore the old one
                                User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY]; //Backup the new one

                                if (_room.RoomBlockingEnabled == 0)
                                {
                                    RoomUser Users = _room.GetRoomUserManager().GetUserForSquare(nextX, nextY);
                                    if (Users != null)
                                        _room.GetGameMap().GameMap[nextX, nextY] = 0;
                                }
                                else
                                    _room.GetGameMap().GameMap[nextX, nextY] = 1;
                            }
                        }
                        if (!User.RidingHorse)
                            User.UpdateNeeded = true;
                    }
                    else
                    {
                        if (User.Statusses.ContainsKey("mv"))
                        {
                            User.RemoveStatus("mv");
                            User.UpdateNeeded = true;

                            if (User.RidingHorse)
                            {
                                RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.RemoveStatus("mv");
                                    Horse.UpdateNeeded = true;
                                }
                            }
                        }
                    }

                    if (User.RidingHorse)
                        User.ApplyEffect(77);

                    if(!User.IsBot && User.GetClient() != null && User.GetClient().GetPlay() != null && User.GetClient().GetPlay().Pasajero)
                        User.ApplyEffect(EffectsList.Invisible);

                    if (User.IsBot && User.BotAI != null)
                        User.BotAI.OnTimerTick();
                    else
                        userCounter++;

                    if (!updated)
                    {
                        UpdateUserEffect(User, User.X, User.Y);
                    }
                }

                foreach (RoomUser toRemove in ToRemove.ToList())
                {
                    GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(toRemove.HabboId);
                    if (client != null)
                    {
                        RemoveUserFromRoom(client, true, false);
                    }
                    else
                        RemoveRoomUser(toRemove);
                }

                if (userCount != userCounter)
                    UpdateUserCount(userCounter);
            }
            catch (Exception e)
            {
                int rId = 0;
                if (_room != null)
                    rId = _room.Id;

                Logging.LogCriticalException("Affected Room - ID: " + rId + " - " + e.ToString());
            }
        }

        public void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            if (User == null)
                return;

            try
            {
                bool isBot = User.IsBot;
                if (isBot)
                    cyclegameitems = false;

                if (PlusEnvironment.GetUnixTimestamp() > PlusEnvironment.GetUnixTimestamp() + User.SignTime)
                {
                    if (User.Statusses.ContainsKey("sign"))
                    {
                        User.Statusses.Remove("sign");
                        User.UpdateNeeded = true;
                    }
                }

                if ((User.Statusses.ContainsKey("lay") && !User.isLying) || (User.Statusses.ContainsKey("sit") && !User.isSitting))
                {
                    if (User.Statusses.ContainsKey("lay"))
                        User.Statusses.Remove("lay");
                    if (User.Statusses.ContainsKey("sit"))
                        User.Statusses.Remove("sit");
                    User.UpdateNeeded = true;
                }
                else if (User.isLying || User.isSitting)
                    return;

                double newZ;
                List<Item> ItemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
                if (ItemsOnSquare != null || ItemsOnSquare.Count != 0)
                {
                    if (User.RidingHorse && User.IsPet == false)
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList()) + 1;
                    else
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList());
                }
                else
                {
                    newZ = 1;
                }

                if (newZ != User.Z)
                {
                    User.Z = newZ;
                    User.UpdateNeeded = true;
                }

                DynamicRoomModel Model = _room.GetGameMap().Model;
                if (Model.SqState[User.X, User.Y] == SquareState.SEAT)
                {
                    if (!User.Statusses.ContainsKey("sit"))
                        User.Statusses.Add("sit", "1.0");
                    User.Z = Model.SqFloorHeight[User.X, User.Y];
                    User.RotHead = Model.SqSeatRot[User.X, User.Y];
                    User.RotBody = Model.SqSeatRot[User.X, User.Y];

                    User.UpdateNeeded = true;
                }


                if (ItemsOnSquare.Count == 0)
                    User.LastItem = null;


                foreach (Item Item in ItemsOnSquare.ToList())
                {
                    if (Item == null)
                        continue;

                    if (Item.GetBaseItem().IsSeat)
                    {
                        if (!User.Statusses.ContainsKey("sit"))
                        {
                            if (!User.Statusses.ContainsKey("sit"))
                                User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                        }

                        User.Z = Item.GetZ;
                        User.RotHead = Item.Rotation;
                        User.RotBody = Item.Rotation;
                        User.UpdateNeeded = true;
                    }

                    switch (Item.GetBaseItem().InteractionType)
                    {
                        #region Roleplay

                        #region Shower
                        case InteractionType.SHOWER:
                            {
                                if (User.Coordinate.X == Item.GetX && User.Coordinate.Y == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetPlay() == null)
                                        continue;

                                    Room Room;

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                        return;

                                    if (User.GetClient().GetPlay().Hygiene >= 100)
                                    {
                                        User.GetClient().SendWhisper("¡Tu Higiene ya se encuentra al Máximo!", 1);
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.InteractingUser != 0)
                                    {
                                        User.GetClient().SendWhisper("Esta ducha está siendo usada por alguien más. Lo sentimos, pero no puedes ducharte con otra persona a la vez.", 1);
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.ExtraData == "0" || Item.ExtraData == "")
                                    {
                                        Item.ExtraData = "1";
                                        Item.UpdateState(false, true);
                                        Item.RequestUpdate(1, true);
                                    }
                                    if (Item.ExtraData == "1")
                                    {
                                        if (!User.GetClient().GetPlay().InShower)
                                        {
                                            User.ClearMovement(true);
                                            Item.InteractingUser = User.GetClient().GetHabbo().Id;
                                            User.GetClient().GetPlay().InShower = true;
                                            RoleplayManager.Shout(User.GetClient(), "*Comienza a tomar una refrescante ducha*", 5);
                                            User.GetClient().GetPlay().TimerManager.CreateTimer("shower", 1000, false, Item.Id);
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion
                        #endregion

                        #region Beds & Tents
                        case InteractionType.BED:
                        case InteractionType.TENT_SMALL:
                            {
                                if (!User.Statusses.ContainsKey("lay"))
                                    User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");

                                User.Z = Item.GetZ;
                                User.RotHead = Item.Rotation;
                                User.RotBody = Item.Rotation;

                                User.UpdateNeeded = true;
                                break;
                            }
                        #endregion

                        #region Banzai Gates
                        case InteractionType.banzaigategreen:
                        case InteractionType.banzaigateblue:
                        case InteractionType.banzaigatered:
                        case InteractionType.banzaigateyellow:
                            {
                                if (cyclegameitems)
                                {
                                    int effectID = Convert.ToInt32(Item.team + 32);
                                    TeamManager t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();

                                    if (User.Team == TEAM.NONE)
                                    {
                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            if (User.Team != TEAM.NONE)
                                                t.OnUserLeave(User);
                                            User.Team = Item.team;

                                            t.AddUser(User);

                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                        }
                                    }
                                    else if (User.Team != TEAM.NONE && User.Team != Item.team)
                                    {
                                        t.OnUserLeave(User);
                                        User.Team = TEAM.NONE;
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    else
                                    {
                                        //usersOnTeam--;
                                        t.OnUserLeave(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        User.Team = TEAM.NONE;
                                    }
                                    //Item.ExtraData = usersOnTeam.ToString();
                                    //Item.UpdateState(false, true);                                
                                }
                                break;
                            }
                        #endregion

                        #region Freeze Gates
                        case InteractionType.FREEZE_YELLOW_GATE:
                        case InteractionType.FREEZE_RED_GATE:
                        case InteractionType.FREEZE_GREEN_GATE:
                        case InteractionType.FREEZE_BLUE_GATE:
                            {
                                if (cyclegameitems)
                                {
                                    int effectID = Convert.ToInt32(Item.team + 39);
                                    TeamManager t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

                                    if (User.Team == TEAM.NONE)
                                    {
                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            if (User.Team != TEAM.NONE)
                                                t.OnUserLeave(User);
                                            User.Team = Item.team;
                                            t.AddUser(User);

                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                        }
                                    }
                                    else if (User.Team != TEAM.NONE && User.Team != Item.team)
                                    {
                                        t.OnUserLeave(User);
                                        User.Team = TEAM.NONE;
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    else
                                    {
                                        //usersOnTeam--;
                                        t.OnUserLeave(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        User.Team = TEAM.NONE;
                                    }
                                    //Item.ExtraData = usersOnTeam.ToString();
                                    //Item.UpdateState(false, true);                                
                                }
                                break;
                            }
                        #endregion

                        #region Banzai Teles
                        case InteractionType.banzaitele:
                            {
                                if (User.Statusses.ContainsKey("mv"))
                                    _room.GetGameItemHandler().onTeleportRoomUserEnter(User, Item);
                                break;
                            }
                        #endregion

                        #region Football Gate

                        #endregion

                        #region Effects
                        case InteractionType.EFFECT:
                            {
                                if (User == null)
                                    return;

                                if (!User.IsBot)
                                {
                                    if (Item == null || Item.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                                        return;

                                    if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                        return;

                                    User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                    Item.ExtraData = "1";
                                    Item.UpdateState(false, true);
                                    Item.RequestUpdate(2, true);
                                }
                                break;
                            }
                        #endregion

                        #region Arrows
                        #region Original Algorithm (OFF)
                        /*
                        case InteractionType.ARROW:
                            {
                                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                        continue;

                                    Room Room;

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                        return;

                                    if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                        User.UnlockWalking();
                                    else
                                    {
                                        int LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id, Room);
                                        int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);

                                        if (TeleRoomId == Room.RoomId)
                                        {
                                            Item TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                            if (TargetItem == null)
                                            {
                                                if (User.GetClient() != null)
                                                    User.GetClient().SendWhisper("Hey, that arrow is poorly!");
                                                return;
                                            }
                                            else
                                            {
                                                Room.GetGameMap().TeleportToItem(User, TargetItem);
                                            }
                                        }
                                        else if (TeleRoomId != Room.RoomId)
                                        {
                                            if (User != null && !User.IsBot && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                            {
                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                User.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                User.GetClient().GetHabbo().TeleporterId = LinkedTele;

                                                User.GetClient().GetHabbo().PrepareRoom(TeleRoomId, "");
                                            }
                                        }
                                        else if (this._room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                        {
                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                            User.SetRot(Item.Rotation, false);
                                        }
                                        else
                                            User.UnlockWalking();
                                    }
                                }
                                break;
                            }
                        */
                        #endregion
                        case InteractionType.ARROW:
                            {
                                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                                {
                                    //if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().IsTeleporting)
                                    if (User == null || (User.GetClient() != null && User.GetClient().GetHabbo() == null && User.GetClient().GetHabbo().IsTeleporting))
                                        continue;

                                    Room Room = _room;

                                    //if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                      //  break;
                                                                        
                                    #region RP Conditions
                                    if (!User.IsBot)
                                    {
                                        if ((User.GetClient().GetPlay().IsJailed && !Room.IsPrison && !Room.IsPrison2))
                                        {
                                            User.GetClient().SendWhisper("No puedes escapar de prisión.", 1);
                                            break;
                                        }

                                        if (User.GetClient().GetPlay().IsDead)
                                        {
                                            User.GetClient().SendWhisper("No puedes ir a otra zona estando muert@.", 1);
                                            break;
                                        }

                                        #region Not able to use the Arrow when driving or as a passanger *Zedd*
                                        //Aseguramos que no pueda entrar a establecimientos con auto
                                        if (Item.BaseItem == 7908 && User.GetClient().GetPlay().DrivingCar)
                                        {
                                            User.GetClient().SendWhisper("No puedes entrar a establecimientos con tu vehículo en marcha.", 1);
                                            if(!User.GetClient().GetPlay().Chofer)
                                                User.MoveTo(Item.SquareInFront);
                                            break;
                                        }

                                        //Aseguramos que no pueda entrar a establecimientos con auto
                                        if (Item.BaseItem == 7908 && User.GetClient().GetPlay().Pasajero)
                                        {
                                            User.GetClient().SendWhisper("No puedes entrar a establecimientos como pasajero", 1);
                                            //User.MoveTo(Item.SquareInFront);
                                            break;
                                        }
                                        #endregion Not able to use the Arrow when driving or as a passanger *Zedd

                                        User.ClearMovement(true);
                                    }
                                    #endregion

                                    if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                        User.UnlockWalking();
                                    else
                                    {
                                        int LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id, Room);
                                        int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);
                                        // For Bots. No used now.
                                        /*
                                        if (User.GetClient() != null)
                                        {

                                            object[] Bits = new object[6];
                                            Bits[0] = Item.GetX;
                                            Bits[1] = Item.GetY;
                                            Bits[2] = Item.RoomId;
                                            Bits[3] = TeleRoomId;
                                            Bits[4] = Item.Id;
                                            Bits[5] = LinkedTele;
                                            
                                            //EventManager.TriggerEvent("OnTeleport", User.GetClient(), Bits);

                                        }
                                        */
                                        if (TeleRoomId == Room.RoomId)
                                        {
                                            Item TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                            if (TargetItem == null)
                                            {
                                                if (User.GetClient() != null)
                                                    User.GetClient().SendWhisper("((No se ha encontrado el destino de esta flecha))", 1);
                                                break;
                                            }
                                            else
                                            {
                                                if (User.GetClient() != null && User.GetClient().GetHabbo().EscortID > 0)
                                                {
                                                    RoomUser police = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(User.GetClient().GetHabbo().EscortID);
                                                    if (police != null)
                                                    {
                                                        Room.GetGameMap().TeleportToItem(police, TargetItem);
                                                        police.MoveTo(TargetItem.SquareInFront.X, TargetItem.SquareInFront.Y);
                                                    }
                                                }

                                                if (User.GetClient() != null &&  User.GetClient().GetPlay().Chofer)
                                                {
                                                    #region Pasajeros
                                                    //Vars
                                                    string Pasajeros = User.GetClient().GetPlay().Pasajeros;
                                                    string[] stringSeparators = new string[] { ";" };
                                                    string[] result;
                                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                                    foreach (string psjs in result)
                                                    {
                                                        GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                                        if (PJ != null)
                                                        {
                                                            if (PJ.GetPlay().ChoferName == User.GetClient().GetHabbo().Username)
                                                            {
                                                                Room.GetGameMap().TeleportToItem(PJ.GetRoomUser(), TargetItem);
                                                                //PJ.GetRoomUser().MoveTo(TargetItem.SquareBehind.X, TargetItem.SquareBehind.Y);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }

                                                if (User.GetClient() != null && User.GetClient().GetHabbo().TaxiChofer > 0)
                                                {
                                                    RoomUser taxibot = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetBotByName("Taxi#" + User.GetClient().GetHabbo().Username);
                                                    if (taxibot != null)
                                                    {
                                                        Room.GetGameMap().TeleportToItem(taxibot, TargetItem);
                                                    }
                                                }

                                                if (User.IsBot && User.BotData.AiType == BotAIType.TAXIBOT)
                                                {
                                                    string PassengerName = User.BotData.Name.Split('#')[1];
                                                    RoomUser Passenger = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PassengerName).GetRoomUser();

                                                    if (Passenger != null)
                                                    {
                                                        Room.GetGameMap().TeleportToItem(Passenger, TargetItem);
                                                    }
                                                }

                                                Room.GetGameMap().TeleportToItem(User, TargetItem);
                                            }

                                            // Empujamos al Usuario OFF
                                            #region Move User From    
                                            //if (User.GetClient().GetHabbo().EscortID <= 0)
                                              //  User.MoveTo(TargetItem.SquareBehind.X, TargetItem.SquareBehind.Y);
                                            #endregion
                                        }
                                        else if (TeleRoomId != Room.RoomId)
                                        {
                                            #region Turf Check
                                            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetPlay() != null)
                                            {
                                                bool CheckTurf = false;
                                                string TypeCheck = "";
                                                if (User.GetClient().GetHabbo().EscortID > 0)
                                                {
                                                    TypeCheck = "Escorted";
                                                    CheckTurf = true;
                                                }
                                                else if (User.GetClient().GetPlay().IsWorking && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(User.GetClient(), "stun"))
                                                {
                                                    TypeCheck = "Police";
                                                    CheckTurf = true;
                                                }
                                                else if (User.GetClient().GetPlay().PassiveMode)
                                                {
                                                    TypeCheck = "Passive";
                                                    CheckTurf = true;
                                                }

                                                if (CheckTurf)
                                                {
                                                    RoomData CheckRoom = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(TeleRoomId);
                                                    if (CheckRoom == null)
                                                        User.UnlockWalking();
                                                    else if (CheckRoom.TurfEnabled)
                                                    {
                                                        if (TypeCheck == "Police")
                                                            User.GetClient().SendWhisper("No puedes entrar a barrios mientras trabajas de policía.", 1);
                                                        else if (TypeCheck == "Passive")
                                                            User.GetClient().SendWhisper("No puedes entrar a barrios mientras estás en modo pasivo.", 1);

                                                        break;
                                                    }
                                                }
                                            }
                                            #endregion

                                            if (User != null && !User.IsBot && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                            {
                                                RoomUser police = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(User.GetClient().GetHabbo().EscortID);
                                                if (police != null)
                                                {
                                                    police.GetClient().GetHabbo().IsTeleporting = true;
                                                    police.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                    police.GetClient().GetHabbo().TeleporterId = LinkedTele;
                                                    RoleplayManager.SendUser(police.GetClient(), TeleRoomId);
                                                }

                                                if (User.GetClient().GetPlay().Chofer)
                                                {
                                                    #region Pasajeros
                                                    //Vars
                                                    string Pasajeros = User.GetClient().GetPlay().Pasajeros;
                                                    string[] stringSeparators = new string[] { ";" };
                                                    string[] result;
                                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                                    foreach (string psjs in result)
                                                    {
                                                        GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                                        if (PJ != null)
                                                        {
                                                            if (PJ.GetPlay().ChoferName == User.GetClient().GetHabbo().Username)
                                                            {
                                                                PJ.GetHabbo().IsTeleporting = true;
                                                                PJ.GetHabbo().TeleportingRoomID = TeleRoomId;
                                                                PJ.GetHabbo().TeleporterId = LinkedTele;
                                                                RoleplayManager.SendUser(PJ, TeleRoomId);
                                                                //User.MoveTo(User.SquareInFront.X, User.SquareInFront.Y);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }

                                                if (User.GetClient().GetHabbo().TaxiChofer > 0)
                                                {
                                                    RoomUser taxibot = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetBotByName("Taxi#" + User.GetClient().GetHabbo().Username);
                                                    if (taxibot != null)
                                                    {
                                                        // Remove bot from room
                                                        _room.GetGameMap().RemoveUserFromMap(taxibot, new Point(taxibot.X, taxibot.Y));
                                                        _room.GetRoomUserManager().RemoveBot(taxibot.VirtualId, false);
                                                    }
                                                }

                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                User.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                User.GetClient().GetHabbo().TeleporterId = LinkedTele;
                                                RoleplayManager.SendUser(User.GetClient(), TeleRoomId);
                                            }
                                            else if (User.IsBot && User.BotData.AiType == BotAIType.TAXIBOT)
                                            {
                                                string PassengerName = User.BotData.Name.Split('#')[1];

                                                // Remove bot from room
                                                _room.GetGameMap().RemoveUserFromMap(User, new Point(User.X, User.Y));
                                                _room.GetRoomUserManager().RemoveBot(User.VirtualId, false);

                                                GameClient Passenger = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PassengerName);
                                                if (Passenger != null)
                                                {
                                                    Passenger.GetHabbo().IsTeleporting = true;
                                                    Passenger.GetHabbo().TeleportingRoomID = TeleRoomId;
                                                    Passenger.GetHabbo().TeleporterId = LinkedTele;
                                                    RoleplayManager.SendUser(Passenger, TeleRoomId);
                                                }
                                            }
                                        }
                                        else if (this._room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                        {
                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                            User.SetRot(Item.Rotation, false);
                                        }
                                        else
                                            User.UnlockWalking();
                                    }
                                }
                                break;
                            }
                            #endregion
                    }
                }

                if (User.isSitting && User.TeleportEnabled)
                {
                    User.Z -= 0.35;
                    User.UpdateNeeded = true;
                }

                if (cyclegameitems)
                {
                    if (_room.GotSoccer())
                        _room.GetSoccer().OnUserWalk(User);

                    if (_room.GotBanzai())
                        _room.GetBanzai().OnUserWalk(User);

                    if (_room.GotFreeze())
                        _room.GetFreeze().OnUserWalk(User);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            if (User == null || User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                return;

            try
            {
                byte NewCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (NewCurrentUserItemEffect > 0)
                {
                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                        User.CurrentItemEffect = ItemEffectType.NONE;

                    ItemEffectType Type = ByteToItemEffectEnum.Parse(NewCurrentUserItemEffect);
                    if (Type != User.CurrentItemEffect)
                    {
                        switch (Type)
                        {
                            case ItemEffectType.Iceskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                    User.CurrentItemEffect = ItemEffectType.Iceskates;
                                    break;
                                }

                            case ItemEffectType.Normalskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SWIM:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimLow:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }

                            case ItemEffectType.NONE:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                        }
                    }
                }
                else if (User.CurrentItemEffect != ItemEffectType.NONE && NewCurrentUserItemEffect == 0)
                {
                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                    User.CurrentItemEffect = ItemEffectType.NONE;
                }
            }
            catch
            {
            }
        }

        public int PetCount
        {
            get { return petCount; }
        }

        public ICollection<RoomUser> GetUserList()
        {
            return this._users.Values;
        }

        public int SquareInFront(int X, int Y, int RotBody, string find)
        {
            int Sq = 0;
            if (RotBody == 0)
            {
                Y--;
            }
            else if (RotBody == 2)
            {
                X++;
            }
            else if (RotBody == 4)
            {
                Y++;
            }
            else if (RotBody == 6)
            {
                X--;
            }
            if (find == "x") Sq = X;
            else Sq = Y;
            return Sq;
        }
    }
}
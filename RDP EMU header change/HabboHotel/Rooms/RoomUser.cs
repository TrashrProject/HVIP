using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.Games;

using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Incoming;

using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUser
    {
        public bool AllowOverride;
        public BotAI BotAI;
        public RoomBot BotData;
        public bool CanWalk;
        public int CarryItemID; //byte
        public int CarryTimer; //byte
        public int ChatSpamCount = 0;
        public int ChatSpamTicks = 16;
        public ItemEffectType CurrentItemEffect;
        public int DanceId;
        public bool FastWalking = false;
        public bool SuperFastWalking = false;
        public int FreezeCounter;
        public int FreezeLives;
        public bool Freezed;
        public bool Frozen;
        public int GateId;

        public int GoalX; //byte
        public int GoalY; //byte
        public int HabboId;
        public int HorseID = 0;
        public int IdleTime; //byte
        public bool InteractingGate;
        public int InternalRoomID;
        public bool IsAsleep;
        public bool IsWalking;
        public int LastBubble = 0;
        public int LastEffect = 0;
        public double LastInteraction;
        public Item LastItem = null;
        public int LockedTilesCount;

        public List<Vector2D> Path = new List<Vector2D>();
        public bool PathRecalcNeeded = false;
        public int PathStep = 1;
        public Pet PetData;

        public int PrevTime;
        public bool RidingHorse = false;
        public int RoomId;
        public int RotBody; //byte
        public int RotHead; //byte

        public bool SetStep;
        public int SetX; //byte
        public int SetY; //byte
        public double SetZ;
        public double SignTime;
        public byte SqState;
        public Dictionary<string, string> Statusses;
        public int TeleDelay; //byte
        public bool TeleportEnabled;
        public bool UpdateNeeded;
        public int VirtualId;

        public int X; //byte
        public int Y; //byte
        public double Z;

        public FreezePowerUp banzaiPowerUp;
        public bool isLying = false;
        public bool isSitting = false;
        private GameClient mClient;
        private Room mRoom;
        public bool moonwalkEnabled = false;
        public bool shieldActive;
        public int shieldCounter;
        public TEAM Team;
        public bool FreezeInteracting;
        public int UserId;
        public bool IsJumping;

        public bool isRolling = false;
        public int rollerDelay = 0;

        public int LLPartner = 0;
        public double TimeInRoom = 0;

        public RoomUser(int HabboId, int RoomId, int VirtualId, Room room)
        {
            this.Freezed = false;
            this.HabboId = HabboId;
            this.RoomId = RoomId;
            this.VirtualId = VirtualId;
            this.IdleTime = 0;

            this.X = 0;
            this.Y = 0;
            this.Z = 0;
            this.PrevTime = 0;
            this.RotHead = 0;
            this.RotBody = 0;
            this.UpdateNeeded = true;
            this.Statusses = new Dictionary<string, string>();

            this.TeleDelay = -1;
            this.mRoom = room;

            this.AllowOverride = false;
            this.CanWalk = true;


            this.SqState = 3;

            this.InternalRoomID = 0;
            this.CurrentItemEffect = ItemEffectType.NONE;

            this.FreezeLives = 0;
            this.InteractingGate = false;
            this.GateId = 0;
            this.LastInteraction = 0;
            this.LockedTilesCount = 0;

            this.IsJumping = false;
            this.TimeInRoom = 0;
        }


        public Point Coordinate
        {
            get { return new Point(X, Y); }
        }

        public bool IsPet
        {
            get { return (IsBot && BotData.IsPet); }
        }

        public int CurrentEffect
        {
            get {
                if (GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().Effects() == null)
                    return 0;
                else
                    return GetClient().GetHabbo().Effects().CurrentEffect; 
            }
        }


        public bool IsDancing
        {
            get
            {
                if (DanceId >= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public bool NeedsAutokick
        {
            get
            {
                if (IsBot)
                    return false;

                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return true;

                if (GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || GetRoom().OwnerId == HabboId)
                    return false;

                if (GetRoom().Id == 1649919)
                    return false;

                if (IdleTime >= 7200)
                    return true;

                return false;
            }
        }

        public bool IsTrading
        {
            get
            {
                if (IsBot)
                    return false;

                if (Statusses.ContainsKey("trd"))
                    return true;

                return false;
            }
        }

        public bool IsBot
        {
            get
            {
                if (BotData != null)
                    return true;

                return false;
            }
        }

        public string GetUsername()
        {
            if (IsBot)
                return string.Empty;

            if (GetClient() != null)
            {
                if (GetClient().GetHabbo() != null)
                {
                    return GetClient().GetHabbo().Username;
                }
                else
                    return PlusEnvironment.GetUsernameById(HabboId);

            }
            else
                return PlusEnvironment.GetUsernameById(HabboId);
        }

        public void UnIdle()
        {
            /* OLD OFF
            if (!IsBot)
            {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                    GetClient().GetHabbo().TimeAFK = 0;
            }

            IdleTime = 0;

            if (IsAsleep)
            {
                IsAsleep = false;
                GetRoom().SendMessage(new SleepComposer(this, false));
            }
            */
            if (!IsBot)
            {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                    GetClient().GetHabbo().TimeAFK = 0;
            }

            IdleTime = 0;

            if (IsAsleep)
            {
                IsAsleep = false;
                GetRoom().SendMessage(new SleepComposer(this, false));

                if (GetClient() != null && GetClient().GetHabbo() != null && GetClient().GetPlay() != null)
                {
                    if (!GetClient().GetPlay().IsJailed && !GetClient().GetPlay().IsDead)
                    {
                        GetClient().GetHabbo().Motto = "Ciudadan@";
                        GetClient().GetHabbo().Poof(false);
                    }
                }
            }
        }

        public void Dispose()
        {
            Statusses.Clear();
            mRoom = null;
            mClient = null;
        }

        public void Chat(string Message, bool Shout, int colour = 0)
        {
            if (GetRoom() == null)
                return;

            if (!IsBot)
                return;


            if (IsPet)
            {
                foreach (RoomUser User in GetRoom().GetRoomUserManager().GetUserList().ToList())
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!User.GetClient().GetHabbo().AllowPetSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, 0));
                }
            }
            else
            {
                foreach (RoomUser User in GetRoom().GetRoomUserManager().GetUserList().ToList())
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!User.GetClient().GetHabbo().AllowBotSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, (colour == 0 ? 2 : colour)));
                }
            }
        }

        public void HandleSpamTicks()
        {
            if (ChatSpamTicks >= 0)
            {
                ChatSpamTicks--;

                if (ChatSpamTicks == -1)
                {
                    ChatSpamCount = 0;
                }
            }
        }

        public bool IncrementAndCheckFlood(out int MuteTime)
        {
            MuteTime = 0;

            ChatSpamCount++;
            if (ChatSpamTicks == -1)
                ChatSpamTicks = 8;
            else if (ChatSpamCount >= 6)
            {
                if (GetClient().GetHabbo().GetPermissions().HasRight("events_staff"))
                    MuteTime = 3;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("gold_vip"))
                    MuteTime = 7;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("silver_vip"))
                    MuteTime = 10;
                else
                    MuteTime = 20;

                GetClient().GetHabbo().FloodTime = PlusEnvironment.GetUnixTimestamp() + MuteTime;

                ChatSpamCount = 0;
                return true;
            }
            return false;
        }

        public void OnChat(int Colour, string Message, bool Shout)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || mRoom == null || Message == null)
                return;

            if (mRoom.GetWired().TriggerEvent(Items.Wired.WiredBoxType.TriggerUserSays, GetClient().GetHabbo(), Message))
            {
                ChatSpamCount = 0;
                return;
            }
            if (mRoom.GetWired().TriggerEvent(Items.Wired.WiredBoxType.TriggerUserSaysCommand, GetClient().GetHabbo(), Message))
            {
                ChatSpamCount = 0;
                return;
            }


            GetClient().GetHabbo().HasSpoken = true;

            if (mRoom.WordFilterList.Count > 0 && !GetClient().GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                Message = mRoom.GetFilter().CheckMessage(Message);
            }

            string ColouredMessage = Message;
            if (!string.IsNullOrEmpty(GetClient().GetHabbo().chatColour))
            {
                ColouredMessage = "@" + GetClient().GetHabbo().chatColour + "@" + Message;
            }

            ServerPacket Packet = null;
            if (Shout)
                Packet = new ShoutComposer(VirtualId, ColouredMessage, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
            else
                Packet = new ChatComposer(VirtualId, ColouredMessage, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);


            if (GetClient().GetHabbo().TentId > 0)
            {
                mRoom.SendToTent(GetClient().GetHabbo().Id, GetClient().GetHabbo().TentId, Packet);

                Packet = new WhisperComposer(this.VirtualId, "[Tent Chat] " + Message, 0, Colour);

                List<RoomUser> ToNotify = mRoom.GetRoomUserManager().GetRoomUserByRank(2);

                if (ToNotify.Count > 0)
                {
                    foreach (RoomUser user in ToNotify)
                    {
                        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null ||
                            user.GetClient().GetHabbo().TentId == GetClient().GetHabbo().TentId)
                        {
                            continue;
                        }

                        user.GetClient().SendMessage(Packet);
                    }
                }
            }
            else
            {
                SendNameColourPacket();
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetRoomUsers().ToList())
                {
                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().MutedUsers.Contains(mClient.GetHabbo().Id))
                        continue;

                    if (mRoom.chatDistance > 0 && Gamemap.TileDistance(this.X, this.Y, User.X, User.Y) > mRoom.chatDistance)
                        continue;

                    User.GetClient().SendMessage(Packet);
                }
                SendNamePacket();
            }

            #region Pets/Bots responces
            if (Shout)
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (!User.IsBot)
                        continue;

                    if (User.IsBot)
                        User.BotAI.OnUserShout(this, Message);
                }
            }
            else
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (!User.IsBot)
                        continue;

                    if (User.IsBot)
                        User.BotAI.OnUserSay(this, Message);
                }
            }
            #endregion

        }

        public void ClearMovement(bool Update)
        {
            IsWalking = false;
            Statusses.Remove("mv");
            GoalX = 0;
            GoalY = 0;
            SetStep = false;
            SetX = 0;
            SetY = 0;
            SetZ = 0;

            if (Update)
            {
                UpdateNeeded = true;
            }
        }

        public void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        public void MoveTo(int pX, int pY, bool pOverride)
        {
            if (TeleportEnabled)
            {
                UnIdle();
                GetRoom().SendMessage(GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, new Point(pX, pY), 0, GetRoom().GetGameMap().SqAbsoluteHeight(GoalX, GoalY)));
                if (Statusses.ContainsKey("sit"))
                    Z -= 0.35;
                UpdateNeeded = true;
                return;
            }

            if (/*(GetRoom().GetGameMap().SquareHasUsers(pX, pY) && !pOverride) ||*/ Frozen)
              return;

            UnIdle();

            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
        }

        public void MoveDriving(int pX, int pY, RoomUser Chofer)
        {
            
            UnIdle();

            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
            //Vars
            string Pasajeros = Chofer.GetClient().GetPlay().Pasajeros;
            string[] stringSeparators = new string[] { ";" };
            string[] result;
            result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string psjs in result)
            {
                GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                if (PJ != null && PJ.GetRoomUser() != null)
                {
                    PJ.GetRoomUser().UnIdle();

                    PJ.GetRoomUser().GoalX = pX;
                    PJ.GetRoomUser().GoalY = pY;
                    PJ.GetRoomUser().PathRecalcNeeded = true;
                    PJ.GetRoomUser().FreezeInteracting = false;
                }
            }
        }

        public void MovePoliceEscort(int pX, int pY, RoomUser Police)
        {

            UnIdle();

            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
            
            if (Police != null && !Police.IsWalking)
            {
                Police.UnIdle();
                int UsBhX = pX, UsBhY = pY;
                #region Calc Behind
                if (RotBody == 0)
                {
                    UsBhY++;
                }
                if (RotBody == 2)
                {
                    UsBhX--;
                }
                if (RotBody == 4)
                {
                    UsBhY--;
                }
                if (RotBody == 6)
                {
                    UsBhX++;
                }
                #endregion
                Police.GoalX = UsBhX;
                Police.GoalY = UsBhY;
                Police.PathRecalcNeeded = true;
                Police.FreezeInteracting = false;
            }
        }

        public void MoveConvictEscort(int pX, int pY, RoomUser Police)
        {

            UnIdle();

            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
            GameClient Criminal = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(GetClient().GetPlay().EscortedName);
            RoomUser Convicto = Criminal.GetRoomUser();
            if (Convicto != null)
            {
                Convicto.UnIdle();
                Convicto.GoalX = pX;
                Convicto.GoalY = pY;
                //Convicto.GoalX = SquareFindInFront(pX, pY, RotBody, "x");
                //Convicto.GoalY = SquareFindInFront(pX, pY, RotBody, "y");
                Convicto.PathRecalcNeeded = true;
                Convicto.FreezeInteracting = false;
            }
        }


        public void MoveTo(int pX, int pY)
        {
            MoveTo(pX, pY, false);
        }

        public void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }


        public void SetPos(int pX, int pY, double pZ)
        {
            X = pX;
            Y = pY;
            Z = pZ;
        }

        public void CarryItem(int Item)
        {
            CarryItemID = Item;

            if (Item > 0)
                CarryTimer = 240;
            else
                CarryTimer = 0;

            GetRoom().SendMessage(new CarryObjectComposer(VirtualId, Item));
        }


        public void SetRot(int Rotation, bool HeadOnly)
        {
            if (Statusses.ContainsKey("lay") || IsWalking)
            {
                return;
            }

            int diff = RotBody - Rotation;

            RotHead = RotBody;

            if (Statusses.ContainsKey("sit") || HeadOnly)
            {
                if (RotBody == 2 || RotBody == 4)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
                else if (RotBody == 0 || RotBody == 6)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
            }
            else if (diff <= -2 || diff >= 2)
            {
                RotHead = Rotation;
                RotBody = Rotation;
            }
            else
            {
                RotHead = Rotation;
            }

            UpdateNeeded = true;
        }

        public void SetStatus(string Key, string Value)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses[Key] = Value;
            }
            else
            {
                AddStatus(Key, Value);
            }
        }

        public void AddStatus(string Key, string Value)
        {
            Statusses[Key] = Value;
        }

        public void RemoveStatus(string Key)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses.Remove(Key);
            }
        }

        public void ApplyEffect(int effectID)
        {
            if (IsBot)
            {
                this.mRoom.SendMessage(new AvatarEffectComposer(VirtualId, effectID));
                return;
            }

            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().Effects() == null)
                return;

            GetClient().GetHabbo().Effects().ApplyEffect(effectID);
        }
        public void SendMeCommandPacket()
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null)
                return;

            string Username = "*" + GetClient().GetHabbo().Username;

            if (!GetClient().GetHabbo().ChatPreference)
            {
                if (GetClient().GetHabbo().chatHTMLColour != null && GetClient().GetHabbo().chatHTMLColour != String.Empty)
                {
                    if (GetClient().GetHabbo().chatHTMLColour.ToLower() == "rainbow")
                        Username = "*" + CommandManager.GenerateRainbowText(GetClient().GetHabbo().Username);
                    else
                        Username = "*<font color='#" + GetClient().GetHabbo().chatHTMLColour + "'>" + GetClient().GetHabbo().Username + "</font>";
                }
            }
            if (GetRoom() != null)
                GetRoom().SendMessage(new UserNameChangeComposer(RoomId, VirtualId, Username));
        }
        public void SendNameColourPacket()
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null)
                return;
            if (GetClient().GetHabbo().chatHTMLColour == null || GetClient().GetHabbo().chatHTMLColour == String.Empty)
                return;
            if (GetClient().GetHabbo().ChatPreference)
                return;
            string Username = "<font size='" + GetClient().GetHabbo().chatHTMLSize + "' color='#" + GetClient().GetHabbo().chatHTMLColour + "'>" + GetClient().GetHabbo().Username + "</font>";

            /*
            if (GetClient().GetHabbo().chatHTMLColour.ToLower() == "rainbow")
                Username = CommandManager.GenerateRainbowText(GetClient().GetHabbo().Username);
            */

            if (GetRoom() != null)
                GetRoom().SendMessage(new UserNameChangeComposer(RoomId, VirtualId, Username));
             
        }

        public void SendNamePacket()
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null)
                return;

            string Username = GetClient().GetHabbo().Username;

            if (GetRoom() != null)
                GetRoom().SendMessage(new UserNameChangeComposer(RoomId, VirtualId, Username));
        }
        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.Y--;
                }
                else if (RotBody == 2)
                {
                    Sq.X++;
                }
                else if (RotBody == 4)
                {
                    Sq.Y++;
                }
                else if (RotBody == 6)
                {
                    Sq.X--;
                }

                return Sq;
            }
        }



        public Point SquareBehind
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.Y++;
                }
                else if (RotBody == 2)
                {
                    Sq.X--;
                }
                else if (RotBody == 4)
                {
                    Sq.Y--;
                }
                else if (RotBody == 6)
                {
                    Sq.X++;
                }

                return Sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.X++;
                }
                else if (RotBody == 2)
                {
                    Sq.Y--;
                }
                else if (RotBody == 4)
                {
                    Sq.X--;
                }
                else if (RotBody == 6)
                {
                    Sq.Y++;
                }

                return Sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.X--;
                }
                else if (RotBody == 2)
                {
                    Sq.Y++;
                }
                else if (RotBody == 4)
                {
                    Sq.X++;
                }
                else if (RotBody == 6)
                {
                    Sq.Y--;
                }
                return Sq;
            }
        }

        public int SquareFindInFront(int X, int Y, int RotBody, string find)
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

        public GameClient GetClient()
        {
            if (IsBot)
            {
                return null;
            }
            if (mClient == null)
                mClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);
            return mClient;
        }

        public Room GetRoom()
        {
            if (mRoom == null)
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out mRoom))
                    return mRoom;

            return mRoom;
        }

        public void warp(int X, int Y, double Z)
        {
            GetRoom().GetGameMap().UpdateUserMovement(new Point(this.X, this.Y), new Point(X, Y), this);
            this.SetStep = true;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.UpdateNeeded = true;
            this.ClearMovement(true);
        }
    }

    public enum ItemEffectType
    {
        NONE,
        SWIM,
        SwimLow,
        SwimHalloween,
        Iceskates,
        Normalskates,
        PublicPool,
        //Skateboard?
    }

    public static class ByteToItemEffectEnum
    {
        public static ItemEffectType Parse(byte pByte)
        {
            switch (pByte)
            {
                case 0:
                    return ItemEffectType.NONE;
                case 1:
                    return ItemEffectType.SWIM;
                case 2:
                    return ItemEffectType.Normalskates;
                case 3:
                    return ItemEffectType.Iceskates;
                case 4:
                    return ItemEffectType.SwimLow;
                case 5:
                    return ItemEffectType.SwimHalloween;
                case 6:
                    return ItemEffectType.PublicPool;
                //case 7:
                //return ItemEffectType.Custom;
                default:
                    return ItemEffectType.NONE;
            }
        }
    }

    //0 = none
    //1 = pool
    //2 = normal skates
    //3 = ice skates
}
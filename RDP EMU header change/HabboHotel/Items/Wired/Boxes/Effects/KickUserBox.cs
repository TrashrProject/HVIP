using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    class KickUserBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.EffectKickUser; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public int TickCount { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public int Delay { get; set; }
        public string ItemsData { get; set; }
        private Queue _toKick;

        public KickUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
            this.TickCount = Delay;
            this._toKick = new Queue();

            if (this.SetItems.Count > 0)
                this.SetItems.Clear();
        }

        public void HandleSave(ClientPacket Packet)
        {
            if (this.SetItems.Count > 0)
                this.SetItems.Clear();

            int Unknown = Packet.PopInt();
            string Message = Packet.PopString();

            this.StringData = Message;
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length != 1)
                return false;

            Habbo Player = (Habbo)Params[0];
            if (Player == null)
                return false;

            if (this.TickCount <= 0)
                this.TickCount = 3;

            if (!this._toKick.Contains(Player))
            {
                RoomUser User = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
                if (User == null)
                    return false;

                if (Player.GetPermissions().HasRight("mod_tool")  || this.Instance.OwnerId == Player.Id)
                {
                    Player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, "Wired Kick Exception: Unkickable Player", 0, 0));
                    return false;
                }

                this._toKick.Enqueue(Player);
                Player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, this.StringData, 0, 0));
            }
            return true;
        }

        public bool OnCycle()
        {
            if (Instance == null)
                return false;

            if (this._toKick.Count == 0)
            {
                this.TickCount = 3;
                return true;
            }

            lock (this._toKick.SyncRoot)
            {
                while (this._toKick.Count > 0)
                {
                    Habbo Player = (Habbo)this._toKick.Dequeue();
                    if (Player == null || !Player.InRoom || Player.CurrentRoom != Instance)
                        continue;

                    var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByInsideRoom(Player.CurrentRoom.RoomId);
                    var ApartInside = PlusEnvironment.GetGame().GetApartmentOwnedManager().GetApartmentByInsideRoom(Player.CurrentRoom.RoomId);

                    #region Casa
                    if (House != null)
                    {
                        // Enviar a la Sala Exterior y Posición de la Puerta
                        Player.GetClient().GetPlay().ExitingHouse = true;
                        Player.GetClient().GetPlay().HouseX = House.DoorX;
                        Player.GetClient().GetPlay().HouseY = House.DoorY;
                        Player.GetClient().GetPlay().HouseZ = House.DoorZ;
                        RoleplayManager.SendUserOld(Player.GetClient(), House.RoomId, "Te han echado de la casa.");
                    }
                    #endregion

                    #region Apartament
                    else if (ApartInside != null)
                    {
                        RoleplayManager.SendUserOld(Player.GetClient(), ApartInside.LobbyId, "Te han echado del apartamento.");
                    }
                    #endregion

                    //Instance.GetRoomUserManager().RemoveUserFromRoom(Player.GetClient(), true, false);
                }
            }
            this.TickCount = 3;
            return true;
        }
    }
}
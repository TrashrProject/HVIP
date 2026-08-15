using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Incoming;
using System;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    public class ActionEvent : IPacketEvent
    {
        #region OLD OFF
        /*
        public void Parse(GameClient Session, ClientPacket Packet)
        {

            if (!Session.GetHabbo().InRoom)
                return;

            int Action = Packet.PopInt();

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (User.DanceId > 0)
                User.DanceId = 0;
            
            // I Think... We don't need this...
            //if (Session.GetHabbo().Effects().CurrentEffect > 0)
              //  Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

            User.UnIdle();
            Room.SendMessage(new ActionComposer(User.VirtualId, Action));

            if (Action == 5) // idle
            {
                User.IsAsleep = true;
                Room.SendMessage(new SleepComposer(User, true));
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_WAVE);
        }
        */
        #endregion

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            if (Session.GetPlay().DrivingCar || Session.GetPlay().Pasajero || Session.GetPlay().EquippedWeapon != null
                || Session.GetPlay().IsFarming || Session.GetPlay().WateringCan || Session.GetPlay().IsDying || Session.GetPlay().IsDead)
                return;

            int Action = Packet.PopInt();

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Action != 1 && Action != 7)
                User.UnIdle();

            if (Action == 3) // giggle, it removes the gun enable
            {
                return;
            }
            else if (Action == 5) // idle
            {
                if (!Session.GetHabbo().GetPermissions().HasCommand("command_idle"))
                {
                    Session.SendWhisper("Lo sentimos, esa acción ha sido inhabilitada para prevenir abusos.", 1);
                    return;
                }
                else
                {
                    if (User.DanceId > 0)
                        User.DanceId = 0;

                    if (Session.GetHabbo().Effects().CurrentEffect > 0)
                        Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

                    User.IsAsleep = true;
                    Room.SendMessage(new SleepComposer(User, true));

                    if (!Session.GetPlay().IsJailed && !Session.GetPlay().IsDead)
                    {
                        Session.GetHabbo().Motto = "[AFK] Ciudadan@";
                        Session.GetHabbo().Poof(false);
                    }
                }
            }
            else
            {
                if (User.DanceId > 0)
                    User.DanceId = 0;

                if (Session.GetHabbo().Effects().CurrentEffect > 0)
                    Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

                Room.SendMessage(new ActionComposer(User.VirtualId, Action));
            }
        }
    }
}
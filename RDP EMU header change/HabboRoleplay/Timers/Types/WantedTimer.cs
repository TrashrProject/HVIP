using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Houses;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to evade police
    /// </summary>
    public class WantedTimer : RoleplayTimer
    {
        public WantedTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 10 minutes converted to miliseconds
            TimeLeft = base.Client.GetPlay().WantedTimeLeft * 60000;
        }
 
        /// <summary>
        /// Removes the user from the wanted list
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetPlay().IsWanted || base.Client.GetPlay().IsJailed)
                {
                    base.Client.GetPlay().IsWanted = false;
                    base.Client.GetPlay().WantedLevel = 0;
                    base.Client.GetPlay().WantedTimeLeft = 0;
                    base.EndTimer();
                    return;
                }
                

                RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(base.Client.GetHabbo().CurrentRoomId);

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                House House;
                if (roomData.TurfEnabled || base.Client.GetHabbo().CurrentRoom.TryGetHouse(out House))
                    return;


                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetPlay().WantedTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        base.Client.SendWhisper("Te resta(n) " + (TimeLeft / 60000) + " minuto(s) para evadir a la policía.", 1);
                        //base.Client.SendMessage(new RoomNotificationComposer("wanted_level", "message", "resta(n) " + (TimeLeft / 60000) + " minuto(s) para evadir a la policía."));
                        TimeCount = 0;
                    }
                    return;
                }

                base.Client.GetPlay().IsWanted = false;
                base.Client.GetPlay().WantedLevel = 0;
                base.Client.GetPlay().WantedTimeLeft = 0;
                //base.Client.GetPlay().Evasions++;
                //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(base.Client, "ACH_Evasions", 1);

                Wanted Junk;
                RoleplayManager.WantedList.TryRemove(base.Client.GetHabbo().Id, out Junk);
                PlusEnvironment.GetGame().GetClientManager().JailAlert("[RADIO] " + base.Client.GetHabbo().Username + " ha evadido a sus cargos legales. ¡Mejor suerte para la próxima vez!");
                RoleplayManager.Shout(base.Client, "*Ha evadido a la policía*", 5);
                base.Client.GetPlay().Evasions++;
                //base.Client.SendMessage(new RoomNotificationComposer("evasion_success_notice", "message", "Has evadido a las leyes de las autoridades exitosamente."));
                base.EndTimer();
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}
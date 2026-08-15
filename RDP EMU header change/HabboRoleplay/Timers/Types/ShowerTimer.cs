using System;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Begins showering
    /// </summary>
    public class ShowerTimer : RoleplayTimer
    {
        public ShowerTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            if (Client.GetPlay().Hygiene > 100)
                Client.GetPlay().Hygiene = 100;

            if (Client.GetPlay().Hygiene < 0)
                Client.GetPlay().Hygiene = 0;

            TimeLeft = (100 - Client.GetPlay().Hygiene) * 1000;
        }

        /// <summary>
        /// Executes shower tick
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

                if (base.Client.GetRoomUser() == null || base.Client.GetRoomUser().GetRoom() == null)
                {
                    base.Client.GetPlay().InShower = false;
                    base.EndTimer();
                    return;
                }

                int ItemId = (int)Params[0];
                Item Shower = base.Client.GetRoomUser().GetRoom().GetRoomItemHandler().GetItem(ItemId);

                if (Shower == null || !base.Client.GetPlay().InShower || Shower.Coordinate != base.Client.GetRoomUser().Coordinate)
                {
                    RoleplayManager.Shout(base.Client, "*Ha dejado de bañarse*", 5);
                    base.Client.GetPlay().InShower = false;
                    base.EndTimer();
                    return;
                }

                TimeLeft -= 1000;
                base.Client.GetPlay().Hygiene++;

                if (TimeLeft > 0)
                    return;

                Point OffShower = new Point(Shower.SquareInFront.X, Shower.SquareInFront.Y);
                base.Client.GetRoomUser().MoveTo(OffShower);

                RoleplayManager.Shout(base.Client, "*Se siente refrescad@ después de esa gran ducha*", 5);
                base.Client.GetPlay().InShower = false;
                base.EndTimer();
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}
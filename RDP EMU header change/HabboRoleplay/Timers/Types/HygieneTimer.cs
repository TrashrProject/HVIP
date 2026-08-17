using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizens cleanliness decrease over time
    /// </summary>
    public class HygieneTimer : RoleplayTimer
    {
        public HygieneTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {

        }
 
        /// <summary>
        /// Decreases the users hygiene
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

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client != null && base.Client.GetRoomUser() != null && base.Client.GetRoomUser().IsAsleep)
                    return;

                
                if (base.Client.GetPlay().InShower)
                    return;

                TimeCount++;

                if (TimeCount < ((base.Client.GetHabbo().VIPRank == 2) ? (RoleplayManager.HygieneTime * 2) : RoleplayManager.HygieneTime))// 500 segundos para disminuir la higiene
                    return;

                TimeCount = 0;

                if (base.Client.GetPlay().Hygiene <= 0)
                {
                    if (base.Client.GetRoomUser() != null)
                        base.Client.GetRoomUser().ApplyEffect(10);
                    

                    //base.Client.SendWhisper("You really do smell! You better hurry up and take a shower!", 1);
                    base.Client.SendMessage(new RoomNotificationComposer("hygiene_low_warning", "message", "Algo huele mal... ¿No? ¡Toma un baño urgentemente para mejorar tu Higiene!"));
                    return;
                }

                int AmountOfHygiene = Random.Next(1, 5);

                if (base.Client.GetPlay().Hygiene - AmountOfHygiene <= 0)
                    base.Client.GetPlay().Hygiene = 0;
                else
                    base.Client.GetPlay().Hygiene -= AmountOfHygiene;

                if (base.Client.GetPlay().Hygiene > 0)
                    return;

                if (base.Client.GetRoomUser() != null)
                    base.Client.GetRoomUser().ApplyEffect(10);
                //base.Client.SendWhisper("You start to really stink! You've got to do something about this smell!", 1);
                base.Client.SendMessage(new RoomNotificationComposer("hygiene_low_warning", "message", "¡Estás comenzando a apestar! Toma un baño pronto para quitarte esas moscas."));
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}
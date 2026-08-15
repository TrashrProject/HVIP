using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Cooldowns.Types
{
    /// <summary>
    /// Default cooldown
    /// </summary>
    public class DefaultCooldown : Cooldown
    {
        public DefaultCooldown(string Type, GameClient Client, int Time, int Amount) 
            : base(Type, Client, Time, Amount)
        {
            TimeLeft = Amount * 1000;
        }
 
        /// <summary>
        /// Removes the cooldown
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetPlay() == null || base.Client.GetHabbo() == null)
                {
                    base.EndCooldown();
                    return;
                }

                TimeLeft -= 1000;

                if (Type.ToLower() == "reload" && TimeLeft > 0)
                    base.Client.SendWhisper("Recargando: " + (TimeLeft / 1000) + "/" + Amount, 1);

                if (TimeLeft > 0)
                    return;

                if (Type.ToLower() == "weed")
                {
                    RoleplayManager.Shout(base.Client, "*Feels their weed high wear out*", 4);
                    //base.Client.GetPlay().HighOffWeed = false;
                }
                else if (Type.ToLower() == "cocaine")
                {
                    RoleplayManager.Shout(base.Client, "*Feels their cocaine high wear out*", 4);
                    //base.Client.GetPlay().HighOffCocaine = false;
                }
                else if (Type.ToLower() == "reload")
                    base.Client.SendWhisper("¡Arma Recargada!", 1);

                if (base.Client.GetPlay().SpecialCooldowns.ContainsKey(Type.ToLower()))
                    base.Client.GetPlay().SpecialCooldowns.TryUpdate(Type.ToLower(), TimeLeft, base.Client.GetPlay().SpecialCooldowns[Type.ToLower()]);

                base.EndCooldown();
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error in Execute() void: " + e);
            }
        }
    }
}
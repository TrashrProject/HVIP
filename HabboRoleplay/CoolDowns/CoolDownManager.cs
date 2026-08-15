using System;
using System.Linq;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Cooldowns.Types;

namespace Plus.HabboRoleplay.Cooldowns
{
    public class CooldownManager
    {
        /// <summary>
        /// The client
        /// </summary>
        public GameClient Client;

        /// <summary>
        /// Contains all running cooldowns
        /// </summary>
        public ConcurrentDictionary<string, Cooldown> ActiveCooldowns;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public CooldownManager(GameClient Client)
        {
            this.Client = Client;
            ActiveCooldowns = new ConcurrentDictionary<string, Cooldown>();
        }
        /// <summary>
        /// Creates a timer
        /// </summary>
        public void CreateCooldown(string Type, int Time, int Amount = 1)
        {
            if (ActiveCooldowns.ContainsKey(Type))
                return;

            Cooldown Cooldown = GetCooldownFromType(Type, Time, Amount);

            if (Cooldown == null)
                return;

            ActiveCooldowns.TryAdd(Type, Cooldown);
        }

        /// <summary>
        /// Returns a new cooldown based on the type
        /// </summary>
        /// <param name="TypeOfTimer"></param>
        private Cooldown GetCooldownFromType(string TypeOfCooldown, int Time, int Amount)
        {
            switch (TypeOfCooldown)
            {
                //case "gun":
                  //  return new GunCooldown(TypeOfCooldown, Client, Time, Amount);
                //case "farming":
                  //  return new FarmingCooldown(TypeOfCooldown, Client, Time, Amount);
                default:
                    return new DefaultCooldown(TypeOfCooldown, Client, Time, Amount);
            }
        }

        /// <summary>
        /// Ends all of the cooldowns
        /// </summary>
        public void EndAllCooldowns()
        {
            foreach (Cooldown Cooldown in ActiveCooldowns.Values.ToList())
                Cooldown.EndCooldown();
        }
    }
    

}
using System;
using System.Linq;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Timers.Types;

namespace Plus.HabboRoleplay.Timers
{
    public class TimerManager
    {
        /// <summary>
        /// The client
        /// </summary>
        public GameClient Client;

        /// <summary>
        /// Contains all running timers
        /// </summary>
        public ConcurrentDictionary<string, RoleplayTimer> ActiveTimers;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public TimerManager(GameClient Client)
        {
            this.Client = Client;
            ActiveTimers = new ConcurrentDictionary<string, RoleplayTimer>();

            // Start up our Forever timers
            CreateTimer("hunger", 1000, true);
            CreateTimer("hygiene", 1000, true);
            CreateTimer("conditioncheck", 1000, true);
            //CreateTimer("interest", 1000, true);
        }
        /// <summary>
        /// Creates a timer
        /// </summary>
        public RoleplayTimer CreateTimer(string Type, int Time, bool Forever, params object[] Params)
        {
            if (ActiveTimers.ContainsKey(Type))
                return null;

            RoleplayTimer Timer = GetTimerFromType(Type, Time, Forever, Params);

            if (Timer == null)
                return null;

            ActiveTimers.TryAdd(Type, Timer);

            return Timer;
        }

        /// <summary>
        /// Returns a new timer based on the type
        /// </summary>
        /// <param name="TypeOfTimer"></param>
        private RoleplayTimer GetTimerFromType(string TypeOfTimer, int Time, bool Forever, object[] Params)
        {
            switch (TypeOfTimer)
            {
                case "hunger":
                    return new HungerTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "hygiene":
                    return new HygieneTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "conditioncheck":
                    return new ConditionCheckTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "heal":
                    return new HealTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "shower":
                    return new ShowerTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "work":
                    return new WorkTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "sendhome":
                    return new SendhomeTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "death":
                    return new DeathTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "jail":
                    return new JailTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "stun":
                    return new StunTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "cuff":
                    return new CuffTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "wanted":
                    return new WantedTimer(TypeOfTimer, Client, Time, Forever, Params);                
                case "workout":
                    return new WorkoutTimer(TypeOfTimer, Client, Time, Forever, Params);                
                case "noob":
                    return new NoobTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "dying":
                    return new DyingTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "general":
                    return new GeneralTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "atmrob":
                    return new ATMRobTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "chnban":
                    return new ChNBanTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "taxi":
                    return new TaxiTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "vehiclejob":
                    return new VehicleJobTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "sanc":
                    return new SancTimer(TypeOfTimer, Client, Time, Forever, Params);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Ends all of the timers
        /// </summary>
        public void EndAllTimers()
        {
            lock (ActiveTimers.Values)
            {
                foreach (RoleplayTimer Timer in ActiveTimers.Values)
                    Timer.EndTimer();
            }
        }
    }
    

    public class SystemTimerManager
    {
        /// <summary>
        /// Contains all running timers
        /// </summary>
        public ConcurrentDictionary<string, SystemRoleplayTimer> ActiveTimers;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public SystemTimerManager()
        {
            ActiveTimers = new ConcurrentDictionary<string, SystemRoleplayTimer>();
            //CreateTimer("farmingspace", 1000, true);
            CreateTimer("daynight", 5000, true);
            CreateTimer("payday", 1000, true);
            //CreateTimer("respawnvehicle", 1200000, true);//1200000 = 20 mins.
        }

        /// <summary>
        /// Creates a timer
        /// </summary>
        public SystemRoleplayTimer CreateTimer(string Type, int Time, bool Forever, params object[] Params)
        {
            if (ActiveTimers.ContainsKey(Type))
                return null;

            SystemRoleplayTimer Timer = GetTimerFromType(Type, Time, Forever, Params);

            if (Timer == null)
                return null;

            ActiveTimers.TryAdd(Type, Timer);

            return Timer;
        }

        /// <summary>
        /// Returns a new timer based on the type
        /// </summary>
        /// <param name="TypeOfTimer"></param>
        private SystemRoleplayTimer GetTimerFromType(string TypeOfTimer, int Time, bool Forever, object[] Params)
        {
            switch (TypeOfTimer)
            {
                case "daynight":
                    return new DayNightCycleTimer(TypeOfTimer, Time, Forever, Params);
                case "payday":
                    return new PayDayCycleTimer(TypeOfTimer, Time, Forever, Params);
                case "purge":
                    return new PurgeTimer(TypeOfTimer, Time, Forever, Params);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Ends all of the timers
        /// </summary>
        public void EndAllTimers()
        {
            lock (ActiveTimers.Values)
            {
                foreach (SystemRoleplayTimer Timer in ActiveTimers.Values)
                    Timer.EndTimer();
            }
        }
    }
}
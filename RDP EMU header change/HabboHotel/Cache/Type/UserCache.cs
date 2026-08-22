using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Cache
{
    // Restored from the matching legacy RP emulator lineage
    // (SuupraaaMK4/Plus-Emulator-RP-NacionKeko @ 35cdeb2c195e74eb9e0e98a21549623e75666722).
    public class UserCache : IDisposable
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Motto { get; set; }
        public string Look { get; set; }
        public string SocketStatistics { get; set; }
        public DateTime AddedTime { get; set; }

        public UserCache(int Id, string Username, string Motto, string Look, string SocketStatistics)
        {
            this.Id = Id;
            this.Username = Username;
            this.Motto = Motto;
            this.Look = Look;
            this.AddedTime = DateTime.Now;
            this.SocketStatistics = SocketStatistics;
        }

        public bool isExpired()
        {
            TimeSpan CacheTime = DateTime.Now - this.AddedTime;
            return CacheTime.TotalMinutes >= 30;
        }

        public void Dispose()
        {
            UserCache OutCache = null;
            PlusEnvironment.GetGame().GetCacheManager()._usersCached.TryRemove(this.Id, out OutCache);

            new Thread(() =>
            {
                Thread.Sleep(500);
                this.Id = 0;
                this.Username = null;
                this.Motto = null;
                this.Look = null;
                this.SocketStatistics = null;
            }).Start();
        }
    }
}

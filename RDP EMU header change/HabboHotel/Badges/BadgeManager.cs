using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;

using log4net;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Badges
{
    public class BadgeManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Badges.BadgeManager");

        private readonly Dictionary<string, BadgeDefinition> _badges;

        public BadgeManager()
        {
            this._badges = new Dictionary<string, BadgeDefinition>();
        }

        public void Init()
        {
            log.Info("Loaded badge manager.");
        }

        public bool TryGetBadge(string BadgeCode, out BadgeDefinition Badge)
        {
            return this._badges.TryGetValue(BadgeCode.ToUpper(), out Badge);
        }
    }
}
using log4net;

using Plus.HabboHotel.Groups.Forums.Process;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForumManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Groups.Forums");

        /// <summary>
        /// Timed process to thread views, scores & settings.
        /// </summary>
        private ProcessComponent _process = null;

        public GroupForumManager()
        {
            Init();
            // ??
        }

        public void Init()
        {
            this._process = new ProcessComponent();
            this._process.Init();

            log.Info("Successfully initialized group forum manager.");
        }

        public void Dispose()
        {
            this._process.Dispose();
        }
    }
}
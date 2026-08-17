using System;
using System.Linq;
using System.Data;
using System.Threading;

using log4net;

using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Groups.Forums.Process
{
    public class ProcessComponent
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Groups.Forums.ProcessComponent");

        /// <summary>
        /// ThreadPooled Timer.
        /// </summary>
        private Timer _timer = null;

        /// <summary>
        /// Prevents the timer from overlapping itself.
        /// </summary>
        private bool _timerRunning = false;

        /// <summary>
        /// Checks if the timer is lagging behind (server can't keep up).
        /// </summary>
        private bool _timerLagging = false;

        /// <summary>
        /// Enable/Disable the timer WITHOUT disabling the timer itself.
        /// </summary>
        private bool _disabled = false;

        /// <summary>
        /// Used for disposing the ProcessComponent safely.
        /// </summary>
        private AutoResetEvent _resetEvent = new AutoResetEvent(true);

        /// <summary>
        /// How often the timer should execute.
        /// </summary>
        private static int _runtimeInSec = 60;

        /// <summary>
        /// Default.
        /// </summary>
        public ProcessComponent()
        {
        }

        /// <summary>
        /// Initializes the ProcessComponent.
        /// </summary>
        /// <param name="Player">Player.</param>
        public void Init()
        {
            this._timer = new Timer(new TimerCallback(Run), null, _runtimeInSec * 1000, _runtimeInSec * 1000);
        }

        /// <summary>
        /// Called for each time the timer ticks.
        /// </summary>
        /// <param name="State"></param>
        public void Run(object State)
        {
            if (this._disabled)
                return;

            if (this._timerRunning)
            {
                this._timerLagging = true;
                log.Warn("Server can't keep up, Group Forum Manager timer is lagging behind.");
                return;
            }

            this._resetEvent.Reset();

            // BEGIN CODE

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (PlusEnvironment.GetGame() != null && PlusEnvironment.GetGame().GetGroupManager() != null)
                {
                    foreach (Group group in PlusEnvironment.GetGame().GetGroupManager().Groups.ToList())
                    {
                        if (group.ForumEnabled == false || group.GetForum() == null)
                            continue;

                        if (!group.GetForum().UpdateRequired)
                            continue;

                        dbClient.SetQuery("UPDATE `group_forum_settings` SET `score` = @score WHERE `group_id` = @groupId LIMIT 1");
                        dbClient.AddParameter("score", group.GetForum().Score);
                        dbClient.AddParameter("groupId", group.Id);
                        dbClient.RunQuery();

                        // Is this a stupid idea?
                        foreach (GroupThread thread in group.GetForum().GetThreads())
                        {
                            dbClient.SetQuery("UPDATE `group_forum_threads` SET `views` = @views, `locked` = @locked, `deleted` = @status, `pinned` = @pinned, `moderator_id` = @moderatorId, `updated_at` = @updatedAt WHERE `id` = @threadId LIMIT 1");
                            dbClient.AddParameter("views", thread.Views);
                            dbClient.AddParameter("locked", thread.Locked ? "1" : "0");
                            dbClient.AddParameter("status", thread.Deleted ? "10" : "0");
                            dbClient.AddParameter("pinned", thread.Pinned ? "1" : "0");
                            dbClient.AddParameter("moderatorId", thread.ModeratorId);
                            dbClient.AddParameter("updatedAt", thread.UpdatedAt);
                            dbClient.AddParameter("threadId", thread.Id);
                            dbClient.RunQuery();

                            foreach (GroupPost post in thread.GetPosts())
                            {
                                dbClient.SetQuery("UPDATE `group_forum_posts` SET `deleted` = @deleted, `moderator_id` = @moderatorId WHERE `id` = @postId LIMIT 1");
                                dbClient.AddParameter("deleted", post.Deleted ? "10" : "0");
                                dbClient.AddParameter("moderatorId", post.ModeratorId);
                                dbClient.AddParameter("postId", post.Id);
                                dbClient.RunQuery();
                            }
                        }

                        // Is this a stupid idea?
                        dbClient.SetQuery("UPDATE `group_forum_settings` SET `readability_setting` = @readSettings, `post_creation_setting` = @writeSettings, `thread_creation_setting` = @threadSettings, `moderation_setting` = @moderationSettings, `score` = @score WHERE `group_id` = @groupId LIMIT 1");
                        dbClient.AddParameter("readSettings", group.GetForum().ReadabilitySetting.ToString());
                        dbClient.AddParameter("writeSettings", group.GetForum().PostCreationSetting.ToString());
                        dbClient.AddParameter("threadSettings", group.GetForum().ThreadCreationSetting.ToString());
                        dbClient.AddParameter("moderationSettings", group.GetForum().ForumModerationSetting.ToString());
                        dbClient.AddParameter("score", group.GetForum().Score);
                        dbClient.AddParameter("groupId", group.Id);
                        dbClient.RunQuery();

                        group.GetForum().UpdateRequired = false;
                    }
                }
            }

            // END CODE

            // Reset the values
            this._timerRunning = false;
            this._timerLagging = false;

            this._resetEvent.Set();
        }

        /// <summary>
        /// Stops the timer and disposes everything.
        /// </summary>
        public void Dispose()
        {
            // Wait until any processing is complete first.
            try
            {
                this._resetEvent.WaitOne(TimeSpan.FromMinutes(5));
            }
            catch { } // give up

            // Set the timer to disabled
            this._disabled = true;

            // Dispose the timer to disable it.
            this._timer.Dispose();

            // Remove reference to the timer.
            this._timer = null;
        }
    }
}
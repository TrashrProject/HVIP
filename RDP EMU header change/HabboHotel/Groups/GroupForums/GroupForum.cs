using System;
using System.Data;
using System.Linq;

using Plus.Database.Interfaces;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForum
    {
        public int GroupId { get; private set; }
        public int ReadabilitySetting { get; set; }
        public int PostCreationSetting { get; set; }
        public int ThreadCreationSetting { get; set; }
        public int ForumModerationSetting { get; set; }
        public int Score { get; set; }
        public bool UpdateRequired { get; set; }

        private ConcurrentDictionary<int, GroupThread> _threads;

        public GroupForum(int groupId, int readabilitySetting, int postCreationSetting, int threadCreationSetting, int moderationSetting, int score)
        {
            this.GroupId = groupId;
            this.ReadabilitySetting = readabilitySetting;
            this.PostCreationSetting = postCreationSetting;
            this.ThreadCreationSetting = threadCreationSetting;
            this.ForumModerationSetting = moderationSetting;
            this.Score = score;
            this.UpdateRequired = false;

            this._threads = new ConcurrentDictionary<int, GroupThread>();

            DataTable getThreads = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `group_forum_threads` WHERE `group_id` = @GroupId ORDER BY `id` DESC");
                dbClient.AddParameter("GroupId", this.GroupId);
                getThreads = dbClient.getTable();
            }

            if (getThreads != null)
            {
                foreach (DataRow row in getThreads.Rows)
                {
                    if (!this._threads.TryAdd(Convert.ToInt32(row["id"]), new GroupThread(Convert.ToInt32(row["id"]), Convert.ToInt32(row["group_id"]), Convert.ToString(row["title"]), Convert.ToInt32(row["user_id"]), Convert.ToDouble(row["created_at"]), Convert.ToDouble(row["updated_at"]), PlusEnvironment.EnumToBool(row["pinned"].ToString()), PlusEnvironment.EnumToBool(row["locked"].ToString()), row["deleted"].ToString() == "10", Convert.ToInt32(row["moderator_id"]), Convert.ToInt32(row["views"]))))
                    {
                        // Do something?
                    }
                }
            }
        }

        /// <summary>
        /// This is used for grabbing the last replying user Id from posts inside of threads.
        /// </summary>
        public int LastReplierId
        {
            get
            {
                if (this._threads.Count > 0)
                {
                    return this._threads.OrderByDescending(x => x.Value.CreatedAt).FirstOrDefault().Value.GetPosts().OrderByDescending(x => x.CreatedAt).FirstOrDefault().CreatorId;
                }

                return 0;
            }
        }

        public double LastReplierDate
        {
            get
            {
                if (this._threads.Count > 0)
                {
                    return this._threads.OrderByDescending(x => x.Value.UpdatedAt).FirstOrDefault().Value.GetPosts().OrderByDescending(x => x.CreatedAt).FirstOrDefault().CreatedAt;
                }

                return 0;
            }
        }

        public void AddScore(int score)
        {
            this.Score += score;
        }

        public bool TryAddThread(int threadId, GroupThread thread)
        {
            return this._threads.TryAdd(threadId, thread);
        }

        public bool TryRemoveThread(int threadId, GroupThread thread)
        {
            return this._threads.TryRemove(threadId, out thread);
        }

        public bool TryGetThread(int threadId, out GroupThread thread)
        {
            return this._threads.TryGetValue(threadId, out thread);
        }

        public int MessageCount
        {
            get { return this._threads.Values.Sum(x => x.GetPosts().Count()); }
        }

        public ICollection<GroupThread> GetThreads()
        {
            return this._threads.Values;
        }

        public void Dispose()
        {
            if (this._threads.Count > 0)
            {
                foreach (GroupThread thread in this._threads.Values.ToList())
                {
                    thread.Dispose();
                }

                this._threads.Clear();
            }
        }
    }
}
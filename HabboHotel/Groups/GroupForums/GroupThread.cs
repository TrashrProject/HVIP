using System;
using System.Data;
using System.Linq;
using System.Collections.Concurrent;

using Plus.Database.Interfaces;
using System.Collections.Generic;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupThread
    {
        public int Id { get; private set; }
        public int GroupId { get; set; }
        public string Title { get; set; }
        public int CreatorId { get; set; }
        public string CreatorUsername { get; set; }
        public double CreatedAt { get; set; }
        public double UpdatedAt { get; set; }
        public bool Pinned { get; set; }
        public bool Locked { get; set; }
        public bool Deleted { get; set; }
        public int ModeratorId { get; set; }
        public int Views { get; set; }

        private ConcurrentDictionary<int, GroupPost> _posts;

        public GroupThread(int id, int groupId, string title, int creatorId, double createdAt, double updatedAt, bool pinned, bool locked, bool deleted, int moderatorId, int views)
        {
            this.Id = id;
            this.GroupId = groupId;
            this.Title = title;
            this.CreatorId = creatorId;
            this.CreatorUsername = PlusEnvironment.GetUsernameById(this.CreatorId);
            this.CreatedAt = createdAt;
            this.UpdatedAt = updatedAt;
            this.Pinned = pinned;
            this.Locked = locked;
            this.Deleted = deleted;
            this.ModeratorId = moderatorId;
            this.Views = views;

            this._posts = new ConcurrentDictionary<int, GroupPost>();

            DataTable getPosts = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `group_forum_posts` WHERE `thread_id` = @ThreadId ORDER BY `id` ASC");
                dbClient.AddParameter("ThreadId", this.Id);
                getPosts = dbClient.getTable();
            }

            if (getPosts != null)
            {
                int orderId = 0;
                foreach (DataRow Row in getPosts.Rows)
                {
                    if (this._posts.TryAdd(Convert.ToInt32(Row["id"]), new GroupPost(Convert.ToInt32(Row["id"]), this.Id, Convert.ToString(Row["content"]), Convert.ToInt32(Row["user_id"]), Convert.ToDouble(Row["created_at"]), Row["deleted"].ToString() == "10", Convert.ToInt32(Row["moderator_id"]), orderId)))
                    {
                        orderId++;
                    }
                }
            }
        }

        public int LastReplierId
        {
            get
            {
                return (this._posts.Count > 0 ? this._posts.OrderByDescending(x => x.Value.CreatedAt).FirstOrDefault().Value.CreatorId : this.CreatorId);
            }
        }

        public double LastReplierDate
        {
            get
            {
                return (this._posts.Count > 0 ? this._posts.OrderByDescending(x => x.Value.CreatedAt).FirstOrDefault().Value.CreatedAt : this.CreatedAt);
            }
        }

        public bool TryGetPost(int postId, out GroupPost post)
        {
            return this._posts.TryGetValue(postId, out post);
        }

        public bool TryAddPost(int postId, GroupPost post)
        {
            return this._posts.TryAdd(postId, post);
        }

        public bool TryRemovePost(int postId, GroupPost post)
        {
            return this._posts.TryRemove(postId, out post);
        }

        public int MessageCount
        {
            get { return this._posts.Count; }
        }

        public ICollection<GroupPost> GetPosts()
        {
            return this._posts.Values;
        }

        public void Dispose()
        {
            if (this._posts.Count > 0)
            {
                this._posts.Clear();
            }
        }
    }
}
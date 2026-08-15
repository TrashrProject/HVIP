using System;
using System.Linq;

using Plus.Utilities;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Groups.Forums;

namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class PostMessageEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int threadId = packet.PopInt();
            string title = packet.PopString();
            string message = packet.PopString();

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
                return;

            if (!group.ForumEnabled)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.group.not_found"));
                return;
            }

            if ((DateTime.Now - session.GetHabbo().LastForumMessageUpdateTime).TotalSeconds <= 30.0)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.message.cooldown"));
                return;
            }

            if (string.IsNullOrEmpty(title) && (threadId == 0))
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.title.empty"));
                return;
            }

            if (title.Length < 10 && (threadId == 0))
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.title.too_short"));
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.content_empty"));
                return;
            }

            if (message.Length < 10)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.message.too_short"));
                return;
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(title))
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.message.title.contains_flagged_words"));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                title = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(title);

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(message))
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.message.content.contains_flagged_words"));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(message);

            if (title.Length > 60)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.title.length.too_long"));
                return;
            }

            if (message.Length > 4000)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.content.length.too_long"));
                return;
            }

            if (threadId == 0)
            {
                if (!session.GetHabbo().GetPermissions().HasRight("group_forum_membership_override"))
                {
                    if (group.GetForum().ThreadCreationSetting == 1 && !group.IsMember(session.GetHabbo().Id) && group.CreatorId != session.GetHabbo().Id)
                    {
                        session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.create_thread.member_admin_creator_only"));
                        return;
                    }
                    else if (group.GetForum().ThreadCreationSetting == 2 && !group.IsAdmin(session.GetHabbo().Id) && group.CreatorId != session.GetHabbo().Id)
                    {
                        session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.create_thread.admin_creator_only"));
                        return;
                    }
                    else if (group.GetForum().ThreadCreationSetting == 3 && group.CreatorId != session.GetHabbo().Id)
                    {
                        session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.create_thread.creator_only"));
                        return;
                    }
                }

                int postId = 0;
                int newThreadId = 0;

                double createdAt = UnixTimestamp.GetNow();

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `group_forum_threads` (`title`,`user_id`,`created_at`,`group_id`) VALUES (@title, @userId, @createdAt, @groupId)");
                    dbClient.AddParameter("title", title);
                    dbClient.AddParameter("userId", session.GetHabbo().Id);
                    dbClient.AddParameter("createdAt", createdAt);
                    dbClient.AddParameter("groupId", groupId);
                    newThreadId = Convert.ToInt32(dbClient.InsertQuery());
                }

                GroupThread newThread = new GroupThread(newThreadId, group.Id, title, session.GetHabbo().Id, createdAt, createdAt, false, false, false, 0, 0);
                if (group.GetForum().TryAddThread(newThreadId, newThread))
                {
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("INSERT INTO `group_forum_posts` (`thread_id`,`content`,`user_id`,`created_at`) VALUES (@threadId, @message, @userId, @createdAt)");
                        dbClient.AddParameter("threadId", newThreadId);
                        dbClient.AddParameter("message", message);
                        dbClient.AddParameter("userId", session.GetHabbo().Id);
                        dbClient.AddParameter("createdAt", createdAt);
                        postId = Convert.ToInt32(dbClient.InsertQuery());
                    }

                    if (newThread.TryAddPost(postId, new GroupPost(postId, newThreadId, message, session.GetHabbo().Id, createdAt, false, 0, 1)))
                    {
                        group.GetForum().AddScore(Convert.ToInt32(PlusEnvironment.GetDBConfig().DBData["forum.post_thread.score"]));
                        session.GetHabbo().GetStats().ForumPosts += 1;
                        session.GetHabbo().LastForumMessageUpdateTime = DateTime.Now;
                        session.SendMessage(new ThreadCreatedComposer(group, newThread));
                    }
                }
                else
                {
                    session.SendNotification("Oops, there was an error whilst adding your post to this thread.");

                    if (group.GetForum().TryRemoveThread(newThreadId, newThread))
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("DELETE FROM `group_forum_threads` WHERE `id` = '" + newThreadId + "'");
                        }
                    }
                    return;
                }
            }
            else
            {
                GroupThread thread = null;
                if (!group.GetForum().TryGetThread(threadId, out thread))
                    return;

                if (!session.GetHabbo().GetPermissions().HasRight("group_forum_membership_override"))
                {
                    if (group.GetForum().PostCreationSetting == 1 && !group.IsMember(session.GetHabbo().Id) && group.CreatorId != session.GetHabbo().Id)
                    {
                        session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.group.not_found"));
                        session.SendNotification("Oops, only Group Members, Group Administrators and the Group Creator can post messages on the forums.");
                        return;
                    }
                    else if (group.GetForum().PostCreationSetting == 2 && !group.IsAdmin(session.GetHabbo().Id) && group.CreatorId != session.GetHabbo().Id)
                    {
                        session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.create_post.admin_creator_only"));
                        return;
                    }
                    else if (group.GetForum().PostCreationSetting == 3 && group.CreatorId != session.GetHabbo().Id)
                    {
                        session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.create_post.creator_only"));
                        return;
                    }
                }

                if (message.Length < 5)
                {
                    session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.post.reply.message.too_short"));
                    return;
                }

                int PostId = 0;
                double Created = UnixTimestamp.GetNow();
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `group_forum_posts` (`content`,`user_id`,`created_at`,`thread_id`) VALUES (@message, @userId, @createdAt, @threadId)");
                    dbClient.AddParameter("message", message);
                    dbClient.AddParameter("userId", session.GetHabbo().Id);
                    dbClient.AddParameter("createdAt", Created);
                    dbClient.AddParameter("threadId", thread.Id);
                    PostId = Convert.ToInt32(dbClient.InsertQuery());
                }

                GroupPost post = new GroupPost(PostId, threadId, message, session.GetHabbo().Id, Created, false, 0, thread.GetPosts().Count());
                if (!thread.TryAddPost(PostId, post))
                {
                    session.SendNotification("Oops, there was an error whilst adding your post to this thread.");

                    if (thread.TryRemovePost(PostId, post))
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("DELETE FROM `group_forum_posts` WHERE `thread_id` = '" + thread.Id + "'");
                        }
                    }
                    return;
                }

                thread.UpdatedAt = UnixTimestamp.GetNow();
                group.GetForum().UpdateRequired = true;

                group.GetForum().AddScore(Convert.ToInt32(PlusEnvironment.GetDBConfig().DBData["forum.post_message.score"]));
                session.GetHabbo().GetStats().ForumPosts += 1;
                session.GetHabbo().LastForumMessageUpdateTime = DateTime.Now;
                session.SendMessage(new ThreadReplyComposer(group, thread, post));
            }
        }
    }
}
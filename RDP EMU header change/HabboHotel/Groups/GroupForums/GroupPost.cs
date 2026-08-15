namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupPost
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public string Content { get; set; }
        public int CreatorId { get; set; }
        public string CreatorUsername { get; set; }
        public double CreatedAt { get; set; }
        public bool Deleted { get; set; }
        public int ModeratorId { get; set; }
        public int OrderId { get; private set; }

        public GroupPost(int id, int threadId, string content, int creatorId, double createdAt, bool deleted, int moderatorId, int orderId)
        {
            this.Id = id;
            this.ThreadId = threadId;
            this.Content = content;
            this.CreatorId = creatorId;
            this.CreatorUsername = PlusEnvironment.GetUsernameById(this.CreatorId);
            this.CreatedAt = createdAt;
            this.Deleted = deleted;
            this.ModeratorId = moderatorId;
            this.OrderId = orderId;
        }
    }
}
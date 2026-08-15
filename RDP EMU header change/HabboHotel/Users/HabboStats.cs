using Plus.Database.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Users
{
    public class HabboStats
    {
        public int WelcomeLevel { get; set; }
        public int habboId { get; set; }
        public int RoomVisits { get; set; }
        public double OnlineTime { get; set; }
        public int Respect { get; set; }
        public int RespectGiven { get; set; }
        public int GiftsGiven { get; set; }
        public int GiftsReceived { get; set; }
        public int DailyRespectPoints { get; set; }
        public int DailyPetRespectPoints { get; set; }
        public int AchievementPoints { get; set; }
        public int QuestID { get; set; }
        public int QuestProgress { get; set; }
        public int FavouriteGroupId { get; set; }
        public string RespectsTimestamp { get; set; }
        public int ForumPosts { get; set; }
        public List<int> openedGifts { get; private set; }

        public void addOpenedGift(int eventDate)
        {
            if (this.openedGifts.Contains(eventDate))
            {
                return;
            }

            this.openedGifts.Add(eventDate);
            string[] giftData = this.openedGifts.Select(giftDay => giftDay.ToString()).ToArray();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_stats` SET `calendar_gifts` = @giftData WHERE `id` = @habboId LIMIT 1");
                dbClient.AddParameter("giftData", string.Join(",", giftData));
                dbClient.AddParameter("habboId", habboId);
                dbClient.RunQuery();
            }

        }

        public HabboStats(int roomVisits, double onlineTime, int Respect, int respectGiven, int giftsGiven, int giftsReceived, int dailyRespectPoints, int dailyPetRespectPoints, int achievementPoints, int questID, int questProgress, int groupID, string RespectsTimestamp, int ForumPosts,int habboId, string openedGifts)
        {
            this.habboId = habboId;
            this.RoomVisits = roomVisits;
            this.OnlineTime = onlineTime;
            this.Respect = Respect;
            this.RespectGiven = respectGiven;
            this.GiftsGiven = giftsGiven;
            this.GiftsReceived = giftsReceived;
            this.DailyRespectPoints = dailyRespectPoints;
            this.DailyPetRespectPoints = dailyPetRespectPoints;
            this.AchievementPoints = achievementPoints;
            this.QuestID = questID;
            this.QuestProgress = questProgress;
            this.FavouriteGroupId = groupID;
            this.RespectsTimestamp = RespectsTimestamp;
            this.ForumPosts = ForumPosts;
            this.WelcomeLevel = 0;
            this.openedGifts = new List<int>();
            foreach (string subStr in openedGifts.Split(','))
            {
                int openedDay = 0;
                if (int.TryParse(subStr, out openedDay))
                {
                    this.openedGifts.Add(openedDay);
                }
            }
        }
    }
}

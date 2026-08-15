
#region
using System;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
#endregion
namespace Plus.HabboHotel.Users.Helpers
{
    /// <summary>
    /// Class GuideManager.
    /// </summary>
    public class GuideManager
    {
        /// <summary>
        /// The en cours
        /// </summary>
        public Dictionary<uint, GameClient> EnCours = new Dictionary<uint, GameClient>();
        //internal int HelpersCount = 0;
        //internal int GuardiansCount = 0;
        /// <summary>
        /// The guides on duty
        /// </summary>
        public List<GameClient> GuidesOnDuty = new List<GameClient>();
        public List<GameClient> HelpersOnDuty = new List<GameClient>();
        public List<GameClient> GuardiansOnDuty = new List<GameClient>();
        /// <summary>
        /// Gets or sets the guides count.
        /// </summary>
        /// <value>The guides count.</value>
        public int GuidesCount
        {
            get
            {
                return GuidesOnDuty.Count;
            }
            set
            {
            }
        }
        public int HelpersCount
        {
            get
            {
                return HelpersOnDuty.Count;
            }
        }
        public int GuardiansCount
        {
            get
            {
                return GuardiansOnDuty.Count;
            }
            set
            {
            }
        }
        /// <summary>
        /// Gets the random guide.
        /// </summary>
        /// <returns>GameClient.</returns>
        public GameClient GetRandomGuide()
        {
            var random = new Random();
            return GuidesOnDuty[random.Next(0, GuidesCount - 1)];
        }
        /// <summary>
        /// Adds the guide.
        /// </summary>
        /// <param name="guide">The guide.</param>
        public void AddGuide(GameClient guide)
        {
            switch (guide.GetHabbo().DutyLevel)
            {
                case 1:
                    if (!GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Add(guide);
                    break;
                case 2:
                    if (!HelpersOnDuty.Contains(guide))
                        HelpersOnDuty.Add(guide);
                    break;
                case 3:
                    if (!GuardiansOnDuty.Contains(guide))
                        GuardiansOnDuty.Add(guide);
                    break;
                default:
                    if (!GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Add(guide);
                    break;
            }
        }
        /// <summary>
        /// Removes the guide.
        /// </summary>
        /// <param name="guide">The guide.</param>
        public void RemoveGuide(GameClient guide)
        {
            switch (guide.GetHabbo().DutyLevel)
            {
                case 1:
                    if (GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Remove(guide);
                    break;
                case 2:
                    if (HelpersOnDuty.Contains(guide))
                        HelpersOnDuty.Remove(guide);
                    break;
                case 3:
                    if (GuardiansOnDuty.Contains(guide))
                        GuardiansOnDuty.Remove(guide);
                    break;
                default:
                    if (GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Remove(guide);
                    break;
            }
        }
    }
}
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// GetUserComponent class.
    /// </summary>
    public class GetUserComponent
    {

        /// <summary>
        /// Returns the user statistics.
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static string ReturnUserStatistics(GameClient User)
        {
            if (User == null)
                return null;

            int UserID = User.GetHabbo().Id;
            string Figure = User.GetHabbo().Look;
            int CurHealth = User.GetPlay().CurHealth;
            int MaxHealth = User.GetPlay().MaxHealth;
            int Armor = User.GetPlay().Armor;
            int Hunger = User.GetPlay().Hunger;
            int Level = User.GetPlay().Level;
            int CurXP = User.GetPlay().CurXP;
            int NeedXP = User.GetPlay().NeedXP;
            int Money = User.GetHabbo().Credits;
            string Weapon = "fist";
            //if (User.GetPlay().EquippedWeapon != null)
            //  Weapon = User.GetPlay().EquippedWeapon.Name;
            string Username = User.GetHabbo().Username;
            string GangName = "";
            string GangId = "";
            int Platinos = User.GetHabbo().Diamonds;
            string GangBadge = "";
            bool IsNewsReporter = User.GetHabbo().Rank >= 5 || User.GetPlay().IsNewsReporter;

            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(User.GetHabbo().Id);
            if (Groups != null && Groups.Count > 0)
            {
                GangName = Groups[0].Name;
                GangId = Groups[0].Id.ToString();
                GangBadge = Groups[0].GetBadge();
            }

            string Statistics = 
                UserID + "," +
                Figure + "," +
                CurHealth + "," +
                MaxHealth + "," +
                Armor + "," +
                Hunger + "," +
                Level + "," +
                CurXP + "," +
                NeedXP + "," +
                Money + "," +
                Weapon + "," +
                Username + "," +
                GangName + "," +
                GangId + "," +
                Platinos + "," + 
                GangBadge + "," +
                IsNewsReporter + ","
            ;

            return Statistics;
        }

        /// <summary>
        /// Returns the user statistics via database.
        /// </summary>
        /// <param name="dRow"></param>
        /// <param name="dRowRP"></param>
        /// <returns></returns>
        public static string ReturnUserStatistics(DataRow dRow, DataRow dRowRP)
        {
            int UserID = Convert.ToInt32(dRowRP["id"]);
            string Figure = Convert.ToString(dRow["look"]);
            int CurHealth = Convert.ToInt32(dRowRP["curhealth"]);
            int MaxHealth = Convert.ToInt32(dRowRP["maxhealth"]);
            int Armor = Convert.ToInt32(dRowRP["armor"]);
            int Hunger = Convert.ToInt32(dRowRP["hunger"]);
            int Level = Convert.ToInt32(dRowRP["level"]);
            int CurXP = Convert.ToInt32(dRowRP["curxp"]);
            int NeedXP = Convert.ToInt32(dRowRP["needxp"]);
            int Money = Convert.ToInt32(dRow["credits"]);
            string Weapon = "fist";
            string Username = Convert.ToString(dRow["username"]);
            string GangName = "";
            string GangId = "";
            int Platinos = Convert.ToInt32(dRow["vip_points"]);
            string GangBadge = "";
            bool IsNewsReporter = Convert.ToInt32(dRow["rank"]) >= 5 || PlusEnvironment.EnumToBool(dRowRP["is_news_reporter"].ToString());

            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(UserID);
            if (Groups != null && Groups.Count > 0)
            {
                GangName = Groups[0].Name;
                GangId = Groups[0].Id.ToString();
                GangBadge = Groups[0].GetBadge();
            }

            string Statistics =
                UserID + "," +
                Figure + "," +
                CurHealth + "," +
                MaxHealth + "," +
                Armor + "," +
                Hunger + "," +
                Level + "," +
                CurXP + "," +
                NeedXP + "," +
                Money + "," +
                Weapon + "," +
                Username + "," +
                GangName + "," +
                GangId + "," +
                Platinos + ","  + 
                GangBadge + "," +
                IsNewsReporter + ","
            ;

            return Statistics;
        }

        /// <summary>
        /// Return user statistics via cache.
        /// </summary>
        /// <param name="CachedUser"></param>
        /// <returns></returns>
        public static string ReturnUserStatistics(UserCache CachedUser)
        {
            string[] SocketParts = CachedUser.SocketStatistics.Split(',');

            int UserID = Convert.ToInt32(SocketParts[0]);
           
            string Figure = Convert.ToString(SocketParts[1]);
            int CurHealth = Convert.ToInt32(SocketParts[2]);
            int MaxHealth = Convert.ToInt32(SocketParts[3]);
            int Armor = Convert.ToInt32(SocketParts[4]);
            int Hunger = Convert.ToInt32(SocketParts[5]);
            int Level = Convert.ToInt32(SocketParts[6]);
            int CurXP = Convert.ToInt32(SocketParts[7]);
            int NeedXP = Convert.ToInt32(SocketParts[8]);
            int Money = Convert.ToInt32(SocketParts[9]);
            string Weapon = Convert.ToString(SocketParts[10]);
            string Username = Convert.ToString(SocketParts[11]);
            string GangName = Convert.ToString(SocketParts[12]);
            string GangId = Convert.ToString(SocketParts[13]);
            int Platinos = Convert.ToInt32(SocketParts[14]);
            string GangBadge = Convert.ToString(SocketParts[15]);
            string IsNewsReporter = Convert.ToString(SocketParts[16]);

            string Statistics =
                UserID + "," +
                Figure + "," +
                CurHealth + "," +
                MaxHealth + "," +
                Armor + "," +
                Hunger + "," +
                Level + "," +
                CurXP + "," +
                NeedXP + "," +
                Money + "," +
                Weapon + "," +
                Username + "," +
                GangName + "," +
                GangId + "," +
                Platinos + "," +
                GangBadge + "," +
                IsNewsReporter + ","
            ;

            return Statistics;
        }

        /// <summary>
        /// Clears the statistic dialogue.
        /// </summary>
        /// <param name="User"></param>
        public static void ClearStatisticsDialogue(GameClient User)
        {
            if (User.GetPlay().WebSocketConnection != null)
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, "compose_clear_characterbar|true");
        }
    }
}

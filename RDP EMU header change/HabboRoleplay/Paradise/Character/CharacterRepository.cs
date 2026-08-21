using System;
using System.Data;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.Character
{
    public sealed class CharacterRepository
    {
        public ParadiseCharacter LoadByUserId(int userId)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("SELECT `id`,`user_id`,`citizen_id`,`first_name`,`last_name`,`birth_date`,`gender`,`nationality`,`biography`,`reputation`,`created_at`,`updated_at` FROM `rp_characters` WHERE `user_id` = @user_id LIMIT 1");
                db.AddParameter("user_id", userId);
                DataRow row = db.getRow();
                return row == null ? null : Map(row);
            }
        }

        public ParadiseCharacter Create(int userId, string citizenId, string firstName, string lastName,
            DateTime birthDate, string gender, string nationality, string biography)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("INSERT INTO `rp_characters` (`user_id`,`citizen_id`,`first_name`,`last_name`,`birth_date`,`gender`,`nationality`,`biography`,`reputation`) VALUES (@user_id,@citizen_id,@first_name,@last_name,@birth_date,@gender,@nationality,@biography,0)");
                db.AddParameter("user_id", userId);
                db.AddParameter("citizen_id", citizenId);
                db.AddParameter("first_name", firstName);
                db.AddParameter("last_name", lastName);
                db.AddParameter("birth_date", birthDate.ToString("yyyy-MM-dd"));
                db.AddParameter("gender", String.IsNullOrWhiteSpace(gender) ? (object)DBNull.Value : gender);
                db.AddParameter("nationality", nationality);
                db.AddParameter("biography", String.IsNullOrWhiteSpace(biography) ? (object)DBNull.Value : biography);
                db.InsertQuery();
            }

            return LoadByUserId(userId);
        }

        public void UpdateBiography(int userId, string biography)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("UPDATE `rp_characters` SET `biography` = @biography WHERE `user_id` = @user_id LIMIT 1");
                db.AddParameter("biography", String.IsNullOrWhiteSpace(biography) ? (object)DBNull.Value : biography);
                db.AddParameter("user_id", userId);
                db.RunQuery();
            }
        }

        private static ParadiseCharacter Map(DataRow row)
        {
            return new ParadiseCharacter(
                Convert.ToInt32(row["id"]),
                Convert.ToInt32(row["user_id"]),
                Convert.ToString(row["citizen_id"]),
                Convert.ToString(row["first_name"]),
                Convert.ToString(row["last_name"]),
                Convert.ToDateTime(row["birth_date"]),
                row["gender"] == DBNull.Value ? null : Convert.ToString(row["gender"]),
                Convert.ToString(row["nationality"]),
                row["biography"] == DBNull.Value ? String.Empty : Convert.ToString(row["biography"]),
                Convert.ToInt32(row["reputation"]),
                Convert.ToDateTime(row["created_at"]),
                Convert.ToDateTime(row["updated_at"]));
        }
    }
}

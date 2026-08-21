using System;
using System.Data;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.Character
{
    /// <summary>
    /// Data access for Character V2 identity-only data.
    /// Existing gameplay data remains in users/play_stats and is not duplicated here.
    /// </summary>
    public sealed class CharacterRepository
    {
        public CharacterIdentity GetOrCreate(uint userId)
        {
            CharacterIdentity fallback = CreateFallback(userId);

            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (db == null)
                        return fallback;

                    db.SetQuery("SELECT `user_id`,`citizen_number`,`first_name`,`last_name`,`birth_date`,`gender`,`nationality` FROM `rp_characters` WHERE `user_id` = @userId LIMIT 1");
                    db.AddParameter("userId", userId);
                    DataRow row = db.getRow();

                    if (row == null)
                    {
                        db.SetQuery("INSERT IGNORE INTO `rp_characters` (`user_id`,`citizen_number`) VALUES (@userId,@citizenNumber)");
                        db.AddParameter("userId", userId);
                        db.AddParameter("citizenNumber", fallback.CitizenNumber);
                        db.RunQuery();

                        db.SetQuery("SELECT `user_id`,`citizen_number`,`first_name`,`last_name`,`birth_date`,`gender`,`nationality` FROM `rp_characters` WHERE `user_id` = @userId LIMIT 1");
                        db.AddParameter("userId", userId);
                        row = db.getRow();
                    }

                    return row == null ? fallback : Map(row, fallback);
                }
            }
            catch
            {
                // Character V2 is additive. A missing/unapplied migration must never break
                // the live Habbo session; the bridge can still expose legacy-safe data.
                return fallback;
            }
        }

        private static CharacterIdentity CreateFallback(uint userId)
        {
            return new CharacterIdentity
            {
                UserId = userId,
                CitizenNumber = "PR-" + userId.ToString("D5")
            };
        }

        private static CharacterIdentity Map(DataRow row, CharacterIdentity fallback)
        {
            CharacterIdentity identity = new CharacterIdentity
            {
                UserId = ToUInt(row["user_id"], fallback.UserId),
                CitizenNumber = ToStringOrNull(row["citizen_number"]) ?? fallback.CitizenNumber,
                FirstName = ToStringOrNull(row["first_name"]),
                LastName = ToStringOrNull(row["last_name"]),
                Gender = ToStringOrNull(row["gender"]),
                Nationality = ToStringOrNull(row["nationality"])
            };

            object birthDate = row["birth_date"];
            if (birthDate != null && birthDate != DBNull.Value)
            {
                DateTime parsed;
                if (DateTime.TryParse(Convert.ToString(birthDate), out parsed))
                    identity.BirthDate = parsed.Date;
            }

            return identity;
        }

        private static uint ToUInt(object value, uint fallback)
        {
            uint result;
            return UInt32.TryParse(Convert.ToString(value), out result) ? result : fallback;
        }

        private static string ToStringOrNull(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            string result = Convert.ToString(value);
            return String.IsNullOrWhiteSpace(result) ? null : result.Trim();
        }
    }
}

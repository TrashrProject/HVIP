using Newtonsoft.Json;

namespace Plus.HabboRoleplay.Paradise.Character
{
    public sealed class CharacterSnapshot
    {
        [JsonProperty("player")]
        public CharacterPlayerSnapshot Player { get; set; }

        [JsonProperty("identity")]
        public CharacterIdentitySnapshot Identity { get; set; }

        [JsonProperty("vitals")]
        public CharacterVitalsSnapshot Vitals { get; set; }

        [JsonProperty("progression")]
        public CharacterProgressionSnapshot Progression { get; set; }

        [JsonProperty("employment")]
        public CharacterEmploymentSnapshot Employment { get; set; }

        [JsonProperty("economy")]
        public CharacterEconomySnapshot Economy { get; set; }

        [JsonProperty("room")]
        public CharacterRoomSnapshot Room { get; set; }
    }

    public sealed class CharacterPlayerSnapshot
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("look")]
        public string Look { get; set; }

        [JsonProperty("motto")]
        public string Motto { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }

    public sealed class CharacterIdentitySnapshot
    {
        [JsonProperty("citizenNumber")]
        public string CitizenNumber { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("birthDate")]
        public string BirthDate { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; }

        [JsonProperty("complete")]
        public bool Complete { get; set; }
    }

    public sealed class CharacterVitalsSnapshot
    {
        [JsonProperty("health")]
        public CharacterValueSnapshot Health { get; set; }

        [JsonProperty("armor")]
        public int Armor { get; set; }

        [JsonProperty("hunger")]
        public int Hunger { get; set; }

        [JsonProperty("hygiene")]
        public int Hygiene { get; set; }
    }

    public sealed class CharacterValueSnapshot
    {
        [JsonProperty("current")]
        public int Current { get; set; }

        [JsonProperty("max")]
        public int Max { get; set; }
    }

    public sealed class CharacterProgressionSnapshot
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("xp")]
        public int XP { get; set; }

        [JsonProperty("nextXp")]
        public int NextXP { get; set; }
    }

    public sealed class CharacterEmploymentSnapshot
    {
        [JsonProperty("jobId")]
        public int JobId { get; set; }

        [JsonProperty("jobName")]
        public string JobName { get; set; }

        [JsonProperty("jobRank")]
        public int JobRank { get; set; }

        [JsonProperty("onDuty")]
        public bool OnDuty { get; set; }
    }

    public sealed class CharacterEconomySnapshot
    {
        [JsonProperty("cash")]
        public int Cash { get; set; }

        [JsonProperty("bank")]
        public int Bank { get; set; }
    }

    public sealed class CharacterRoomSnapshot
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }
    }
}

using System;

namespace Plus.HabboRoleplay.Paradise.Character
{
    /// <summary>
    /// RP identity extension for a Habbo account.
    /// Gameplay state such as health, money and job does not belong here.
    /// </summary>
    public sealed class CharacterIdentity
    {
        public uint UserId { get; set; }
        public string CitizenNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }

        public bool IsComplete
        {
            get
            {
                return !String.IsNullOrWhiteSpace(FirstName)
                    && !String.IsNullOrWhiteSpace(LastName)
                    && BirthDate.HasValue;
            }
        }
    }
}

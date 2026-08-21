using System;

namespace Plus.HabboRoleplay.Paradise.Character
{
    public sealed class ParadiseCharacter
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string CitizenId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public DateTime BirthDate { get; private set; }
        public string Gender { get; private set; }
        public string Nationality { get; private set; }
        public string Biography { get; private set; }
        public int Reputation { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public string FullName
        {
            get { return (FirstName + " " + LastName).Trim(); }
        }

        public int Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return Math.Max(0, age);
            }
        }

        public ParadiseCharacter(int id, int userId, string citizenId, string firstName, string lastName,
            DateTime birthDate, string gender, string nationality, string biography, int reputation,
            DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            UserId = userId;
            CitizenId = citizenId ?? String.Empty;
            FirstName = firstName ?? String.Empty;
            LastName = lastName ?? String.Empty;
            BirthDate = birthDate;
            Gender = gender;
            Nationality = nationality ?? String.Empty;
            Biography = biography ?? String.Empty;
            Reputation = reputation;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public void SetBiography(string biography)
        {
            Biography = biography ?? String.Empty;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

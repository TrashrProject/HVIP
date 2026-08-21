using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Paradise.Character
{
    /// <summary>
    /// Builds the authoritative Character V2 view from live emulator state plus
    /// the identity extension stored in rp_characters.
    /// </summary>
    public sealed class CharacterService
    {
        private readonly CharacterRepository _repository;

        public CharacterService()
        {
            _repository = new CharacterRepository();
        }

        public CharacterSnapshot GetSnapshot(GameClient client)
        {
            if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                return null;

            int userId = client.GetHabbo().Id;
            CharacterIdentity identity = _repository.GetOrCreate((uint)Math.Max(0, userId));

            Group job = null;
            int jobId = client.GetPlay().JobId;
            if (jobId > 0 && PlusEnvironment.GetGame() != null && PlusEnvironment.GetGame().GetGroupManager() != null)
                job = PlusEnvironment.GetGame().GetGroupManager().GetJob(jobId);

            Room room = client.GetHabbo().CurrentRoom;

            return new CharacterSnapshot
            {
                Player = new CharacterPlayerSnapshot
                {
                    UserId = userId,
                    Username = client.GetHabbo().Username,
                    Look = client.GetHabbo().Look,
                    Motto = client.GetHabbo().Motto,
                    Rank = client.GetHabbo().Rank,
                    Role = RoleLabel(client.GetHabbo().Rank)
                },
                Identity = new CharacterIdentitySnapshot
                {
                    CitizenNumber = identity.CitizenNumber,
                    FirstName = identity.FirstName,
                    LastName = identity.LastName,
                    BirthDate = identity.BirthDate.HasValue ? identity.BirthDate.Value.ToString("yyyy-MM-dd") : null,
                    Gender = identity.Gender,
                    Nationality = identity.Nationality,
                    Complete = identity.IsComplete
                },
                Vitals = new CharacterVitalsSnapshot
                {
                    Health = new CharacterValueSnapshot
                    {
                        Current = client.GetPlay().CurHealth,
                        Max = client.GetPlay().MaxHealth
                    },
                    Armor = client.GetPlay().Armor,
                    Hunger = client.GetPlay().Hunger,
                    Hygiene = client.GetPlay().Hygiene
                },
                Progression = new CharacterProgressionSnapshot
                {
                    Level = client.GetPlay().Level,
                    XP = client.GetPlay().CurXP,
                    NextXP = client.GetPlay().NeedXP
                },
                Employment = new CharacterEmploymentSnapshot
                {
                    JobId = jobId,
                    JobName = job != null ? job.Name : null,
                    JobRank = client.GetPlay().JobRank,
                    OnDuty = client.GetPlay().IsWorking
                },
                Economy = new CharacterEconomySnapshot
                {
                    Cash = client.GetHabbo().Credits,
                    Bank = client.GetPlay().Bank
                },
                Room = new CharacterRoomSnapshot
                {
                    Id = room != null ? (int?)room.Id : null,
                    Name = room != null ? room.Name : null,
                    City = room != null ? room.City : null
                }
            };
        }

        private static string RoleLabel(int rank)
        {
            if (rank >= 8) return "Fondateur";
            if (rank >= 6) return "Staff";
            if (rank >= 3) return "Équipe";
            return "Citoyen";
        }
    }
}

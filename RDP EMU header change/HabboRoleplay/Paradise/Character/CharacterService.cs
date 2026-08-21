using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Plus.HabboRoleplay.Paradise.Documents;

namespace Plus.HabboRoleplay.Paradise.Character
{
    public static class CharacterService
    {
        private static readonly CharacterRepository Repository = new CharacterRepository();
        private static readonly ConcurrentDictionary<int, ParadiseCharacter> Cache = new ConcurrentDictionary<int, ParadiseCharacter>();
        private static readonly Regex NamePattern = new Regex("^[A-Za-zÀ-ÖØ-öø-ÿ' -]{2,32}$", RegexOptions.Compiled);

        public static ParadiseCharacter GetOrLoad(int userId, bool forceReload = false)
        {
            ParadiseCharacter cached;
            if (!forceReload && Cache.TryGetValue(userId, out cached)) return cached;

            ParadiseCharacter loaded = Repository.LoadByUserId(userId);
            if (loaded != null) Cache[userId] = loaded;
            else Cache.TryRemove(userId, out cached);
            return loaded;
        }

        public static ParadiseCharacter CreateIdentity(int userId, string firstName, string lastName,
            DateTime birthDate, string gender, string nationality, string biography, out string error)
        {
            error = null;
            if (userId <= 0)
            {
                error = "Utilisateur invalide.";
                return null;
            }

            if (GetOrLoad(userId) != null)
            {
                error = "Votre identité RP existe déjà.";
                return null;
            }

            firstName = Clean(firstName);
            lastName = Clean(lastName);
            gender = Clean(gender);
            nationality = Clean(nationality);
            biography = CleanBiography(biography);

            if (!NamePattern.IsMatch(firstName))
            {
                error = "Prénom invalide (2 à 32 caractères).";
                return null;
            }
            if (!NamePattern.IsMatch(lastName))
            {
                error = "Nom invalide (2 à 32 caractères).";
                return null;
            }
            if (nationality.Length < 2 || nationality.Length > 48)
            {
                error = "Nationalité / origine invalide.";
                return null;
            }
            if (gender.Length > 24)
            {
                error = "Genre RP trop long.";
                return null;
            }
            if (biography.Length > 400)
            {
                error = "La biographie est limitée à 400 caractères.";
                return null;
            }

            DateTime today = DateTime.Today;
            if (birthDate.Date > today.AddYears(-16) || birthDate.Date < today.AddYears(-100))
            {
                error = "Date de naissance incohérente pour un personnage RP.";
                return null;
            }

            ParadiseCharacter character = null;
            for (int attempt = 0; attempt < 3 && character == null; attempt++)
            {
                string citizenId = GenerateCitizenId();
                try
                {
                    character = Repository.Create(userId, citizenId, firstName, lastName, birthDate,
                        gender, nationality, biography);
                }
                catch
                {
                    if (attempt == 2) throw;
                }
            }

            if (character == null)
            {
                error = "Impossible de créer l'identité RP.";
                return null;
            }

            Cache[userId] = character;
            DocumentService.EnsureIdentityCard(userId, character);
            return character;
        }

        public static bool UpdateBiography(int userId, string biography, out string error)
        {
            error = null;
            ParadiseCharacter character = GetOrLoad(userId);
            if (character == null)
            {
                error = "Créez d'abord votre identité RP.";
                return false;
            }

            biography = CleanBiography(biography);
            if (biography.Length > 400)
            {
                error = "La biographie est limitée à 400 caractères.";
                return false;
            }

            Repository.UpdateBiography(userId, biography);
            character.SetBiography(biography);
            Cache[userId] = character;
            return true;
        }

        public static void Invalidate(int userId)
        {
            ParadiseCharacter ignored;
            Cache.TryRemove(userId, out ignored);
            DocumentService.Invalidate(userId);
        }

        private static string GenerateCitizenId()
        {
            return "PID-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        private static string Clean(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;
            return Regex.Replace(value.Trim(), "\\s+", " ");
        }

        private static string CleanBiography(string value)
        {
            string cleaned = Clean(value);
            cleaned = Regex.Replace(cleaned, "[<>]", String.Empty);
            return cleaned;
        }
    }
}

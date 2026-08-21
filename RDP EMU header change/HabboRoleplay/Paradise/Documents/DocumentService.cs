using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Paradise.Character;

namespace Plus.HabboRoleplay.Paradise.Documents
{
    public static class DocumentService
    {
        public const string IdentityCode = "PLACID_ID";
        public const string DriverLicenseCode = "DRIVER_LICENSE";

        private static readonly DocumentRepository Repository = new DocumentRepository();
        private static readonly ConcurrentDictionary<int, List<ParadiseDocument>> Cache = new ConcurrentDictionary<int, List<ParadiseDocument>>();

        public static IList<ParadiseDocument> GetForUser(int userId, bool forceReload = false)
        {
            List<ParadiseDocument> cached;
            if (!forceReload && Cache.TryGetValue(userId, out cached)) return cached.AsReadOnly();
            List<ParadiseDocument> loaded = Repository.LoadForUser(userId);
            Cache[userId] = loaded;
            return loaded.AsReadOnly();
        }

        public static ParadiseDocument GetDocument(int userId, string typeCode, bool forceReload = false)
        {
            return GetForUser(userId, forceReload).FirstOrDefault(x =>
                String.Equals(x.TypeCode, typeCode, StringComparison.OrdinalIgnoreCase));
        }

        public static ParadiseDocument EnsureIdentityCard(int userId, ParadiseCharacter character)
        {
            ParadiseDocument current = GetDocument(userId, IdentityCode, true);
            if (current != null) return current;
            if (character == null) return null;

            ParadiseDocument created = null;
            for (int attempt = 0; attempt < 3 && created == null; attempt++)
            {
                try
                {
                    created = Repository.Create(userId, IdentityCode, GenerateDocumentNumber("PI"), null, null);
                }
                catch
                {
                    if (attempt == 2) throw;
                }
            }
            Invalidate(userId);
            return created ?? GetDocument(userId, IdentityCode, true);
        }

        public static bool Present(GameClient sender, string targetUsername, string typeCode, out string message)
        {
            message = null;
            if (sender == null || sender.GetHabbo() == null)
            {
                message = "Session invalide.";
                return false;
            }

            targetUsername = (targetUsername ?? String.Empty).Trim();
            if (targetUsername.Length == 0)
            {
                message = "Indiquez le nom du joueur.";
                return false;
            }

            GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(targetUsername);
            if (target == null || target.GetHabbo() == null || target.LoggingOut)
            {
                message = "Ce joueur n'est pas connecté.";
                return false;
            }
            if (target.GetHabbo().Id == sender.GetHabbo().Id)
            {
                message = "Vous n'avez pas besoin de vous présenter ce document à vous-même.";
                return false;
            }
            if (!sender.GetHabbo().InRoom || !target.GetHabbo().InRoom ||
                sender.GetHabbo().CurrentRoomId != target.GetHabbo().CurrentRoomId)
            {
                message = "Le joueur doit être dans le même appartement.";
                return false;
            }

            ParadiseCharacter character = CharacterService.GetOrLoad(sender.GetHabbo().Id);
            if (character == null)
            {
                message = "Créez d'abord votre identité RP.";
                return false;
            }

            ParadiseDocument document = GetDocument(sender.GetHabbo().Id, typeCode, true);
            if (document == null || !document.IsValid)
            {
                message = String.Equals(typeCode, DriverLicenseCode, StringComparison.OrdinalIgnoreCase)
                    ? "Vous ne possédez pas de permis de conduire valide."
                    : "Ce document n'est pas disponible.";
                return false;
            }

            Repository.CreateShare(sender.GetHabbo().Id, target.GetHabbo().Id, document.Id, DateTime.UtcNow.AddMinutes(2));

            string documentLabel = String.Equals(typeCode, DriverLicenseCode, StringComparison.OrdinalIgnoreCase)
                ? "son permis de conduire"
                : "sa carte d'identité";

            sender.SendWhisper("Vous présentez " + documentLabel + " à " + target.GetHabbo().Username + ".", 1);
            target.SendWhisper(character.FullName + " vous présente " + documentLabel + ".", 1);
            message = "Document présenté.";
            return true;
        }

        public static void Invalidate(int userId)
        {
            List<ParadiseDocument> ignored;
            Cache.TryRemove(userId, out ignored);
        }

        private static string GenerateDocumentNumber(string prefix)
        {
            return prefix + "-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }
    }
}

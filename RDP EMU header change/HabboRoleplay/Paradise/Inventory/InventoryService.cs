using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    public static class InventoryService
    {
        private static readonly InventoryRepository Repository = new InventoryRepository();
        private static readonly ConcurrentDictionary<int, long> LastUseAt = new ConcurrentDictionary<int, long>();
        private static readonly ConcurrentDictionary<int, long> LastGiveAt = new ConcurrentDictionary<int, long>();
        private const long UseGuardMilliseconds = 450;
        private const long GiveGuardMilliseconds = 650;

        public static IList<InventoryItem> GetItems(int userId)
        {
            try { return Repository.LoadForUser(userId).AsReadOnly(); }
            catch { return new List<InventoryItem>().AsReadOnly(); }
        }

        public static InventoryCapacity GetCapacity(int userId)
        {
            try { return Repository.GetCapacity(userId); }
            catch { return new InventoryCapacity { BaseCapacity = 50m, CapacityBonus = 0m, MaxSlots = 30 }; }
        }

        public static decimal GetWeight(int userId)
        {
            return GetItems(userId).Sum(x => x.TotalWeight);
        }

        public static bool TryResolveItem(int userId, string token, out InventoryItem item, out string message)
        {
            item = null;
            message = null;
            token = (token ?? String.Empty).Trim();
            if (token.Length == 0)
            {
                message = "Indiquez un objet.";
                return false;
            }

            IList<InventoryItem> items = GetItems(userId);
            long id;
            if (Int64.TryParse(token, out id))
            {
                item = items.FirstOrDefault(x => x.Id == id);
                if (item == null) message = "Objet introuvable.";
                return item != null;
            }

            string normalized = Normalize(token);
            List<InventoryItem> matches = items.Where(x => x.Definition != null &&
                (Normalize(x.Definition.Code) == normalized || Normalize(x.Definition.Name) == normalized)).ToList();

            if (matches.Count == 0)
            {
                message = "Objet introuvable dans votre inventaire.";
                return false;
            }
            if (matches.Count > 1)
            {
                message = "Plusieurs objets correspondent. Utilisez l’inventaire visuel pour choisir l’instance exacte.";
                return false;
            }

            item = matches[0];
            return true;
        }

        public static bool Use(GameClient session, long itemId, out string message)
        {
            message = null;
            if (session == null || session.GetHabbo() == null)
            {
                message = "Session invalide.";
                return false;
            }

            int userId = session.GetHabbo().Id;
            if (session.GetPlay() != null && (session.GetPlay().IsDead || session.GetPlay().IsDying))
            {
                message = "Vous ne pouvez pas utiliser cet objet dans votre état actuel.";
                return false;
            }
            if (!AcquireGuard(LastUseAt, userId, UseGuardMilliseconds))
            {
                message = "Action trop rapide. Réessayez.";
                return false;
            }

            InventoryItem item = Repository.LoadItem(userId, itemId);
            if (item == null || item.Definition == null)
            {
                message = "Cet objet n’est plus dans votre inventaire.";
                return false;
            }
            if (item.Quantity <= 0)
            {
                message = "Quantité invalide.";
                return false;
            }
            if (!item.Definition.Usable)
            {
                message = "Cet objet ne possède pas d’action Utiliser.";
                return false;
            }

            string effect = item.Definition.EffectType ?? "NONE";
            switch (effect.ToUpperInvariant())
            {
                case "EAT":
                    if (session.GetPlay() == null)
                    {
                        message = "Les besoins du personnage ne sont pas disponibles.";
                        return false;
                    }
                    if (!Repository.ConsumeOne(userId, item.Id))
                    {
                        message = "Impossible d’utiliser cet objet.";
                        return false;
                    }
                    if (item.Definition.EffectValue > 0)
                    {
                        session.GetPlay().Hunger = Math.Min(100, session.GetPlay().Hunger + item.Definition.EffectValue);
                        if (session.GetPlay().UserDataHandler != null) session.GetPlay().UserDataHandler.SaveData();
                    }
                    session.SendWhisper("[INVENTAIRE] " + item.Definition.Name + " utilisé.", 1);
                    InventoryUiEventService.Toast(userId, "Objet utilisé", item.Definition.Name + " utilisé.");
                    return true;

                case "DRINK":
                    // There is no authoritative Thirst stat in the audited RP core.
                    // Water is therefore a real consumable without inventing a fake thirst meter.
                    if (!Repository.ConsumeOne(userId, item.Id))
                    {
                        message = "Impossible d’utiliser cet objet.";
                        return false;
                    }
                    session.SendWhisper("[INVENTAIRE] " + item.Definition.Name + " utilisé.", 1);
                    InventoryUiEventService.Toast(userId, "Objet utilisé", item.Definition.Name + " utilisé.");
                    return true;

                case "PHONE":
                    Repository.LogUseWithoutConsume(userId, item);
                    InventoryUiEventService.OpenPhone(userId);
                    message = "Téléphone ouvert.";
                    return true;

                case "KEY":
                    Repository.LogUseWithoutConsume(userId, item);
                    message = "Clé inspectée. Son accès sera validé par le système concerné lorsqu’il sera branché.";
                    session.SendWhisper("[INVENTAIRE] " + message, 1);
                    return true;

                case "NONE":
                default:
                    message = "Cet objet n’a pas encore d’effet serveur actif.";
                    return false;
            }
        }

        public static bool Use(GameClient session, string token, out string message)
        {
            message = null;
            if (session == null || session.GetHabbo() == null)
            {
                message = "Session invalide.";
                return false;
            }
            InventoryItem item;
            if (!TryResolveItem(session.GetHabbo().Id, token, out item, out message)) return false;
            return Use(session, item.Id, out message);
        }

        public static bool Give(GameClient sender, string targetUsername, long itemId, int quantity, out string message)
        {
            message = null;
            if (sender == null || sender.GetHabbo() == null)
            {
                message = "Session invalide.";
                return false;
            }
            if (quantity <= 0)
            {
                message = "La quantité doit être supérieure à zéro.";
                return false;
            }

            int senderUserId = sender.GetHabbo().Id;
            if (!AcquireGuard(LastGiveAt, senderUserId, GiveGuardMilliseconds))
            {
                message = "Transfert trop rapide. Réessayez.";
                return false;
            }

            targetUsername = (targetUsername ?? String.Empty).Trim();
            GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(targetUsername);
            if (target == null || target.GetHabbo() == null || target.LoggingOut)
            {
                message = "Ce joueur n’est pas connecté.";
                return false;
            }
            if (target.GetHabbo().Id == senderUserId)
            {
                message = "Vous ne pouvez pas vous donner un objet à vous-même.";
                return false;
            }
            if (!sender.GetHabbo().InRoom || !target.GetHabbo().InRoom || sender.GetHabbo().CurrentRoomId != target.GetHabbo().CurrentRoomId)
            {
                message = "Le joueur doit être dans le même appartement.";
                return false;
            }

            InventoryItem source = Repository.LoadItem(senderUserId, itemId);
            if (source == null || source.Definition == null)
            {
                message = "Objet introuvable dans votre inventaire.";
                return false;
            }

            if (!Repository.Transfer(senderUserId, target.GetHabbo().Id, source.Id, quantity, out message)) return false;

            string quantityText = quantity > 1 ? " ×" + quantity.ToString(CultureInfo.InvariantCulture) : String.Empty;
            sender.SendWhisper("[INVENTAIRE] Vous avez donné " + source.Definition.Name + quantityText + " à " + target.GetHabbo().Username + ".", 1);
            target.SendWhisper("[INVENTAIRE] " + sender.GetHabbo().Username + " vous a donné " + source.Definition.Name + quantityText + ".", 1);
            InventoryUiEventService.Toast(target.GetHabbo().Id, "Objet reçu", source.Definition.Name + quantityText);
            InventoryUiEventService.Toast(senderUserId, "Objet donné", source.Definition.Name + quantityText);
            return true;
        }

        public static string WeightText(int userId)
        {
            InventoryCapacity capacity = GetCapacity(userId);
            decimal weight = GetWeight(userId);
            return "Inventaire : " + weight.ToString("0.##", CultureInfo.GetCultureInfo("fr-FR")) + " / " +
                   capacity.MaximumWeight.ToString("0.##", CultureInfo.GetCultureInfo("fr-FR")) + " kg";
        }

        private static bool AcquireGuard(ConcurrentDictionary<int, long> guard, int userId, long milliseconds)
        {
            long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            while (true)
            {
                long previous;
                if (!guard.TryGetValue(userId, out previous))
                {
                    if (guard.TryAdd(userId, now)) return true;
                    continue;
                }
                if (now - previous < milliseconds) return false;
                if (guard.TryUpdate(userId, now, previous)) return true;
            }
        }

        private static string Normalize(string value)
        {
            return (value ?? String.Empty).Trim().Replace(" ", "_").ToUpperInvariant();
        }
    }
}

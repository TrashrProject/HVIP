using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Paradise.Inventory;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    /// <summary>
    /// Existing :inventario / :inv entry point, refactored for ParadiseRP V2.
    /// Keeping this command avoids introducing a second competing inventory.
    /// UI actions use exact server item ids through private-looking subcommands.
    /// </summary>
    class InventoryCommand : IChatCommand
    {
        public string PermissionRequired { get { return "command_inventory"; } }
        public string Parameters { get { return "[use|useid|give|giveid|weight]"; } }
        public string Description { get { return "Ouvre et contrôle l’inventaire physique ParadiseRP."; } }

        public void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;

            if (parameters == null || parameters.Length <= 1)
            {
                InventoryUiEventService.OpenInventory(session.GetHabbo().Id);
                return;
            }

            string action = (parameters[1] ?? String.Empty).Trim().ToLowerInvariant();
            switch (action)
            {
                case "weight":
                case "poids":
                    session.SendWhisper(InventoryService.WeightText(session.GetHabbo().Id), 1);
                    return;

                case "useid":
                    ExecuteUseId(session, parameters);
                    return;

                case "use":
                    ExecuteUse(session, parameters);
                    return;

                case "giveid":
                    ExecuteGiveId(session, parameters);
                    return;

                case "give":
                    ExecuteGive(session, parameters);
                    return;

                default:
                    session.SendWhisper("Inventaire : :inv | :inv use <objet> | :inv weight", 1);
                    return;
            }
        }

        private static void ExecuteUseId(GameClient session, string[] parameters)
        {
            long id;
            if (parameters.Length != 3 || !Int64.TryParse(parameters[2], out id) || id <= 0)
            {
                session.SendWhisper("Objet invalide.", 1);
                return;
            }
            string message;
            if (!InventoryService.Use(session, id, out message) && !String.IsNullOrWhiteSpace(message))
                session.SendWhisper(message, 1);
        }

        private static void ExecuteUse(GameClient session, string[] parameters)
        {
            if (parameters.Length < 3)
            {
                session.SendWhisper("Syntaxe : :use <objet>", 1);
                return;
            }
            string token = String.Join(" ", parameters, 2, parameters.Length - 2).Trim();
            string message;
            if (!InventoryService.Use(session, token, out message) && !String.IsNullOrWhiteSpace(message))
                session.SendWhisper(message, 1);
        }

        private static void ExecuteGiveId(GameClient session, string[] parameters)
        {
            long itemId;
            int quantity = 1;
            if (parameters.Length < 4 || !Int64.TryParse(parameters[3], out itemId) || itemId <= 0)
            {
                session.SendWhisper("Objet invalide.", 1);
                return;
            }
            if (parameters.Length >= 5 && (!Int32.TryParse(parameters[4], out quantity) || quantity <= 0))
            {
                session.SendWhisper("Quantité invalide.", 1);
                return;
            }
            string message;
            if (!InventoryService.Give(session, parameters[2], itemId, quantity, out message) && !String.IsNullOrWhiteSpace(message))
                session.SendWhisper(message, 1);
        }

        private static void ExecuteGive(GameClient session, string[] parameters)
        {
            if (parameters.Length < 4)
            {
                session.SendWhisper("Syntaxe : :giveitem <joueur> <objet> [quantité]", 1);
                return;
            }
            string target = parameters[2];
            int quantity = 1;
            int end = parameters.Length;
            int parsed;
            if (parameters.Length >= 5 && Int32.TryParse(parameters[parameters.Length - 1], out parsed))
            {
                quantity = parsed;
                end--;
            }
            if (quantity <= 0)
            {
                session.SendWhisper("La quantité doit être supérieure à zéro.", 1);
                return;
            }
            string token = String.Join(" ", parameters, 3, end - 3).Trim();
            InventoryItem item;
            string message;
            if (!InventoryService.TryResolveItem(session.GetHabbo().Id, token, out item, out message))
            {
                session.SendWhisper(message, 1);
                return;
            }
            if (!InventoryService.Give(session, target, item.Id, quantity, out message) && !String.IsNullOrWhiteSpace(message))
                session.SendWhisper(message, 1);
        }
    }
}

using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    public static class ParadiseInventoryCommandBootstrap
    {
        public static void Register(CommandManager commands)
        {
            if (commands == null) return;
            commands.Register("use", new ParadiseUseItemCommand());
            commands.Register("useitemid", new ParadiseUseItemByIdCommand());
            commands.Register("giveitem", new ParadiseGiveItemCommand(), "logged");
            commands.Register("giveitemid", new ParadiseGiveItemByIdCommand(), "logged");
            commands.Register("donnerobjet", new ParadiseGiveItemCommand(), "logged");
            commands.Register("weight", new ParadiseWeightCommand());
            commands.Register("poids", new ParadiseWeightCommand());
        }
    }

    internal abstract class ParadiseInventoryCommandBase : IChatCommand
    {
        public string PermissionRequired { get { return String.Empty; } }
        public abstract string Parameters { get; }
        public abstract string Description { get; }
        public abstract void Execute(GameClient session, Room room, string[] parameters);

        protected static string Merge(string[] parameters, int start)
        {
            if (parameters == null || parameters.Length <= start) return String.Empty;
            return CommandManager.MergeParams(parameters, start).Trim();
        }
    }

    internal sealed class ParadiseUseItemCommand : ParadiseInventoryCommandBase
    {
        public override string Parameters { get { return "<objet>"; } }
        public override string Description { get { return "Utilise un objet de votre inventaire ParadiseRP."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                session.SendWhisper("Syntaxe : :use <objet>", 1);
                return;
            }
            string message;
            bool ok = InventoryService.Use(session, Merge(parameters, 1), out message);
            if (!ok && !String.IsNullOrWhiteSpace(message)) session.SendWhisper(message, 1);
        }
    }

    internal sealed class ParadiseUseItemByIdCommand : ParadiseInventoryCommandBase
    {
        public override string Parameters { get { return "<id>"; } }
        public override string Description { get { return "Commande interne de l’interface Inventaire."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            long id;
            if (parameters == null || parameters.Length != 2 || !Int64.TryParse(parameters[1], out id) || id <= 0)
            {
                session.SendWhisper("Objet invalide.", 1);
                return;
            }
            string message;
            bool ok = InventoryService.Use(session, id, out message);
            if (!ok && !String.IsNullOrWhiteSpace(message)) session.SendWhisper(message, 1);
        }
    }

    internal sealed class ParadiseGiveItemCommand : ParadiseInventoryCommandBase
    {
        public override string Parameters { get { return "<joueur> <objet> [quantité]"; } }
        public override string Description { get { return "Donne un objet physique à un joueur présent dans la même room."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (parameters == null || parameters.Length < 3)
            {
                session.SendWhisper("Syntaxe : :giveitem <joueur> <objet> [quantité]", 1);
                return;
            }

            string target = parameters[1];
            int quantity = 1;
            int objectEnd = parameters.Length;
            int parsedQuantity;
            if (parameters.Length >= 4 && Int32.TryParse(parameters[parameters.Length - 1], out parsedQuantity))
            {
                quantity = parsedQuantity;
                objectEnd--;
            }
            if (quantity <= 0)
            {
                session.SendWhisper("La quantité doit être supérieure à zéro.", 1);
                return;
            }

            string token = String.Join(" ", parameters, 2, objectEnd - 2).Trim();
            InventoryItem item;
            string message;
            if (!InventoryService.TryResolveItem(session.GetHabbo().Id, token, out item, out message))
            {
                session.SendWhisper(message, 1);
                return;
            }
            if (!InventoryService.Give(session, target, item.Id, quantity, out message))
                session.SendWhisper(message, 1);
        }
    }

    internal sealed class ParadiseGiveItemByIdCommand : ParadiseInventoryCommandBase
    {
        public override string Parameters { get { return "<joueur> <id> [quantité]"; } }
        public override string Description { get { return "Commande interne sécurisée de l’interface Inventaire."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            long itemId;
            int quantity = 1;
            if (parameters == null || parameters.Length < 3 || !Int64.TryParse(parameters[2], out itemId) || itemId <= 0)
            {
                session.SendWhisper("Objet invalide.", 1);
                return;
            }
            if (parameters.Length >= 4 && (!Int32.TryParse(parameters[3], out quantity) || quantity <= 0))
            {
                session.SendWhisper("Quantité invalide.", 1);
                return;
            }

            string message;
            if (!InventoryService.Give(session, parameters[1], itemId, quantity, out message))
                session.SendWhisper(message, 1);
        }
    }

    internal sealed class ParadiseWeightCommand : ParadiseInventoryCommandBase
    {
        public override string Parameters { get { return String.Empty; } }
        public override string Description { get { return "Affiche le poids actuel et la capacité de l’inventaire."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;
            session.SendWhisper(InventoryService.WeightText(session.GetHabbo().Id), 1);
        }
    }
}

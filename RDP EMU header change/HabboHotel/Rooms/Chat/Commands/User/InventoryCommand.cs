using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Paradise.Inventory;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    /// <summary>
    /// Legacy :inventario / :inv entry point, now intentionally reused by
    /// ParadiseRP Inventory V2 instead of opening the old text-only MOTD.
    /// The underlying legacy RP fields are not deleted by Phase 3.
    /// </summary>
    class InventoryCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_inventory"; }
        }

        public string Parameters
        {
            get { return String.Empty; }
        }

        public string Description
        {
            get { return "Ouvre l’inventaire physique ParadiseRP."; }
        }

        public void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;

            InventoryUiEventService.OpenInventory(session.GetHabbo().Id);
            session.SendWhisper(InventoryService.WeightText(session.GetHabbo().Id), 1);
        }
    }
}

using System;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    public sealed class InventoryItem
    {
        public long Id { get; private set; }
        public int OwnerUserId { get; private set; }
        public ItemDefinition Definition { get; private set; }
        public int Quantity { get; private set; }
        public string Metadata { get; private set; }
        public int? Slot { get; private set; }

        public decimal TotalWeight
        {
            get { return Definition == null ? 0 : Definition.Weight * Quantity; }
        }

        public InventoryItem(long id, int ownerUserId, ItemDefinition definition, int quantity, string metadata, int? slot)
        {
            Id = id;
            OwnerUserId = ownerUserId;
            Definition = definition;
            Quantity = quantity < 0 ? 0 : quantity;
            Metadata = String.IsNullOrWhiteSpace(metadata) ? null : metadata;
            Slot = slot;
        }
    }

    public sealed class InventoryCapacity
    {
        public decimal BaseCapacity { get; set; }
        public decimal CapacityBonus { get; set; }
        public int MaxSlots { get; set; }
        public decimal MaximumWeight { get { return BaseCapacity + CapacityBonus; } }
    }
}

using System;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    public sealed class ItemDefinition
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Category { get; private set; }
        public decimal Weight { get; private set; }
        public int MaxStack { get; private set; }
        public string Icon { get; private set; }
        public bool Usable { get; private set; }
        public bool Tradeable { get; private set; }
        public bool Droppable { get; private set; }
        public string EffectType { get; private set; }
        public int EffectValue { get; private set; }
        public string MetadataSchema { get; private set; }

        public ItemDefinition(int id, string code, string name, string description, string category,
            decimal weight, int maxStack, string icon, bool usable, bool tradeable, bool droppable,
            string effectType, int effectValue, string metadataSchema)
        {
            Id = id;
            Code = code ?? String.Empty;
            Name = name ?? String.Empty;
            Description = description ?? String.Empty;
            Category = String.IsNullOrWhiteSpace(category) ? "OBJECT" : category.ToUpperInvariant();
            Weight = weight < 0 ? 0 : weight;
            MaxStack = maxStack < 1 ? 1 : maxStack;
            Icon = String.IsNullOrWhiteSpace(icon) ? null : icon;
            Usable = usable;
            Tradeable = tradeable;
            Droppable = droppable;
            EffectType = String.IsNullOrWhiteSpace(effectType) ? "NONE" : effectType.ToUpperInvariant();
            EffectValue = effectValue;
            MetadataSchema = String.IsNullOrWhiteSpace(metadataSchema) ? null : metadataSchema;
        }
    }
}

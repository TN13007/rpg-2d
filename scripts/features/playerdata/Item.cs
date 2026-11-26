using System;

namespace RPG2D.Features.PlayerData
{
    public abstract class Item
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool Stackable { get; init; } = true;
        public int MaxStackSize { get; init; } = 99;

        public virtual bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            return Stackable ? MaxStackSize > 0 : MaxStackSize == 1;
        }
    }

    public sealed class ConsumableItem : Item
    {
    }

    public sealed class EquipmentItem : Item
    {
        public EquipmentSlot Slot { get; init; }

        public override bool IsValid()
        {
            return base.IsValid()
                && !Stackable
                && MaxStackSize == 1
                && Slot != EquipmentSlot.None
                && Enum.IsDefined(Slot);
        }
    }

    public sealed class QuestItem : Item
    {
    }

    public enum EquipmentSlot
    {
        None,
        MainHand,
        OffHand,
        Head,
        Body,
        Legs,
        Feet,
        Accessory
    }
}

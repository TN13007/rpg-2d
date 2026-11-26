using System;
using System.Collections.Generic;

namespace RPG2D.Features.PlayerData
{
    public sealed class PlayerItemStack
    {
        internal PlayerItemStack(Item item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public Item Item { get; }
        public string ItemId => Item.Id;
        public int Quantity { get; }
    }

    public sealed class PlayerData
    {
        private readonly List<PlayerItemStack> _inventory = new();
        private readonly IReadOnlyList<PlayerItemStack> _inventoryView;

        public PlayerData()
        {
            _inventoryView = _inventory.AsReadOnly();
        }

        public IReadOnlyList<PlayerItemStack> Inventory => _inventoryView;

        public int GetItemQuantity(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            long total = 0;
            foreach (PlayerItemStack stack in _inventory)
            {
                if (string.Equals(stack.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    total += stack.Quantity;
                }
            }

            return total > int.MaxValue ? int.MaxValue : (int)total;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return quantity > 0 && GetItemQuantity(itemId) >= quantity;
        }

        public bool TryAddItem(Item item, int quantity)
        {
            if (item == null || !item.IsValid() || quantity <= 0)
            {
                return false;
            }

            long currentQuantity = GetItemQuantity(item.Id);
            if (currentQuantity + quantity > int.MaxValue)
            {
                return false;
            }

            List<PlayerItemStack> updatedInventory = new(_inventory.Count + 1);
            foreach (PlayerItemStack stack in _inventory)
            {
                if (string.Equals(
                    stack.ItemId,
                    item.Id,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    if (stack.Quantity <= 0 ||
                        !AreCompatibleDefinitions(stack.Item, item) ||
                        (item.Stackable && stack.Quantity > item.MaxStackSize) ||
                        (!item.Stackable && stack.Quantity != 1))
                    {
                        return false;
                    }
                }

                updatedInventory.Add(new PlayerItemStack(stack.Item, stack.Quantity));
            }

            int remaining = quantity;

            if (item.Stackable)
            {
                for (int index = 0; index < updatedInventory.Count && remaining > 0; index++)
                {
                    PlayerItemStack stack = updatedInventory[index];
                    if (!string.Equals(
                        stack.ItemId,
                        item.Id,
                        StringComparison.OrdinalIgnoreCase
                    ))
                    {
                        continue;
                    }

                    int availableSpace = item.MaxStackSize - stack.Quantity;
                    int amountToAdd = Math.Min(availableSpace, remaining);
                    if (amountToAdd <= 0)
                    {
                        continue;
                    }

                    updatedInventory[index] = new PlayerItemStack(
                        stack.Item,
                        stack.Quantity + amountToAdd
                    );
                    remaining -= amountToAdd;
                }
            }

            while (remaining > 0)
            {
                int amountToAdd = item.Stackable
                    ? Math.Min(item.MaxStackSize, remaining)
                    : 1;

                updatedInventory.Add(new PlayerItemStack(item, amountToAdd));
                remaining -= amountToAdd;
            }

            _inventory.Clear();
            _inventory.AddRange(updatedInventory);
            return true;
        }

        public bool TryRemoveItem(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0 || !HasItem(itemId, quantity))
            {
                return false;
            }

            int remaining = quantity;
            for (int index = _inventory.Count - 1; index >= 0 && remaining > 0; index--)
            {
                PlayerItemStack stack = _inventory[index];
                if (!string.Equals(stack.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (stack.Quantity <= remaining)
                {
                    remaining -= stack.Quantity;
                    _inventory.RemoveAt(index);
                }
                else
                {
                    _inventory[index] = new PlayerItemStack(
                        stack.Item,
                        stack.Quantity - remaining
                    );
                    remaining = 0;
                }
            }

            return remaining == 0;
        }

        private static bool AreCompatibleDefinitions(Item first, Item second)
        {
            if (first.GetType() != second.GetType() ||
                first.Stackable != second.Stackable ||
                first.MaxStackSize != second.MaxStackSize)
            {
                return false;
            }

            if (first is EquipmentItem firstEquipment && second is EquipmentItem secondEquipment)
            {
                return firstEquipment.Slot == secondEquipment.Slot;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace RPG2D.Features.PlayerData
{
    public sealed class ItemDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;
        [JsonPropertyName("stackable")]
        public bool Stackable { get; init; } = true;
        [JsonPropertyName("maxStackSize")]
        public int MaxStackSize { get; init; } = 99;

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
                return false;
            return Stackable ? MaxStackSize > 0 : MaxStackSize == 1;
        }
    }

    public sealed class PlayerItemStack
    {
        internal PlayerItemStack(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
        public string ItemId { get; }
        public int Quantity { get; }
    }

    public sealed class PlayerData
    {
        private readonly List<PlayerItemStack> _inventory = new();
        private readonly IReadOnlyList<PlayerItemStack> _inventoryView;

        public PlayerData(int currency = 0)
        {
            if (currency < 0)
                throw new ArgumentOutOfRangeException(nameof(currency));
            Currency = currency;
            _inventoryView = _inventory.AsReadOnly();
        }

        public int Currency { get; private set; }
        public IReadOnlyList<PlayerItemStack> Inventory => _inventoryView;

        public int GetItemQuantity(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return 0;

            long total = 0;
            foreach (PlayerItemStack stack in _inventory)
            {
                if (string.Equals(stack.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                    total += stack.Quantity;
            }
            return total > int.MaxValue ? int.MaxValue : (int)total;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return quantity > 0 && GetItemQuantity(itemId) >= quantity;
        }

        public bool TryAddItem(ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || !itemDefinition.IsValid() || quantity <= 0)
                return false;

            long currentQuantity = GetItemQuantity(itemDefinition.Id);
            if (currentQuantity + quantity > int.MaxValue)
                return false;

            List<PlayerItemStack> updatedInventory = new(_inventory.Count + 1);
            foreach (PlayerItemStack stack in _inventory)
            {
                if (string.Equals(stack.ItemId, itemDefinition.Id, StringComparison.OrdinalIgnoreCase) &&
                    (stack.Quantity <= 0 ||
                     (itemDefinition.Stackable && stack.Quantity > itemDefinition.MaxStackSize) ||
                     (!itemDefinition.Stackable && stack.Quantity != 1)))
                    return false;

                updatedInventory.Add(new PlayerItemStack(stack.ItemId, stack.Quantity));
            }

            int remaining = quantity;
            if (itemDefinition.Stackable)
            {
                for (int index = 0; index < updatedInventory.Count && remaining > 0; index++)
                {
                    PlayerItemStack stack = updatedInventory[index];
                    if (!string.Equals(stack.ItemId, itemDefinition.Id, StringComparison.OrdinalIgnoreCase))
                        continue;

                    int availableSpace = itemDefinition.MaxStackSize - stack.Quantity;
                    int amountToAdd = Math.Min(availableSpace, remaining);
                    if (amountToAdd <= 0)
                        continue;

                    updatedInventory[index] = new PlayerItemStack(stack.ItemId, stack.Quantity + amountToAdd);
                    remaining -= amountToAdd;
                }
            }

            while (remaining > 0)
            {
                int amountToAdd = itemDefinition.Stackable
                    ? Math.Min(itemDefinition.MaxStackSize, remaining)
                    : 1;
                updatedInventory.Add(new PlayerItemStack(itemDefinition.Id, amountToAdd));
                remaining -= amountToAdd;
            }

            _inventory.Clear();
            _inventory.AddRange(updatedInventory);
            return true;
        }

        public bool TryRemoveItem(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0 || !HasItem(itemId, quantity))
                return false;

            int remaining = quantity;
            for (int index = _inventory.Count - 1; index >= 0 && remaining > 0; index--)
            {
                PlayerItemStack stack = _inventory[index];
                if (!string.Equals(stack.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (stack.Quantity <= remaining)
                {
                    remaining -= stack.Quantity;
                    _inventory.RemoveAt(index);
                }
                else
                {
                    _inventory[index] = new PlayerItemStack(stack.ItemId, stack.Quantity - remaining);
                    remaining = 0;
                }
            }
            return remaining == 0;
        }

        public bool TryAddCurrency(int amount)
        {
            if (amount <= 0 || (long)Currency + amount > int.MaxValue)
                return false;
            Currency += amount;
            return true;
        }

        public bool TrySpendCurrency(int amount, bool allowDebt = false)
        {
            if (amount <= 0)
                return false;

            long nextBalance = (long)Currency - amount;
            if ((!allowDebt && nextBalance < 0) || nextBalance < int.MinValue)
                return false;

            Currency = (int)nextBalance;
            return true;
        }

        public bool TryPurchaseItem(
            ItemDefinition itemDefinition,
            int unitPrice,
            int quantity,
            out string failureReason)
        {
            failureReason = string.Empty;
            if (itemDefinition == null || !itemDefinition.IsValid() || unitPrice < 0 || quantity <= 0)
            {
                failureReason = "Thông tin giao dịch không hợp lệ.";
                return false;
            }

            long totalCost = (long)unitPrice * quantity;
            if (totalCost > int.MaxValue)
            {
                failureReason = "Tổng giá trị giao dịch vượt giới hạn.";
                return false;
            }
            if (Currency < totalCost)
            {
                failureReason = "Bạn không đủ tiền.";
                return false;
            }
            if (!TryAddItem(itemDefinition, quantity))
            {
                failureReason = "Không thể thêm vật phẩm vào Inventory.";
                return false;
            }

            Currency -= (int)totalCost;
            return true;
        }
    }
}
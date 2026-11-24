using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RPG2D.Features.PlayerData
{
    public sealed class ItemDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public sealed class ItemManifest
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonPropertyName("items")]
        public List<ItemDefinition> Items { get; set; } = new();
    }

    public sealed class ItemDatabase
    {
        private const string ManifestPath = "res://data/items/items.json";
        private readonly Dictionary<string, ItemDefinition> _items =
            new(StringComparer.OrdinalIgnoreCase);

        public static ItemDatabase Load()
        {
            ItemDatabase database = new();
            Godot.FileAccess? file = Godot.FileAccess.Open(
                ManifestPath,
                Godot.FileAccess.ModeFlags.Read
            );

            if (file == null)
            {
                GD.PushWarning($"Could not read item manifest '{ManifestPath}'.");
                return database;
            }

            string json = file.GetAsText();
            file.Close();

            try
            {
                ItemManifest? manifest = JsonSerializer.Deserialize<ItemManifest>(json);
                if (manifest == null || manifest.SchemaVersion != 1 || manifest.Items == null)
                {
                    GD.PushWarning($"Invalid item manifest '{ManifestPath}'.");
                    return database;
                }

                foreach (ItemDefinition item in manifest.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Id) ||
                        string.IsNullOrWhiteSpace(item.Name) ||
                        !database._items.TryAdd(item.Id, item))
                    {
                        GD.PushWarning($"Skipping invalid or duplicate item in '{ManifestPath}'.");
                    }
                }
            }
            catch (JsonException exception)
            {
                GD.PushWarning($"Invalid item JSON in '{ManifestPath}': {exception.Message}");
            }

            return database;
        }

        public bool TryGetItem(string itemId, out ItemDefinition item)
        {
            return _items.TryGetValue(itemId, out item!);
        }
    }

    public sealed class PlayerItemStack
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public sealed class PlayerData
    {
        [JsonPropertyName("currency")]
        public int Currency { get; set; }

        [JsonPropertyName("inventory")]
        public List<PlayerItemStack> Inventory { get; set; } = new();

        public int GetItemQuantity(string itemId)
        {
            return Inventory.FirstOrDefault(
                stack => string.Equals(stack.ItemId, itemId, StringComparison.OrdinalIgnoreCase)
            )?.Quantity ?? 0;
        }

        public bool TryPurchaseItem(
            string itemId,
            int unitPrice,
            int quantity,
            out string failureReason
        )
        {
            failureReason = string.Empty;

            if (string.IsNullOrWhiteSpace(itemId) || unitPrice < 0 || quantity <= 0)
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

            PlayerItemStack? existingStack = Inventory.FirstOrDefault(
                stack => string.Equals(stack.ItemId, itemId, StringComparison.OrdinalIgnoreCase)
            );

            if (existingStack != null && existingStack.Quantity > int.MaxValue - quantity)
            {
                failureReason = "Số lượng vật phẩm vượt giới hạn.";
                return false;
            }

            Currency -= (int)totalCost;

            if (existingStack == null)
            {
                Inventory.Add(new PlayerItemStack { ItemId = itemId, Quantity = quantity });
            }
            else
            {
                existingStack.Quantity += quantity;
            }

            return true;
        }

        public bool TryRemoveItem(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            {
                return false;
            }

            PlayerItemStack? stack = Inventory.FirstOrDefault(
                item => string.Equals(item.ItemId, itemId, StringComparison.OrdinalIgnoreCase)
            );

            if (stack == null || stack.Quantity < quantity)
            {
                return false;
            }

            stack.Quantity -= quantity;
            if (stack.Quantity == 0)
            {
                Inventory.Remove(stack);
            }

            return true;
        }
    }

    public static class PlayerDataDatabase
    {
        private const string PlayerDataPath = "res://data/player/player_data.json";

        public static PlayerData Load()
        {
            Godot.FileAccess? file = Godot.FileAccess.Open(
                PlayerDataPath,
                Godot.FileAccess.ModeFlags.Read
            );

            if (file == null)
            {
                GD.PushWarning($"Could not read player data '{PlayerDataPath}'.");
                return new PlayerData();
            }

            string json = file.GetAsText();
            file.Close();

            try
            {
                PlayerData? playerData = JsonSerializer.Deserialize<PlayerData>(json);
                if (playerData == null || playerData.Currency < 0 || playerData.Inventory == null)
                {
                    GD.PushWarning($"Invalid player data '{PlayerDataPath}'.");
                    return new PlayerData();
                }

                playerData.Inventory.RemoveAll(
                    stack => string.IsNullOrWhiteSpace(stack.ItemId) || stack.Quantity <= 0
                );
                return playerData;
            }
            catch (JsonException exception)
            {
                GD.PushWarning($"Invalid player data JSON: {exception.Message}");
                return new PlayerData();
            }
        }
    }
}

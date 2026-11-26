using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RPG2D.Features.Shop
{
    public sealed class ShopItemDefinition
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }
    }

    public sealed class ShopBuybackDefinition
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public int Price { get; set; }
    }

    public sealed class ShopDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<ShopItemDefinition> Items { get; set; } = null!;

        [JsonPropertyName("buybackPrices")]
        public List<ShopBuybackDefinition> BuybackPrices { get; set; } = null!;

        public bool TryGetBuybackPrice(string itemId, out int price)
        {
            price = 0;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            foreach (ShopBuybackDefinition entry in BuybackPrices)
            {
                if (string.Equals(entry.ItemId, itemId, StringComparison.OrdinalIgnoreCase) &&
                    entry.Price > 0)
                {
                    price = entry.Price;
                    return true;
                }
            }

            return false;
        }
    }

    public static class ShopDatabase
    {
        private const string DefaultShopPath = "res://data/shops/default_shop.json";

        public static ShopDefinition? LoadDefaultShop()
        {
            Godot.FileAccess? file = Godot.FileAccess.Open(
                DefaultShopPath,
                Godot.FileAccess.ModeFlags.Read
            );

            if (file == null)
            {
                GD.PushWarning($"Could not read shop data '{DefaultShopPath}'.");
                return null;
            }

            string json = file.GetAsText();
            file.Close();

            try
            {
                return FromJson(json);
            }
            catch (JsonException exception)
            {
                GD.PushWarning($"Invalid shop JSON in '{DefaultShopPath}': {exception.Message}");
                return null;
            }
        }

        public static ShopDefinition FromJson(string json)
        {
            ShopDefinition? shop = JsonSerializer.Deserialize<ShopDefinition>(json);
            if (shop == null ||
                string.IsNullOrWhiteSpace(shop.Id) ||
                string.IsNullOrWhiteSpace(shop.Name) ||
                shop.Items == null ||
                shop.BuybackPrices == null)
            {
                throw new JsonException("Shop data must contain an id, name, items and buybackPrices.");
            }

            HashSet<string> seenShopItems = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < shop.Items.Count; index++)
            {
                ShopItemDefinition? item = shop.Items[index];
                if (item == null ||
                    string.IsNullOrWhiteSpace(item.ItemId) ||
                    item.Price < 0 ||
                    item.Stock < 0 ||
                    !seenShopItems.Add(item.ItemId))
                {
                    throw new JsonException(
                        $"Shop item at index {index} is invalid or has a duplicate itemId."
                    );
                }
            }

            HashSet<string> seenBuybackItems = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < shop.BuybackPrices.Count; index++)
            {
                ShopBuybackDefinition? entry = shop.BuybackPrices[index];
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.ItemId) ||
                    entry.Price < 0 ||
                    !seenBuybackItems.Add(entry.ItemId))
                {
                    throw new JsonException(
                        $"Shop buyback entry at index {index} is invalid or has a duplicate itemId."
                    );
                }
            }

            return shop;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RPG2D.Features.PlayerData
{
    internal sealed class ItemManifest
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonPropertyName("items")]
        public List<JsonElement>? Items { get; set; }
    }

    public sealed class ItemDatabase
    {
        private const string ManifestPath = "res://data/items/items.json";
        private readonly Dictionary<string, Item> _items =
            new(StringComparer.OrdinalIgnoreCase);

        public static ItemDatabase Load()
        {
            Godot.FileAccess? file = Godot.FileAccess.Open(
                ManifestPath,
                Godot.FileAccess.ModeFlags.Read
            );

            if (file == null)
            {
                GD.PushWarning("Could not read item manifest '" + ManifestPath + "'.");
                return new ItemDatabase();
            }

            string json = file.GetAsText();
            file.Close();

            try
            {
                return FromJson(json);
            }
            catch (JsonException exception)
            {
                GD.PushWarning("Invalid item JSON in '" + ManifestPath + "': " + exception.Message);
                return new ItemDatabase();
            }
        }

        public static ItemDatabase FromJson(string json)
        {
            ItemManifest? manifest = JsonSerializer.Deserialize<ItemManifest>(json);
            if (manifest == null || manifest.SchemaVersion != 1 || manifest.Items == null)
            {
                throw new JsonException("The item manifest must use schemaVersion 1 and contain an items array.");
            }

            ItemDatabase database = new();
            foreach (JsonElement itemData in manifest.Items)
            {
                if (!TryCreateItem(itemData, out Item? item) ||
                    item == null ||
                    !item.IsValid() ||
                    !database._items.TryAdd(item.Id, item))
                {
                    continue;
                }
            }

            return database;
        }

        public bool TryGetItem(string itemId, out Item item)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                item = null!;
                return false;
            }

            return _items.TryGetValue(itemId, out item!);
        }

        private static bool TryCreateItem(JsonElement itemData, out Item? item)
        {
            item = null;

            if (itemData.ValueKind != JsonValueKind.Object ||
                !TryReadString(itemData, "type", out string type) ||
                !TryReadString(itemData, "id", out string id) ||
                !TryReadString(itemData, "name", out string name) ||
                !TryReadBoolean(itemData, "stackable", out bool stackable) ||
                !TryReadInteger(itemData, "maxStackSize", out int maxStackSize))
            {
                return false;
            }

            string description = string.Empty;
            if (itemData.TryGetProperty("description", out JsonElement descriptionElement))
            {
                if (descriptionElement.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                description = descriptionElement.GetString() ?? string.Empty;
            }

            Item commonItem = type switch
            {
                "Consumable" => new ConsumableItem(),
                "Equipment" => new EquipmentItem(),
                "Quest" => new QuestItem(),
                _ => null!
            };

            if (commonItem == null)
            {
                return false;
            }

            if (commonItem is EquipmentItem)
            {
                if (!TryReadString(itemData, "equipmentSlot", out string slotName) ||
                    !TryParseEquipmentSlot(slotName, out EquipmentSlot slot))
                {
                    return false;
                }

                item = new EquipmentItem
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    Stackable = stackable,
                    MaxStackSize = maxStackSize,
                    Slot = slot
                };
                return true;
            }

            item = commonItem switch
            {
                ConsumableItem => new ConsumableItem
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    Stackable = stackable,
                    MaxStackSize = maxStackSize
                },
                QuestItem => new QuestItem
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    Stackable = stackable,
                    MaxStackSize = maxStackSize
                },
                _ => null
            };

            return item != null;
        }

        private static bool TryReadString(JsonElement element, string propertyName, out string value)
        {
            value = string.Empty;
            if (!element.TryGetProperty(propertyName, out JsonElement property) ||
                property.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            value = property.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryReadBoolean(JsonElement element, string propertyName, out bool value)
        {
            value = false;
            if (!element.TryGetProperty(propertyName, out JsonElement property) ||
                (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False))
            {
                return false;
            }

            value = property.GetBoolean();
            return true;
        }

        private static bool TryReadInteger(JsonElement element, string propertyName, out int value)
        {
            value = 0;
            return element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind == JsonValueKind.Number &&
                property.TryGetInt32(out value);
        }

        private static bool TryParseEquipmentSlot(string value, out EquipmentSlot slot)
        {
            slot = value switch
            {
                "main_hand" => EquipmentSlot.MainHand,
                "off_hand" => EquipmentSlot.OffHand,
                "head" => EquipmentSlot.Head,
                "body" => EquipmentSlot.Body,
                "legs" => EquipmentSlot.Legs,
                "feet" => EquipmentSlot.Feet,
                "accessory" => EquipmentSlot.Accessory,
                _ => (EquipmentSlot)(-1)
            };

            return Enum.IsDefined(slot);
        }
    }
}

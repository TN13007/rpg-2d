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
        public List<ItemDefinition?> Items { get; set; } = new();
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

                foreach (ItemDefinition? item in manifest.Items)
                {
                    if (item == null || !item.IsValid() || !database._items.TryAdd(item.Id, item))
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
            if (string.IsNullOrWhiteSpace(itemId))
            {
                item = null!;
                return false;
            }

            return _items.TryGetValue(itemId, out item!);
        }
    }
}

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RPG2D.Features.PlayerData
{
    internal sealed class PlayerDataManifest
    {
        [JsonPropertyName("currency")]
        public int Currency { get; set; }

        [JsonPropertyName("inventory")]
        public List<PlayerItemStackData?> Inventory { get; set; } = new();
    }

    internal sealed class PlayerItemStackData
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public static class PlayerDataDatabase
    {
        private const string PlayerDataPath = "res://data/player/player_data.json";

        public static PlayerData Load(ItemDatabase itemDatabase)
        {
            Godot.FileAccess? file = Godot.FileAccess.Open(
                PlayerDataPath,
                Godot.FileAccess.ModeFlags.Read
            );

            if (file == null)
            {
                GD.PushWarning("Could not read player data '" + PlayerDataPath + "'.");
                return new PlayerData();
            }

            string json = file.GetAsText();
            file.Close();

            try
            {
                PlayerDataManifest? manifest = JsonSerializer.Deserialize<PlayerDataManifest>(json);
                if (manifest == null || manifest.Currency < 0 || manifest.Inventory == null)
                {
                    GD.PushWarning("Invalid player data '" + PlayerDataPath + "'.");
                    return new PlayerData();
                }

                PlayerData playerData = new(manifest.Currency);
                foreach (PlayerItemStackData? stack in manifest.Inventory)
                {
                    if (stack == null ||
                        stack.Quantity <= 0 ||
                        !itemDatabase.TryGetItem(stack.ItemId, out Item item) ||
                        !playerData.TryAddItem(item, stack.Quantity))
                    {
                        GD.PushWarning("Skipping invalid inventory stack in '" + PlayerDataPath + "'.");
                    }
                }

                return playerData;
            }
            catch (JsonException exception)
            {
                GD.PushWarning("Invalid player data JSON: " + exception.Message);
                return new PlayerData();
            }

        }
    }
}

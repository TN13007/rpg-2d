using System;
using System.Text.Json.Serialization;

namespace RPG2D.Features.Interaction.Data
{
    public sealed class DialogueChoiceDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("shopMode")]
        public string ShopMode { get; set; } = string.Empty;

        public static bool IsSupportedShopMode(string shopMode)
        {
            return string.Equals(shopMode, "Buying", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(shopMode, "Selling", StringComparison.OrdinalIgnoreCase);
        }

        [JsonPropertyName("closesInteraction")]
        public bool ClosesInteraction { get; set; }

        [JsonPropertyName("nextDialogueId")]
        public string NextDialogueId { get; set; } = string.Empty;
    }
}

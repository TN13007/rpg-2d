using System.Text.Json.Serialization;

namespace RPG2D.Features.Interaction.Data
{
    public sealed class DialogueChoiceDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("closesInteraction")]
        public bool ClosesInteraction { get; set; }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RPG2D.Features.Interaction.Data
{
    public sealed class DialogueDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<DialogueChoiceDefinition> Choices { get; set; } = new();
    }
}
using RPG2D.Features.PlayerData;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class ItemDatabaseTests
    {
        [Fact]
        public void FromJson_CreatesTheMatchingItemTypes()
        {
            const string json = """
            {
              "schemaVersion": 1,
              "items": [
                {
                  "id": "health_potion",
                  "type": "Consumable",
                  "name": "Health Potion",
                  "stackable": true,
                  "maxStackSize": 20
                },
                {
                  "id": "iron_sword",
                  "type": "Equipment",
                  "name": "Iron Sword",
                  "stackable": false,
                  "maxStackSize": 1,
                  "equipmentSlot": "main_hand"
                },
                {
                  "id": "ancient_key",
                  "type": "Quest",
                  "name": "Ancient Key",
                  "stackable": true,
                  "maxStackSize": 10
                }
              ]
            }
            """;

            ItemDatabase database = ItemDatabase.FromJson(json);

            Assert.True(database.TryGetItem("health_potion", out Item consumable));
            Assert.IsType<ConsumableItem>(consumable);

            Assert.True(database.TryGetItem("iron_sword", out Item equipment));
            EquipmentItem equipmentItem = Assert.IsType<EquipmentItem>(equipment);
            Assert.Equal(EquipmentSlot.MainHand, equipmentItem.Slot);

            Assert.True(database.TryGetItem("ancient_key", out Item questItem));
            Assert.IsType<QuestItem>(questItem);
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("")]
        public void FromJson_SkipsUnknownItemTypes(string itemType)
        {
            string itemJson = "{\"id\":\"unknown_item\",\"type\":" +
                System.Text.Json.JsonSerializer.Serialize(itemType) +
                ",\"name\":\"Unknown\",\"stackable\":true,\"maxStackSize\":10}";

            ItemDatabase database = ItemDatabase.FromJson(CreateManifest(itemJson));

            Assert.False(database.TryGetItem("unknown_item", out _));
        }

        [Theory]
        [InlineData("invalid_slot")]
        [InlineData("")]
        public void FromJson_SkipsEquipmentWithUnknownSlot(string slot)
        {
            string itemJson = "{\"id\":\"invalid_equipment\",\"type\":\"Equipment\"," +
                "\"name\":\"Invalid Equipment\",\"stackable\":false,\"maxStackSize\":1," +
                "\"equipmentSlot\":" + System.Text.Json.JsonSerializer.Serialize(slot) + "}";

            ItemDatabase database = ItemDatabase.FromJson(CreateManifest(itemJson));

            Assert.False(database.TryGetItem("invalid_equipment", out _));
        }

        [Fact]
        public void FromJson_SkipsEquipmentThatCanStack()
        {
            const string json = """
            {
              "schemaVersion": 1,
              "items": [
                {
                  "id": "stacking_sword",
                  "type": "Equipment",
                  "name": "Stacking Sword",
                  "stackable": true,
                  "maxStackSize": 5,
                  "equipmentSlot": "main_hand"
                }
              ]
            }
            """;

            ItemDatabase database = ItemDatabase.FromJson(json);

            Assert.False(database.TryGetItem("stacking_sword", out _));
        }

        [Fact]
        public void FromJson_RejectsUnsupportedSchemaVersion()
        {
            const string json = """{"schemaVersion":2,"items":[]}""";

            Assert.Throws<System.Text.Json.JsonException>(() => ItemDatabase.FromJson(json));
        }

        private static string CreateManifest(string itemJson)
        {
            return """
            {
              "schemaVersion": 1,
              "items": [
            """ + itemJson + """
              ]
            }
            """;
        }
    }
}

using System.Text.Json;
using RPG2D.Features.Interaction.Data;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class DialogueChoiceDefinitionTests
    {
        [Theory]
        [InlineData("Buying")]
        [InlineData("buying")]
        [InlineData("Selling")]
        [InlineData("selling")]
        public void IsSupportedShopMode_WhenModeIsKnown_ReturnsTrue(string shopMode)
        {
            Assert.True(DialogueChoiceDefinition.IsSupportedShopMode(shopMode));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Buy")]
        [InlineData("Trading")]
        [InlineData("Unknown")]
        public void IsSupportedShopMode_WhenModeIsUnknown_ReturnsFalse(string shopMode)
        {
            Assert.False(DialogueChoiceDefinition.IsSupportedShopMode(shopMode));
        }

        [Fact]
        public void Deserialize_WhenShopModeIsProvided_ReadsActionParameter()
        {
            const string json = """
                {
                  "id": "shop_sell",
                  "text": "Bán vật phẩm",
                  "action": "openShop",
                  "shopMode": "Selling"
                }
                """;

            DialogueChoiceDefinition? choice =
                JsonSerializer.Deserialize<DialogueChoiceDefinition>(json);

            Assert.NotNull(choice);
            Assert.Equal("openShop", choice.Action);
            Assert.Equal("Selling", choice.ShopMode);
            Assert.True(DialogueChoiceDefinition.IsSupportedShopMode(choice.ShopMode));
        }
    }
}

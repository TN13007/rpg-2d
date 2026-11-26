using System.Text.Json;
using RPG2D.Features.Shop;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class ShopDatabaseTests
    {
        [Fact]
        public void FromJson_WhenShopItemHasInvalidData_ThrowsJsonException()
        {
            const string json = """
                {
                  "id": "test_shop",
                  "name": "Test Shop",
                  "items": [
                    {
                      "itemId": "",
                      "price": 100,
                      "stock": 5
                    }
                  ],
                  "buybackPrices": []
                }
                """;

            Assert.Throws<JsonException>(() => ShopDatabase.FromJson(json));
        }

        [Fact]
        public void FromJson_WhenShopItemIdIsDuplicated_ThrowsJsonException()
        {
            const string json = """
                {
                  "id": "test_shop",
                  "name": "Test Shop",
                  "items": [
                    {
                      "itemId": "item_a",
                      "price": 100,
                      "stock": 5
                    },
                    {
                      "itemId": "ITEM_A",
                      "price": 120,
                      "stock": 2
                    }
                  ],
                  "buybackPrices": []
                }
                """;

            Assert.Throws<JsonException>(() => ShopDatabase.FromJson(json));
        }

        [Fact]
        public void FromJson_WhenShopItemEntryIsNull_ThrowsJsonException()
        {
            const string json = """
                {
                  "id": "test_shop",
                  "name": "Test Shop",
                  "items": [null],
                  "buybackPrices": []
                }
                """;

            Assert.Throws<JsonException>(() => ShopDatabase.FromJson(json));
        }
    }
}

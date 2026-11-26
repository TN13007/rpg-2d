using RPG2D.Features.PlayerData;
using RPG2D.Features.Shop;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class ShopSellingTests
    {
        private static ConsumableItem CreateConsumable(int maxStackSize = 20)
        {
            return new ConsumableItem
            {
                Id = "health_potion",
                Name = "Bình máu",
                Stackable = true,
                MaxStackSize = maxStackSize
            };
        }

        [Fact]
        public void TrySellItem_WhenSaleIsValid_RemovesItemsAndAddsCurrency()
        {
            PlayerData playerData = new(100);
            Assert.True(playerData.TryAddItem(CreateConsumable(), 3));

            bool sold = playerData.TrySellItem("health_potion", 40, 2, out string failureReason);

            Assert.True(sold);
            Assert.Empty(failureReason);
            Assert.Equal(180, playerData.Currency);
            Assert.Equal(1, playerData.GetItemQuantity("health_potion"));
        }

        [Fact]
        public void TrySellItem_WhenQuantitySpansMultipleStacks_RemovesAcrossStacks()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateConsumable(5);
            Assert.True(playerData.TryAddItem(item, 7));

            bool sold = playerData.TrySellItem(item.Id, 25, 6, out string failureReason);

            Assert.True(sold);
            Assert.Empty(failureReason);
            Assert.Equal(150, playerData.Currency);
            Assert.Equal(1, playerData.GetItemQuantity(item.Id));
            Assert.Single(playerData.Inventory);
            Assert.Equal(1, playerData.Inventory[0].Quantity);
        }

        [Fact]
        public void TrySellItem_WhenQuantityIsInsufficient_LeavesPlayerDataUnchanged()
        {
            PlayerData playerData = new(100);
            Assert.True(playerData.TryAddItem(CreateConsumable(), 2));

            bool sold = playerData.TrySellItem(
                "health_potion",
                40,
                3,
                out string failureReason
            );

            Assert.False(sold);
            Assert.Equal("Bạn không có đủ vật phẩm.", failureReason);
            Assert.Equal(100, playerData.Currency);
            Assert.Equal(2, playerData.GetItemQuantity("health_potion"));
        }

        [Fact]
        public void TrySellItem_WhenCurrencyWouldOverflow_LeavesPlayerDataUnchanged()
        {
            PlayerData playerData = new(int.MaxValue - 10);
            Assert.True(playerData.TryAddItem(CreateConsumable(), 1));

            bool sold = playerData.TrySellItem(
                "health_potion",
                20,
                1,
                out string failureReason
            );

            Assert.False(sold);
            Assert.Equal("Số vàng nhận được vượt giới hạn.", failureReason);
            Assert.Equal(int.MaxValue - 10, playerData.Currency);
            Assert.Equal(1, playerData.GetItemQuantity("health_potion"));
        }

        [Fact]
        public void TrySellItem_WhenTotalRevenueWouldOverflow_LeavesPlayerDataUnchanged()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateConsumable(int.MaxValue);
            Assert.True(playerData.TryAddItem(item, int.MaxValue));

            bool sold = playerData.TrySellItem(
                item.Id,
                2,
                int.MaxValue,
                out string failureReason
            );

            Assert.False(sold);
            Assert.Equal("Tổng giá trị giao dịch vượt giới hạn.", failureReason);
            Assert.Equal(0, playerData.Currency);
            Assert.Equal(int.MaxValue, playerData.GetItemQuantity(item.Id));
        }

        [Theory]
        [InlineData("", 10, 1)]
        [InlineData("health_potion", 0, 1)]
        [InlineData("health_potion", -1, 1)]
        [InlineData("health_potion", 10, 0)]
        [InlineData("health_potion", 10, -1)]
        public void TrySellItem_WhenTransactionInputIsInvalid_FailsWithoutMutation(
            string itemId,
            int unitPrice,
            int quantity
        )
        {
            PlayerData playerData = new(100);
            Assert.True(playerData.TryAddItem(CreateConsumable(), 2));

            bool sold = playerData.TrySellItem(
                itemId,
                unitPrice,
                quantity,
                out string failureReason
            );

            Assert.False(sold);
            Assert.Equal("Thông tin giao dịch không hợp lệ.", failureReason);
            Assert.Equal(100, playerData.Currency);
            Assert.Equal(2, playerData.GetItemQuantity("health_potion"));
        }

        [Fact]
        public void ShopDatabase_WhenLoaded_ProvidesConfiguredBuybackPrices()
        {
            const string json = """
                {
                  "id": "test_shop",
                  "name": "Test Shop",
                  "items": [],
                  "buybackPrices": [
                    { "itemId": "health_potion", "price": 40 },
                    { "itemId": "iron_sword", "price": 150 },
                    { "itemId": "ancient_key", "price": 0 }
                  ]
                }
                """;

            ShopDefinition shop = ShopDatabase.FromJson(json);

            Assert.True(shop.TryGetBuybackPrice("HEALTH_POTION", out int potionPrice));
            Assert.Equal(40, potionPrice);
            Assert.True(shop.TryGetBuybackPrice("iron_sword", out int swordPrice));
            Assert.Equal(150, swordPrice);
            Assert.False(shop.TryGetBuybackPrice("ancient_key", out _));
        }
    }
}

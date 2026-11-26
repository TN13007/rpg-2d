using RPG2D.Features.PlayerData;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class PlayerDataTests
    {
        private static ConsumableItem CreateStackableItem(int maxStackSize = 20)
        {
            return new ConsumableItem
            {
                Id = "health_potion",
                Name = "Bình máu",
                Stackable = true,
                MaxStackSize = maxStackSize
            };
        }

        private static EquipmentItem CreateNonStackableItem()
        {
            return new EquipmentItem
            {
                Id = "iron_sword",
                Name = "Kiếm sắt",
                Stackable = false,
                MaxStackSize = 1,
                Slot = EquipmentSlot.MainHand
            };
        }

        [Fact]
        public void TryAddItem_WhenItemIsNew_AddsItToInventory()
        {
            PlayerData playerData = new();

            bool added = playerData.TryAddItem(CreateNonStackableItem(), 1);

            Assert.True(added);
            Assert.Equal(1, playerData.GetItemQuantity("iron_sword"));
            Assert.Single(playerData.Inventory);
            Assert.IsType<EquipmentItem>(playerData.Inventory[0].Item);
        }

        [Fact]
        public void TryAddItem_WhenStackExists_IncreasesItsQuantity()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem();

            Assert.True(playerData.TryAddItem(item, 1));
            Assert.True(playerData.TryAddItem(item, 2));

            Assert.Equal(3, playerData.GetItemQuantity(item.Id));
            Assert.Single(playerData.Inventory);
            Assert.Equal(3, playerData.Inventory[0].Quantity);
            Assert.Same(item, playerData.Inventory[0].Item);
        }

        [Fact]
        public void TryAddItem_WhenQuantityExceedsStackLimit_FillsAndCreatesAnotherStack()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem();

            Assert.True(playerData.TryAddItem(item, 19));
            Assert.True(playerData.TryAddItem(item, 5));

            Assert.Equal(24, playerData.GetItemQuantity(item.Id));
            Assert.Equal(2, playerData.Inventory.Count);
            Assert.Equal(20, playerData.Inventory[0].Quantity);
            Assert.Equal(4, playerData.Inventory[1].Quantity);
            Assert.All(playerData.Inventory, stack => Assert.Same(item, stack.Item));
        }

        [Fact]
        public void TryAddItem_WhenItemIsNotStackable_CreatesOneStackPerItem()
        {
            PlayerData playerData = new();
            EquipmentItem item = CreateNonStackableItem();

            bool added = playerData.TryAddItem(item, 3);

            Assert.True(added);
            Assert.Equal(3, playerData.GetItemQuantity(item.Id));
            Assert.Equal(3, playerData.Inventory.Count);
            Assert.All(playerData.Inventory, stack => Assert.Equal(1, stack.Quantity));
        }

        [Fact]
        public void HasItem_WhenQuantityIsAvailable_ReturnsTrue()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem();
            Assert.True(playerData.TryAddItem(item, 3));

            Assert.True(playerData.HasItem(item.Id));
            Assert.True(playerData.HasItem(item.Id, 3));
            Assert.False(playerData.HasItem(item.Id, 4));
        }

        [Fact]
        public void TryRemoveItem_WhenQuantitySpansMultipleStacks_RemovesAcrossStacks()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem(5);
            Assert.True(playerData.TryAddItem(item, 7));

            bool removed = playerData.TryRemoveItem(item.Id, 6);

            Assert.True(removed);
            Assert.Equal(1, playerData.GetItemQuantity(item.Id));
            Assert.Single(playerData.Inventory);
            Assert.Equal(1, playerData.Inventory[0].Quantity);
            Assert.Same(item, playerData.Inventory[0].Item);
        }

        [Fact]
        public void TryRemoveItem_WhenQuantityIsInsufficient_DoesNotChangeInventory()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem();
            Assert.True(playerData.TryAddItem(item, 3));

            bool removed = playerData.TryRemoveItem(item.Id, 4);

            Assert.False(removed);
            Assert.Equal(3, playerData.GetItemQuantity(item.Id));
            Assert.Single(playerData.Inventory);
            Assert.Equal(3, playerData.Inventory[0].Quantity);
        }

        [Fact]
        public void TryRemoveItem_WhenStackBecomesEmpty_RemovesTheStack()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem();
            Assert.True(playerData.TryAddItem(item, 2));

            bool removed = playerData.TryRemoveItem(item.Id, 2);

            Assert.True(removed);
            Assert.Empty(playerData.Inventory);
            Assert.Equal(0, playerData.GetItemQuantity(item.Id));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TryAddItem_WhenQuantityIsNotPositive_FailsWithoutChangingInventory(int quantity)
        {
            PlayerData playerData = new();

            bool added = playerData.TryAddItem(CreateStackableItem(), quantity);

            Assert.False(added);
            Assert.Empty(playerData.Inventory);
        }

        [Fact]
        public void TryAddItem_WhenDefinitionIsInvalid_FailsWithoutChangingInventory()
        {
            PlayerData playerData = new();
            EquipmentItem invalidItem = new()
            {
                Id = "invalid_item",
                Name = "Invalid",
                Stackable = false,
                MaxStackSize = 10,
                Slot = EquipmentSlot.MainHand
            };

            bool added = playerData.TryAddItem(invalidItem, 1);

            Assert.False(added);
            Assert.Empty(playerData.Inventory);
        }

        [Fact]
        public void TryAddItem_WhenTotalQuantityWouldOverflow_FailsWithoutChangingInventory()
        {
            PlayerData playerData = new();
            ConsumableItem item = CreateStackableItem(int.MaxValue);
            Assert.True(playerData.TryAddItem(item, int.MaxValue));

            bool added = playerData.TryAddItem(item, 1);

            Assert.False(added);
            Assert.Equal(int.MaxValue, playerData.GetItemQuantity(item.Id));
            Assert.Single(playerData.Inventory);
            Assert.Equal(int.MaxValue, playerData.Inventory[0].Quantity);
        }

        [Fact]
        public void GetItemQuantity_IsCaseInsensitive()
        {
            PlayerData playerData = new();
            Assert.True(playerData.TryAddItem(CreateStackableItem(), 2));

            Assert.Equal(2, playerData.GetItemQuantity("HEALTH_POTION"));
        }

        [Fact]
        public void TryAddCurrency_WhenAmountIsPositive_IncreasesBalance()
        {
            PlayerData playerData = new(100);
            Assert.True(playerData.TryAddCurrency(50));
            Assert.Equal(150, playerData.Currency);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TryAddCurrency_WhenAmountIsNotPositive_FailsWithoutChangingBalance(int amount)
        {
            PlayerData playerData = new(100);
            Assert.False(playerData.TryAddCurrency(amount));
            Assert.Equal(100, playerData.Currency);
        }

        [Fact]
        public void TryAddCurrency_WhenBalanceWouldOverflow_FailsWithoutChangingBalance()
        {
            PlayerData playerData = new(int.MaxValue);
            Assert.False(playerData.TryAddCurrency(1));
            Assert.Equal(int.MaxValue, playerData.Currency);
        }

        [Fact]
        public void TrySpendCurrency_WhenFundsAreAvailable_DecreasesBalance()
        {
            PlayerData playerData = new(100);
            Assert.True(playerData.TrySpendCurrency(40));
            Assert.Equal(60, playerData.Currency);
        }

        [Fact]
        public void TrySpendCurrency_WhenFundsAreInsufficient_FailsWithoutChangingBalance()
        {
            PlayerData playerData = new(50);
            Assert.False(playerData.TrySpendCurrency(51));
            Assert.Equal(50, playerData.Currency);
        }

        [Fact]
        public void TrySpendCurrency_WhenDebtIsAllowed_CanMakeBalanceNegative()
        {
            PlayerData playerData = new(50);
            Assert.True(playerData.TrySpendCurrency(75, allowDebt: true));
            Assert.Equal(-25, playerData.Currency);
        }

        [Fact]
        public void TryPurchaseItem_WhenPurchaseIsValid_AddsItemAndDeductsCurrency()
        {
            PlayerData playerData = new(500);
            ConsumableItem item = CreateStackableItem();
            bool purchased = playerData.TryPurchaseItem(item, 100, 2, out string failureReason);
            Assert.True(purchased);
            Assert.Empty(failureReason);
            Assert.Equal(300, playerData.Currency);
            Assert.Equal(2, playerData.GetItemQuantity(item.Id));
        }

        [Fact]
        public void TryPurchaseItem_WhenFundsAreInsufficient_DoesNotChangePlayerData()
        {
            PlayerData playerData = new(100);
            ConsumableItem item = CreateStackableItem();
            bool purchased = playerData.TryPurchaseItem(item, 100, 2, out string failureReason);
            Assert.False(purchased);
            Assert.Equal("Bạn không đủ tiền.", failureReason);
            Assert.Equal(100, playerData.Currency);
            Assert.Equal(0, playerData.GetItemQuantity(item.Id));
        }

        [Fact]
        public void TryPurchaseItem_WhenInventoryRejectsItem_DoesNotDeductCurrency()
        {
            PlayerData playerData = new(500);
            EquipmentItem invalidEquipment = new()
            {
                Id = "invalid_equipment",
                Name = "Invalid",
                Stackable = true,
                MaxStackSize = 5,
                Slot = EquipmentSlot.MainHand
            };

            bool purchased = playerData.TryPurchaseItem(
                invalidEquipment,
                100,
                1,
                out string failureReason
            );

            Assert.False(purchased);
            Assert.Equal("Không thể thêm vật phẩm vào Inventory.", failureReason);
            Assert.Equal(500, playerData.Currency);
            Assert.False(playerData.HasItem(invalidEquipment.Id));
        }
    }
}

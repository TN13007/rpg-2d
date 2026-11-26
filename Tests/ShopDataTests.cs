using RPG2D.Features.PlayerData;
using RPG2D.Features.Shop;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class ShopDataTests
    {
        private static ItemDatabase CreateItemDatabase()
        {
            const string json = """
                {
                  "schemaVersion": 1,
                  "items": [
                    {
                      "id": "item_a",
                      "type": "Consumable",
                      "name": "Đồ A",
                      "description": "Vật phẩm A.",
                      "stackable": true,
                      "maxStackSize": 20
                    },
                    {
                      "id": "health_potion",
                      "type": "Consumable",
                      "name": "Bình máu",
                      "description": "Hồi máu.",
                      "stackable": true,
                      "maxStackSize": 2
                    },
                    {
                      "id": "ancient_key",
                      "type": "Quest",
                      "name": "Chìa khóa cổ",
                      "description": "Vật phẩm nhiệm vụ.",
                      "stackable": false,
                      "maxStackSize": 1
                    }
                  ]
                }
                """;

            return ItemDatabase.FromJson(json);
        }

        private static ShopDefinition CreateShop()
        {
            const string json = """
                {
                  "id": "test_shop",
                  "name": "Cửa hàng thử nghiệm",
                  "items": [
                    {
                      "itemId": "item_a",
                      "price": 100,
                      "stock": 5
                    }
                  ],
                  "buybackPrices": [
                    {
                      "itemId": "health_potion",
                      "price": 40
                    }
                  ]
                }
                """;

            return ShopDatabase.FromJson(json);
        }

        [Fact]
        public void ConfirmSelection_WhenPurchaseSucceeds_UpdatesPlayerDataAndStock()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            PlayerData playerData = new(500);
            ShopData shopData = new(CreateShop(), playerData, itemDatabase);

            shopData.ChangeQuantity(1);
            ShopDataOutcome outcome = shopData.ConfirmSelection();

            Assert.Equal(ShopDataOutcome.None, outcome);
            Assert.Equal(300, playerData.Currency);
            Assert.Equal(2, playerData.GetItemQuantity("item_a"));
            Assert.Equal(3, shopData.Items[0].Stock);
            Assert.Equal(ShopFeedbackKind.PurchaseSucceeded, shopData.Feedback.Kind);
        }

        [Fact]
        public void Items_WhenSelling_GroupsInventoryStacksByItemId()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            Item potion = GetItem(itemDatabase, "health_potion");
            PlayerData playerData = new();
            Assert.True(playerData.TryAddItem(potion, 3));

            ShopData shopData = new(CreateShop(), playerData, itemDatabase, ShopMode.Selling);
            Assert.Equal(ShopMode.Selling, shopData.Mode);


            Assert.Single(shopData.Items);
            Assert.Equal("health_potion", shopData.Items[0].ItemId);
            Assert.Equal(3, shopData.Items[0].OwnedQuantity);
            Assert.True(shopData.Items[0].IsAvailable);
        }

        [Fact]
        public void ConfirmSelection_WhenSaleSucceeds_RemovesSelectedQuantityAndAddsCurrency()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            Item potion = GetItem(itemDatabase, "health_potion");
            PlayerData playerData = new();
            Assert.True(playerData.TryAddItem(potion, 3));

            ShopData shopData = new(CreateShop(), playerData, itemDatabase, ShopMode.Selling);

            shopData.ChangeQuantity(1);
            ShopDataOutcome outcome = shopData.ConfirmSelection();

            Assert.Equal(ShopDataOutcome.None, outcome);
            Assert.Equal(80, playerData.Currency);
            Assert.Equal(1, playerData.GetItemQuantity("health_potion"));
            Assert.Equal(1, shopData.Items[0].OwnedQuantity);
            Assert.Equal(ShopFeedbackKind.SaleSucceeded, shopData.Feedback.Kind);
        }

        [Fact]
        public void ConfirmSelection_WhenItemIsNotAccepted_LeavesPlayerDataUnchanged()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            Item key = GetItem(itemDatabase, "ancient_key");
            PlayerData playerData = new(25);
            Assert.True(playerData.TryAddItem(key, 1));

            ShopData shopData = new(CreateShop(), playerData, itemDatabase, ShopMode.Selling);

            shopData.ConfirmSelection();

            Assert.Equal(25, playerData.Currency);
            Assert.Equal(1, playerData.GetItemQuantity("ancient_key"));
            Assert.Equal(ShopFeedbackKind.ItemNotAccepted, shopData.Feedback.Kind);
            Assert.False(shopData.Items[0].IsAvailable);
        }

        [Fact]
        public void ConfirmSelection_WhenExitOptionIsSelected_ReturnsLeaveRequest()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            PlayerData playerData = new();
            ShopData shopData = new(CreateShop(), playerData, itemDatabase);

            shopData.MoveSelection(1);

            Assert.Equal(ShopDataOutcome.LeaveRequested, shopData.ConfirmSelection());
        }

        [Fact]
        public void ConfirmSelection_WhenPurchaseLeavesLessStock_ClampsQuantityForNextPurchase()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            PlayerData playerData = new(1000);
            ShopData shopData = new(CreateShop(), playerData, itemDatabase);

            shopData.ChangeQuantity(1);
            shopData.ChangeQuantity(1);
            shopData.ChangeQuantity(1);

            Assert.Equal(4, shopData.SelectedQuantity);

            shopData.ConfirmSelection();

            Assert.Equal(1, shopData.Items[0].Stock);
            Assert.Equal(1, shopData.SelectedQuantity);
            Assert.Equal(4, shopData.Feedback.Quantity);

            shopData.ConfirmSelection();

            Assert.Equal(0, shopData.Items[0].Stock);
            Assert.Equal(5, playerData.GetItemQuantity("item_a"));
            Assert.Equal(500, playerData.Currency);
            Assert.Equal(1, shopData.Feedback.Quantity);
        }

        [Fact]
        public void ShopData_WhenBuying_ExposesCatalogAndPlayerData()
        {
            ItemDatabase itemDatabase = CreateItemDatabase();
            PlayerData playerData = new(500);
            ShopData shopData = new(CreateShop(), playerData, itemDatabase);


            Assert.Equal("Cửa hàng thử nghiệm", shopData.ShopName);
            Assert.Equal(ShopMode.Buying, shopData.Mode);
            Assert.Equal(500, shopData.Currency);
            Assert.Single(shopData.Items);
            Assert.Equal("item_a", shopData.Items[0].ItemId);
            Assert.Equal(5, shopData.Items[0].Stock);
            Assert.Equal(1, shopData.SelectedQuantity);
        }

        private static Item GetItem(ItemDatabase itemDatabase, string itemId)
        {
            Assert.True(itemDatabase.TryGetItem(itemId, out Item item));
            return item;
        }
    }
}

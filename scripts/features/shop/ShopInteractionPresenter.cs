using System.Collections.Generic;

namespace RPG2D.Features.Shop
{
    public sealed class ShopDetailsPresentation
    {
        public ShopDetailsPresentation(
            string name,
            string description,
            string price,
            string stock,
            string currency,
            string quantity,
            string feedback
        )
        {
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            Currency = currency;
            Quantity = quantity;
            Feedback = feedback;
        }

        public string Name { get; }
        public string Description { get; }
        public string Price { get; }
        public string Stock { get; }
        public string Currency { get; }
        public string Quantity { get; }
        public string Feedback { get; }
    }

    public sealed class ShopInteractionPresentation
    {
        public ShopInteractionPresentation(
            string title,
            string listHeading,
            IReadOnlyList<string> itemNames,
            int selectedIndex,
            bool isBuying,
            ShopDetailsPresentation details
        )
        {
            Title = title;
            ListHeading = listHeading;
            ItemNames = itemNames;
            SelectedIndex = selectedIndex;
            IsBuying = isBuying;
            Details = details;
        }

        public string Title { get; }
        public string ListHeading { get; }
        public IReadOnlyList<string> ItemNames { get; }
        public int SelectedIndex { get; }
        public bool IsBuying { get; }
        public ShopDetailsPresentation Details { get; }
    }

    public static class ShopInteractionPresenter
    {
        public static ShopInteractionPresentation Create(ShopSessionView session)
        {
            List<string> itemNames = new();

            foreach (ShopItemOption option in session.Items)
            {
                if (session.Mode == ShopMode.Buying)
                {
                    itemNames.Add(option.Item?.Name ?? $"Không rõ: {option.ItemId}");
                    continue;
                }

                string unavailable = option.IsAvailable ? string.Empty : " (không thu mua)";
                string itemName = option.Item?.Name ?? option.ItemId;
                itemNames.Add($"{itemName} ×{option.OwnedQuantity}{unavailable}");
            }

            itemNames.Add("Rời cửa hàng");

            return new ShopInteractionPresentation(
                session.ShopName,
                session.Mode == ShopMode.Buying ? "Hàng hóa" : "Vật phẩm đang sở hữu",
                itemNames.AsReadOnly(),
                session.SelectedIndex,
                session.Mode == ShopMode.Buying,
                CreateDetails(session)
            );
        }

        private static ShopDetailsPresentation CreateDetails(ShopSessionView session)
        {
            string currency = $"Tiền hiện có: {session.Currency:N0} vàng";
            string feedback = FormatFeedback(session.Feedback);

            if (session.Mode == ShopMode.Buying)
            {
                if (session.SelectedIndex == session.Items.Count)
                {
                    return new ShopDetailsPresentation(
                        "Rời cửa hàng",
                        "Quay lại danh sách lựa chọn ban đầu.",
                        string.Empty,
                        string.Empty,
                        currency,
                        string.Empty,
                        feedback
                    );
                }

                ShopItemOption? option = session.SelectedIndex >= 0 &&
                    session.SelectedIndex < session.Items.Count
                        ? session.Items[session.SelectedIndex]
                        : null;

                if (option?.Item == null)
                {
                    return new ShopDetailsPresentation(
                        "Không rõ vật phẩm",
                        option?.ItemId ?? string.Empty,
                        option == null ? string.Empty : $"Giá mua: {option.UnitPrice:N0}",
                        option == null ? string.Empty : $"Tồn kho: {option.Stock}",
                        currency,
                        option == null ? string.Empty : $"Số lượng mua: {session.SelectedQuantity}",
                        feedback
                    );
                }

                return new ShopDetailsPresentation(
                    option.Item.Name,
                    option.Item.Description,
                    $"Giá mua: {option.UnitPrice:N0} vàng / món",
                    $"Còn lại: {option.Stock}",
                    currency,
                    $"Số lượng mua: {session.SelectedQuantity}",
                    feedback
                );
            }

            if (session.InventoryIsEmpty)
            {
                return new ShopDetailsPresentation(
                    "Inventory trống",
                    "Bạn chưa có vật phẩm nào để bán.",
                    string.Empty,
                    string.Empty,
                    currency,
                    string.Empty,
                    feedback
                );
            }

            if (session.SelectedIndex == session.Items.Count)
            {
                return new ShopDetailsPresentation(
                    "Rời cửa hàng",
                    "Quay lại danh sách lựa chọn ban đầu.",
                    string.Empty,
                    $"Đang sở hữu: {session.Items.Count} loại vật phẩm",
                    currency,
                    string.Empty,
                    feedback
                );
            }

            ShopItemOption selectedOption = session.Items[session.SelectedIndex];
            string price = selectedOption.IsAvailable
                ? $"Giá thu mua: {selectedOption.UnitPrice:N0} vàng / món"
                : "Cửa hàng không thu mua vật phẩm này.";

            return new ShopDetailsPresentation(
                selectedOption.Item?.Name ?? selectedOption.ItemId,
                selectedOption.Item?.Description ?? string.Empty,
                price,
                $"Sở hữu: {selectedOption.OwnedQuantity}",
                currency,
                selectedOption.IsAvailable
                    ? $"Số lượng bán: {session.SelectedQuantity}"
                    : string.Empty,
                feedback
            );
        }

        private static string FormatFeedback(ShopFeedback feedback)
        {
            return feedback.Kind switch
            {
                ShopFeedbackKind.Error => feedback.Message,
                ShopFeedbackKind.ItemNotAccepted => "Cửa hàng không thu mua vật phẩm này.",
                ShopFeedbackKind.PurchaseSucceeded =>
                    $"Đã mua {feedback.ItemName} × {feedback.Quantity}.",
                ShopFeedbackKind.SaleSucceeded =>
                    $"Đã bán {feedback.ItemName} × {feedback.Quantity}, nhận {feedback.Amount:N0} vàng.",
                ShopFeedbackKind.CurrencyAdded =>
                    $"Đã cộng {feedback.Amount:N0} vàng để kiểm thử.",
                ShopFeedbackKind.CurrencyAddFailed =>
                    "Không thể cộng thêm vàng (vượt giới hạn).",
                ShopFeedbackKind.CurrencySpent =>
                    $"Đã trừ {feedback.Amount:N0} vàng để kiểm thử.",
                ShopFeedbackKind.CurrencySpendFailed =>
                    $"Không đủ vàng để trừ {feedback.Amount:N0}.",
                _ => string.Empty
            };
        }
    }
}
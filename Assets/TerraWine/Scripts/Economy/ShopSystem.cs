using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Economy
{
    public class ShopSystem : IGameSystem
    {
        private readonly List<ShopItemRuntimeDefinition> catalog = new List<ShopItemRuntimeDefinition>
        {
            new ShopItemRuntimeDefinition("shop_seed_merlot", "Merlot Seed", "A reliable red grape seed.", 20, ShopItemCategory.Seeds, "seed_merlot", 1, 1),
            new ShopItemRuntimeDefinition("shop_seed_muscat", "Muscat Seed", "A sweet white grape seed.", 18, ShopItemCategory.Seeds, "seed_muscat", 1, 1),
            new ShopItemRuntimeDefinition("shop_seed_fast_test", "Fast Test Seed", "MVP test seed pack using Merlot seed data.", 5, ShopItemCategory.Seeds, "seed_merlot", 1, 1),
            new ShopItemRuntimeDefinition("shop_barrel_basic_oak", "Basic Oak Barrel", "Adds one basic aging barrel.", 120, ShopItemCategory.Barrels, "barrel_basic_oak", 1, 1),
            new ShopItemRuntimeDefinition("shop_barrel_better_oak", "Better Oak Barrel", "Adds one stronger MVP aging barrel.", 220, ShopItemCategory.Barrels, "barrel_better_oak", 1, 1),
            new ShopItemRuntimeDefinition("shop_vineyard_plot", "Vineyard Expansion", "Adds one empty vineyard plot.", 150, ShopItemCategory.Vineyard, "vineyard_plot", 1, 1),
            new ShopItemRuntimeDefinition("shop_storage_upgrade", "Storage Upgrade", "Increases storage capacity.", 100, ShopItemCategory.Storage, "storage_capacity", 25, 1),
            new ShopItemRuntimeDefinition("shop_vault_upgrade_4", "Vault Upgrade 4 Digits", "Upgrades recipe vault security to 4 digits.", 180, ShopItemCategory.Vault, "vault_4_digits", 1, 1)
        };

        private GameSession session;
        private GameData data;

        public event Action ShopChanged;
        public event Action<string> MessageRaised;

        public IReadOnlyList<ShopItemRuntimeDefinition> Catalog => catalog;
        public string LastMessage { get; private set; } = string.Empty;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
        }

        public bool Buy(string itemId)
        {
            ShopItemRuntimeDefinition item = GetItem(itemId);
            if (item == null)
            {
                RaiseMessage("Shop item not found.");
                return false;
            }

            if (item.Price < 0 || item.Quantity <= 0)
            {
                RaiseMessage("This shop item is invalid.");
                return false;
            }

            if (data.calendar.currentYear < item.UnlockYear)
            {
                RaiseMessage($"{item.DisplayName} is not unlocked yet.");
                return false;
            }

            if (session.ResourceSystem.Get(ResourceType.Money) < item.Price)
            {
                RaiseMessage($"Not enough money for {item.DisplayName}.");
                return false;
            }

            if (!CanApplyPurchase(item))
            {
                return false;
            }

            if (!session.ResourceSystem.Spend(ResourceType.Money, item.Price))
            {
                RaiseMessage($"Not enough money for {item.DisplayName}.");
                return false;
            }

            ApplyPurchase(item);
            data.shop.purchases.Add(new ShopPurchaseData
            {
                itemId = item.ItemId,
                purchasedAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow),
                quantity = item.Quantity
            });

            RaiseMessage($"Bought {item.DisplayName}.");
            ShopChanged?.Invoke();
            return true;
        }

        public ShopItemRuntimeDefinition GetItem(string itemId)
        {
            return catalog.Find(item => item.ItemId == itemId);
        }

        private bool CanApplyPurchase(ShopItemRuntimeDefinition item)
        {
            switch (item.Category)
            {
                case ShopItemCategory.Seeds:
                    if (!session.StorageSystem.HasSpace(item.Quantity))
                    {
                        RaiseMessage("Storage is full. Free space before buying seeds.");
                        return false;
                    }

                    return true;
                case ShopItemCategory.Barrels:
                case ShopItemCategory.Vineyard:
                case ShopItemCategory.Storage:
                    return true;
                case ShopItemCategory.Vault:
                    if (data.winery.vaultDigits >= 4)
                    {
                        RaiseMessage("Vault is already upgraded to 4 digits or higher.");
                        return false;
                    }

                    return true;
                default:
                    RaiseMessage("Unsupported shop item category.");
                    return false;
            }
        }

        private void ApplyPurchase(ShopItemRuntimeDefinition item)
        {
            switch (item.Category)
            {
                case ShopItemCategory.Seeds:
                    session.InventorySystem.TryAddItem(item.TargetId, InventoryItemType.Seed, item.Quantity);
                    break;
                case ShopItemCategory.Barrels:
                    session.BarrelSystem.AddBarrel(item.TargetId);
                    break;
                case ShopItemCategory.Vineyard:
                    session.VineyardSystem.AddEmptyPlot();
                    break;
                case ShopItemCategory.Storage:
                    session.StorageSystem.IncreaseCapacity(item.Quantity);
                    data.storage.upgradeLevel++;
                    break;
                case ShopItemCategory.Vault:
                    session.VaultSystem.UpgradePlayerVault(4);
                    break;
            }
        }

        private void RaiseMessage(string message)
        {
            LastMessage = message;
            MessageRaised?.Invoke(message);
        }
    }

    public class ShopItemRuntimeDefinition
    {
        public ShopItemRuntimeDefinition(string itemId, string displayName, string description, int price, ShopItemCategory category, string targetId, int quantity, int unlockYear)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Description = description;
            Price = price;
            Category = category;
            TargetId = targetId;
            Quantity = quantity;
            UnlockYear = unlockYear;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Price { get; }
        public ShopItemCategory Category { get; }
        public string TargetId { get; }
        public int Quantity { get; }
        public int UnlockYear { get; }
    }
}

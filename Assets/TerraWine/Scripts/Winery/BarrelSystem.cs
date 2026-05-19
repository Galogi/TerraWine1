using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Winery
{
    public class BarrelSystem : IGameSystem
    {
        private readonly Dictionary<string, BarrelRuntimeDefinition> barrelDefinitions = new Dictionary<string, BarrelRuntimeDefinition>
        {
            { "barrel_basic_oak", new BarrelRuntimeDefinition("barrel_basic_oak", "Basic Oak Barrel", 45, 5, 10, 5, 1) },
            { "barrel_better_oak", new BarrelRuntimeDefinition("barrel_better_oak", "Better Oak Barrel", 60, 8, 18, 8, 1) },
            { "basic_oak_barrel", new BarrelRuntimeDefinition("barrel_basic_oak", "Basic Oak Barrel", 45, 5, 10, 5, 1) }
        };

        private GameSession session;
        private GameData data;

        public event Action BarrelsChanged;
        public event Action<string> MessageRaised;

        public IReadOnlyList<BarrelData> Barrels => data.winery.barrels;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            EnsureStartingBarrel();
            UpdateAging();
        }

        public bool StartAgingFirstAvailableWine(string barrelId)
        {
            WineBottleData bottle = FindFirstAvailableBottle();
            if (bottle == null)
            {
                RaiseMessage("No finished wine bottle is available for aging.");
                return false;
            }

            return StartAging(barrelId, GetBottleId(bottle));
        }

        public bool StartAging(string barrelId, string wineBottleId)
        {
            BarrelData barrel = FindBarrel(barrelId);
            if (barrel == null)
            {
                RaiseMessage("Barrel not found.");
                return false;
            }

            if (GetBarrelState(barrel) != BarrelState.Empty)
            {
                RaiseMessage("This barrel is already occupied.");
                return false;
            }

            WineBottleData bottle = FindBottle(wineBottleId);
            if (bottle == null)
            {
                RaiseMessage("Wine bottle not found.");
                return false;
            }

            if (bottle.isAged)
            {
                RaiseMessage("This bottle is already aged.");
                return false;
            }

            if (!session.InventorySystem.RemoveItem(bottle.itemId, 1))
            {
                RaiseMessage("This wine bottle is not in storage.");
                return false;
            }

            BarrelRuntimeDefinition definition = GetDefinition(barrel.barrelDefinitionId);
            DateTime now = session.TimeSystem.UtcNow;
            barrel.barrelDefinitionId = definition.Id;
            barrel.displayName = definition.DisplayName;
            barrel.wineBottleId = GetBottleId(bottle);
            barrel.agingStartedAtUtc = session.TimeSystem.ToSaveString(now);
            barrel.agingEndsAtUtc = session.TimeSystem.ToSaveString(now.AddSeconds(definition.AgingDurationSeconds));
            barrel.isAging = true;
            barrel.isReadyToCollect = false;

            RaiseMessage($"Started aging {GetBottleDisplayName(bottle)}.");
            BarrelsChanged?.Invoke();
            return true;
        }

        public bool CollectAgedWine(string barrelId)
        {
            UpdateAging();
            BarrelData barrel = FindBarrel(barrelId);
            if (barrel == null)
            {
                RaiseMessage("Barrel not found.");
                return false;
            }

            if (GetBarrelState(barrel) != BarrelState.Ready)
            {
                RaiseMessage("This barrel is not ready yet.");
                return false;
            }

            WineBottleData bottle = FindBottle(barrel.wineBottleId);
            if (bottle == null)
            {
                RaiseMessage("Aged wine bottle data is missing.");
                return false;
            }

            if (!session.StorageSystem.HasSpace(1))
            {
                RaiseMessage("Storage is full. Free space before collecting aged wine.");
                return false;
            }

            ApplyBarrelBonus(barrel, bottle);
            if (!session.InventorySystem.TryAddItem(bottle.itemId, InventoryItemType.WineBottle, 1))
            {
                RaiseMessage("Storage is full. Free space before collecting aged wine.");
                return false;
            }

            ClearBarrel(barrel);
            RaiseMessage($"Collected aged {GetBottleDisplayName(bottle)}.");
            BarrelsChanged?.Invoke();
            return true;
        }

        public void UpdateAging()
        {
            DateTime now = session.TimeSystem.UtcNow;
            bool changed = false;
            foreach (BarrelData barrel in data.winery.barrels)
            {
                if (!barrel.isAging || barrel.isReadyToCollect)
                {
                    continue;
                }

                if (session.TimeSystem.TryParseSaveTime(barrel.agingEndsAtUtc, out DateTime endsAt) && endsAt <= now)
                {
                    barrel.isReadyToCollect = true;
                    changed = true;
                }
            }

            if (changed)
            {
                BarrelsChanged?.Invoke();
            }
        }

        public BarrelState GetBarrelState(BarrelData barrel)
        {
            if (barrel == null || string.IsNullOrWhiteSpace(barrel.wineBottleId))
            {
                return BarrelState.Empty;
            }

            return barrel.isReadyToCollect ? BarrelState.Ready : BarrelState.Aging;
        }

        public TimeSpan GetTimeRemaining(BarrelData barrel)
        {
            if (barrel == null || barrel.isReadyToCollect || string.IsNullOrWhiteSpace(barrel.agingEndsAtUtc))
            {
                return TimeSpan.Zero;
            }

            if (!session.TimeSystem.TryParseSaveTime(barrel.agingEndsAtUtc, out DateTime endsAt))
            {
                return TimeSpan.Zero;
            }

            TimeSpan remaining = endsAt - session.TimeSystem.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }

        public BarrelRuntimeDefinition GetDefinition(string barrelDefinitionId)
        {
            if (!string.IsNullOrWhiteSpace(barrelDefinitionId) && barrelDefinitions.TryGetValue(barrelDefinitionId, out BarrelRuntimeDefinition definition))
            {
                return definition;
            }

            return barrelDefinitions["barrel_basic_oak"];
        }

        public BarrelData AddBarrel(string barrelDefinitionId)
        {
            BarrelRuntimeDefinition definition = GetDefinition(barrelDefinitionId);
            BarrelData barrel = new BarrelData
            {
                barrelId = $"barrel_{data.winery.barrels.Count + 1:000}",
                barrelDefinitionId = definition.Id,
                displayName = definition.DisplayName
            };
            data.winery.barrels.Add(barrel);
            BarrelsChanged?.Invoke();
            return barrel;
        }

        public WineBottleData GetBottleInside(BarrelData barrel)
        {
            return barrel == null ? null : FindBottle(barrel.wineBottleId);
        }

        private void EnsureStartingBarrel()
        {
            if (data.winery.barrels.Count == 0)
            {
                data.winery.barrels.Add(new BarrelData
                {
                    barrelId = "barrel_001",
                    barrelDefinitionId = "barrel_basic_oak",
                    displayName = "Basic Oak Barrel"
                });
                return;
            }

            foreach (BarrelData barrel in data.winery.barrels)
            {
                if (string.IsNullOrWhiteSpace(barrel.barrelDefinitionId) || barrel.barrelDefinitionId == "basic_oak_barrel")
                {
                    barrel.barrelDefinitionId = "barrel_basic_oak";
                }

                if (string.IsNullOrWhiteSpace(barrel.displayName))
                {
                    barrel.displayName = GetDefinition(barrel.barrelDefinitionId).DisplayName;
                }
            }
        }

        private WineBottleData FindFirstAvailableBottle()
        {
            foreach (WineBottleData bottle in data.wineBottles)
            {
                if (bottle.isAged || string.IsNullOrWhiteSpace(bottle.itemId) || session.InventorySystem.GetAmount(bottle.itemId) <= 0)
                {
                    continue;
                }

                return bottle;
            }

            return null;
        }

        private BarrelData FindBarrel(string barrelId)
        {
            return data.winery.barrels.Find(barrel => barrel.barrelId == barrelId);
        }

        private WineBottleData FindBottle(string wineBottleId)
        {
            return data.wineBottles.Find(bottle => GetBottleId(bottle) == wineBottleId);
        }

        private void ApplyBarrelBonus(BarrelData barrel, WineBottleData bottle)
        {
            if (bottle.agingBonusApplied)
            {
                return;
            }

            BarrelRuntimeDefinition definition = GetDefinition(barrel.barrelDefinitionId);
            int originalQuality = bottle.quality > 0 ? bottle.quality : bottle.qualityScore;
            int originalPrice = bottle.currentPrice > 0 ? bottle.currentPrice : bottle.salePrice;
            bottle.quality = Math.Min(100, originalQuality + definition.QualityBonus);
            bottle.qualityScore = bottle.quality;
            bottle.currentPrice = Math.Max(1, originalPrice + definition.PriceBonus);
            bottle.salePrice = bottle.currentPrice;
            bottle.competitionScore += definition.CompetitionScoreBonus;
            bottle.isAged = true;
            bottle.barrelDefinitionId = definition.Id;
            bottle.barrelIdUsed = barrel.barrelId;
            bottle.agingBonusApplied = true;
        }

        private static void ClearBarrel(BarrelData barrel)
        {
            barrel.wineBottleId = string.Empty;
            barrel.agingStartedAtUtc = string.Empty;
            barrel.agingEndsAtUtc = string.Empty;
            barrel.isAging = false;
            barrel.isReadyToCollect = false;
        }

        private static string GetBottleId(WineBottleData bottle)
        {
            if (bottle == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(bottle.wineBottleId))
            {
                return bottle.wineBottleId;
            }

            bottle.wineBottleId = bottle.bottleId;
            return bottle.wineBottleId;
        }

        private static string GetBottleDisplayName(WineBottleData bottle)
        {
            if (bottle == null)
            {
                return "wine";
            }

            return string.IsNullOrWhiteSpace(bottle.displayName) ? bottle.itemId : bottle.displayName;
        }

        private void RaiseMessage(string message)
        {
            MessageRaised?.Invoke(message);
        }
    }

    public class BarrelRuntimeDefinition
    {
        public BarrelRuntimeDefinition(string id, string displayName, int agingDurationSeconds, int qualityBonus, int priceBonus, int competitionScoreBonus, int slotCount)
        {
            Id = id;
            DisplayName = displayName;
            AgingDurationSeconds = agingDurationSeconds;
            QualityBonus = qualityBonus;
            PriceBonus = priceBonus;
            CompetitionScoreBonus = competitionScoreBonus;
            SlotCount = slotCount;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int AgingDurationSeconds { get; }
        public int QualityBonus { get; }
        public int PriceBonus { get; }
        public int CompetitionScoreBonus { get; }
        public int SlotCount { get; }
    }
}

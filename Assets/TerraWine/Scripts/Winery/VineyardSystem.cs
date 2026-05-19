using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Tutorial;

namespace TerraWine.Winery
{
    public class VineyardSystem : IGameSystem
    {
        private readonly Dictionary<string, SeedRuntimeDefinition> seedDefinitions = new Dictionary<string, SeedRuntimeDefinition>
        {
            { "seed_merlot", new SeedRuntimeDefinition("seed_merlot", "Merlot Seed", "grape_merlot", "Merlot Grapes", 300, 2, 2) },
            { "seed_muscat", new SeedRuntimeDefinition("seed_muscat", "Muscat Seed", "grape_muscat", "Muscat Grapes", 240, 1, 2) }
        };

        private GameSession session;
        private GameData data;

        public event Action VineyardChanged;
        public event Action<string> MessageRaised;

        public IReadOnlyList<VineyardPlotData> Plots => data.winery.vineyardPlots;
        public IReadOnlyCollection<SeedRuntimeDefinition> Seeds => seedDefinitions.Values;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            EnsureStartingPlots();
            UpdateGrowth();
        }

        public bool PlantSeed(string plotId, string seedId)
        {
            VineyardPlotData plot = FindPlot(plotId);
            if (plot == null || GetPlotState(plot) != VineyardPlotState.Empty)
            {
                RaiseMessage("Choose an empty vineyard plot.");
                return false;
            }

            if (!seedDefinitions.TryGetValue(seedId, out SeedRuntimeDefinition seed))
            {
                RaiseMessage("Unknown seed.");
                return false;
            }

            if (!session.InventorySystem.RemoveItem(seedId, 1))
            {
                RaiseMessage($"You do not own {seed.DisplayName}.");
                return false;
            }

            plot.plantedSeedId = seed.Id;
            plot.grapeId = seed.GrapeId;
            plot.plantedAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow);
            plot.wateredAtUtc = string.Empty;
            plot.readyAtUtc = string.Empty;
            plot.waterRequired = seed.WaterRequired;
            plot.harvestAmount = seed.HarvestAmount;
            plot.isWatered = false;
            plot.isReadyToHarvest = false;

            session.TutorialSystem?.CompleteStep(TutorialSystem.PlantSeedStepId);
            VineyardChanged?.Invoke();
            return true;
        }

        public bool PlantFirstOwnedSeed(string plotId)
        {
            string seedId = GetFirstOwnedSeedId();
            if (string.IsNullOrWhiteSpace(seedId))
            {
                RaiseMessage("You do not own any seeds.");
                return false;
            }

            return PlantSeed(plotId, seedId);
        }

        public bool WaterPlot(string plotId)
        {
            VineyardPlotData plot = FindPlot(plotId);
            if (plot == null || GetPlotState(plot) == VineyardPlotState.Empty)
            {
                RaiseMessage("Plant a seed before watering.");
                return false;
            }

            if (plot.isWatered)
            {
                RaiseMessage("This plot is already watered.");
                return false;
            }

            if (!session.ResourceSystem.Spend(ResourceType.Water, plot.waterRequired))
            {
                RaiseMessage($"Not enough water. This plot needs {plot.waterRequired} water.");
                return false;
            }

            DateTime now = session.TimeSystem.UtcNow;
            SeedRuntimeDefinition seed = GetSeed(plot.plantedSeedId);
            plot.isWatered = true;
            plot.wateredAtUtc = session.TimeSystem.ToSaveString(now);
            plot.readyAtUtc = session.TimeSystem.ToSaveString(now.AddSeconds(seed.GrowSeconds));

            session.TutorialSystem?.CompleteStep(TutorialSystem.WaterPlotStepId);
            UpdateGrowth(now);
            VineyardChanged?.Invoke();
            return true;
        }

        public bool HarvestPlot(string plotId)
        {
            VineyardPlotData plot = FindPlot(plotId);
            UpdateGrowth();

            if (plot == null || !plot.isReadyToHarvest)
            {
                RaiseMessage("This plot is not ready to harvest.");
                return false;
            }

            SeedRuntimeDefinition seed = GetSeed(plot.plantedSeedId);
            int harvestAmount = Math.Max(1, plot.harvestAmount);
            if (!session.InventorySystem.TryAddItem(plot.grapeId, InventoryItemType.Grape, harvestAmount))
            {
                RaiseMessage("Storage is full. Free space before harvesting.");
                return false;
            }

            ClearPlot(plot);

            session.TutorialSystem?.CompleteStep(TutorialSystem.HarvestGrapesStepId);
            RaiseMessage($"Harvested {seed.GrapeDisplayName}.");
            VineyardChanged?.Invoke();
            return true;
        }

        public void UpdateGrowth()
        {
            UpdateGrowth(session.TimeSystem.UtcNow);
        }

        public void UpdateGrowth(DateTime now)
        {
            bool changed = false;
            foreach (VineyardPlotData plot in data.winery.vineyardPlots)
            {
                if (!plot.isWatered || plot.isReadyToHarvest || string.IsNullOrWhiteSpace(plot.readyAtUtc))
                {
                    continue;
                }

                if (session.TimeSystem.TryParseSaveTime(plot.readyAtUtc, out DateTime readyAt) && readyAt <= now)
                {
                    plot.isReadyToHarvest = true;
                    changed = true;
                }
            }

            if (changed)
            {
                VineyardChanged?.Invoke();
            }
        }

        public VineyardPlotState GetPlotState(VineyardPlotData plot)
        {
            if (plot == null || string.IsNullOrWhiteSpace(plot.plantedSeedId))
            {
                return VineyardPlotState.Empty;
            }

            if (!plot.isWatered)
            {
                return VineyardPlotState.NeedsWater;
            }

            return plot.isReadyToHarvest ? VineyardPlotState.Ready : VineyardPlotState.Growing;
        }

        public TimeSpan GetTimeRemaining(VineyardPlotData plot)
        {
            if (plot == null || string.IsNullOrWhiteSpace(plot.readyAtUtc) || plot.isReadyToHarvest)
            {
                return TimeSpan.Zero;
            }

            if (!session.TimeSystem.TryParseSaveTime(plot.readyAtUtc, out DateTime readyAt))
            {
                return TimeSpan.Zero;
            }

            TimeSpan remaining = readyAt - session.TimeSystem.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }

        public SeedRuntimeDefinition GetSeed(string seedId)
        {
            return !string.IsNullOrWhiteSpace(seedId) && seedDefinitions.TryGetValue(seedId, out SeedRuntimeDefinition seed)
                ? seed
                : SeedRuntimeDefinition.Unknown(seedId);
        }

        public string GetFirstOwnedSeedId()
        {
            foreach (SeedRuntimeDefinition seed in seedDefinitions.Values)
            {
                if (session.InventorySystem.GetAmount(seed.Id) > 0)
                {
                    return seed.Id;
                }
            }

            return string.Empty;
        }

        private void EnsureStartingPlots()
        {
            if (data.winery.vineyardPlots.Count == 0)
            {
                data.winery.vineyardPlots.Add(new VineyardPlotData { plotId = "plot_001" });
            }
        }

        private VineyardPlotData FindPlot(string plotId)
        {
            return data.winery.vineyardPlots.Find(plot => plot.plotId == plotId);
        }

        private static void ClearPlot(VineyardPlotData plot)
        {
            plot.plantedSeedId = string.Empty;
            plot.grapeId = string.Empty;
            plot.plantedAtUtc = string.Empty;
            plot.wateredAtUtc = string.Empty;
            plot.readyAtUtc = string.Empty;
            plot.waterRequired = 0;
            plot.harvestAmount = 1;
            plot.isWatered = false;
            plot.isReadyToHarvest = false;
        }

        private void RaiseMessage(string message)
        {
            MessageRaised?.Invoke(message);
        }
    }

    public class SeedRuntimeDefinition
    {
        public SeedRuntimeDefinition(string id, string displayName, string grapeId, string grapeDisplayName, int growSeconds, int waterRequired, int harvestAmount)
        {
            Id = id;
            DisplayName = displayName;
            GrapeId = grapeId;
            GrapeDisplayName = grapeDisplayName;
            GrowSeconds = growSeconds;
            WaterRequired = waterRequired;
            HarvestAmount = harvestAmount;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string GrapeId { get; }
        public string GrapeDisplayName { get; }
        public int GrowSeconds { get; }
        public int WaterRequired { get; }
        public int HarvestAmount { get; }

        public static SeedRuntimeDefinition Unknown(string seedId)
        {
            string safeId = string.IsNullOrWhiteSpace(seedId) ? "unknown_seed" : seedId;
            return new SeedRuntimeDefinition(safeId, safeId, "unknown_grape", "Unknown Grapes", 300, 1, 1);
        }
    }
}

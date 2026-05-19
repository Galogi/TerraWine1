using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraWine.Data
{
    public enum ResourceType
    {
        Money,
        Water,
        Wood,
        Metal
    }

    public enum SeasonType
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public enum TimedTaskType
    {
        VineyardGrowth,
        WineProduction,
        BarrelAging,
        WorldMapGathering,
        RecipeRecovery,
        TemporaryModifier
    }

    public enum InventoryItemType
    {
        Seed,
        Grape,
        Resource,
        WineBottle,
        Recipe,
        SpecialItem
    }

    public enum VineyardPlotState
    {
        Empty,
        Planted,
        NeedsWater,
        Growing,
        Ready
    }

    public enum BarrelState
    {
        Empty,
        Aging,
        Ready
    }

    public enum ShopItemCategory
    {
        Seeds,
        Barrels,
        Vineyard,
        Storage,
        Vault
    }

    public enum WorldMapNodeType
    {
        WaterSource,
        Forest,
        MetalMine,
        Ruins
    }

    public enum WorldMapDistance
    {
        Near,
        Mid,
        Far
    }

    public enum WeatherType
    {
        Sunny,
        Rain
    }

    public enum TheftTargetType
    {
        Recipe,
        Bottle,
        Resources
    }

    public enum SaleModifierType
    {
        SalePenalty,
        SaleBonus
    }

    [Serializable]
    public class GameData
    {
        public string saveVersion = "0.1.0";
        public string createdAtUtc;
        public string lastSavedAtUtc;
        public PlayerData player = new PlayerData();
        public WineryData winery = new WineryData();
        public InventoryData inventory = new InventoryData();
        public StorageData storage = new StorageData();
        public ShopData shop = new ShopData();
        public ResourceData resources = new ResourceData();
        public WorldMapData worldMap = new WorldMapData();
        public WeatherData weather = new WeatherData();
        public CalendarData calendar = new CalendarData();
        public DailyActionData dailyActions = new DailyActionData();
        public List<TimedTaskData> timedTasks = new List<TimedTaskData>();
        public List<RecipeStateData> recipes = new List<RecipeStateData>();
        public List<WineBottleData> wineBottles = new List<WineBottleData>();
        public List<ReputationModifierData> reputationModifiers = new List<ReputationModifierData>();
        public List<CompetitionData> competitions = new List<CompetitionData>();
        public List<BotWineryData> botWineries = new List<BotWineryData>();
        public List<TheftHistoryData> theftHistory = new List<TheftHistoryData>();
        public List<TemporarySaleModifierData> temporarySaleModifiers = new List<TemporarySaleModifierData>();
        public List<DailyMissionData> dailyMissions = new List<DailyMissionData>();
        public TutorialProgressData tutorialProgress = new TutorialProgressData();
        public SeasonStateData seasonState = new SeasonStateData();
        public List<RandomEventData> randomEvents = new List<RandomEventData>();
        public List<AchievementProgressData> achievements = new List<AchievementProgressData>();
        public List<string> offlineNotifications = new List<string>();
    }

    [Serializable]
    public class PlayerData
    {
        public string playerId = Guid.NewGuid().ToString("N");
        public string playerName = "New Winemaker";
        public int reputation = 0;
    }

    [Serializable]
    public class WineryData
    {
        public string wineryId = Guid.NewGuid().ToString("N");
        public string wineryName = "TerraWine Winery";
        public bool isPlacedOnWorldMap;
        public Vector2Serializable worldMapPosition = new Vector2Serializable();
        public int vaultLevel = 1;
        public int vaultDigits = 3;
        public string vaultPassword = "123";
        public List<VineyardPlotData> vineyardPlots = new List<VineyardPlotData>();
        public List<BarrelData> barrels = new List<BarrelData>();
    }

    [Serializable]
    public class InventoryData
    {
        public List<InventoryStackData> stacks = new List<InventoryStackData>();
    }

    [Serializable]
    public class InventoryStackData
    {
        public string itemId;
        public InventoryItemType itemType;
        public int amount;
    }

    [Serializable]
    public class StorageData
    {
        public int usedCapacity;
        public int maxCapacity = 100;
        public int upgradeLevel;
    }

    [Serializable]
    public class ShopData
    {
        public List<ShopPurchaseData> purchases = new List<ShopPurchaseData>();
    }

    [Serializable]
    public class ShopPurchaseData
    {
        public string purchaseId = Guid.NewGuid().ToString("N");
        public string itemId;
        public string purchasedAtUtc;
        public int quantity;
    }

    [Serializable]
    public class ResourceData
    {
        public int money;
        public int water;
        public int wood;
        public int metal;
    }

    [Serializable]
    public class WorldMapData
    {
        public WineryLocationData wineryLocation = new WineryLocationData();
        public List<WorldMapNodeData> nodes = new List<WorldMapNodeData>();
        public List<string> ruinsRewardHistory = new List<string>();
    }

    [Serializable]
    public class WineryLocationData
    {
        public bool isPlaced;
        public string locationId = "starting_hill";
        public Vector2Serializable position = new Vector2Serializable();
    }

    [Serializable]
    public class WorldMapNodeData
    {
        public string nodeId;
        public bool isDiscovered = true;
        public int timesUsed;
        public string lastUsedAtUtc;
    }

    [Serializable]
    public class WeatherData
    {
        public WeatherType currentWeather = WeatherType.Sunny;
        public WeatherType todayForecast = WeatherType.Sunny;
        public WeatherType tomorrowForecast = WeatherType.Sunny;
        public string generatedAtUtc;
        public bool rainRewardAppliedToday;
        public string lastForecastMessage;
    }

    [Serializable]
    public class CalendarData
    {
        public int currentYear = 1;
        public int currentDay = 1;
        public int totalYears = 3;
    }

    [Serializable]
    public class DailyActionData
    {
        public int actionsRemaining = 10;
        public int maxActions = 10;
        public string lastRefreshAtUtc;
    }

    [Serializable]
    public class TimedTaskData
    {
        public string taskId = Guid.NewGuid().ToString("N");
        public TimedTaskType taskType;
        public string definitionId;
        public string ownerId;
        public string startedAtUtc;
        public string completesAtUtc;
        public int amount = 1;
        public bool isComplete;
    }

    [Serializable]
    public class VineyardPlotData
    {
        public string plotId = Guid.NewGuid().ToString("N");
        public string plantedSeedId;
        public string grapeId;
        public string plantedAtUtc;
        public string wateredAtUtc;
        public string readyAtUtc;
        public int waterRequired;
        public int harvestAmount = 1;
        public bool isWatered;
        public bool isReadyToHarvest;
    }

    [Serializable]
    public class BarrelData
    {
        public string barrelId = Guid.NewGuid().ToString("N");
        public string barrelDefinitionId;
        public string displayName;
        public string wineBottleId;
        public string agingStartedAtUtc;
        public string agingEndsAtUtc;
        public bool isAging;
        public bool isReadyToCollect;
    }

    [Serializable]
    public class RecipeStateData
    {
        public string recipeId;
        public bool isKnown;
        public bool isStolen;
        public string stolenUntilUtc;
    }

    [Serializable]
    public class WineBottleData
    {
        public string bottleId = Guid.NewGuid().ToString("N");
        public string wineBottleId;
        public string itemId;
        public string recipeId;
        public string displayName;
        public string barrelDefinitionId;
        public string producedAtUtc;
        public int quality;
        public int qualityScore;
        public int basePrice;
        public int currentPrice;
        public int competitionScore;
        public int salePrice;
        public bool isAged;
        public string barrelIdUsed;
        public bool agingBonusApplied;
    }

    [Serializable]
    public class ReputationModifierData
    {
        public string modifierId = Guid.NewGuid().ToString("N");
        public string source;
        public int reputationDelta;
        public float salePriceMultiplier = 1f;
        public string expiresAtUtc;
    }

    [Serializable]
    public class CompetitionData
    {
        public string competitionId;
        public int year;
        public bool isCompleted;
        public int playerScore;
        public int playerRank;
    }

    [Serializable]
    public class BotWineryData
    {
        public string botWineryId;
        public string definitionId;
        public string wineryName;
        public string displayName;
        public int reputation;
        public int vaultDigits = 3;
        public string vaultPassword;
        public List<string> ownedRecipeIds = new List<string>();
        public int storedBottleCount;
        public ResourceData storedResources = new ResourceData();
        public int defenseLevel;
        public string lastTheftResult;
        public List<string> stolenRecipeIds = new List<string>();
    }

    [Serializable]
    public class TheftHistoryData
    {
        public string theftId = Guid.NewGuid().ToString("N");
        public string attackerId;
        public string defenderId;
        public TheftTargetType targetType;
        public string targetId;
        public bool success;
        public string resultMessage;
        public string occurredAtUtc;
    }

    [Serializable]
    public class TemporarySaleModifierData
    {
        public string modifierId = Guid.NewGuid().ToString("N");
        public SaleModifierType type;
        public float percentModifier;
        public string expiresAtUtc;
        public string source;
        public string reason;
    }

    [Serializable]
    public class DailyMissionData
    {
        public string missionId;
        public int progress;
        public bool isCompleted;
        public bool rewardClaimed;
    }

    [Serializable]
    public class TutorialProgressData
    {
        public string currentStepId;
        public List<string> completedStepIds = new List<string>();
    }

    [Serializable]
    public class SeasonStateData
    {
        public SeasonType currentSeason = SeasonType.Spring;
        public string seasonDefinitionId = "spring";
    }

    [Serializable]
    public class RandomEventData
    {
        public string eventId;
        public string definitionId;
        public string startedAtUtc;
        public string endsAtUtc;
        public bool isActive;
    }

    [Serializable]
    public class AchievementProgressData
    {
        public string achievementId;
        public int progress;
        public bool isUnlocked;
    }

    [Serializable]
    public class Vector2Serializable
    {
        public float x;
        public float y;

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public void FromVector2(Vector2 value)
        {
            x = value.x;
            y = value.y;
        }
    }
}

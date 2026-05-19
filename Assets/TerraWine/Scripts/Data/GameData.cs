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
        public ResourceData resources = new ResourceData();
        public CalendarData calendar = new CalendarData();
        public DailyActionData dailyActions = new DailyActionData();
        public List<TimedTaskData> timedTasks = new List<TimedTaskData>();
        public List<RecipeStateData> recipes = new List<RecipeStateData>();
        public List<WineBottleData> wineBottles = new List<WineBottleData>();
        public List<ReputationModifierData> reputationModifiers = new List<ReputationModifierData>();
        public List<CompetitionData> competitions = new List<CompetitionData>();
        public List<BotWineryData> botWineries = new List<BotWineryData>();
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
    public class ResourceData
    {
        public int money;
        public int water;
        public int wood;
        public int metal;
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
        public int reputation;
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

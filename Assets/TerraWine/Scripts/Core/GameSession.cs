using System;
using TerraWine.Data;
using TerraWine.Economy;
using TerraWine.Inventory;
using TerraWine.Theft;
using TerraWine.Tutorial;
using TerraWine.WorldMap;
using TerraWine.Winery;
using UnityEngine;

namespace TerraWine.Core
{
    public class GameSession
    {
        public ISaveSystem SaveSystem { get; private set; }
        public ITimeSystem TimeSystem { get; private set; }
        public OfflineProgressSystem OfflineProgressSystem { get; private set; }
        public CalendarSystem CalendarSystem { get; private set; }
        public DailyActionSystem DailyActionSystem { get; private set; }
        public ResourceSystem ResourceSystem { get; private set; }
        public ShopSystem ShopSystem { get; private set; }
        public WeatherSystem WeatherSystem { get; private set; }
        public FortuneTellerSystem FortuneTellerSystem { get; private set; }
        public WorldMapSystem WorldMapSystem { get; private set; }
        public BotWinerySystem BotWinerySystem { get; private set; }
        public VaultSystem VaultSystem { get; private set; }
        public TheftSystem TheftSystem { get; private set; }
        public ReputationSystem ReputationSystem { get; private set; }
        public StorageSystem StorageSystem { get; private set; }
        public InventorySystem InventorySystem { get; private set; }
        public TutorialSystem TutorialSystem { get; private set; }
        public VineyardSystem VineyardSystem { get; private set; }
        public RecipeSystem RecipeSystem { get; private set; }
        public WineQualitySystem WineQualitySystem { get; private set; }
        public WineProductionSystem WineProductionSystem { get; private set; }
        public BarrelSystem BarrelSystem { get; private set; }
        public GameData Data { get; private set; }
        public bool IsLoaded { get; private set; }

        public event Action<GameData> GameLoaded;
        public event Action<GameData> GameSaved;

        public void Initialize()
        {
            SaveSystem = new SaveSystem();
            TimeSystem = new TimeSystem();
            Debug.Log("TerraWine GameSession initialized.");
        }

        public void StartNewGame()
        {
            Data = CreateInitialGameData();
            InitializeRuntimeSystems();
            IsLoaded = true;
            SaveGame();
            GameLoaded?.Invoke(Data);
        }

        public bool LoadGame()
        {
            GameData loadedData = SaveSystem.LoadGame();
            if (loadedData == null)
            {
                return false;
            }

            Data = loadedData;
            InitializeRuntimeSystems();
            IsLoaded = true;
            OfflineProgressSystem.ApplyOfflineProgress();
            SaveGame();
            GameLoaded?.Invoke(Data);
            return true;
        }

        public void SaveGame()
        {
            if (Data == null)
            {
                return;
            }

            Data.lastSavedAtUtc = TimeSystem.ToSaveString(TimeSystem.UtcNow);
            SaveSystem.SaveGame(Data);
            GameSaved?.Invoke(Data);
        }

        private void InitializeRuntimeSystems()
        {
            CalendarSystem = new CalendarSystem();
            DailyActionSystem = new DailyActionSystem();
            ResourceSystem = new ResourceSystem();
            ShopSystem = new ShopSystem();
            WeatherSystem = new WeatherSystem();
            FortuneTellerSystem = new FortuneTellerSystem();
            WorldMapSystem = new WorldMapSystem();
            BotWinerySystem = new BotWinerySystem();
            VaultSystem = new VaultSystem();
            ReputationSystem = new ReputationSystem();
            TheftSystem = new TheftSystem();
            StorageSystem = new StorageSystem();
            InventorySystem = new InventorySystem();
            TutorialSystem = new TutorialSystem();
            VineyardSystem = new VineyardSystem();
            RecipeSystem = new RecipeSystem();
            WineQualitySystem = new WineQualitySystem();
            WineProductionSystem = new WineProductionSystem();
            BarrelSystem = new BarrelSystem();
            OfflineProgressSystem = new OfflineProgressSystem();

            CalendarSystem.Initialize(this, Data);
            DailyActionSystem.Initialize(this, Data);
            ResourceSystem.Initialize(this, Data);
            ShopSystem.Initialize(this, Data);
            WeatherSystem.Initialize(this, Data);
            FortuneTellerSystem.Initialize(this, Data);
            WorldMapSystem.Initialize(this, Data);
            BotWinerySystem.Initialize(this, Data);
            VaultSystem.Initialize(this, Data);
            ReputationSystem.Initialize(this, Data);
            TheftSystem.Initialize(this, Data);
            StorageSystem.Initialize(this, Data);
            InventorySystem.Initialize(this, Data);
            TutorialSystem.Initialize(this, Data);
            VineyardSystem.Initialize(this, Data);
            RecipeSystem.Initialize(this, Data);
            WineQualitySystem.Initialize(this, Data);
            WineProductionSystem.Initialize(this, Data);
            BarrelSystem.Initialize(this, Data);
            OfflineProgressSystem.Initialize(this, Data);
        }

        private GameData CreateInitialGameData()
        {
            DateTime now = TimeSystem.UtcNow;
            GameData gameData = new GameData
            {
                createdAtUtc = TimeSystem.ToSaveString(now),
                lastSavedAtUtc = TimeSystem.ToSaveString(now)
            };

            gameData.resources.money = 500;
            gameData.resources.water = 30;
            gameData.resources.wood = 20;
            gameData.resources.metal = 10;
            gameData.dailyActions.actionsRemaining = 10;
            gameData.dailyActions.maxActions = 10;
            gameData.dailyActions.lastRefreshAtUtc = TimeSystem.ToSaveString(now);

            gameData.recipes.Add(new RecipeStateData { recipeId = "house_red", isKnown = true });
            gameData.recipes.Add(new RecipeStateData { recipeId = "sunny_semidry", isKnown = true });
            gameData.inventory.stacks.Add(new InventoryStackData { itemId = "seed_merlot", itemType = InventoryItemType.Seed, amount = 5 });
            gameData.inventory.stacks.Add(new InventoryStackData { itemId = "seed_muscat", itemType = InventoryItemType.Seed, amount = 3 });
            gameData.storage.usedCapacity = 8;
            gameData.winery.vineyardPlots.Add(new VineyardPlotData { plotId = "plot_001" });
            gameData.winery.barrels.Add(new BarrelData { barrelId = "barrel_001", barrelDefinitionId = "barrel_basic_oak", displayName = "Basic Oak Barrel" });
            gameData.competitions.Add(new CompetitionData { competitionId = "year_1_local_fair", year = 1 });
            gameData.competitions.Add(new CompetitionData { competitionId = "year_2_regional_cup", year = 2 });
            gameData.competitions.Add(new CompetitionData { competitionId = "year_3_final_championship", year = 3 });

            return gameData;
        }
    }
}

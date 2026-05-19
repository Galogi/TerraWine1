using System;
using System.Collections.Generic;
using System.Linq;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.WorldMap;
using TerraWine.Winery;
using UnityEngine;

namespace TerraWine.Testing
{
    public class TerraWineMVPIntegrationTestRunner : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = false;

        private int passed;
        private int failed;
        private GameSession session;

        private void Start()
        {
            if (runOnStart)
            {
                RunFullTest();
            }
        }

        public void RunFullTest()
        {
            passed = 0;
            failed = 0;
            session = GameBootstrap.Session;

            Info("TerraWine MVP Integration Test Started");
            if (!Require(session != null, "GameSession exists."))
            {
                PrintSummary();
                return;
            }

            try
            {
                TestSystemsExist();
                TestCoreLoop();
                TestVineyardLoop();
                TestWineProductionLoop();
                TestStorageRules();
                TestBarrelLoop();
                TestOfflineProgress();
                TestShop();
                TestWorldMap();
                TestDailyActions();
            }
            catch (Exception exception)
            {
                Fail($"Unexpected exception: {exception}");
            }

            PrintSummary();
        }

        private void TestSystemsExist()
        {
            Info("SYSTEM TEST");
            PassIf(session.SaveSystem != null, "SaveSystem exists.");
            PassIf(session.TimeSystem != null, "TimeSystem exists.");
            PassIf(session.OfflineProgressSystem != null, "OfflineProgressSystem exists.");
            PassIf(session.ResourceSystem != null, "ResourceSystem exists.");
            PassIf(session.ShopSystem != null, "ShopSystem exists.");
            PassIf(session.WorldMapSystem != null, "WorldMapSystem exists.");
            PassIf(session.WeatherSystem != null, "WeatherSystem exists.");
            PassIf(session.FortuneTellerSystem != null, "FortuneTellerSystem exists.");
            PassIf(session.InventorySystem != null, "InventorySystem exists.");
            PassIf(session.StorageSystem != null, "StorageSystem exists.");
            PassIf(session.VineyardSystem != null, "VineyardSystem exists.");
            PassIf(session.RecipeSystem != null, "RecipeSystem exists.");
            PassIf(session.WineProductionSystem != null, "WineProductionSystem exists.");
            PassIf(session.WineQualitySystem != null, "WineQualitySystem exists.");
            PassIf(session.BarrelSystem != null, "BarrelSystem exists.");
        }

        private void TestCoreLoop()
        {
            Info("CORE TEST");
            session.StartNewGame();
            PassIf(session.Data != null && session.IsLoaded, "New game starts and runtime data is loaded.");
            PassIf(session.ResourceSystem.Money > 0, "Starting money is greater than zero.");
            PassIf(session.ResourceSystem.Water > 0, "Starting water is greater than zero.");
            PassIf(session.ResourceSystem.Wood >= 0, "Starting wood is non-negative.");
            PassIf(session.ResourceSystem.Metal >= 0, "Starting metal is non-negative.");
            PassIf(session.DailyActionSystem.ActionsRemaining <= session.DailyActionSystem.MaxActions, "Daily actions are not above max.");
            PassIf(session.DailyActionSystem.MaxActions > 0, "Daily actions have a positive max.");

            session.SaveGame();
            int moneyBeforeLoad = session.Data.resources.money;
            string playerIdBeforeLoad = session.Data.player.playerId;
            bool loaded = session.LoadGame();
            PassIf(loaded, "Saved game loads successfully.");
            PassIf(session.Data.resources.money == moneyBeforeLoad, "Loaded data preserves money.");
            PassIf(session.Data.player.playerId == playerIdBeforeLoad, "Loaded data preserves player state.");
        }

        private void TestVineyardLoop()
        {
            Info("VINEYARD TEST");
            session.StartNewGame();
            VineyardPlotData plot = session.Data.winery.vineyardPlots.FirstOrDefault();
            string seedId = session.VineyardSystem.GetFirstOwnedSeedId();
            int seedBefore = session.InventorySystem.GetAmount(seedId);

            PassIf(plot != null, "Player has at least one vineyard plot.");
            PassIf(!string.IsNullOrWhiteSpace(seedId), "Player has at least one starting seed.");
            if (plot == null || string.IsNullOrWhiteSpace(seedId))
            {
                return;
            }

            PassIf(session.VineyardSystem.GetPlotState(plot) == VineyardPlotState.Empty, "First vineyard plot starts empty.");
            PassIf(session.VineyardSystem.PlantSeed(plot.plotId, seedId), "Planting a seed succeeds.");
            PassIf(session.InventorySystem.GetAmount(seedId) == seedBefore - 1, "Planting consumes one seed.");
            PassIf(session.VineyardSystem.GetPlotState(plot) == VineyardPlotState.NeedsWater, "Planted plot needs water.");

            int waterBefore = session.ResourceSystem.Water;
            PassIf(session.VineyardSystem.WaterPlot(plot.plotId), "Watering the plot succeeds.");
            PassIf(session.ResourceSystem.Water < waterBefore, "Watering consumes water.");
            PassIf(session.VineyardSystem.GetPlotState(plot) == VineyardPlotState.Growing, "Watered plot starts growing.");

            string grapeId = plot.grapeId;
            int grapesBefore = session.InventorySystem.GetAmount(grapeId);
            ForceVineyardReady(plot);
            session.VineyardSystem.UpdateGrowth();
            PassIf(session.VineyardSystem.GetPlotState(plot) == VineyardPlotState.Ready, "Fast-forwarded plot becomes ready.");
            PassIf(session.VineyardSystem.HarvestPlot(plot.plotId), "Harvesting ready grapes succeeds.");
            PassIf(session.InventorySystem.GetAmount(grapeId) > grapesBefore, "Harvest adds grapes to inventory/storage.");
            PassIf(session.VineyardSystem.GetPlotState(plot) == VineyardPlotState.Empty, "Harvested plot becomes empty.");
        }

        private void TestWineProductionLoop()
        {
            Info("WINE PRODUCTION TEST");
            WineRecipeRuntimeDefinition recipe = GetFirstOwnedAvailableRecipe();
            PassIf(recipe != null, "Player owns at least one active recipe.");
            if (recipe == null)
            {
                return;
            }

            RecipeStateData recipeState = session.RecipeSystem.GetState(recipe.Id);
            PassIf(recipeState != null && recipeState.isKnown && !recipeState.isStolen, "Recipe is known and not stolen.");
            EnsureIngredients(recipe);

            Dictionary<string, int> beforeAmounts = CaptureIngredientAmounts(recipe);
            int tasksBefore = session.WineProductionSystem.GetProductionTasks().Count;
            PassIf(session.WineProductionSystem.StartProduction(recipe.Id), "Wine production starts.");
            PassIf(session.WineProductionSystem.GetProductionTasks().Count == tasksBefore + 1, "Active production task exists.");
            PassIf(IngredientsWereConsumed(recipe, beforeAmounts), "Required grapes were consumed.");

            TimedTaskData task = session.WineProductionSystem.GetProductionTasks().LastOrDefault();
            ForceTaskComplete(task);
            session.WineProductionSystem.UpdateProduction();
            PassIf(task != null && task.isComplete, "Fast-forwarded wine production finishes.");

            int bottlesBefore = session.Data.wineBottles.Count;
            PassIf(task != null && session.WineProductionSystem.CollectFinishedWine(task.taskId), "Finished wine can be collected.");
            PassIf(session.Data.wineBottles.Count > bottlesBefore, "Collected wine creates WineBottleData.");

            WineBottleData bottle = session.Data.wineBottles.LastOrDefault();
            PassIf(bottle != null && !string.IsNullOrWhiteSpace(bottle.recipeId), "Wine bottle has recipeId.");
            PassIf(bottle != null && !string.IsNullOrWhiteSpace(bottle.displayName), "Wine bottle has displayName.");
            PassIf(bottle != null && bottle.quality > 0, "Wine bottle has quality.");
            PassIf(bottle != null && (bottle.basePrice > 0 || bottle.currentPrice > 0), "Wine bottle has base/current price.");
            PassIf(bottle != null && bottle.competitionScore > 0, "Wine bottle has competition score.");
        }

        private void TestStorageRules()
        {
            Info("STORAGE TEST");
            int usedBefore = session.StorageSystem.UsedCapacity;
            PassIf(session.StorageSystem.MaxCapacity > 0, "Storage has max capacity.");
            PassIf(session.StorageSystem.UsedCapacity <= session.StorageSystem.MaxCapacity, "Storage does not exceed max capacity.");

            string testItemId = "test_storage_probe";
            bool added = session.InventorySystem.TryAddItem(testItemId, InventoryItemType.SpecialItem, 1);
            PassIf(!added || session.StorageSystem.UsedCapacity == usedBefore + 1, "Used capacity updates when an item is added.");
            if (added)
            {
                session.InventorySystem.RemoveItem(testItemId, 1);
            }

            int originalMax = session.Data.storage.maxCapacity;
            int originalUsed = session.Data.storage.usedCapacity;
            int itemBefore = session.InventorySystem.GetAmount(testItemId);
            session.Data.storage.maxCapacity = session.Data.storage.usedCapacity;
            bool fullAdd = session.InventorySystem.TryAddItem(testItemId, InventoryItemType.SpecialItem, 1);
            PassIf(!fullAdd, "Storage rejects new items when full.");
            PassIf(session.InventorySystem.GetAmount(testItemId) == itemBefore, "Rejected full-storage item is not added/lost.");
            session.Data.storage.maxCapacity = originalMax;
            session.Data.storage.usedCapacity = originalUsed;
        }

        private void TestBarrelLoop()
        {
            Info("BARREL TEST");
            BarrelData barrel = session.Data.winery.barrels.FirstOrDefault();
            PassIf(barrel != null, "Player has at least one barrel.");
            WineBottleData bottle = GetFirstAvailableUnagedBottle();
            if (bottle == null)
            {
                bottle = CreateDebugBottle("barrel_test_bottle");
            }

            PassIf(bottle != null && session.InventorySystem.GetAmount(bottle.itemId) > 0, "A finished wine bottle is available.");
            if (barrel == null || bottle == null)
            {
                return;
            }

            int qualityBefore = bottle.quality;
            int priceBefore = bottle.currentPrice;
            int scoreBefore = bottle.competitionScore;
            int bottleAmountBefore = session.InventorySystem.GetAmount(bottle.itemId);

            PassIf(session.BarrelSystem.StartAging(barrel.barrelId, bottle.wineBottleId), "Barrel aging starts.");
            PassIf(session.InventorySystem.GetAmount(bottle.itemId) == bottleAmountBefore - 1, "Wine bottle is removed from available storage while aging.");
            PassIf(session.BarrelSystem.GetBarrelState(barrel) == BarrelState.Aging, "Barrel state becomes Aging.");

            ForceBarrelReady(barrel);
            session.BarrelSystem.UpdateAging();
            PassIf(session.BarrelSystem.GetBarrelState(barrel) == BarrelState.Ready, "Fast-forwarded barrel becomes Ready.");
            PassIf(session.BarrelSystem.CollectAgedWine(barrel.barrelId), "Aged wine can be collected.");
            PassIf(session.InventorySystem.GetAmount(bottle.itemId) >= bottleAmountBefore, "Aged wine returns to storage.");
            PassIf(bottle.isAged, "Aged wine is marked isAged.");
            PassIf(!string.IsNullOrWhiteSpace(bottle.barrelIdUsed), "Aged wine stores barrelIdUsed.");
            PassIf(bottle.quality > qualityBefore, "Aged wine quality increased.");
            PassIf(bottle.currentPrice > priceBefore, "Aged wine current price increased.");
            PassIf(bottle.competitionScore > scoreBefore, "Aged wine competition score increased.");
        }

        private void TestOfflineProgress()
        {
            Info("OFFLINE PROGRESS TEST");
            session.StartNewGame();

            VineyardPlotData plot = session.Data.winery.vineyardPlots.First();
            string seedId = session.VineyardSystem.GetFirstOwnedSeedId();
            session.VineyardSystem.PlantSeed(plot.plotId, seedId);
            session.VineyardSystem.WaterPlot(plot.plotId);

            WineRecipeRuntimeDefinition recipe = GetFirstOwnedAvailableRecipe();
            EnsureIngredients(recipe);
            session.WineProductionSystem.StartProduction(recipe.Id);
            TimedTaskData productionTask = session.WineProductionSystem.GetProductionTasks().LastOrDefault();

            WineBottleData bottle = CreateDebugBottle("offline_barrel_test_bottle");
            BarrelData barrel = session.Data.winery.barrels.First();
            session.BarrelSystem.StartAging(barrel.barrelId, bottle.wineBottleId);

            ForceVineyardReady(plot);
            ForceTaskComplete(productionTask);
            ForceBarrelReady(barrel);
            session.Data.lastSavedAtUtc = session.TimeSystem.ToSaveString(DateTime.UtcNow.AddHours(-2));
            session.OfflineProgressSystem.ApplyOfflineProgress();

            PassIf(session.VineyardSystem.GetPlotState(plot) == VineyardPlotState.Ready, "Offline progress marks vineyard ready.");
            PassIf(productionTask != null && productionTask.isComplete, "Offline progress marks wine production finished.");
            PassIf(session.BarrelSystem.GetBarrelState(barrel) == BarrelState.Ready, "Offline progress marks barrel ready.");
            PassIf(session.Data.winery.vineyardPlots.Count > 0 && session.Data.winery.barrels.Count > 0, "Offline progress keeps data intact.");
        }

        private void TestDailyActions()
        {
            Info("DAILY ACTION TEST");
            session.StartNewGame();
            int max = session.DailyActionSystem.MaxActions;
            PassIf(max > 0, "DailyActionSystem has max actions.");
            session.DailyActionSystem.SpendAction(3);
            PassIf(session.DailyActionSystem.ActionsRemaining == max - 3, "Daily actions can be spent for refresh test.");

            DateTime yesterdayBeforeRefresh = DateTime.Now.Date.AddDays(-1).AddHours(7).ToUniversalTime();
            session.Data.dailyActions.lastRefreshAtUtc = session.TimeSystem.ToSaveString(yesterdayBeforeRefresh);
            bool refreshed = session.DailyActionSystem.RefreshIfNeeded();
            PassIf(refreshed, "Daily actions refresh after crossing 08:00.");
            PassIf(session.DailyActionSystem.ActionsRemaining == max, "Daily actions refresh back to max.");
            PassIf(session.DailyActionSystem.ActionsRemaining <= max, "Daily actions never exceed max.");
        }

        private void TestShop()
        {
            Info("SHOP TEST");
            session.StartNewGame();
            PassIf(session.ShopSystem != null, "ShopSystem exists.");
            PassIf(session.ShopSystem.Catalog.Count > 0, "Shop catalog has items.");

            int moneyBeforeSeed = session.ResourceSystem.Money;
            int merlotBefore = session.InventorySystem.GetAmount("seed_merlot");
            PassIf(session.ShopSystem.Buy("shop_seed_merlot"), "Buying a seed succeeds.");
            PassIf(session.ResourceSystem.Money < moneyBeforeSeed, "Buying a seed decreases money.");
            PassIf(session.InventorySystem.GetAmount("seed_merlot") == merlotBefore + 1, "Buying a seed increases seed count.");

            session.Data.resources.money = 0;
            int moneyBeforeFail = session.ResourceSystem.Money;
            bool expensiveBought = session.ShopSystem.Buy("shop_barrel_better_oak");
            PassIf(!expensiveBought, "Expensive purchase fails without enough money.");
            PassIf(session.ResourceSystem.Money == moneyBeforeFail && session.ResourceSystem.Money >= 0, "Failed purchase does not make money negative.");

            session.Data.resources.money = 1000;
            int plotsBefore = session.Data.winery.vineyardPlots.Count;
            PassIf(session.ShopSystem.Buy("shop_vineyard_plot"), "Buying vineyard expansion succeeds.");
            PassIf(session.Data.winery.vineyardPlots.Count == plotsBefore + 1, "Vineyard plot count increases.");

            int maxCapacityBefore = session.StorageSystem.MaxCapacity;
            PassIf(session.ShopSystem.Buy("shop_storage_upgrade"), "Buying storage upgrade succeeds.");
            PassIf(session.StorageSystem.MaxCapacity > maxCapacityBefore, "Storage max capacity increases.");

            int barrelsBefore = session.Data.winery.barrels.Count;
            PassIf(session.ShopSystem.Buy("shop_barrel_basic_oak"), "Buying a barrel succeeds.");
            PassIf(session.Data.winery.barrels.Count == barrelsBefore + 1, "Barrel count increases.");
        }

        private void TestWorldMap()
        {
            Info("WORLD MAP TEST");
            session.StartNewGame();
            PassIf(session.WorldMapSystem != null, "WorldMapSystem exists.");
            PassIf(session.DailyActionSystem.ActionsRemaining > 0, "Player has daily world actions.");
            PassIf(session.WorldMapSystem.GetNode("water_source_near") != null, "World map catalog has water source.");
            PassIf(session.WorldMapSystem.GetNode("forest_near") != null, "World map catalog has forest.");
            PassIf(session.WorldMapSystem.GetNode("metal_mine_mid") != null, "World map catalog has metal mine.");
            PassIf(session.WorldMapSystem.GetNode("old_ruins") != null, "World map catalog has ruins.");

            int actionsBeforeWood = session.DailyActionSystem.ActionsRemaining;
            int woodBefore = session.ResourceSystem.Wood;
            PassIf(session.WorldMapSystem.StartNodeAction("forest_near"), "Gathering wood succeeds.");
            PassIf(session.DailyActionSystem.ActionsRemaining == actionsBeforeWood - 1, "Wood gathering consumes one daily action.");
            PassIf(session.ResourceSystem.Wood > woodBefore, "Wood gathering increases wood.");

            int actionsBeforeMetal = session.DailyActionSystem.ActionsRemaining;
            int metalBefore = session.ResourceSystem.Metal;
            PassIf(session.WorldMapSystem.StartNodeAction("metal_mine_mid"), "Gathering metal succeeds.");
            PassIf(session.DailyActionSystem.ActionsRemaining == actionsBeforeMetal - 1, "Metal gathering consumes one daily action.");
            PassIf(session.ResourceSystem.Metal > metalBefore, "Metal gathering increases metal.");

            int actionsBeforeWater = session.DailyActionSystem.ActionsRemaining;
            int waterBefore = session.ResourceSystem.Water;
            PassIf(session.WorldMapSystem.StartNodeAction("water_source_near"), "Water gathering starts.");
            PassIf(session.DailyActionSystem.ActionsRemaining == actionsBeforeWater - 1, "Water gathering consumes one daily action.");
            TimedTaskData waterTask = session.WorldMapSystem.GetWorldTasks().Find(task => task.definitionId == "water_source_near");
            ForceTaskComplete(waterTask);
            session.WorldMapSystem.UpdateWorldTasks();
            PassIf(waterTask != null && waterTask.isComplete, "Water gathering task becomes complete.");
            PassIf(waterTask != null && session.WorldMapSystem.CollectCompletedWorldTask(waterTask.taskId), "Completed water gathering can be collected.");
            PassIf(session.ResourceSystem.Water > waterBefore, "Water gathering increases water after collection.");

            int actionsBeforeRuins = session.DailyActionSystem.ActionsRemaining;
            int rewardHistoryBefore = session.Data.worldMap.ruinsRewardHistory.Count;
            int moneyBeforeRuins = session.ResourceSystem.Money;
            int woodBeforeRuins = session.ResourceSystem.Wood;
            int metalBeforeRuins = session.ResourceSystem.Metal;
            int waterBeforeRuins = session.ResourceSystem.Water;
            int relicBefore = session.InventorySystem.GetAmount("rare_ruins_relic");
            PassIf(session.WorldMapSystem.StartNodeAction("old_ruins"), "Ruins excavation succeeds.");
            PassIf(session.DailyActionSystem.ActionsRemaining == actionsBeforeRuins - 1, "Ruins excavation consumes one daily action.");
            bool gotRuinsReward = session.Data.worldMap.ruinsRewardHistory.Count > rewardHistoryBefore
                || session.ResourceSystem.Money > moneyBeforeRuins
                || session.ResourceSystem.Wood > woodBeforeRuins
                || session.ResourceSystem.Metal > metalBeforeRuins
                || session.ResourceSystem.Water > waterBeforeRuins
                || session.InventorySystem.GetAmount("rare_ruins_relic") > relicBefore;
            PassIf(gotRuinsReward, "Ruins excavation grants one valid reward.");

            while (session.DailyActionSystem.ActionsRemaining > 0)
            {
                session.DailyActionSystem.SpendAction();
            }

            int woodBeforeFail = session.ResourceSystem.Wood;
            PassIf(!session.WorldMapSystem.StartNodeAction("forest_near"), "World map action fails when daily actions are zero.");
            PassIf(session.ResourceSystem.Wood == woodBeforeFail, "Failed world map action does not increase resources.");

            DateTime yesterdayBeforeRefresh = DateTime.Now.Date.AddDays(-1).AddHours(7).ToUniversalTime();
            session.Data.dailyActions.lastRefreshAtUtc = session.TimeSystem.ToSaveString(yesterdayBeforeRefresh);
            PassIf(session.DailyActionSystem.RefreshIfNeeded(), "Daily action refresh can be simulated across 08:00.");
            PassIf(session.DailyActionSystem.ActionsRemaining == session.DailyActionSystem.MaxActions, "Daily actions return to max after refresh.");

            int waterBeforeRain = session.ResourceSystem.Water;
            session.WeatherSystem.ForceWeather(WeatherType.Rain);
            PassIf(session.WeatherSystem.CurrentWeather == WeatherType.Rain, "WeatherSystem can force rain for test.");
            PassIf(session.ResourceSystem.Water > waterBeforeRain || session.Data.weather.rainRewardAppliedToday, "Rain is saved and applies water reward once.");

            int moneyBeforeForecast = session.ResourceSystem.Money;
            PassIf(session.FortuneTellerSystem.AskForRainForecast(), "Fortune teller forecast succeeds with enough money.");
            PassIf(session.ResourceSystem.Money < moneyBeforeForecast, "Fortune teller forecast costs money.");
            PassIf(!string.IsNullOrWhiteSpace(session.Data.weather.lastForecastMessage), "Fortune teller stores forecast message.");

            session.Data.resources.money = 0;
            PassIf(!session.FortuneTellerSystem.AskForRainForecast(), "Fortune teller fails safely without enough money.");
            PassIf(session.ResourceSystem.Money >= 0, "Failed fortune teller request does not make money negative.");
        }

        private WineRecipeRuntimeDefinition GetFirstOwnedAvailableRecipe()
        {
            foreach (WineRecipeRuntimeDefinition recipe in session.RecipeSystem.Recipes)
            {
                if (session.RecipeSystem.IsOwned(recipe.Id) && session.RecipeSystem.IsAvailable(recipe.Id))
                {
                    return recipe;
                }
            }

            return null;
        }

        private void EnsureIngredients(WineRecipeRuntimeDefinition recipe)
        {
            if (recipe == null)
            {
                return;
            }

            foreach (RecipeIngredient ingredient in recipe.RequiredGrapes)
            {
                int missing = ingredient.Amount - session.InventorySystem.GetAmount(ingredient.ItemId);
                if (missing > 0)
                {
                    // Test-only setup: create grapes so production can be verified without relying on manual play.
                    session.InventorySystem.TryAddItem(ingredient.ItemId, InventoryItemType.Grape, missing);
                }
            }
        }

        private Dictionary<string, int> CaptureIngredientAmounts(WineRecipeRuntimeDefinition recipe)
        {
            Dictionary<string, int> amounts = new Dictionary<string, int>();
            foreach (RecipeIngredient ingredient in recipe.RequiredGrapes)
            {
                amounts[ingredient.ItemId] = session.InventorySystem.GetAmount(ingredient.ItemId);
            }

            return amounts;
        }

        private bool IngredientsWereConsumed(WineRecipeRuntimeDefinition recipe, Dictionary<string, int> beforeAmounts)
        {
            foreach (RecipeIngredient ingredient in recipe.RequiredGrapes)
            {
                int before = beforeAmounts.TryGetValue(ingredient.ItemId, out int amount) ? amount : 0;
                if (session.InventorySystem.GetAmount(ingredient.ItemId) != before - ingredient.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        private WineBottleData GetFirstAvailableUnagedBottle()
        {
            return session.Data.wineBottles.FirstOrDefault(bottle =>
                bottle != null &&
                !bottle.isAged &&
                !string.IsNullOrWhiteSpace(bottle.itemId) &&
                session.InventorySystem.GetAmount(bottle.itemId) > 0);
        }

        private WineBottleData CreateDebugBottle(string itemId)
        {
            WineRecipeRuntimeDefinition recipe = GetFirstOwnedAvailableRecipe();
            if (recipe == null)
            {
                return null;
            }

            WineBottleData bottle = new WineBottleData
            {
                itemId = itemId,
                recipeId = recipe.Id,
                displayName = "Integration Test Wine Bottle",
                producedAtUtc = session.TimeSystem.ToSaveString(DateTime.UtcNow),
                quality = 50,
                qualityScore = 50,
                basePrice = recipe.BasePrice,
                currentPrice = recipe.BasePrice,
                competitionScore = 50,
                salePrice = recipe.BasePrice,
                isAged = false
            };
            bottle.wineBottleId = bottle.bottleId;

            // Test-only setup: create a bottle when the barrel test needs an isolated input item.
            if (!session.InventorySystem.TryAddItem(itemId, InventoryItemType.WineBottle, 1))
            {
                return null;
            }

            session.Data.wineBottles.Add(bottle);
            return bottle;
        }

        private void ForceVineyardReady(VineyardPlotData plot)
        {
            if (plot == null)
            {
                return;
            }

            plot.readyAtUtc = session.TimeSystem.ToSaveString(DateTime.UtcNow.AddSeconds(-1));
        }

        private void ForceTaskComplete(TimedTaskData task)
        {
            if (task == null)
            {
                return;
            }

            task.completesAtUtc = session.TimeSystem.ToSaveString(DateTime.UtcNow.AddSeconds(-1));
        }

        private void ForceBarrelReady(BarrelData barrel)
        {
            if (barrel == null)
            {
                return;
            }

            barrel.agingEndsAtUtc = session.TimeSystem.ToSaveString(DateTime.UtcNow.AddSeconds(-1));
        }

        private bool Require(bool condition, string message)
        {
            PassIf(condition, message);
            return condition;
        }

        private void PassIf(bool condition, string message)
        {
            if (condition)
            {
                Pass(message);
            }
            else
            {
                Fail(message);
            }
        }

        private void Pass(string message)
        {
            passed++;
            Debug.Log($"[PASS] {message}");
        }

        private void Fail(string message)
        {
            failed++;
            Debug.LogError($"[FAIL] {message}");
        }

        private void Info(string message)
        {
            Debug.Log($"[INFO] {message}");
        }

        private void PrintSummary()
        {
            Debug.Log("[INFO] TerraWine MVP Integration Test Finished");
            Debug.Log($"[INFO] Passed: {passed}");
            if (failed > 0)
            {
                Debug.LogError($"[FAIL] Failed: {failed}");
            }
            else
            {
                Debug.Log("[PASS] Failed: 0");
            }
        }
    }
}

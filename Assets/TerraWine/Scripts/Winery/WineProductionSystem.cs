using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Winery
{
    public class WineProductionSystem : IGameSystem
    {
        private GameSession session;
        private GameData data;

        public event Action ProductionChanged;
        public event Action<string> MessageRaised;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            UpdateProduction();
        }

        public IReadOnlyList<TimedTaskData> GetProductionTasks()
        {
            return data.timedTasks.FindAll(task => task.taskType == TimedTaskType.WineProduction);
        }

        public bool StartProduction(string recipeId)
        {
            WineRecipeRuntimeDefinition recipe = session.RecipeSystem.GetRecipe(recipeId);
            if (recipe == null)
            {
                RaiseMessage("Unknown recipe.");
                return false;
            }

            if (!session.RecipeSystem.IsOwned(recipeId))
            {
                RaiseMessage("You do not own this recipe.");
                return false;
            }

            if (!session.RecipeSystem.IsAvailable(recipeId))
            {
                RaiseMessage("This recipe is unavailable.");
                return false;
            }

            foreach (RecipeIngredient ingredient in recipe.RequiredGrapes)
            {
                if (session.InventorySystem.GetAmount(ingredient.ItemId) < ingredient.Amount)
                {
                    RaiseMessage($"Missing {ingredient.Amount} {ingredient.DisplayName}.");
                    return false;
                }
            }

            foreach (RecipeIngredient ingredient in recipe.RequiredGrapes)
            {
                session.InventorySystem.RemoveItem(ingredient.ItemId, ingredient.Amount);
            }

            DateTime now = session.TimeSystem.UtcNow;
            data.timedTasks.Add(new TimedTaskData
            {
                taskType = TimedTaskType.WineProduction,
                definitionId = recipe.Id,
                ownerId = data.player.playerId,
                startedAtUtc = session.TimeSystem.ToSaveString(now),
                completesAtUtc = session.TimeSystem.ToSaveString(now.AddSeconds(recipe.ProductionSeconds)),
                amount = 1,
                isComplete = false
            });

            RaiseMessage($"Started {recipe.DisplayName} production.");
            ProductionChanged?.Invoke();
            return true;
        }

        public bool CollectFinishedWine(string taskId)
        {
            UpdateProduction();
            TimedTaskData task = data.timedTasks.Find(item => item.taskId == taskId && item.taskType == TimedTaskType.WineProduction);
            if (task == null)
            {
                RaiseMessage("Production task not found.");
                return false;
            }

            if (!task.isComplete)
            {
                RaiseMessage("Wine production is still in progress.");
                return false;
            }

            WineRecipeRuntimeDefinition recipe = session.RecipeSystem.GetRecipe(task.definitionId);
            if (recipe == null)
            {
                RaiseMessage("Recipe data is missing.");
                return false;
            }

            if (!session.StorageSystem.HasSpace(task.amount))
            {
                RaiseMessage("Storage is full. Free space before collecting wine.");
                return false;
            }

            int quality = session.WineQualitySystem.CalculateQuality(recipe, true);
            string bottleItemId = $"{recipe.Id}_bottle";
            WineBottleData bottle = new WineBottleData
            {
                itemId = bottleItemId,
                recipeId = recipe.Id,
                displayName = $"{recipe.DisplayName} Bottle",
                producedAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow),
                quality = quality,
                qualityScore = quality,
                basePrice = recipe.BasePrice,
                currentPrice = Math.Max(1, recipe.BasePrice + quality / 3),
                competitionScore = quality,
                salePrice = Math.Max(1, recipe.BasePrice + quality / 3),
                isAged = false
            };
            bottle.wineBottleId = bottle.bottleId;

            if (!session.InventorySystem.TryAddItem(bottleItemId, InventoryItemType.WineBottle, task.amount))
            {
                RaiseMessage("Storage is full. Free space before collecting wine.");
                return false;
            }

            data.wineBottles.Add(bottle);
            data.timedTasks.Remove(task);
            RaiseMessage($"Collected {recipe.DisplayName} bottle.");
            ProductionChanged?.Invoke();
            return true;
        }

        public void UpdateProduction()
        {
            DateTime now = session.TimeSystem.UtcNow;
            bool changed = false;
            foreach (TimedTaskData task in data.timedTasks)
            {
                if (task.taskType != TimedTaskType.WineProduction || task.isComplete)
                {
                    continue;
                }

                if (session.TimeSystem.TryParseSaveTime(task.completesAtUtc, out DateTime completesAt) && completesAt <= now)
                {
                    task.isComplete = true;
                    changed = true;
                }
            }

            if (changed)
            {
                ProductionChanged?.Invoke();
            }
        }

        public TimeSpan GetTimeRemaining(TimedTaskData task)
        {
            if (task == null || task.isComplete || !session.TimeSystem.TryParseSaveTime(task.completesAtUtc, out DateTime completesAt))
            {
                return TimeSpan.Zero;
            }

            TimeSpan remaining = completesAt - session.TimeSystem.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }

        private void RaiseMessage(string message)
        {
            MessageRaised?.Invoke(message);
        }
    }
}

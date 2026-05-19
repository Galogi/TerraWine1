using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Winery
{
    public class RecipeSystem : IGameSystem
    {
        private readonly Dictionary<string, WineRecipeRuntimeDefinition> recipeDefinitions = new Dictionary<string, WineRecipeRuntimeDefinition>
        {
            {
                "house_red",
                new WineRecipeRuntimeDefinition(
                    "house_red",
                    "House Red",
                    new List<RecipeIngredient> { new RecipeIngredient("grape_merlot", "Merlot Grapes", 2) },
                    30,
                    35,
                    55)
            },
            {
                "sunny_semidry",
                new WineRecipeRuntimeDefinition(
                    "sunny_semidry",
                    "Sunny Semi-Dry",
                    new List<RecipeIngredient> { new RecipeIngredient("grape_muscat", "Muscat Grapes", 2) },
                    25,
                    30,
                    52)
            },
            {
                "recipe_rose_blend",
                new WineRecipeRuntimeDefinition(
                    "recipe_rose_blend",
                    "Rose Blend",
                    new List<RecipeIngredient> { new RecipeIngredient("grape_merlot", "Merlot Grapes", 1), new RecipeIngredient("grape_muscat", "Muscat Grapes", 1) },
                    35,
                    42,
                    60)
            },
            {
                "recipe_gold_reserve",
                new WineRecipeRuntimeDefinition(
                    "recipe_gold_reserve",
                    "Gold Reserve",
                    new List<RecipeIngredient> { new RecipeIngredient("grape_merlot", "Merlot Grapes", 2) },
                    40,
                    55,
                    65)
            }
        };

        private GameSession session;
        private GameData data;

        public event Action RecipesChanged;

        public IReadOnlyCollection<WineRecipeRuntimeDefinition> Recipes => recipeDefinitions.Values;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            EnsureStartingRecipes();
            RefreshStolenRecipes();
        }

        public bool IsOwned(string recipeId)
        {
            RecipeStateData state = GetState(recipeId);
            return state != null && state.isKnown;
        }

        public bool IsAvailable(string recipeId)
        {
            RecipeStateData state = GetState(recipeId);
            if (state == null || !state.isKnown)
            {
                return false;
            }

            if (!state.isStolen)
            {
                return true;
            }

            if (session.TimeSystem.TryParseSaveTime(state.stolenUntilUtc, out DateTime stolenUntil) && stolenUntil <= session.TimeSystem.UtcNow)
            {
                state.isStolen = false;
                state.stolenUntilUtc = string.Empty;
                RecipesChanged?.Invoke();
                return true;
            }

            return false;
        }

        public WineRecipeRuntimeDefinition GetRecipe(string recipeId)
        {
            return recipeDefinitions.TryGetValue(recipeId, out WineRecipeRuntimeDefinition recipe) ? recipe : null;
        }

        public RecipeStateData GetState(string recipeId)
        {
            return data.recipes.Find(recipe => recipe.recipeId == recipeId);
        }

        public void LearnRecipe(string recipeId)
        {
            RecipeStateData state = GetState(recipeId);
            if (state == null)
            {
                data.recipes.Add(new RecipeStateData { recipeId = recipeId, isKnown = true });
            }
            else
            {
                state.isKnown = true;
                state.isStolen = false;
                state.stolenUntilUtc = string.Empty;
            }

            RecipesChanged?.Invoke();
        }

        public bool MarkRecipeStolen(string recipeId, TimeSpan duration)
        {
            RecipeStateData state = GetState(recipeId);
            if (state == null || !state.isKnown)
            {
                return false;
            }

            state.isStolen = true;
            state.stolenUntilUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow.Add(duration));
            RecipesChanged?.Invoke();
            return true;
        }

        public bool ReturnStolenRecipe(string recipeId)
        {
            RecipeStateData state = GetState(recipeId);
            if (state == null)
            {
                return false;
            }

            state.isStolen = false;
            state.stolenUntilUtc = string.Empty;
            RecipesChanged?.Invoke();
            return true;
        }

        public void RefreshStolenRecipes()
        {
            bool changed = false;
            foreach (RecipeStateData state in data.recipes)
            {
                if (!state.isStolen)
                {
                    continue;
                }

                if (session.TimeSystem.TryParseSaveTime(state.stolenUntilUtc, out DateTime stolenUntil) && stolenUntil <= session.TimeSystem.UtcNow)
                {
                    state.isStolen = false;
                    state.stolenUntilUtc = string.Empty;
                    changed = true;
                }
            }

            if (changed)
            {
                RecipesChanged?.Invoke();
            }
        }

        private void EnsureStartingRecipes()
        {
            EnsureKnownRecipe("house_red");
            EnsureKnownRecipe("sunny_semidry");
        }

        private void EnsureKnownRecipe(string recipeId)
        {
            RecipeStateData state = GetState(recipeId);
            if (state == null)
            {
                data.recipes.Add(new RecipeStateData { recipeId = recipeId, isKnown = true });
            }
            else
            {
                state.isKnown = true;
            }
        }
    }

    public class WineRecipeRuntimeDefinition
    {
        public WineRecipeRuntimeDefinition(string id, string displayName, List<RecipeIngredient> requiredGrapes, int productionSeconds, int basePrice, int baseQuality)
        {
            Id = id;
            DisplayName = displayName;
            RequiredGrapes = requiredGrapes;
            ProductionSeconds = productionSeconds;
            BasePrice = basePrice;
            BaseQuality = baseQuality;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<RecipeIngredient> RequiredGrapes { get; }
        public int ProductionSeconds { get; }
        public int BasePrice { get; }
        public int BaseQuality { get; }
    }

    public class RecipeIngredient
    {
        public RecipeIngredient(string itemId, string displayName, int amount)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Amount = amount;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
        public int Amount { get; }
    }
}

using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Winery
{
    public class WineQualitySystem : IGameSystem
    {
        private readonly Dictionary<string, int> grapeQuality = new Dictionary<string, int>
        {
            { "grape_merlot", 58 },
            { "grape_muscat", 54 }
        };

        private readonly Random random = new Random();
        private GameSession session;

        public void Initialize(GameSession gameSession, GameData data)
        {
            session = gameSession;
        }

        public int CalculateQuality(WineRecipeRuntimeDefinition recipe, bool includeRandomVariation)
        {
            if (recipe == null)
            {
                return 0;
            }

            int grapeBonus = 0;
            foreach (RecipeIngredient ingredient in recipe.RequiredGrapes)
            {
                grapeBonus += grapeQuality.TryGetValue(ingredient.ItemId, out int quality) ? (quality - 50) / 2 : 0;
            }

            int vineyardBonus = session.Data.resources.water > 0 ? 2 : 0;
            int variation = includeRandomVariation ? random.Next(-3, 4) : 0;
            return Math.Clamp(recipe.BaseQuality + grapeBonus + vineyardBonus + variation, 1, 100);
        }
    }
}

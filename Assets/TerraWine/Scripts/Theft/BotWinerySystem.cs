using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Theft
{
    public class BotWinerySystem : IGameSystem
    {
        private GameData data;

        public IReadOnlyList<BotWineryData> Bots => data.botWineries;

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
            EnsureBots();
        }

        public BotWineryData GetBot(string botWineryId)
        {
            return data.botWineries.Find(bot => bot.botWineryId == botWineryId);
        }

        private void EnsureBots()
        {
            EnsureBot(new BotWineryData
            {
                botWineryId = "bot_rose_hill",
                definitionId = "bot_rose_hill",
                wineryName = "Rose Hill Winery",
                displayName = "Rose Hill Winery",
                reputation = 12,
                vaultDigits = 3,
                vaultPassword = "123",
                ownedRecipeIds = new List<string> { "recipe_rose_blend" },
                storedBottleCount = 2,
                defenseLevel = 1,
                storedResources = new ResourceData { money = 40, water = 8, wood = 5, metal = 2 }
            });
            EnsureBot(new BotWineryData
            {
                botWineryId = "bot_golden_barrel",
                definitionId = "bot_golden_barrel",
                wineryName = "Golden Barrel Estate",
                displayName = "Golden Barrel Estate",
                reputation = 18,
                vaultDigits = 3,
                vaultPassword = "742",
                ownedRecipeIds = new List<string> { "recipe_gold_reserve" },
                storedBottleCount = 3,
                defenseLevel = 2,
                storedResources = new ResourceData { money = 60, water = 6, wood = 7, metal = 4 }
            });
            EnsureBot(new BotWineryData
            {
                botWineryId = "bot_shadow_vine",
                definitionId = "bot_shadow_vine",
                wineryName = "Shadow Vine Cellar",
                displayName = "Shadow Vine Cellar",
                reputation = 25,
                vaultDigits = 4,
                vaultPassword = "9157",
                ownedRecipeIds = new List<string> { "recipe_gold_reserve" },
                storedBottleCount = 4,
                defenseLevel = 4,
                storedResources = new ResourceData { money = 90, water = 4, wood = 4, metal = 8 }
            });
        }

        private void EnsureBot(BotWineryData template)
        {
            BotWineryData existing = GetBot(template.botWineryId);
            if (existing == null)
            {
                data.botWineries.Add(template);
                return;
            }

            if (string.IsNullOrWhiteSpace(existing.displayName))
            {
                existing.displayName = existing.wineryName;
            }

            if (existing.ownedRecipeIds.Count == 0)
            {
                existing.ownedRecipeIds.AddRange(template.ownedRecipeIds);
            }
        }
    }
}

using System;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Theft
{
    public class TheftSystem : IGameSystem
    {
        private readonly Random random = new Random();
        private GameSession session;
        private GameData data;

        public event Action TheftChanged;
        public event Action<string> MessageRaised;

        public string LastMessage { get; private set; } = string.Empty;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
        }

        public bool TryStealRecipe(string botWineryId, string recipeId, string passwordGuess)
        {
            BotWineryData bot = session.BotWinerySystem.GetBot(botWineryId);
            if (bot == null || !bot.ownedRecipeIds.Contains(recipeId))
            {
                RaiseMessage("Target recipe is not available from this winery.");
                return false;
            }

            if (!SpendTheftAction())
            {
                return false;
            }

            bool success = session.VaultSystem.ValidateBotPassword(bot, passwordGuess);
            if (success)
            {
                session.RecipeSystem.LearnRecipe(recipeId);
                if (!bot.stolenRecipeIds.Contains(recipeId))
                {
                    bot.stolenRecipeIds.Add(recipeId);
                }

                bot.lastTheftResult = $"Recipe stolen: {recipeId}";
                AddHistory(bot.botWineryId, TheftTargetType.Recipe, recipeId, true, "Recipe theft succeeded.");
                RaiseMessage($"Stole recipe {recipeId} from {GetBotName(bot)}.");
            }
            else
            {
                ApplyFailedTheftPenalty(bot, TheftTargetType.Recipe, recipeId);
            }

            TheftChanged?.Invoke();
            return success;
        }

        public bool TryStealResources(string botWineryId)
        {
            BotWineryData bot = session.BotWinerySystem.GetBot(botWineryId);
            if (bot == null)
            {
                RaiseMessage("Target winery not found.");
                return false;
            }

            if (!SpendTheftAction())
            {
                return false;
            }

            bool success = random.Next(0, 100) >= bot.defenseLevel * 15;
            if (!success)
            {
                ApplyFailedTheftPenalty(bot, TheftTargetType.Resources, "resources");
                TheftChanged?.Invoke();
                return false;
            }

            int money = Math.Min(20, bot.storedResources.money);
            bot.storedResources.money -= money;
            session.ResourceSystem.Add(ResourceType.Money, money);
            AddHistory(bot.botWineryId, TheftTargetType.Resources, "money", true, "Resource theft succeeded.");
            RaiseMessage($"Stole {money} money from {GetBotName(bot)}.");
            TheftChanged?.Invoke();
            return true;
        }

        public bool TryStealBottle(string botWineryId)
        {
            BotWineryData bot = session.BotWinerySystem.GetBot(botWineryId);
            if (bot == null)
            {
                RaiseMessage("Target winery not found.");
                return false;
            }

            if (!SpendTheftAction())
            {
                return false;
            }

            bool success = bot.storedBottleCount > 0 && random.Next(0, 100) >= bot.defenseLevel * 15;
            if (!success)
            {
                ApplyFailedTheftPenalty(bot, TheftTargetType.Bottle, "wine_bottle");
                TheftChanged?.Invoke();
                return false;
            }

            bot.storedBottleCount--;
            session.InventorySystem.TryAddItem("stolen_bot_bottle", InventoryItemType.WineBottle, 1);
            AddHistory(bot.botWineryId, TheftTargetType.Bottle, "stolen_bot_bottle", true, "Bottle theft succeeded.");
            RaiseMessage($"Stole a bottle from {GetBotName(bot)}.");
            TheftChanged?.Invoke();
            return true;
        }

        public bool MarkPlayerRecipeStolen(string recipeId)
        {
            bool changed = session.RecipeSystem.MarkRecipeStolen(recipeId, TimeSpan.FromMinutes(5));
            RaiseMessage(changed ? $"{recipeId} is stolen and unavailable." : "Could not mark recipe stolen.");
            TheftChanged?.Invoke();
            return changed;
        }

        public bool ReturnPlayerRecipe(string recipeId)
        {
            bool changed = session.RecipeSystem.ReturnStolenRecipe(recipeId);
            RaiseMessage(changed ? $"{recipeId} returned." : "Could not return recipe.");
            TheftChanged?.Invoke();
            return changed;
        }

        private bool SpendTheftAction()
        {
            if (session.DailyActionSystem.ActionsRemaining <= 0)
            {
                RaiseMessage("No daily actions remaining for theft.");
                return false;
            }

            return session.DailyActionSystem.SpendAction();
        }

        private void ApplyFailedTheftPenalty(BotWineryData bot, TheftTargetType targetType, string targetId)
        {
            session.ReputationSystem.AddReputation(-10);
            bot.reputation += 5;
            bot.lastTheftResult = "Theft failed. Defender gained reputation.";
            data.temporarySaleModifiers.Add(new TemporarySaleModifierData
            {
                type = SaleModifierType.SalePenalty,
                percentModifier = -0.2f,
                expiresAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow.AddMinutes(5)),
                source = bot.botWineryId,
                reason = "Failed theft attempt"
            });
            AddHistory(bot.botWineryId, targetType, targetId, false, "Theft failed.");
            RaiseMessage($"Theft failed against {GetBotName(bot)}. Reputation -10.");
        }

        private void AddHistory(string defenderId, TheftTargetType targetType, string targetId, bool success, string message)
        {
            data.theftHistory.Add(new TheftHistoryData
            {
                attackerId = data.player.playerId,
                defenderId = defenderId,
                targetType = targetType,
                targetId = targetId,
                success = success,
                resultMessage = message,
                occurredAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow)
            });
        }

        private static string GetBotName(BotWineryData bot)
        {
            return string.IsNullOrWhiteSpace(bot.displayName) ? bot.wineryName : bot.displayName;
        }

        private void RaiseMessage(string message)
        {
            LastMessage = message;
            MessageRaised?.Invoke(message);
        }
    }
}

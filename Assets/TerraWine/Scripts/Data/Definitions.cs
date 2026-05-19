using System.Collections.Generic;
using UnityEngine;

namespace TerraWine.Data
{
    public abstract class TerraWineDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [TextArea, SerializeField] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
    }

    [CreateAssetMenu(menuName = "TerraWine/Seeds/Seed Definition")]
    public class SeedDefinition : TerraWineDefinition
    {
        public string producesGrapeId;
        public int growSeconds = 300;
        public int waterRequired = 1;
        public int baseQuality = 50;
    }

    [CreateAssetMenu(menuName = "TerraWine/Grapes/Grape Definition")]
    public class GrapeDefinition : TerraWineDefinition
    {
        public int sweetness;
        public int acidity;
        public int body;
        public int baseQuality = 50;
    }

    [CreateAssetMenu(menuName = "TerraWine/Recipes/Wine Recipe Definition")]
    public class WineRecipeDefinition : TerraWineDefinition
    {
        public List<string> requiredGrapeIds = new List<string>();
        public bool supportsDry = true;
        public bool supportsSemiDry = true;
        public int productionSeconds = 600;
        public int basePrice = 25;
        public int baseCompetitionScore = 50;
    }

    [CreateAssetMenu(menuName = "TerraWine/Barrels/Barrel Definition")]
    public class BarrelDefinition : TerraWineDefinition
    {
        public int agingSeconds = 900;
        public float priceMultiplier = 1.1f;
        public int scoreBonus = 5;
        public int capacity = 1;
    }

    [CreateAssetMenu(menuName = "TerraWine/Shop/Shop Item Definition")]
    public class ShopItemDefinition : TerraWineDefinition
    {
        public string itemId;
        public InventoryItemType itemType;
        public ResourceData cost = new ResourceData();
    }

    [CreateAssetMenu(menuName = "TerraWine/Decorations/Decoration Definition")]
    public class DecorationDefinition : TerraWineDefinition
    {
        public int reputationBonus;
    }

    [CreateAssetMenu(menuName = "TerraWine/Defenses/Defense Upgrade Definition")]
    public class DefenseUpgradeDefinition : TerraWineDefinition
    {
        public int vaultDigitRequirement = 3;
        public int theftDefenseBonus;
    }

    [CreateAssetMenu(menuName = "TerraWine/Competitions/Competition Definition")]
    public class CompetitionDefinition : TerraWineDefinition
    {
        public int year = 1;
        public List<string> judgeIds = new List<string>();
        public int entryFee;
    }

    [CreateAssetMenu(menuName = "TerraWine/Judges/Judge Definition")]
    public class JudgeDefinition : TerraWineDefinition
    {
        public int dryPreference;
        public int semiDryPreference;
        public int sweetnessPreference;
        public int bodyPreference;
    }

    [CreateAssetMenu(menuName = "TerraWine/Visitors/Visitor Definition")]
    public class VisitorDefinition : TerraWineDefinition
    {
        public int dryPreference;
        public int semiDryPreference;
        public int maxBottlePrice;
    }

    [CreateAssetMenu(menuName = "TerraWine/AI/Bot Winery Definition")]
    public class BotWineryDefinition : TerraWineDefinition
    {
        public int startingReputation = 5;
        public List<string> knownRecipeIds = new List<string>();
    }

    [CreateAssetMenu(menuName = "TerraWine/Quests/Daily Mission Definition")]
    public class DailyMissionDefinition : TerraWineDefinition
    {
        public string targetId;
        public int targetAmount = 1;
        public ResourceData reward = new ResourceData();
    }

    [CreateAssetMenu(menuName = "TerraWine/Tutorial/Tutorial Step Definition")]
    public class TutorialStepDefinition : TerraWineDefinition
    {
        public string nextStepId;
        public string completionEventId;
    }

    [CreateAssetMenu(menuName = "TerraWine/Weather/Season Definition")]
    public class SeasonDefinition : TerraWineDefinition
    {
        public SeasonType season;
        public float grapeGrowthMultiplier = 1f;
        public float waterUseMultiplier = 1f;
    }

    [CreateAssetMenu(menuName = "TerraWine/Events/Random Event Definition")]
    public class RandomEventDefinition : TerraWineDefinition
    {
        public int durationSeconds = 3600;
        public float priceMultiplier = 1f;
        public float waterMultiplier = 1f;
    }

    [CreateAssetMenu(menuName = "TerraWine/Achievements/Achievement Definition")]
    public class AchievementDefinition : TerraWineDefinition
    {
        public string trackedStatId;
        public int requiredAmount = 1;
        public ResourceData reward = new ResourceData();
    }
}

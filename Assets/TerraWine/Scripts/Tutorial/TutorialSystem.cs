using System;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Tutorial
{
    public class TutorialSystem : IGameSystem
    {
        public const string PlantSeedStepId = "plant_first_seed";
        public const string WaterPlotStepId = "water_first_plot";
        public const string HarvestGrapesStepId = "harvest_first_grapes";

        private GameData data;

        public event Action TutorialChanged;

        public string CurrentStepId => data.tutorialProgress.currentStepId;
        public string CurrentInstruction => GetInstruction(CurrentStepId);

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
            if (string.IsNullOrWhiteSpace(data.tutorialProgress.currentStepId))
            {
                data.tutorialProgress.currentStepId = PlantSeedStepId;
            }
        }

        public void CompleteStep(string stepId)
        {
            if (string.IsNullOrWhiteSpace(stepId) || data.tutorialProgress.completedStepIds.Contains(stepId))
            {
                return;
            }

            data.tutorialProgress.completedStepIds.Add(stepId);
            if (data.tutorialProgress.currentStepId == stepId)
            {
                data.tutorialProgress.currentStepId = GetNextStep(stepId);
            }

            TutorialChanged?.Invoke();
        }

        private static string GetNextStep(string stepId)
        {
            return stepId switch
            {
                PlantSeedStepId => WaterPlotStepId,
                WaterPlotStepId => HarvestGrapesStepId,
                HarvestGrapesStepId => string.Empty,
                _ => string.Empty
            };
        }

        private static string GetInstruction(string stepId)
        {
            return stepId switch
            {
                PlantSeedStepId => "Plant your first grape seed.",
                WaterPlotStepId => "Water the planted vineyard plot.",
                HarvestGrapesStepId => "Harvest grapes when the plot is ready.",
                _ => "Tutorial complete."
            };
        }
    }
}

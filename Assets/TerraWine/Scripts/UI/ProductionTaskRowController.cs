using System;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class ProductionTaskRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text recipeNameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text timeRemainingText;
        [SerializeField] private Button collectButton;

        private GameSession session;
        private TimedTaskData task;

        private void Awake()
        {
            if (collectButton != null)
            {
                collectButton.onClick.AddListener(Collect);
            }
        }

        public void Bind(GameSession gameSession, TimedTaskData taskData)
        {
            session = gameSession;
            task = taskData;
            Refresh();
        }

        private void Refresh()
        {
            WineRecipeRuntimeDefinition recipe = session.RecipeSystem.GetRecipe(task.definitionId);
            TimeSpan remaining = session.WineProductionSystem.GetTimeRemaining(task);
            SetText(recipeNameText, recipe == null ? task.definitionId : recipe.DisplayName);
            SetText(stateText, task.isComplete ? "Finished" : "Producing");
            SetText(timeRemainingText, FormatRemaining(remaining));
            SetInteractable(collectButton, task.isComplete);
        }

        private void Collect()
        {
            session?.WineProductionSystem.CollectFinishedWine(task.taskId);
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return "-";
            }

            return remaining.TotalMinutes >= 1 ? $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s" : $"{remaining.Seconds}s";
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static void SetInteractable(Button button, bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }
    }
}

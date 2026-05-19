using System;
using System.Collections.Generic;
using System.Text;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class ProductionPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Transform recipeRowsRoot;
        [SerializeField] private ProductionRecipeRowController recipeRowPrefab;
        [SerializeField] private Transform taskRowsRoot;
        [SerializeField] private ProductionTaskRowController taskRowPrefab;
        [SerializeField] private List<ProductionRecipeRowController> staticRecipeRows = new List<ProductionRecipeRowController>();
        [SerializeField] private List<ProductionTaskRowController> staticTaskRows = new List<ProductionTaskRowController>();
        [SerializeField] private float timerRefreshSeconds = 1f;

        private readonly List<ProductionRecipeRowController> recipeRows = new List<ProductionRecipeRowController>();
        private readonly List<ProductionTaskRowController> taskRows = new List<ProductionTaskRowController>();
        private GameSession session;
        private float nextRefreshTime;

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            if (session?.WineProductionSystem != null)
            {
                session.WineProductionSystem.ProductionChanged -= Refresh;
                session.WineProductionSystem.MessageRaised -= ShowMessage;
            }

            if (session?.RecipeSystem != null)
            {
                session.RecipeSystem.RecipesChanged -= Refresh;
            }

            if (session?.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged -= Refresh;
            }
        }

        private void Update()
        {
            if (session?.WineProductionSystem == null || Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + timerRefreshSeconds;
            Refresh();
        }

        public void Refresh()
        {
            if (session?.RecipeSystem == null || session.WineProductionSystem == null)
            {
                return;
            }

            session.WineProductionSystem.UpdateProduction();
            List<WineRecipeRuntimeDefinition> recipes = new List<WineRecipeRuntimeDefinition>(session.RecipeSystem.Recipes);
            IReadOnlyList<TimedTaskData> tasks = session.WineProductionSystem.GetProductionTasks();

            EnsureRecipeRows(recipes.Count);
            EnsureTaskRows(tasks.Count);

            for (int i = 0; i < recipeRows.Count; i++)
            {
                bool active = i < recipes.Count;
                recipeRows[i].gameObject.SetActive(active);
                if (active)
                {
                    recipeRows[i].Bind(session, recipes[i]);
                }
            }

            for (int i = 0; i < taskRows.Count; i++)
            {
                bool active = i < tasks.Count;
                taskRows[i].gameObject.SetActive(active);
                if (active)
                {
                    taskRows[i].Bind(session, tasks[i]);
                }
            }
        }

        private void Bind()
        {
            if (GameBootstrap.Session == null || session == GameBootstrap.Session)
            {
                return;
            }

            OnDisable();
            session = GameBootstrap.Session;
            if (session.WineProductionSystem != null)
            {
                session.WineProductionSystem.ProductionChanged += Refresh;
                session.WineProductionSystem.MessageRaised += ShowMessage;
            }

            if (session.RecipeSystem != null)
            {
                session.RecipeSystem.RecipesChanged += Refresh;
            }

            if (session.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged += Refresh;
            }
        }

        private void EnsureRecipeRows(int count)
        {
            if (recipeRows.Count == 0)
            {
                recipeRows.AddRange(staticRecipeRows.FindAll(row => row != null));
            }

            if (recipeRowPrefab == null || recipeRowsRoot == null)
            {
                return;
            }

            while (recipeRows.Count < count)
            {
                recipeRows.Add(Instantiate(recipeRowPrefab, recipeRowsRoot));
            }
        }

        private void EnsureTaskRows(int count)
        {
            if (taskRows.Count == 0)
            {
                taskRows.AddRange(staticTaskRows.FindAll(row => row != null));
            }

            if (taskRowPrefab == null || taskRowsRoot == null)
            {
                return;
            }

            while (taskRows.Count < count)
            {
                taskRows.Add(Instantiate(taskRowPrefab, taskRowsRoot));
            }
        }

        private void ShowMessage(string message)
        {
            SetText(messageText, message);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }

    public class ProductionRecipeRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text recipeNameText;
        [SerializeField] private TMP_Text requiredGrapesText;
        [SerializeField] private TMP_Text productionTimeText;
        [SerializeField] private TMP_Text expectedQualityText;
        [SerializeField] private TMP_Text availabilityText;
        [SerializeField] private Button startProductionButton;

        private GameSession session;
        private WineRecipeRuntimeDefinition recipe;

        private void Awake()
        {
            if (startProductionButton != null)
            {
                startProductionButton.onClick.AddListener(StartProduction);
            }
        }

        public void Bind(GameSession gameSession, WineRecipeRuntimeDefinition recipeData)
        {
            session = gameSession;
            recipe = recipeData;
            Refresh();
        }

        private void Refresh()
        {
            bool owned = session.RecipeSystem.IsOwned(recipe.Id);
            bool available = session.RecipeSystem.IsAvailable(recipe.Id);
            SetText(recipeNameText, recipe.DisplayName);
            SetText(requiredGrapesText, FormatIngredients(recipe.RequiredGrapes));
            SetText(productionTimeText, FormatSeconds(recipe.ProductionSeconds));
            SetText(expectedQualityText, session.WineQualitySystem.CalculateQuality(recipe, false).ToString());
            SetText(availabilityText, owned ? available ? "Available" : "Unavailable" : "Locked");
            SetInteractable(startProductionButton, owned && available);
        }

        private void StartProduction()
        {
            session?.WineProductionSystem.StartProduction(recipe.Id);
        }

        private static string FormatIngredients(IReadOnlyList<RecipeIngredient> ingredients)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(ingredients[i].Amount);
                builder.Append(" ");
                builder.Append(ingredients[i].DisplayName);
            }

            return builder.ToString();
        }

        private static string FormatSeconds(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.TotalMinutes >= 1 ? $"{(int)time.TotalMinutes}m {time.Seconds}s" : $"{time.Seconds}s";
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

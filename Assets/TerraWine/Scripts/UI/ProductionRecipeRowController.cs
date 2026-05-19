using System;
using System.Collections.Generic;
using System.Text;
using TerraWine.Core;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
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
}

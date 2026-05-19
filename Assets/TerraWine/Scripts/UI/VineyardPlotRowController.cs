using System;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class VineyardPlotRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text plotNameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text seedText;
        [SerializeField] private TMP_Text grapeText;
        [SerializeField] private TMP_Text timeRemainingText;
        [SerializeField] private TMP_Text waterNeededText;
        [SerializeField] private Button plantButton;
        [SerializeField] private Button waterButton;
        [SerializeField] private Button harvestButton;

        private GameSession session;
        private VineyardPlotData plot;

        private void Awake()
        {
            if (plantButton != null)
            {
                plantButton.onClick.AddListener(Plant);
            }

            if (waterButton != null)
            {
                waterButton.onClick.AddListener(Water);
            }

            if (harvestButton != null)
            {
                harvestButton.onClick.AddListener(Harvest);
            }
        }

        public void Bind(GameSession gameSession, VineyardPlotData plotData)
        {
            session = gameSession;
            plot = plotData;
            Refresh();
        }

        private void Refresh()
        {
            if (session?.VineyardSystem == null || plot == null)
            {
                return;
            }

            VineyardPlotState state = session.VineyardSystem.GetPlotState(plot);
            SeedRuntimeDefinition seed = session.VineyardSystem.GetSeed(plot.plantedSeedId);
            TimeSpan remaining = session.VineyardSystem.GetTimeRemaining(plot);

            SetText(plotNameText, plot.plotId);
            SetText(stateText, state.ToString());
            SetText(seedText, state == VineyardPlotState.Empty ? "-" : seed.DisplayName);
            SetText(grapeText, state == VineyardPlotState.Empty ? "-" : seed.GrapeDisplayName);
            SetText(timeRemainingText, FormatRemaining(remaining));
            SetText(waterNeededText, state == VineyardPlotState.NeedsWater ? plot.waterRequired.ToString() : "-");

            SetInteractable(plantButton, state == VineyardPlotState.Empty && !string.IsNullOrWhiteSpace(session.VineyardSystem.GetFirstOwnedSeedId()));
            SetInteractable(waterButton, state == VineyardPlotState.NeedsWater);
            SetInteractable(harvestButton, state == VineyardPlotState.Ready);
        }

        private void Plant()
        {
            session?.VineyardSystem.PlantFirstOwnedSeed(plot.plotId);
        }

        private void Water()
        {
            session?.VineyardSystem.WaterPlot(plot.plotId);
        }

        private void Harvest()
        {
            session?.VineyardSystem.HarvestPlot(plot.plotId);
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return "-";
            }

            if (remaining.TotalHours >= 1)
            {
                return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
            }

            if (remaining.TotalMinutes >= 1)
            {
                return $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s";
            }

            return $"{remaining.Seconds}s";
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

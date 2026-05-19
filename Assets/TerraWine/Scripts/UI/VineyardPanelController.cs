using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class VineyardPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text tutorialText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private VineyardPlotRowController rowPrefab;
        [SerializeField] private List<VineyardPlotRowController> staticRows = new List<VineyardPlotRowController>();

        private readonly List<VineyardPlotRowController> rows = new List<VineyardPlotRowController>();
        private GameSession session;

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            if (session?.VineyardSystem != null)
            {
                session.VineyardSystem.VineyardChanged -= Refresh;
                session.VineyardSystem.MessageRaised -= ShowMessage;
            }

            if (session?.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged -= Refresh;
            }

            if (session?.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged -= Refresh;
            }

            if (session?.TutorialSystem != null)
            {
                session.TutorialSystem.TutorialChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            if (session?.VineyardSystem == null)
            {
                return;
            }

            session.VineyardSystem.UpdateGrowth();
            SetText(tutorialText, session.TutorialSystem?.CurrentInstruction ?? string.Empty);
            EnsureRows(session.VineyardSystem.Plots.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                if (i < session.VineyardSystem.Plots.Count)
                {
                    rows[i].gameObject.SetActive(true);
                    rows[i].Bind(session, session.VineyardSystem.Plots[i]);
                }
                else
                {
                    rows[i].gameObject.SetActive(false);
                }
            }
        }

        private void Bind()
        {
            if (session == GameBootstrap.Session || GameBootstrap.Session == null)
            {
                return;
            }

            OnDisable();
            session = GameBootstrap.Session;
            if (session.VineyardSystem != null)
            {
                session.VineyardSystem.VineyardChanged += Refresh;
                session.VineyardSystem.MessageRaised += ShowMessage;
            }

            if (session.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged += Refresh;
            }

            if (session.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged += Refresh;
            }

            if (session.TutorialSystem != null)
            {
                session.TutorialSystem.TutorialChanged += Refresh;
            }
        }

        private void EnsureRows(int count)
        {
            if (rows.Count == 0)
            {
                rows.AddRange(staticRows.FindAll(row => row != null));
            }

            if (rowPrefab == null || rowsRoot == null)
            {
                return;
            }

            while (rows.Count < count)
            {
                rows.Add(Instantiate(rowPrefab, rowsRoot));
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

using System;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class BarrelRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text barrelNameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text wineInsideText;
        [SerializeField] private TMP_Text timeRemainingText;
        [SerializeField] private TMP_Text qualityBonusText;
        [SerializeField] private TMP_Text priceBonusText;
        [SerializeField] private TMP_Text competitionScoreBonusText;
        [SerializeField] private Button startAgingButton;
        [SerializeField] private Button collectButton;

        private GameSession session;
        private BarrelData barrel;

        private void Awake()
        {
            if (startAgingButton != null)
            {
                startAgingButton.onClick.AddListener(StartAging);
            }

            if (collectButton != null)
            {
                collectButton.onClick.AddListener(Collect);
            }
        }

        public void Bind(GameSession gameSession, BarrelData barrelData)
        {
            session = gameSession;
            barrel = barrelData;
            Refresh();
        }

        private void Refresh()
        {
            if (session?.BarrelSystem == null || barrel == null)
            {
                return;
            }

            BarrelRuntimeDefinition definition = session.BarrelSystem.GetDefinition(barrel.barrelDefinitionId);
            WineBottleData bottle = session.BarrelSystem.GetBottleInside(barrel);
            BarrelState state = session.BarrelSystem.GetBarrelState(barrel);
            TimeSpan remaining = session.BarrelSystem.GetTimeRemaining(barrel);

            SetText(barrelNameText, string.IsNullOrWhiteSpace(barrel.displayName) ? definition.DisplayName : barrel.displayName);
            SetText(stateText, state.ToString());
            SetText(wineInsideText, bottle == null ? "-" : GetBottleDisplayName(bottle));
            SetText(timeRemainingText, FormatRemaining(remaining));
            SetText(qualityBonusText, $"+{definition.QualityBonus}");
            SetText(priceBonusText, $"+{definition.PriceBonus}");
            SetText(competitionScoreBonusText, $"+{definition.CompetitionScoreBonus}");
            SetInteractable(startAgingButton, state == BarrelState.Empty);
            SetInteractable(collectButton, state == BarrelState.Ready);
        }

        private void StartAging()
        {
            session?.BarrelSystem.StartAgingFirstAvailableWine(barrel.barrelId);
        }

        private void Collect()
        {
            session?.BarrelSystem.CollectAgedWine(barrel.barrelId);
        }

        private static string GetBottleDisplayName(WineBottleData bottle)
        {
            return string.IsNullOrWhiteSpace(bottle.displayName) ? bottle.itemId : bottle.displayName;
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

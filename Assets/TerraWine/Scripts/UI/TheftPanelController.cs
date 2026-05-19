using System.Collections.Generic;
using System.Linq;
using TerraWine.Core;
using TerraWine.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class TheftPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Transform botRowsRoot;
        [SerializeField] private TheftBotRowController botRowPrefab;
        [SerializeField] private List<TheftBotRowController> staticRows = new List<TheftBotRowController>();
        [SerializeField] private Button stealRecipeButton;
        [SerializeField] private Button simulatePlayerRecipeStolenButton;
        [SerializeField] private Button returnStolenRecipeButton;

        private readonly List<TheftBotRowController> rows = new List<TheftBotRowController>();
        private GameSession session;
        private BotWineryData selectedBot;
        private string selectedRecipeId;

        private void Awake()
        {
            if (stealRecipeButton != null)
            {
                stealRecipeButton.onClick.AddListener(TryStealRecipe);
            }

            if (simulatePlayerRecipeStolenButton != null)
            {
                simulatePlayerRecipeStolenButton.onClick.AddListener(SimulatePlayerRecipeStolen);
            }

            if (returnStolenRecipeButton != null)
            {
                returnStolenRecipeButton.onClick.AddListener(ReturnStolenRecipe);
            }
        }

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void Start()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            if (session?.TheftSystem != null)
            {
                session.TheftSystem.TheftChanged -= Refresh;
                session.TheftSystem.MessageRaised -= ShowMessage;
            }
        }

        public void SelectBot(BotWineryData bot)
        {
            selectedBot = bot;
            selectedRecipeId = bot?.ownedRecipeIds.FirstOrDefault();
            Refresh();
        }

        public void Refresh()
        {
            if (session?.TheftSystem == null)
            {
                return;
            }

            if (selectedBot == null && session.BotWinerySystem.Bots.Count > 0)
            {
                SelectBot(session.BotWinerySystem.Bots[0]);
                return;
            }

            SetText(statusText, $"Reputation: {session.Data.player.reputation} | Actions: {session.DailyActionSystem.ActionsRemaining}/{session.DailyActionSystem.MaxActions} | Vault: {session.Data.winery.vaultDigits} digits | Target: {GetSelectedTargetText()}");
            SetText(messageText, session.TheftSystem.LastMessage);
            EnsureRows(session.BotWinerySystem.Bots.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                bool active = i < session.BotWinerySystem.Bots.Count;
                rows[i].gameObject.SetActive(active);
                if (active)
                {
                    rows[i].Bind(this, session.BotWinerySystem.Bots[i], selectedBot == session.BotWinerySystem.Bots[i]);
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
            session.TheftSystem.TheftChanged += Refresh;
            session.TheftSystem.MessageRaised += ShowMessage;
        }

        private void EnsureRows(int count)
        {
            if (rows.Count == 0)
            {
                rows.AddRange(staticRows.FindAll(row => row != null));
            }

            if (botRowPrefab == null || botRowsRoot == null)
            {
                return;
            }

            while (rows.Count < count)
            {
                rows.Add(Instantiate(botRowPrefab, botRowsRoot));
            }
        }

        private void TryStealRecipe()
        {
            if (selectedBot == null || string.IsNullOrWhiteSpace(selectedRecipeId))
            {
                ShowMessage("Select a rival winery first.");
                return;
            }

            session.TheftSystem.TryStealRecipe(selectedBot.botWineryId, selectedRecipeId, passwordInput == null ? string.Empty : passwordInput.text);
        }

        private void SimulatePlayerRecipeStolen()
        {
            session?.TheftSystem.MarkPlayerRecipeStolen("house_red");
        }

        private void ReturnStolenRecipe()
        {
            session?.TheftSystem.ReturnPlayerRecipe("house_red");
        }

        private string GetSelectedTargetText()
        {
            if (selectedBot == null)
            {
                return "-";
            }

            return $"{selectedBot.displayName} / {selectedRecipeId}";
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
}

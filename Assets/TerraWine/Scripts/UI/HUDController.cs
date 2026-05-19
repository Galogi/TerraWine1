using TerraWine.Core;
using TerraWine.Data;
using TMPro;
using UnityEngine;

namespace TerraWine.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text waterText;
        [SerializeField] private TMP_Text woodText;
        [SerializeField] private TMP_Text metalText;
        [SerializeField] private TMP_Text reputationText;

        [Header("Time")]
        [SerializeField] private TMP_Text yearText;
        [SerializeField] private TMP_Text seasonText;
        [SerializeField] private TMP_Text dailyActionsText;

        private GameSession session;

        private void OnEnable()
        {
            TryBind();
            Refresh();
        }

        private void Start()
        {
            TryBind();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeFromSessionSystems();
        }

        public void Refresh()
        {
            if (session?.Data == null)
            {
                return;
            }

            ResourceData resources = session.Data.resources;
            SetText(moneyText, resources.money.ToString());
            SetText(waterText, resources.water.ToString());
            SetText(woodText, resources.wood.ToString());
            SetText(metalText, resources.metal.ToString());
            SetText(reputationText, session.Data.player.reputation.ToString());
            SetText(yearText, $"{session.CalendarSystem.CurrentYear}/{session.CalendarSystem.TotalYears}");
            SetText(seasonText, session.CalendarSystem.CurrentSeason.ToString());
            SetText(dailyActionsText, $"{session.DailyActionSystem.ActionsRemaining}/{session.DailyActionSystem.MaxActions}");
        }

        private void TryBind()
        {
            if (GameBootstrap.Session == null || session == GameBootstrap.Session)
            {
                return;
            }

            UnsubscribeFromSessionSystems();
            session = GameBootstrap.Session;
            session.GameLoaded += OnGameLoaded;
            SubscribeToSessionSystems();
        }

        private void SubscribeToSessionSystems()
        {
            if (session == null)
            {
                return;
            }

            if (session.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged += Refresh;
            }

            if (session.DailyActionSystem != null)
            {
                session.DailyActionSystem.DailyActionsChanged += Refresh;
            }

            if (session.CalendarSystem != null)
            {
                session.CalendarSystem.CalendarChanged += Refresh;
            }
        }

        private void UnsubscribeFromSessionSystems()
        {
            if (session == null)
            {
                return;
            }

            session.GameLoaded -= OnGameLoaded;
            if (session.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged -= Refresh;
            }

            if (session.DailyActionSystem != null)
            {
                session.DailyActionSystem.DailyActionsChanged -= Refresh;
            }

            if (session.CalendarSystem != null)
            {
                session.CalendarSystem.CalendarChanged -= Refresh;
            }
        }

        private void OnGameLoaded(GameData data)
        {
            SubscribeToSessionSystems();
            Refresh();
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

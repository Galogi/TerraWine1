using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.WorldMap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class WorldMapPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text actionsText;
        [SerializeField] private TMP_Text weatherText;
        [SerializeField] private TMP_Text forecastText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button fortuneButton;
        [SerializeField] private Button collectTaskButton;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private WorldMapNodeRowController rowPrefab;
        [SerializeField] private List<WorldMapNodeRowController> staticRows = new List<WorldMapNodeRowController>();
        [SerializeField] private float timerRefreshSeconds = 1f;

        private readonly List<WorldMapNodeRowController> rows = new List<WorldMapNodeRowController>();
        private GameSession session;
        private float nextRefreshTime;

        private void Awake()
        {
            if (fortuneButton != null)
            {
                fortuneButton.onClick.AddListener(AskFortuneTeller);
            }

            if (collectTaskButton != null)
            {
                collectTaskButton.onClick.AddListener(CollectTask);
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
            if (session?.WorldMapSystem != null)
            {
                session.WorldMapSystem.WorldMapChanged -= Refresh;
                session.WorldMapSystem.MessageRaised -= ShowMessage;
            }

            if (session?.DailyActionSystem != null)
            {
                session.DailyActionSystem.DailyActionsChanged -= Refresh;
            }

            if (session?.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged -= Refresh;
            }

            if (session?.WeatherSystem != null)
            {
                session.WeatherSystem.WeatherChanged -= Refresh;
            }
        }

        private void Update()
        {
            if (session?.WorldMapSystem == null || Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + timerRefreshSeconds;
            Refresh();
        }

        public void Refresh()
        {
            if (session?.WorldMapSystem == null)
            {
                return;
            }

            session.WorldMapSystem.UpdateWorldTasks();
            SetText(actionsText, $"Actions: {session.DailyActionSystem.ActionsRemaining}/{session.DailyActionSystem.MaxActions}");
            SetText(weatherText, $"Weather: {session.WeatherSystem.CurrentWeather}");
            SetText(forecastText, string.IsNullOrWhiteSpace(session.Data.weather.lastForecastMessage) ? "Forecast: -" : session.Data.weather.lastForecastMessage);

            List<TimedTaskData> tasks = session.WorldMapSystem.GetWorldTasks();
            SetText(messageText, tasks.Count == 0 ? session.WorldMapSystem.LastMessage : FormatTask(tasks[0]));
            if (collectTaskButton != null)
            {
                collectTaskButton.interactable = tasks.Exists(task => task.isComplete);
            }

            EnsureRows(session.WorldMapSystem.Catalog.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                bool active = i < session.WorldMapSystem.Catalog.Count;
                rows[i].gameObject.SetActive(active);
                if (active)
                {
                    rows[i].Bind(session, session.WorldMapSystem.Catalog[i]);
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
            session.WorldMapSystem.WorldMapChanged += Refresh;
            session.WorldMapSystem.MessageRaised += ShowMessage;
            session.DailyActionSystem.DailyActionsChanged += Refresh;
            session.ResourceSystem.ResourcesChanged += Refresh;
            session.WeatherSystem.WeatherChanged += Refresh;
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

        private void AskFortuneTeller()
        {
            if (session?.FortuneTellerSystem == null)
            {
                return;
            }

            session.FortuneTellerSystem.AskForRainForecast();
            ShowMessage(session.FortuneTellerSystem.LastMessage);
            Refresh();
        }

        private void CollectTask()
        {
            session?.WorldMapSystem.CollectFirstCompletedWorldTask();
        }

        private void ShowMessage(string message)
        {
            SetText(messageText, message);
        }

        private string FormatTask(TimedTaskData task)
        {
            WorldMapNodeRuntimeDefinition node = session.WorldMapSystem.GetNode(task.definitionId);
            string nodeName = node == null ? task.definitionId : node.DisplayName;
            return task.isComplete
                ? $"{nodeName}: ready to collect"
                : $"{nodeName}: {FormatRemaining(session.WorldMapSystem.GetTimeRemaining(task))}";
        }

        private static string FormatRemaining(System.TimeSpan remaining)
        {
            if (remaining <= System.TimeSpan.Zero)
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
    }
}

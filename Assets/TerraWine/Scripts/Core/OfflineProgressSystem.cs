using System;
using TerraWine.Data;

namespace TerraWine.Core
{
    public class OfflineProgressSystem : IGameSystem
    {
        private GameSession session;
        private GameData data;

        public TimeSpan LastElapsed { get; private set; }

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
        }

        public void ApplyOfflineProgress()
        {
            LastElapsed = session.TimeSystem.GetElapsedSince(data.lastSavedAtUtc);
            if (LastElapsed <= TimeSpan.Zero)
            {
                return;
            }

            int completedTasks = 0;
            DateTime now = session.TimeSystem.UtcNow;

            foreach (TimedTaskData task in data.timedTasks)
            {
                if (task.isComplete)
                {
                    continue;
                }

                if (session.TimeSystem.TryParseSaveTime(task.completesAtUtc, out DateTime completesAt) && completesAt <= now)
                {
                    task.isComplete = true;
                    completedTasks++;
                }
            }

            session.VineyardSystem?.UpdateGrowth(now);
            bool dailyRefreshed = session.DailyActionSystem.RefreshIfNeeded();
            data.offlineNotifications.Clear();
            data.offlineNotifications.Add($"Elapsed offline time: {FormatElapsed(LastElapsed)}.");
            if (completedTasks > 0)
            {
                data.offlineNotifications.Add($"{completedTasks} timed task(s) completed while you were away.");
            }

            if (dailyRefreshed)
            {
                data.offlineNotifications.Add("Daily world-map actions refreshed.");
            }
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed.TotalDays >= 1)
            {
                return $"{(int)elapsed.TotalDays}d {elapsed.Hours}h";
            }

            if (elapsed.TotalHours >= 1)
            {
                return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
            }

            return $"{Math.Max(1, elapsed.Minutes)}m";
        }
    }
}

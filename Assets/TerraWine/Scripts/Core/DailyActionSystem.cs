using System;
using TerraWine.Data;

namespace TerraWine.Core
{
    public class DailyActionSystem : IGameSystem
    {
        private static readonly TimeSpan RefreshTimeOfDay = new TimeSpan(8, 0, 0);
        private GameSession session;
        private GameData data;

        public event Action DailyActionsChanged;

        public int ActionsRemaining => data.dailyActions.actionsRemaining;
        public int MaxActions => data.dailyActions.maxActions;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            if (string.IsNullOrEmpty(data.dailyActions.lastRefreshAtUtc))
            {
                data.dailyActions.lastRefreshAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow);
            }
        }

        public bool SpendAction(int amount = 1)
        {
            if (amount <= 0 || data.dailyActions.actionsRemaining < amount)
            {
                return false;
            }

            data.dailyActions.actionsRemaining -= amount;
            DailyActionsChanged?.Invoke();
            return true;
        }

        public bool RefreshIfNeeded()
        {
            DateTime now = session.TimeSystem.UtcNow.ToLocalTime();
            if (!session.TimeSystem.TryParseSaveTime(data.dailyActions.lastRefreshAtUtc, out DateTime lastRefreshUtc))
            {
                Refresh(now.ToUniversalTime(), 0);
                return true;
            }

            DateTime last = lastRefreshUtc.ToLocalTime();
            DateTime nextRefresh = GetNextRefreshAfter(last);
            if (now < nextRefresh)
            {
                return false;
            }

            int refreshes = 0;
            while (nextRefresh <= now)
            {
                refreshes++;
                nextRefresh = nextRefresh.AddDays(1);
            }

            Refresh(nextRefresh.AddDays(-1).ToUniversalTime(), refreshes);
            return true;
        }

        private void Refresh(DateTime refreshUtc, int calendarDaysToAdvance)
        {
            data.dailyActions.actionsRemaining = data.dailyActions.maxActions;
            data.dailyActions.lastRefreshAtUtc = session.TimeSystem.ToSaveString(refreshUtc);
            if (calendarDaysToAdvance > 0)
            {
                session.CalendarSystem.AdvanceDays(calendarDaysToAdvance);
            }

            DailyActionsChanged?.Invoke();
        }

        private static DateTime GetNextRefreshAfter(DateTime localTime)
        {
            DateTime candidate = localTime.Date.Add(RefreshTimeOfDay);
            return localTime < candidate ? candidate : candidate.AddDays(1);
        }
    }
}

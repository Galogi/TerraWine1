using System;
using TerraWine.Data;

namespace TerraWine.Core
{
    public class CalendarSystem : IGameSystem
    {
        private const int DaysPerYear = 28;
        private GameData data;

        public event Action CalendarChanged;

        public int CurrentYear => data.calendar.currentYear;
        public int CurrentDay => data.calendar.currentDay;
        public int TotalYears => data.calendar.totalYears;
        public SeasonType CurrentSeason => data.seasonState.currentSeason;

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
            RecalculateSeason();
        }

        public void AdvanceDays(int days)
        {
            if (days <= 0)
            {
                return;
            }

            int absoluteDay = ((data.calendar.currentYear - 1) * DaysPerYear) + data.calendar.currentDay + days;
            int maxDay = data.calendar.totalYears * DaysPerYear;
            absoluteDay = Math.Min(absoluteDay, maxDay);

            data.calendar.currentYear = ((absoluteDay - 1) / DaysPerYear) + 1;
            data.calendar.currentDay = ((absoluteDay - 1) % DaysPerYear) + 1;
            RecalculateSeason();
            CalendarChanged?.Invoke();
        }

        private void RecalculateSeason()
        {
            int seasonIndex = Math.Max(0, (data.calendar.currentDay - 1) / 7);
            data.seasonState.currentSeason = (SeasonType)Math.Min(seasonIndex, 3);
            data.seasonState.seasonDefinitionId = data.seasonState.currentSeason.ToString().ToLowerInvariant();
        }
    }
}

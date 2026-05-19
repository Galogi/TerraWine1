using System;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.WorldMap
{
    public class WeatherSystem : IGameSystem
    {
        private readonly Random random = new Random();
        private GameSession session;
        private GameData data;

        public event Action WeatherChanged;

        public WeatherType CurrentWeather => data.weather.currentWeather;
        public WeatherType TodayForecast => data.weather.todayForecast;
        public WeatherType TomorrowForecast => data.weather.tomorrowForecast;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            if (string.IsNullOrWhiteSpace(data.weather.generatedAtUtc))
            {
                GenerateDailyWeather();
            }
            else
            {
                ApplyRainRewardIfNeeded();
            }
        }

        public void GenerateDailyWeatherIfNeeded()
        {
            if (!session.TimeSystem.TryParseSaveTime(data.weather.generatedAtUtc, out DateTime generatedAt))
            {
                GenerateDailyWeather();
                return;
            }

            if (generatedAt.ToLocalTime().Date < session.TimeSystem.UtcNow.ToLocalTime().Date)
            {
                GenerateDailyWeather();
            }
        }

        public void GenerateDailyWeather()
        {
            data.weather.currentWeather = data.weather.todayForecast;
            data.weather.todayForecast = RollWeather();
            data.weather.tomorrowForecast = RollWeather();
            data.weather.generatedAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow);
            data.weather.rainRewardAppliedToday = false;
            ApplyRainRewardIfNeeded();
            WeatherChanged?.Invoke();
        }

        public void ForceWeather(WeatherType weatherType)
        {
            data.weather.currentWeather = weatherType;
            data.weather.todayForecast = weatherType;
            data.weather.generatedAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow);
            data.weather.rainRewardAppliedToday = false;
            ApplyRainRewardIfNeeded();
            WeatherChanged?.Invoke();
        }

        private WeatherType RollWeather()
        {
            return random.NextDouble() < 0.3 ? WeatherType.Rain : WeatherType.Sunny;
        }

        private void ApplyRainRewardIfNeeded()
        {
            if (data.weather.currentWeather != WeatherType.Rain || data.weather.rainRewardAppliedToday)
            {
                return;
            }

            session.ResourceSystem.Add(ResourceType.Water, 5);
            data.weather.rainRewardAppliedToday = true;
        }
    }
}

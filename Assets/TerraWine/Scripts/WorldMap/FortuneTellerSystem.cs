using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.WorldMap
{
    public class FortuneTellerSystem : IGameSystem
    {
        private const int ForecastPrice = 15;

        private GameSession session;
        private GameData data;

        public string LastMessage { get; private set; } = string.Empty;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
        }

        public bool AskForRainForecast()
        {
            if (!session.ResourceSystem.Spend(ResourceType.Money, ForecastPrice))
            {
                LastMessage = "Not enough money for the fortune teller.";
                data.weather.lastForecastMessage = LastMessage;
                return false;
            }

            LastMessage = $"Forecast: today {session.WeatherSystem.TodayForecast}, tomorrow {session.WeatherSystem.TomorrowForecast}.";
            data.weather.lastForecastMessage = LastMessage;
            return true;
        }
    }
}

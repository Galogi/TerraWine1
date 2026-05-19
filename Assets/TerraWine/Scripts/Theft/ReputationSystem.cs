using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Theft
{
    public class ReputationSystem : IGameSystem
    {
        private GameData data;

        public int Reputation => data.player.reputation;

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
        }

        public void AddReputation(int amount)
        {
            data.player.reputation += amount;
        }
    }
}

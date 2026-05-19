using System;
using TerraWine.Data;

namespace TerraWine.Core
{
    public interface ISaveSystem
    {
        bool HasSave();
        GameData LoadGame();
        void SaveGame(GameData data);
        void DeleteSave();
    }

    public interface ITimeSystem
    {
        DateTime UtcNow { get; }
        TimeSpan GetElapsedSince(string savedUtc);
        string ToSaveString(DateTime time);
        bool TryParseSaveTime(string savedUtc, out DateTime time);
    }

    public interface IGameSystem
    {
        void Initialize(GameSession session, GameData data);
    }
}

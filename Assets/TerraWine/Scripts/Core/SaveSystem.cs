using System.IO;
using TerraWine.Data;
using UnityEngine;

namespace TerraWine.Core
{
    public class SaveSystem : ISaveSystem
    {
        private const string SaveFileName = "terrawine_save.json";
        private readonly string savePath;

        public SaveSystem()
        {
            savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        public bool HasSave()
        {
            return File.Exists(savePath);
        }

        public GameData LoadGame()
        {
            if (!HasSave())
            {
                return null;
            }

            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<GameData>(json);
        }

        public void SaveGame(GameData data)
        {
            string directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"TerraWine saved to {savePath}");
        }

        public void DeleteSave()
        {
            if (HasSave())
            {
                File.Delete(savePath);
            }
        }
    }
}

using UnityEngine;

namespace TerraWine.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        public static GameSession Session { get; private set; }

        [SerializeField] private bool autoLoadOrCreateGame = true;

        private void Awake()
        {
            if (Session != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Session = new GameSession();
            Session.Initialize();

            if (autoLoadOrCreateGame)
            {
                if (!Session.LoadGame())
                {
                    Session.StartNewGame();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Session?.SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            Session?.SaveGame();
        }
    }
}

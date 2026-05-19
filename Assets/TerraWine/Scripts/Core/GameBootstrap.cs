using UnityEngine;

namespace TerraWine.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        public static GameSession Session { get; private set; }

        [SerializeField] private bool autoLoadOrCreateGame = true;
        [SerializeField] private float vineyardTickSeconds = 1f;

        private float nextVineyardTickTime;

        private void Awake()
        {
            if (Session != null)
            {
                Destroy(gameObject);
                return;
            }

            if (transform.parent != null)
            {
                transform.SetParent(null);
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

        private void Update()
        {
            if (Session?.VineyardSystem == null || Time.unscaledTime < nextVineyardTickTime)
            {
                return;
            }

            nextVineyardTickTime = Time.unscaledTime + vineyardTickSeconds;
            Session.VineyardSystem.UpdateGrowth();
        }

        private void OnApplicationQuit()
        {
            Session?.SaveGame();
        }
    }
}

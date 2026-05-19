using TerraWine.Core;
using UnityEngine;

namespace TerraWine.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public void OnContinueClicked()
        {
            if (GameBootstrap.Session == null)
            {
                Debug.LogError("GameSession is missing.");
                return;
            }

            if (!GameBootstrap.Session.LoadGame())
            {
                GameBootstrap.Session.StartNewGame();
            }
        }

        public void OnNewGameClicked()
        {
            if (GameBootstrap.Session == null)
            {
                Debug.LogError("GameSession is missing.");
                return;
            }

            GameBootstrap.Session.StartNewGame();
        }

        public void OnSaveClicked()
        {
            GameBootstrap.Session?.SaveGame();
        }

        public void OnExitClicked()
        {
            GameBootstrap.Session?.SaveGame();
            Application.Quit();
        }
    }
}

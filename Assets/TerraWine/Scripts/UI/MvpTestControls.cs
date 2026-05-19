using TerraWine.Core;
using UnityEngine;

namespace TerraWine.UI
{
    public class MvpTestControls : MonoBehaviour
    {
        public void StartNewGame()
        {
            if (GameBootstrap.Session == null)
            {
                Debug.LogWarning("Cannot start a new game because GameSession is missing.");
                return;
            }

            GameBootstrap.Session.StartNewGame();
        }

        public void SaveGame()
        {
            GameBootstrap.Session?.SaveGame();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraWine.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadWorldMap()
        {
            SceneManager.LoadScene("WorldMap");
        }

        public void LoadBootstrap()
        {
            SceneManager.LoadScene("Bootstrap");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}

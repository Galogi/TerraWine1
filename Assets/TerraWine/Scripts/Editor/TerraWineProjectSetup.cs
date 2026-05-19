using System.Collections.Generic;
using System.IO;
using TerraWine.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraWine.EditorTools
{
    public static class TerraWineProjectSetup
    {
        private static readonly string[] SceneNames =
        {
            "Bootstrap",
            "MainMenu",
            "CreateWinery",
            "WorldMap",
            "WineryExterior",
            "WineryInterior",
            "BarrelCellar",
            "Competition",
            "EndGame"
        };

        [MenuItem("TerraWine/Setup/Create Placeholder Scenes")]
        public static void CreatePlaceholderScenes()
        {
            Directory.CreateDirectory("Assets/TerraWine/Scenes");

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in SceneNames)
            {
                string path = $"Assets/TerraWine/Scenes/{sceneName}.unity";
                if (!File.Exists(path))
                {
                    Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    CreateRootObjects(sceneName);
                    EditorSceneManager.SaveScene(scene, path);
                }

                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("TerraWine placeholder scenes created and added to build settings.");
        }

        private static void CreateRootObjects(string sceneName)
        {
            GameObject root = new GameObject($"{sceneName}Root");

            if (sceneName == "Bootstrap")
            {
                GameObject bootstrap = new GameObject("GameBootstrap");
                bootstrap.AddComponent<GameBootstrap>();
            }

            GameObject marker = new GameObject("Placeholder");
            marker.transform.SetParent(root.transform);
            TextMesh text = marker.AddComponent<TextMesh>();
            text.text = sceneName;
            text.characterSize = 0.4f;
            text.anchor = TextAnchor.MiddleCenter;
            marker.transform.position = Vector3.zero;
        }
    }
}

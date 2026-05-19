using System.Collections.Generic;
using System.IO;
using TerraWine.Core;
using TerraWine.Testing;
using TerraWine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TerraWine.EditorTools
{
    public static class TerraWinePlayableSceneBuilder
    {
        private const string BootstrapScenePath = "Assets/TerraWine/Scenes/Bootstrap.unity";

        [MenuItem("TerraWine/Setup/Create Playable MVP Bootstrap")]
        public static void CreatePlayableBootstrap()
        {
            Directory.CreateDirectory("Assets/TerraWine/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            CreateCamera();
            CreateEventSystem();
            CreateCanvas();

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
            EnsureBootstrapFirstInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("TerraWine playable MVP Bootstrap scene created.");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.11f, 0.10f);
            cameraObject.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static void CreateCanvas()
        {
            GameObject canvasObject = new GameObject("MVP Test Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject background = CreateUiObject("Background", canvasObject.transform);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.10f, 0.13f, 0.11f);
            Stretch(background);

            CreateHud(canvasObject.transform);

            GameObject body = CreateUiObject("Body", canvasObject.transform);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 0.92f);
            bodyRect.offsetMin = new Vector2(18f, 18f);
            bodyRect.offsetMax = new Vector2(-18f, -8f);
            HorizontalLayoutGroup bodyLayout = body.AddComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 14f;
            bodyLayout.childControlHeight = true;
            bodyLayout.childControlWidth = true;
            bodyLayout.childForceExpandHeight = true;
            bodyLayout.childForceExpandWidth = true;

            GameObject leftColumn = CreateColumn(body.transform, 0.8f);
            CreateVineyardPanel(leftColumn.transform);
            CreateStoragePanel(leftColumn.transform);
            CreateTheftPanel(leftColumn.transform);

            GameObject middleColumn = CreateColumn(body.transform, 1.05f);
            CreateWorldMapPanel(middleColumn.transform);
            CreateProductionPanel(middleColumn.transform);

            GameObject rightColumn = CreateColumn(body.transform, 0.8f);
            CreateShopPanel(rightColumn.transform);
            CreateBarrelPanel(rightColumn.transform);
        }

        private static void CreateHud(Transform parent)
        {
            GameObject hud = CreatePanel(parent, "HUD", new Color(0.15f, 0.21f, 0.17f));
            RectTransform rect = hud.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.92f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(18f, 4f);
            rect.offsetMax = new Vector2(-18f, -12f);

            HorizontalLayoutGroup layout = hud.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 10, 10);
            layout.spacing = 18f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            HUDController hudController = hud.AddComponent<HUDController>();
            TMP_Text money = AddHudMetric(hud.transform, "Money");
            TMP_Text water = AddHudMetric(hud.transform, "Water");
            TMP_Text wood = AddHudMetric(hud.transform, "Wood");
            TMP_Text metal = AddHudMetric(hud.transform, "Metal");
            TMP_Text reputation = AddHudMetric(hud.transform, "Reputation");
            TMP_Text year = AddHudMetric(hud.transform, "Year");
            TMP_Text season = AddHudMetric(hud.transform, "Season");
            TMP_Text actions = AddHudMetric(hud.transform, "Actions");
            MvpTestControls controls = hud.AddComponent<MvpTestControls>();
            TerraWineMVPIntegrationTestRunner testRunner = hud.AddComponent<TerraWineMVPIntegrationTestRunner>();
            Button newGame = AddButton(hud.transform, "New Game", new Color(0.42f, 0.30f, 0.20f));
            Button save = AddButton(hud.transform, "Save", new Color(0.25f, 0.38f, 0.28f));
            Button runTest = AddButton(hud.transform, "Run MVP Integration Test", new Color(0.34f, 0.30f, 0.54f));
            UnityEventTools.AddPersistentListener(newGame.onClick, controls.StartNewGame);
            UnityEventTools.AddPersistentListener(save.onClick, controls.SaveGame);
            UnityEventTools.AddPersistentListener(runTest.onClick, testRunner.RunFullTest);

            SetObject(hudController, "moneyText", money);
            SetObject(hudController, "waterText", water);
            SetObject(hudController, "woodText", wood);
            SetObject(hudController, "metalText", metal);
            SetObject(hudController, "reputationText", reputation);
            SetObject(hudController, "yearText", year);
            SetObject(hudController, "seasonText", season);
            SetObject(hudController, "dailyActionsText", actions);
        }

        private static void CreateVineyardPanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "Vineyard", 1.2f);
            VineyardPanelController controller = panel.AddComponent<VineyardPanelController>();
            TMP_Text tutorial = AddText(panel.transform, "TutorialText", "Plant your first grape seed.", 20, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text message = AddText(panel.transform, "VineyardMessage", "", 18, FontStyles.Normal, TextAlignmentOptions.Left);
            AddHeaderRow(panel.transform, "Plot", "State", "Seed", "Grape", "Time", "Water", "Actions");
            Transform rowsRoot = CreateRowsRoot(panel.transform, "VineyardRows");
            VineyardPlotRowController row = CreateVineyardRow(rowsRoot, "VineyardRowTemplate");
            row.gameObject.SetActive(false);

            SetObject(controller, "messageText", message);
            SetObject(controller, "tutorialText", tutorial);
            SetObject(controller, "rowsRoot", rowsRoot);
            SetObject(controller, "rowPrefab", row);
        }

        private static VineyardPlotRowController CreateVineyardRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            VineyardPlotRowController row = rowObject.AddComponent<VineyardPlotRowController>();
            TMP_Text plot = AddCell(rowObject.transform, "Plot");
            TMP_Text state = AddCell(rowObject.transform, "State");
            TMP_Text seed = AddCell(rowObject.transform, "Seed");
            TMP_Text grape = AddCell(rowObject.transform, "Grape");
            TMP_Text time = AddCell(rowObject.transform, "Time");
            TMP_Text water = AddCell(rowObject.transform, "Water");
            GameObject actions = CreateActionGroup(rowObject.transform, "Actions");
            Button plant = AddButton(actions.transform, "Plant", new Color(0.31f, 0.48f, 0.27f));
            Button waterButton = AddButton(actions.transform, "Water", new Color(0.25f, 0.42f, 0.62f));
            Button harvest = AddButton(actions.transform, "Harvest", new Color(0.55f, 0.39f, 0.20f));

            SetObject(row, "plotNameText", plot);
            SetObject(row, "stateText", state);
            SetObject(row, "seedText", seed);
            SetObject(row, "grapeText", grape);
            SetObject(row, "timeRemainingText", time);
            SetObject(row, "waterNeededText", water);
            SetObject(row, "plantButton", plant);
            SetObject(row, "waterButton", waterButton);
            SetObject(row, "harvestButton", harvest);
            return row;
        }

        private static void CreateProductionPanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "Production", 1f);
            ProductionPanelController controller = panel.AddComponent<ProductionPanelController>();
            TMP_Text message = AddText(panel.transform, "ProductionMessage", "", 18, FontStyles.Normal, TextAlignmentOptions.Left);

            AddText(panel.transform, "RecipeTitle", "Recipes", 24, FontStyles.Bold, TextAlignmentOptions.Left);
            AddHeaderRow(panel.transform, "Recipe", "Needs", "Time", "Quality", "State", "Action");
            Transform recipeRows = CreateRowsRoot(panel.transform, "RecipeRows");
            ProductionRecipeRowController recipeRow = CreateRecipeRow(recipeRows, "RecipeRowTemplate");
            recipeRow.gameObject.SetActive(false);

            AddText(panel.transform, "TaskTitle", "Active Production", 24, FontStyles.Bold, TextAlignmentOptions.Left);
            AddHeaderRow(panel.transform, "Wine", "State", "Time", "Action");
            Transform taskRows = CreateRowsRoot(panel.transform, "ProductionTaskRows");
            ProductionTaskRowController taskRow = CreateTaskRow(taskRows, "TaskRowTemplate");
            taskRow.gameObject.SetActive(false);

            SetObject(controller, "messageText", message);
            SetObject(controller, "recipeRowsRoot", recipeRows);
            SetObject(controller, "recipeRowPrefab", recipeRow);
            SetObject(controller, "taskRowsRoot", taskRows);
            SetObject(controller, "taskRowPrefab", taskRow);
        }

        private static ProductionRecipeRowController CreateRecipeRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            ProductionRecipeRowController row = rowObject.AddComponent<ProductionRecipeRowController>();
            TMP_Text recipe = AddCell(rowObject.transform, "Recipe");
            TMP_Text needs = AddCell(rowObject.transform, "Needs");
            TMP_Text time = AddCell(rowObject.transform, "Time");
            TMP_Text quality = AddCell(rowObject.transform, "Quality");
            TMP_Text state = AddCell(rowObject.transform, "State");
            Button start = AddButton(rowObject.transform, "Start Production", new Color(0.43f, 0.25f, 0.50f));

            SetObject(row, "recipeNameText", recipe);
            SetObject(row, "requiredGrapesText", needs);
            SetObject(row, "productionTimeText", time);
            SetObject(row, "expectedQualityText", quality);
            SetObject(row, "availabilityText", state);
            SetObject(row, "startProductionButton", start);
            return row;
        }

        private static ProductionTaskRowController CreateTaskRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            ProductionTaskRowController row = rowObject.AddComponent<ProductionTaskRowController>();
            TMP_Text recipe = AddCell(rowObject.transform, "Wine");
            TMP_Text state = AddCell(rowObject.transform, "State");
            TMP_Text time = AddCell(rowObject.transform, "Time");
            Button collect = AddButton(rowObject.transform, "Collect Finished Wine", new Color(0.39f, 0.48f, 0.25f));

            SetObject(row, "recipeNameText", recipe);
            SetObject(row, "stateText", state);
            SetObject(row, "timeRemainingText", time);
            SetObject(row, "collectButton", collect);
            return row;
        }

        private static void CreateWorldMapPanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "World Map", 1f);
            WorldMapPanelController controller = panel.AddComponent<WorldMapPanelController>();
            TMP_Text actions = AddText(panel.transform, "WorldActions", "Actions: -", 18, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text weather = AddText(panel.transform, "Weather", "Weather: -", 18, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text forecast = AddText(panel.transform, "Forecast", "Forecast: -", 17, FontStyles.Normal, TextAlignmentOptions.Left);
            TMP_Text message = AddText(panel.transform, "WorldMapMessage", "", 17, FontStyles.Normal, TextAlignmentOptions.Left);
            GameObject buttons = CreateActionGroup(panel.transform, "WorldMapButtons");
            Button fortune = AddButton(buttons.transform, "Ask Fortune Teller", new Color(0.38f, 0.31f, 0.52f));
            Button collect = AddButton(buttons.transform, "Collect Completed World Task", new Color(0.36f, 0.45f, 0.28f));
            AddHeaderRow(panel.transform, "Node", "Type", "Distance", "Reward", "Cost", "Action");
            Transform rowsRoot = CreateRowsRoot(panel.transform, "WorldMapRows");
            WorldMapNodeRowController row = CreateWorldMapRow(rowsRoot, "WorldMapRowTemplate");
            row.gameObject.SetActive(false);

            SetObject(controller, "actionsText", actions);
            SetObject(controller, "weatherText", weather);
            SetObject(controller, "forecastText", forecast);
            SetObject(controller, "messageText", message);
            SetObject(controller, "fortuneButton", fortune);
            SetObject(controller, "collectTaskButton", collect);
            SetObject(controller, "rowsRoot", rowsRoot);
            SetObject(controller, "rowPrefab", row);
        }

        private static WorldMapNodeRowController CreateWorldMapRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            WorldMapNodeRowController row = rowObject.AddComponent<WorldMapNodeRowController>();
            TMP_Text nodeName = AddCell(rowObject.transform, "Node");
            TMP_Text type = AddCell(rowObject.transform, "Type");
            TMP_Text distance = AddCell(rowObject.transform, "Distance");
            TMP_Text reward = AddCell(rowObject.transform, "Reward");
            TMP_Text cost = AddCell(rowObject.transform, "Cost");
            Button action = AddButton(rowObject.transform, "Gather", new Color(0.29f, 0.43f, 0.35f));

            SetObject(row, "nameText", nodeName);
            SetObject(row, "typeText", type);
            SetObject(row, "distanceText", distance);
            SetObject(row, "rewardText", reward);
            SetObject(row, "actionCostText", cost);
            SetObject(row, "actionButton", action);
            return row;
        }

        private static void CreateStoragePanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "Storage", 0.8f);
            StoragePanelController controller = panel.AddComponent<StoragePanelController>();
            TMP_Text capacity = AddText(panel.transform, "Capacity", "0/0", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text grapes = AddBoxText(panel.transform, "Grapes", "Grapes\n-");
            TMP_Text bottles = AddBoxText(panel.transform, "WineBottles", "Wine Bottles\n-");
            TMP_Text items = AddBoxText(panel.transform, "Items", "Items\n-");

            SetObject(controller, "capacityText", capacity);
            SetObject(controller, "grapesText", grapes);
            SetObject(controller, "wineBottlesText", bottles);
            SetObject(controller, "itemsText", items);
        }

        private static void CreateShopPanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "Shop", 1f);
            ShopPanelController controller = panel.AddComponent<ShopPanelController>();
            TMP_Text money = AddText(panel.transform, "ShopMoney", "Money: -", 20, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text message = AddText(panel.transform, "ShopMessage", "", 17, FontStyles.Normal, TextAlignmentOptions.Left);
            AddHeaderRow(panel.transform, "Item", "Category", "Price", "Description", "Action");
            Transform rowsRoot = CreateRowsRoot(panel.transform, "ShopRows");
            ShopItemRowController row = CreateShopRow(rowsRoot, "ShopRowTemplate");
            row.gameObject.SetActive(false);

            SetObject(controller, "moneyText", money);
            SetObject(controller, "messageText", message);
            SetObject(controller, "rowsRoot", rowsRoot);
            SetObject(controller, "rowPrefab", row);
        }

        private static ShopItemRowController CreateShopRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            ShopItemRowController row = rowObject.AddComponent<ShopItemRowController>();
            TMP_Text itemName = AddCell(rowObject.transform, "Item");
            TMP_Text category = AddCell(rowObject.transform, "Category");
            TMP_Text price = AddCell(rowObject.transform, "Price");
            TMP_Text description = AddCell(rowObject.transform, "Description");
            Button buy = AddButton(rowObject.transform, "Buy", new Color(0.32f, 0.43f, 0.28f));

            SetObject(row, "nameText", itemName);
            SetObject(row, "categoryText", category);
            SetObject(row, "priceText", price);
            SetObject(row, "descriptionText", description);
            SetObject(row, "buyButton", buy);
            return row;
        }

        private static void CreateTheftPanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "Theft", 1f);
            TheftPanelController controller = panel.AddComponent<TheftPanelController>();
            TMP_Text status = AddText(panel.transform, "TheftStatus", "Reputation: -", 17, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text message = AddText(panel.transform, "TheftMessage", "", 17, FontStyles.Normal, TextAlignmentOptions.Left);
            TMP_InputField input = AddInput(panel.transform, "PasswordGuess", "Password guess");
            AddHeaderRow(panel.transform, "Bot", "Vault", "Recipes", "Defense", "Action");
            Transform rowsRoot = CreateRowsRoot(panel.transform, "TheftBotRows");
            TheftBotRowController row = CreateTheftBotRow(rowsRoot, "TheftBotRowTemplate");
            row.gameObject.SetActive(false);
            GameObject actions = CreateActionGroup(panel.transform, "TheftActions");
            Button steal = AddButton(actions.transform, "Try Steal Recipe", new Color(0.44f, 0.25f, 0.30f));
            Button simulate = AddButton(actions.transform, "Simulate Player Recipe Stolen", new Color(0.42f, 0.30f, 0.20f));
            Button returnRecipe = AddButton(actions.transform, "Return Stolen Recipe", new Color(0.28f, 0.42f, 0.28f));

            SetObject(controller, "statusText", status);
            SetObject(controller, "messageText", message);
            SetObject(controller, "passwordInput", input);
            SetObject(controller, "botRowsRoot", rowsRoot);
            SetObject(controller, "botRowPrefab", row);
            SetObject(controller, "stealRecipeButton", steal);
            SetObject(controller, "simulatePlayerRecipeStolenButton", simulate);
            SetObject(controller, "returnStolenRecipeButton", returnRecipe);
        }

        private static TheftBotRowController CreateTheftBotRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            TheftBotRowController row = rowObject.AddComponent<TheftBotRowController>();
            TMP_Text botName = AddCell(rowObject.transform, "Bot");
            TMP_Text vault = AddCell(rowObject.transform, "Vault");
            TMP_Text recipes = AddCell(rowObject.transform, "Recipes");
            TMP_Text defense = AddCell(rowObject.transform, "Defense");
            Button select = AddButton(rowObject.transform, "Select", new Color(0.32f, 0.36f, 0.50f));

            SetObject(row, "nameText", botName);
            SetObject(row, "vaultText", vault);
            SetObject(row, "recipesText", recipes);
            SetObject(row, "defenseText", defense);
            SetObject(row, "selectButton", select);
            return row;
        }

        private static void CreateBarrelPanel(Transform parent)
        {
            GameObject panel = CreateSection(parent, "Barrel Cellar", 1f);
            BarrelPanelController controller = panel.AddComponent<BarrelPanelController>();
            TMP_Text message = AddText(panel.transform, "BarrelMessage", "", 18, FontStyles.Normal, TextAlignmentOptions.Left);
            AddHeaderRow(panel.transform, "Barrel", "State", "Wine", "Time", "Q", "Price", "Score", "Actions");
            Transform rowsRoot = CreateRowsRoot(panel.transform, "BarrelRows");
            BarrelRowController row = CreateBarrelRow(rowsRoot, "BarrelRowTemplate");
            row.gameObject.SetActive(false);

            SetObject(controller, "messageText", message);
            SetObject(controller, "rowsRoot", rowsRoot);
            SetObject(controller, "rowPrefab", row);
        }

        private static BarrelRowController CreateBarrelRow(Transform parent, string name)
        {
            GameObject rowObject = CreateRow(parent, name);
            BarrelRowController row = rowObject.AddComponent<BarrelRowController>();
            TMP_Text barrelName = AddCell(rowObject.transform, "Barrel");
            TMP_Text state = AddCell(rowObject.transform, "State");
            TMP_Text wine = AddCell(rowObject.transform, "Wine");
            TMP_Text time = AddCell(rowObject.transform, "Time");
            TMP_Text quality = AddCell(rowObject.transform, "Quality");
            TMP_Text price = AddCell(rowObject.transform, "Price");
            TMP_Text score = AddCell(rowObject.transform, "Score");
            GameObject actions = CreateActionGroup(rowObject.transform, "Actions");
            Button start = AddButton(actions.transform, "Start Aging", new Color(0.45f, 0.30f, 0.18f));
            Button collect = AddButton(actions.transform, "Collect Aged Wine", new Color(0.42f, 0.48f, 0.25f));

            SetObject(row, "barrelNameText", barrelName);
            SetObject(row, "stateText", state);
            SetObject(row, "wineInsideText", wine);
            SetObject(row, "timeRemainingText", time);
            SetObject(row, "qualityBonusText", quality);
            SetObject(row, "priceBonusText", price);
            SetObject(row, "competitionScoreBonusText", score);
            SetObject(row, "startAgingButton", start);
            SetObject(row, "collectButton", collect);
            return row;
        }

        private static GameObject CreateColumn(Transform parent, float flexibleWidth)
        {
            GameObject column = CreateUiObject("Column", parent);
            VerticalLayoutGroup layout = column.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            LayoutElement element = column.AddComponent<LayoutElement>();
            element.flexibleWidth = flexibleWidth;
            return column;
        }

        private static GameObject CreateSection(Transform parent, string title, float flexibleHeight)
        {
            GameObject panel = CreatePanel(parent, title, new Color(0.17f, 0.20f, 0.18f));
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            LayoutElement element = panel.AddComponent<LayoutElement>();
            element.flexibleHeight = flexibleHeight;
            AddText(panel.transform, $"{title}Title", title, 28, FontStyles.Bold, TextAlignmentOptions.Left);
            return panel;
        }

        private static Transform CreateRowsRoot(Transform parent, string name)
        {
            GameObject root = CreateUiObject(name, parent);
            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            LayoutElement element = root.AddComponent<LayoutElement>();
            element.flexibleHeight = 1f;
            return root.transform;
        }

        private static GameObject CreateRow(Transform parent, string name)
        {
            GameObject row = CreatePanel(parent, name, new Color(0.20f, 0.24f, 0.21f));
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            LayoutElement element = row.AddComponent<LayoutElement>();
            element.minHeight = 58f;
            return row;
        }

        private static GameObject CreateActionGroup(Transform parent, string name)
        {
            GameObject group = CreateUiObject(name, parent);
            HorizontalLayoutGroup layout = group.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            LayoutElement element = group.AddComponent<LayoutElement>();
            element.flexibleWidth = 2f;
            return group;
        }

        private static void AddHeaderRow(Transform parent, params string[] labels)
        {
            GameObject row = CreateUiObject("HeaderRow", parent);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            LayoutElement element = row.AddComponent<LayoutElement>();
            element.minHeight = 26f;
            foreach (string label in labels)
            {
                TMP_Text text = AddCell(row.transform, label);
                text.fontStyle = FontStyles.Bold;
                text.color = new Color(0.78f, 0.83f, 0.74f);
            }
        }

        private static TMP_Text AddHudMetric(Transform parent, string label)
        {
            TMP_Text text = AddText(parent, label, $"{label}: -", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            LayoutElement element = text.gameObject.AddComponent<LayoutElement>();
            element.flexibleWidth = 1f;
            return text;
        }

        private static TMP_Text AddCell(Transform parent, string name)
        {
            TMP_Text text = AddText(parent, name, "-", 16, FontStyles.Normal, TextAlignmentOptions.Left);
            LayoutElement element = text.gameObject.AddComponent<LayoutElement>();
            element.flexibleWidth = 1f;
            return text;
        }

        private static TMP_Text AddBoxText(Transform parent, string name, string value)
        {
            GameObject box = CreatePanel(parent, $"{name}Box", new Color(0.20f, 0.24f, 0.21f));
            LayoutElement boxElement = box.AddComponent<LayoutElement>();
            boxElement.minHeight = 88f;
            TMP_Text text = AddText(box.transform, name, value, 18, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            Stretch(text.gameObject, 10f);
            return text;
        }

        private static Button AddButton(Transform parent, string label, Color color)
        {
            GameObject buttonObject = CreateUiObject(label, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            LayoutElement element = buttonObject.AddComponent<LayoutElement>();
            element.minHeight = 36f;
            element.flexibleWidth = 1f;

            TMP_Text text = AddText(buttonObject.transform, "Label", label, 15, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(text.gameObject, 4f);
            return button;
        }

        private static TMP_InputField AddInput(Transform parent, string name, string placeholder)
        {
            GameObject inputObject = CreatePanel(parent, name, new Color(0.12f, 0.15f, 0.13f));
            LayoutElement element = inputObject.AddComponent<LayoutElement>();
            element.minHeight = 36f;
            TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
            TMP_Text text = AddText(inputObject.transform, "Text", "", 16, FontStyles.Normal, TextAlignmentOptions.Left);
            TMP_Text placeholderText = AddText(inputObject.transform, "Placeholder", placeholder, 16, FontStyles.Italic, TextAlignmentOptions.Left);
            placeholderText.color = new Color(0.60f, 0.64f, 0.58f);
            Stretch(text.gameObject, 8f);
            Stretch(placeholderText.gameObject, 8f);
            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private static TMP_Text AddText(Transform parent, string name, string value, int fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.93f, 0.91f, 0.83f);
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = CreateUiObject(name, parent);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(GameObject gameObject, float padding = 0f)
        {
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureBootstrapFirstInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true)
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == BootstrapScenePath)
                {
                    continue;
                }

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}

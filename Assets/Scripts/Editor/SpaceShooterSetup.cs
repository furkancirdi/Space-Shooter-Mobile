#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tek tikla ana menu sahnesini, oyun ici HUD/Game Over ekranini,
// dusman gemisi prefab'ini ve Build Settings'i kurar.
// Menu: Tools > Space Shooter > Setup Game
public static class SpaceShooterSetup
{
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string EnemyShipPrefabPath = "Assets/Prefabs/EnemyShip.prefab";
    private const string MeteorPrefabPath = "Assets/Prefabs/MeteorOne.prefab";
    private const string FontPath = "Assets/Sprites/Fonts/ethnocentric rg.otf";
    private const string BackgroundSpritePath = "Assets/Sprites/Backgrounds/BackgroundBase_1024x2048.png";
    private const string EnemySpritePath = "Assets/Sprites/Enemies/Enemies_128.png";
    private const string ExplosionSpritePath = "Assets/Sprites/Enemies/Explosion_128.png";

    private const string GeneratedTag = "[Generated]";

    private static Font font;

    [MenuItem("Tools/Space Shooter/Setup Game")]
    public static void SetupGame()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject shipPrefab = CreateEnemyShipPrefab();
        SetupGameScene(shipPrefab);
        CreateMenuScene();
        SetupBuildSettings();

        EditorSceneManager.OpenScene(MenuScenePath);
        EditorUtility.DisplayDialog("Space Shooter",
            "Kurulum tamamlandi!\n\nMainMenu sahnesi acildi. Play'e basip test edebilirsin.", "Tamam");
    }

    // ---------------- Enemy ship ----------------

    private static GameObject CreateEnemyShipPrefab()
    {
        GameObject meteor = AssetDatabase.LoadAssetAtPath<GameObject>(MeteorPrefabPath);

        GameObject go = new GameObject("EnemyShip");
        go.layer = meteor != null ? meteor.layer : 7;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(EnemySpritePath);
        if (meteor != null)
        {
            SpriteRenderer meteorSr = meteor.GetComponent<SpriteRenderer>();
            sr.sharedMaterial = meteorSr.sharedMaterial;
            sr.sortingLayerID = meteorSr.sortingLayerID;
            sr.sortingOrder = meteorSr.sortingOrder;
        }

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        EnemyShip ship = go.AddComponent<EnemyShip>();
        SerializedObject so = new SerializedObject(ship);
        so.FindProperty("health").floatValue = 4;
        so.FindProperty("rb").objectReferenceValue = rb;
        so.FindProperty("scoreValue").intValue = 25;
        so.FindProperty("dropChance").floatValue = 0.07f;
        so.FindProperty("hitFlashColor").colorValue = new Color(1f, 0.4f, 0.4f);
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(ExplosionSpritePath).OfType<Sprite>()
            .OrderBy(f => int.Parse(f.name.Substring(f.name.LastIndexOf('_') + 1))).ToArray();
        SerializedProperty framesProp = so.FindProperty("explosionFrames");
        framesProp.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++)
            framesProp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyShipPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ---------------- Game scene ----------------

    private static void SetupGameScene(GameObject shipPrefab)
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);
        RemoveGenerated(scene);

        EnsureEventSystem();

        // Dusman gemisi spawner'i (mevcut MeteorSpawner'i yeniden kullanir)
        GameObject spawnerGo = new GameObject("EnemyShipSpawner " + GeneratedTag);
        MeteorSpawner spawner = spawnerGo.AddComponent<MeteorSpawner>();
        SerializedObject spawnerSo = new SerializedObject(spawner);
        SerializedProperty arr = spawnerSo.FindProperty("meteor");
        arr.arraySize = 1;
        arr.GetArrayElementAtIndex(0).objectReferenceValue = shipPrefab;
        spawnerSo.FindProperty("spawnTime").floatValue = 3.5f;
        spawnerSo.FindProperty("randomRotationAndSize").boolValue = false;
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();

        // HUD
        GameObject canvas = CreateCanvas("GameCanvas " + GeneratedTag);

        Text scoreText = CreateText(canvas.transform, "ScoreText", "SCORE: 0", 48, TextAnchor.UpperLeft);
        SetRect(scoreText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -60), new Vector2(600, 80));

        Text bestText = CreateText(canvas.transform, "HighScoreText", "BEST: 0", 48, TextAnchor.UpperRight);
        SetRect(bestText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -60), new Vector2(600, 80));

        // Game Over paneli
        GameObject dim = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
        dim.transform.SetParent(canvas.transform, false);
        Stretch(dim.GetComponent<RectTransform>());
        dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(dim.transform, false);
        Image panelImg = panel.GetComponent<Image>();
        panelImg.sprite = UISprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = new Color(0.08f, 0.1f, 0.25f, 0.95f);
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 1000));

        Text title = CreateText(panel.transform, "Title", "GAME OVER", 80, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(800, 120));
        title.color = new Color(1f, 0.35f, 0.35f);

        Text finalScore = CreateText(panel.transform, "FinalScoreText", "SCORE: 0", 60, TextAnchor.MiddleCenter);
        SetRect(finalScore.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -290), new Vector2(800, 100));

        Text newBest = CreateText(panel.transform, "NewHighScoreText", "NEW HIGH SCORE!", 46, TextAnchor.MiddleCenter);
        SetRect(newBest.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -400), new Vector2(800, 80));
        newBest.color = new Color(1f, 0.85f, 0.2f);

        GameManager gm = new GameObject("GameManager " + GeneratedTag).AddComponent<GameManager>();

        Button restart = CreateButton(panel.transform, "RestartButton", "RESTART", new Vector2(0, -80));
        UnityEventTools.AddPersistentListener(restart.onClick, gm.Restart);
        Button menu = CreateButton(panel.transform, "MenuButton", "MENU", new Vector2(0, -260));
        UnityEventTools.AddPersistentListener(menu.onClick, gm.GoToMenu);

        SerializedObject gmSo = new SerializedObject(gm);
        gmSo.FindProperty("scoreText").objectReferenceValue = scoreText;
        gmSo.FindProperty("highScoreText").objectReferenceValue = bestText;
        gmSo.FindProperty("gameOverPanel").objectReferenceValue = dim;
        gmSo.FindProperty("finalScoreText").objectReferenceValue = finalScore;
        gmSo.FindProperty("newHighScoreText").objectReferenceValue = newBest;
        gmSo.ApplyModifiedPropertiesWithoutUndo();

        dim.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------- Menu scene ----------------

    private static void CreateMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.transform.position = new Vector3(0, 0, -10);

        EnsureEventSystem();

        GameObject canvas = CreateCanvas("MenuCanvas");

        CreateFullScreenImage(canvas.transform, "Background", LoadSprite(BackgroundSpritePath));

        Text title = CreateText(canvas.transform, "Title", "SPACE\nSHOOTER", 100, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 450), new Vector2(1000, 400));
        title.color = new Color(0.55f, 0.85f, 1f);

        Text best = CreateText(canvas.transform, "HighScoreText", "BEST SCORE: 0", 50, TextAnchor.MiddleCenter);
        SetRect(best.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1000, 100));
        best.color = new Color(1f, 0.85f, 0.2f);

        MainMenu menu = new GameObject("MainMenu").AddComponent<MainMenu>();

        Button start = CreateButton(canvas.transform, "StartButton", "START", new Vector2(0, -150));
        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        Button quit = CreateButton(canvas.transform, "QuitButton", "QUIT", new Vector2(0, -350));
        UnityEventTools.AddPersistentListener(quit.onClick, menu.QuitGame);

        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("gameSceneName").stringValue = System.IO.Path.GetFileNameWithoutExtension(GameScenePath);
        so.FindProperty("highScoreText").objectReferenceValue = best;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    private static void SetupBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };
        scenes.AddRange(EditorBuildSettings.scenes.Where(s => s.path != MenuScenePath && s.path != GameScenePath));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------------- Helpers ----------------

    private static void RemoveGenerated(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name.Contains(GeneratedTag)) Object.DestroyImmediate(root);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        go.GetComponent<Canvas>().sortingOrder = 10;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        return go;
    }

    private static Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(560, 150));

        Image img = go.GetComponent<Image>();
        img.sprite = UISprite();
        img.type = Image.Type.Sliced;
        img.color = new Color(0.25f, 0.5f, 0.95f);

        Text t = CreateText(go.transform, "Label", label, 56, TextAnchor.MiddleCenter);
        Stretch(t.rectTransform);
        return go.GetComponent<Button>();
    }

    private static void CreateFullScreenImage(Transform parent, string name, Sprite sprite)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>());
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
    }

    private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Sprite UISprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }
}
#endif

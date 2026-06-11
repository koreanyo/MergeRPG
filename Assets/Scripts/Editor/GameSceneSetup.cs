using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public static class GameSceneSetup
{
    [MenuItem("Slagma/Take Screenshot")]
    public static void TakeScreenshot()
    {
        var cam = Camera.main;
        if (cam == null) { Debug.LogError("Main Camera 없음"); return; }
        int w = 1080, h = 1920;
        var rt  = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        System.IO.File.WriteAllBytes("Assets/screenshot.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.Refresh();
        Debug.Log("[Slagma] screenshot.png 저장됨");
    }

    
[MenuItem("Slagma/Remove Missing Scripts")]
    public static void RemoveMissingScripts()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        int total = 0;
        foreach (var root in roots)
            total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
        Debug.Log($"[Slagma] Missing script {total}개 제거됨");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    
[MenuItem("Slagma/Setup GameScene")]
    public static void SetupScene()
    {
        // EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        if (Camera.main != null) canvas.worldCamera = Camera.main;
        canvas.planeDistance = 100f;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // BattleArea (상단 45%)
        var battleArea = CreatePanel(canvasGo.transform, "BattleArea",
            new Vector2(0, 0.45f), new Vector2(1, 1));

        // AllySlots (BattleArea 하단 40%)
        var allySlotsGo = CreatePanel(battleArea.transform, "AllySlots",
            new Vector2(0, 0), new Vector2(1, 0.4f));
        float[] allyX = { -270f, 0f, 270f };
        for (int i = 0; i < 3; i++)
            CreateSlot(allySlotsGo.transform, $"AllySlot_{i}", new Vector2(allyX[i], 0));

        // EnemySlots (BattleArea 상단 40%)
        var enemySlotsGo = CreatePanel(battleArea.transform, "EnemySlots",
            new Vector2(0, 0.6f), new Vector2(1, 1));
        float[] enemyX = { -270f, 0f, 270f };
        for (int i = 0; i < 3; i++)
            CreateSlot(enemySlotsGo.transform, $"EnemySlot_{i}", new Vector2(enemyX[i], 0));

        // MergeBoardArea (하단 45%)
        var mergeBoardArea = CreatePanel(canvasGo.transform, "MergeBoardArea",
            new Vector2(0, 0), new Vector2(1, 0.45f));

        // BoardParent (600x700 고정 크기, 중앙)
        var boardParentGo = new GameObject("BoardParent");
        boardParentGo.transform.SetParent(mergeBoardArea.transform, false);
        var boardRT = boardParentGo.AddComponent<RectTransform>();
        boardRT.anchorMin = new Vector2(0.5f, 0.5f);
        boardRT.anchorMax = new Vector2(0.5f, 0.5f);
        boardRT.sizeDelta = new Vector2(600, 700);
        boardRT.anchoredPosition = Vector2.zero;

        // UILayer (팝업용 최상단 레이어)
        var uiLayer = CreatePanel(canvasGo.transform, "UILayer",
            Vector2.zero, Vector2.one);
        uiLayer.GetComponent<RectTransform>().SetAsLastSibling();
        uiLayer.GetComponent<Image>().raycastTarget = false;

        // ---- Non-UI Manager GameObjects ----
        var gm = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        AddIfMissing<GameManager>(gm);

        var bmGo = new GameObject("BattleManager");
        AddIfMissing<BattleManager>(bmGo);

        var mbGo = new GameObject("MergeBoard");
        AddIfMissing<MergeBoard>(mbGo);

        var genGo = new GameObject("Generator");
        AddIfMissing<Generator>(genGo);
        AddIfMissing<GeneratorEnergy>(genGo);

        var pmGo = new GameObject("PopupManager");
        AddIfMissing<PopupManager>(pmGo);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        Debug.Log("[Slagma] GameScene setup complete!");
        Selection.activeGameObject = canvasGo;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;
        return go;
    }

    static void CreateSlot(Transform parent, string name, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(160, 160);
        rt.anchoredPosition = anchoredPos;
    }

    static void AddIfMissing<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null) go.AddComponent<T>();
    }
}

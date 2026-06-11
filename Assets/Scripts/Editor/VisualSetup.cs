using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GeneratorView 버튼 추가 + CharacterView 프리팹 시각화.
/// Slagma/Setup Visuals 메뉴 실행.
/// </summary>
public static class VisualSetup
{
    [MenuItem("Slagma/Setup Visuals")]
    public static void Run()
    {
        AddGeneratorViewButton();
        UpdateCellPrefabSize();
        UpdateCharacterViewPrefabs();

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Slagma] 시각 설정 완료!");
    }

    // ── GeneratorView 버튼 생성 ────────────────────────────────────
    static void AddGeneratorViewButton()
    {
        // 이미 있으면 스킵
        if (GameObject.Find("GeneratorButton") != null) return;

        var boardParent = GameObject.Find("BoardParent");
        if (boardParent == null) { Debug.LogError("BoardParent를 찾을 수 없음"); return; }

        // Generator 버튼: 셀 크기 90, col=2 row=6 위치
        // CellPosition(2, 6) = x=(2-2.5)*90=-45, y=(3-6)*90=-270
        var go = new GameObject("GeneratorButton");
        go.transform.SetParent(boardParent.transform, false);

        var rt          = go.AddComponent<RectTransform>();
        rt.sizeDelta    = new Vector2(86, 86);
        rt.anchoredPosition = new Vector2(-45f, -270f);

        var img         = go.AddComponent<Image>();
        img.color       = new Color(1.0f, 0.75f, 0.0f);  // 골드색

        var btn         = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // 텍스트 자식
        var txtGo   = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txtRT   = txtGo.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        var txt     = txtGo.AddComponent<TextMeshProUGUI>();
        txt.text    = "GEN";
        txt.fontSize = 18;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color   = Color.black;

        // GeneratorView 스크립트 부착
        var genView = go.AddComponent<GeneratorView>();

        // Button.onClick → GeneratorView는 IPointerClickHandler 사용하므로 Button 이벤트는 불필요
        // 단, GeneratorView가 Generator.Instance를 찾아야 하므로 Generator GO 연결은 런타임에 됨

        Debug.Log("[Slagma] GeneratorButton 추가됨");
    }

static void UpdateCellPrefabSize()
    {
        const string path = "Assets/Prefabs/CellView.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            var root = scope.prefabContentsRoot;
            var img  = root.GetComponent<Image>();
            if (img == null) return;

            // 90px 셀 안에 84px 이미지 → 3px 간격으로 격자 구분
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(84f, 84f);
        }
        Debug.Log("[Slagma] CellView 크기 84×84 업데이트");
    }


    // ── CharacterView 프리팹에 Image 추가 ─────────────────────────
static void UpdateCharacterViewPrefabs()
    {
        // 기존 프리팹 삭제 후 재생성 (World Space Canvas 제거)
        RecreatePrefab("Assets/Prefabs/AllyCharacterView.prefab",
            new Color(0.3f, 0.6f, 1.0f), "Ally");
        RecreatePrefab("Assets/Prefabs/EnemyCharacterView.prefab",
            new Color(1.0f, 0.3f, 0.3f), "Enm");
    }

static void RecreatePrefab(string path, Color color, string label)
    {
        // 기존 프리팹 삭제
        AssetDatabase.DeleteAsset(path);

        // 임시 Canvas 안에서 UGUI 요소로 생성
        var tempCanvas    = new GameObject("_TempC");
        var canvas        = tempCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tempCanvas.AddComponent<CanvasScaler>();
        tempCanvas.AddComponent<GraphicRaycaster>();

        var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        go.transform.SetParent(tempCanvas.transform, false);

        var rt       = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 120);

        var img      = go.AddComponent<Image>();
        img.color    = color;
        img.raycastTarget = false;

        go.AddComponent<CharacterView>();

        // 텍스트
        var txtGo   = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txtRT   = txtGo.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        var txt     = txtGo.AddComponent<TextMeshProUGUI>();
        txt.text    = label;
        txt.fontSize    = 22;
        txt.alignment   = TextAlignmentOptions.Center;
        txt.color   = Color.white;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(tempCanvas);
        Debug.Log($"[Slagma] {path} 재생성됨");
    }

    
static void UpdatePrefab(string path, Color color, string label, float size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null || prefab.GetComponent<Canvas>() != null) return;

        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            var root = scope.prefabContentsRoot;

            // Canvas(WorldSpace) 추가 → RectTransform 자동 생성
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            root.AddComponent<CanvasScaler>();

            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            // 알맞은 world scale: 스크린 1080px 기준 캔버스 비율에 맞춤
            root.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            var img = root.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(root.transform, false);
            var txtRT = txtGo.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 20;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
        }
        Debug.Log($"[Slagma] {path} 업데이트됨");
    }
}

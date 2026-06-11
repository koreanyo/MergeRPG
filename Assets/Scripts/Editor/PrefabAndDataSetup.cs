using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// JSON 데이터, 프리팹, ScriptableObject, 씬 연결, 실행 순서를 한 번에 설정.
/// Slagma/Setup Prefabs & Data 메뉴 실행.
/// </summary>
public static class PrefabAndDataSetup
{
    const string JSON_PATH    = "Assets/Resources/Data/JSON";
    const string PREFAB_PATH  = "Assets/Prefabs";
    const string DATA_PATH    = "Assets/Data";

    [MenuItem("Slagma/Setup Prefabs & Data")]
    public static void Run()
    {
        CreateDirectories();
        WriteJSONFiles();
        CreateDeckConfig();
        CreatePrefabs();
        ConnectScene();
        SetExecutionOrder();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Slagma] 프리팹 & 데이터 설정 완료!");
    }

    // ── 1. 폴더 생성 ──────────────────────────────────────────────
    static void CreateDirectories()
    {
        EnsureDir("Assets/Resources");
        EnsureDir("Assets/Resources/Data");
        EnsureDir(JSON_PATH);
        EnsureDir(PREFAB_PATH);
        EnsureDir(DATA_PATH);
    }

    static void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts  = path.Split('/');
            var parent = string.Join("/", parts, 0, parts.Length - 1);
            AssetDatabase.CreateFolder(parent, parts[parts.Length - 1]);
        }
    }

    // ── 2. JSON 파일 작성 ──────────────────────────────────────────
    static void WriteJSONFiles()
    {
        Write("unit_data", @"[
  {""unitId"":""DUMMY_A"",""displayName"":""더미 A"",""race"":""Human"",""tier"":1,""hp"":100,""atk"":10,""spd"":1.0,""def"":3},
  {""unitId"":""DUMMY_A"",""displayName"":""더미 A"",""race"":""Human"",""tier"":2,""hp"":160,""atk"":18,""spd"":1.0,""def"":5},
  {""unitId"":""DUMMY_A"",""displayName"":""더미 A"",""race"":""Human"",""tier"":3,""hp"":250,""atk"":30,""spd"":1.0,""def"":8},
  {""unitId"":""DUMMY_A"",""displayName"":""더미 A"",""race"":""Human"",""tier"":4,""hp"":380,""atk"":48,""spd"":1.0,""def"":12},
  {""unitId"":""DUMMY_A"",""displayName"":""더미 A"",""race"":""Human"",""tier"":5,""hp"":580,""atk"":75,""spd"":1.0,""def"":18},
  {""unitId"":""ENEMY_A"",""displayName"":""적 A"",""race"":""Human"",""tier"":1,""hp"":80,""atk"":8,""spd"":0.8,""def"":2}
]");
        Write("generator_pool_config", @"[
  {""stageId"":""stage_01"",""entries"":[{""unitId"":""DUMMY_A"",""race"":""Human"",""baseWeight"":10}]}
]");
        Write("generator_energy_config", @"[
  {""stageId"":""stage_01"",""maxEnergy"":99,""costPerSpawn"":1}
]");
        Write("stage_config", @"[
  {
    ""stageId"":""stage_01"",""displayName"":""스테이지 1"",""poolConfigId"":""stage_01"",""energyConfigId"":""stage_01"",
    ""waves"":[
      {""waveIndex"":0,""type"":""normal"",""waveClearEnergy"":5,""enemies"":[{""unitId"":""ENEMY_A"",""multiplier"":1.0,""count"":2}]},
      {""waveIndex"":1,""type"":""elite"",""waveClearEnergy"":10,""enemies"":[{""unitId"":""ENEMY_A"",""multiplier"":2.0,""count"":3}]}
    ]
  }
]");
        Write("stage_base_config", @"{""enemyKillEnergy"":2,""initialEnergy"":20}");
        Write("game_balance",       @"{""maxTier"":5}");
    }

    static void Write(string name, string content)
    {
        var fullPath = Path.GetFullPath($"{JSON_PATH}/{name}.json");
        File.WriteAllText(fullPath, content);
    }

    // ── 3. DeckConfig ScriptableObject ────────────────────────────
    static void CreateDeckConfig()
    {
        var assetPath = $"{DATA_PATH}/DeckConfig.asset";
        var existing  = AssetDatabase.LoadAssetAtPath<DeckConfig>(assetPath);
        if (existing == null)
        {
            var asset    = ScriptableObject.CreateInstance<DeckConfig>();
            asset.unitIds = new[] { "DUMMY_A", "DUMMY_A", "DUMMY_A" };
            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }

    // ── 4. 프리팹 생성 ─────────────────────────────────────────────
    static void CreatePrefabs()
    {
        // 임시 Canvas 부모 (UGUI 프리팹용)
        var tempCanvas    = new GameObject("_TempCanvas");
        var canvas        = tempCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tempCanvas.AddComponent<CanvasScaler>();
        tempCanvas.AddComponent<GraphicRaycaster>();

        CreateCellViewPrefab(tempCanvas.transform);
        CreateUnitPieceViewPrefab(tempCanvas.transform);
        CreateAllyCharacterViewPrefab();
        CreateEnemyCharacterViewPrefab();

        Object.DestroyImmediate(tempCanvas);
    }

    static void CreateCellViewPrefab(Transform parent)
    {
        var path = $"{PREFAB_PATH}/CellView.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("CellView");
        go.transform.SetParent(parent, false);

        var rt         = go.AddComponent<RectTransform>();
        rt.sizeDelta   = new Vector2(90, 90);

        var img        = go.AddComponent<Image>();
        img.color      = new Color(0.25f, 0.25f, 0.30f, 1f);  // 어두운 회색 셀
        img.raycastTarget = true;

        go.AddComponent<CellView>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreateUnitPieceViewPrefab(Transform parent)
    {
        var path = $"{PREFAB_PATH}/UnitPieceView.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("UnitPieceView");
        go.transform.SetParent(parent, false);

        var rt       = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);

        var img      = go.AddComponent<Image>();
        img.color    = new Color(0.45f, 0.82f, 1.00f);  // 초기 sky blue (tier1)
        img.raycastTarget = true;

        go.AddComponent<CanvasGroup>();
        go.AddComponent<UnitPieceView>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreateAllyCharacterViewPrefab()
    {
        var path = $"{PREFAB_PATH}/AllyCharacterView.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("AllyCharacterView");
        go.AddComponent<CharacterView>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void CreateEnemyCharacterViewPrefab()
    {
        var path = $"{PREFAB_PATH}/EnemyCharacterView.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("EnemyCharacterView");
        go.AddComponent<CharacterView>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    // ── 5. 씬 연결 ────────────────────────────────────────────────
    static void ConnectScene()
    {
        var cellPrefab       = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_PATH}/CellView.prefab");
        var piecePrefab      = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_PATH}/UnitPieceView.prefab");
        var allyPrefab       = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_PATH}/AllyCharacterView.prefab");
        var enemyPrefab      = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_PATH}/EnemyCharacterView.prefab");
        var deckConfig       = AssetDatabase.LoadAssetAtPath<DeckConfig>($"{DATA_PATH}/DeckConfig.asset");

        // DataManager (씬에 없으면 추가)
        if (Object.FindFirstObjectByType<DataManager>() == null)
        {
            var dmGo = new GameObject("DataManager");
            dmGo.AddComponent<DataManager>();
        }

        // MergeBoard 연결
        var mb = Object.FindFirstObjectByType<MergeBoard>();
        if (mb != null)
        {
            var so = new SerializedObject(mb);
            so.FindProperty("cellPrefab")     .objectReferenceValue = cellPrefab;
            so.FindProperty("unitPiecePrefab").objectReferenceValue = piecePrefab;

            // BoardParent 연결 (Canvas/MergeBoardArea/BoardParent)
            var boardParent = GameObject.Find("BoardParent");
            if (boardParent != null)
                so.FindProperty("boardParent").objectReferenceValue =
                    boardParent.GetComponent<RectTransform>();
            so.ApplyModifiedProperties();
        }

        // BattleManager 연결
        var bm = Object.FindFirstObjectByType<BattleManager>();
        if (bm != null)
        {
            var so = new SerializedObject(bm);
            so.FindProperty("allyCharacterViewPrefab") .objectReferenceValue = allyPrefab;
            so.FindProperty("enemyCharacterViewPrefab").objectReferenceValue = enemyPrefab;
            so.FindProperty("deckConfig")              .objectReferenceValue = deckConfig;

            // stageBaseConfig 인라인 값 설정 (enemyKillEnergy=2)
            var sbcProp = so.FindProperty("stageBaseConfig");
            sbcProp.FindPropertyRelative("enemyKillEnergy").intValue = 2;
            sbcProp.FindPropertyRelative("initialEnergy")  .intValue = 20;

            // allySlots 배열 (AllySlot_0~2)
            var allySlotsProp = so.FindProperty("allySlots");
            allySlotsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var slot = GameObject.Find($"AllySlot_{i}");
                if (slot != null)
                    allySlotsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        slot.transform;
            }

            // enemySlots 배열 (EnemySlot_0~2)
            var enemySlotsProp = so.FindProperty("enemySlots");
            enemySlotsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var slot = GameObject.Find($"EnemySlot_{i}");
                if (slot != null)
                    enemySlotsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        slot.transform;
            }

            so.ApplyModifiedProperties();
        }

        // GameManager 연결
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var so = new SerializedObject(gm);
            so.FindProperty("deckConfig").objectReferenceValue = deckConfig;
            so.ApplyModifiedProperties();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    // ── 6. Script Execution Order ──────────────────────────────────
    static void SetExecutionOrder()
    {
        SetOrder("BattleManager", -100);
        SetOrder("MergeBoard",    -90);
        SetOrder("GameManager",   -80);
    }

    static void SetOrder(string scriptName, int order)
    {
        foreach (var mono in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (mono.GetClass() != null && mono.GetClass().Name == scriptName)
            {
                MonoImporter.SetExecutionOrder(mono, order);
                return;
            }
        }
    }
}

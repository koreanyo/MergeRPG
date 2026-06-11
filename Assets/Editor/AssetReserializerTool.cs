using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AI(Claude MCP)와의 작업 정확도를 높이기 위한 에셋 재직렬화 도구.
/// 스크립트 구조 변경 후 .prefab, .asset 등 파일을 최신 상태로 강제 동기화합니다.
/// </summary>
[InitializeOnLoad]
public static class AssetReserializerTool
{
    // 자동 재직렬화 대상 폴더 (Assets/ 기준 상대 경로)
    private static readonly string[] AutoTargetFolders =
    {
        "Assets/Prefabs",
        "Assets/Resources",
    };

    static AssetReserializerTool()
    {
        // 에디터 로딩 완료 후 자동 재직렬화 등록
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    private static void OnAfterAssemblyReload()
    {
        var existingFolders = AutoTargetFolders
            .Where(f => AssetDatabase.IsValidFolder(f))
            .ToArray();

        if (existingFolders.Length == 0) return;

        var paths = CollectAssetPaths(existingFolders);
        if (paths.Count == 0) return;

        Debug.Log($"[AssetReserializer] 스크립트 리로드 후 자동 재직렬화: {paths.Count}개 에셋");
        AssetDatabase.ForceReserializeAssets(paths);
    }

    // ──────────────────────────────────────────
    // 메뉴: Tools > AI Tools
    // ──────────────────────────────────────────

    [MenuItem("Tools/AI Tools/전체 에셋 재직렬화 (Assets/)")]
    public static void ReserializeAll()
    {
        ReserializeFolder("Assets");
    }

    [MenuItem("Tools/AI Tools/핵심 폴더만 재직렬화 (Prefabs, Resources)")]
    public static void ReserializeKeyFolders()
    {
        var folders = AutoTargetFolders.Where(f => AssetDatabase.IsValidFolder(f)).ToArray();
        if (folders.Length == 0)
        {
            Debug.LogWarning("[AssetReserializer] 대상 폴더 없음. Prefabs 또는 Resources 폴더를 생성하세요.");
            return;
        }
        ReserializeFolder(folders);
    }

    [MenuItem("Tools/AI Tools/Prefab만 재직렬화")]
    public static void ReserializePrefabsOnly()
    {
        ReserializeByExtension("Assets", ".prefab");
    }

    [MenuItem("Tools/AI Tools/ScriptableObject만 재직렬화 (.asset)")]
    public static void ReserializeAssetsOnly()
    {
        ReserializeByExtension("Assets", ".asset");
    }

    // ──────────────────────────────────────────
    // 우클릭 컨텍스트 메뉴 (Project 창)
    // ──────────────────────────────────────────

    [MenuItem("Assets/AI Tools/이 폴더 재직렬화", true)]
    private static bool ValidateReserializeSelected()
    {
        return Selection.activeObject != null;
    }

    [MenuItem("Assets/AI Tools/이 폴더 재직렬화")]
    public static void ReserializeSelected()
    {
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.IsValidFolder(path))
                ReserializeFolder(path);
            else
                AssetDatabase.ForceReserializeAssets(new[] { path });
        }
    }

    // ──────────────────────────────────────────
    // 내부 유틸
    // ──────────────────────────────────────────

    private static void ReserializeFolder(params string[] folders)
    {
        var paths = CollectAssetPaths(folders);
        if (paths.Count == 0)
        {
            Debug.LogWarning($"[AssetReserializer] 재직렬화할 에셋 없음: {string.Join(", ", folders)}");
            return;
        }
        Debug.Log($"[AssetReserializer] 재직렬화 시작: {paths.Count}개 에셋");
        AssetDatabase.ForceReserializeAssets(paths);
        Debug.Log("[AssetReserializer] 완료.");
    }

    private static void ReserializeByExtension(string folder, string extension)
    {
        var guids = AssetDatabase.FindAssets("", new[] { folder });
        var paths = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => Path.GetExtension(p) == extension)
            .ToList();

        if (paths.Count == 0)
        {
            Debug.LogWarning($"[AssetReserializer] '{extension}' 파일 없음.");
            return;
        }
        Debug.Log($"[AssetReserializer] '{extension}' {paths.Count}개 재직렬화 시작");
        AssetDatabase.ForceReserializeAssets(paths);
        Debug.Log("[AssetReserializer] 완료.");
    }

    private static readonly HashSet<string> SerializableExtensions = new HashSet<string>
    {
        ".prefab", ".asset", ".unity", ".mat", ".anim", ".controller"
    };

    private static List<string> CollectAssetPaths(IEnumerable<string> folders)
    {
        var guids = AssetDatabase.FindAssets("", folders.ToArray());
        return guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => !AssetDatabase.IsValidFolder(p) && SerializableExtensions.Contains(Path.GetExtension(p)))
            .Distinct()
            .ToList();
    }
}

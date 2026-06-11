using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// BootScene에서 모든 JSON 데이터를 로드하고 캐싱한다.
/// DataManager.GetXxx() 정적 메서드로 접근.
/// JSON 파일 위치: Assets/Resources/Data/JSON/
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        Instance = null;
        _unitData.Clear();
        _poolConfigs.Clear();
        _energyConfigs.Clear();
        _stageConfigs.Clear();
        _stageBaseConfig = null;
        _balance = null;
    }

    // ── 캐시 ─────────────────────────────────────────────────────
    // unit_data: (unitId, tier) 복합키
    private static Dictionary<(string, int), UnitData>      _unitData       = new();
    private static Dictionary<string, GeneratorPoolConfig>   _poolConfigs    = new();
    private static Dictionary<string, GeneratorEnergyConfig> _energyConfigs  = new();
    private static Dictionary<string, StageConfig>           _stageConfigs   = new();
    private static StageBaseConfig                           _stageBaseConfig;
    private static GameBalance                               _balance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    // ── 전체 로드 ─────────────────────────────────────────────────
    void LoadAll()
    {
        LoadUnitData();
        LoadPoolConfigs();
        LoadEnergyConfigs();
        LoadStageConfigs();
        LoadStageBaseConfig();
        LoadGameBalance();
        Debug.Log("[DataManager] 모든 데이터 로드 완료.");
    }

    void LoadUnitData()
    {
        var text = Load("unit_data");
        var list = JsonConvert.DeserializeObject<List<UnitData>>(text);
        foreach (var d in list)
            _unitData[(d.unitId, d.tier)] = d;
    }

    void LoadPoolConfigs()
    {
        var text = Load("generator_pool_config");
        var list = JsonConvert.DeserializeObject<List<GeneratorPoolConfig>>(text);
        foreach (var c in list)
            _poolConfigs[c.stageId] = c;
    }

    void LoadEnergyConfigs()
    {
        var text = Load("generator_energy_config");
        var list = JsonConvert.DeserializeObject<List<GeneratorEnergyConfig>>(text);
        foreach (var c in list)
            _energyConfigs[c.stageId] = c;
    }

    void LoadStageConfigs()
    {
        var text = Load("stage_config");
        var list = JsonConvert.DeserializeObject<List<StageConfig>>(text);
        foreach (var c in list)
            _stageConfigs[c.stageId] = c;
    }

    void LoadStageBaseConfig()
    {
        var text = Load("stage_base_config");
        _stageBaseConfig = JsonConvert.DeserializeObject<StageBaseConfig>(text);
    }

    void LoadGameBalance()
    {
        var text = Load("game_balance");
        _balance = JsonConvert.DeserializeObject<GameBalance>(text);
    }

    static string Load(string fileName)
    {
        var asset = Resources.Load<TextAsset>($"Data/JSON/{fileName}");
        if (asset == null)
            throw new System.Exception($"[DataManager] JSON 파일을 찾을 수 없음: Data/JSON/{fileName}");
        return asset.text;
    }

    // ── 조회 API ──────────────────────────────────────────────────
    public static UnitData GetUnitData(string unitId, int tier)
    {
        if (_unitData.TryGetValue((unitId, tier), out var data)) return data;
        Debug.LogError($"[DataManager] UnitData 없음: {unitId} tier{tier}");
        return null;
    }

    public static GeneratorPoolConfig GetPoolConfig(string stageId)
    {
        if (_poolConfigs.TryGetValue(stageId, out var c)) return c;
        Debug.LogError($"[DataManager] PoolConfig 없음: {stageId}");
        return null;
    }

    public static GeneratorEnergyConfig GetEnergyConfig(string stageId)
    {
        if (_energyConfigs.TryGetValue(stageId, out var c)) return c;
        Debug.LogError($"[DataManager] EnergyConfig 없음: {stageId}");
        return null;
    }

    public static StageConfig GetStageConfig(string stageId)
    {
        if (_stageConfigs.TryGetValue(stageId, out var c)) return c;
        Debug.LogError($"[DataManager] StageConfig 없음: {stageId}");
        return null;
    }

    public static StageBaseConfig GetStageBaseConfig() => _stageBaseConfig;

    public static GameBalance Balance => _balance;
}

using TMPro;
using UnityEngine;


/// <summary>
/// 스테이지 흐름 관리 (웨이브 진행, 클리어/실패 판정, 씬 전환).
/// Script Execution Order: -80 (BattleManager -100, MergeBoard -90 보다 나중)
/// Start()에서 OnStageStart() 호출 → BattleManager/MergeBoard OnEnable 완료 보장.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private DeckConfig      deckConfig;
    [SerializeField] private TextMeshProUGUI waveText;  // HUDCanvas/WaveText

    private int _currentWaveIndex;

    void Awake()
    {
        // 데이터 로드만. EventBus 구독 없음.
        // (DataManager가 별도 GameObject에 있어야 함)
    }

    void OnEnable()
    {
        EventBus.Subscribe<int> (EventNames.OnWaveCleared,  HandleWaveCleared);
        EventBus.Subscribe<bool>(EventNames.OnStageFailed,  HandleStageFailed);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<int> (EventNames.OnWaveCleared,  HandleWaveCleared);
        EventBus.Unsubscribe<bool>(EventNames.OnStageFailed,  HandleStageFailed);
    }

    void Start()
    {
        // BattleManager, MergeBoard의 OnEnable 완료 이후 보장
        OnStageStart();
    }

    void OnStageStart()
    {
        // 1. 에너지 시스템 초기화
        var energyConfig = DataManager.GetEnergyConfig("stage_01");
        var baseConfig   = DataManager.GetStageBaseConfig();
        GeneratorEnergy.Instance.Load(energyConfig);
        GeneratorEnergy.Instance.AddEnergy(baseConfig.initialEnergy);

        // 2. Generator 풀 초기화
        Generator.Instance.InitPool("stage_01");

        // 3. 덱 3종 tier1 피스 보드에 배치
        foreach (var unitId in deckConfig.unitIds)
        {
            var data = DataManager.GetUnitData(unitId, tier: 1);
            MergeBoard.Instance.PlaceInitialPiece(data);
        }

        // 4. 첫 웨이브 시작 (0-based)
        _currentWaveIndex = 0;
        var stageConfig0 = DataManager.GetStageConfig("stage_01");
        UpdateWaveText(0, stageConfig0.waves.Length);
        EventBus.Publish<int>(EventNames.OnWaveStarted, 0);
    }

    private void HandleWaveCleared(int waveIndex)
    {
        var stageConfig = DataManager.GetStageConfig("stage_01");
        int next        = waveIndex + 1;

        if (next < stageConfig.waves.Length)
        {
            _currentWaveIndex = next;
            UpdateWaveText(next, stageConfig.waves.Length);
            EventBus.Publish<int>(EventNames.OnWaveStarted, next);
        }
        else
        {
            SessionData.IsCleared    = true;
            SessionData.ClearedWaves = next;
            SessionData.TotalWaves   = stageConfig.waves.Length;
            PopupManager.Instance.ShowResultPopup(true, next, stageConfig.waves.Length);
        }
    }

    private void UpdateWaveText(int waveIndex, int totalWaves)
    {
        if (waveText != null)
            waveText.text = $"Wave {waveIndex + 1}/{totalWaves}";
    }

    private void HandleStageFailed(bool _)
    {
        var stageConfig = DataManager.GetStageConfig("stage_01");
        int cleared     = _currentWaveIndex;
        SessionData.IsCleared    = false;
        SessionData.ClearedWaves = cleared;
        SessionData.TotalWaves   = stageConfig.waves.Length;
        PopupManager.Instance.ShowResultPopup(false, cleared, stageConfig.waves.Length);
    }
}

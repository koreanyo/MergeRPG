/// <summary>
/// EventBus에서 사용하는 이벤트 이름 상수.
/// 타입 주석은 페이로드 타입을 명시한다.
/// </summary>
public static class EventNames
{
    public const string OnSpawnRequested   = "OnSpawnRequested";   // Generator   → MergeBoard     (UnitData)
    public const string OnUnitSpawned      = "OnUnitSpawned";      // MergeBoard  → BattleManager  (string unitId)
    public const string OnUnitMerged       = "OnUnitMerged";       // MergeBoard  → BattleManager  (string unitId)
    public const string OnBoardFullChanged = "OnBoardFullChanged"; // MergeBoard  → GeneratorView  (bool)
    public const string OnCharDied         = "OnCharDied";         // BattleManager → MergeBoard   (string unitId)
    public const string OnWaveStarted      = "OnWaveStarted";      // GameManager → BattleManager  (int waveIndex)
    public const string OnWaveCleared      = "OnWaveCleared";      // BattleManager → GameManager  (int waveIndex)
    public const string OnStageFailed      = "OnStageFailed";      // BattleManager → GameManager  (bool, true 고정)
    public const string OnEnergyChanged    = "OnEnergyChanged";    // GeneratorEnergy → GeneratorView (int)
    public const string OnSkillQueued      = "OnSkillQueued";      // MergeBoard  → BattleManager  (string unitId)
}

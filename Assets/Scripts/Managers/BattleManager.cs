using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ATB 루프, 타겟팅, 데미지 계산, CharacterView 레지스트리 담당.
/// MergeBoard와는 EventBus로만 통신.
/// Script Execution Order: -100 (GameManager보다 먼저 OnEnable 실행)
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // Enter Play Mode (Reload Domain 꺼져 있을 때) static 잔존 방지
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    // 인스펙터 연결
    [SerializeField] private GameObject      allyCharacterViewPrefab;
    [SerializeField] private GameObject      enemyCharacterViewPrefab;
    [SerializeField] private Transform[]     allySlots;   // 아군 3슬롯
    [SerializeField] private Transform[]     enemySlots;  // 적 슬롯
    [SerializeField] private DeckConfig      deckConfig;

    // stageBaseConfig는 런타임에 DataManager에서 로드
    private StageBaseConfig _stageBaseConfig;

    // 전투 데이터
    private Dictionary<string, int>           effectiveTier    = new();
    private List<string>                      activeCharacters = new();
    private Dictionary<string, CharacterView> _charViews       = new();   // key = unitId
    private Dictionary<string, CharacterView> _enemyViews      = new();   // key = instanceId
    private List<EnemyUnit>                   activeEnemies    = new();
    private Dictionary<string, float>         _allyHp          = new();

    // 웨이브 데이터
    private StageConfig   _currentStageConfig;
    private StageWaveData _currentWaveConfig;
    private int           _currentWaveIndex;

    // ATB 큐
    private List<CharacterView> _gaugeFullQueue = new();

    // 스킬 대기
    private HashSet<string> _pendingSkills = new();

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        EventBus.Subscribe<string>(EventNames.OnUnitSpawned, HandleUnitSpawned);
        EventBus.Subscribe<string>(EventNames.OnUnitMerged,  HandleUnitMerged);
        EventBus.Subscribe<string>(EventNames.OnCharDied,    HandleCharDied);
        EventBus.Subscribe<int>   (EventNames.OnWaveStarted, HandleWaveStarted);
        EventBus.Subscribe<string>(EventNames.OnSkillQueued, HandleSkillQueued);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<string>(EventNames.OnUnitSpawned, HandleUnitSpawned);
        EventBus.Unsubscribe<string>(EventNames.OnUnitMerged,  HandleUnitMerged);
        EventBus.Unsubscribe<string>(EventNames.OnCharDied,    HandleCharDied);
        EventBus.Unsubscribe<int>   (EventNames.OnWaveStarted, HandleWaveStarted);
        EventBus.Unsubscribe<string>(EventNames.OnSkillQueued, HandleSkillQueued);
    }

    // 이벤트 핸들러
    private void HandleUnitSpawned(string unitId)
    {
        int maxTier = MergeBoard.Instance.GetMaxTierOf(unitId);
        effectiveTier[unitId] = maxTier;

        if (activeCharacters.Contains(unitId)) return;
        activeCharacters.Add(unitId);

        if (_charViews.ContainsKey(unitId)) return;

        int slotIndex = GetAllySlotIndex(unitId);
        if (slotIndex < 0 || slotIndex >= allySlots.Length) return;

        var go   = Instantiate(allyCharacterViewPrefab, allySlots[slotIndex]);
        var rt   = go.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;

        var view = go.GetComponent<CharacterView>();
        var data = DataManager.GetUnitData(unitId, maxTier);
        view.unitId = unitId;
        view.isAlly = true;
        view.Init(data.spd);

        _charViews[unitId] = view;
        _allyHp[unitId]    = data.hp;

        view.UpdateHpBar(_allyHp[unitId], data.hp);
    }

    private void HandleUnitMerged(string unitId)
    {
        int oldTier = effectiveTier.ContainsKey(unitId) ? effectiveTier[unitId] : 0;
        int newTier = MergeBoard.Instance.GetMaxTierOf(unitId);
        effectiveTier[unitId] = newTier;

        if (!_charViews.TryGetValue(unitId, out var view)) return;

        var data = DataManager.GetUnitData(unitId, newTier);
        view.PlayMergeFeedback();
        FloatingTextManager.Instance.Show(view.transform.position, $"+ATK {data.atk}", Color.red);

        // 최고 tier가 실제로 올라간 경우에만 HP 20% 회복
        if (newTier > oldTier)
        {
            float healAmount = data.hp * 0.2f;
            _allyHp[unitId] = Mathf.Min(_allyHp[unitId] + healAmount, data.hp);
            view.UpdateHpBar(_allyHp[unitId], data.hp);
            FloatingTextManager.Instance.Show(
                view.transform.position + Vector3.right * 0.3f,
                $"+HP {Mathf.FloorToInt(healAmount)}", Color.green);
        }
    }

    private void HandleCharDied(string unitId)
    {
        effectiveTier.Remove(unitId);
        activeCharacters.Remove(unitId);
        _charViews.Remove(unitId);
        CheckDefeatCondition();
    }

    private void HandleWaveStarted(int waveIndex)
    {
        _currentWaveIndex   = waveIndex;
        _currentStageConfig = DataManager.GetStageConfig("stage_01");
        _currentWaveConfig  = _currentStageConfig.waves[_currentWaveIndex];
        _stageBaseConfig    = DataManager.GetStageBaseConfig();

        foreach (var entry in _currentWaveConfig.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                var baseData = DataManager.GetUnitData(entry.unitId, tier: 1);
                var enemy = new EnemyUnit
                {
                    instanceId = System.Guid.NewGuid().ToString(),
                    unitId     = entry.unitId,
                    hp         = baseData.hp  * entry.multiplier,
                    maxHp      = baseData.hp  * entry.multiplier,
                    atk        = baseData.atk * entry.multiplier,
                    spd        = baseData.spd,
                    def        = baseData.def * entry.multiplier,
                };
                activeEnemies.Add(enemy);

                int slotIndex = activeEnemies.Count - 1;
                if (slotIndex >= enemySlots.Length) continue;

                var go   = Instantiate(enemyCharacterViewPrefab, enemySlots[slotIndex]);
                var rt   = go.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = Vector2.zero;

                var view = go.GetComponent<CharacterView>();
                view.unitId = enemy.instanceId;
                view.isAlly = false;
                view.Init(enemy.spd);
                _enemyViews[enemy.instanceId] = view;

                view.UpdateHpBar(enemy.hp, enemy.maxHp);
            }
        }
    }

    private void HandleSkillQueued(string unitId) => _pendingSkills.Add(unitId);

    // ATB 루프
    public void QueueGaugeFull(CharacterView view) => _gaugeFullQueue.Add(view);

    void LateUpdate()
    {
        if (_gaugeFullQueue.Count == 0) return;
        _gaugeFullQueue.Sort((a, b) => b.isAlly.CompareTo(a.isAlly));  // 아군 우선
        foreach (var view in _gaugeFullQueue)
            OnGaugeFull(view);
        _gaugeFullQueue.Clear();
    }

    void OnGaugeFull(CharacterView view)
    {
        if (view.isAlly)
        {
            if (activeEnemies.Count == 0) return;
            var target = _enemyViews[activeEnemies[0].instanceId];
            if (HasPendingSkill(view.unitId)) view.ExecuteSkill(target);
            else                              view.ExecuteAttack(target);
        }
        else
        {
            if (activeCharacters.Count == 0) return;
            var target = _charViews[activeCharacters[0]];
            view.ExecuteAttack(target);
        }
    }

    // 데미지 계산
    public void ProcessAttackHit(CharacterView attacker, CharacterView target)
    {
        if (attacker.isAlly)
        {
            var enemy = activeEnemies.Find(e => e.instanceId == target.unitId);
            if (enemy == null) return;

            var data  = DataManager.GetUnitData(attacker.unitId, effectiveTier[attacker.unitId]);
            float dmg = Mathf.Max(1f, data.atk - enemy.def);
            enemy.hp -= dmg;
            target.PlayHit();
            target.UpdateHpBar(enemy.hp, enemy.maxHp);

            if (enemy.hp <= 0f) HandleEnemyDeath(enemy);
        }
        else
        {
            var enemy = activeEnemies.Find(e => e.instanceId == attacker.unitId);
            if (enemy == null) return;

            if (!effectiveTier.ContainsKey(target.unitId)) return;
            var data  = DataManager.GetUnitData(target.unitId, effectiveTier[target.unitId]);
            float dmg = Mathf.Max(1f, enemy.atk - data.def);
            _allyHp[target.unitId] -= dmg;
            target.PlayHit();
            target.UpdateHpBar(_allyHp[target.unitId], data.hp);

            if (_allyHp[target.unitId] <= 0f)
            {
                target.OnDie();
                EventBus.Publish<string>(EventNames.OnCharDied, target.unitId);
            }
        }
    }

    private void HandleEnemyDeath(EnemyUnit enemy)
    {
        if (_enemyViews.TryGetValue(enemy.instanceId, out var view))
        {
            view.OnDie();
            _enemyViews.Remove(enemy.instanceId);
        }
        activeEnemies.Remove(enemy);

        GeneratorEnergy.Instance.AddEnergy(_stageBaseConfig.enemyKillEnergy);

        if (activeEnemies.Count == 0)
        {
            GeneratorEnergy.Instance.AddEnergy(_currentWaveConfig.waveClearEnergy);
            EventBus.Publish<int>(EventNames.OnWaveCleared, _currentWaveIndex);
        }
    }

    private void CheckDefeatCondition()
    {
        if (activeCharacters.Count == 0)
            EventBus.Publish<bool>(EventNames.OnStageFailed, true);
    }

    // 유틸
    private int GetAllySlotIndex(string unitId)
    {
        for (int i = 0; i < deckConfig.unitIds.Length; i++)
            if (deckConfig.unitIds[i] == unitId) return i;
        return -1;
    }

    public void ClearPendingSkill(string unitId) => _pendingSkills.Remove(unitId);
    public bool HasPendingSkill(string unitId)   => _pendingSkills.Contains(unitId);
}

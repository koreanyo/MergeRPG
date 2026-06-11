using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 확률 계산 후 OnSpawnRequested 이벤트를 발행한다.
/// 실제 피스 생성·배치는 MergeBoard가 담당.
/// </summary>
public class Generator : MonoBehaviour
{
    public static Generator Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    private GeneratorProbability _probability = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>스테이지 시작 시 GameManager가 호출. 풀을 JSON에서 주입한다.</summary>
    public void InitPool(string stageId)
    {
        var config = DataManager.GetPoolConfig(stageId);
        if (config == null) return;

        var entries = config.entries.Select(e => new PoolEntry
        {
            unitId     = e.unitId,
            race       = e.race,
            baseWeight = e.baseWeight
        }).ToList();

        _probability.SetPool(entries);
    }

    /// <summary>GeneratorView가 클릭 시 호출. 에너지 소비 후 이벤트 발행.</summary>
    public void RequestSpawn()
    {
        if (!GeneratorEnergy.Instance.TrySpend()) return;

        var entry = _probability.Roll();
        if (entry == null) return;

        var data = DataManager.GetUnitData(entry.unitId, tier: 1);
        if (data == null) return;

        EventBus.Publish<UnitData>(EventNames.OnSpawnRequested, data);
    }

    // 외부에서 가중치 보정 추가/제거 (패시브, 기믹 등)
    public void AddModifier(WeightModifier mod)    => _probability.AddModifier(mod);
    public void RemoveModifier(WeightModifier mod) => _probability.RemoveModifier(mod);
}

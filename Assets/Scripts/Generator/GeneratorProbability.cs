using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 풀 항목과 WeightModifier를 합산해 최종 가중치로 랜덤 추출한다.
/// </summary>
public class GeneratorProbability
{
    private List<PoolEntry>      _pool      = new();
    private List<WeightModifier> _modifiers = new();

    public void SetPool(List<PoolEntry> pool) => _pool = pool;

    public void AddModifier(WeightModifier mod)    => _modifiers.Add(mod);
    public void RemoveModifier(WeightModifier mod) => _modifiers.Remove(mod);

    public PoolEntry Roll()
    {
        var weights = new List<(PoolEntry entry, int weight)>();

        foreach (var entry in _pool)
        {
            int w = entry.baseWeight;
            foreach (var mod in _modifiers)
            {
                if      (mod.targetType == WeightModifier.TargetType.All)  w += mod.value;
                else if (mod.targetType == WeightModifier.TargetType.Race  && mod.targetId == entry.race)   w += mod.value;
                else if (mod.targetType == WeightModifier.TargetType.Unit  && mod.targetId == entry.unitId) w += mod.value;
            }
            weights.Add((entry, Mathf.Max(w, 0)));  // 최소 0 보장
        }

        int total = 0;
        foreach (var (_, w) in weights) total += w;
        if (total == 0) return null;

        int roll   = Random.Range(0, total);
        int cursor = 0;
        foreach (var (entry, w) in weights)
        {
            cursor += w;
            if (roll < cursor) return entry;
        }
        return null;
    }
}

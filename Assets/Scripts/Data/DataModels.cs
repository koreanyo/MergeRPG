using System;

// ── unit_data.json ────────────────────────────────────────────────
[Serializable]
public class UnitData
{
    public string unitId;
    public string displayName;
    public string race;       // "Human" | "Elf" | "Orc"
    public int    tier;       // 1 ~ maxTier  (level 사용 금지)
    public int    hp;
    public int    atk;
    public float  spd;        // atbGauge += spd * Time.deltaTime * 100f
    public int    def;
}

// ── generator_pool_config.json ────────────────────────────────────
[Serializable]
public class GeneratorPoolConfig
{
    public string           stageId;
    public PoolEntryData[]  entries;
}

[Serializable]
public class PoolEntryData
{
    public string unitId;
    public string race;
    public int    baseWeight;
}

// ── generator_energy_config.json ─────────────────────────────────
[Serializable]
public class GeneratorEnergyConfig
{
    public string stageId;
    public int    maxEnergy;
    public int    costPerSpawn;
}

// ── stage_config.json ─────────────────────────────────────────────
[Serializable]
public class StageConfig
{
    public string         stageId;
    public string         displayName;
    public string         poolConfigId;
    public string         energyConfigId;
    public StageWaveData[] waves;
}

[Serializable]
public class StageWaveData
{
    public int              waveIndex;
    public string           type;             // "normal" | "elite" | "boss"
    public int              waveClearEnergy;
    public WaveEnemyEntry[] enemies;
}

[Serializable]
public class WaveEnemyEntry
{
    public string unitId;
    public float  multiplier;
    public int    count;
}

// ── stage_base_config.json ────────────────────────────────────────
[Serializable]
public class StageBaseConfig
{
    public int enemyKillEnergy;
    public int initialEnergy;
}

// ── game_balance.json ─────────────────────────────────────────────
[Serializable]
public class GameBalance
{
    public int maxTier;
}

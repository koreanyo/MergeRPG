using UnityEngine;

/// <summary>
/// 에너지 시스템 MonoBehaviour 싱글턴.
/// BattleManager가 적 처치/웨이브 클리어 시 AddEnergy()를 직접 호출한다.
/// </summary>
public class GeneratorEnergy : MonoBehaviour
{
    public static GeneratorEnergy Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    private int _current;
    private int _max;
    private int _costPerSpawn;

    public bool CanSpawn => _current >= _costPerSpawn;
    public int  Current  => _current;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Load(GeneratorEnergyConfig config)
    {
        _max          = config.maxEnergy;
        _costPerSpawn = config.costPerSpawn;
        _current      = 0;
    }

    public bool TrySpend()
    {
        if (!CanSpawn) return false;
        _current -= _costPerSpawn;
        EventBus.Publish<int>(EventNames.OnEnergyChanged, _current);
        return true;
    }

    public void AddEnergy(int amount)
    {
        _current = Mathf.Min(_current + amount, _max);
        EventBus.Publish<int>(EventNames.OnEnergyChanged, _current);
    }
}

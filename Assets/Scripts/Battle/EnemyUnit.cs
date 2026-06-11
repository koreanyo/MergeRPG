/// <summary>
/// 적 유닛의 런타임 스탯.
/// ATB 루프 주체는 CharacterView이므로 atbGauge 필드 없음.
/// </summary>
public class EnemyUnit
{
    public string instanceId;  // 런타임 고유 ID (Guid)
    public string unitId;      // unit_data.json 참조 키
    public float  hp;
    public float  maxHp;   // 스폰 시 초기 HP 저장 (UpdateHpBar 기준값)
    public float  atk;
    public float  spd;
    public float  def;
}

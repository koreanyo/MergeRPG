/// <summary>
/// Generator 확률 보정자.
/// 패시브 / 기믹 / 스킬이 AddModifier() 한 번으로 확률을 건드릴 수 있다.
/// </summary>
public class WeightModifier
{
    public enum TargetType { Unit, Race, All }

    public TargetType targetType;  // 보정 대상 범위
    public string     targetId;    // unitId 또는 race명 (All이면 무시)
    public int        value;       // 보정값 (양수 = 증가, 음수 = 감소)
    public string     source;      // 출처 식별자 ("passive", "gimmick", "skill" 등)
}

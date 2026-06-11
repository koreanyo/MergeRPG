/// <summary>
/// 보드 위 피스의 순수 데이터.
/// 뷰는 UnitPieceView(보드)와 CharacterView(전투)로 분리.
/// </summary>
public class UnitPiece
{
    public string    instanceId;  // 런타임 고유 ID (Guid)
    public string    unitId;      // 종류 식별자 (예: "DUMMY_A")
    public string    race;        // "Human" | "Elf" | "Orc"
    public int       tier;        // 1 ~ maxTier
    public MergeCell cell;        // 현재 점유 중인 셀 참조
}

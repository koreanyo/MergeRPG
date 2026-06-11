/// <summary>
/// 머지 보드의 셀 하나를 나타내는 데이터 클래스.
/// </summary>
public class MergeCell
{
    public int  col;               // 0 ~ 6
    public int  row;               // 0 ~ 3
    public UnitPiece occupiedBy;   // 점유 중인 피스 (없으면 null)

    public bool IsEmpty => occupiedBy == null;
}

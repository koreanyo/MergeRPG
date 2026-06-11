using UnityEngine;

/// <summary>
/// 스테이지에서 사용할 덱 3종을 인스펙터에서 지정하는 ScriptableObject.
/// Phase 0: 하드코딩으로 DUMMY_A 등록.
/// </summary>
[CreateAssetMenu(fileName = "DeckConfig", menuName = "Slagma/DeckConfig")]
public class DeckConfig : ScriptableObject
{
    public string[] unitIds;  // 예) ["DUMMY_A", "DUMMY_A", "DUMMY_A"]
}

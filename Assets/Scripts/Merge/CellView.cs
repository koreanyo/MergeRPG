using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 보드 셀 하나의 뷰 컴포넌트.
/// IDropHandler로 드래그된 피스를 수신한다.
/// 프리팹 필수 컴포넌트: Image (GraphicRaycaster 감지용)
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class CellView : MonoBehaviour, IDropHandler
{
    public MergeCell cellData;

    public void OnDrop(PointerEventData eventData)
    {
        // Step 5에서 드래그 판정 로직 연결
        MergeBoard.Instance.OnDropped(eventData, cellData);
    }
}

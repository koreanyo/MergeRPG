using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UnitPieceView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public UnitPiece data;

    [SerializeField] private Image    _bgImage;
    [SerializeField] private Image    _borderImage;
    [SerializeField] private Image    _pieceImage;   // 종류 아이콘 — 추후 테이블 연동
    [SerializeField] private TMP_Text _countText;

    private CanvasGroup _canvasGroup;
    private Canvas      _canvas;

    public void Init(UnitPiece piece)
    {
        data         = piece;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvas      = GetComponentInParent<Canvas>();
        RefreshVisual();
    }

    void RefreshVisual()
    {
        var categoryColor = GetCategoryColor(data?.race);
        if (_bgImage)     _bgImage.color     = categoryColor;
        if (_borderImage) _borderImage.color = categoryColor;
        if (_countText)   _countText.text    = (data?.tier ?? 0).ToString();
    }

    // 카테고리(Unit/Item/Skill/Gimick) 색상 — 추후 테이블 정의 예정
    static Color GetCategoryColor(string race) => Color.white;

    int tier => data?.tier ?? 0;

    // ── 드래그 ──────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;  // 드래그 중 자신이 레이캐스트 막지 않도록
        MergeBoard.Instance.OnDragStart(data);
        transform.SetAsLastSibling();          // z-order 최상단
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Screen Space - Camera 캔버스: 스크린 → 월드 좌표 변환
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            eventData.position,
            _canvas.worldCamera,
            out Vector3 worldPos))
        {
            transform.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        MergeBoard.Instance.OnDragEnd();
    }

    public void OnDrop(PointerEventData eventData)
    {
        MergeBoard.Instance.OnDropped(eventData, data.cell);
    }
}

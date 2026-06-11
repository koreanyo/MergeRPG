using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 6×7 머지 보드 관리 싱글턴.
/// 셀 상태, 피스 배치/제거, 합성 판정을 담당.
/// EventBus로만 외부와 통신.
/// </summary>
public class MergeBoard : MonoBehaviour
{
    public static MergeBoard Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    // ── 인스펙터 연결 ──────────────────────────────────────────
    [SerializeField] private GameObject cellPrefab;       // CellView 프리팹
    [SerializeField] private GameObject unitPiecePrefab;  // UnitPieceView 프리팹
    [SerializeField] private float      cellSize    = 120f; // 셀 한 칸 시각 크기 (px), CellView sizeDelta와 동일
    [SerializeField] private float      cellSpacing = 10f;  // 셀 간격 (px)
    [SerializeField] private float      paddingLeft = 40f;
    [SerializeField] private float      paddingTop  = 40f;

    private RectTransform _boardRect;

    // ── 그리드 데이터 ─────────────────────────────────────────
    private const int COLS = 7;
    private const int ROWS = 4;
    private MergeCell[,] grid = new MergeCell[COLS, ROWS];

    // ── 뷰 레지스트리 (instanceId → UnitPieceView) ──────────
    private Dictionary<string, UnitPieceView> _views = new();

    // ── 드래그 상태 ───────────────────────────────────────────
    private UnitPiece  _selectedPiece;
    private MergeCell  _originCell;

    // ── CellView 레지스트리 (위치 복귀용) ────────────────────
    private Dictionary<(int, int), CellView> _cellViews = new();

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _boardRect = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        EventBus.Subscribe<UnitData>(EventNames.OnSpawnRequested, OnSpawnRequested);
        EventBus.Subscribe<string>(EventNames.OnCharDied, HandleCharDied);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<UnitData>(EventNames.OnSpawnRequested, OnSpawnRequested);
        EventBus.Unsubscribe<string>(EventNames.OnCharDied, HandleCharDied);
    }

void Start()
    {
        InitGrid();
        // GEN 버튼이 셀들 위에 렌더링되도록 마지막 자식으로 이동
        var genBtn = _boardRect.Find("GeneratorButton");
        if (genBtn != null) genBtn.SetAsLastSibling();
    }

    // ── 그리드 초기화 ─────────────────────────────────────────
    void InitGrid()
    {
        for (int col = 0; col < COLS; col++)
        {
            for (int row = 0; row < ROWS; row++)
            {
                var cell = new MergeCell { col = col, row = row };
                grid[col, row] = cell;

                // CellView 생성
                var go   = Instantiate(cellPrefab, _boardRect);
                var view = go.GetComponent<CellView>();
                view.cellData = cell;
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin        = new Vector2(0f, 1f);
                rect.anchorMax        = new Vector2(0f, 1f);
                rect.pivot            = new Vector2(0.5f, 0.5f);
                rect.sizeDelta        = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = CellPosition(col, row);
                go.name = $"Cell_{col}_{row}";

                _cellViews[(col, row)] = view;
            }
        }
    }

    // col/row → anchoredPosition 변환 (Upper Left 기준, GLG 동일 방식)
    Vector2 CellPosition(int col, int row)
    {
        float step = cellSize + cellSpacing;
        float x =   paddingLeft + col * step + cellSize * 0.5f;
        float y = -(paddingTop  + row * step + cellSize * 0.5f);
        return new Vector2(x, y);
    }

    // ── 피스 배치 (유일한 뷰 생성 진입점) ────────────────────
    public void PlacePiece(UnitPiece piece, MergeCell targetCell)
    {
        targetCell.occupiedBy = piece;
        piece.cell = targetCell;

        var go   = Instantiate(unitPiecePrefab, _boardRect);
        var view = go.GetComponent<UnitPieceView>();
        view.Init(piece);
        var rect          = go.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0f, 1f);
        rect.anchorMax        = new Vector2(0f, 1f);
        rect.pivot            = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = CellPosition(targetCell.col, targetCell.row);
        go.name = $"Piece_{piece.unitId}_t{piece.tier}";

        _views[piece.instanceId] = view;
    }

    // GameManager가 스테이지 시작 시 호출 (PlacePiece + OnUnitSpawned 발행)
    public void PlaceInitialPiece(UnitData data)
    {
        var cell = GetRandomEmptyCell();
        if (cell == null) return;

        var piece = new UnitPiece
        {
            instanceId = Guid.NewGuid().ToString(),
            unitId     = data.unitId,
            race       = data.race,
            tier       = data.tier
        };

        PlacePiece(piece, cell);
        EventBus.Publish<string>(EventNames.OnUnitSpawned, piece.unitId);
    }

    // ── Generator 스폰 요청 수신 ──────────────────────────────
    private void OnSpawnRequested(UnitData data)
    {
        var cell = GetRandomEmptyCell();
        if (cell == null) return;

        var piece = new UnitPiece
        {
            instanceId = Guid.NewGuid().ToString(),
            unitId     = data.unitId,
            race       = data.race,
            tier       = data.tier
        };

        PlacePiece(piece, cell);
        EventBus.Publish<string>(EventNames.OnUnitSpawned, piece.unitId);
        EventBus.Publish<bool>(EventNames.OnBoardFullChanged, !HasEmptyCell());
    }

    // ── 캐릭터 사망 수신 → 해당 unitId 피스 전부 제거 ────────
    private void HandleCharDied(string unitId)
    {
        for (int col = 0; col < COLS; col++)
        for (int row = 0; row < ROWS; row++)
        {
            var cell = grid[col, row];
            if (cell.occupiedBy != null && cell.occupiedBy.unitId == unitId)
            {
                if (_views.TryGetValue(cell.occupiedBy.instanceId, out var view))
                {
                    Destroy(view.gameObject);
                    _views.Remove(cell.occupiedBy.instanceId);
                }
                cell.occupiedBy = null;
            }
        }
        EventBus.Publish<bool>(EventNames.OnBoardFullChanged, !HasEmptyCell());
    }

    // ── 드래그 인터페이스 ─────────────────────────────────────
    public void OnDragStart(UnitPiece piece)
    {
        _selectedPiece = piece;
        _originCell    = piece.cell;
        // 드래그 중 출발 셀 occupiedBy 유지 (상태 변경은 PointerUp 시점)
    }

    // CellView.OnDrop → 호출됨
    public void OnDropped(PointerEventData eventData, MergeCell targetCell)
    {
        if (_selectedPiece == null) return;

        var piece = _selectedPiece;
        _selectedPiece = null;

        // A. 같은 셀 드롭 → 원위치
        if (targetCell == _originCell)
        {
            ReturnToOrigin(piece);
            return;
        }

        // B. 빈 셀 → 이동
        if (targetCell.IsEmpty)
        {
            MovePiece(piece, targetCell);
            return;
        }

        // C. 머지 가능 여부 확인
        var target = targetCell.occupiedBy;
        if (target != null
            && target.unitId == piece.unitId
            && target.tier   == piece.tier
            && piece.tier < DataManager.Balance.maxTier)
        {
            MergePieces(piece, target, targetCell);
            return;
        }

        // D. 그 외 (점유된 셀, 머지 불가) → 원위치
        ReturnToOrigin(piece);
    }

    // CellView.OnDrop 없이 드래그 종료 (보드 밖 드롭) → 원위치
    public void OnDragEnd()
    {
        if (_selectedPiece != null)
        {
            ReturnToOrigin(_selectedPiece);
            _selectedPiece = null;
        }
    }

    // ── 이동 ──────────────────────────────────────────────────
    void MovePiece(UnitPiece piece, MergeCell targetCell)
    {
        _originCell.occupiedBy  = null;
        targetCell.occupiedBy   = piece;
        piece.cell              = targetCell;

        if (_views.TryGetValue(piece.instanceId, out var view))
            view.GetComponent<RectTransform>().anchoredPosition =
                CellPosition(targetCell.col, targetCell.row);
    }

    // ── 머지 ──────────────────────────────────────────────────
    void MergePieces(UnitPiece dragPiece, UnitPiece dropTarget, MergeCell targetCell)
    {
        // 1~2. 셀 점유 해제
        _originCell.occupiedBy = null;
        targetCell.occupiedBy  = null;

        // 3~4. 드래그 피스 뷰 제거 (OnCharDied 발행 없음)
        if (_views.TryGetValue(dragPiece.instanceId, out var dragView))
        {
            Destroy(dragView.gameObject);
            _views.Remove(dragPiece.instanceId);
        }
        if (_views.TryGetValue(dropTarget.instanceId, out var targetView))
        {
            Destroy(targetView.gameObject);
            _views.Remove(dropTarget.instanceId);
        }

        // 5~6. tier+1 신규 피스 생성 → 드롭 대상 셀에 배치
        var newPiece = new UnitPiece
        {
            instanceId = Guid.NewGuid().ToString(),
            unitId     = dropTarget.unitId,
            race       = dropTarget.race,
            tier       = dropTarget.tier + 1
        };
        PlacePiece(newPiece, targetCell);

        // 7. 머지 완료 이벤트
        EventBus.Publish<string>(EventNames.OnUnitMerged, newPiece.unitId);

        // 8. 보드 상태 변화 통보
        EventBus.Publish<bool>(EventNames.OnBoardFullChanged, !HasEmptyCell());
    }

    // ── 원위치 복귀 ───────────────────────────────────────────
    void ReturnToOrigin(UnitPiece piece)
    {
        _originCell.occupiedBy = piece;
        if (_views.TryGetValue(piece.instanceId, out var view))
            view.GetComponent<RectTransform>().anchoredPosition =
                CellPosition(_originCell.col, _originCell.row);
    }

    // ── 유틸 쿼리 ─────────────────────────────────────────────
    public MergeCell GetRandomEmptyCell()
    {
        var empty = new System.Collections.Generic.List<MergeCell>();
        foreach (var cell in grid)
            if (cell.IsEmpty) empty.Add(cell);
        if (empty.Count == 0) return null;
        return empty[UnityEngine.Random.Range(0, empty.Count)];
    }

    public bool HasEmptyCell()
    {
        foreach (var cell in grid)
            if (cell.IsEmpty) return true;
        return false;
    }

    /// <summary>보드 위 특정 unitId의 최고 tier 반환. 없으면 0.</summary>
    public int GetMaxTierOf(string unitId)
    {
        int maxTier = 0;
        for (int col = 0; col < COLS; col++)
        for (int row = 0; row < ROWS; row++)
        {
            var piece = grid[col, row].occupiedBy;
            if (piece != null && piece.unitId == unitId)
                maxTier = Mathf.Max(maxTier, piece.tier);
        }
        return maxTier;
    }
}

using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragOffsetY = 2f;

    public BlockPieceData PieceData { get; private set; }
    public Color PieceColor { get; private set; }
    public int SlotIndex { get; private set; }

    private BlockSpawner spawner;
    private GridManager gridManager;
    private Camera mainCamera;

    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private void Start()
    {
        mainCamera = Camera.main;
        gridManager = GridManager.Instance;
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    public void Initialize(BlockPieceData data, Color color, int slot, BlockSpawner spawnerRef)
    {
        PieceData = data;
        PieceColor = color;
        SlotIndex = slot;
        spawner = spawnerRef;
    }

    private void Update()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            if (isDragging) CancelDrag();
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            TryStartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag(Input.mousePosition);
        }
    }

    private void TryStartDrag(Vector2 screenPos)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        foreach (Collider2D hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                StartDrag();
                return;
            }
        }
    }

    private void StartDrag()
    {
        isDragging = true;
        originalPosition = transform.position;
        originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        SetSortingOrder(50);
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        if (mainCamera == null) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        worldPos.y += dragOffsetY;

        transform.position = worldPos;

        if (gridManager != null && PieceData != null)
        {
            Vector2Int gridPos = gridManager.WorldToGrid(worldPos);
            gridManager.ShowPlacementPreview(PieceData, gridPos);
        }
    }

    private void EndDrag(Vector2 screenPos)
    {
        isDragging = false;

        if (mainCamera == null) 
        {
            ReturnToOriginal();
            return;
        }

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        worldPos.y += dragOffsetY;

        if (gridManager != null)
        {
            gridManager.ClearPreview();
        }

        if (gridManager == null || PieceData == null)
        {
            ReturnToOriginal();
            return;
        }

        Vector2Int gridPos = gridManager.WorldToGrid(worldPos);
        bool canPlace = gridManager.CanPlacePiece(PieceData, gridPos);

        if (canPlace)
        {
            PlacePiece(gridPos);
        }
        else
        {
            ReturnToOriginal();
        }
    }

    private void PlacePiece(Vector2Int gridPos)
    {
        if (gridManager == null)
        {
            ReturnToOriginal();
            return;
        }

        PlacementResult result = gridManager.PlacePiece(PieceData, gridPos, PieceColor);

        if (result.Success)
        {
            int baseScore = PieceData.BlockCount;
            int lineBonus = result.LinesCleared * 10;
            int comboBonus = result.IsCombo ? result.LinesCleared * 5 : 0;
            int totalScore = baseScore + lineBonus + comboBonus;

            // CRITICAL: Tell spawner FIRST so AllPiecesPlaced() works correctly
            if (spawner != null)
            {
                spawner.OnPiecePlaced(SlotIndex);
            }

            // THEN notify game manager (which checks AllPiecesPlaced and spawns new set)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBlockPlaced(totalScore, result.LinesCleared, result.IsCombo);
            }

            // Notify UI about line clears for achievements
            if (result.LinesCleared > 0 && UIManager.Instance != null)
            {
                UIManager.Instance.OnLinesCleared(result.LinesCleared, result.IsCombo);
            }

            Destroy(gameObject);
        }
        else
        {
            ReturnToOriginal();
        }
    }

    private void ReturnToOriginal()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        SetSortingOrder(10);
    }

    private void CancelDrag()
    {
        isDragging = false;
        if (gridManager != null) gridManager.ClearPreview();
        ReturnToOriginal();
    }

    private void SetSortingOrder(int order)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
        {
            sr.sortingOrder = order;
        }
    }
}
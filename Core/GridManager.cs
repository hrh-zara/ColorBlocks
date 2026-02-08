using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float cellSize = 0.9f;
    [SerializeField] private float cellSpacing = 0.1f;
    [SerializeField] private Transform gridParent;

    [Header("Visual Settings")]
    [SerializeField] private Color emptyCellColor = new Color(0.35f, 0.3f, 0.45f, 1f);
    [SerializeField] private Color gridBorderColor = new Color(0.25f, 0.2f, 0.35f, 1f);

    private bool[,] gridOccupied;
    private GameObject[,] placedBlocks;
    private GameObject[,] gridCells;
    private SpriteRenderer[,] cellRenderers;
    private Sprite squareSprite;

    public int Width => gridWidth;
    public int Height => gridHeight;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            CreateSquareSprite();
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        squareSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }

    public void InitializeGrid()
    {
        ClearGrid();
        
        if (gridParent == null) gridParent = transform;
        if (squareSprite == null) CreateSquareSprite();

        gridOccupied = new bool[gridWidth, gridHeight];
        placedBlocks = new GameObject[gridWidth, gridHeight];
        gridCells = new GameObject[gridWidth, gridHeight];
        cellRenderers = new SpriteRenderer[gridWidth, gridHeight];

        CreateGridVisuals();
    }

    private void CreateGridVisuals()
    {
        float totalSize = cellSize + cellSpacing;
        float offsetX = (gridWidth - 1) * totalSize / 2f;
        float offsetY = (gridHeight - 1) * totalSize / 2f;

        // Create border
        GameObject border = new GameObject("GridBorder");
        border.transform.parent = gridParent;
        border.transform.localPosition = Vector3.zero;
        SpriteRenderer borderSR = border.AddComponent<SpriteRenderer>();
        borderSR.sprite = squareSprite;
        borderSR.color = gridBorderColor;
        float borderSize = gridWidth * totalSize + 0.5f;
        border.transform.localScale = new Vector3(borderSize, borderSize, 1);
        borderSR.sortingOrder = -1;

        // Create cells
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 cellPos = new Vector3(
                    x * totalSize - offsetX,
                    y * totalSize - offsetY,
                    0
                );

                GameObject cell = new GameObject("Cell_" + x + "_" + y);
                cell.transform.parent = gridParent;
                cell.transform.localPosition = cellPos;
                cell.transform.localScale = Vector3.one * cellSize;

                SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = squareSprite;
                sr.color = emptyCellColor;
                sr.sortingOrder = 0;

                gridCells[x, y] = cell;
                cellRenderers[x, y] = sr;
                gridOccupied[x, y] = false;
            }
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float totalSize = cellSize + cellSpacing;
        float offsetX = (gridWidth - 1) * totalSize / 2f;
        float offsetY = (gridHeight - 1) * totalSize / 2f;

        Vector3 localPos = gridParent.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt((localPos.x + offsetX) / totalSize);
        int y = Mathf.RoundToInt((localPos.y + offsetY) / totalSize);

        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        float totalSize = cellSize + cellSpacing;
        float offsetX = (gridWidth - 1) * totalSize / 2f;
        float offsetY = (gridHeight - 1) * totalSize / 2f;

        Vector3 localPos = new Vector3(
            x * totalSize - offsetX,
            y * totalSize - offsetY,
            0
        );

        return gridParent.TransformPoint(localPos);
    }

    public bool CanPlacePiece(BlockPieceData piece, Vector2Int gridPos)
    {
        if (piece == null) return false;

        foreach (Vector2Int offset in piece.BlockOffsets)
        {
            int checkX = gridPos.x + offset.x;
            int checkY = gridPos.y + offset.y;

            if (checkX < 0 || checkX >= gridWidth || checkY < 0 || checkY >= gridHeight)
                return false;

            if (gridOccupied[checkX, checkY])
                return false;
        }
        return true;
    }

    public PlacementResult PlacePiece(BlockPieceData piece, Vector2Int gridPos, Color color)
    {
        PlacementResult result = new PlacementResult();

        if (!CanPlacePiece(piece, gridPos))
        {
            result.Success = false;
            return result;
        }

        foreach (Vector2Int offset in piece.BlockOffsets)
        {
            int placeX = gridPos.x + offset.x;
            int placeY = gridPos.y + offset.y;

            gridOccupied[placeX, placeY] = true;

            Vector3 worldPos = GridToWorld(placeX, placeY);
            
            GameObject block = new GameObject("Block_" + placeX + "_" + placeY);
            block.transform.position = worldPos;
            block.transform.parent = gridParent;
            block.transform.localScale = Vector3.one * cellSize * 0.95f;

            SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            sr.color = color;
            sr.sortingOrder = 1;

            placedBlocks[placeX, placeY] = block;
        }

        result.Success = true;
        result.LinesCleared = CheckAndClearLines();
        result.IsCombo = result.LinesCleared > 1;

        return result;
    }

    private int CheckAndClearLines()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        for (int y = 0; y < gridHeight; y++)
        {
            bool rowComplete = true;
            for (int x = 0; x < gridWidth; x++)
            {
                if (!gridOccupied[x, y]) { rowComplete = false; break; }
            }
            if (rowComplete) rowsToClear.Add(y);
        }

        for (int x = 0; x < gridWidth; x++)
        {
            bool colComplete = true;
            for (int y = 0; y < gridHeight; y++)
            {
                if (!gridOccupied[x, y]) { colComplete = false; break; }
            }
            if (colComplete) colsToClear.Add(x);
        }

        foreach (int y in rowsToClear)
            for (int x = 0; x < gridWidth; x++) 
                ClearCell(x, y);

        foreach (int x in colsToClear)
            for (int y = 0; y < gridHeight; y++) 
                ClearCell(x, y);

        return rowsToClear.Count + colsToClear.Count;
    }

    private void ClearCell(int x, int y)
    {
        if (placedBlocks[x, y] != null)
        {
            Destroy(placedBlocks[x, y]);
            placedBlocks[x, y] = null;
        }
        gridOccupied[x, y] = false;
    }

    public void ShowPlacementPreview(BlockPieceData piece, Vector2Int gridPos)
    {
        ClearPreview();
        if (piece == null) return;

        bool canPlace = CanPlacePiece(piece, gridPos);
        Color highlight = canPlace ? new Color(0.3f, 0.9f, 0.3f, 0.8f) : new Color(0.9f, 0.3f, 0.3f, 0.8f);

        foreach (Vector2Int offset in piece.BlockOffsets)
        {
            int cx = gridPos.x + offset.x;
            int cy = gridPos.y + offset.y;
            if (cx >= 0 && cx < gridWidth && cy >= 0 && cy < gridHeight)
            {
                if (cellRenderers[cx, cy] != null && !gridOccupied[cx, cy])
                    cellRenderers[cx, cy].color = highlight;
            }
        }
    }

    public void ClearPreview()
    {
        if (cellRenderers == null) return;
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (cellRenderers[x, y] != null && !gridOccupied[x, y])
                    cellRenderers[x, y].color = emptyCellColor;
    }

    public bool CanAnyPieceFit(List<BlockPieceData> pieces)
    {
        foreach (BlockPieceData piece in pieces)
            if (piece != null && CanPieceFitAnywhere(piece)) return true;
        return false;
    }

    public bool CanPieceFitAnywhere(BlockPieceData piece)
    {
        if (piece == null) return false;
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (CanPlacePiece(piece, new Vector2Int(x, y))) return true;
        return false;
    }

    public void ClearGrid()
    {
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridParent.GetChild(i).gameObject);
            }
        }
    }

    private void OnDestroy() 
    { 
        if (Instance == this) Instance = null; 
    }
}

public struct PlacementResult
{
    public bool Success;
    public int LinesCleared;
    public bool IsCombo;
}

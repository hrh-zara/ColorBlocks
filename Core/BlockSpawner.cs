using UnityEngine;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints = new Transform[3];
    [SerializeField] private float pieceScale = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color[] blockColors = new Color[]
    {
        new Color(0.95f, 0.3f, 0.3f, 1f),
        new Color(0.3f, 0.85f, 0.3f, 1f),
        new Color(0.3f, 0.5f, 0.95f, 1f),
        new Color(0.95f, 0.85f, 0.2f, 1f),
        new Color(0.95f, 0.5f, 0.2f, 1f),
        new Color(0.7f, 0.3f, 0.9f, 1f),
        new Color(0.3f, 0.9f, 0.9f, 1f),
        new Color(0.95f, 0.4f, 0.7f, 1f)
    };

    private GameObject[] currentPieceObjects = new GameObject[3];
    private BlockPieceData[] currentPieceData = new BlockPieceData[3];
    private Difficulty currentDifficulty = Difficulty.Easy;
    private Sprite squareSprite;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            CreateSquareSprite();
        }
        else Destroy(gameObject);
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

    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
    }

    public void SpawnNewSet()
    {
        ClearAllPieces();

        List<BlockPieceData> allPieces = BlockPieceDefinitions.GetPiecesForDifficulty(currentDifficulty);
        List<int> usedIndices = new List<int>();
        
        for (int i = 0; i < 3; i++)
        {
            int index;
            int attempts = 0;
            do {
                index = Random.Range(0, allPieces.Count);
                attempts++;
            } while (usedIndices.Contains(index) && attempts < 20);
            
            usedIndices.Add(index);
            Color pieceColor = blockColors[Random.Range(0, blockColors.Length)];
            SpawnPieceAtSlot(i, allPieces[index], pieceColor);
        }
    }

    private void SpawnPieceAtSlot(int slotIndex, BlockPieceData pieceData, Color pieceColor)
    {
        if (slotIndex < 0 || slotIndex >= 3) return;
        if (spawnPoints[slotIndex] == null) return;

        GameObject pieceParent = new GameObject("Piece_" + slotIndex + "_" + pieceData.Name);
        pieceParent.transform.position = spawnPoints[slotIndex].position;
        pieceParent.transform.localScale = Vector3.one * pieceScale;

        DraggablePiece draggable = pieceParent.AddComponent<DraggablePiece>();
        draggable.Initialize(pieceData, pieceColor, slotIndex, this);

        Vector2 center = CalculatePieceCenter(pieceData);

        foreach (Vector2Int offset in pieceData.BlockOffsets)
        {
            GameObject block = new GameObject("Block");
            block.transform.parent = pieceParent.transform;
            block.transform.localPosition = new Vector3(offset.x - center.x, offset.y - center.y, 0);
            block.transform.localScale = Vector3.one * 0.85f;

            SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            sr.color = pieceColor;
            sr.sortingOrder = 10;

            BoxCollider2D col = block.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * 0.9f;
        }

        currentPieceObjects[slotIndex] = pieceParent;
        currentPieceData[slotIndex] = pieceData;
    }

    private Vector2 CalculatePieceCenter(BlockPieceData piece)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector2Int offset in piece.BlockOffsets)
        {
            if (offset.x < minX) minX = offset.x;
            if (offset.x > maxX) maxX = offset.x;
            if (offset.y < minY) minY = offset.y;
            if (offset.y > maxY) maxY = offset.y;
        }

        return new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
    }

    public void OnPiecePlaced(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 3)
        {
            currentPieceObjects[slotIndex] = null;
            currentPieceData[slotIndex] = null;
        }
    }

    public bool AllPiecesPlaced()
    {
        for (int i = 0; i < 3; i++)
            if (currentPieceObjects[i] != null) return false;
        return true;
    }

    public List<BlockPieceData> GetRemainingPieces()
    {
        List<BlockPieceData> remaining = new List<BlockPieceData>();
        for (int i = 0; i < 3; i++)
            if (currentPieceData[i] != null && currentPieceObjects[i] != null)
                remaining.Add(currentPieceData[i]);
        return remaining;
    }

    public void ClearAllPieces()
    {
        for (int i = 0; i < 3; i++)
        {
            if (currentPieceObjects[i] != null)
            {
                Destroy(currentPieceObjects[i]);
                currentPieceObjects[i] = null;
            }
            currentPieceData[i] = null;
        }
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
}

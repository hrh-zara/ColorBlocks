using UnityEngine;
using System.Collections.Generic;

public enum Difficulty { Easy, Medium, Hard }

[System.Serializable]
public class BlockPieceData
{
    public string Name;
    public Vector2Int[] BlockOffsets;
    public Color PieceColor;
    public Difficulty MinDifficulty;

    public BlockPieceData(string name, Vector2Int[] offsets, Color color, Difficulty minDiff = Difficulty.Easy)
    {
        Name = name;
        BlockOffsets = offsets;
        PieceColor = color;
        MinDifficulty = minDiff;
    }

    public int BlockCount => BlockOffsets.Length;

    public static BlockPieceData GetRandomPiece(Difficulty difficulty)
    {
        List<BlockPieceData> pieces = BlockPieceDefinitions.GetPiecesForDifficulty(difficulty);
        if (pieces.Count == 0) return null;
        return pieces[Random.Range(0, pieces.Count)];
    }
}

public static class BlockPieceDefinitions
{
    public static readonly Color ColorRed = new Color(0.9f, 0.25f, 0.25f);
    public static readonly Color ColorGreen = new Color(0.3f, 0.8f, 0.3f);
    public static readonly Color ColorBlue = new Color(0.3f, 0.5f, 0.9f);
    public static readonly Color ColorYellow = new Color(0.95f, 0.8f, 0.2f);
    public static readonly Color ColorPurple = new Color(0.6f, 0.3f, 0.8f);
    public static readonly Color ColorCyan = new Color(0.3f, 0.85f, 0.85f);
    public static readonly Color ColorOrange = new Color(0.95f, 0.5f, 0.2f);
    public static readonly Color ColorPink = new Color(0.9f, 0.4f, 0.7f);

    public static List<BlockPieceData> GetPiecesForDifficulty(Difficulty difficulty)
    {
        List<BlockPieceData> pieces = new List<BlockPieceData>();

        // EASY PIECES
        pieces.Add(new BlockPieceData("Single", new Vector2Int[] {
            new Vector2Int(0, 0)
        }, ColorYellow, Difficulty.Easy));

        pieces.Add(new BlockPieceData("Domino_H", new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(1, 0)
        }, ColorGreen, Difficulty.Easy));

        pieces.Add(new BlockPieceData("Domino_V", new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(0, 1)
        }, ColorPink, Difficulty.Easy));

        pieces.Add(new BlockPieceData("Line3_H", new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
        }, ColorOrange, Difficulty.Easy));

        pieces.Add(new BlockPieceData("Line3_V", new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)
        }, ColorCyan, Difficulty.Easy));

        pieces.Add(new BlockPieceData("Square2x2", new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(1, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 1)
        }, ColorYellow, Difficulty.Easy));

        pieces.Add(new BlockPieceData("SmallL", new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1)
        }, ColorPink, Difficulty.Easy));

        // MEDIUM PIECES
        if (difficulty >= Difficulty.Medium)
        {
            pieces.Add(new BlockPieceData("Line4_H", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(1, 0), 
                new Vector2Int(2, 0), new Vector2Int(3, 0)
            }, ColorCyan, Difficulty.Medium));

            pieces.Add(new BlockPieceData("Line4_V", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(0, 1), 
                new Vector2Int(0, 2), new Vector2Int(0, 3)
            }, ColorBlue, Difficulty.Medium));

            pieces.Add(new BlockPieceData("L_Right", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(0, 1), 
                new Vector2Int(0, 2), new Vector2Int(1, 0)
            }, ColorOrange, Difficulty.Medium));

            pieces.Add(new BlockPieceData("L_Left", new Vector2Int[] {
                new Vector2Int(1, 0), new Vector2Int(1, 1), 
                new Vector2Int(1, 2), new Vector2Int(0, 0)
            }, ColorPurple, Difficulty.Medium));

            pieces.Add(new BlockPieceData("T_Up", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(1, 1)
            }, ColorPurple, Difficulty.Medium));
        }

        // HARD PIECES
        if (difficulty >= Difficulty.Hard)
        {
            pieces.Add(new BlockPieceData("Line5_H", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(3, 0), new Vector2Int(4, 0)
            }, ColorBlue, Difficulty.Hard));

            pieces.Add(new BlockPieceData("Line5_V", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2),
                new Vector2Int(0, 3), new Vector2Int(0, 4)
            }, ColorPurple, Difficulty.Hard));

            pieces.Add(new BlockPieceData("Square3x3", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
                new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)
            }, ColorRed, Difficulty.Hard));

            pieces.Add(new BlockPieceData("BigL", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2),
                new Vector2Int(1, 0), new Vector2Int(2, 0)
            }, ColorBlue, Difficulty.Hard));
        }

        return pieces;
    }

    public static Color GetRandomColor()
    {
        Color[] colors = { ColorRed, ColorGreen, ColorBlue, ColorYellow, 
                          ColorPurple, ColorCyan, ColorOrange, ColorPink };
        return colors[Random.Range(0, colors.Length)];
    }
}

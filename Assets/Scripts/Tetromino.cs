using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z
}

[System.Serializable]
public struct TetrominoData
{
    public Tetromino Tetromino;
    public Tile Tile;
    public Vector2Int[] Cells { get; private set; }
    public Vector2Int[,] WallKicks { get; private set; }

    public void Initialize()
    {
        // Bunun yerine koordinatlar elle de verilebilir.
        Cells = Data.Cells[Tetromino];
        WallKicks = Data.WallKicks[Tetromino];
    }
}
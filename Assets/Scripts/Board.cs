using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap Tilemap { get; private set; }
    public Piece ActivePiece { get; private set; }

    public TetrominoData[] TetrominoDatas;
    public Vector3Int SpawnPosition = new Vector3Int(-1, 8, 0);
    public Vector2Int BoardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-BoardSize.x / 2, -BoardSize.y / 2);
            return new RectInt(position, BoardSize);
        }
    }

    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        ActivePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < TetrominoDatas.Length; i++)
        {
            TetrominoDatas[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int rand = Random.Range(0, TetrominoDatas.Length);
        TetrominoData data = TetrominoDatas[rand];

        ActivePiece.Initialize(this, SpawnPosition, data);

        if (IsValidPosition(ActivePiece, SpawnPosition))
        {
            Set(ActivePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void SpawnPiece(TetrominoData data)
    {
        ActivePiece.Initialize(this, SpawnPosition, data);

        if (IsValidPosition(ActivePiece, SpawnPosition))
        {
            Set(ActivePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        Tilemap.ClearAllTiles();

        // Do other things
    }

    public void Set(Piece piece)
    {
        for(int i = 0;i < piece.Cells.Length;i++)
        {
            Vector3Int tilePos = piece.Cells[i] + piece.Position;
            Tilemap.SetTile(tilePos, piece.TetrominoData.Tile);
        }
    }

    public void Clear(Piece piece)
    {
        for(int i = 0;i < piece.Cells.Length;i++)
        {
            Vector3Int tilePos = piece.Cells[i] + piece.Position;
            Tilemap.SetTile(tilePos, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for(int i = 0; i < piece.Cells.Length; i++)
        {
            Vector3Int tilePos = piece.Cells[i] + position;

            if(!bounds.Contains((Vector2Int) tilePos))
            {
                return false;
            }

            if(Tilemap.HasTile(tilePos))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        while(row < bounds.yMax)
        {
            if(IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if(!Tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            Tilemap.SetTile(position, null);
        }

        // Grab tiles above removed tiles down
        while(row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = Tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                Tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}

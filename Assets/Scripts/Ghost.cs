using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile Tile;
    public Board Board;
    public Piece TrackingPiece;

    public Tilemap Tilemap { get; private set; }
    public Vector3Int Position { get; private set; }
    public Vector3Int[] Cells { get; private set; }

    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        Cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3Int tilePos = Cells[i] + Position;
            Tilemap.SetTile(tilePos, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i] = TrackingPiece.Cells[i];
        }
    }

    private void Drop()
    {
        Vector3Int position = TrackingPiece.Position;

        int currentRow = position.y;
        int bottom = -Board.BoardSize.y / 2 - 1;

        // To Check the positions below the trackingpiece, first we need to clear it.
        Board.Clear(TrackingPiece);

        for (int row = currentRow; row >= bottom; row--)
        {
            position.y = row;

            if (Board.IsValidPosition(TrackingPiece, position))
            {
                Position = position;
            }
            else
            {
                break;
            }
        }

        Board.Set(TrackingPiece);
    }

    private void Set()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3Int tilePos = Cells[i] + Position;
            Tilemap.SetTile(tilePos, Tile);
        }
    }
}

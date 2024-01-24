using UnityEngine;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public Board Board { get; private set; }
    public TetrominoData TetrominoData { get; private set; }
    public Vector3Int Position { get; private set; }
    public Vector3Int[] Cells { get; private set; }
    public int RotationIndex { get; private set; }

    [Header("Piece Step and Lock")]
    public float FreeStepDelay = 1f;
    public float ControlledStepDelay = 0.5f;
    public float LockDelay = 0.5f;

    private float FreeStepTime;
    private float ControlledStepTime;
    private float LockTime;

    [Header("Piece Holding")]
    public bool IsHoldingPiece;
    public bool HoldedThisTurn;
    public TetrominoData HoldedPiece;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        Board = board;
        Position = position;
        TetrominoData = data;
        RotationIndex = 0;
        FreeStepTime = Time.time + this.FreeStepDelay;
        ControlledStepTime = 0f;
        LockTime = 0f;
        HoldedThisTurn = false;

        if (Cells == null)
        {
            Cells = new Vector3Int[data.Cells.Length];
        }

        for(int i = 0; i < Cells.Length; i++)
        {
            Cells[i] = (Vector3Int) data.Cells[i];
        }
    }

    private void Update()
    {
        Board.Clear(this);

        LockTime += Time.deltaTime;
        ControlledStepTime += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }

        if(Input.GetKey(KeyCode.A))
        {
            if(ControlledStepTime >= ControlledStepDelay)
            {
                ControlledStepTime = 0f;
                Move(Vector2Int.left);
            }
        }
        else if(Input.GetKey(KeyCode.D))
        {
            if (ControlledStepTime >= ControlledStepDelay)
            {
                ControlledStepTime = 0f;
                Move(Vector2Int.right);
            }
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            if (ControlledStepTime >= ControlledStepDelay)
            {
                ControlledStepTime = 0f;
                Move(Vector2Int.down);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            Hold();
        }

        if(Time.time >= FreeStepTime)
        {
            Step();
        }

        Board.Set(this);
    }

    // Sonsuz kez tile hareket ettirmeyi kapatmak için burada step atlayamadýðýmýzý kontrol ettirip, lockTime'ý move'da sýfýrlamayý engellememiz gerekiyor.
    private void Step()
    {
        FreeStepTime = Time.time + FreeStepDelay;

        Move(Vector2Int.down);

        if(LockTime >= LockDelay)
        {
            Lock();
        }
    }

    private void Hold()
    {
        if(!HoldedThisTurn)
        {
            HoldedThisTurn = true;

            if (!IsHoldingPiece)
            {
                IsHoldingPiece = true;

                HoldedPiece = TetrominoData;
                Board.Clear(this);
                Lock();
            }
            else
            {
                TetrominoData holdedPiece = HoldedPiece;
                HoldedPiece = TetrominoData;

                Board.Clear(this);
                Board.SpawnPiece(holdedPiece);
            }
        }
    }

    private void HardDrop()
    {
        while(Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        Board.Set(this);
        Board.ClearLines();
        Board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = Position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = Board.IsValidPosition(this, newPosition);

        if(valid)
        {
            Position = newPosition;
            LockTime = 0f;
        }

        return valid;
    }

    public void Rotate(int direction)
    {
        int originalRotation = RotationIndex;
        RotationIndex = Wrap(RotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if(!TestWallKicks(RotationIndex, direction))
        {
            RotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] Matrix = Data.RotationMatrix;

        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3 cell = Cells[i];

            int x, y;

            switch (TetrominoData.Tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Matrix[0] * direction) + (cell.y * Matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Matrix[2] * direction) + (cell.y * Matrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * Matrix[0] * direction) + (cell.y * Matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Matrix[2] * direction) + (cell.y * Matrix[3] * direction));
                    break;
            }

            Cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);
        
        for(int i = 0; i < TetrominoData.WallKicks.GetLength(1); i++)
        {
            Vector2Int translation = TetrominoData.WallKicks[wallKickIndex, i];

            if(Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if(rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, TetrominoData.WallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if(input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
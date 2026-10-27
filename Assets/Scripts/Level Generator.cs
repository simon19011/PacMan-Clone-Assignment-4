using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Tilemap tileMap;

    [Header("Tiles")]
    public TileBase outsideCorner;
    public TileBase outsideWall;
    public TileBase insideCorner;
    public TileBase insideWall;
    public TileBase pellet;
    public TileBase powerPellet;
    public TileBase tJunction;
    public TileBase ghostExitWall;

    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    public int[,] fullMap;
    void Awake()
    {
        GenerateLevel();
    }

    private int[,] GenerateFullMap()
    {
        int height = levelMap.GetLength(0);
        int width = levelMap.GetLength(1);

        int fullHeight = height * 2 - 1;
        int fullWidth = width * 2;

        int[,] fullMap = new int[fullHeight, fullWidth];

        for (int row = 0; row < fullHeight; row++)
        {
            for (int col = 0; col < fullWidth; col++)
            {
                int sourceRow = row < height ? row : fullHeight - 1 - row;
                int sourceCol = col < width ? col : fullWidth -1 - col;

                fullMap[row, col] = levelMap[sourceRow, sourceCol];
            }
        }

        return fullMap;
    }

    private void GenerateLevel()
    {
        tileMap.ClearAllTiles();

        fullMap = GenerateFullMap();

        int height = fullMap.GetLength(0);
        int width = fullMap.GetLength(1);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                PlaceTile(row, col, fullMap[row, col]);
            }
        }
    }

    private void PlaceTile(int row, int col, int tileID)
    {
        TileBase tile = GetTileFromID(tileID);

        if (tile == null)
        {
            return;
        }

        Vector3Int cellPos = new Vector3Int(col, -row, 0);

        tileMap.SetTile(cellPos, tile);

        tileMap.SetTileFlags(cellPos, TileFlags.None);

        float rotation = GetRotation(row, col, tileID);
        Vector3 scale = GetScale(row, col, tileID);

        Matrix4x4 matrix = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.Euler(0f, 0f, rotation),
            scale
        );

        tileMap.SetTransformMatrix(cellPos, matrix);
    }

    // Public functions to check for walkability (added here to keep tileMap data in a single place)
    public bool IsWalkableCell(Vector3Int cell)
    {
        if (fullMap == null)
        {
            return false;
        }

        int row = -cell.y;
        int col = cell.x;

        if (row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1))
        {
            return false;
        }

        int tileID = fullMap[row, col];

        return tileID == 5 || tileID == 6 || tileID == 0;
    }

    public bool IsPelletCell(Vector3Int cell)
    {
        if (fullMap == null)
        {
            return false;
        }

        int row = -cell.y;
        int col = cell.x;

        if (row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1))
        {
            return false;
        }

        int tileID = fullMap[row, col];

        return tileID == 5 || tileID == 6;
    }

    public bool IsTeleporter(Vector3Int cell)
    {
        int row = -cell.y;
        int col = cell.x;

        return row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1);
    }

    public int CollectItem(Vector3Int cell)
    {
        if (fullMap == null || tileMap == null)
        {
            return 0;
        }

        int row = -cell.y;
        int col = cell.x;

        if (row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1))
        {
            return 0;
        }

        int tileID = fullMap[row, col];

        if (tileID != 5 && tileID != 6)
        {
            return 0;
        }

        tileMap.SetTile(cell, null);
        fullMap[row, col] = 0;

        return tileID;
    }

    private bool IsWall(int row, int col)
    {
        if (row < 0 || row >= fullMap.GetLength(0) ||
            col < 0 || col >= fullMap.GetLength(1))
        {
            return false;
        }

        int tileID = fullMap[row, col];

        return tileID == 1 || tileID == 2 || tileID == 3 || tileID == 4 || tileID == 7 || tileID == 8;
    }

    private bool IsOutsideWall(int row, int col)
    {
        if (row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1)) 
        {
            return false;
        }

        int tileID = fullMap[row, col];

        return tileID == 1 || tileID == 2;
    }

    private bool IsInsideWall(int row, int col)
    {
        if (row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1))
        {
            return false;
        }

        int tileID = fullMap[row, col];

        return tileID == 3 || tileID == 4;
    }

    private bool IsTJunction(int row, int col)
    {
        if (row < 0 || row >= fullMap.GetLength(0) || col < 0 || col >= fullMap.GetLength(1))
        {
            return false;
        }

        return fullMap[row, col] == 7;
    }

    private int GetConnectionMask(int row, int col)
    {
        // Returns bitmask for easier direction checking
        int mask = 0;

        if (IsWall(row - 1, col))
        {
            mask |= 1; // Up
        }

        if (IsWall(row + 1, col))
        {
            mask |= 2; // Down
        }

        if (IsWall(row, col - 1))
        {
            mask |= 4; // Left
        }

        if (IsWall(row, col + 1))
        {
            mask |= 8; // Right
        }

        return mask;
    }

    private float GetRotation(int row, int col, int tileID)
    {
        int mask = GetConnectionMask(row, col);
        
        // Corners
        if (tileID == 1 || tileID == 3)
        {
            if (mask == 10)
            {
                return 0f;
            }

            if (mask == 6)
            {
                return 270f;
            }

            if (mask == 5)
            {
                return 180f;
            }

            if (mask == 9)
            {
                return 90f;
            }
        }

        // Walls
        if (tileID == 2 || tileID == 4)
        {
            if (mask == 3 || (mask - 3) == 4 || (mask - 3) == 8)
            {
                return 0f;
            }

            else if (mask == 12 || (mask - 12) == 1 || (mask - 12) == 2)
            {
                return 90f;
            }
            
            else if (mask == 1 || mask == 2)
            {
                return 0f;
            }

            else 
            {
                return 90f;
            }
        }

        // T Junction
        if (tileID == 7)
        {
            if (mask == 14)
            {
                return 0f;
            }

            if (mask == 7)
            {
                return 270f;
            }

            if (mask == 13)
            {
                return 180f;
            }

            if (mask == 11)
            {
                return 90f;
            }
        }

        return 0f;
    }

    private Vector3 GetScale(int row, int col, int tileID)
    {
        if (tileID != 7)
        {
            return Vector3.one;
        }

        int mask = GetConnectionMask(row, col);

        bool outsideLeft = IsOutsideWall(row, col - 1);
        bool outsideRight = IsOutsideWall(row, col + 1);

        bool tLeft = IsTJunction(row, col - 1);
        bool tRight = IsTJunction(row, col + 1);

        if (mask == 14)
        {
            if (outsideLeft && tRight)
            {
                return Vector3.one;
            }

            if (outsideRight && tLeft)
            {
                return new Vector3(-1f, 1f, 1f);
            }
        }

        return Vector3.one;
    }

    private TileBase GetTileFromID(int tileID)
    {
        switch (tileID)
        {
            case 1:
                return outsideCorner;

            case 2:
                return outsideWall;

            case 3:
                return insideCorner;

            case 4:
                return insideWall;

            case 5:
                return pellet;

            case 6:
                return powerPellet;

            case 7:
                return tJunction;

            case 8:
                return ghostExitWall;

            case 0:
                return null;

            default:
                return null;
        }
    }
}

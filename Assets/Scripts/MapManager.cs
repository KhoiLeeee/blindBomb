using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor.Timeline;
using UnityEngine;

public enum CellType
{
    Empty, 
    IndestructibleWall,
    DestructibleWall,
    Bomb,
    Player,
    Enemy,
    Explosion,
    Item,
}
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public int width;
    public int height;
    private CellType[,] mapGrid;
    private  int[,] explosionMap;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeMap();
    }

    private void InitializeMap()
    {
        InitializeMapGrid();
        InitializeExplosionMap();
    }
    public void SetCell(Vector2Int position, CellType type)
    {
        int x = position.x + width / 2;
        int y = position.y + height / 2;
        if (IsInsideMap(x, y))
            mapGrid[x, y] = type;
    }
    public CellType GetCell(Vector2Int position)
    {
        int x = position.x + width / 2;
        int y = position.y + height / 2;
        if (IsInsideMap(x, y))
        {
            return mapGrid[x, y];
        } else
        {
            return CellType.IndestructibleWall;
        }
    }
    public bool IsInsideMap(int x, int y)
    {
        return x >= 0 && x < this.width && y >= 0 && y < this.height;
    }

    public bool IsInsideMap(Vector2Int position)
    {
        int x = position.x + width / 2;
        int y = position.y + height / 2;
        return x >= 0 && x < this.width && y >= 0 && y < this.height;
    }
    public CellType[,] getMapGrid()
    {
        return mapGrid;
    }
    public void PrintMap()
    {
        for (int y = height - 1; y >= 0; y--) // Top to bottom
        {
            string row = "";
            for (int x = 0; x < width; x++)
            {
                row += mapGrid[x, y].ToString().PadRight(10); // Add some spacing if you want
            }
            if (y == 1)
            {
                Debug.Log(y);
                Debug.Log(row);
            }
            
        }
    }

    public List<Tuple<int, int>> Vec2IntToGridBased(List<Vector2Int> positions)
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
        foreach (Vector2Int position in positions)
        {
            int x = position.x + width / 2;
            int y = position.y + height / 2;
            tuples.Add(Tuple.Create(x, y));
        }

        return tuples;
    }

    public Tuple<int, int> Vec2IntToGridBased(Vector2Int position)
    {
        int x = position.x + width / 2;
        int y = position.y + height / 2;
        return Tuple.Create(x, y);
    }

    private void InitializeMapGrid()
    {
        mapGrid = new CellType[width, height];
        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector2 worldPos = new Vector2(x + 0.5f, y + 0.5f);
                Collider2D collider = Physics2D.OverlapPoint(worldPos);
                if (collider != null)
                {
                    if (collider.CompareTag("Indestructible"))
                    {
                        mapGrid[x + width / 2, y + height / 2] = CellType.IndestructibleWall;
                    }
                    else if (collider.CompareTag("Destructible"))
                    {
                        mapGrid[x + width / 2, y + height / 2] = CellType.DestructibleWall;
                    }
                    else if (collider.CompareTag("Player"))
                    {
                        mapGrid[x + width / 2, y + height / 2] = CellType.Player;
                    }
                    else if (collider.CompareTag("Enemy"))
                    {
                        mapGrid[x + width / 2, y + height / 2] = CellType.Enemy;
                    }
                }
                else
                {
                    mapGrid[x + width / 2, y + height / 2] = CellType.Empty;
                }
            }
        }
    }

    private void InitializeExplosionMap()
    {
        explosionMap = new int [width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                explosionMap[i, j] = 0;
            }
        }
    }

    private void UpdateExplosionMap()
    {
        InitializeExplosionMap();
        List<GameObject> bombs = GameManager.GetAllBombs();
        foreach (GameObject bomb in bombs)
        {
            if (bomb == null) continue;
            Bomb bombComponent = bomb.GetComponent<Bomb>();
            Tuple<int, int> bombPos = Vec2IntToGridBased(new Vector2Int(Mathf.FloorToInt(bomb.transform.position.x), Mathf.FloorToInt(bomb.transform.position.y)));
            int x = bombPos.Item1;
            int y = bombPos.Item2;

            int remainTime = (int)bombComponent.GetRemainingTime() + 1;
            explosionMap[x, y] = Mathf.Max(explosionMap[x, y], remainTime);
            for (int i = 1; i <= bombComponent.GetExplosionRadius(); i++)
            {
                explosionMap[x + i, y] = Mathf.Max(explosionMap[x + i, y], remainTime);
                explosionMap[x - i, y] = Mathf.Max(explosionMap[x - i, y], remainTime);
                explosionMap[x, y + i] = Mathf.Max(explosionMap[x, y + i], remainTime);
                explosionMap[x, y - i] = Mathf.Max(explosionMap[x, y - i], remainTime);
            }
        }
    }

    public int[,] GetExplosionMap()
    {
        UpdateExplosionMap();
        return explosionMap;
    }

}

using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using JetBrains.Annotations;
using System.Collections;

public class RuleBasedAgent : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;
    private string agentName;
    public float speed = 4f;
    public int level;
    private bool isWaiting = false;
    public float waitTime = 0.5f;

    private Queue<Tuple<int, int>> bombHistory = new Queue<Tuple<int, int>>(5);
    private Queue<Tuple<int, int>> coordinateHistory = new Queue<Tuple<int, int>>(20);
    private int ignore_others_timer = 0;

    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    [Header("Sprites")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
    }

    private void Start()
    {
        agentName = gameObject.name;
        //Debug.Log("This is: " + agentName);

        switch (level) {
            case 0:
                ApplyRuleBasedMedium();
                break;
            case 1:
                ApplyComplexRuleBased();
                break;
            default:
                ApplyRuleBasedMedium();
                break;
        }
    }
    private void Update()
    {
        if (isWaiting) return;

        Vector2 playerPosition = transform.position;
        Tuple<int, int> pos = Tuple.Create(Mathf.FloorToInt(playerPosition.x), Mathf.FloorToInt(playerPosition.y));
        EnqueueWithLimit(bombHistory, pos, 5);
        EnqueueWithLimit(coordinateHistory, pos, 20);

        string action = ApplyComplexRuleBased();
        Debug.Log("Action: " + action);

        StartCoroutine(PerformActionWithDelay(action));
    }

    private IEnumerator PerformActionWithDelay(string action)
    {
        isWaiting = true;

        // Apply the action
        switch (action)
        {
            case "UP":
                SetDirection(Vector2.up, spriteRendererUp);
                break;
            case "DOWN":
                SetDirection(Vector2.down, spriteRendererDown);
                break;
            case "LEFT":
                SetDirection(Vector2.left, spriteRendererLeft);
                break;
            case "RIGHT":
                SetDirection(Vector2.right, spriteRendererRight);
                break;
            case "BOMB":
                GetComponent<BombController>().PlaceBombManually();
                SetDirection(Vector2.zero, activeSpriteRenderer);
                break;
            case "WAIT":
                SetDirection(Vector2.zero, activeSpriteRenderer);
                break;
        }

        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
    }
    private void FixedUpdate()
    {
        Vector2 position = rb.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        Vector2Int curCell = new Vector2Int(
            Mathf.FloorToInt(position.x),
            Mathf.FloorToInt(position.y)
        );
        Vector2Int newCell = new Vector2Int(
           Mathf.FloorToInt(position.x + translation.x),
            Mathf.FloorToInt(position.y + translation.y)
        );

        MapManager.Instance.SetCell(curCell, CellType.Empty);
        MapManager.Instance.SetCell(newCell, CellType.Enemy);

        rb.MovePosition(position + translation);
    }

    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;

        spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
        spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
        spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
        spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombController>().enabled = false;

        spriteRendererUp.enabled = false;
        spriteRendererDown.enabled = false;
        spriteRendererLeft.enabled = false;
        spriteRendererRight.enabled = false;
        spriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        Manager.Instance.CheckWinCondition();
    }

    private string ApplyRuleBasedMedium()
    {
        CellType[,] mapGrid = MapManager.Instance.getMapGrid();
        int bomb_left = GameManager.GetBombLeft(this.agentName);
        Vector2Int position = new Vector2Int(Mathf.FloorToInt(rb.position.x), Mathf.FloorToInt(rb.position.y));
        Tuple<int, int> self_position = MapManager.Instance.Vec2IntToGridBased(position);

        List<GameObject> bombs = GameManager.GetAllBombs();
        List<Tuple<int, int>> bomb_position = new List<Tuple<int, int>>();
        float[,] bombMap = new float[mapGrid.GetLength(0), mapGrid.GetLength(1)];
        for (int i = 0; i < bombMap.GetLength(0); i++)
            for (int j = 0; j < bombMap.GetLength(1); j++)
                bombMap[i, j] = 5f;

        foreach (GameObject bomb in bombs)
        {
            if (bomb == null) continue;
            Bomb bomb_component = bomb.GetComponent<Bomb>();
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(bomb.transform.position.x), Mathf.FloorToInt(bomb.transform.position.y));
            Tuple<int, int> bombXY = MapManager.Instance.Vec2IntToGridBased(pos);
            bomb_position.Add(bombXY);

            for (int h = -3; h <= 3; h++)
            {
                Tuple<int, int>[] points = {
                Tuple.Create(bombXY.Item1 + h, bombXY.Item2),
                Tuple.Create(bombXY.Item1, bombXY.Item2 + h)
            };
                foreach (var point in points)
                {
                    if (point.Item1 > 0 && point.Item1 < bombMap.GetLength(0) &&
                        point.Item2 > 0 && point.Item2 < bombMap.GetLength(1))
                        bombMap[point.Item1, point.Item2] = Mathf.Min(bombMap[point.Item1, point.Item2], bomb_component.GetRemainingTime());
                }
            }
        }

        List<Tuple<int, int>> other_position = MapManager.Instance.Vec2IntToGridBased(GameManager.GetOtherPosition(this.agentName));
        int[,] explosionMap = MapManager.Instance.GetExplosionMap();

        Tuple<int, int>[] directions = {
        self_position,
        Tuple.Create(self_position.Item1 + 1, self_position.Item2),
        Tuple.Create(self_position.Item1 - 1, self_position.Item2),
        Tuple.Create(self_position.Item1, self_position.Item2 + 1),
        Tuple.Create(self_position.Item1, self_position.Item2 - 1),
    };

        List<Tuple<int, int>> validTiles = new List<Tuple<int, int>>();
        List<string> validActions = new List<string>();
        foreach (var dir in directions)
        {
            int x = dir.Item1;
            int y = dir.Item2;
            if (mapGrid[x, y] == CellType.Empty && bombMap[x, y] > 0.5f && explosionMap[x, y] <= 1
                && !other_position.Contains(dir) && !bomb_position.Contains(dir))
            {
                validTiles.Add(dir);
            }
        }

        if (validTiles.Contains(Tuple.Create(self_position.Item1 - 1, self_position.Item2))) validActions.Add("LEFT");
        if (validTiles.Contains(Tuple.Create(self_position.Item1 + 1, self_position.Item2))) validActions.Add("RIGHT");
        if (validTiles.Contains(Tuple.Create(self_position.Item1, self_position.Item2 - 1))) validActions.Add("DOWN");
        if (validTiles.Contains(Tuple.Create(self_position.Item1, self_position.Item2 + 1))) validActions.Add("UP");
        if (validTiles.Contains(self_position)) validActions.Add("WAIT");

        if (bomb_left > 0 && HasSafeEscape(self_position, bombMap))
            validActions.Add("BOMB");

        List<GameObject> coins = GameManager.GetCoins();
        List<Tuple<int, int>> coin_position = new List<Tuple<int, int>>();
        foreach (var coin in coins)
        {
            Vector2Int pos = new Vector2Int((int)coin.transform.position.x, (int)coin.transform.position.y);
            coin_position.Add(MapManager.Instance.Vec2IntToGridBased(pos));
        }

        bool[,] freeSpace = new bool[mapGrid.GetLength(0), mapGrid.GetLength(1)];
        for (int i = 0; i < mapGrid.GetLength(0); i++)
            for (int j = 0; j < mapGrid.GetLength(1); j++)
                freeSpace[i, j] = (mapGrid[i, j] == CellType.Empty);

        foreach (var o in other_position)
            freeSpace[o.Item1, o.Item2] = false;

        Tuple<int, int> target = LookForTargets(freeSpace, self_position, coin_position);

        List<string> actionIdeas = new List<string>();
        if (target != null)
        {
            if (target.Equals(Tuple.Create(self_position.Item1, self_position.Item2 + 1))) actionIdeas.Add("UP");
            if (target.Equals(Tuple.Create(self_position.Item1, self_position.Item2 - 1))) actionIdeas.Add("DOWN");
            if (target.Equals(Tuple.Create(self_position.Item1 - 1, self_position.Item2))) actionIdeas.Add("LEFT");
            if (target.Equals(Tuple.Create(self_position.Item1 + 1, self_position.Item2))) actionIdeas.Add("RIGHT");
        }
        else
        {
            actionIdeas.Add("WAIT");
        }

        foreach (var enemy in other_position)
        {
            int dist = Mathf.Abs(enemy.Item1 - self_position.Item1) + Mathf.Abs(enemy.Item2 - self_position.Item2);
            if (dist <= 1)
            {
                actionIdeas.Add("BOMB");
                break;
            }
        }

        actionIdeas.Add("WAIT");

        while (actionIdeas.Count > 0)
        {
            string a = actionIdeas[actionIdeas.Count - 1];
            actionIdeas.RemoveAt(actionIdeas.Count - 1);
            if (validActions.Contains(a))
            {
                if (a == "BOMB") bombHistory.Enqueue(self_position);
                return a;
            }
        }
        return "WAIT";
    }



    private string ApplyComplexRuleBased()
    {
        CellType[,] mapGrid = MapManager.Instance.getMapGrid();

        int score = GameManager.GetScore(this.agentName);

        int bomb_left = GameManager.GetBombLeft(this.agentName);


        List<GameObject> coins = GameManager.GetCoins();
        List<Tuple<int, int>> coin_position = new List<Tuple<int, int>>();
        foreach (var coin in coins)
        {
            if (coin == null) continue;
            Vector2Int pos = new Vector2Int((int)coin.transform.position.x, (int)coin.transform.position.y);
            coin_position.Add(MapManager.Instance.Vec2IntToGridBased(pos));
        }
        List<GameObject> bombs = GameManager.GetAllBombs();

        Vector2Int position = new Vector2Int(Mathf.FloorToInt(rb.position.x), Mathf.FloorToInt(rb.position.y));
        Tuple<int, int> self_position = MapManager.Instance.Vec2IntToGridBased(position);

        List<Vector2Int> vector2Ints = GameManager.GetOtherPosition(this.agentName);
        List<Tuple<int, int>> other_position = MapManager.Instance.Vec2IntToGridBased(vector2Ints);

        List<Tuple<int, int>> bomb_position = new List<Tuple<int, int>>();

        float[,] bombMap = new float[mapGrid.GetLength(0), mapGrid.GetLength(1)];
        for (int i = 0; i < bombMap.GetLength(0); i++)
            for (int j = 0; j < bombMap.GetLength(1); j++)
                bombMap[i, j] = 5f;

        foreach (GameObject bomb in bombs)
        {
            if (bomb == null) continue;
            Bomb bomb_component = bomb.GetComponent<Bomb>();
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(bomb.transform.position.x), Mathf.FloorToInt(bomb.transform.position.y));
            Tuple<int, int> bombXY = MapManager.Instance.Vec2IntToGridBased(pos);
            bomb_position.Add(bombXY);
                
            for (int h = -3; h <= 3; h++)
            {
                Tuple<int, int>[] points = new Tuple<int, int>[] {
                    Tuple.Create(bombXY.Item1 + h, bombXY.Item2),
                    Tuple.Create(bombXY.Item1, bombXY.Item2 + h)
                };

                foreach(Tuple<int, int> point in points)
                {
                    if (point.Item1 > 0 && point.Item1 < bombMap.GetLength(0) && point.Item2 > 0 && point.Item2 < bombMap.GetLength(1))
                        bombMap[point.Item1, point.Item2] = Mathf.Min(bombMap[point.Item1, point.Item2], bomb_component.GetRemainingTime());
                }
            }
        }

        if (CountPositionFrequency(coordinateHistory, self_position) > 2)
        {
            ignore_others_timer = 5;
        }
        else
        {
            ignore_others_timer = -1;
        }
        coordinateHistory.Enqueue(self_position);

        Tuple<int, int>[] directions = {self_position,
            Tuple.Create(self_position.Item1 + 1, self_position.Item2),
            Tuple.Create(self_position.Item1 - 1, self_position.Item2),
            Tuple.Create(self_position.Item1, self_position.Item2 + 1),
            Tuple.Create(self_position.Item1, self_position.Item2 - 1),
        };

        List<Tuple<int, int>> validTiles = new List<Tuple<int, int>>();
        List<string> validActions = new List<string>();

        int[,] explosionMap = MapManager.Instance.GetExplosionMap();
      
        foreach(var direction in directions)
        {
            int x = direction.Item1;
            int y = direction.Item2;
            if (mapGrid[x, y] == CellType.Empty && bombMap[x, y] > 0.5 && explosionMap[x, y] <= 2
                && !other_position.Contains(direction) && !bomb_position.Contains(direction))
            {
                validTiles.Add(direction);
            }
        }

        if (validTiles.Contains(Tuple.Create(self_position.Item1 - 1, self_position.Item2))) validActions.Add("LEFT");
        if (validTiles.Contains(Tuple.Create(self_position.Item1 + 1, self_position.Item2))) validActions.Add("RIGHT");
        if (validTiles.Contains(Tuple.Create(self_position.Item1, self_position.Item2 - 1))) validActions.Add("DOWN");
        if (validTiles.Contains(Tuple.Create(self_position.Item1, self_position.Item2 + 1))) validActions.Add("UP");
        if (validTiles.Contains(self_position)) validActions.Add("WAIT");
        if (bomb_left > 0 && !bombHistory.Contains(self_position) && HasSafeEscape(self_position, bombMap))
            validActions.Add("BOMB");

        List<string> actionIdeas = new List<string> { "UP", "DOWN", "LEFT", "RIGHT" };
        Shuffle(actionIdeas);

        List<Tuple<int, int>> targets = new List<Tuple<int, int>>();
        List<Tuple<int, int>> deadEnds = new List<Tuple<int, int>>();
        List<Tuple<int, int>> destructable = new List<Tuple<int, int>>();

        for (int i = 1; i < mapGrid.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < mapGrid.GetLength(1) - 1; j++)
            {
                if (mapGrid[i, j] == CellType.Empty)
                {
                    int count = 0;
                    if (mapGrid[i + 1, j] == CellType.Empty) count++;
                    if (mapGrid[i - 1, j] == CellType.Empty) count++;
                    if (mapGrid[i, j + 1] == CellType.Empty) count++;
                    if (mapGrid[i, j - 1] == CellType.Empty) count++;
                    if (count == 1)
                        deadEnds.Add(Tuple.Create(i, j));
                }
                if (mapGrid[i, j] == CellType.DestructibleWall)
                {
                    destructable.Add(Tuple.Create(i, j));
                }
            }
        }

        targets.AddRange(coin_position);
        targets.AddRange(deadEnds);
        targets.AddRange(destructable);

        if (destructable.Count + coin_position.Count == 0 || ignore_others_timer <= 0)
            targets.AddRange(other_position);

        targets.RemoveAll(t => bomb_position.Contains(t));

        bool[,] freeSpace = new bool[mapGrid.GetLength(0), mapGrid.GetLength(1)];
        for (int i = 0; i < mapGrid.GetLength(0); i++)
        {
            for (int j = 0; j < mapGrid.GetLength(1); j++)
            {
                freeSpace[i, j] = (mapGrid[i, j] == CellType.Empty);
            }
        }

        if (ignore_others_timer > 0)
        {
            foreach (var o in other_position)
                freeSpace[o.Item1, o.Item2] = false;
        }

        Tuple<int, int> direct = LookForTargets(freeSpace, self_position, targets);

        if (direct != null)
        {
            if (direct == Tuple.Create(self_position.Item1, self_position.Item2 + 1)) actionIdeas.Add("UP");
            if (direct == Tuple.Create(self_position.Item1, self_position.Item2 - 1)) actionIdeas.Add("DOWN");
            if (direct == Tuple.Create(self_position.Item1 - 1, self_position.Item2)) actionIdeas.Add("LEFT");
            if (direct == Tuple.Create(self_position.Item1 + 1, self_position.Item2)) actionIdeas.Add("RIGHT");
        } else
        {
            actionIdeas.Add("WAIT");
        }
        if (deadEnds.Contains(self_position)) actionIdeas.Add("BOMB");

        if (other_position.Count > 0)
        {
            int minDistance = 10000;
            foreach(Tuple<int, int> pos in other_position)
            {
                minDistance = Mathf.Min(Mathf.Abs(pos.Item1 - self_position.Item1) + Mathf.Abs(pos.Item2 - self_position.Item2), minDistance);
            }
            if (minDistance <= 1)
            {
                actionIdeas.Add("BOMB");
            }
        }

        if (direct != null && direct.Equals(self_position))
        {
            int count = 0;
            if (mapGrid[self_position.Item1 + 1, self_position.Item2] == CellType.DestructibleWall) count++;
            if (mapGrid[self_position.Item1 - 1, self_position.Item2] == CellType.DestructibleWall) count++;
            if (mapGrid[self_position.Item1, self_position.Item2 + 1] == CellType.DestructibleWall) count++;
            if (mapGrid[self_position.Item1, self_position.Item2 - 1] == CellType.DestructibleWall) count++;
            if (count > 0)
            {

                actionIdeas.Add("BOMB");
            }

        }
        foreach (Tuple<int, int> bomb in bomb_position)
        {
            if (bomb.Item1 == self_position.Item1 && Mathf.Abs(bomb.Item2 - self_position.Item2) < 4)
            {
                if (bomb.Item2 > self_position.Item2) actionIdeas.Add("DOWN");
                if (bomb.Item2 < self_position.Item2) actionIdeas.Add("UP");
                actionIdeas.Add("LEFT");
                actionIdeas.Add("RIGHT");
            }

            if (bomb.Item2 == self_position.Item2 && Mathf.Abs(bomb.Item1 - self_position.Item1) < 4)
            {
                if (bomb.Item1 > self_position.Item1) actionIdeas.Add("LEFT");
                if (bomb.Item1 < self_position.Item1) actionIdeas.Add("RIGHT");
                actionIdeas.Add("UP");
                actionIdeas.Add("DOWN");
            }
        }

        foreach(Tuple<int, int> bomb in bomb_position)
        {
            if (bomb.Equals(self_position))
            {
                int count = Math.Min(4, actionIdeas.Count);
                actionIdeas.AddRange(actionIdeas.GetRange(0, count));
            }
        }
  
        while (actionIdeas.Count > 0)
        {
            string a = actionIdeas[actionIdeas.Count - 1];
            actionIdeas.RemoveAt(actionIdeas.Count - 1);
            if (validActions.Contains(a))
            {
                if (a == "BOMB")
                    bombHistory.Enqueue(self_position);
                return a;
            }
        }
        return "WAIT";
    }
    private Tuple<int, int> LookForTargets(bool[,] freeSpace, Tuple<int, int> start, List<Tuple<int, int>> targets)
    {
        if (targets == null || targets.Count == 0) return null;

        Queue<Tuple<int, int>> frontier = new Queue<Tuple<int, int>>();
        frontier.Enqueue(start);

        Dictionary<Tuple<int, int>, Tuple<int, int>> parentDict = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
        parentDict[start] = start;

        Dictionary<Tuple<int, int>, int> distSoFar = new Dictionary<Tuple<int, int>, int>();
        distSoFar[start] = 0;

        Tuple<int, int> best = start;
        int bestDist = GetBestDistance(targets, start);

        while (frontier.Count > 0)
        {
            Tuple<int, int> current = frontier.Dequeue();

            int d = GetBestDistance(targets, current);
            if (d + distSoFar[current] <= bestDist)
            {
                best = current;
                bestDist = d + distSoFar[current];
            }

            if (d == 0)
            {
                best = current;
                break;
            }

            if (best.Equals(start))
            {
                return start;
            }


            List<Tuple<int, int>> neighbors = GetNeighbors(current, freeSpace);
            Shuffle(neighbors);

            foreach (var neighbor in neighbors)
            {
                if (!parentDict.ContainsKey(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    parentDict[neighbor] = current;
                    distSoFar[neighbor] = distSoFar[current] + 1;
                }
            }
        }

        Tuple<int, int> step = best;
        while (true)
        {
            if (parentDict[step] == start)
                return step;
            step = parentDict[step];
        }
    }

    private int GetBestDistance(List<Tuple<int, int>> targets, Tuple<int, int> current)
    {
        int minDist = int.MaxValue;
        foreach (var target in targets)
        {
            int dist = Mathf.Abs(target.Item1 - current.Item1) + Mathf.Abs(target.Item2 - current.Item2);
            if (dist < minDist)
            {
                minDist = dist;
            }
        }
        return minDist;
    }

    private List<Tuple<int, int>> GetNeighbors(Tuple<int, int> position, bool[,] freeSpace)
    {
        List<Tuple<int, int>> neighbors = new List<Tuple<int, int>>();
        int width = freeSpace.GetLength(0);
        int height = freeSpace.GetLength(1);
        Tuple<int, int>[] posible = new Tuple<int, int>[]
        {
            Tuple.Create(position.Item1 + 1, position.Item2),
            Tuple.Create(position.Item1 - 1, position.Item2),
            Tuple.Create(position.Item1, position.Item2 + 1),
            Tuple.Create(position.Item1, position.Item2 - 1),
        };

        foreach (var p in posible)
        {
            if (p.Item1 >= 0 && p.Item1 < width && p.Item2 < height && p.Item1 >= 0 && freeSpace[p.Item1, p.Item2])
            {
                neighbors.Add(p);
            }
        }
        return neighbors;
    }
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
    private void EnqueueWithLimit<T>(Queue<T> queue, T item, int maxSize)
    {
        if (queue.Count >= maxSize)
        {
            queue.Dequeue();
        }
        queue.Enqueue(item);
    }
    public int CountPositionFrequency(Queue<Tuple<int, int>> queue, Tuple<int, int> targetPosition)
    {
        int count = 0;
        foreach (var position in queue)
        {
            if (position == targetPosition)
            {
                count++;
            }
        }
        return count;
    }
    private bool HasSafeEscape(Tuple<int, int> pos, float[,] bombMap)
    {
        Tuple<int, int>[] directions = {
        Tuple.Create(pos.Item1 + 1, pos.Item2),
        Tuple.Create(pos.Item1 - 1, pos.Item2),
        Tuple.Create(pos.Item1, pos.Item2 + 1),
        Tuple.Create(pos.Item1, pos.Item2 - 1)
    };

        foreach (var dir in directions)
        {
            if (dir.Item1 >= 0 && dir.Item1 < bombMap.GetLength(0) &&
                dir.Item2 >= 0 && dir.Item2 < bombMap.GetLength(1))
            {
                if (bombMap[dir.Item1, dir.Item2] > 1.5f)
                    return true;
            }
        }
        return false;
    }

}

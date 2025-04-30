using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;

public class V2RuleBased : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;
    private string agentName;
    public float speed = 5f;
    public int level;
    private bool isWaiting = false;
    private float waitTime = 1f;

    private Queue<Tuple<int, int>> bombHistory = new Queue<Tuple<int, int>>(5);
    private Queue<Tuple<int, int>> coordinateHistory = new Queue<Tuple<int, int>>(20);
    private int ignoreOthersTimer = 0;

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
        Debug.Log("This is: " + agentName);

        switch (level)
        {
            case 0:
                ApplySimpleRuleBased();
                break;
            case 1:
                ApplyComplexRuleBased();
                break;
            default:
                ApplySimpleRuleBased();
                break;
        }
    }
    private void Update()
    {
        if (isWaiting) return;

        Vector2 playerPosition = transform.position;
        Tuple<int, int> pos = Tuple.Create((int)playerPosition.x, (int)playerPosition.y);
        EnqueueWithLimit(bombHistory, pos, 5);
        EnqueueWithLimit(coordinateHistory, pos, 20);

        string action = ApplyComplexRuleBased();
        Debug.Log(action);
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
        GameManager.Instance.CheckWinState();
    }

    private void ApplySimpleRuleBased()
    {

    }

    private string ApplyComplexRuleBased()
    {


        CellType[,] mapGrid = MapManager.Instance.getMapGrid();
        int score = GameManager.GetScore(this.agentName);
        int bombLeft = GameManager.GetBombLeft(this.agentName);

        Tuple<int, int> selfPosition = GetSelfPosition();
        HashSet<Tuple<int, int>> coinPositions = GetCoinPosition();
        HashSet<Tuple<int, int>> bombPositions = GetBombPosition(out float[,] bombMap);
        HashSet<Tuple<int, int>> otherPositions = new HashSet<Tuple<int, int>>(
            MapManager.Instance.Vec2IntToGridBased(
                GameManager.GetOtherPosition(this.agentName
        )));

        UpdateIgnoreOthersTimer(selfPosition);
        coordinateHistory.Enqueue(selfPosition);

        var validTiles = GetValidTiles(mapGrid, bombMap, otherPositions, selfPosition, bombPositions);
        var validActions = GetValidActions(selfPosition, validTiles);

        if (bombLeft > 0 && !bombHistory.Contains(selfPosition))
            validActions.Add("BOMB");

        var actionIdeas = GenerateActionIdeas(mapGrid, selfPosition, coinPositions, otherPositions, bombPositions, ignoreOthersTimer);

        // Try actions in order
        foreach (var action in actionIdeas)
        {
            if (validActions.Contains(action))
            {
                if (action == "BOMB")
                    bombHistory.Enqueue(selfPosition);
                return action;
            }
        }
        return "WAIT";
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

    //--------------------------------
    private Tuple<int, int> GetSelfPosition()
    {
        var pos = new Vector2Int((int)rb.position.x, (int)rb.position.y);
        return MapManager.Instance.Vec2IntToGridBased(pos);
    }
    private HashSet<Tuple<int, int>> GetCoinPosition()
    {
        List<GameObject> coins = GameManager.GetCoins();
        HashSet<Tuple<int, int>> result = new HashSet<Tuple<int, int>>();
        foreach (var coin in coins)
        {
            if (coin == null) continue;
            Vector2Int pos = new Vector2Int((int)coin.transform.position.x, (int)coin.transform.position.y);
            result.Add(MapManager.Instance.Vec2IntToGridBased(pos));
        }

        return result;
    }

    private HashSet<Tuple<int, int>> GetBombPosition(out float[,] bombMap)
    {
        var mapGrid = MapManager.Instance.getMapGrid();
        bombMap = new float[mapGrid.GetLength(0), mapGrid.GetLength(1)];
        for (int i = 0; i < bombMap.GetLength(0); i++)
            for (int j = 0; j < bombMap.GetLength(1); j++)
                bombMap[i, j] = 5f;

        List<GameObject> bombs = GameManager.GetAllBombs();
        HashSet<Tuple<int, int>> result = new HashSet<Tuple<int, int>>();

        foreach (var bomb in bombs)
        {
            var transform = bomb.transform;
            var bombComponent = bomb.GetComponent<Bomb>();
            var pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            var bombPos = MapManager.Instance.Vec2IntToGridBased(pos);

            result.Add(bombPos);

            for (int h = -3; h <= 3; h++)
            {
                var points = new Tuple<int, int>[] {
                Tuple.Create(bombPos.Item1 + h, bombPos.Item2),
                Tuple.Create(bombPos.Item1, bombPos.Item2 + h)
            };
                foreach (var p in points)
                {
                    if (MapManager.Instance.IsInsideMap(p.Item1, p.Item2))
                        bombMap[p.Item1, p.Item2] = Mathf.Min(bombMap[p.Item1, p.Item2], bombComponent.GetRemainingTime());
                }
            }
        }
        return result;
    }

    private void UpdateIgnoreOthersTimer(Tuple<int, int> selfPosition)
    {
        if (coordinateHistory.Count >= 5)
        {
            var lastPositions = coordinateHistory.ToArray();
            bool same = true;
            for (int i = 0; i < 5; i++)
            {
                if (!lastPositions[i].Equals(selfPosition))
                {
                    same = false;
                    break;
                }
            }
            if (same)
                ignoreOthersTimer += 1;
            else
                ignoreOthersTimer = 0;
        }
    }

    private List<Tuple<int, int>> GetValidTiles(CellType[,] mapGrid, float[,] bombMap, HashSet<Tuple<int, int>> otherPositions, Tuple<int, int> selfPosition, HashSet<Tuple<int, int>> bombPositions)
    {
        Tuple<int, int>[] directions = {selfPosition,
            Tuple.Create(selfPosition.Item1 + 1, selfPosition.Item2),
            Tuple.Create(selfPosition.Item1 - 1, selfPosition.Item2),
            Tuple.Create(selfPosition.Item1, selfPosition.Item2 + 1),
            Tuple.Create(selfPosition.Item1, selfPosition.Item2 - 1),
        };

        var validTiles = new List<Tuple<int, int>>();
        int[,] explosionMap = MapManager.Instance.GetExplosionMap();

        foreach (var direction in directions)
        {
            int x = direction.Item1;
            int y = direction.Item2;
            if ((mapGrid[x, y] == CellType.Empty || mapGrid[x, y] == CellType.Item) && explosionMap[x, y] < 1 
                && !otherPositions.Contains(direction) && !bombPositions.Contains(direction))
            {
                validTiles.Add(direction);
            }
        }
        return validTiles;
    }

    private HashSet<string> GetValidActions(Tuple<int, int> selfPosition, List<Tuple<int, int>> validTiles)
    {
        var actions = new HashSet<string>();
        var directions = new Dictionary<string, Tuple<int, int>> {
        { "LEFT", Tuple.Create(-1, 0) },
        { "RIGHT", Tuple.Create(1, 0) },
        { "DOWN", Tuple.Create(0, -1) },
        { "UP", Tuple.Create(0, 1) }
    };

        foreach (var dir in directions)
        {
            var newPos = Tuple.Create(selfPosition.Item1 + dir.Value.Item1, selfPosition.Item2 + dir.Value.Item2);
            if (validTiles.Contains(newPos))
                actions.Add(dir.Key);
        }
        if (validTiles.Contains(selfPosition)) actions.Add("WAIT");
        return actions;
    }

    public static List<string> GenerateActionIdeas(CellType[,] mapGrid, Tuple<int, int> selfPosition, HashSet<Tuple<int, int>> coins,
                                             HashSet<Tuple<int, int>> otherPositions, HashSet<Tuple<int, int>> bombPositions, int ignoreOthersTimer)
    {
        List<string> actionIdeas = new List<string> { "UP", "DOWN", "LEFT", "RIGHT" };
        Shuffle(actionIdeas);

        int width = mapGrid.GetLength(0);
        int height = mapGrid.GetLength(1);

        HashSet<Tuple<int, int>> deadEnds = new HashSet<Tuple<int, int>>();
        HashSet<Tuple<int, int>> crates = new HashSet<Tuple<int, int>>(); // Destructible Block

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (mapGrid[x, y] == CellType.Empty)
                {
                    int count = 0;
                    if (mapGrid[x + 1, y] == CellType.Enemy) count++;
                    if (mapGrid[x - 1, y] == CellType.Enemy) count++;
                    if (mapGrid[x, y + 1] == CellType.Enemy) count++;
                    if (mapGrid[x, y - 1] == CellType.Enemy) count++;
                    if (count == 1)
                        deadEnds.Add(Tuple.Create(x, y));
                }
                if (mapGrid[x, y] == CellType.DestructibleWall)
                {
                    crates.Add(Tuple.Create(x, y));
                }
            }
        }

        HashSet<Tuple<int, int>> targets = new HashSet<Tuple<int, int>>();
        targets.AddRange(coins);
        targets.AddRange(deadEnds);
        targets.AddRange(crates);

        if (ignoreOthersTimer <= 0 || (crates.Count + coins.Count == 0))
            targets.AddRange(otherPositions);

        targets = new HashSet<Tuple<int, int>>(targets.Where(t => !bombPositions.Contains(t)));

        bool[,] freeSpace = new bool[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                freeSpace[i, j] = (mapGrid[i, j] == CellType.Empty);
            }
        }
        if (ignoreOthersTimer > 0)
        {
            foreach (var o in otherPositions)
            {
                freeSpace[o.Item1, o.Item2] = false;
            }
        }

        var direction = LookForTargets(freeSpace, selfPosition, targets);

        if (direction != null)
        {
            var d = direction;
            if (d.Item1 == selfPosition.Item1 && d.Item2 == selfPosition.Item2 - 1) actionIdeas.Add("DOWN");
            if (d.Item1 == selfPosition.Item1 && d.Item2 == selfPosition.Item2 + 1) actionIdeas.Add("UP");
            if (d.Item1 == selfPosition.Item1 - 1 && d.Item2 == selfPosition.Item2) actionIdeas.Add("LEFT");
            if (d.Item1 == selfPosition.Item1 + 1 && d.Item2 == selfPosition.Item2) actionIdeas.Add("RIGHT");
        }
        else
        {
            Debug.Log("All targets gone, nothing to do anymore");
            actionIdeas.Add("WAIT");
        }

        if (deadEnds.Contains(selfPosition))
            actionIdeas.Add("BOMB");

        if (otherPositions.Any(o => Math.Abs(o.Item1 - selfPosition.Item1) + Math.Abs(o.Item2 - selfPosition.Item2) <= 1))
            actionIdeas.Add("BOMB");

        if (HasAdjacentCrate(mapGrid, selfPosition)) //Adjust
            actionIdeas.Add("BOMB");

        foreach (var bombPos in bombPositions)
        {
            if (bombPos.Item1 == selfPosition.Item1 && Math.Abs(bombPos.Item2 - selfPosition.Item2) < 4)
            {
                if (bombPos.Item2 > selfPosition.Item2) actionIdeas.Add("DOWN");
                if (bombPos.Item2 < selfPosition.Item2) actionIdeas.Add("UP");
                actionIdeas.Add("LEFT");
                actionIdeas.Add("RIGHT");
            }
            if (bombPos.Item2 == selfPosition.Item2 && Math.Abs(bombPos.Item1 - selfPosition.Item1) < 4)
            {
                if (bombPos.Item1 > selfPosition.Item1) actionIdeas.Add("LEFT");
                if (bombPos.Item1 < selfPosition.Item1) actionIdeas.Add("RIGHT");
                actionIdeas.Add("UP");
                actionIdeas.Add("DOWN");
            }
            if (bombPos.Equals(selfPosition))
            {
                int count = Math.Min(4, actionIdeas.Count);
                actionIdeas.AddRange(actionIdeas.GetRange(0, count));
            } 
        }

        return actionIdeas;
    }
    public static Tuple<int, int> LookForTargets(bool[,] freeSpace, Tuple<int, int> start, HashSet<Tuple<int, int>> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        Queue<Tuple<int, int>> frontier = new Queue<Tuple<int, int>>();
        Dictionary<Tuple<int, int>, Tuple<int, int>> parentMap = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
        Dictionary<Tuple<int, int>, int> distSoFar = new Dictionary<Tuple<int, int>, int>();

        frontier.Enqueue(start);
        parentMap[start] = start;
        distSoFar[start] = 0;

        Tuple<int, int> best = start;
        int bestDist = MinManhattanDistance(start, targets);

        int width = freeSpace.GetLength(0);
        int height = freeSpace.GetLength(1);
        Tuple<int, int>[] directions = new Tuple<int, int>[]
        {
            Tuple.Create(1, 0), Tuple.Create(-1, 0),
            Tuple.Create(0, 1), Tuple.Create(0, -1)
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            int distToTargets = MinManhattanDistance(current, targets);
            int totalCost = distToTargets + distSoFar[current];

            if (totalCost <= bestDist)
            {
                best = current;
                bestDist = totalCost;
            }
            if (distToTargets == 0)
                //best = current;
                break;

            if (best.Equals(start))
            {
                return start;
            }

            foreach (var dir in directions)
            {
                var neighbor = Tuple.Create(current.Item1 + dir.Item1, current.Item2 + dir.Item2);
                if (IsValid(neighbor, width, height) && freeSpace[neighbor.Item1, neighbor.Item2] && !parentMap.ContainsKey(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    parentMap[neighbor] = current;
                    distSoFar[neighbor] = distSoFar[current] + 1;
                }
            }
        }

        var trace = best;
        while (!parentMap[trace].Equals(start))
        {
            trace = parentMap[trace];
        }
        return trace;
    }

    private static int MinManhattanDistance(Tuple<int, int> point, HashSet<Tuple<int, int>> targets)
    {
        int min = int.MaxValue;
        foreach (var target in targets)
        {
            int d = Math.Abs(target.Item1 - point.Item1) + Math.Abs(target.Item2 - point.Item2);
            if (d < min)
                min = d;
        }
        return min;
    }

    private static bool IsValid(Tuple<int, int> pos, int width, int height)
    {
        return pos.Item1 >= 0 && pos.Item1 < width && pos.Item2 >= 0 && pos.Item2 < height;

    }

    private static bool HasAdjacentCrate(CellType[,] mapGrid, Tuple<int, int> selfPosition)
    {
        int count = 0;
        if (mapGrid[selfPosition.Item1 + 1, selfPosition.Item2] == CellType.DestructibleWall) count++;
        if (mapGrid[selfPosition.Item1 - 1, selfPosition.Item2] == CellType.DestructibleWall) count++;
        if (mapGrid[selfPosition.Item1, selfPosition.Item2 + 1] == CellType.DestructibleWall) count++;
        if (mapGrid[selfPosition.Item1, selfPosition.Item2 - 1] == CellType.DestructibleWall) count++;
        if (count > 0)
            return true;

        return false;
    }
}

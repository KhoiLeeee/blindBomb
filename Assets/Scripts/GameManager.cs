using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static GameObject[] players;
    private static GameObject[] enemies;

    private static List<GameObject> coins = new List<GameObject>();
    private static Dictionary<string, int> scores = new Dictionary<string, int>();
    private static Dictionary<string, int> hearts = new Dictionary<string, int>();
    private static Dictionary<string, int> explosion_ranges = new Dictionary<string, int>();
    private static Dictionary<string, List<GameObject>> bombs = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    public void CheckWinState()
    {
        int aliveCount = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].activeSelf)
            {
                aliveCount++;
            }
        }

        if (aliveCount <= 1)
        {
            Invoke(nameof(NewRound), 3f);
        }
    }

    private void NewRound()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static int GetScore(string agent_name)
    {
        if (scores.ContainsKey(agent_name))
        {
            return scores[agent_name];
        }
        return 0;
    }

    public static void SetScore(string agent_name, int score)
    {
        if (scores.ContainsKey(agent_name))
        {
            scores[agent_name] = score; 
        } else
        {
            scores.Add(agent_name, score);
        }
    }

    public static int GetHeart(string agent_name)
    {
        if (hearts.ContainsKey(agent_name))
        {
            return hearts[agent_name]; 
        }
        return 0;
    }

    public static void SetHeart(string agent_name, int heart)
    {
        if (hearts.ContainsKey(agent_name))
        {
            hearts[agent_name] = heart;
        } else
        {
            hearts.Add(agent_name, heart);
        }
    }

    public static void IncreaseHeart(string agent_name, int heart)
    {
        if (!hearts.ContainsKey(agent_name))
        {
            return;
        }

        hearts[agent_name] += heart;
    }

    public static int GetExplosionRange(string agent_name)
    {
        if (explosion_ranges.ContainsKey(agent_name))
        {
            return explosion_ranges[agent_name];
        }
        return -1;
    }

    public static void SetExplosionRange(string agent_name, int range)
    {
        if (explosion_ranges.ContainsKey(agent_name))
        {
            explosion_ranges[agent_name] = range;
        } else
        {
            explosion_ranges.Add(agent_name, range);
        }
    }

    public static void IncreaseExplosionRange(string agent_name, int range)
    {
        if (!explosion_ranges.ContainsKey(agent_name))
        {
            return;
        }

        explosion_ranges[agent_name] += range;
    }

    public static List<GameObject> GetBomb(string agent_name)
    {
        if (bombs.ContainsKey(agent_name))
        {
            return bombs[agent_name];
        }
        return new List<GameObject>();
    }

    public static List<GameObject> GetAllBombs()
    {
        List<GameObject> allBombs = new List<GameObject>();
        foreach(var bombList in bombs.Values)
        {
            allBombs.AddRange(bombList);
        }
        return allBombs;
       
    }
    public static void SetBombs(string agent_name, List<GameObject> bomb)
    {
        if (bombs.ContainsKey(agent_name))
        {
            bombs[agent_name] = bomb;
        } else
        {
            bombs.Add(agent_name, bomb);
        }
    }   

    public static void AddBombs(string agent_name, GameObject bomb)
    {
        if (!bombs.ContainsKey(agent_name))
        {
            List<GameObject> new_bomb = new List<GameObject> { bomb };
            bombs.Add(agent_name, new_bomb);
            
        }
        bombs[agent_name].Add(bomb);
    }

    public static void RemoveBomb(string agent_name, GameObject bomb)
    {
        if (bombs.ContainsKey(agent_name))
        {
            bombs[agent_name].Remove(bomb);
        } else {
            Debug.Log("Agent " + agent_name + "is not in bomb list");
        }
    }
    public static List<GameObject> GetCoins()
    {
        return coins;
    }

    public static void AddCoins(GameObject gameObject)
    {
        coins.Add(gameObject);
    }

    public static void RemoveCoin(GameObject gameObject)
    {
        coins.Remove(gameObject);
    }

    public static int GetBombLeft(string agent_name)
    {
        foreach (GameObject gameObject in players)
        {
            if (gameObject.name == agent_name)
            {
                BombController bombcontroller = gameObject.GetComponent<BombController>();
                return bombcontroller.getBombRemaining();
            }
        }

        foreach (GameObject gameObject in enemies)
        {
            if (gameObject.name == agent_name)
            {
                BombController bombcontroller = gameObject.GetComponent<BombController>();
                return bombcontroller.getBombRemaining();
            }
        }
        return -1;
    }

    public static List<Vector2Int> GetOtherPosition(string agent_name)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (GameObject gameObject in players)
        {
            if (gameObject.name != agent_name)
            {
                Vector3 pos = gameObject.transform.position;
                positions.Add(new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y)));
            }
        }

        foreach (GameObject gameObject in enemies)
        {
            if (gameObject.name != agent_name)
            {
                Vector3 pos = gameObject.transform.position;
                positions.Add(new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y)));
            }
        }
        return positions;
    }
}

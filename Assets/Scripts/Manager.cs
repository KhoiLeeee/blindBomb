using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }

    public GameObject[] players;
    public GameObject[] enemies;

    private List<GameObject> teamA = new List<GameObject>();
    private List<GameObject> teamB = new List<GameObject>();

    public GameObject pauseGameScreen;
    public GameObject endGameScreen;
    public TMP_Dropdown playerNum;
    public TMP_Dropdown AiNum;
    public TMP_Dropdown mode;

    public Tilemap destructibleTiles;
    public Tilemap indestructibleTiles;

    public Tile barrelTile;
    public Tile boxTile;
    public Tile signTile;

    public int barrelCount = 10;
    public int boxCount = 10;
    public int signCount = 10;

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
    void Start()
    {
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(false);
        }
        SpawnPlayers();
        SetupTeams();

        if (DropdownData.mapType == "Normal")
        {
            ClearDestructibles();
            Debug.Log("Generating random map...");
            GenerateRandomMap();
        }
        else
        {
            foreach (GameObject player in players)
            {
                player.GetComponent<MovementController>().speed = 8f;
                player.GetComponent<BombController>().AddBomb();
                player.GetComponent<BombController>().AddBomb();
                player.GetComponent<BombController>().explosionRadiusMin = 4;
                player.GetComponent<BombController>().explosionRadiusMax = 6;
            }
            foreach (GameObject enemy in enemies)
            {
                enemy.GetComponent<RuleBasedAgent>().speed = 8f;
                enemy.GetComponent<BombController>().AddBomb();
                enemy.GetComponent<BombController>().AddBomb();
                enemy.GetComponent<BombController>().explosionRadiusMin = 4;
                enemy.GetComponent<BombController>().explosionRadiusMax = 6;
            }
        }
    }

    void SetupTeams()
    {
        teamA.Clear();
        teamB.Clear();
        int playerNum = int.Parse(DropdownData.playerNumber);
        int AiNum = int.Parse(DropdownData.AiNumber);


        if (AiNum == 0)
        {
            teamA.Add(players[0]);
            teamB.Add(players[1]);
        }
        else
        {
            foreach (GameObject player in players)
            {
                if(player.activeInHierarchy)
                {
                    teamA.Add(player);
                }
            }
            foreach (GameObject enemy in enemies)
            {
                if (enemy.activeInHierarchy)
                {
                    teamB.Add(enemy);
                }
            }
        }
    }

    void SpawnPlayers()
    {
        int playerNum = int.Parse(DropdownData.playerNumber);
        int AiNum = int.Parse(DropdownData.AiNumber);
        Debug.Log(playerNum);
        Debug.Log(AiNum);
        for (int i = 0; i < playerNum; i++)
        {
            players[i].SetActive(true);
        }
        for (int i = 0; i < AiNum; i++)
        {
            enemies[i].SetActive(true);
        }
    }

    public void CheckWinCondition()
    {
        teamA.RemoveAll(player => player == null || !player.activeInHierarchy);
        teamB.RemoveAll(player => player == null || !player.activeInHierarchy);

        if (teamA.Count == 0)
        {
            EndGame("Team B Wins!");
        }
        else if (teamB.Count == 0)
        {
            EndGame("Team A Wins!");
        }
    }

    public void EndGame(string message)
    {
        Debug.Log(message);
        endGameScreen.SetActive(true);
        Time.timeScale = 0f; // pause game
    }

    void ClearDestructibles()
    {
        destructibleTiles.ClearAllTiles();
    }

    List<Vector3Int> GetAvailablePositions()
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        int xMin = -8;
        int xMax = 8;   // exclusive (goes from -8 to +7)
        int yMin = -4;
        int yMax = 4;   // exclusive (goes from -4 to +3)

        HashSet<Vector3Int> playerCells = new HashSet<Vector3Int>();
        foreach (GameObject player in players)
        {
            Vector3Int cell = destructibleTiles.WorldToCell(player.transform.position);
            playerCells.Add(cell);
            playerCells.Add(cell + Vector3Int.up);
            playerCells.Add(cell + Vector3Int.down);
            playerCells.Add(cell + Vector3Int.left);
            playerCells.Add(cell + Vector3Int.right);
        }
        Vector3Int enemyCell = destructibleTiles.WorldToCell(enemies[0].transform.position);
        playerCells.Add(enemyCell);
        playerCells.Add(enemyCell + Vector3Int.up);
        playerCells.Add(enemyCell + Vector3Int.down);
        playerCells.Add(enemyCell + Vector3Int.left);
        playerCells.Add(enemyCell + Vector3Int.right);
        enemyCell = destructibleTiles.WorldToCell(enemies[1].transform.position);
        playerCells.Add(enemyCell);
        playerCells.Add(enemyCell + Vector3Int.up);
        playerCells.Add(enemyCell + Vector3Int.down);
        playerCells.Add(enemyCell + Vector3Int.left);
        playerCells.Add(enemyCell + Vector3Int.right);

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (destructibleTiles.GetTile(pos) == null &&
                    indestructibleTiles.GetTile(pos) == null &&
                    !playerCells.Contains(pos))
                {
                    positions.Add(pos);
                }
            }
        }

        Debug.Log("Available positions: " + positions.Count);
        return positions;
    }

    void GenerateRandomMap()
    {
        List<Vector3Int> positions = GetAvailablePositions();
        System.Random rng = new System.Random();
        positions = positions.OrderBy(x => rng.Next()).ToList(); // shuffle

        int total = barrelCount + boxCount + signCount;
        int placed = 0;

        for (int i = 0; i < total && i < positions.Count; i++)
        {
            Vector3Int cell = positions[i];

            Tile tileToPlace = null;

            if (placed < barrelCount) tileToPlace = barrelTile;
            else if (placed < barrelCount + boxCount) tileToPlace = boxTile;
            else tileToPlace = signTile;

            destructibleTiles.SetTile(cell, tileToPlace);
            placed++;
        }
    }

    public void onPauseButtonClicked()
    {
        pauseGameScreen.SetActive(true);
        Time.timeScale = 0f;
    }
    public void onContinueButtonClicked()
    {
        Time.timeScale = 1f;
        pauseGameScreen.SetActive(false);
    }
    
    public void onMenuButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("mainMenu");
    }

    public void onPlayAgainButtonClicked()
    {
        DropdownData.playerNumber = playerNum.options[playerNum.value].text;
        DropdownData.AiNumber = AiNum.options[AiNum.value].text;
        DropdownData.mapType = mode.options[mode.value].text;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
